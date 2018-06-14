using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SSISProjectBuilder
{
    /// <summary>
    /// Class to define the arguments to this program
    /// </summary>
    class BuilderArguments
    {
        [Argument(ArgumentType.Required
            , HelpText = "The fully qualified name and path to the .dtproj file to build"
            , LongName = "dtproj"
            , ShortName = "d")]
        public string dtprojFileName = string.Empty;

        [Argument(ArgumentType.Required
            , HelpText = "The fully qualified name and path to the ispac file to generate"
            , LongName = "ispac"
            , ShortName = "i")]
        public string ispacFileName = string.Empty;

        [Argument(ArgumentType.Required
            , HelpText = "The SQL Server Version to build to (SQL2012, SQL2014, SQL2016, SQL2017)"
            , LongName = "version"
            , ShortName = "v")]
        public string sqlVersion = string.Empty;
    }

    class Program
    {
        static int Main(string[] args)
        {
            int result = 1;
            DeploymentFileBuilder builder = new DeploymentFileBuilder();
            string commandLineArguments = "";
            foreach (string tmpString in args)
            {
                commandLineArguments += tmpString + " ";
            }
            // parse the command line
            BuilderArguments dependencyArguments = new BuilderArguments();
            if (CommandLine.Parser.ParseArgumentsWithUsage(args, dependencyArguments))
            {
                try
                {
                    builder.InputProject = dependencyArguments.dtprojFileName;
                    builder.OutputISPAC = dependencyArguments.ispacFileName;
                    builder.ProtectionLevel = Microsoft.SqlServer.Dts.Runtime.DTSProtectionLevel.DontSaveSensitive.ToString();
                    switch(dependencyArguments.sqlVersion)
                    {
                        case "SQL2012":
                            builder.SQLVersion = Microsoft.SqlServer.Dts.Runtime.DTSTargetServerVersion.SQLServer2012.ToString();
                            break;
                        case "SQL2014":
                            builder.SQLVersion = Microsoft.SqlServer.Dts.Runtime.DTSTargetServerVersion.SQLServer2014.ToString();
                            break;
                        case "SQL2016":
                            builder.SQLVersion = Microsoft.SqlServer.Dts.Runtime.DTSTargetServerVersion.SQLServer2016.ToString();
                            break;
                        case "SQL2017":
                            builder.SQLVersion = Microsoft.SqlServer.Dts.Runtime.DTSTargetServerVersion.SQLServer2017.ToString();
                            break;
                        default:
                            builder.SQLVersion = Microsoft.SqlServer.Dts.Runtime.DTSTargetServerVersion.Latest.ToString();
                            break;
                    }
                    if (builder.Execute())
                    {
                        result = 0;
                    }
                }
                catch (System.Exception ex)
                {
                    Console.Write(string.Format("Unexpected error occurred: {0}\r\nStack Trace:\r\n{1}", ex.Message, ex.StackTrace));
                }
            }
            return result;

        }
    }
}
