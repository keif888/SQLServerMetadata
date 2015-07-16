///
/// Microsoft SQL Server 2008 Business Intelligence Metadata Reporting Samples
/// Dependency Analyzer Sample
/// 
/// Copyright (c) Microsoft Corporation.  All rights reserved.
///
using System;
using System.Collections.Generic;
using System.Text;
using CommandLine; 

namespace Microsoft.Samples.DependencyAnalyzer
{
    class DependencyArguments
    {
        [Argument(ArgumentType.AtMostOnce
            , HelpText = "ADO.Net SqlConnection compatible connection string to dependency database location."
            , DefaultValue="Server=localhost;database=SSIS_Meta;Integrated Security=SSPI;"
            , LongName="depDb"
            , ShortName="d")]
        public string depDb = "Server=localhost;database=SSIS_Meta;Integrated Security=SSPI;";
        
        [Argument(ArgumentType.MultipleUnique
            , HelpText = "Root folders of file system packages."
            , LongName="folders"
            , ShortName = "f")]
        public string[] folders = null;

        [Argument(ArgumentType.MultipleUnique
            , HelpText = "Root folders of SSIS datbase system packages."
            , LongName="ssisFolders"
            , ShortName = "sf")]
        public string[] ssisFolders = null;

        [Argument(ArgumentType.MultipleUnique
            , HelpText = "SQL Servers where SSIS packages are stored.  If you need to use different passwords per server, use \"Server=servername;User=username;Password=pwd\""
            , DefaultValue = new string[] { "localhost" }
            , LongName = "isDbServer"
            , ShortName = "i")]
        public string[] isDbServer = null;
        
        [Argument(ArgumentType.AtMostOnce
            , HelpText = "SQL Server user that has access to stored SSIS packages"
            , DefaultValue = null
            , LongName = "isDbUser"
            , ShortName = "iu")]
        public string isDbUser = null;

        [Argument(ArgumentType.AtMostOnce
            , HelpText = "SQL Server password for getting access to stored SSIS packages"
            , DefaultValue = null
            , LongName = "isDbPwd"
            , ShortName = "ip")]
        public string isDbPwd = null;

        [Argument(ArgumentType.MultipleUnique
            , HelpText = "Passwords to access SSIS Packages."
            , DefaultValue = null
            , LongName = "isPkgPwd"
            , ShortName = "pp")]
        public string[] isPkgPwd = null;

        [Argument(ArgumentType.MultipleUnique
            , HelpText = "AMO compatible connection string to Analysis Services."
            , DefaultValue = new string[] { "Provider=msolap;Data Source=localhost;" }
            , LongName = "asCon"
            , ShortName = "a")]
        public string[] asCon = null;

        [Argument(ArgumentType.AtMostOnce
            , HelpText = "Whether to recurse file system sub folders when enumerating objects."
            , DefaultValue = true
            , LongName = "recurse"
            , ShortName = "r")]
        public bool recurse = true;

        [Argument(ArgumentType.AtMostOnce
            , HelpText= "Whether to start execution without asking the user to continue."
            , DefaultValue=false
            , LongName = "batchMode"
            , ShortName = "b")]
        public bool batchMode = false;

        [Argument(ArgumentType.AtMostOnce
            , HelpText = "Whether to skip enumerating packages in SQL Server."
            , DefaultValue = false
            , LongName = "skipSQL"
            , ShortName = "s")]
        public bool skipSQL = false;

        [Argument(ArgumentType.AtMostOnce
            , HelpText = "Whether to skip enumerating packages completely."
            , DefaultValue = true
            , LongName = "skipSSIS"
            , ShortName = "ss")]
        public bool skipSSIS = true;

        [Argument(ArgumentType.AtMostOnce
            , HelpText = "Whether to skip enumerating Analysis Services objects."
            , DefaultValue = true
            , LongName = "skipAS"
            , ShortName = "sa")]
        public bool skipAS = true;

        [Argument(ArgumentType.AtMostOnce
            , HelpText = "Whether to match on database name only for connection strings."
            , DefaultValue = false
            , LongName = "matchDBOnly"
            , ShortName = "m")]
        public bool matchDBOnly = false;

        [Argument(ArgumentType.MultipleUnique
            , HelpText = "Database Prefixes to exclude when matching on database names."
            , LongName = "dbPrefix"
            , ShortName = "dp")]
        public string[] dbPrefix = null;

        [Argument(ArgumentType.AtMostOnce
            , HelpText = "Remove all records from the database on execution."
            , DefaultValue = false
            , LongName = "clearDatabase"
            , ShortName = "clearDB")]
        public bool clearDatabase = false;

        [Argument(ArgumentType.MultipleUnique
            , HelpText = "SqlClient SqlConnection compatible connection string to SQL Server databases to analyse.\r\nServer=(local);Database=\"AdventureWorks\";Integrated Security=SSPI"
            , LongName = "dbToScan"
            , ShortName = "d2s")]
        public string[] databasesToScan = null;

        [Argument(ArgumentType.AtMostOnce
            , HelpText = "Whether to skip enumerating Reporting Services."
            , DefaultValue = true
            , LongName = "skipRS"
            , ShortName = "sr")]
        public bool skipReportingServices = true;

        [Argument(ArgumentType.MultipleUnique
            , HelpText = "Reporting Services URL with ? and Path to Enumerate\r\neg. http://localhost/reportserver?/"
            , LongName = "ReportUrl"
            , ShortName = "rpt")]
        public string[] reportURLs = null;

        [Argument(ArgumentType.AtMostOnce
            , HelpText = "Whether to store object names as three part names [dbname].[schema].[object]"
            , LongName = "threePartNames"
            , ShortName = "tpn"
            , DefaultValue = false)]
        public bool storeThreePartNames = false;
    }
    
    class Program
    {

        static void Main(string[] args)
        {
            string commandLineArguments  = "";
            foreach (string tmpString in args)
            {
                commandLineArguments += tmpString + " ";
            }
            // parse the command line
            DependencyArguments dependencyArguments = new DependencyArguments();
            if (CommandLine.Parser.ParseArgumentsWithUsage(args, dependencyArguments))
            {
                try
                {
                    if (dependencyArguments.batchMode == false && dependencyArguments.clearDatabase == true)
                    {
                        Console.WriteLine("This tool will delete existing information in the dependency database!");
                        Console.Write("Press any key to begin...");
                        Console.ReadKey(true);
                        Console.WriteLine("started");
                    }

                    // open a connection to the repository
                    Repository repository = InitializeRepository(dependencyArguments, commandLineArguments);

                    if (repository == null)
                    {
                        Console.WriteLine(String.Format("Unable to open database on connection string '{0}'.  Exiting...", dependencyArguments.depDb));
                        return;
                    }

                    EnumerateRelational(dependencyArguments, repository);
                    EnumerateFile(dependencyArguments, repository);
                    EnumerateColumn(dependencyArguments, repository);
                    EnumerateReport(dependencyArguments, repository);

                    if (dependencyArguments.databasesToScan != null)
                    {
                        EnumerateDatabase(dependencyArguments, repository);
                    }

                    if (dependencyArguments.skipSSIS == false)
                    {
                        EnumerateSSIS(dependencyArguments, repository);
                    }

                    if (dependencyArguments.skipAS == false)
                    {
                        EnumerateSSAS(dependencyArguments, repository);
                    }

                    if (dependencyArguments.skipReportingServices == false)
                    {
                        EnumerateSSRS(dependencyArguments, repository);
                    }

                    Commit(repository);
                }
                catch (System.Exception ex)
                {
                    Console.Write(string.Format("Unexpected error occurred: {0}\r\nStack Trace:\r\n{1}", ex.Message, ex.StackTrace));
                }

                if (dependencyArguments.batchMode == false)
                {
                    Console.Write("Press any key to continue...");
                    Console.ReadKey(true);
                    Console.WriteLine("done");
                }
            }
        }

        private static void EnumerateReport(DependencyArguments dependencyArguments, Repository repository)
        {
            ReportEnumerator enumerator = new ReportEnumerator();
            if (enumerator.Initialize(repository))
            {
                // nothing to enumerate right now, initialize call should have created the relational types
                // in the repository
            }
        }
      
        public static void Commit(Repository repository)
        {
            Console.Write("Committing analysis information to database...");
            repository.Commit();
            Console.WriteLine("Completed.");
        }

        private static void EnumerateRelational(DependencyArguments dependencyArguments, Repository repository)
        {
            RelationalEnumerator enumerator = new RelationalEnumerator();
            if (enumerator.Initialize(repository))
            {
                // nothing to enumerate right now, initialize call should have created the relational types
                // in the repository
            }
        }

        private static void EnumerateFile(DependencyArguments dependencyArguments, Repository repository)
        {
            FileEnumerator enumerator = new FileEnumerator();
            if (enumerator.Initialize(repository))
            {
                // nothing to enumerate right now, initialize call should have created the relational types
                // in the repository
            }
        }

        private static void EnumerateColumn(DependencyArguments dependencyArguments, Repository repository)
        {
            ColumnEnumerator enumerator = new ColumnEnumerator();
            if (enumerator.Initialize(repository))
            {
                // nothing to enumerate right now, initialize call should have created the relational types
                // in the repository
            }
        }

        private static void EnumerateDatabase(DependencyArguments dependencyArguments, Repository repository)
        {
            // ToDo: Code the call to DatabaseEnumerator()...
            SQLDBEnumerator enumerator = new SQLDBEnumerator();
            if (enumerator.Initialize(repository))
            {
                foreach (string dbConnection in dependencyArguments.databasesToScan)
                {
                    Console.WriteLine("Enumerating Database metadata for {0}.", dbConnection);
                    enumerator.EnumerateDatabase(dbConnection, dependencyArguments.storeThreePartNames);
                }
            }
        }


        private static void EnumerateSSIS(DependencyArguments dependencyArguments, Repository repository)
        {
            SSISEnumerator enumerator = new SSISEnumerator();
            if (enumerator.Initialize(repository))
            {
                Console.WriteLine("Enumerating File System Integration Services metadata.");
                enumerator.EnumerateFileSystemPackages(dependencyArguments.folders, dependencyArguments.recurse, dependencyArguments.storeThreePartNames, dependencyArguments.isPkgPwd);

                if (dependencyArguments.skipSQL == false)
                {
                    Console.WriteLine("Enumerating Integration Services metadata.");
                    foreach (string dbServer in dependencyArguments.isDbServer)
                    {
                        if (dbServer.Contains(";"))
                        {
                            String dbUser = null;
                            String dbPass = null;
                            String ssisServer = string.Empty;
                            int posServer = dbServer.IndexOf("server=", 0, StringComparison.InvariantCultureIgnoreCase) + "server=".Length;
                            int posUser = dbServer.IndexOf("user=", 0, StringComparison.InvariantCultureIgnoreCase);
                            int posPass = dbServer.IndexOf("password=", 0, StringComparison.InvariantCultureIgnoreCase);

                            ssisServer = dbServer.Substring(posServer, posUser - posServer - 1);
                            posUser += "user=".Length;
                            dbUser = dbServer.Substring(posUser, posPass - posUser - 1);
                            posPass += "password=".Length;
                            dbPass = dbServer.Substring(posPass, dbServer.Length - posPass);
                            if (String.IsNullOrEmpty(dbUser))
                            {
                                dbUser = null;
                                dbPass = null;
                            }
                            enumerator.EnumerateSqlPackages(ssisServer, dbUser, dbPass, dependencyArguments.ssisFolders, dependencyArguments.storeThreePartNames, dependencyArguments.isPkgPwd);
                        }
                        else
                            enumerator.EnumerateSqlPackages(dbServer, dependencyArguments.isDbUser, dependencyArguments.isDbPwd, dependencyArguments.ssisFolders, dependencyArguments.storeThreePartNames, dependencyArguments.isPkgPwd);
                    }
                }
            }
        }

        private static void EnumerateSSAS(DependencyArguments dependencyArguments, Repository repository)
        {
            Console.WriteLine("Enumerating Analysis Services metadata.");
            try
            {
                SSASEnumerator enumerator = new SSASEnumerator();

                if (enumerator.Initialize(repository))
                {
                    foreach(string connStr in dependencyArguments.asCon)
                        enumerator.EnumerateServer(connStr, dependencyArguments.storeThreePartNames);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(string.Format("Error occurred: {0}", ex.Message));
            }
        }

        private static void EnumerateSSRS(DependencyArguments dependencyArguments, Repository repository)
        {
            Console.WriteLine("Enumerating Reporting Services metadata.");
            try
            {
                SSRSEnumerator enumerator = new SSRSEnumerator();

                if (enumerator.Initialize(repository))
                {
                    foreach (string connStr in dependencyArguments.reportURLs)
                        enumerator.EnumerateReportingServer(connStr, dependencyArguments.recurse, dependencyArguments.storeThreePartNames);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(string.Format("Error occurred: {0}", ex.Message));
            }
        }

        private static Repository InitializeRepository(DependencyArguments dependencyArguments, string commandLineArguments)
        {
            Repository repository = new Repository(dependencyArguments.depDb);
            try
            {
                repository.Open();

                // check if this is a valid repository database
                if (repository.IsValidRepository() == false)
                {
                    repository.CreateRepository();
                }

                if (dependencyArguments.clearDatabase)
                {
                    repository.DeleteExistingRepository();
                }
                else
                {
                    repository.LoadExisingRepository();
                }

                if (commandLineArguments.Length > 512)
                    repository.InitialiseRepository(commandLineArguments.Substring(0, 512));
                else
                    repository.InitialiseRepository(commandLineArguments);
                repository.DatabaseNameOnlyCompare = dependencyArguments.matchDBOnly;

                foreach (string dbPrefix in dependencyArguments.dbPrefix)
                {
                    repository.DatabasePrefixExclusions.Add(dbPrefix);
                }

                return repository;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Unable to connect to Dependency Database: {0}\r\nMessage:\r\n{1}", dependencyArguments.depDb, ex.Message));
                return null;
            }
        }
    }
}
