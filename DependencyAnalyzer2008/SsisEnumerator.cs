///
/// Microsoft SQL Server 2008 Business Intelligence Metadata Reporting Samples
/// Dependency Analyzer Sample
/// 
/// Copyright (c) Microsoft Corporation.  All rights reserved.
///
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Text;
using System.Xml;


using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using DTR = Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System.Collections.Specialized;
using TSQLParser;
#if SQL2005
using IDTSComponentMetaData = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData90;
using IDTSPipeline = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPipeline90;
using IDTSInput = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInput90;
using IDTSInputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInputColumn90;
using IDTSOutput = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutput90;
using IDTSOutputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutputColumn90;
using IDTSRuntimeConnection = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSRuntimeConnection90;
using IDTSPath = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPath90;
using IDTSCustomProperty = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSCustomProperty90;
#else
using IDTSComponentMetaData = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData100;
using IDTSPipeline = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPipeline100;
using IDTSInput = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInput100;
using IDTSInputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInputColumn100;
using IDTSOutput = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutput100;
using IDTSOutputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutputColumn100;
using IDTSRuntimeConnection = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSRuntimeConnection100;
using IDTSPath = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPath100;
using IDTSCustomProperty = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSCustomProperty100;
#endif

namespace Microsoft.Samples.DependencyAnalyzer
{
    /// <summary>
    ///  Enumerates Integration Services objects and the relationships between them. The result of this enumeration is persisted
    ///  in a repository.
    /// </summary>
    class SSISEnumerator
    {
        // 5 access modes used by ole db adapters
        enum AccessMode : int
        {
            AM_OPENROWSET = 0,
            AM_OPENROWSET_VARIABLE = 1,
            AM_SQLCOMMAND = 2,
            AM_SQLCOMMAND_VARIABLE = 3,
            AM_OPENROWSET_FASTLOAD_VARIABLE = 4
        }

        enum SqlStatementSourceType : int
        {
            DirectInput = 1,
            FileConnection = 2,
            Variable = 3
        }

        private Application app;
        private Repository repository;

        private const string dtsxPattern = "*.dtsx";
        private bool threePartNames;

        /// <summary>
        /// Different component Class IDs that we understand about
        /// </summary>
        private class ClassIDs
        {
#if SQL2005
            internal const string OleDbSource = "{2C0A8BE5-1EDC-4353-A0EF-B778599C65A0}";
            internal const string ExcelSource = "{B551FCA8-23BD-4719-896F-D8F352A5283C}";
            internal const string FlatFileSource = "{90C7770B-DE7C-435E-880E-E718C92C0573}";
            internal const string RawFileSource = "{E2568105-9550-4F71-A638-B7FE42E66930}";
            internal const string XmlSource = "Microsoft.SqlServer.Dts.Pipeline.XmlSourceAdapter, Microsoft.SqlServer.XmlSrc, Version=9.0.242.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

            internal const string OleDbDestination = "{E2568105-9550-4F71-A638-B7FE42E66922}";
            internal const string SqlDestination = "{5244B484-7C76-4026-9A01-00928EA81550}";
            internal const string FlatFileDest = "{A1DF9F6D-8EE4-4EF0-BB2E-D526130D7A7B}";
            internal const string RawFileDest = "{E2568105-9550-4F71-A638-B7FE42E66933}";

            internal const string Lookup = "{0FB4AABB-C027-4440-809A-1198049BF117}";
            internal const string FuzzyLookup = "{9F4EB4D4-AD71-496D-B70B-31ECE1139884}";

            internal const string ManagedComponentWrapper = "{bf01d463-7089-41ee-8f05-0a6dc17ce633}";

            internal const string DerivedColumn = "{9CF90BF0-5BCC-4C63-B91D-1F322DC12C26}";
            internal const string MultipleHash = "Martin.SQLServer.Dts.MultipleHash, MultipleHash2005, Version=1.0.0.0, Culture=neutral, PublicKeyToken=51c551904274ab44";
            internal const string KimballSCD = "MouldingAndMillwork.SSIS.KimballMethodSCD, KimballMethodSCD90, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8b0551303405e96c";
            internal const string OLEDBCommand = "{C60ACAD1-9BE8-46B3-87DA-70E59EADEA46}";
#endif
#if SQL2008
            internal const string OleDbSource = "{BCEFE59B-6819-47F7-A125-63753B33ABB7}";
            internal const string ExcelSource = "{A4B1E1C8-17F3-46C8-AAD0-34F0C6FE42DE}";
            internal const string FlatFileSource = "{5ACD952A-F16A-41D8-A681-713640837664}";
            internal const string RawFileSource = "{51DC0B24-7421-45C3-B4AB-9481A683D91D}";
            internal const string XmlSource = "Microsoft.SqlServer.Dts.Pipeline.XmlSourceAdapter, Microsoft.SqlServer.XmlSrc, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

            internal const string OleDbDestination = "{5A0B62E8-D91D-49F5-94A5-7BE58DE508F0}";
            internal const string SqlDestination = "{E4B61516-847B-4BDF-9CC6-1968A2D43E73}";
            internal const string FlatFileDest = "{D658C424-8CF0-441C-B3C4-955E183B7FBA}";
            internal const string RawFileDest = "{485E7329-8754-42B4-AA5B-29C5DA09CAD5}";
            internal const string ExcelDestination = "{C9269E28-EBDE-4DED-91EB-0BF42842F9F4}";

            internal const string Lookup = "{27648839-180F-45E6-838D-AFF53DF682D2}";
            internal const string FuzzyLookup = "{5056651F-F227-4978-94DF-53CDF9E8CCB6}";

            internal const string ManagedComponentWrapper = "{2E42D45B-F83C-400F-8D77-61DDE6A7DF29}";

            internal const string DerivedColumn = "{2932025B-AB99-40F6-B5B8-783A73F80E24}";
            internal const string MultipleHash = "Martin.SQLServer.Dts.MultipleHash, MultipleHash2008, Version=1.0.0.0, Culture=neutral, PublicKeyToken=51c551904274ab44";
            internal const string KimballSCD = "MouldingAndMillwork.SSIS.KimballMethodSCD, KimballMethodSCD100, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8b0551303405e96c";
            internal const string OLEDBCommand = "{8E61C8F6-C91D-43B6-97EB-3423C06571CC}";
#endif
#if SQL2012
            internal const string OleDbSource = "{165A526D-D5DE-47FF-96A6-F8274C19826B}";
            internal const string ExcelSource = "{8C084929-27D1-479F-9641-ABB7CDADF1AC}";
            internal const string FlatFileSource = "{D23FD76B-F51D-420F-BBCB-19CBF6AC1AB4}";
            internal const string RawFileSource = "{480C7D5A-CE63-405C-B338-3C7F26560EE3}";
            internal const string XmlSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}"; //"Microsoft.SqlServer.Dts.Pipeline.XmlSourceAdapter, Microsoft.SqlServer.XmlSrc, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

            internal const string OleDbDestination = "{4ADA7EAA-136C-4215-8098-D7A7C27FC0D1}";
            internal const string SqlDestination = "{F452EAF3-5EF0-43F1-8067-09DDF0BC6316}";
            internal const string FlatFileDest = "{8DA75FED-1B7C-407D-B2AD-2B24209CCCA4}";
            internal const string RawFileDest = "{04762BB6-892F-4EE6-AD46-9CEB0A7EC7A2}";
            internal const string ExcelDestination = "{1F5D5712-2FBA-4CB9-A95A-86C1F336E1DA}";

            internal const string Lookup = "{671046B0-AA63-4C9F-90E4-C06E0B710CE3}";
            internal const string FuzzyLookup = "{E4A5F949-EC93-45AB-8B36-B52936257EC2}";

            internal const string ManagedComponentWrapper = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}"; //Script Component? "{2E42D45B-F83C-400F-8D77-61DDE6A7DF29}";

            internal const string DerivedColumn = "{49928E82-9C4E-49F0-AABE-3812B82707EC}";
            internal const string MultipleHash = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}"; // "Martin.SQLServer.Dts.MultipleHash, MultipleHash2012, Version=1.0.0.0, Culture=neutral, PublicKeyToken=51c551904274ab44";
            internal const string KimballSCD = "MouldingAndMillwork.SSIS.KimballMethodSCD, KimballMethodSCD100, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8b0551303405e96c";
            internal const string OLEDBCommand = "{93FFEC66-CBC8-4C7F-9C6A-CB1C17A7567D}";

            //internal const string ADONetSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string CDCSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string DataMiningModel = "{3D9FFAE9-B89B-43D9-80C8-B97D2740C746}";
            //internal const string DataReaderDestination = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string DimensionProcessing = "{2C2F0891-3AAA-4865-A676-D7476FE4CE90}";
            //internal const string ODBCDestination = "{074B8736-CD73-40A5-822E-888215AF57DA}";
            //internal const string ODBCSource = "{A77F5655-A006-443A-9B7E-90B6BD55CB84}";
            //internal const string PartitionProcessing = "{DA510FB7-E3A8-4D96-9F59-55E15E67FE3D}";
            //internal const string RecordSetDestination = "{C457FD7E-CE98-4C4B-AEFE-F3AE0044F181}";
            //internal const string SQLServerCompactDestination = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";


#endif
#if SQL2014
            internal const string OleDbSource = "{C8D886B3-1825-4FCC-94DE-C3F108986E21}";
            internal const string ExcelSource = "{9F5C585F-2F02-4622-B273-F75D52419D4A}";
            internal const string FlatFileSource = "{C4D48377-EFD6-4C95-9A0B-049219453431}";
            internal const string RawFileSource = "{480C7D5A-CE63-405C-B338-3C7F26560EE3}";
            internal const string XmlSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}"; //"Microsoft.SqlServer.Dts.Pipeline.XmlSourceAdapter, Microsoft.SqlServer.XmlSrc, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

            internal const string OleDbDestination = "{4ADA7EAA-136C-4215-8098-D7A7C27FC0D1}";
            internal const string SqlDestination = "{F452EAF3-5EF0-43F1-8067-09DDF0BC6316}";
            internal const string FlatFileDest = "{FD4FFB90-EECF-4B5A-A3A7-DE2E1FA8052C}";
            internal const string RawFileDest = "{04762BB6-892F-4EE6-AD46-9CEB0A7EC7A2}";
            internal const string ExcelDestination = "{90E2E609-1207-4CB0-A8CE-CC7B8CFE2510}";

            internal const string Lookup = "{9345248B-9709-4C04-90C1-0853F8B68EE8}";
            internal const string FuzzyLookup = "{AD9B9B83-DB60-4188-B57D-93C5155DFACC}";

            internal const string ManagedComponentWrapper = "{33D831DE-5DCF-48F0-B431-4D327B9E785D}"; //Script Component? "{2E42D45B-F83C-400F-8D77-61DDE6A7DF29}";

            internal const string DerivedColumn = "{18E9A11B-7393-47C5-9D47-687BE04A6B09}";
            internal const string MultipleHash = "Martin.SQLServer.Dts.MultipleHash, MultipleHash2014, Version=1.0.0.0, Culture=neutral, PublicKeyToken=51c551904274ab44";
            internal const string KimballSCD = "MouldingAndMillwork.SSIS.KimballMethodSCD, KimballMethodSCD100, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8b0551303405e96c";
            internal const string OLEDBCommand = "{93FFEC66-CBC8-4C7F-9C6A-CB1C17A7567D}";

            //internal const string ADONetSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string CDCSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string DataMiningModel = "{3D9FFAE9-B89B-43D9-80C8-B97D2740C746}";
            //internal const string DataReaderDestination = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string DimensionProcessing = "{2C2F0891-3AAA-4865-A676-D7476FE4CE90}";
            //internal const string ODBCDestination = "{074B8736-CD73-40A5-822E-888215AF57DA}";
            //internal const string ODBCSource = "{A77F5655-A006-443A-9B7E-90B6BD55CB84}";
            //internal const string PartitionProcessing = "{DA510FB7-E3A8-4D96-9F59-55E15E67FE3D}";
            //internal const string RecordSetDestination = "{C457FD7E-CE98-4C4B-AEFE-F3AE0044F181}";
            //internal const string SQLServerCompactDestination = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";


#endif
        }

        private const string objectTypePackage = "SSIS Package";

        private const string attributePackageLocation = "PackageLocation";
        private const string attributePackageID = "PackageGUID";

        private OleDbConnectionStringBuilder connectionStringBuilder = new OleDbConnectionStringBuilder();

        /// <summary>
        /// information about registered pipeline components
        /// </summary>
        private PipelineComponentInfos pipelineComponentInfos = null;

        /// <summary>
        ///  information about registered connections
        /// </summary>
        private ConnectionInfos connectionInfos = null;

        /// <summary>
        /// information about registered tasks
        /// </summary>
        private TaskInfos taskInfos = null;

        private Dictionary<string, string> connectionTypeToIDMap = null;

        public SSISEnumerator()
        {
        }

        public bool Initialize(Repository repository)
        {
            bool success;

            try
            {
                this.repository = repository;

                // lets handle the infos enumeration right here.
                app = new Application();

                EnumerateInfos(app);

                success = true;
                threePartNames = false;

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(string.Format("Could not initialize the SSIS Packages Enumerator: {0}", ex.Message));
                success = false;
            }

            return success;
        }

        /// <summary>
        ///  enumerates all packages stored off in SQL Server database
        /// </summary>
        /// <param name="sqlConnectionString"></param>
        public void EnumerateSqlPackages(string server, string user, string pwd, string[] rootFolders, bool storeThreePartNames)
        {
            threePartNames = storeThreePartNames;
            try
            {
                Queue<string> folders = new Queue<string>();

                foreach (string folderName in rootFolders)
                {
                    folders.Enqueue(folderName);
                }

                if (folders.Count == 0)
                {
                    folders.Enqueue("\\"); // the root folder
                }

                do
                {
                    string folder = folders.Dequeue();

                    PackageInfos packageInfos = app.GetPackageInfos(folder, server, user, pwd);
                    foreach (PackageInfo packageInfo in packageInfos)
                    {
                        string location = packageInfo.Folder + "\\" + packageInfo.Name;
                        if (packageInfo.Flags == DTSPackageInfoFlags.Folder)
                        {
                            folders.Enqueue(location);
                        }
                        else
                        {
                            Debug.Assert(packageInfo.Flags == DTSPackageInfoFlags.Package);

                            try
                            {
                                Console.Write(string.Format("Loading SQL package '{0}'... ", location));
                                using (Package package = app.LoadFromSqlServer(location, server, user, pwd, null))
                                {
                                    EnumeratePackage(package, location);
                                }
                                Console.WriteLine("Completed Successfully.");
                            }
                            catch (System.Exception ex2)
                            {
                                Console.WriteLine(string.Format("Error occurred: '{0}'", ex2.Message));
                            }
                        }
                    }
                } while (folders.Count > 0);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(string.Format("Error enumerating packages on SQL Server '{0}': {1}", server, ex.Message));
            }
        }

        /// <summary>
        /// Enumerates all packages that're in a directory and all sub directories if user asked us to.
        /// </summary>
        /// <param name="rootFolders"></param>
        /// <param name="recurseSubFolders"></param>
        public void EnumerateFileSystemPackages(string[] rootFolders, bool recurseSubFolders, bool storeThreePartNames)
        {
            threePartNames = storeThreePartNames;
            foreach (string rootFolder in rootFolders)
            {
                if (System.IO.Directory.Exists(rootFolder) == false)
                {
                    throw new Exception(string.Format("Root package folder '{0}' doesn't exist.", rootFolder));
                }

                EnumeratePackages(rootFolder, dtsxPattern, recurseSubFolders);
            }
        }

        private void EnumeratePackages(string rootFolder, string pattern, bool recurseSubFolders)
        {
            Console.Write("Enumerating packages...");
            string[] filesToInspect = System.IO.Directory.GetFiles(rootFolder, pattern, (recurseSubFolders) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);

            Console.WriteLine("done.");

            foreach (string packageFileName in filesToInspect)
            {
                try
                {
                    Console.Write(string.Format("Loading file package '{0}'... ", packageFileName));

                    // load the package
                    using (Package package = app.LoadPackage(packageFileName, null))
                    {
                        EnumeratePackage(package, packageFileName);
                    }

                    Console.WriteLine("Completed Successfully.");
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(string.Format("Error occurred: '{0}'", ex.Message));
                }
            }
        }

        private void EnumerateInfos(Application app)
        {
            Console.Write("Enumerating registered SSIS Data Flow components...");
            pipelineComponentInfos = app.PipelineComponentInfos;
            foreach (PipelineComponentInfo pipelineComponentInfo in pipelineComponentInfos)
            {
                repository.AddObjectType(Repository.Domains.SSIS, pipelineComponentInfo.ID, pipelineComponentInfo.Name, pipelineComponentInfo.Description);
            }
            Console.WriteLine("Done");

            Console.Write("Enumerating registered SSIS Connection Managers...");

            connectionInfos = app.ConnectionInfos;
            connectionTypeToIDMap = new Dictionary<string, string>(connectionInfos.Count);
            foreach (ConnectionInfo connectionInfo in connectionInfos)
            {
                connectionTypeToIDMap.Add(connectionInfo.ConnectionType, connectionInfo.ID);
                repository.AddObjectType(Repository.Domains.SSIS, connectionInfo.ID, connectionInfo.Name, connectionInfo.Description);
            }

            Console.WriteLine("Done");

            Console.Write("Enumerating registered SSIS Tasks...");

            taskInfos = app.TaskInfos;
            foreach (TaskInfo taskInfo in taskInfos)
            {
                repository.AddObjectType(Repository.Domains.SSIS, taskInfo.ID, taskInfo.Name, taskInfo.Description);
            }

            Console.WriteLine("Done");
        }

        /// <summary>
        /// recurses through all dtsContainers looking for any pipelines...
        /// Now handles ForEachLoop, ForLoop and Sequence Containers
        /// Disabled Tasks are not tested...
        /// </summary>
        /// <param name="package">The SSIS Package that is being investigated</param>
        /// <param name="location">The text string of the location of this package</param>
        /// <param name="packageRepositoryID">The internal ID for this package</param>
        /// <param name="dtsContainer">The Container that is to be assessed</param>
        private void EnumerateTask(Package package, string location, int packageRepositoryID, DtsContainer dtsContainer)
        {
            if (!dtsContainer.Disable)
            {
                if (dtsContainer is TaskHost)
                {
                    TaskHost taskHost = dtsContainer as TaskHost;

                    IDTSPipeline pipeline = taskHost.InnerObject as IDTSPipeline;

                    if (pipeline != null)
                    {
                        // this is a data flow task
                        packageRepositoryID = InspectDataFlow(pipeline, packageRepositoryID, package, taskHost, location);
                    }
                    else if (dtsContainer.CreationName.Contains("Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ExecuteSQLTask"))
                    {
                        EnumerateSqlTask(package, location, packageRepositoryID, taskHost);
                    }
                }
                else if (dtsContainer is ForEachLoop)
                {
                    ForEachLoop felContainer = dtsContainer as ForEachLoop;
                    foreach (DtsContainer innerDtsContainer in felContainer.Executables)
                    {
                        EnumerateTask(package, location, packageRepositoryID, innerDtsContainer);
                    }
                }
                else if (dtsContainer is ForLoop)
                {
                    ForLoop flContainer = dtsContainer as ForLoop;
                    foreach (DtsContainer innerDtsContainer in flContainer.Executables)
                    {
                        EnumerateTask(package, location, packageRepositoryID, innerDtsContainer);
                    }
                }
                else if (dtsContainer is Sequence)
                {
                    Sequence sContainer = dtsContainer as Sequence;
                    foreach (DtsContainer innerDtsContainer in sContainer.Executables)
                    {
                        EnumerateTask(package, location, packageRepositoryID, innerDtsContainer);
                    }
                }
            }
        }

        /// <summary>
        /// Handles parsing an SQLTask object, and finding what is used within it.
        /// </summary>
        /// <param name="package">The SSIS Package that is being investigated</param>
        /// <param name="location">The text string of the location of this package</param>
        /// <param name="packageRepositoryID">The internal ID for this package</param>
        /// <param name="taskHost">The SQL Task that is to be investigated.</param>
        private void EnumerateSqlTask(Package package, string location, int packageRepositoryID, TaskHost taskHost)
        {
            int tableID, procID, funcID;
            if (packageRepositoryID == -1)
            {
                // add this package to the repository
                packageRepositoryID = AddObject(package.Name, package.Description, objectTypePackage, repository.RootRepositoryObjectID);
                repository.AddAttribute(packageRepositoryID, attributePackageLocation, location);
                repository.AddAttribute(packageRepositoryID, attributePackageID, package.ID);
            }
            // Add this task to the repository
            int sqlTaskRepositoryObjectID = AddObject(taskHost.Name, taskHost.Description, taskHost.CreationName, packageRepositoryID);
            
            if (taskHost.Properties.Contains("SqlStatementSource") & taskHost.Properties.Contains("SqlStatementSourceType") & taskHost.Properties.Contains("Connection"))
            {
                string queryDefinition = string.Empty;

                SqlStatementSourceType stmtSource = (SqlStatementSourceType)taskHost.Properties["SqlStatementSourceType"].GetValue(taskHost);
                switch (stmtSource)
                {
                    case SqlStatementSourceType.DirectInput:
                        queryDefinition = taskHost.Properties["SqlStatementSource"].GetValue(taskHost).ToString();
                        repository.AddAttribute(sqlTaskRepositoryObjectID, Repository.Attributes.QueryDefinition, queryDefinition);
                        break;
                    case SqlStatementSourceType.FileConnection:
                        break;
                    case SqlStatementSourceType.Variable:
                        queryDefinition = GetVariable(package, taskHost.Properties["SqlStatementSource"].GetValue(taskHost).ToString());
                        repository.AddAttribute(sqlTaskRepositoryObjectID, Repository.Attributes.QueryDefinition, queryDefinition);
                        break;
                    default:
                        throw new Exception(string.Format("Invalid Sql Statement Source Type {0}.", stmtSource));
                }
                ConnectionManager connectionManager = package.Connections[taskHost.Properties["Connection"].GetValue(taskHost).ToString()];

                    string connectionManagerType = connectionManager.CreationName;
                    int connectionID = repository.GetConnection(connectionManager.ConnectionString);

                    string serverName = null;
                    if (connectionID == -1)
                    {
                        connectionID = CreateConnection(connectionManager, out serverName);
                    }

                    if (!string.IsNullOrEmpty(queryDefinition))
                    {
                        try
                        {
                            SqlStatement toBeParsed = new SqlStatement();

                            //toBeParsed.sqlString = statement;
                            toBeParsed.quotedIdentifiers = true;
                            if (toBeParsed.ParseString(queryDefinition))
                            {
                                foreach (string tableName in toBeParsed.getTableNames(true))
                                {
                                    if (threePartNames)
                                    {
                                        String dbName = repository.RetrieveDatabaseNameFromConnectionID(connectionID);
                                        tableID = repository.GetTable(connectionID, String.Format("[{0}].{1}", dbName, tableName));
                                    }
                                    else
                                        tableID = repository.GetTable(connectionID, tableName);
                                    repository.AddMapping(tableID, sqlTaskRepositoryObjectID);
                                }
                                foreach (string procedureName in toBeParsed.getProcedureNames(true))
                                {
                                    if (threePartNames)
                                    {
                                        String dbName = repository.RetrieveDatabaseNameFromConnectionID(connectionID);
                                        procID = repository.GetProcedure(connectionID, String.Format("[{0}].{1}", dbName, procedureName));
                                    }
                                    else
                                        procID = repository.GetProcedure(connectionID, procedureName);
                                    repository.AddMapping(procID, sqlTaskRepositoryObjectID);
                                }
                                foreach (string funcName in toBeParsed.getFunctionNames(true))
                                {
                                    if (threePartNames)
                                    {
                                        String dbName = repository.RetrieveDatabaseNameFromConnectionID(connectionID);
                                        funcID = repository.GetFunction(connectionID, String.Format("[{0}].{1}", dbName, funcName));
                                    }
                                    else
                                        funcID = repository.GetFunction(connectionID, funcName);
                                    repository.AddMapping(funcID, sqlTaskRepositoryObjectID);
                                }
                            }
                            else
                            {
                                string errorMessage = "The following messages where generated whilst parsing the sql statement\r\n" + queryDefinition + "\r\n";
                                foreach (string error in toBeParsed.parseErrors)
                                {
                                    errorMessage += error + "\r\n";
                                }
                                Console.WriteLine(errorMessage);
                            }
                        }
                        catch (System.Exception err)
                        {
                            Console.WriteLine("The exception \r\n{0}\r\nwas raised against query \r\n{1}\r\nPlease report to http://sqlmetadata.codeplex.com/WorkItem/List.aspx\r\n", err.Message, queryDefinition);
                        }
                    }
            }
            //ExecuteSQLTask sqlTask = taskHost.InnerObject as ExecuteSQLTask;
            //string queryDefinition = sqlTask.SqlStatementSource;
            //// add the query definition attribute if we have one
            //if (queryDefinition != null)
            //{
            //    repository.AddAttribute(sqlTaskRepositoryObjectID, Repository.Attributes.QueryDefinition, queryDefinition);
            //}
        }

        /// <summary>
        /// handles loading information about a package to the repository
        /// </summary>
        /// <param name="package"></param>
        private void EnumeratePackage(Package package, string location)
        {
            // add this package to the repository
            int packageRepositoryID = AddObject(package.Name, package.Description, objectTypePackage, repository.RootRepositoryObjectID);
            repository.AddAttribute(packageRepositoryID, attributePackageLocation, location);
            repository.AddAttribute(packageRepositoryID, attributePackageID, package.ID);

            // loop through all data flow tasks
            foreach (DtsContainer dtsContainer in package.Executables)
            {
                EnumerateTask(package, location, packageRepositoryID, dtsContainer);
            }

            //foreach (Executable executable in package.Executables)
            //{
            //    if (executable is TaskHost)
            //    {
            //        if ((executable as TaskHost).InnerObject is ExecuteSQLTask)
            //        {
            //            EnumerateSqlTask(package, location, packageRepositoryID, (executable as TaskHost));
            //        }
            //    }
            //}
        }

        /// <summary>
        /// adds interesting components to the repository. If at least one interesting object exists in the data flow task 
        /// the data flow task is also added to the repository. If at least one such interesting data flow task exists in the
        /// package and the package hasn't been created in the repository as yet, it will be created as well. What's returned
        /// is the package repository ID.
        /// </summary>
        /// <param name="packageRepositoryID"></param>
        /// <param name="package"></param>
        /// <param name="taskHost"></param>
        /// <returns></returns>
        private int InspectDataFlow(IDTSPipeline pipeline, int packageRepositoryID, Package package, TaskHost taskHost, string packageLocation)
        {
            int dataFlowRepositoryObjectID = -1;

            Dictionary<int, int> componentIDToSourceRepositoryObjectMap = new Dictionary<int, int>();
            Dictionary<int, int> componentIDToRepositoryObjectMap = new Dictionary<int, int>();

            // go through all components looking for sources or destinations
            foreach (IDTSComponentMetaData component in pipeline.ComponentMetaDataCollection)
            {
                EnumerateDataFlowComponent(ref packageRepositoryID, package, taskHost, packageLocation, ref dataFlowRepositoryObjectID, ref componentIDToSourceRepositoryObjectMap, ref componentIDToRepositoryObjectMap, component);
            }

            // after all the objects have been added, traverse their mappings
            if (componentIDToSourceRepositoryObjectMap.Count > 0)
            {
                EnumerateMappings(pipeline, dataFlowRepositoryObjectID, componentIDToSourceRepositoryObjectMap, componentIDToRepositoryObjectMap);
            }

            return packageRepositoryID;
        }

        private void EnumerateDataFlowComponent(ref int packageRepositoryID, Package package, TaskHost taskHost, string packageLocation, ref int dataFlowRepositoryObjectID, ref Dictionary<int, int> componentIDToSourceRepositoryObjectMap, ref Dictionary<int, int> componentIDToRepositoryObjectMap, IDTSComponentMetaData component)
        {
            string objectType;
            DTSPipelineComponentType dataFlowComponentType;
            string domain, tableOrViewName, queryDefinition;
            bool tableOrViewSource;
            string kimballPropertyValue;
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            XmlReader objXMLText;

            // if we have a source, note it as the starting point and add it to the repository
            if (IsComponentInteresting(package, component, out objectType, out dataFlowComponentType, out domain, out tableOrViewName, out tableOrViewSource, out queryDefinition))
            {
                // if the data flow object itself hasn't been created as  yet, do so now
                if (dataFlowRepositoryObjectID == -1)
                {
                    // if the package itself doesn't exist either, create it now
                    if (packageRepositoryID == -1)
                    {
                        // add this package to the repository
                        packageRepositoryID = AddObject(package.Name, package.Description, objectTypePackage, repository.RootRepositoryObjectID);
                        repository.AddAttribute(packageRepositoryID, attributePackageLocation, packageLocation);
                        repository.AddAttribute(packageRepositoryID, attributePackageID, package.ID);
                    }

                    dataFlowRepositoryObjectID = AddObject(taskHost.Name, taskHost.Description, taskHost.CreationName, packageRepositoryID);
                }

                // add the component to the repository
                int componentRepositoryID = AddObject(component.Name, component.Description, objectType, dataFlowRepositoryObjectID);

                Debug.Assert(componentRepositoryID > 0);

                if (dataFlowComponentType == DTSPipelineComponentType.SourceAdapter)
                {
                    componentIDToSourceRepositoryObjectMap.Add(component.ID, componentRepositoryID);
                }

                componentIDToRepositoryObjectMap.Add(component.ID, componentRepositoryID);

                // add the query definition attribute if we have one
                if (queryDefinition != null)
                {
                    repository.AddAttribute(componentRepositoryID, Repository.Attributes.QueryDefinition, queryDefinition);


                }
                if (component.ComponentClassID == ClassIDs.OLEDBCommand)
                {
                    GetStringComponentPropertyValue(component, "SqlCommand");
                }
                else if (component.ComponentClassID == ClassIDs.DerivedColumn)
                {
                    foreach (IDTSInput localInput in component.InputCollection)
                    {
                        if (localInput.IsAttached)
                        {
                            foreach (IDTSInputColumn localIColumn in localInput.InputColumnCollection)
                            {
                                if (localIColumn.CustomPropertyCollection.Count == 2)
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale) + " Expression " + localIColumn.CustomPropertyCollection["FriendlyExpression"].Value.ToString());
                                }
                                else
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale) + " Expression (See Ouput Column)");
                                }
                                //repository.AddObject(localIColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                            }
                        }
                    }
                    foreach (IDTSOutput localOutput in component.OutputCollection)
                    {
                        if (localOutput.IsAttached)
                        {
                            foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                            {
                                if (localOColumn.CustomPropertyCollection.Count == 2)
                                {
                                    repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale) + " Expression " + localOColumn.CustomPropertyCollection["FriendlyExpression"].Value.ToString());
                                }
                                //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                                //ToDo: Add connection from Input to Output Column.
                            }
                        }
                    }
                }
                else if (component.ComponentClassID == ClassIDs.Lookup)
                {
                    foreach (IDTSInput localInput in component.InputCollection)
                    {
                        if (localInput.IsAttached)
                        {
                            foreach (IDTSInputColumn localIColumn in localInput.InputColumnCollection)
                            {
                                if (localIColumn.CustomPropertyCollection.Count == 2)
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale) + " Reference Column [" + localIColumn.CustomPropertyCollection["JoinToReferenceColumn"].Value.ToString() + "]");
                                }
                                else
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale) + " Expression (See Ouput Column)");
                                }
                                //repository.AddObject(localIColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                            }
                        }
                    }
                    foreach (IDTSOutput localOutput in component.OutputCollection)
                    {
                        if (localOutput.IsAttached)
                        {
                            foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                            {
                                if (localOColumn.CustomPropertyCollection.Count == 1)
                                {
                                    repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale) + " Return Column [" + localOColumn.CustomPropertyCollection["CopyFromReferenceColumn"].Value.ToString() + "]");
                                }
                                //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                            }
                        }
                    }
                }
                else if (component.ComponentClassID == ClassIDs.FuzzyLookup)
                {
                    foreach (IDTSInput localInput in component.InputCollection)
                    {
                        if (localInput.IsAttached)
                        {
                            foreach (IDTSInputColumn localIColumn in localInput.InputColumnCollection)
                            {
                                if (localIColumn.CustomPropertyCollection.Count == 5)
                                {
                                    if (localIColumn.CustomPropertyCollection["JoinToReferenceColumn"].Value != null)
                                    {
                                        repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale) + " Reference Column [" + localIColumn.CustomPropertyCollection["JoinToReferenceColumn"].Value.ToString() + "]");
                                    }
                                    else
                                    {
                                        repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale));
                                    }
                                }
                                else
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale) + " Expression (See Ouput Column)");
                                }
                                //repository.AddObject(localIColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                            }
                        }
                    }
                    foreach (IDTSOutput localOutput in component.OutputCollection)
                    {
                        if (localOutput.IsAttached)
                        {
                            foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                            {
                                if (localOColumn.CustomPropertyCollection.Count == 3)
                                {
                                    if (localOColumn.CustomPropertyCollection["CopyFromReferenceColumn"].Value != null)
                                    {
                                        repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale) + " Return Column [" + localOColumn.CustomPropertyCollection["CopyFromReferenceColumn"].Value.ToString() + "]");
                                    }
                                    else
                                    {
                                        repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale));
                                    }
                                }
                                else
                                {
                                    repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale));
                                }
                                //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                            }
                        }
                    }
                }
                else if (component.ComponentClassID == ClassIDs.ManagedComponentWrapper)
                {
                    if (component.CustomPropertyCollection["UserComponentTypeName"].Value.ToString() == ClassIDs.MultipleHash)
                    {
                        foreach (IDTSInput localInput in component.InputCollection)
                        {
                            if (localInput.IsAttached)
                            {
                                foreach (IDTSInputColumn localIColumn in localInput.InputColumnCollection)
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale));
                                    //repository.AddObject(localIColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                                }
                            }
                        }
                        foreach (IDTSOutput localOutput in component.OutputCollection)
                        {
                            if (localOutput.IsAttached)
                            {
                                foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                                {
                                    if (localOColumn.CustomPropertyCollection.Count == 2)
                                    {
                                        string localResults = "";
                                        if (localOColumn.CustomPropertyCollection["InputColumnLineageIDs"].Value.ToString().Length > 0)
                                        {
                                            foreach (string localIDs in localOColumn.CustomPropertyCollection["InputColumnLineageIDs"].Value.ToString().Split(','))
                                            {
                                                string withoutHash = localIDs;
                                                if (localIDs[0] == '#')
                                                {
                                                    withoutHash = localIDs.Substring(1);
                                                }
                                                if (localResults.Length == 0)
                                                {
                                                    localResults = component.InputCollection[0].InputColumnCollection.GetInputColumnByLineageID(System.Convert.ToInt32(withoutHash)).Name;
                                                }
                                                else
                                                {
                                                    localResults += ", " + component.InputCollection[0].InputColumnCollection.GetInputColumnByLineageID(System.Convert.ToInt32(withoutHash)).Name;
                                                }
                                            }
                                        }
                                        repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale) + " Generated From " + localResults);
                                    }
                                    //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                                }
                            }
                        }
                    }
                    else if (component.CustomPropertyCollection["UserComponentTypeName"].Value.ToString() == ClassIDs.KimballSCD)
                    {
                        foreach (IDTSInput localInput in component.InputCollection)
                        {
                            if (localInput.IsAttached)
                            {
                                foreach (IDTSInputColumn localIColumn in localInput.InputColumnCollection)
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale));
                                    //repository.AddObject(localIColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                                }
                            }
                        }

                        foreach (IDTSOutput localOutput in component.OutputCollection)
                        {
                            if (localOutput.IsAttached)
                            {
                                kimballPropertyValue = string.Empty;
                                switch (localOutput.Name)
                                {
                                    case "New":
                                        kimballPropertyValue = component.CustomPropertyCollection["New Output Columns"].Value.ToString();
                                        break;
                                    case "Unchanged":
                                        kimballPropertyValue = component.CustomPropertyCollection["Unchanged Output Columns"].Value.ToString();
                                        break;
                                    case "Deleted":
                                        kimballPropertyValue = component.CustomPropertyCollection["Deleted Output Columns"].Value.ToString();
                                        break;
                                    case "Updated SCD1":
                                        kimballPropertyValue = component.CustomPropertyCollection["Update SCD1 Columns"].Value.ToString();
                                        break;
                                    case "Auditing":
                                        kimballPropertyValue = component.CustomPropertyCollection["Audit Columns"].Value.ToString();
                                        break;
                                    case "Expired SCD2":
                                        kimballPropertyValue = component.CustomPropertyCollection["Expired SCD2 Columns"].Value.ToString();
                                        break;
                                    case "Invalid Input":
                                        break;
                                    case "New SCD2":
                                        kimballPropertyValue = component.CustomPropertyCollection["New SCD2 Columns"].Value.ToString();
                                        break;
                                    default:
                                        break;
                                }
                                string localResults = "Unknown.";
                                if (kimballPropertyValue != string.Empty)
                                {
                                    objXMLText = XmlReader.Create(new System.IO.StringReader(kimballPropertyValue), settings);
                                    objXMLText.Read();
                                    foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                                    {
                                        string strLineageID;
                                        string strMappedToExistingDimensionLineageID;

                                        objXMLText.ReadToFollowing("OutputColumnDefinition");
                                        do
                                        {
                                            localResults = "Unknown.";
                                            strLineageID = objXMLText.GetAttribute("LineageID");
                                            strMappedToExistingDimensionLineageID = objXMLText.GetAttribute("MappedToExistingDimensionLineageID");
                                            if (localOColumn.LineageID == System.Convert.ToInt32(strLineageID))
                                            {
                                                foreach (IDTSInput localInput in component.InputCollection)
                                                {
                                                    IDTSInputColumn temp;
                                                    try
                                                    {
                                                        temp = localInput.InputColumnCollection.GetInputColumnByLineageID(System.Convert.ToInt32(strMappedToExistingDimensionLineageID));
                                                        localResults = "[" + localInput.Name + "] Column [" + temp.Name + "].";
                                                        break;
                                                    }
                                                    catch (Exception)
                                                    {
                                                    }
                                                }
                                                break;
                                            }
                                        }
                                        while (objXMLText.ReadToFollowing("OutputColumnDefinition"));

                                        repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale) + " Generated From " + localResults);
                                        //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                                    }
                                }
                                else
                                {
                                    foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                                    {
                                        repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale) + " Generated From " + localResults);
                                        //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        AddColumnNameAttributes(component, componentRepositoryID);
                    }
                }
                else
                {
                    // Add generic column attribute information here.
                    AddColumnNameAttributes(component, componentRepositoryID);
                }
                EnumerateComponentConnections(package, component, objectType, dataFlowComponentType, tableOrViewName, tableOrViewSource, queryDefinition, componentRepositoryID);
            }
        }

        private string FormatColumnDescription(string lName, Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType lType, int lLength, int lPrecision, int lScale)
        {
            string lTemp = "Column [" + lName + "] of Type [" + lType.ToString() + "]";
            switch (lType)
            {
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_BYREF_DECIMAL:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_BYREF_NUMERIC:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DECIMAL:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_NUMERIC:
                    lTemp += " Precision " + lPrecision.ToString() + " Scale " + lScale.ToString();
                    break;
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_BYTES:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_IMAGE:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_NTEXT:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_STR:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_TEXT:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR:
                    lTemp += " Length " + lLength;
                    break;
                default:
                    break;
            }
            return lTemp;
        }

        private void AddColumnNameAttributes(IDTSComponentMetaData component, int componentRepositoryID)
        {
            foreach (IDTSInput localInput in component.InputCollection)
            {
                if (localInput.IsAttached)
                {
                    foreach (IDTSInputColumn localIColumn in localInput.InputColumnCollection)
                    {
                        repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale));
                        //repository.AddObject(localIColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                    }
                }
            }
            foreach (IDTSOutput localOutput in component.OutputCollection)
            {
                if (localOutput.IsAttached)
                {
                    foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                    {
                        repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale));
                        //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                    }
                }
            }
        }


        private void AddTableMappings(DTSPipelineComponentType dataFlowComponentType, bool tableOrViewSource, int componentRepositoryID, int tableRepositoryID)
        {
            if (tableRepositoryID != -1)
            {
                // add the mapping between the source table and the source component
                if ((dataFlowComponentType == DTSPipelineComponentType.SourceAdapter)
                    || ((dataFlowComponentType == DTSPipelineComponentType.Transform) && (tableOrViewSource = true)))
                {
                    repository.AddMapping(tableRepositoryID, componentRepositoryID);
                }
                else if ((dataFlowComponentType == DTSPipelineComponentType.DestinationAdapter)
                    || ((dataFlowComponentType == DTSPipelineComponentType.Transform) && (tableOrViewSource == false)))
                {
                    // add the mapping from destination transform to destination table
                    repository.AddMapping(componentRepositoryID, tableRepositoryID);
                }
            }
        }


        private void ParseTSqlStatement(string statement, int connectionID, DTSPipelineComponentType dataFlowComponentType, bool tableOrViewSource, int componentRepositoryID)
        {
            SqlStatement toBeParsed = new SqlStatement();
            int tableID, procID, funcID;
            //toBeParsed.sqlString = statement;
            toBeParsed.quotedIdentifiers = true;
            if (toBeParsed.ParseString(statement))
            {
                foreach (string tableName in toBeParsed.getTableNames(true))
                {
                    if (threePartNames)
                    {
                        String dbName = repository.RetrieveDatabaseNameFromConnectionID(connectionID);
                        tableID = repository.GetTable(connectionID, String.Format("[{0}].{1}", dbName, tableName));
                    }
                    else
                        tableID = repository.GetTable(connectionID, tableName);
                    AddTableMappings(dataFlowComponentType, tableOrViewSource, componentRepositoryID, tableID);
                }
                foreach (string procedureName in toBeParsed.getProcedureNames(true))
                {
                    if (threePartNames)
                    {
                        String dbName = repository.RetrieveDatabaseNameFromConnectionID(connectionID);
                        procID = repository.GetProcedure(connectionID, String.Format("[{0}].{1}", dbName, procedureName));
                    }
                    else
                        procID = repository.GetProcedure(connectionID, procedureName);
                    AddTableMappings(dataFlowComponentType, tableOrViewSource, componentRepositoryID, procID);
                }
                foreach (string funcName in toBeParsed.getFunctionNames(true))
                {
                    if (threePartNames)
                    {
                        String dbName = repository.RetrieveDatabaseNameFromConnectionID(connectionID);
                        funcID = repository.GetFunction(connectionID, String.Format("[{0}].{1}", dbName, funcName));
                    }
                    else
                        funcID = repository.GetFunction(connectionID, funcName);
                    AddTableMappings(dataFlowComponentType, tableOrViewSource, componentRepositoryID, funcID);
                }
            }
            else
            {
                string errorMessage = "The following messages where generated whilst parsing the sql statement\r\n" + statement + "\r\n";
                foreach (string error in toBeParsed.parseErrors)
                {
                    errorMessage += error + "\r\n";
                }
                Console.WriteLine(errorMessage);
            }
        }

        private void EnumerateComponentConnections(Package package, IDTSComponentMetaData component, string objectType,
            DTSPipelineComponentType dataFlowComponentType, string tableOrViewName, bool tableOrViewSource, string queryDefinition, int componentRepositoryID)
        {
            foreach (IDTSRuntimeConnection runtimeConnection in component.RuntimeConnectionCollection)
            {
                // todo: what happens if this connection isn't available anymore
                if (package.Connections.Contains(runtimeConnection.ConnectionManagerID))
                {
                    ConnectionManager connectionManager = package.Connections[runtimeConnection.ConnectionManagerID];

                    string connectionManagerType = connectionManager.CreationName;
                    int connectionID = repository.GetConnection(connectionManager.ConnectionString);

                    string serverName = null;
                    if (connectionID == -1)
                    {
                        connectionID = CreateConnection(connectionManager, out serverName);
                    }

                    if (!string.IsNullOrEmpty(queryDefinition))
                    {
                        try
                        {
                            ParseTSqlStatement(queryDefinition, connectionID, dataFlowComponentType, tableOrViewSource, componentRepositoryID);
                        }
                        catch (System.Exception err)
                        {
                            Console.WriteLine("The exception \r\n{0}\r\nwas raised against query \r\n{1}\r\nPlease report to http://sqlmetadata.codeplex.com/WorkItem/List.aspx\r\n", err.Message, queryDefinition);
                        }

                        /*
                                                var statements = new List<IStatement> { };
                                                try
                                                {
                                                    statements = ParserFactory.Execute(queryDefinition);
                                                }
                                                catch (System.Exception err)
                                                {
                                                    Console.WriteLine("The exception \r\n{0}\r\nwas raised against query \r\n{1}\r\nPlease report to http://sqlmetadata.codeplex.com/WorkItem/List.aspx\r\n", err.Message, queryDefinition);
                                                }
                                                foreach (IStatement statement in statements)
                                                {
                                                    if (statement is Laan.Sql.Parser.Entities.SelectStatement)
                                                    {
                                                        ParseLaanSqlStatement(statement, connectionID, dataFlowComponentType, tableOrViewSource, componentRepositoryID, String.Empty);
                                                    }
                                                    if (statement is Laan.Sql.Parser.Entities.WithStatement)
                                                    {
                                                        ParseLaanSqlStatement((statement as Laan.Sql.Parser.Entities.WithStatement).CTERetrieve, connectionID, dataFlowComponentType, tableOrViewSource, componentRepositoryID, (statement as Laan.Sql.Parser.Entities.WithStatement).CTETable.Value);
                                                        ParseLaanSqlStatement((statement as Laan.Sql.Parser.Entities.WithStatement).CTESelect, connectionID, dataFlowComponentType, tableOrViewSource, componentRepositoryID, (statement as Laan.Sql.Parser.Entities.WithStatement).CTETable.Value);
                                                    }
                                                }
                         */
                    }
                    else
                    {
                        int tableRepositoryID = -1;
                        if (connectionManagerType == "ADO" || connectionManagerType == "OLEDB" ||
                            connectionManagerType == "ADO.NET" || connectionManagerType == "ODBC")
                        {
                            if (!string.IsNullOrEmpty(tableOrViewName))
                            {
                                // add the table to the repository for each distinct connection
                                if (threePartNames)
                                {
                                    String dbName = repository.RetrieveDatabaseNameFromConnectionID(connectionID);
                                    tableRepositoryID = repository.GetTable(connectionID, String.Format("[{0}].{1}", dbName, tableOrViewName));
                                }
                                else
                                    tableRepositoryID = repository.GetTable(connectionID, tableOrViewName);
                            }
                        }
                        else if (connectionManagerType == "FILE" || connectionManagerType == "FLATFILE" ||
                            connectionManagerType == "EXCEL")
                        {
                            // add the table to the repository for each distinct connection
                            tableRepositoryID = repository.GetFile(connectionManager.ConnectionString, "localhost");
                        }
                        AddTableMappings(dataFlowComponentType, tableOrViewSource, componentRepositoryID, tableRepositoryID);
                    }
                }
            }


            // special case some components that don't use connection managers
            if (objectType == ClassIDs.XmlSource)
            {
                string xmlFile = GetStringComponentPropertyValue(component, "XMLData");
                int fileID = repository.GetFile(xmlFile, "localhost");
                repository.AddMapping(fileID, componentRepositoryID);

                xmlFile = GetStringComponentPropertyValue(component, "XMLSchemaDefinition");
                fileID = repository.GetFile(xmlFile, "localhost");
                repository.AddMapping(fileID, componentRepositoryID);
            }
            else if (objectType == ClassIDs.RawFileSource)
            {
                string rawFile = GetStringComponentPropertyValue(component, "FileName");
                if (rawFile == null)
                {
                    rawFile = GetStringComponentPropertyValue(component, "FileNameVariable");
                }
                int fileID = repository.GetFile(rawFile, "localhost");
                repository.AddMapping(fileID, componentRepositoryID);
            }
            else if (objectType == ClassIDs.RawFileDest)
            {
                string rawFile = GetStringComponentPropertyValue(component, "FileName");
                if (rawFile == null)
                {
                    rawFile = GetStringComponentPropertyValue(component, "FileNameVariable");
                }
                int fileID = repository.GetFile(rawFile, "localhost");
                repository.AddMapping(componentRepositoryID, fileID);
            }
            // add all the Columns as Attributes (just for fun?)
            // AddColumnNameAttributes(component, componentRepositoryID);
        }

        private int CreateConnection(ConnectionManager connectionManager, out string serverName)
        {
            // todo: is the root object of the connection the root repository object ID?
            string connectionType = connectionManager.CreationName;
            if (connectionType.IndexOf(':') > 0)
                connectionType = connectionType.Substring(0, connectionType.IndexOf(':'));

            int connectionID = AddConnectionObject(connectionManager.Name, connectionManager.Description, connectionType, repository.RootRepositoryObjectID);

            // add the attributes of the connection as well
            repository.AddAttribute(connectionID, Repository.Attributes.ConnectionString, connectionManager.ConnectionString);

            // get the server name/initial catalog, etc. 
            string initialCatalog;

            GetConnectionAttributes(connectionManager, out serverName, out initialCatalog);

            if (string.IsNullOrEmpty(serverName) == false)
            {
                repository.AddAttribute(connectionID, Repository.Attributes.ConnectionServer, serverName);
            }

            if (string.IsNullOrEmpty(initialCatalog) == false)
            {
                repository.AddAttribute(connectionID, Repository.Attributes.ConnectionDatabase, initialCatalog);
            }
            return connectionID;
        }

        private void GetConnectionAttributes(ConnectionManager connectionManager, out string serverName, out string initialCatalog)
        {
            serverName = null;
            initialCatalog = null;

            try
            {
                if ((connectionManager.CreationName == "OLEDB") || (connectionManager.CreationName == "EXCEL"))
                {
                    connectionStringBuilder.Clear();

                    connectionStringBuilder.ConnectionString = connectionManager.ConnectionString;

                    serverName = connectionStringBuilder.DataSource;

                    object outValue;
                    connectionStringBuilder.TryGetValue("Initial Catalog", out outValue);

                    if (outValue != null)
                    {
                        initialCatalog = outValue.ToString();
                    }
                }
                else
                {
                    if (connectionManager.CreationName == "FLATFILE")
                    {
                        serverName = connectionManager.ConnectionString;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(string.Format("Error occurred: Could not completely parse connection string for '{0}': {1}", connectionManager.Name, ex.Message));
            }
        }

        /// <summary>
        ///  enumerate all mappings that exist between a group of source and target components and write them out to the repository.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="dataFlowRepositoryObjectID"></param>
        /// <param name="componentIDToSourceRepositoryObjectMap"></param>
        /// <param name="componentIDToTargetRepositoryObjectMap"></param>
        private void EnumerateMappings(IDTSPipeline pipeline, int dataFlowRepositoryObjectID,
            Dictionary<int, int> componentIDToSourceRepositoryObjectMap,
            Dictionary<int, int> componentIDToRepositoryObjectMap)
        {
            Debug.Assert(dataFlowRepositoryObjectID > 0);
            Debug.Assert(componentIDToSourceRepositoryObjectMap.Count > 0);
            Debug.Assert(componentIDToRepositoryObjectMap.Count > 0);

            Dictionary<int, bool> outputsAlreadyInvestigated = new Dictionary<int, bool>(); // key: outputID

            foreach (int sourceComponentID in componentIDToSourceRepositoryObjectMap.Keys)
            {
                EnumerateMappingForSource(pipeline, dataFlowRepositoryObjectID, sourceComponentID,
                    componentIDToRepositoryObjectMap, outputsAlreadyInvestigated);
            }
        }

        /// <summary>
        /// Enumerate all mappings that exist between a source and a list of target components and write them out to the repository. 
        /// If there are multiple mappings between a source and target only one will be written out.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="dataFlowRepositoryObjectID"></param>
        /// <param name="sourceComponentID"></param>
        /// <param name="componentIDToTargetRepositoryObjectMap"></param>
        private void EnumerateMappingForSource(IDTSPipeline pipeline, int dataFlowRepositoryObjectID,
            int sourceComponentID, Dictionary<int, int> componentIDToRepositoryObjectMap,
            Dictionary<int, bool> outputsAlreadyInvestigated)
        {
            // get the component
            IDTSComponentMetaData componentMetadata = pipeline.ComponentMetaDataCollection.GetObjectByID(sourceComponentID);
            Debug.Assert(componentMetadata != null);

            Queue<IDTSOutput> outputsToInvestigate = new Queue<IDTSOutput>();

            // go through each output of the source.
            foreach (IDTSOutput output in componentMetadata.OutputCollection)
            {
                outputsToInvestigate.Enqueue(output);
            }

            while (outputsToInvestigate.Count > 0)
            {
                IDTSOutput output = outputsToInvestigate.Dequeue();

                int outputID = output.ID;

                if (outputsAlreadyInvestigated.ContainsKey(outputID) == false)
                {
                    outputsAlreadyInvestigated.Add(outputID, true);

                    // find out whethere the next component has one of the source IDs
                    IDTSComponentMetaData connectedComponent = GetConnectedComponentID(pipeline, output);
                    if (connectedComponent != null)
                    {
                        int connectedComponentID = connectedComponent.ID;

                        // if the target component doesn't exist in the repository already, add it now
                        int targetObjectRepositoryID = -1;
                        if (componentIDToRepositoryObjectMap.ContainsKey(connectedComponentID))
                        {
                            targetObjectRepositoryID = componentIDToRepositoryObjectMap[connectedComponentID];
                        }
                        else
                        {
                            targetObjectRepositoryID = AddObject(connectedComponent.Name, connectedComponent.Description, connectedComponent.ComponentClassID, dataFlowRepositoryObjectID);
                            componentIDToRepositoryObjectMap.Add(connectedComponentID, targetObjectRepositoryID);
                        }

                        Debug.Assert(componentIDToRepositoryObjectMap.ContainsKey(output.Component.ID));
                        int sourceObjectRepositoryID = componentIDToRepositoryObjectMap[output.Component.ID];

                        // create a mapping if it doesn't exist already
                        if (repository.DoesMappingExist(sourceObjectRepositoryID, targetObjectRepositoryID) == false)
                        {
                            repository.AddMapping(sourceObjectRepositoryID, targetObjectRepositoryID);
                        }

                        // otherwise add all outputs from the connected component as well
                        foreach (IDTSOutput outputToTraverse in connectedComponent.OutputCollection)
                        {
                            outputsToInvestigate.Enqueue(outputToTraverse);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Traverses the output->input path (if there is one) and reports the component of the input.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        private IDTSComponentMetaData GetConnectedComponentID(IDTSPipeline pipeline, IDTSOutput output)
        {
            int outputID = output.ID;

            foreach (IDTSPath path in pipeline.PathCollection)
            {
                if (path.StartPoint.ID == outputID)
                {
                    // got it
                    return path.EndPoint.Component;
                }
            }

            // this output is dangling, not connected to anything else
            return null;
        }

        /// <summary>
        /// if the component is interesting, return that fact and the object type
        /// </summary>
        /// <param name="component">component to inspect</param>
        /// <param name="objectTypeName">object type name</param>
        /// <param name="componentType">Source/Destination/Transform</param>
        /// <param name="domain">Repository.Domains.Relational or Repository.Domains.File</param>
        /// <param name="tableOrViewName">table or view for relational domain</param>
        /// <param name="tableOrViewSource">Whether the table/view (if specified) is a source or a destination</param>
        /// <returns></returns>
        private bool IsComponentInteresting(Package package, IDTSComponentMetaData component,
            out string objectTypeName,
            out DTSPipelineComponentType componentType,
            out string domain,
            out string tableOrViewName,
            out bool tableOrViewSource,
            out string queryDefinition)
        {
            objectTypeName = component.ComponentClassID;
            domain = null;
            tableOrViewName = null;
            queryDefinition = null;
            tableOrViewSource = false;

            // for managed components, the type name is stored in a custom property
            if (string.Compare(objectTypeName, ClassIDs.ManagedComponentWrapper, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                objectTypeName = component.CustomPropertyCollection["UserComponentTypeName"].Value.ToString();
            }

            if (pipelineComponentInfos.Contains(objectTypeName) == false)
            {
                throw new Exception(string.Format("Unknown component type encountered: {0}, {1}", objectTypeName, component.Name));
            }

            PipelineComponentInfo pipelineComponentInfo = pipelineComponentInfos[objectTypeName];

            Debug.Assert(pipelineComponentInfo != null);

            componentType = pipelineComponentInfo.ComponentType;

            string componentClassID = component.ComponentClassID;

            if ((string.Equals(componentClassID, ClassIDs.OleDbSource))
                 || (string.Equals(componentClassID, ClassIDs.ExcelSource))
                 || (string.Equals(componentClassID, ClassIDs.OleDbDestination)))
            {
                domain = Repository.Domains.Relational;
                GetOleDbComponentsInfo(package, component, ref tableOrViewName, ref queryDefinition);
                tableOrViewSource = !(string.Equals(componentClassID, ClassIDs.OleDbDestination));
            }
            else if (string.Equals(componentClassID, ClassIDs.SqlDestination))
            {
                domain = Repository.Domains.Relational;
                tableOrViewName = GetStringComponentPropertyValue(component, "BulkInsertTableName");
                tableOrViewSource = false;
            }
            else if ((string.Equals(componentClassID, ClassIDs.FlatFileSource))
                     || (string.Equals(componentClassID, ClassIDs.FlatFileDest)))
            {
                domain = Repository.Domains.File;

                // flat file connection is always 'interesting', but does not use table name, which was nulled out earlier anyways
            }
            else if (string.Equals(componentClassID, ClassIDs.Lookup))
            {
                domain = Repository.Domains.Relational;
                queryDefinition = GetLookupInfo(component);
            }
            else if (string.Equals(componentClassID, ClassIDs.FuzzyLookup))
            {
                domain = Repository.Domains.Relational;
                tableOrViewName = GetFuzzyLookupInfo(component);
                tableOrViewSource = true;
            }
            else if (string.Equals(componentClassID, ClassIDs.OLEDBCommand))
            {
                queryDefinition = GetLookupInfo(component);
            }

            return (objectTypeName != null);
        }

        private static string GetFuzzyLookupInfo(IDTSComponentMetaData component)
        {
            return GetStringComponentPropertyValue(component, "ReferenceTableName");
        }

        /// <summary>
        ///  returns information about the query used by the lookup/fuzzy lookup components
        /// </summary>
        /// <param name="component"></param>
        /// <param name="queryDefinition"></param>
        private static string GetLookupInfo(IDTSComponentMetaData component)
        {
            return GetStringComponentPropertyValue(component, "SqlCommand");
        }

        private static string GetStringComponentPropertyValue(IDTSComponentMetaData component, string propertyName)
        {
            IDTSCustomProperty prop = null;
            try
            {
                prop = component.CustomPropertyCollection[propertyName];
            }
            catch
            {
                // No Action here.
            }
            string value = null;

            if (prop != null && prop.Value is string)
            {
                value = prop.Value.ToString();
            }

            return value;
        }

        /// <summary>
        ///  returns information about OleDb source, destination and Excel source.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="tableOrViewName"></param>
        /// <param name="queryDefinition"></param>
        private static void GetOleDbComponentsInfo(Package package, IDTSComponentMetaData component, ref string tableOrViewName, ref string queryDefinition)
        {
            IDTSCustomProperty prop = component.CustomPropertyCollection["AccessMode"];
            string strVariableName;
            tableOrViewName = "";
            queryDefinition = "";

            if (prop != null && prop.Value is int)
            {
                int accessMode = (int)prop.Value;

                switch (accessMode)
                {
                    case (int)AccessMode.AM_OPENROWSET:
                        tableOrViewName = GetStringComponentPropertyValue(component, "OpenRowset");
                        break;

                    case (int)AccessMode.AM_OPENROWSET_VARIABLE:
                        strVariableName = GetStringComponentPropertyValue(component, "OpenRowsetVariable");
                        tableOrViewName = GetVariable(package, strVariableName);
                        break;

                    case (int)AccessMode.AM_SQLCOMMAND:
                        queryDefinition = GetStringComponentPropertyValue(component, "SqlCommand");
                        break;

                    case (int)AccessMode.AM_SQLCOMMAND_VARIABLE:
                        strVariableName = GetStringComponentPropertyValue(component, "SqlCommandVariable");
                        if (strVariableName == null)
                        {
                            tableOrViewName = GetStringComponentPropertyValue(component, "OpenRowset");
                        }
                        else
                        {
                            queryDefinition = GetVariable(package, strVariableName);
                        }
                        break;

                    case (int)AccessMode.AM_OPENROWSET_FASTLOAD_VARIABLE:
                        strVariableName = GetStringComponentPropertyValue(component, "OpenRowsetVariable");
                        if (strVariableName == null)
                        {
                            tableOrViewName = string.Empty;
                            queryDefinition = string.Empty;
                            Console.WriteLine("Unexpected setup for OLEDB Fast Load from Variable.  Table details not collected.\r\nPlease report to http://sqlmetadata.codeplex.com/WorkItem/List.aspx\r\nWith the SSIS Package if possible.");
                        }
                        else
                        {
                            tableOrViewName = GetVariable(package, strVariableName);
                        }
                        break;

                    default:
                        throw new Exception(string.Format("Invalid access mode {0}.", accessMode));
                }
            }
        }

        private static string GetVariable(Package package, string strVariableName)
        {
            if (strVariableName != null)
                return package.Variables[strVariableName].Value.ToString();
            else
                return null;
        }

        private int AddConnectionObject(string name, string description, string type, int parentObjectID)
        {
            string id = connectionTypeToIDMap[type];
            return repository.AddObject(name, description, id, parentObjectID);
        }

        /// <summary>
        /// Add a new object to the repository
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="objectType"></param>
        /// <param name="parentObjectID"></param>
        /// <returns></returns>
        private int AddObject(string name, string description, string objectType, int parentObjectID)
        {
            if (!repository.IsTypeDefined(objectType))
            {
                // object type might be a progid instead of a clsid. We need to use clsids if one exists
                bool isGuid = true;
                Guid guid;
                isGuid = Guid.TryParse(objectType, out guid);

                if (isGuid == false)
                {
                    try
                    {
                        uint result = NativeMethods.CLSIDFromProgID(objectType, out guid);
                        if (result == 0)
                        {
                            objectType = guid.ToString("B").ToUpper();
                        }
                    }
                    catch (System.Exception)
                    {
                        // ignore, use the original objectType
                    }
                }
            }

            return repository.AddObject(name, description, objectType, parentObjectID);
        }
    }
}
