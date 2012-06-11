///
/// Microsoft SQL Server 2005 Business Intelligence Metadata Reporting Samples
/// Dependency Analyzer Sample
/// 
/// Copyright (c) Microsoft Corporation.  All rights reserved.
///
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using Microsoft.AnalysisServices; 

namespace Microsoft.Samples.DependencyAnalyzer
{
    /// <summary>
    ///  Enumerates Analysis Services objects and the relationships between them. 
    ///  The result of this enumeration is persisted in a repository.
    /// </summary>
    class SSASEnumerator
    {
        /// <summary>
        ///  For storing repository information
        /// </summary>
        private class DsvRepositoryInformation
        {
            public int dsvID;
            public Dictionary<string, int> dsvTableNameToIDMap;
        }

        private Repository repository;

        /// <summary>
        /// Types of Analysis Services objects we recognise for the repository
        /// </summary>
        private class ObjectTypes
        {
            internal const string Server = "Ssas.Analysis Server";
            internal const string Database = "Ssas.Database";
            internal const string Cube = "Ssas.Cube";
            internal const string CubeDimension = "Ssas.Cube Dimension";
            internal const string MeasureGroup = "Ssas.Measure Group";
            internal const string MeasureGroupDimension = "Ssas.Measure Group Dimension";
            internal const string Partition = "Ssas.Partition";
            internal const string DatabaseDimension = "Ssas.Database Dimension";
            internal const string DataSource = "Ssas.DataSource";
            internal const string DataSourceView = "Ssas.DSV";
            internal const string DataSourceViewTable = "Ssas.DSV Table";
        }

        /// <summary>
        ///  since we can't get the quoting characters offline, we'll have to hard code them here.
        /// </summary>
        private class QuotingCharacters
        {
            public static string OpenQuote = "[";
            public static string CloseQuote = "]";
        }

        public bool Initialize(Repository repository)
        {
            this.repository = repository;

            // add types of objects to the object types table
            AddASObjectType(ObjectTypes.Server, "Analysis Server");
            AddASObjectType(ObjectTypes.Database, "Database");
            AddASObjectType(ObjectTypes.DataSource, "Data Source");
            AddASObjectType(ObjectTypes.DataSourceView, "Data Source View");
            AddASObjectType(ObjectTypes.DataSourceViewTable, "Data Source View Table");
            AddASObjectType(ObjectTypes.Cube, "Cube");
            AddASObjectType(ObjectTypes.MeasureGroup, "Measure Group");
            AddASObjectType(ObjectTypes.Partition, "Partition");
            AddASObjectType(ObjectTypes.DatabaseDimension, "Database Dimension");
            AddASObjectType(ObjectTypes.CubeDimension, "Cube Dimension");
            AddASObjectType(ObjectTypes.MeasureGroupDimension, "Measure Group Dimension");

            return true;
        }

        private void AddASObjectType(string type, string name)
        {
            repository.AddObjectType(Repository.Domains.SSAS, type, name);
        }
        
        /// <summary>
        ///  Opens a connection to the Analysis Server and does the actual object model walk
        /// </summary>
        /// <param name="connectionString"></param>
        public void EnumerateServer(string connectionString)
        {
            using (Microsoft.AnalysisServices.Server server = new Server())
            {
                server.Connect(connectionString);

                int svID = repository.AddObject(server.Name, server.Description, ObjectTypes.Server, 0);

                if (connectionString.ToLower().Contains("catalog"))
                {
                    Database db = server.Databases[server.ConnectionInfo.Catalog];
                    Console.Write(string.Format("Enumerating '{0}'...", db.Name));
                    EnumerateDatabase(svID, db);
                    Console.WriteLine("Done");
                }
                else
                {
                    // get the databases
                    foreach (Database db in server.Databases)
                    {
                        Console.Write(string.Format("Enumerating '{0}'...", db.Name));
                        EnumerateDatabase(svID, db);
                        Console.WriteLine("Done");
                    }
                }
            }

        }

        private void EnumerateDatabase(int svID, Database db)
        {
            int dbID = repository.AddObject(db.Name, db.Description, ObjectTypes.Database, svID);
            Dictionary<DataSource, int> dsToIdMap; 

            EnumerateDataSources(db, dbID, out dsToIdMap);

            Dictionary<DataSourceView, DsvRepositoryInformation> dsvToObjectIDMap;
            EnumerateDataSourceViews(db, dbID, out dsvToObjectIDMap);

            Dictionary<Dimension, int> dimensionToIDMap;
            EnumerateDimensions(db, dbID, dsvToObjectIDMap, dsToIdMap, out dimensionToIDMap);

            EnumerateCubes(db, dbID, dsvToObjectIDMap, dsToIdMap, dimensionToIDMap);
        }

        private void EnumerateDataSourceViews(Database db, int dbID, out Dictionary<DataSourceView, DsvRepositoryInformation> dsvToObjectIDMap)
        {
            dsvToObjectIDMap = new Dictionary<DataSourceView, DsvRepositoryInformation>();

            foreach (DataSourceView dsv in db.DataSourceViews)
            {
                // get the connection ID
                int connectionID = -1;

                if (dsv.DataSource != null)
                {
                    connectionID = repository.GetConnection(dsv.DataSource.ConnectionString);

                    if (connectionID == -1)
                    {
                        connectionID = CreateConnection(dsv.DataSource);
                    }
                }

                // a DSV might exist without a data source. So, we have to use the database as the parent.
                int dsvID = repository.AddObject(dsv.Name, dsv.Description, ObjectTypes.DataSourceView, dbID);

                DsvRepositoryInformation dsvRepositoryInformation = new DsvRepositoryInformation();
                dsvRepositoryInformation.dsvID = dsvID;

                EnumerateDsvTables(dsv, connectionID, dsvID, out dsvRepositoryInformation.dsvTableNameToIDMap);

                dsvToObjectIDMap.Add(dsv, dsvRepositoryInformation);
            }
        }

        private int CreateConnection(DataSource ds)
        {                       
            int connectionID = repository.AddObject(ds.Name, string.Empty, ds.GetType().Name, repository.RootRepositoryObjectID);

            // add the attributes of the connection as well
           
            repository.AddAttribute(connectionID, Repository.Attributes.ConnectionString, ds.ConnectionString);

            /*// get the server name/initial catalog, etc. 
            string serverName;
            string initialCatalog;

            GetConnectionAttributes(connectionManager, out serverName, out initialCatalog);

            if (string.IsNullOrEmpty(serverName) == false)
            {
                repository.AddAttribute(connectionID, attributeConnectionServer, serverName);
            }

            if (string.IsNullOrEmpty(initialCatalog) == false)
            {
                repository.AddAttribute(connectionID, attributeConnectionDatabase, initialCatalog);
            }
            */
            return connectionID;
        }

        private void EnumerateDsvTables(DataSourceView dsv, int connectionID, int dsvID, out Dictionary<string,int> tableNameToIDMap)
        {
            tableNameToIDMap = new Dictionary<string, int>(dsv.Schema.Tables.Count); 

            foreach (DataTable table in dsv.Schema.Tables)
            {
                string tableType = GetExtendedProperty(table, ExtendedProperties.TableType);

                // add the data source view
                int dsvTableID = repository.AddObject(table.TableName, string.Empty, ObjectTypes.DataSourceViewTable, dsvID);

                tableNameToIDMap.Add(table.TableName, dsvTableID); 

                if (connectionID != -1)
                {
                    if (tableType == ExtendedProperties.TableTypeValue)
                    {
                        // this is mapped to an underlying table

                        // now add the table
                        string tableName = GetFullyQualifiedTableName(table);

                        int tableID = repository.GetTable(connectionID, tableName);
                        if (tableID == -1)
                        {
                            tableID = repository.AddObject(tableName, string.Empty, RelationalEnumerator.ObjectTypes.Table, connectionID);
                        }

                        // add the lineage
                        repository.AddMapping(tableID, dsvTableID);
                    }
                    else
                    {
                        if (tableType == ExtendedProperties.ViewTypeValue)
                        {
                            // add the query definition
                            object queryDefinition = table.ExtendedProperties["QueryDefinition"];
                            if (queryDefinition != null)
                            {
                                repository.AddAttribute(dsvTableID, Repository.Attributes.QueryDefinition, queryDefinition.ToString());
                            }
                        }
                    }
                }
            }
        }

        private void EnumerateDataSources(Database db, int dbID, out Dictionary<DataSource,int> dsToIdMap)
        {
            dsToIdMap = new Dictionary<DataSource, int>(db.DataSources.Count);

            foreach (DataSource ds in db.DataSources)
            {
                int dsID = repository.AddObject(ds.Name, ds.Description, ObjectTypes.DataSource, dbID);

                // add connections   
                int connectionID = repository.GetConnection(ds.ConnectionString);

                if (connectionID == -1)
                {
                    connectionID = repository.AddObject(ds.Name, ds.Description, string.Empty, repository.RootRepositoryObjectID);
                }

                dsToIdMap.Add(ds, connectionID);
            }
        }

        private void EnumerateDimensions(Database db, int dbID, Dictionary<DataSourceView, DsvRepositoryInformation> dsvToObjectIDMap, Dictionary<DataSource, int> dsToIDMap, out Dictionary<Dimension, int> dimensionToIdMap)
        {
            dimensionToIdMap = new Dictionary<Dimension, int>(db.Dimensions.Count);

            foreach (Dimension dim in db.Dimensions)
            {
                // get the connection ID
                int connectionID = -1;

                if ((dim.DataSource != null ) && (dsToIDMap.ContainsKey(dim.DataSource)))
                {
                    connectionID = dsToIDMap[dim.DataSource];
                }

                DsvRepositoryInformation dsvRepositoryInformation = dsvToObjectIDMap[dim.DataSourceView]; 
                Dictionary<string,int> dsvTableNameToIdMap = dsvRepositoryInformation.dsvTableNameToIDMap; 

                int dimID = repository.AddObject(dim.Name, dim.Description, ObjectTypes.DatabaseDimension, dbID);
                dimensionToIdMap.Add(dim, dimID); 

                Dictionary<int,bool> sourceTableIDs = new Dictionary<int,bool>();

                foreach (DimensionAttribute attr in dim.Attributes)
                {
                    if (connectionID > -1)
                    {
                        string query; 
                        int sourceTableID = GetSourceIDForBinding(connectionID, dsvTableNameToIdMap, attr.NameColumn.Source, out query);

                        if (sourceTableID > 0)
                        {
                            // since we track only table level dependencies and not at the column level,
                            // we have to create a distinct list of these source tables
                            sourceTableIDs[sourceTableID] = true;
                        }
                        else
                        {
                            // if we don't have a table ID but have a query, store that as an attribute
                            // so that users can at least take a look at the query itself.
                            if (query != null)
                            {
                                repository.AddAttribute(dimID, Repository.Attributes.QueryDefinition, query);
                            }
                        }
                    }
                }

                // now create the lineage map from the distinct list of source tables to the dimension 
                foreach(int sourceTableID in sourceTableIDs.Keys)
                {
                    repository.AddMapping(sourceTableID, dimID);
                }
            }
        }

        /// <summary>
        ///  returns information about the binding for table/dsvTable or ColumnBinding, we return the table ID
        ///  for query binding, the returned value is -1, but the query out parameter is set to the query definition
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="dsvTableNameToIdMap"></param>
        /// <param name="binding"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        private int GetSourceIDForBinding(int connectionID, Dictionary<string, int> dsvTableNameToIdMap, Binding binding, out string query)
        {
            int sourceID = -1;
            query = null;

            if (binding != null)
            {
                if (binding is ColumnBinding)
                {
                    ColumnBinding colBinding = (ColumnBinding)binding;

                    // get the id of the dsv table colBinding.TableID
                    if (dsvTableNameToIdMap.ContainsKey(colBinding.TableID))
                    {
                        sourceID = dsvTableNameToIdMap[colBinding.TableID];
                    }
                }
                else
                {
                    if (binding is DsvTableBinding)
                    {
                        DsvTableBinding dsvTableBinding = (DsvTableBinding)binding;

                        if (dsvTableNameToIdMap.ContainsKey(dsvTableBinding.TableID))
                        {
                            sourceID = dsvTableNameToIdMap[dsvTableBinding.TableID];
                        }
                    }
                    else
                    {
                        if (binding is TableBinding)
                        {
                            TableBinding tableBinding = (TableBinding)binding;

                            string tableName = GetFullyQualifiedTableName(tableBinding.DbSchemaName, tableBinding.DbTableName);

                            // get the table name from the repository itself
                            sourceID = repository.GetTable(connectionID, tableName);

                            if (sourceID == -1)
                            {
                                sourceID = repository.AddObject(tableName, string.Empty, RelationalEnumerator.ObjectTypes.Table, connectionID);
                            }
                        }
                        else
                        {
                            if (binding is MeasureGroupDimensionBinding)
                            {
                                // the caller will handle this since we need the list of cb dimensions
                            }
                            else
                            {
                                if (binding is QueryBinding)
                                {
                                    QueryBinding queryBinding = (QueryBinding)binding;

                                    query = queryBinding.QueryDefinition;
                                }
                            }
                        }
                    }
                }
            }

            return sourceID;
        }

        private void EnumerateCubes(Database db, int dbID, Dictionary<DataSourceView, DsvRepositoryInformation> dsvToRepositoryMap, Dictionary<DataSource, int> dsToIdMap, Dictionary<Dimension, int> dimensionToIDMap)
        {
            Dictionary<string, int> emptyDsvTableToIdMap = new Dictionary<string, int>();

            foreach (Cube cb in db.Cubes)
            {
                int connectionID = dsToIdMap[cb.DataSource];

                int cbID = repository.AddObject(cb.Name, cb.Description, ObjectTypes.Cube, dbID);

                Dictionary<CubeDimension, int> cubeDimensionToIDMap = new Dictionary<CubeDimension, int>();

                foreach (CubeDimension cbDim in cb.Dimensions)
                {
                    int cbDimID = repository.AddObject(cbDim.Name, cbDim.Description, ObjectTypes.CubeDimension, cbID);

                    // add Use dependency between the cube dimension and the database dimension
                    repository.AddUseDependency(dimensionToIDMap[cbDim.Dimension], cbDimID);

                    cubeDimensionToIDMap.Add(cbDim, cbDimID);
                }

                EnumerateMeasureGroups(dsvToRepositoryMap, emptyDsvTableToIdMap, cb, connectionID, cbID, cubeDimensionToIDMap);
            }
        }

        /// <summary>
        /// enumerates measures groups, mg dimensions and partitions.
        /// </summary>
        /// <param name="dsvToRepositoryMap"></param>
        /// <param name="emptyDsvTableToIdMap"></param>
        /// <param name="cb"></param>
        /// <param name="connectionID"></param>
        /// <param name="cbID"></param>
        /// <param name="cubeDimensionToIDMap"></param>
        private void EnumerateMeasureGroups(Dictionary<DataSourceView, DsvRepositoryInformation> dsvToRepositoryMap, Dictionary<string, int> emptyDsvTableToIdMap, Cube cb, int connectionID, int cbID, Dictionary<CubeDimension, int> cubeDimensionToIDMap)
        {
            // measure group
            foreach (MeasureGroup mg in cb.MeasureGroups)
            {
                int mgID = repository.AddObject(mg.Name, mg.Description, ObjectTypes.MeasureGroup, cbID);

                // partition
                foreach (Partition pt in mg.Partitions)
                {
                    int ptID = repository.AddObject(pt.Name, pt.Description, ObjectTypes.Partition, mgID);
                    Dictionary<string, int> dsvTableToIdMap = emptyDsvTableToIdMap;

                    if ((pt.DataSourceView != null) && (dsvToRepositoryMap.ContainsKey(pt.DataSourceView)))
                    {
                        dsvTableToIdMap = dsvToRepositoryMap[pt.DataSourceView].dsvTableNameToIDMap;
                    }

                    // add the mapping

                    string query;
                    int sourceTableID = GetSourceIDForBinding(connectionID, dsvTableToIdMap, pt.Source, out query);

                    if (sourceTableID > 0)
                    {
                        repository.AddMapping(sourceTableID, ptID);
                    }
                    else
                    {
                        // if we don't have a table ID but have a query, store that as an attribute
                        // so that users can at least take a look at the query itself.

                        if (query != null)
                        {
                            repository.AddAttribute(ptID, Repository.Attributes.QueryDefinition, query);
                        }
                    }
                }

                EnumerateMeasureGroupDimensions(dsvToRepositoryMap, connectionID, cubeDimensionToIDMap, mg, mgID);
            }
        }

        private void EnumerateMeasureGroupDimensions(Dictionary<DataSourceView, DsvRepositoryInformation> dsvToRepositoryMap, int connectionID, Dictionary<CubeDimension, int> cubeDimensionToIDMap, MeasureGroup mg, int mgID)
        {
            foreach (MeasureGroupDimension mgDim in mg.Dimensions)
            {
                if (mgDim.Dimension != null)
                {
                    Dictionary<string, int> dsvTableNameToIdMap = dsvToRepositoryMap[mgDim.Dimension.DataSourceView].dsvTableNameToIDMap;
                    Dictionary<int, bool> distinctTableIDsList = new Dictionary<int, bool>();

                    int mgDimID = repository.AddObject(mgDim.Dimension.Name, mgDim.Dimension.Description, ObjectTypes.MeasureGroupDimension, mgID);

                    // add a Use dependency from cubeDim->mgdim
                    repository.AddUseDependency(cubeDimensionToIDMap[mgDim.CubeDimension], mgDimID);

                    // this handles ReferenceMeasureGroupDimension and DegenerateMeasureGroupDimension as well.
                    RegularMeasureGroupDimension rmgdim = mgDim as RegularMeasureGroupDimension;

                    if (rmgdim != null)
                    {
                        foreach (MeasureGroupAttribute mgDimAttr in rmgdim.Attributes)
                        {
                            foreach (DataItem keyColumn in mgDimAttr.KeyColumns)
                            {
                                string query;

                                int sourceTableID = GetSourceIDForBinding(connectionID, dsvTableNameToIdMap, keyColumn.Source, out query);

                                if (sourceTableID > 0)
                                {
                                    distinctTableIDsList[sourceTableID] = true;
                                }
                                else
                                {
                                    // if we don't have a table ID but have a query, store that as an attribute
                                    // so that users can at least take a look at the query itself.
                                    if (query != null)
                                    {
                                        repository.AddAttribute(mgDimID, Repository.Attributes.QueryDefinition, query);
                                    }
                                }
                            }
                        }
                    }

                    // add a lineage map from the tables to the measure group dim
                    foreach (int tableID in distinctTableIDsList.Keys)
                    {
                        repository.AddMapping(tableID, mgDimID);
                    }
                }
            }
        }


        private string GetFullyQualifiedTableName(DataTable dataTable)
        {
            return GetFullyQualifiedTableName(GetSchemaName(dataTable), GetTableName(dataTable));
        }

        private string GetFullyQualifiedTableName(string schemaName, string tableName)
        {
            return EncodeTableFullName(schemaName, tableName, QuotingCharacters.OpenQuote, QuotingCharacters.CloseQuote);
        }
        
        /// <summary>
        /// Helper function encode full table name
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="objName"></param>
        /// <param name="openQuote"></param>
        /// <param name="closeQuote"></param>
        /// <returns></returns>
        private string EncodeTableFullName(string schemaName, string objName, string openQuote, string closeQuote)
        {
            if (schemaName == null || schemaName.Length == 0)
                return EncodeObjectName(objName, openQuote, closeQuote);
            else
                return (String.Format("{0}.{1}", EncodeObjectName(schemaName, openQuote, closeQuote),
                    EncodeObjectName(objName, openQuote, closeQuote)));
        }

        /// <summary>
        /// Encode Object Name
        /// </summary>
        /// <param name="objectName">Object Name</param>
        /// <returns>Encoded Object Name</returns>
        public static string EncodeObjectName(string objectName, string openQuote, string closeQuote)
        {
            Debug.Assert(openQuote != null, "Undefined open quote.");
            Debug.Assert(closeQuote != null, "Undefined close quote.");

            if (objectName == null)
                return "";

            if (objectName.Trim().Length == 0)
                return "";

            if (openQuote == null || closeQuote == null)
                return objectName;

            string newName = objectName.Replace(closeQuote, closeQuote + closeQuote);
            return (String.Format("{0}{1}{2}", openQuote, newName, closeQuote));
        }

        /// <summary>
        /// Gets the Schema name for the given dataTable.
        /// </summary>
        /// <param name="dataTable">Data Table</param>
        /// <returns>Schema Name</returns>
        private string GetSchemaName(DataTable dataTable)
        {
            return GetExtendedProperty(dataTable, ExtendedProperties.DBSchemaName);
        }

        /// <summary>
        /// Gets the Table name for the given dataTable.
        /// </summary>
        /// <param name="dataTable">Data Table</param>
        /// <returns>Table Name</returns>
        private string GetTableName(DataTable dataTable)
        {

            string tableName = GetExtendedProperty(dataTable, ExtendedProperties.DBTableName);
            if (tableName == null || tableName.Length == 0)
                return dataTable.TableName;

            return tableName;

        }

        /// <summary>
        /// Get Extended Property from Object
        /// </summary>
        /// <param name="obj">Data Object</param>
        /// <param name="propName">Property Name</param>
        /// <returns>Property Value</returns>
        public static string GetExtendedProperty(object obj, string propName)
        {
            if (obj is System.Data.DataSet)
            {
                System.Data.DataSet dataSet = obj as System.Data.DataSet;
                if (dataSet.ExtendedProperties.Contains(propName))
                    return (string)dataSet.ExtendedProperties[propName];

                return null;
            }

            if (obj is System.Data.DataTable)
            {
                System.Data.DataTable dataTable = obj as System.Data.DataTable;
                if (dataTable.ExtendedProperties.Contains(propName))
                    return (string)dataTable.ExtendedProperties[propName];

                return null;
            }

            if (obj is System.Data.DataColumn)
            {
                System.Data.DataColumn dataColumn = obj as System.Data.DataColumn;
                if (dataColumn.ExtendedProperties.Contains(propName))
                    return (string)dataColumn.ExtendedProperties[propName];

                return null;
            }

            if (obj is System.Data.DataRelation)
            {
                System.Data.DataRelation dataRelation = obj as System.Data.DataRelation;
                if (dataRelation.ExtendedProperties.Contains(propName))
                    return (string)dataRelation.ExtendedProperties[propName];

                return null;
            }

            if (obj is System.Data.UniqueConstraint)
            {
                System.Data.UniqueConstraint uc = obj as System.Data.UniqueConstraint;
                if (uc.ExtendedProperties.Contains(propName))
                    return (string)uc.ExtendedProperties[propName];

                return null;
            }

            if (obj is System.Data.ForeignKeyConstraint)
            {
                System.Data.ForeignKeyConstraint fkc = obj as System.Data.ForeignKeyConstraint;
                if (fkc.ExtendedProperties.Contains(propName))
                    return (string)fkc.ExtendedProperties[propName];

                return null;
            }
            return null;

        }
    
    }

    /// <summary>
    /// Serves as an enumeration of Extended property names 
    /// </summary>
    public class ExtendedProperties
    {
        private ExtendedProperties() { }

        public const string DataSourceID = "DataSourceID";
        public const string DBTableName = "DbTableName";
        public const string DBSchemaName = "DbSchemaName";
        public const string DBColumnName = "DbColumnName";
        public const string FriendlyName = "FriendlyName";
        public const string Description = "Description";
        public const string BatchID = "BatchID";
        public const string IsError = "IsError";
        public const string IsLogical = "IsLogical";
        public const string TableType = "TableType";
        public const string QueryDefinition = "QueryDefinition";
        public const string QueryBuilder = "QueryBuilder";
        public const string GenericQueryBuilderValue = "GenericQueryBuilder";
        public const string SpecificQueryBuilderValue = "SpecificQueryBuilder";
        public const string ComputedColumnExpression = "ComputedColumnExpression";
        public const string DataSize = "DataSize";
        public const string NewName = "NewName";
        public const string Regenerating = "Regenerating";
        public const string AllowGen = "AllowGen";
        public const string Project = "Project";
        public const string DSVTable = "DSVTable";
        public const string DSVColumn = "DSVColumn";
        public const string DSVRelation = "DSVRelation";
        public const string TrueValue = "True";
        public const string FalseValue = "False";
        public const string ViewTypeValue = "View";
        public const string TableTypeValue = "Table";
    }		

}
