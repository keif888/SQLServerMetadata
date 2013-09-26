using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;


namespace Microsoft.Samples.DependencyAnalyzer
{
    class SQLDBEnumerator
    {
        private Repository repository;
        private SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder();

#if SQL2005
        private const string OLEDBGuid = "{9B5D63AB-A629-4A56-9F3E-B1044134B649}";
#else
        private const string OLEDBGuid = "{3BA51769-6C3C-46B2-85A1-81E58DB7DAE1}";
#endif
        public SQLDBEnumerator()
        {}

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
                Console.WriteLine(string.Format("Could not initialize the Database Enumerator: {0}", ex.Message));
                success = false;
            }

            return success;
        }

        public void EnumerateDatabase(string dbConnection)
        {
            string serverName = string.Empty;
            string loginName = string.Empty;
            string password = string.Empty;
            Server sqlServer;
            ServerConnection sqlConnection;
            Database sqlDatabase;

            connectionStringBuilder.Clear();
            connectionStringBuilder.ConnectionString = dbConnection;

            try
            {
                sqlConnection = new ServerConnection(connectionStringBuilder.DataSource);
                sqlConnection.ApplicationName = "DependencyAnalyzer";
                
                if (connectionStringBuilder.IntegratedSecurity)
                {
                    sqlConnection.LoginSecure = true;
                }
                else
                {
                    sqlConnection.LoginSecure = false;
                    sqlConnection.Login = connectionStringBuilder.UserID;
                    sqlConnection.Password = connectionStringBuilder.Password;
                }
                sqlServer = new Server(sqlConnection);
                sqlServer.SetDefaultInitFields(typeof(Table), "Name");
                sqlServer.SetDefaultInitFields(typeof(Table), "Schema");
                sqlServer.SetDefaultInitFields(typeof(Table), "IsSystemObject");
                sqlServer.SetDefaultInitFields(typeof(View), "Name");
                sqlServer.SetDefaultInitFields(typeof(View), "Schema");
                sqlServer.SetDefaultInitFields(typeof(View), "IsSystemObject");
                sqlServer.SetDefaultInitFields(typeof(StoredProcedure), "Name");
                sqlServer.SetDefaultInitFields(typeof(StoredProcedure), "Schema");
                sqlServer.SetDefaultInitFields(typeof(StoredProcedure), "IsSystemObject");
                sqlServer.SetDefaultInitFields(typeof(UserDefinedFunction), "Name");
                sqlServer.SetDefaultInitFields(typeof(UserDefinedFunction), "Schema");
                sqlServer.SetDefaultInitFields(typeof(UserDefinedFunction), "IsSystemObject");

                int connectionID = repository.GetConnection(connectionStringBuilder.ConnectionString);
                if (connectionID == -1)
                {
                    // Need to add a new connectionID.
                    connectionID = repository.AddObject("CMD " + connectionStringBuilder.InitialCatalog, string.Empty, OLEDBGuid, repository.RootRepositoryObjectID);
                    repository.AddAttribute(connectionID, Repository.Attributes.ConnectionString, connectionStringBuilder.ConnectionString);
                    repository.AddAttribute(connectionID, Repository.Attributes.ConnectionServer, connectionStringBuilder.DataSource);
                    repository.AddAttribute(connectionID, Repository.Attributes.ConnectionDatabase, connectionStringBuilder.InitialCatalog);
                }
                sqlDatabase = sqlServer.Databases[connectionStringBuilder.InitialCatalog];
                DependencyWalker findDepenency = new DependencyWalker(sqlServer);
                List<SqlSmoObject> walkerList = new List<SqlSmoObject>();
                SqlSmoObject[] walkerArray;
                ScriptingOptions scriptOptions = new ScriptingOptions();
                scriptOptions.AllowSystemObjects = false;

                string objectName, parentName;
                int tableID, parentID, arrayCounter;
                DependencyTree dependTree;
                DependencyTreeNode walkNode, parentNode;


                Console.WriteLine("Enumerate the Tables and their Dependencies");
                // Enumerate the Tables
                arrayCounter = 0;
                foreach (Table sqlTable in sqlDatabase.Tables)
                {
                    if (!sqlTable.IsSystemObject)
                    {
                        walkerList.Add(sqlTable);
                    }
                }
                walkerArray = new SqlSmoObject[walkerList.Count];
                foreach (SqlSmoObject smoObject in walkerList)
                {
                    walkerArray[arrayCounter++] = smoObject;
                }

                if (walkerArray.Length > 0)
                {

                    dependTree = findDepenency.DiscoverDependencies(walkerArray, DependencyType.Parents);
                    // Walk the tree
                    parentNode = dependTree.FirstChild;
                    do
                    {
                        objectName = "[" + parentNode.Urn.GetAttribute("Schema") + "].[" + parentNode.Urn.GetAttribute("Name") + "]";
                        tableID = repository.GetTable(connectionID, objectName);
                        if (parentNode.HasChildNodes)
                        {
                            // Point at the first dependency
                            walkNode = parentNode.FirstChild;
                            do
                            {
                                parentName = "[" + walkNode.Urn.GetAttribute("Schema") + "].[" + walkNode.Urn.GetAttribute("Name") + "]";
                                switch (walkNode.Urn.Type)
                                {
                                    case "Table":
                                    case "View":
                                        parentID = repository.GetTable(connectionID, parentName);
                                        break;
                                    case "UserDefinedFunction":
                                        parentID = repository.GetFunction(connectionID, parentName);
                                        break;
                                    case "StoredProcedure":
                                        parentID = repository.GetProcedure(connectionID, parentName);
                                        break;
                                    case "PartitionScheme":
                                    case "XmlSchemaCollection":
                                        parentID = -1;
                                        break;

                                    default:
                                        if (sqlDatabase.StoredProcedures.Contains(walkNode.Urn.GetAttribute("Name"), walkNode.Urn.GetAttribute("Schema")))
                                        {
                                            parentID = repository.GetProcedure(connectionID, parentName);
                                        }
                                        else if (sqlDatabase.UserDefinedFunctions.Contains(walkNode.Urn.GetAttribute("Name"), walkNode.Urn.GetAttribute("Schema")))
                                        {
                                            parentID = repository.GetFunction(connectionID, parentName);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Warning: Unresolvable Dependency encountered in object {0} for object {1} of type {2}", objectName, parentName, walkNode.Urn.Type);
                                            parentID = repository.GetTable(connectionID, parentName);
                                        }
                                        break;
                                }
                                if (parentID > -1)
                                    repository.AddMapping(parentID, tableID);
                                walkNode = walkNode.NextSibling;
                            }
                            while (walkNode != null);
                        }
                        parentNode = parentNode.NextSibling;
                    }
                    while (parentNode != null);
                }
                Console.WriteLine("Enumerate the Views and their Dependencies");
                walkerList.Clear();
                arrayCounter = 0;
                foreach (View sqlView in sqlDatabase.Views)
                {
                    if (!sqlView.IsSystemObject)
                    {
                        walkerList.Add(sqlView);
                    }
                }
                walkerArray = new SqlSmoObject[walkerList.Count];
                foreach (SqlSmoObject smoObject in walkerList)
                {
                    walkerArray[arrayCounter++] = smoObject;
                }

                if (walkerArray.Length > 0)
                {

                    dependTree = findDepenency.DiscoverDependencies(walkerArray, DependencyType.Parents);
                    // Walk the tree
                    parentNode = dependTree.FirstChild;
                    do
                    {
                        objectName = "[" + parentNode.Urn.GetAttribute("Schema") + "].[" + parentNode.Urn.GetAttribute("Name") + "]";
                        // The following is because we don't know the type of a view in the TSQLParser...
                        tableID = repository.GetTable(connectionID, objectName);
                        if (parentNode.HasChildNodes)
                        {
                            // Point at the first dependency
                            walkNode = parentNode.FirstChild;
                            do
                            {
                                parentName = "[" + walkNode.Urn.GetAttribute("Schema") + "].[" + walkNode.Urn.GetAttribute("Name") + "]";
                                switch (walkNode.Urn.Type)
                                {
                                    case "Table":
                                    case "View":
                                        parentID = repository.GetTable(connectionID, parentName);
                                        break;
                                    case "UserDefinedFunction":
                                        parentID = repository.GetFunction(connectionID, parentName);
                                        break;
                                    case "StoredProcedure":
                                        parentID = repository.GetProcedure(connectionID, parentName);
                                        break;
                                    case "PartitionScheme":
                                    case "XmlSchemaCollection":
                                        parentID = -1;
                                        break;
                                    default:
                                        if (sqlDatabase.StoredProcedures.Contains(walkNode.Urn.GetAttribute("Name"), walkNode.Urn.GetAttribute("Schema")))
                                        {
                                            parentID = repository.GetProcedure(connectionID, parentName);
                                        }
                                        else if (sqlDatabase.UserDefinedFunctions.Contains(walkNode.Urn.GetAttribute("Name"), walkNode.Urn.GetAttribute("Schema")))
                                        {
                                            parentID = repository.GetFunction(connectionID, parentName);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Warning: Unresolvable Dependency encountered in object {0} for object {1} of type {2}", objectName, parentName, walkNode.Urn.Type);
                                            parentID = repository.GetTable(connectionID, parentName);
                                        }
                                        break;
                                }
                                if (parentID > -1)
                                    repository.AddMapping(parentID, tableID);
                                walkNode = walkNode.NextSibling;
                            }
                            while (walkNode != null);
                        }
                        parentNode = parentNode.NextSibling;
                    }
                    while (parentNode != null);

                }
                Console.WriteLine("Enumerate the StoredProcedures and their Dependencies");
                walkerList.Clear();
                arrayCounter = 0;
                foreach (StoredProcedure sqlProcedure in sqlDatabase.StoredProcedures)
                {
                    if (!sqlProcedure.IsSystemObject)
                    {
                        walkerList.Add(sqlProcedure);
                    }
                }
                walkerArray = new SqlSmoObject[walkerList.Count];
                foreach (SqlSmoObject smoObject in walkerList)
                {
                    walkerArray[arrayCounter++] = smoObject;
                }

                if (walkerArray.Length > 0)
                {
                    dependTree = findDepenency.DiscoverDependencies(walkerArray, DependencyType.Parents);
                    // Walk the tree
                    parentNode = dependTree.FirstChild;
                    do
                    {
                        objectName = "[" + parentNode.Urn.GetAttribute("Schema") + "].[" + parentNode.Urn.GetAttribute("Name") + "]";
                        // The following is because we don't know the type of a view in the TSQLParser...
                        tableID = repository.GetProcedure(connectionID, objectName);
                        if (parentNode.HasChildNodes)
                        {
                            // Point at the first dependency
                            walkNode = parentNode.FirstChild;
                            do
                            {
                                parentName = "[" + walkNode.Urn.GetAttribute("Schema") + "].[" + walkNode.Urn.GetAttribute("Name") + "]";
                                switch (walkNode.Urn.Type)
                                {
                                    case "Table":
                                    case "View":
                                        parentID = repository.GetTable(connectionID, parentName);
                                        break;
                                    case "UserDefinedFunction":
                                        parentID = repository.GetFunction(connectionID, parentName);
                                        break;
                                    case "StoredProcedure":
                                        parentID = repository.GetProcedure(connectionID, parentName);
                                        break;
                                    case "PartitionScheme":
                                    case "XmlSchemaCollection":
                                        parentID = -1;
                                        break;

                                    default:
                                        if (sqlDatabase.StoredProcedures.Contains(walkNode.Urn.GetAttribute("Name"), walkNode.Urn.GetAttribute("Schema")))
                                        {
                                            parentID = repository.GetProcedure(connectionID, parentName);
                                        }
                                        else if (sqlDatabase.UserDefinedFunctions.Contains(walkNode.Urn.GetAttribute("Name"), walkNode.Urn.GetAttribute("Schema")))
                                        {
                                            parentID = repository.GetFunction(connectionID, parentName);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Warning: Unresolvable Dependency encountered in object {0} for object {1} of type {2}", objectName, parentName, walkNode.Urn.Type);
                                            parentID = repository.GetTable(connectionID, parentName);
                                        }
                                        break;
                                }
                                if (parentID > -1)
                                    repository.AddMapping(parentID, tableID);
                                walkNode = walkNode.NextSibling;
                            }
                            while (walkNode != null);
                        }
                        parentNode = parentNode.NextSibling;
                    }
                    while (parentNode != null);
                }
                Console.WriteLine("Enumerate the UserDefinedFunction and their Dependencies");

                walkerList.Clear();
                arrayCounter = 0;
                foreach (UserDefinedFunction sqlFunctions in sqlDatabase.UserDefinedFunctions)
                {
                    if (!sqlFunctions.IsSystemObject)
                    {
                        walkerList.Add(sqlFunctions);
                    }
                }
                walkerArray = new SqlSmoObject[walkerList.Count];
                foreach (SqlSmoObject smoObject in walkerList)
                {
                    walkerArray[arrayCounter++] = smoObject;
                }

                if (walkerArray.Length > 0)
                {
                    dependTree = findDepenency.DiscoverDependencies(walkerArray, DependencyType.Parents);
                    // Walk the tree
                    parentNode = dependTree.FirstChild;
                    do
                    {
                        objectName = "[" + parentNode.Urn.GetAttribute("Schema") + "].[" + parentNode.Urn.GetAttribute("Name") + "]";
                        // The following is because we don't know the type of a view in the TSQLParser...
                        tableID = repository.GetFunction(connectionID, objectName);
                        if (parentNode.HasChildNodes)
                        {
                            // Point at the first dependency
                            walkNode = parentNode.FirstChild;
                            do
                            {
                                parentName = "[" + walkNode.Urn.GetAttribute("Schema") + "].[" + walkNode.Urn.GetAttribute("Name") + "]";
                                switch (walkNode.Urn.Type)
                                {
                                    case "Table":
                                    case "View":
                                        parentID = repository.GetTable(connectionID, parentName);
                                        break;
                                    case "UserDefinedFunction":
                                        parentID = repository.GetFunction(connectionID, parentName);
                                        break;
                                    case "StoredProcedure":
                                        parentID = repository.GetProcedure(connectionID, parentName);
                                        break;
                                    case "PartitionScheme":
                                    case "XmlSchemaCollection":
                                        parentID = -1;
                                        break;

                                    default:
                                        if (sqlDatabase.StoredProcedures.Contains(walkNode.Urn.GetAttribute("Name"), walkNode.Urn.GetAttribute("Schema")))
                                        {
                                            parentID = repository.GetProcedure(connectionID, parentName);
                                        }
                                        else if (sqlDatabase.UserDefinedFunctions.Contains(walkNode.Urn.GetAttribute("Name"), walkNode.Urn.GetAttribute("Schema")))
                                        {
                                            parentID = repository.GetFunction(connectionID, parentName);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Warning: Unresolvable Dependency encountered in object {0} for object {1} of type {2}", objectName, parentName, walkNode.Urn.Type);
                                            parentID = repository.GetTable(connectionID, parentName);
                                        }
                                        break;
                                }
                                if (parentID > -1)
                                    repository.AddMapping(parentID, tableID);
                                walkNode = walkNode.NextSibling;
                            }
                            while (walkNode != null);
                        }
                        parentNode = parentNode.NextSibling;
                    }
                    while (parentNode != null);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(string.Format("Could not enumerate the database: {0}", err.Message));
            }
        }
    }
}
