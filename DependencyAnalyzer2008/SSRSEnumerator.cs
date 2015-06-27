using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.Data.SqlClient;
using System.IO;
using TSQLParser;
using System.Data.Common;
using System.Data.OleDb;

namespace Microsoft.Samples.DependencyAnalyzer
{
    class SSRSEnumerator
    {
        private Repository repository;
        private Dictionary<String, int> datasourceInRepository;
        private int reportServerID;

        private const string oleDBTypeString = "{3BA51769-6C3C-46B2-85A1-81E58DB7DAE1}";

        public SSRSEnumerator()
        {
            
        }

        public bool Initialize(Repository repository)
        {
            bool success;

            try
            {
                this.repository = repository;
                success = true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(string.Format("Could not initialize the Reporting Services Enumerator: {0}", ex.Message));
                success = false;
            }

            return success;
        }

        public void EnumerateReportingServer(string reportingServerURL, bool recursive)
        {
            Boolean isRS2010 = true, isRS2005 = false;
            Uri reportingServerUri = new Uri(reportingServerURL);
            string decodedURL, decodedPath;
            SSRS2010.CatalogItem[] items2010 = null;
            SSRS2010.ReportingService2010 reportingServer2010 = new SSRS2010.ReportingService2010();

            if (reportingServerUri.IsDefaultPort)
            {
                decodedURL = reportingServerUri.Scheme + "://" + reportingServerUri.Host + reportingServerUri.LocalPath;
            }
            else
            {
                decodedURL = reportingServerUri.Scheme + "://" + reportingServerUri.Host + ":" + reportingServerUri.Port.ToString() + reportingServerUri.LocalPath;
            }
            decodedPath = Uri.UnescapeDataString(reportingServerUri.Query.Substring(1));

            reportingServer2010.Credentials = System.Net.CredentialCache.DefaultCredentials;
            reportingServer2010.Url = decodedURL + @"/ReportService2010.asmx";

            // Assign the Report Server ID, so that all subsequent objects can be related to this.
            reportServerID = repository.AddObject(reportingServerUri.Host, decodedURL, ReportEnumerator.ObjectTypes.ReportServer, repository.RootRepositoryObjectID);

            SSRS2005.CatalogItem[] items2005 = null;
            SSRS2005.ReportingService2005 reportingServer2005 = new SSRS2005.ReportingService2005();
            reportingServer2005.Credentials = System.Net.CredentialCache.DefaultCredentials;
            reportingServer2005.Url = decodedURL + @"/ReportService2005.asmx";

            datasourceInRepository = new Dictionary<string, int>();

            // Try to connect to the Reporting Server as SQL 2008 R2 first.
            try
            {
                items2010 = reportingServer2010.ListChildren(decodedPath, false);
            }
            catch
            {
                isRS2010 = false;
            }


            if (!isRS2010)
            {
                try
                {
                    items2005 = reportingServer2005.ListChildren(decodedPath, false);
                    isRS2005 = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Unable to connect to either SQL2008R2 or SQL2005 services\r\nMessage {0}\r\nStack Trace {1}", ex.Message, ex.StackTrace));
                    return;
                }
            }
            else
            {
                Enumerate2010Server(reportingServer2010, decodedPath, recursive);
            }
            if (isRS2005)
            {
                Enumerate2005Server(reportingServer2005, decodedPath, recursive);
            }
        }

        #region HandleDataSource
        /// <summary>
        /// Gets an ID from the Repository for this Data Source.
        /// </summary>
        /// <param name="reportingServer2010">The SQL 2008 R2 reporting server connection</param>
        /// <param name="path">Where the Datasource is in the Reporting Server</param>
        /// <returns>The Repository ID.</returns>
        private int HandleDataSource(SSRS2010.ReportingService2010 reportingServer2010, string path)
        {
            int idValue;
            SSRS2010.DataSourceDefinition dsDef = reportingServer2010.GetDataSourceContents(path);
            if (!datasourceInRepository.TryGetValue(path, out idValue))
            {
                idValue = repository.GetConnection(dsDef.ConnectString);
                if (idValue == -1)
                {
                    idValue = repository.AddObject(path, dsDef.ConnectString, oleDBTypeString, repository.RootRepositoryObjectID);
                    repository.AddAttribute(idValue, Repository.Attributes.ConnectionString, dsDef.ConnectString);
                    DbConnectionStringBuilder connectionStringBuilder = repository.GetConnectionStringBuilder(dsDef.ConnectString);
                    string Server = repository.GetServer(connectionStringBuilder);
                    string Database = repository.GetDatabase(connectionStringBuilder);

                    if (string.IsNullOrEmpty(Server))
                        repository.AddAttribute(idValue, Repository.Attributes.ConnectionServer, "(NULL)");
                    else
                        repository.AddAttribute(idValue, Repository.Attributes.ConnectionServer, Server);

                    if (string.IsNullOrEmpty(Database))
                        repository.AddAttribute(idValue, Repository.Attributes.ConnectionDatabase, "(NULL)");
                    else
                        repository.AddAttribute(idValue, Repository.Attributes.ConnectionDatabase, Database);
                }
                datasourceInRepository.Add(path, idValue);
            }
            return idValue;
        }

        /// <summary>
        /// Gets an ID from the Repository for this Data Source.
        /// </summary>
        /// <param name="reportingServer2005">The SQL 2005 reporting server connection</param>
        /// <param name="path">Where the Datasource is in the Reporting Server</param>
        /// <returns>The Repository ID.</returns>
        private int HandleDataSource(SSRS2005.ReportingService2005 reportingServer2005, string path)
        {
            int idValue;
            SSRS2005.DataSourceDefinition dsDef = reportingServer2005.GetDataSourceContents(path);
            if (!datasourceInRepository.TryGetValue(path, out idValue))
            {
                idValue = repository.GetConnection(dsDef.ConnectString);
                if (idValue == -1)
                {
                    idValue = repository.AddObject(path, dsDef.ConnectString, oleDBTypeString, repository.RootRepositoryObjectID);
                    repository.AddAttribute(idValue, Repository.Attributes.ConnectionString, dsDef.ConnectString);
                    DbConnectionStringBuilder connectionStringBuilder = repository.GetConnectionStringBuilder(dsDef.ConnectString);
                    string Server = repository.GetServer(connectionStringBuilder);
                    string Database = repository.GetDatabase(connectionStringBuilder);

                    if (string.IsNullOrEmpty(Server))
                        repository.AddAttribute(idValue, Repository.Attributes.ConnectionServer, "(NULL)");
                    else
                        repository.AddAttribute(idValue, Repository.Attributes.ConnectionServer, Server);

                    if (string.IsNullOrEmpty(Database))
                        repository.AddAttribute(idValue, Repository.Attributes.ConnectionDatabase, "(NULL)");
                    else
                        repository.AddAttribute(idValue, Repository.Attributes.ConnectionDatabase, Database);
                }
                datasourceInRepository.Add(path, idValue);
            }
            return idValue;
        }

        private int HandleDataSource(string connectionString, string dsName)
        {
            int idValue;
            if (!datasourceInRepository.TryGetValue(dsName, out idValue))
            {
                idValue = repository.GetConnection(connectionString);
                if (idValue == -1)
                {
                    idValue = repository.AddObject(dsName, connectionString, oleDBTypeString, repository.RootRepositoryObjectID);
                    repository.AddAttribute(idValue, Repository.Attributes.ConnectionString, connectionString);
                    DbConnectionStringBuilder connectionStringBuilder = repository.GetConnectionStringBuilder(connectionString);
                    string Server = repository.GetServer(connectionStringBuilder);
                    string Database = repository.GetDatabase(connectionStringBuilder);

                    if (string.IsNullOrEmpty(Server))
                        repository.AddAttribute(idValue, Repository.Attributes.ConnectionServer, "(NULL)");
                    else
                        repository.AddAttribute(idValue, Repository.Attributes.ConnectionServer, Server);

                    if (string.IsNullOrEmpty(Database))
                        repository.AddAttribute(idValue, Repository.Attributes.ConnectionDatabase, "(NULL)");
                    else
                        repository.AddAttribute(idValue, Repository.Attributes.ConnectionDatabase, Database);
                }
                datasourceInRepository.Add(dsName, idValue);
            }
            return idValue;
        }
        #endregion


        private void Enumerate2010Server(SSRS2010.ReportingService2010 reportingServer2010, String path, bool recursive)
        {
            byte[] reportDefinition = null;
            String commandText = string.Empty;
            String dsName = string.Empty;
            String sdsName = string.Empty;
            int reportID = -1;
            int dsID = -1;
            SSRS2010.DataSourceDefinition dsDef;
            SSRS2010.DataSource[] dsArray;
            Dictionary<String, int> dsNameToRepository = new Dictionary<string, int>();

            // Get the array of items that are in the location that has been requested.
            SSRS2010.CatalogItem[] items = reportingServer2010.ListChildren(path, recursive);

            // Step through the items.
            foreach (SSRS2010.CatalogItem item in items)
            {
                dsNameToRepository.Clear();

                #region item.TypeName = Report
                if (item.TypeName == "Report")
                {
                    Console.WriteLine(string.Format("Analysing Report {0}", item.Name));
                    reportID = repository.AddObject(item.Path, item.Name, ReportEnumerator.ObjectTypes.Report, reportServerID);

                    reportDefinition = reportingServer2010.GetItemDefinition(item.Path);
                    dsArray = reportingServer2010.GetItemDataSources(item.Path);
                    foreach (SSRS2010.DataSource ds in dsArray)
                    {
                        if (ds.Item is SSRS2010.DataSourceReference)
                        {
                            dsNameToRepository.Add(ds.Name, HandleDataSource(reportingServer2010, ((SSRS2010.DataSourceReference)ds.Item).Reference));
                        }
                        else if (ds.Item is SSRS2010.DataSourceDefinition)
                        {
                            dsNameToRepository.Add(ds.Name, HandleDataSource(((SSRS2010.DataSourceDefinition)ds.Item).ConnectString, ds.Name));
                        }
                        else if (ds.Item is SSRS2010.InvalidDataSourceReference)
                        {
                            dsNameToRepository.Add(ds.Name, HandleDataSource(String.Empty, ds.Name));
                        }
                    }

                    using (var stream = new MemoryStream(reportDefinition))
                    {
                        using (var xmlreader = new XmlTextReader(stream))
                        {
                            xmlreader.MoveToContent();
                            if (xmlreader.ReadToDescendant("DataSet"))
                            {
                                do
                                {
                                    XmlReader dsReader = xmlreader.ReadSubtree();
                                    dsReader.MoveToContent();
                                    if (dsReader.IsStartElement("DataSet"))
                                    {
                                        dsReader.ReadStartElement();
                                        dsReader.MoveToContent();
                                    }
                                    if (dsReader.IsStartElement("Query"))
                                    {
                                        while (dsReader.ReadToDescendant("DataSourceName"))
                                        {
                                            dsName = dsReader.ReadString();
                                            dsID = -1;
                                            if (!dsNameToRepository.TryGetValue(dsName, out dsID))
                                            {
                                                Console.WriteLine(string.Format("Unable to locate DataSourceName {0} Using First Value", dsName));
                                                if (dsNameToRepository.Count == 0)
                                                    dsNameToRepository.Add(dsName, HandleDataSource(String.Empty, dsName));
                                                dsID = dsNameToRepository.ElementAt(0).Value;
                                            }
                                        }
                                        while (dsReader.ReadToNextSibling("CommandText"))
                                        {
                                            commandText = dsReader.ReadString();
                                        }
                                        if (dsID != -1)
                                        {
                                            ParseTSqlStatement(commandText, dsID, reportID);
                                        }
                                    }
                                    else if (dsReader.IsStartElement("Fields"))
                                    {
                                        dsReader.ReadInnerXml();
                                        if (dsReader.IsStartElement("Query"))
                                        {
                                            while (dsReader.ReadToDescendant("DataSourceName"))
                                            {
                                                dsName = dsReader.ReadString();
                                                dsID = -1;
                                                if (!dsNameToRepository.TryGetValue(dsName, out dsID))
                                                {
                                                    Console.WriteLine(string.Format("Unable to locate DataSourceName {0} Using First Value", dsName));
                                                    dsID = dsNameToRepository.ElementAt(0).Value;
                                                }
                                            }
                                            while (dsReader.ReadToNextSibling("CommandText"))
                                            {
                                                commandText = dsReader.ReadString();
                                            }
                                            if (dsID != -1)
                                            {
                                                ParseTSqlStatement(commandText, dsID, reportID);
                                            }
                                        } 
                                    }
                                    else
                                    {
                                        while (dsReader.ReadToDescendant("SharedDataSetReference"))
                                        {
                                            sdsName = dsReader.ReadString();
                                            repository.GetDataSet(sdsName, reportServerID);
                                        }
                                    }
                                }
                                while (xmlreader.ReadToFollowing("DataSet"));
                            }
                        }
                    }
                }
                #endregion
                else if (item.TypeName == "DataSource")
                {
                    HandleDataSource(reportingServer2010, item.Path);
                }
                else if (item.TypeName == "DataSet")
                {
                    reportDefinition = reportingServer2010.GetItemDefinition(item.Path);

                    dsArray = reportingServer2010.GetItemDataSources(item.Path);
                    foreach (SSRS2010.DataSource ds in dsArray)
                    {
                        if (ds.Item is SSRS2010.DataSourceReference)
                        {
                            dsNameToRepository.Add(ds.Name, HandleDataSource(reportingServer2010, ((SSRS2010.DataSourceReference)ds.Item).Reference));
                        }
                        else if (ds.Item is SSRS2010.DataSourceDefinition)
                        {
                            dsNameToRepository.Add(ds.Name, HandleDataSource(((SSRS2010.DataSourceDefinition)ds.Item).ConnectString, ds.Name));
                        }
                    }
                    
                    using (var stream = new MemoryStream(reportDefinition))
                    {
                        using (var xmlreader = new XmlTextReader(stream))
                        {
                            xmlreader.MoveToContent();
                            while (xmlreader.ReadToDescendant("DataSourceReference"))
                            {
                                dsName = xmlreader.ReadString();
                                dsID = -1;
                                if (!dsNameToRepository.TryGetValue(dsName, out dsID))
                                {
                                    Console.WriteLine(string.Format("Unable to locate DataSourceName {0} Using First Entry", dsName));
                                    dsID = dsNameToRepository.ElementAt(0).Value;
                                }
                            }
                            while (xmlreader.ReadToNextSibling("CommandText"))
                            {
                                commandText = xmlreader.ReadString();
                            }
                            if (dsID != -1)
                            {
                                ParseTSqlStatement(commandText, dsID, reportID);
                            }
                        }
                    }
                }
            }
        }



        private void Enumerate2005Server(SSRS2005.ReportingService2005 reportingServer2005, String path, bool recursive)
        {
            byte[] reportDefinition = null;
            String commandText = string.Empty;
            String dsName = string.Empty;
            String sdsName = string.Empty;
            int reportID = -1;
            int dsID = -1;
            SSRS2005.DataSourceDefinition dsDef;
            SSRS2005.DataSource[] dsArray;
            Dictionary<String, int> dsNameToRepository = new Dictionary<string, int>();

            // Get the array of items that are in the location that has been requested.
            SSRS2005.CatalogItem[] items = reportingServer2005.ListChildren(path, recursive);

            // Step through the items.
            foreach (SSRS2005.CatalogItem item in items)
            {
                dsNameToRepository.Clear();

                #region item.TypeName = Report
                if (item.Type == SSRS2005.ItemTypeEnum.Report)
                {
                    Console.WriteLine(string.Format("Analysing Report {0}", item.Name));
                    reportID = repository.AddObject(item.Path, item.Name, ReportEnumerator.ObjectTypes.Report, reportServerID);

                    reportDefinition = reportingServer2005.GetReportDefinition(item.Path);
                    dsArray = reportingServer2005.GetItemDataSources(item.Path);
                    foreach (SSRS2005.DataSource ds in dsArray)
                    {
                        if (ds.Item is SSRS2005.DataSourceReference)
                        {
                            dsNameToRepository.Add(ds.Name, HandleDataSource(reportingServer2005, ((SSRS2005.DataSourceReference)ds.Item).Reference));
                        }
                        else if (ds.Item is SSRS2005.DataSourceDefinition)
                        {
                            dsNameToRepository.Add(ds.Name, HandleDataSource(((SSRS2005.DataSourceDefinition)ds.Item).ConnectString, ds.Name));
                        }
                    }

                    using (var stream = new MemoryStream(reportDefinition))
                    {
                        using (var xmlreader = new XmlTextReader(stream))
                        {
                            xmlreader.MoveToContent();
                            if (xmlreader.ReadToDescendant("DataSet"))
                            {
                                do
                                {
                                    XmlReader dsReader = xmlreader.ReadSubtree();
                                    dsReader.MoveToContent();
                                    if (dsReader.IsStartElement("DataSet"))
                                    {
                                        dsReader.ReadStartElement();
                                        dsReader.MoveToContent();
                                    }
                                    if (dsReader.IsStartElement("Query"))
                                    {
                                        while (dsReader.ReadToDescendant("DataSourceName"))
                                        {
                                            dsName = dsReader.ReadString();
                                            dsID = -1;
                                            if (!dsNameToRepository.TryGetValue(dsName, out dsID))
                                            {
                                                Console.WriteLine(string.Format("Unable to locate DataSourceName {0} Using First Value", dsName));
                                                dsID = dsNameToRepository.ElementAt(0).Value;
                                            }
                                        }
                                        while (dsReader.ReadToNextSibling("CommandText"))
                                        {
                                            commandText = dsReader.ReadString();
                                        }
                                        if (dsID != -1)
                                        {
                                            ParseTSqlStatement(commandText, dsID, reportID);
                                        }
                                    }
                                    else if (dsReader.IsStartElement("Fields"))
                                    {
                                        dsReader.ReadInnerXml();
                                        if (dsReader.IsStartElement("Query"))
                                        {
                                            while (dsReader.ReadToDescendant("DataSourceName"))
                                            {
                                                dsName = dsReader.ReadString();
                                                dsID = -1;
                                                if (!dsNameToRepository.TryGetValue(dsName, out dsID))
                                                {
                                                    Console.WriteLine(string.Format("Unable to locate DataSourceName {0} Using First Value", dsName));
                                                    dsID = dsNameToRepository.ElementAt(0).Value;
                                                }
                                            }
                                            while (dsReader.ReadToNextSibling("CommandText"))
                                            {
                                                commandText = dsReader.ReadString();
                                            }
                                            if (dsID != -1)
                                            {
                                                ParseTSqlStatement(commandText, dsID, reportID);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        while (dsReader.ReadToDescendant("SharedDataSetReference"))
                                        {
                                            sdsName = dsReader.ReadString();
                                            repository.GetDataSet(sdsName, reportServerID);
                                        }
                                    }
                                }
                                while (xmlreader.ReadToFollowing("DataSet"));
                            }
                        }
                    }
                }
                #endregion
                else if (item.Type == SSRS2005.ItemTypeEnum.DataSource)
                {
                    HandleDataSource(reportingServer2005, item.Path);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SSISMeta"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "codeplex")]
        private void ParseTSqlStatement(string statement, int connectionID, int reportID)
        {
            SqlStatement toBeParsed = new SqlStatement();

            try
            {
                toBeParsed.quotedIdentifiers = true;
                if (toBeParsed.ParseString(statement))
                {
                    foreach (string tableName in toBeParsed.getTableNames(true))
                    {
                        int tableID = repository.GetTable(connectionID, tableName);
                        if (!repository.DoesMappingExist(tableID, reportID))
                            repository.AddMapping(tableID, reportID);
                    }
                    foreach (string procedureName in toBeParsed.getProcedureNames(true))
                    {
                        int procID = repository.GetProcedure(connectionID, procedureName);
                        if (!repository.DoesMappingExist(procID, reportID))
                            repository.AddMapping(procID, reportID);
                    }
                    foreach (string funcName in toBeParsed.getFunctionNames(true))
                    {
                        int funcID = repository.GetFunction(connectionID, funcName);
                        if (!repository.DoesMappingExist(funcID, reportID))
                            repository.AddMapping(funcID, reportID);
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
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("SQL Statement Failed with Exception {0}\r\nStatement Was:{1}\r\nPlease report to SSISMeta.codeplex.com\r\n", ex.Message, statement));
            }
        }

    }
}
