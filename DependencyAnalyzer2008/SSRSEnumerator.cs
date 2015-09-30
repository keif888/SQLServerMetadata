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
        private bool threePartNames;

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

            threePartNames = false;
            return success;
        }

        public void EnumerateReportingServer(string reportingServerURL, bool recursive, bool storeThreePartNames)
        {
            Boolean isRS2010 = true, isRS2005 = false;
            Uri reportingServerUri = new Uri(reportingServerURL);
            string decodedURL, decodedPath;
            SSRS2010.CatalogItem[] items2010 = null;
            SSRS2010.ReportingService2010 reportingServer2010 = new SSRS2010.ReportingService2010();

            threePartNames = storeThreePartNames;
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
                    idValue = repository.AddObject(path, dsDef.ConnectString, Repository.OLEDBGuid, repository.RootRepositoryObjectID);
                    repository.AddAttribute(idValue, Repository.Attributes.ConnectionString, dsDef.ConnectString);
                    DbConnectionStringBuilder connectionStringBuilder = repository.GetConnectionStringBuilder(dsDef.ConnectString);
                    if (connectionStringBuilder == null)
                    {
                        connectionStringBuilder = repository.GetConnectionStringBuilder(string.Empty);
                    }
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
                    idValue = repository.AddObject(path, dsDef.ConnectString, Repository.OLEDBGuid, repository.RootRepositoryObjectID);
                    repository.AddAttribute(idValue, Repository.Attributes.ConnectionString, dsDef.ConnectString);
                    DbConnectionStringBuilder connectionStringBuilder = repository.GetConnectionStringBuilder(dsDef.ConnectString);
                    if (connectionStringBuilder == null)
                    {
                        connectionStringBuilder = repository.GetConnectionStringBuilder(String.Empty);
                    }
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
                if (String.IsNullOrEmpty(connectionString))
                {
                    idValue = repository.GetConnection("NULL or Empty Connection String");
                }
                else
                {
                    idValue = repository.GetConnection(connectionString);
                }
                if (idValue == -1)
                {
                    idValue = repository.AddObject(dsName, connectionString, Repository.OLEDBGuid, repository.RootRepositoryObjectID);
                    repository.AddAttribute(idValue, Repository.Attributes.ConnectionString, connectionString);
                    DbConnectionStringBuilder connectionStringBuilder = repository.GetConnectionStringBuilder(connectionString);
                    if (connectionStringBuilder == null)
                        connectionStringBuilder = repository.GetConnectionStringBuilder(string.Empty);
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
                            if (!String.IsNullOrEmpty(((SSRS2010.DataSourceDefinition)ds.Item).ConnectString))  // Don't create if there is an empty Connection String.
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
                            // Find any DataSources
                            if (xmlreader.ReadToDescendant("DataSource"))
                            {
                                do
                                {
                                    XmlReader dsReader = xmlreader.ReadSubtree();
                                    dsReader.MoveToContent();
                                    if (dsReader.IsStartElement("DataSource"))
                                    {
                                        dsName = dsReader.GetAttribute("Name");
                                        while (dsReader.ReadToDescendant("ConnectString"))
                                        {
                                            String connectionString = dsReader.ReadString();
                                            if (String.IsNullOrEmpty(connectionString))
                                                connectionString = String.Empty;
                                            if (!dsNameToRepository.TryGetValue(dsName, out dsID))
                                            {
                                                Console.WriteLine(string.Format("Unable to locate DataSourceName {0} Using First Value", dsName));
                                                dsNameToRepository.Add(dsName, HandleDataSource(connectionString, dsName));
                                            }
                                        }
                                    }
                                }
                                while (xmlreader.ReadToFollowing("DataSource"));
                            }
                        }
                    }
                    using (var stream = new MemoryStream(reportDefinition))
                    {
                        using (var xmlreader = new XmlTextReader(stream))
                        {
                            xmlreader.MoveToContent();
                            // Now find any DataSet.
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
                                                Console.WriteLine(string.Format("Unable to locate DataSourceName {0} Creating with Empty Connection String", dsName));
                                                dsNameToRepository.Add(dsName, HandleDataSource(String.Empty, dsName));
                                                dsNameToRepository.TryGetValue(dsName, out dsID);
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
                                            dsID = repository.GetDataSet(sdsName, reportServerID);
                                            if (!repository.DoesMappingExist(dsID, reportID))
                                                repository.AddMapping(dsID, reportID);
                                            dsID = -1;
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
                    reportID = repository.GetDataSet(item.Path, reportServerID);
                    dsArray = reportingServer2010.GetItemDataSources(item.Path);
                    foreach (SSRS2010.DataSource ds in dsArray)
                    {
                        if (ds.Item is SSRS2010.DataSourceReference)
                        {
                            dsNameToRepository.Add(((SSRS2010.DataSourceReference)ds.Item).Reference, HandleDataSource(reportingServer2010, ((SSRS2010.DataSourceReference)ds.Item).Reference));
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
                                            dsID = repository.GetDataSet(sdsName, reportServerID);
                                            if (!repository.DoesMappingExist(dsID, reportID))
                                                repository.AddMapping(dsID, reportID);
                                            dsID = -1;
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
            String existingDBName = String.Empty;
            object existing;
            String connectionString = repository.RetrieveConnectionString(connectionID);
            OleDbConnectionStringBuilder csBuilder = (OleDbConnectionStringBuilder)repository.GetConnectionStringBuilder(connectionString);
            if (csBuilder != null)
            {
                if (csBuilder.TryGetValue(Repository.ConnectionStringProperties.InitialCatalog, out existing))
                    existingDBName = (String)existing;
                else if (csBuilder.TryGetValue(Repository.ConnectionStringProperties.Database, out existing))
                    existingDBName = (String)existing;
            }
            else
            {
                csBuilder = (OleDbConnectionStringBuilder)repository.GetConnectionStringBuilder(String.Empty);
            }

            try
            {
                toBeParsed.quotedIdentifiers = true;
                if (toBeParsed.ParseString(statement))
                {
                    foreach (string tableName in toBeParsed.getTableNames(true))
                    {
                        int tableID = -1;
                        string[] tableParts = tableName.Split('.');
                        switch(tableParts.Length)
                        {
                            case 3:
                                String dbName = tableParts[0].Replace("[", "").Replace("]", "");
                                if (csBuilder.ContainsKey(Repository.ConnectionStringProperties.InitialCatalog))
                                {
                                    csBuilder.Remove(Repository.ConnectionStringProperties.InitialCatalog);
                                    csBuilder.Add(Repository.ConnectionStringProperties.InitialCatalog, dbName);
                                    connectionString = csBuilder.ConnectionString;
                                }
                                else if (csBuilder.ContainsKey(Repository.ConnectionStringProperties.Database))
                                {
                                    csBuilder.Remove(Repository.ConnectionStringProperties.Database);
                                    csBuilder.Add(Repository.ConnectionStringProperties.Database, dbName);
                                    connectionString = csBuilder.ConnectionString;
                                }
                                int objectConnectionId = repository.GetConnection(connectionString);
                                if (objectConnectionId == -1)
                                {
                                    // Need to add a new connectionID.
                                    objectConnectionId = repository.AddObject("CMD " + dbName, string.Empty, Repository.OLEDBGuid, repository.RootRepositoryObjectID);
                                    repository.AddAttribute(objectConnectionId, Repository.Attributes.ConnectionString, connectionString);
                                    repository.AddAttribute(objectConnectionId, Repository.Attributes.ConnectionServer, csBuilder.DataSource);
                                    repository.AddAttribute(objectConnectionId, Repository.Attributes.ConnectionDatabase, dbName);
                                }
                                if (threePartNames)
                                    tableID = repository.GetTable(objectConnectionId, String.Format("{0}.{1}.{2}", tableParts[0], tableParts[1], tableParts[2]));
                                else
                                    tableID = repository.GetTable(objectConnectionId, String.Format("{0}.{1}", tableParts[1], tableParts[2]));
                                break;
                            default:
                                if (threePartNames)
                                    tableID = repository.GetTable(connectionID, String.Format("[{0}].{1}", existingDBName, tableName));
                                else
                                    tableID = repository.GetTable(connectionID, tableName);
                                break;
                        }
                        if (!repository.DoesMappingExist(tableID, reportID))
                            repository.AddMapping(tableID, reportID);
                    }
                    foreach (string procedureName in toBeParsed.getProcedureNames(true))
                    {
                        int procID = -1;
                        string[] procedureParts = procedureName.Split('.');
                        switch (procedureParts.Length)
                        {
                            case 3:
                                String dbName = procedureParts[0].Replace("[", "").Replace("]", "");
                                if (csBuilder.ContainsKey(Repository.ConnectionStringProperties.InitialCatalog))
                                {
                                    csBuilder.Remove(Repository.ConnectionStringProperties.InitialCatalog);
                                    csBuilder.Add(Repository.ConnectionStringProperties.InitialCatalog, dbName);
                                    connectionString = csBuilder.ConnectionString;
                                }
                                else if (csBuilder.ContainsKey(Repository.ConnectionStringProperties.Database))
                                {
                                    csBuilder.Remove(Repository.ConnectionStringProperties.Database);
                                    csBuilder.Add(Repository.ConnectionStringProperties.Database, dbName);
                                    connectionString = csBuilder.ConnectionString;
                                }
                                int objectConnectionId = repository.GetConnection(connectionString);
                                if (objectConnectionId == -1)
                                {
                                    // Need to add a new connectionID.
                                    objectConnectionId = repository.AddObject("CMD " + dbName, string.Empty, Repository.OLEDBGuid, repository.RootRepositoryObjectID);
                                    repository.AddAttribute(objectConnectionId, Repository.Attributes.ConnectionString, connectionString);
                                    repository.AddAttribute(objectConnectionId, Repository.Attributes.ConnectionServer, csBuilder.DataSource);
                                    repository.AddAttribute(objectConnectionId, Repository.Attributes.ConnectionDatabase, dbName);
                                }
                                if (threePartNames)
                                    procID = repository.GetProcedure(objectConnectionId, String.Format("{0}.{1}.{2}", procedureParts[0], procedureParts[1], procedureParts[2]));
                                else
                                    procID = repository.GetProcedure(objectConnectionId, String.Format("{0}.{1}", procedureParts[1], procedureParts[2]));
                                break;
                            default:
                                if (threePartNames)
                                    procID = repository.GetProcedure(connectionID, String.Format("[{0}].{1}", existingDBName, procedureName));
                                else
                                    procID = repository.GetProcedure(connectionID, procedureName);
                                break;
                        }
                        if (!repository.DoesMappingExist(procID, reportID))
                            repository.AddMapping(procID, reportID);
                    }
                    foreach (string funcName in toBeParsed.getFunctionNames(true))
                    {
                        int funcID = -1;
                        string[] functionParts = funcName.Split('.');
                        switch (functionParts.Length)
                        {
                            case 3:
                                String dbName = functionParts[0].Replace("[", "").Replace("]", "");
                                if (csBuilder.ContainsKey(Repository.ConnectionStringProperties.InitialCatalog))
                                {
                                    csBuilder.Remove(Repository.ConnectionStringProperties.InitialCatalog);
                                    csBuilder.Add(Repository.ConnectionStringProperties.InitialCatalog, dbName);
                                    connectionString = csBuilder.ConnectionString;
                                }
                                else if (csBuilder.ContainsKey(Repository.ConnectionStringProperties.Database))
                                {
                                    csBuilder.Remove(Repository.ConnectionStringProperties.Database);
                                    csBuilder.Add(Repository.ConnectionStringProperties.Database, dbName);
                                    connectionString = csBuilder.ConnectionString;
                                }
                                int objectConnectionId = repository.GetConnection(connectionString);
                                if (objectConnectionId == -1)
                                {
                                    // Need to add a new connectionID.
                                    objectConnectionId = repository.AddObject("CMD " + dbName, string.Empty, Repository.OLEDBGuid, repository.RootRepositoryObjectID);
                                    repository.AddAttribute(objectConnectionId, Repository.Attributes.ConnectionString, connectionString);
                                    repository.AddAttribute(objectConnectionId, Repository.Attributes.ConnectionServer, csBuilder.DataSource);
                                    repository.AddAttribute(objectConnectionId, Repository.Attributes.ConnectionDatabase, dbName);
                                }
                                if (threePartNames)
                                    funcID = repository.GetFunction(objectConnectionId, String.Format("{0}.{1}.{2}", functionParts[0], functionParts[1], functionParts[2]));
                                else
                                    funcID = repository.GetFunction(objectConnectionId, String.Format("{0}.{1}", functionParts[1], functionParts[2]));
                                break;
                            default:
                                if (threePartNames)
                                    funcID = repository.GetFunction(connectionID, String.Format("[{0}].{1}", existingDBName, funcName));
                                else
                                    funcID = repository.GetFunction(connectionID, funcName);
                                break;
                        }
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
