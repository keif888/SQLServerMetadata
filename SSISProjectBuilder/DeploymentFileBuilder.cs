// Original code sourced from
// https://github.com/sabinio/SSISMSBuild/blob/master/SSIS2012Tasks/DeploymentFileCompilerTask.cs
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DataTransformationServices.Project;
using Microsoft.DataTransformationServices.Project.ComponentModel;
using Microsoft.DataTransformationServices.Project.Serialization;
using Microsoft.DataWarehouse.VsIntegration.Shell.Project.Configuration;
using System.Xml.Serialization;
using Microsoft.SqlServer.Dts.Runtime;
using System.IO;

namespace SSISProjectBuilder
{
    class DeploymentFileBuilder
    {
        /// <summary>
        /// Path(s) to the SSIS Visual Studio project files (.dtproj) to compile.
        /// </summary>
        public string InputProject { get; set; }

        /// <summary>
        /// The Visual Studio configuration to use. 
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// On a successful build, this parameter will be populated with the 
        /// full paths to the project deployment files (.ispac) created during
        /// this build.
        /// </summary>
        public string OutputISPAC { get; set; }

        /// <summary>
        /// (Optional) Sets the protection level for the output project (and all packages). 
        /// If not set, the  protection level specified in the .dtproj is used. Must be a value
        /// from the <see cref="DTSProtectionLevel"/> enum.
        /// </summary>
        public string ProtectionLevel
        {
            get
            {
                return m_protectionLevelString;
            }
            set
            {
                if (value != null)
                {
                    // try to parse it
                    Enum.Parse(typeof(DTSProtectionLevel), value, true);
                }

                m_protectionLevelString = value;
            }
        }

        /// <summary>
        /// (Optional) Sets the target SQL Server version for the output project.
        /// IF not set, the target version specified in the .dtproj is used.  Must be a value
        /// from the <see cref="DTSTargetServerVersion"/> enum.
        /// </summary>
        public string SQLVersion
        {
            get
            {
                return m_sqlVersionString;
            }
            set
            {
                if (value != null)
                {
                    Enum.Parse(typeof(DTSTargetServerVersion), value, true);
                }
                m_sqlVersionString = value;
            }
        }

        /// <summary>
        /// (Optional) This property is required when using a protection level that
        /// requires a password.
        /// </summary>
        public string ProjectPassword { get; set; }

        /// <summary>
        /// (Optional) When set, this version value will be used for the version information in
        /// the .ispac files, overridding any version values set in the .dtproj file.
        /// The format is "&lt;Major>.&lt;Minor>.&lt;Build". Ex: 10.0.1
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// (Optional) This value will populate the <see cref="Project.VersionComments"/> field. 
        /// If no value is provided, the text from the .dtproj file is used.
        /// </summary>
        public string VersionComments { get; set; }

        #region Serialization classes		

        private ProjectSerialization VsProject { get; set; }
        private ProjectManifest Manifest { get; set; }
        private DataTransformationsConfiguration ProjectConfiguration
        {
            get
            {
                if (m_projectConfiguration == null)
                {
                    GetProjectConfiguration();
                }

                return m_projectConfiguration;
            }
        }

        private DataTransformationsConfiguration m_projectConfiguration;

        private void GetProjectConfiguration()
        {
            foreach (var c in VsProject.Configurations)
            {
                var config = (DataTransformationsConfiguration)c;
                if (config.Name.Equals(Configuration, StringComparison.OrdinalIgnoreCase))
                {
                    m_projectConfiguration = config;
                    break;
                }
            }

            if (m_projectConfiguration == null)
            {
                if (VsProject.Configurations.Count == 0)
                    throw new Exception("Configuration not found");
                else
                    m_projectConfiguration = (DataTransformationsConfiguration)VsProject.Configurations[0];
            }
        }

        #endregion

        private string m_protectionLevelString;
        private string m_sqlVersionString;

        public bool Execute()
        {
            bool result = true;

            try
            {
                Console.WriteLine(string.Format("Start processing dtproj file {0}", InputProject));
                string projectDirectory = Path.GetDirectoryName(InputProject);

                DeserializeProject(InputProject);

                if (VsProject.DeploymentModel == DeploymentModel.Project)
                {
                    // Create project and set properties
                    var project = Project.CreateProject();
                    project.OfflineMode = true;
                    SetProjectProperties(project, Manifest);

                    // set the protection level
                    var protectionLevel = GetProtectionLevel(Manifest);
                    Console.WriteLine(string.Format("Project protection level is {0}", protectionLevel));
                    project.ProtectionLevel = protectionLevel;

                    // Set the project target version
                    if (SQLVersion != null)
                    {
                        project.TargetServerVersion = (DTSTargetServerVersion)Enum.Parse(typeof(DTSTargetServerVersion), SQLVersion, true);
                    }

                    if (PasswordNeeded(protectionLevel))
                    {
                        if (string.IsNullOrEmpty(ProjectPassword))
                        {
                            Console.WriteLine("Project password is missing.  Unable to build ispac.");
                            result = false;
                            return result;
                        }

                        project.Password = ProjectPassword;
                    }

                    // Add parameters to project
                    string projectParameterPath = GetProjectParameterPath(projectDirectory);
                    var projectParameters = LoadProjectParameters(projectParameterPath);

                    foreach (var p in projectParameters.Parameters)
                    {
                        Console.WriteLine(string.Format("Adding parameter {0}", p.Name));
                        var parameter = project.Parameters.Add(p.Name, (TypeCode)Int32.Parse(p.Properties["DataType"]));
                        parameter.LoadFromXML(p.GetXml(), new DefaultEvents());
                    }

                    // Set parameter values from configuration
                    var parameterSet = new Dictionary<string, ConfigurationSetting>();
                    if (ProjectConfiguration != null)
                    {
                        foreach (string key in ProjectConfiguration.Options.ParameterConfigurationValues.Keys)
                        {
                            // check if it's a GUID
                            Guid guid;
                            if (Guid.TryParse(key, out guid))
                            {
                                var setting = ProjectConfiguration.Options.ParameterConfigurationValues[key];
                                Console.WriteLine(string.Format("Setting conf value: {0}  = {1}", setting.Name.Replace("Project::", ""), setting.Value));
                                project.Parameters[setting.Name.Replace("Project::", "")].Value = setting.Value;
                                parameterSet.Add(key, setting);
                            }
                        }
                    }

                    // Add connections to project
                    var connectionManagerSerializer = new XmlSerializer(typeof(ProjectConnectionManager));
                    foreach (var c in Manifest.ConnectionManagers)
                    {
                        var path = GetConnectionManagerPath(projectDirectory, c);
                        Console.WriteLine(string.Format("Loading connection manager {0}",path));

                        var cmXml = File.ReadAllText(path);
                        var connMgr = (ProjectConnectionManager)connectionManagerSerializer.Deserialize(new StringReader(cmXml));

                        var cm = project.ConnectionManagerItems.Add(connMgr.CreationName, c.Name);

                        cm.Load(null, File.OpenRead(path));
                    }

                    // Add packages to project
                    foreach (var item in Manifest.Packages)
                    {
                        var packagePath = GetPackagePath(projectDirectory, item);
                        var package = LoadPackage(packagePath);

                        // check the protection level
                        if (package.ProtectionLevel != protectionLevel)
                        {
                            Console.WriteLine(string.Format("Package protection level {0} different from Project protection level {1}.", package.ProtectionLevel, protectionLevel));
                            package.ProtectionLevel = protectionLevel;
                            if (PasswordNeeded(protectionLevel))
                            {
                                package.PackagePassword = ProjectPassword;
                            }
                        }

                        // set package parameters
                        if (parameterSet.Count > 0)
                        {
                            SetParameterConfigurationValues(package.Parameters, parameterSet);
                        }

                        project.PackageItems.Add(package, item.Name);
                        project.PackageItems[item.Name].EntryPoint = item.EntryPoint;
                    }

                    // set project overrides
                    var version = GetProjectVersion();
                    if (version != null)
                    {
                        project.VersionMajor = version.Major;
                        project.VersionMinor = version.Minor;
                        project.VersionBuild = version.Build;
                    }

                    if (VersionComments != null)
                    {
                        project.VersionComments = VersionComments;
                    }

                    // Save project
                    Console.WriteLine(string.Format("Saving ispac to {0}", OutputISPAC));

                    project.SaveTo(OutputISPAC);
                    project.Dispose();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Error occurred: '{0}'\r\nStack Trace {1}", e.Message, e.StackTrace));
                result = false;
            }
            return result;
        }

        private bool PasswordNeeded(DTSProtectionLevel level)
        {
            return (level == DTSProtectionLevel.EncryptAllWithPassword ||
                    level == DTSProtectionLevel.EncryptSensitiveWithPassword);
        }

        private void SetParameterConfigurationValues(Parameters parameters, IDictionary<string, ConfigurationSetting> set)
        {
            foreach (Microsoft.SqlServer.Dts.Runtime.Parameter parameter in parameters)
            {
                if (set.ContainsKey(parameter.ID))
                {
                    var configSetting = set[parameter.ID];
                    parameter.Value = configSetting.Value;

                    Console.WriteLine(string.Format("Configuring settings {0}", configSetting.Name));

                    // remove parameter
                    set.Remove(parameter.ID);

                    if (set.Count == 0)
                    {
                        break;
                    }
                }
            }
        }

        private string GetOutputPath(string outputDirectory)
        {
            string outputPath = ProjectConfiguration.Options.OutputPath;
            string path = Path.Combine(outputDirectory, outputPath, ProjectConfiguration.Name);
            // make sure it exists
            Directory.CreateDirectory(path);
            return path;
        }

        private DTSProtectionLevel GetProtectionLevel(ProjectManifest manifest)
        {
            var level = manifest.ProtectionLevel;
            if (ProtectionLevel != null)
            {
                level = (DTSProtectionLevel)Enum.Parse(typeof(DTSProtectionLevel), ProtectionLevel, true);
            }

            return level;
        }

        private string GetConnectionManagerPath(string projectDirectory, ConnectionManager connectionManager)
        {
            return Path.Combine(projectDirectory, connectionManager.Name);
        }

        private string GetProjectParameterPath(string projectDirectory)
        {
            return Path.Combine(projectDirectory, "Project.params");
        }

        private ProjectParameters LoadProjectParameters(string file)
        {
            var serializer = new XmlSerializer(typeof(ProjectParameters));
            var fileStream = File.OpenRead(file);
            return (ProjectParameters)serializer.Deserialize(fileStream);
        }

        private void SetProjectProperties(Project project, ProjectManifest manifest)
        {
            // set the properties we care about
            foreach (var prop in manifest.Properties.Keys)
            {
                switch (prop)
                {
                    case "Name":
                        project.Name = manifest.Properties[prop];
                        break;
                    case "VersionMajor":
                        project.VersionMajor = Int32.Parse(manifest.Properties[prop]);
                        break;
                    case "VersionMinor":
                        project.VersionMinor = Int32.Parse(manifest.Properties[prop]);
                        break;
                    case "VersionBuild":
                        project.VersionBuild = Int32.Parse(manifest.Properties[prop]);
                        break;
                    case "VersionComments":
                        project.VersionComments = manifest.Properties[prop];
                        break;
                    case "Description":
                        project.Description = manifest.Properties[prop];
                        break;
                }
            }
        }

        private string GetPackagePath(string projectDirectory, PackageManifest package)
        {
            return Path.Combine(projectDirectory, package.Name);
        }

        private string GetProjectFilePath(string outputDirectory, Project project)
        {
            string path = Path.Combine(outputDirectory, project.Name);
            return Path.ChangeExtension(path, ".ispac");
        }

        private Version GetProjectVersion()
        {
            Version ver = null;
            if (Version != null)
            {
                ver = new Version(Version);
            }

            return ver;
        }

        private Package LoadPackage(string path)
        {
            Package pkg;

            Console.WriteLine(string.Format("Loading SSIS package {0}", path));
            try
            {
                var xml = File.ReadAllText(path);

                pkg = new Package { IgnoreConfigurationsOnLoad = true, CheckSignatureOnLoad = false, OfflineMode = true };
                pkg.LoadFromXML(xml, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Error {0}\r\nWith stack trace {1}", e.Message, e.StackTrace));
                throw;
            }

            return pkg;
        }

        private void DeserializeProject(string project)
        {
            Console.WriteLine(string.Format("Loading project {0}", project));

            var xmlOverrides = new XmlAttributeOverrides();
            ProjectConfigurationOptions.PrepareSerializationOverrides(typeof(DataTransformationsProjectConfigurationOptions), SerializationLevel.Project, xmlOverrides);

            // Read project file
            var serializer = new XmlSerializer(typeof(ProjectSerialization), xmlOverrides);
            var fileStream = File.OpenRead(project);
            VsProject = (ProjectSerialization)serializer.Deserialize(fileStream);

            // Read project deployment manifest
            if (VsProject.DeploymentModel == DeploymentModel.Project)
            {
                serializer = new XmlSerializer(typeof(ProjectManifest));
                var reader = new StringReader(VsProject.DeploymentModelSpecificXmlNode.InnerXml);
                Manifest = (ProjectManifest)serializer.Deserialize(reader);

                // TODO: read user settings - do we need to do this for MSBuild?
            }
        }
    }
}
