///
/// Microsoft SQL Server 2005 Business Intelligence Metadata Reporting Samples
/// Dependency Analyzer Sample
/// 
/// Copyright (c) Microsoft Corporation.  All rights reserved.
///
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Diagnostics;

namespace Microsoft.Samples.DependencyAnalyzer
{
    class Repository : IDisposable
    {

#if SQL2005
        public const string OLEDBGuid = "{9B5D63AB-A629-4A56-9F3E-B1044134B649}";
#endif
#if SQL2008
        public const string OLEDBGuid = "{3BA51769-6C3C-46B2-85A1-81E58DB7DAE1}";
#endif
#if SQL2012
        public const string OLEDBGuid = "{3269FBD7-897B-4CDF-8988-2E1A24B10FBB}";
#endif
#if SQL2014
        public const string OLEDBGuid = "{F3F3005C-C3CB-4C61-B2A9-056035E4D8F2}";
#endif
#if SQL2016
        public const string OLEDBGuid = "{96A2155A-6C39-4F46-B5A4-EC0B63FA0655}";
#endif
#if SQL2017
        public const string OLEDBGuid = "{5802D1B1-DCFC-4F1E-8ACD-388327A21A9C}";
#endif


        internal class Domains
        {
            internal const string SSIS = "SSIS";
            internal const string SSAS = "SSAS";
            internal const string SSRS = "SSRS";
            internal const string Relational = "RDBMS";
            internal const string File = "FILE";
            internal const string Other = "Other";
            internal const string Column = "COLUMN";
        }

        /// <summary>
        /// Stores the current Database Version
        /// </summary>
        const int _dbVersion = 9;

        /// <summary>
        ///  repository tables
        /// </summary>
        DataTable objectTable = new DataTable("Objects");
        DataTable objectDependenciesTable = new DataTable("ObjectDependencies");
        DataTable objectAttributesTable = new DataTable("ObjectAttributes");
        DataTable objectTypesTable = new DataTable("ObjectTypes");
        DataTable runScanTable = new DataTable("RunScan");

        internal class DependencyTypes
        {
            internal const string Containment = "Containment";
            internal const string Lineage = "Map";
            internal const string Use = "Use";
        }

        internal class ConnectionStringProperties
        {
            internal const string DataSource = "Data Source";
            internal const string Server = "Server";
            internal const string Location = "Location";
            internal const string Provider = "Provider";
            internal const string Database = "Database";
            internal const string InitialCatalog = "Initial Catalog";
        }

        internal class Attributes
        {
            internal const string ConnectionString = "ConnectionString";
            internal const string ConnectionServer = "Server";
            internal const string ConnectionDatabase = "Database";
            internal const string QueryDefinition = "QueryDefinition";
        }

        private SqlConnection repositoryConnection;

        /// <summary>
        ///  the root of the containments
        /// </summary>
        private int rootRepositoryObjectID = 0;

        public int RootRepositoryObjectID
        {
            get
            {
                return rootRepositoryObjectID;
            }
            set
            {
                rootRepositoryObjectID = value;
            }
        }


        public Repository(string connectionString)
        {
            repositoryConnection = new SqlConnection();
            repositoryConnection.ConnectionString = connectionString;
            databasePrefixExclusions = new List<string>();
        }

        private int runKeyValue = 0;
        public int RunKeyValue
        {
            get
            {
                return this.runKeyValue;
            }
            set
            {
                this.runKeyValue = value;
            }
        }

        private bool databaseNameOnlyCompare = false;
        public bool DatabaseNameOnlyCompare
        {
            get
            {
                return databaseNameOnlyCompare;
            }
            set
            {
                databaseNameOnlyCompare = value;
            }
        }

        private List<string> databasePrefixExclusions;
        public List<string> DatabasePrefixExclusions
        {
            get
            {
                return databasePrefixExclusions;
            }
        }

        public void Open()
        {
            repositoryConnection.Open();
            // TODO:
            // Reset the rootRepositoryObjectID to the maximum ObjectID + 1 (if > 0 records)...

            DataColumn column = objectTable.Columns.Add("RunKey");
            column.DataType = typeof(int);
            column = objectTable.Columns.Add("ObjectKey");
            column.AutoIncrement = true;
            column.AutoIncrementSeed = 0;  // ToDo: reset this to correct value...
            column.DataType = typeof(int);
            objectTable.Columns.Add("ObjectName");
            objectTable.Columns.Add("ObjectTypeString");
            objectTable.Columns.Add("ObjectDesc");

            column = objectDependenciesTable.Columns.Add("RunKey");
            column.DataType = typeof(int);
            column = objectDependenciesTable.Columns.Add("SrcObjectKey");
            column.DataType = typeof(int);
            column = objectDependenciesTable.Columns.Add("TgtObjectKey");
            column.DataType = typeof(int);
            column = objectDependenciesTable.Columns.Add("DependencyType");

            column = objectAttributesTable.Columns.Add("RunKey");
            column.DataType = typeof(int);
            column = objectAttributesTable.Columns.Add("ObjectKey");
            column.DataType = typeof(int);
            objectAttributesTable.Columns.Add("ObjectAttrName");
            objectAttributesTable.Columns.Add("ObjectAttrValue");

            objectTypesTable.Columns.Add("ObjectTypeID");
            objectTypesTable.Columns.Add("ObjectTypeName");
            objectTypesTable.Columns.Add("ObjectTypeDesc");
            objectTypesTable.Columns.Add("ObjectMetaType"); // todo: populate this column
            objectTypesTable.Columns.Add("Domain");

            column = runScanTable.Columns.Add("RunKey");
            column.AutoIncrement = true;
            column.AutoIncrementSeed = 0;  // ToDo: reset this to correct value...
            column.DataType = typeof(int);
            column = runScanTable.Columns.Add("RunDate");
            column.DataType = typeof(DateTime);
            column = runScanTable.Columns.Add("RunCommand");
        }

        public void Close()
        {
            repositoryConnection.Close();
        }

        public void Dispose()
        {
            repositoryConnection.Dispose();
        }

        public bool IsValidRepository()
        {
            DataTable schemaInfo = repositoryConnection.GetSchema("Tables");
            DataRow[] rows = schemaInfo.Select(string.Format("TABLE_NAME = 'RunScan'"));
            if (rows.Length == 0)
            {
                return false;
            }
            rows = schemaInfo.Select(string.Format("TABLE_NAME = 'LookupConnectionID'"));
            if (rows.Length == 0)
            {
                return false;
            }
            rows = schemaInfo.Select(string.Format("TABLE_NAME = 'Version'"));
            if (rows.Length == 0)
            {
                return false;
            }
            rows = schemaInfo.Select(string.Format("TABLE_NAME = 'ObjectTypes'"));
            if (rows.Length == 0)
            {
                return false;
            }
            rows = schemaInfo.Select(string.Format("TABLE_NAME = 'Objects'"));
            if (rows.Length == 0)
            {
                return false;
            }
            rows = schemaInfo.Select(string.Format("TABLE_NAME = 'ObjectDependencies'"));
            if (rows.Length == 0)
            {
                return false;
            }
            rows = schemaInfo.Select(string.Format("TABLE_NAME = 'ObjectAttributes'"));
            if (rows.Length == 0)
            {
                return false;
            }
            using (SqlCommand sqlCommand = new SqlCommand("SELECT MAX(VersionID) FROM [dbo].[Version]", repositoryConnection))
            {
                using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
                {
                    sqlReader.Read();
                    if (System.Convert.ToInt32(sqlReader[0]) != _dbVersion)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Commits new records to the repository.
        /// Does not handle records that have been updated after initial commit.
        /// </summary>
        public void Commit()
        {
            Console.WriteLine("Performing Incremental Commit");
            // write each data table to the database
            SqlBulkCopy bulkCopy = new SqlBulkCopy(repositoryConnection);

            bulkCopy.BulkCopyTimeout = 0;  // Set the bulk copy time to infinite.  We really don't want this to time out after 60 seconds which is the default.
            bulkCopy.DestinationTableName = "RunScan";
            bulkCopy.WriteToServer(runScanTable, DataRowState.Added);
            using (SqlCommand runScanUpdate = new SqlCommand("UPDATE [dbo].[RunScan] SET RunDate = @RunDate, RunCommand = @RunCommand WHERE RunKey = @RunKey", repositoryConnection))
            {
                SqlParameter pRunDate = new SqlParameter("@RunDate", SqlDbType.DateTime);
                runScanUpdate.Parameters.Add(pRunDate);
                SqlParameter pRunCommand = new SqlParameter("@RunCommand", SqlDbType.NVarChar);
                runScanUpdate.Parameters.Add(pRunCommand);
                SqlParameter pScanRunKey = new SqlParameter("@RunKey", SqlDbType.Int);
                runScanUpdate.Parameters.Add(pScanRunKey);

                foreach (DataRow row in runScanTable.Rows)
                {
                    if (row.RowState == DataRowState.Modified)
                    {
                        // This record has been changed, and wont be commited without something happening here.
                        pRunDate.Value = row["RunDate"];
                        pRunCommand.Value = row["RunCommand"];
                        pScanRunKey.Value = row["RunKey"];
                        runScanUpdate.ExecuteNonQuery();
                    }
                }
            }
            runScanTable.AcceptChanges();

            bulkCopy.DestinationTableName = "Objects";
            bulkCopy.WriteToServer(objectTable, DataRowState.Added);
            using (SqlCommand objectsUpdate = new SqlCommand("UPDATE [dbo].[Objects] SET [ObjectName] = @ObjectName, [ObjectTypeString] = @ObjectTypeString, [ObjectDesc] = @ObjectDesc WHERE [RunKey] = @RunKey AND [ObjectKey] = @ObjectKey", repositoryConnection))
            {
                SqlParameter pObjectName = new SqlParameter("@ObjectName", SqlDbType.NVarChar);
                objectsUpdate.Parameters.Add(pObjectName);
                SqlParameter pObjectTypeString = new SqlParameter("@ObjectTypeString", SqlDbType.NVarChar);
                objectsUpdate.Parameters.Add(pObjectTypeString);
                SqlParameter pObjectDesc = new SqlParameter("@ObjectDesc", SqlDbType.NVarChar);
                objectsUpdate.Parameters.Add(pObjectDesc);
                SqlParameter pObjectRunKey = new SqlParameter("@RunKey", SqlDbType.Int);
                objectsUpdate.Parameters.Add(pObjectRunKey);
                SqlParameter pObjectKey = new SqlParameter("@ObjectKey", SqlDbType.NVarChar);
                objectsUpdate.Parameters.Add(pObjectKey);
                foreach (DataRow row in objectTable.Rows)
                {
                    if (row.RowState == DataRowState.Modified)
                    {
                        // This record has been changed, and wont be commited without something happening here.
                        pObjectName.Value = row["ObjectName"];
                        pObjectTypeString.Value = row["ObjectTypeString"];
                        pObjectDesc.Value = row["ObjectDesc"];
                        pObjectKey.Value = row["ObjectKey"];
                        pObjectRunKey.Value = row["RunKey"];
                        objectsUpdate.ExecuteNonQuery();
                    }
                }
            }
            objectTable.AcceptChanges();

            bulkCopy.DestinationTableName = "ObjectDependencies";
            bulkCopy.WriteToServer(objectDependenciesTable, DataRowState.Added);
            // This table doesn't support updates :-)
            objectDependenciesTable.AcceptChanges();

            bulkCopy.DestinationTableName = "ObjectAttributes";
            bulkCopy.WriteToServer(objectAttributesTable, DataRowState.Added);
            using (SqlCommand attributesUpdate = new SqlCommand("UPDATE [dbo].[ObjectAttributes] SET [ObjectAttrName] = @ObjectAttrName, [ObjectAttrValue] = @ObjectAttrValue WHERE [RunKey] = @RunKey AND [ObjectKey] = @ObjectKey", repositoryConnection))
            {
                SqlParameter pObjectAttrName = new SqlParameter("@ObjectAttrName", SqlDbType.NVarChar);
                attributesUpdate.Parameters.Add(pObjectAttrName);
                SqlParameter pObjectAttrValue = new SqlParameter("@ObjectAttrValue", SqlDbType.NVarChar);
                attributesUpdate.Parameters.Add(pObjectAttrValue);
                SqlParameter pObjectRunKey = new SqlParameter("@RunKey", SqlDbType.Int);
                attributesUpdate.Parameters.Add(pObjectRunKey);
                SqlParameter pObjectKey = new SqlParameter("@ObjectKey", SqlDbType.NVarChar);
                attributesUpdate.Parameters.Add(pObjectKey);
                foreach (DataRow row in objectAttributesTable.Rows)
                {
                    if (row.RowState == DataRowState.Modified)
                    {
                        // This record has been changed, and wont be commited without something happening here.
                        pObjectAttrName.Value = row["ObjectAttrName"];
                        pObjectAttrValue.Value = row["ObjectAttrValue"];
                        pObjectKey.Value = row["ObjectKey"];
                        pObjectRunKey.Value = row["RunKey"];
                        attributesUpdate.ExecuteNonQuery();
                    }
                }
            }
            objectAttributesTable.AcceptChanges();

            bulkCopy.DestinationTableName = "ObjectTypes";
            bulkCopy.WriteToServer(objectTypesTable, DataRowState.Added);
            objectTypesTable.AcceptChanges();
            // This table doesn't support updates, only adds :-)

            bulkCopy.Close();
        }

        public void CreateRepository()
        {
            int dbVersion = 0;
            int sqlVersion = 9;

            DataTable schemaInfo = repositoryConnection.GetSchema("Tables");
            DataRow[] rows = schemaInfo.Select(string.Format("TABLE_NAME = 'Version'"));
            if (rows.Length != 0)
            {
                using (SqlCommand sqlCommand = new SqlCommand("SELECT MAX(VersionID) FROM [dbo].[Version]", repositoryConnection))
                {
                    using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
                    {
                        sqlReader.Read();
                        dbVersion = System.Convert.ToInt32(sqlReader[0]);
                    }
                }
            }
            using (SqlCommand sqlCommand = new SqlCommand("select left(cast(serverproperty('productversion') as nvarchar(128)), charindex(cast(serverproperty('productversion') as nvarchar(128)), '.') + 1)", repositoryConnection))
            {
                using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
                {
                    sqlReader.Read();
                    sqlVersion = System.Convert.ToInt32(sqlReader[0]);
                }
            }

            if (dbVersion == 0)
            {
#region dbVersion 0
                // The following database create will create a Version 4 database.
                // If the database is prior to Version 4 (No Version Table) then it will be dropped.
                // This is OK, as the previous version didn't support history!
                using (SqlCommand sqlCommand = repositoryConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "SET ANSI_NULLS ON";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "SET QUOTED_IDENTIFIER ON";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Version]') AND type in (N'U'))\r\n" +
                                            "BEGIN\r\n" +
                                            "	IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectAttributes_Objects]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectAttributes]'))\r\n" +
                                            "	ALTER TABLE [dbo].[ObjectAttributes] DROP CONSTRAINT [FK_ObjectAttributes_Objects]\r\n" +
                                            "	IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectDependencies_Objects]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectDependencies]'))\r\n" +
                                            "	ALTER TABLE [dbo].[ObjectDependencies] DROP CONSTRAINT [FK_ObjectDependencies_Objects]\r\n" +
                                            "	IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectDependencies_Objects1]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectDependencies]'))\r\n" +
                                            "	ALTER TABLE [dbo].[ObjectDependencies] DROP CONSTRAINT [FK_ObjectDependencies_Objects1]\r\n" +
                                            "	IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Objects_RunScan]') AND parent_object_id = OBJECT_ID(N'[dbo].[Objects]'))\r\n" +
                                            "	ALTER TABLE [dbo].[Objects] DROP CONSTRAINT [FK_Objects_RunScan]\r\n" +
                                            "	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ObjectAttributes]') AND type in (N'U'))\r\n" +
                                            "	DROP TABLE [dbo].[ObjectAttributes]\r\n" +
                                            "	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ObjectDependencies]') AND type in (N'U'))\r\n" +
                                            "	DROP TABLE [dbo].[ObjectDependencies]\r\n" +
                                            "	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Objects]') AND type in (N'U'))\r\n" +
                                            "	DROP TABLE [dbo].[Objects]\r\n" +
                                            "	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ObjectTypes]') AND type in (N'U'))\r\n" +
                                            "\r\n" +
                                            "DROP TABLE [dbo].[ObjectTypes]\r\n" +
                                            "	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RunScan]') AND type in (N'U'))\r\n" +
                                            "	DROP TABLE [dbo].[RunScan]\r\n" +
                                            "	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Audit]') AND type in (N'U'))\r\n" +
                                            "	DROP TABLE [dbo].[Audit]\r\n" +
                                            "	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LookupConnectionID]') AND type in (N'U'))\r\n" +
                                            "	DROP TABLE [dbo].[LookupConnectionID]\r\n" +
                                            "END";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LookupConnectionID]') AND type in (N'U'))\r\n" +
                                            "BEGIN\r\n" +
                                            "CREATE TABLE [dbo].[LookupConnectionID](\r\n" +
                                            "	[ConnectionGUID] [nvarchar](1000) NOT NULL,\r\n" +
                                            "	[ConnectionDescription] [nvarchar](1000) NOT NULL\r\n" +
                                            ") ON [PRIMARY]\r\n" +
                                            "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{5F2826BC-648B-4f3e-B930-587F4EF331D4}', N'ODBC 2005')\r\n" +
                                            "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{9B5D63AB-A629-4A56-9F3E-B1044134B649}', N'OLEDB 2005')\r\n" +
                                            "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{72692A11-F5CC-42b8-869D-84E7C8E48B14}', N'ADO.NET 2005')\r\n" +
                                            "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{4CF60474-BA87-4ac2-B9F3-B7B9179D4183}', N'ADO 2005')\r\n" +
                                            "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'RelationalDataSource', N'olap relational data source')\r\n" +
                                            "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{09AD884B-0248-42C1-90E6-897D1CD16D37}', N'ODBC 2008')\r\n" +
                                            "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{3BA51769-6C3C-46B2-85A1-81E58DB7DAE1}', N'OLEDB 2008')\r\n" +
                                            "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{A1100566-934E-470C-9ECE-0D5EB920947D}', N'ADO 2008')\r\n" +
                                            "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{894CAE21-539F-46EB-B36D-9381163B6C4E}', N'ADO.Net 2008')\r\n" +
                                            "END\r\n";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Audit]') AND type in (N'U'))\r\n" +
                                            "BEGIN\r\n" +
                                            "CREATE TABLE [dbo].[Audit](\r\n" +
                                            "	[PackageGUID] [varchar](50) NOT NULL,\r\n" +
                                            "	[DataFlowTaskID] [int] NOT NULL,\r\n" +
                                            "	[SourceReadRows] [int] NULL,\r\n" +
                                            "	[SourceReadErrorRows] [int] NULL,\r\n" +
                                            "	[CleansedRows] [int] NULL,\r\n" +
                                            "	[TargetWriteRows] [int] NULL,\r\n" +
                                            "	[TargetWriteErrorRows] [int] NULL,\r\n" +
                                            "	[Comment] [nvarchar](255) NULL\r\n" +
                                            ") ON [PRIMARY]\r\n" +
                                            "END\r\n";
                    sqlCommand.ExecuteNonQuery();
                    if (sqlVersion > 9)
                    {
                        sqlCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Version]') AND type in (N'U'))\r\n" +
                                                "BEGIN\r\n" +
                                                "CREATE TABLE [dbo].[Version](\r\n" +
                                                "	[VersionID] [int] NOT NULL,\r\n" +
                                                "	[InstallDate] [date] NOT NULL,\r\n" +
                                                " CONSTRAINT [PK_Version] PRIMARY KEY CLUSTERED \r\n" +
                                                "(\r\n" +
                                                "	[VersionID] ASC\r\n" +
                                                ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]\r\n" +
                                                ") ON [PRIMARY]\r\n" +
                                                "END\r\n";
                        sqlCommand.ExecuteNonQuery();
                        sqlCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RunScan]') AND type in (N'U'))\r\n" +
                                                "BEGIN\r\n" +
                                                "CREATE TABLE [dbo].[RunScan](\r\n" +
                                                "	[RunKey] [int] NOT NULL,\r\n" +
                                                "	[RunDate] [datetime2](7) NOT NULL,\r\n" +
                                                "	[RunCommand] [nvarchar](512) NOT NULL,\r\n" +
                                                " CONSTRAINT [PK_RunScan] PRIMARY KEY CLUSTERED \r\n" +
                                                "(\r\n" +
                                                "	[RunKey] ASC\r\n" +
                                                ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]\r\n" +
                                                ") ON [PRIMARY]\r\n" +
                                                "END\r\n" +
                                                "IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'RunScan', NULL,NULL))\r\n" +
                                                "EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Stores a row for each execution of the DependancyAnalyzer program' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RunScan'\r\n" +
                                                "";
                    }
                    else
                    {
                        sqlCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Version]') AND type in (N'U'))\r\n" +
                                                "BEGIN\r\n" +
                                                "CREATE TABLE [dbo].[Version](\r\n" +
                                                "	[VersionID] [int] NOT NULL,\r\n" +
                                                "	[InstallDate] [datetime] NOT NULL,\r\n" +
                                                " CONSTRAINT [PK_Version] PRIMARY KEY CLUSTERED \r\n" +
                                                "(\r\n" +
                                                "	[VersionID] ASC\r\n" +
                                                ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]\r\n" +
                                                ") ON [PRIMARY]\r\n" +
                                                "END\r\n";
                        sqlCommand.ExecuteNonQuery();
                        sqlCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RunScan]') AND type in (N'U'))\r\n" +
                                                "BEGIN\r\n" +
                                                "CREATE TABLE [dbo].[RunScan](\r\n" +
                                                "	[RunKey] [int] NOT NULL,\r\n" +
                                                "	[RunDate] [datetime] NOT NULL,\r\n" +
                                                "	[RunCommand] [nvarchar](512) NOT NULL,\r\n" +
                                                " CONSTRAINT [PK_RunScan] PRIMARY KEY CLUSTERED \r\n" +
                                                "(\r\n" +
                                                "	[RunKey] ASC\r\n" +
                                                ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]\r\n" +
                                                ") ON [PRIMARY]\r\n" +
                                                "END\r\n" +
                                                "IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'RunScan', NULL,NULL))\r\n" +
                                                "EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Stores a row for each execution of the DependancyAnalyzer program' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RunScan'\r\n" +
                                                "";
                    }
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ObjectTypes]') AND type in (N'U'))\r\n" +
                                            "BEGIN\r\n" +
                                            "CREATE TABLE [dbo].[ObjectTypes](\r\n" +
                                            "	[ObjectTypeKey] [nvarchar](255) NOT NULL,\r\n" +
                                            "	[ObjectTypeName] [nvarchar](255) NULL,\r\n" +
                                            "	[ObjectTypeDesc] [nvarchar](2000) NULL,\r\n" +
                                            "	[ObjectMetaType] [nvarchar](255) NULL,\r\n" +
                                            "	[Domain] [nvarchar](50) NULL\r\n" +
                                            ") ON [PRIMARY]\r\n" +
                                            "END\r\n" +
                                            "";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Objects]') AND type in (N'U'))\r\n" +
                                            "BEGIN\r\n" +
                                            "CREATE TABLE [dbo].[Objects](\r\n" +
                                            "	[RunKey] [int] NOT NULL,\r\n" +
                                            "	[ObjectKey] [int] NOT NULL,\r\n" +
                                            "	[ObjectName] [nvarchar](1000) NULL,\r\n" +
                                            "	[ObjectTypeString] [nvarchar](1000) NOT NULL,\r\n" +
                                            "	[ObjectDesc] [nvarchar](1000) NULL,\r\n" +
                                            " CONSTRAINT [PK_Objects] PRIMARY KEY CLUSTERED \r\n" +
                                            "(\r\n" +
                                            "	[RunKey] ASC,\r\n" +
                                            "	[ObjectKey] ASC\r\n" +
                                            ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]\r\n" +
                                            ") ON [PRIMARY]\r\n" +
                                            "END\r\n" +
                                            "";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ObjectDependencies]') AND type in (N'U'))\r\n" +
                                            "BEGIN\r\n" +
                                            "CREATE TABLE [dbo].[ObjectDependencies](\r\n" +
                                            "	[RunKey] [int] NOT NULL,\r\n" +
                                            "	[SrcObjectKey] [int] NOT NULL,\r\n" +
                                            "	[TgtObjectKey] [int] NOT NULL,\r\n" +
                                            "	[DependencyType] [nvarchar](50) NOT NULL,\r\n" +
                                            " CONSTRAINT [PK_ObjectDependencies] PRIMARY KEY CLUSTERED \r\n" +
                                            "(\r\n" +
                                            "	[RunKey] ASC,\r\n" +
                                            "	[SrcObjectKey] ASC,\r\n" +
                                            "	[TgtObjectKey] ASC,\r\n" +
                                            "	[DependencyType] ASC\r\n" +
                                            ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]\r\n" +
                                            ") ON [PRIMARY]\r\n" +
                                            "END\r\n" +
                                            "";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ObjectAttributes]') AND type in (N'U'))\r\n" +
                                            "BEGIN\r\n" +
                                            "CREATE TABLE [dbo].[ObjectAttributes](\r\n" +
                                            "	[RunKey] [int] NOT NULL,\r\n" +
                                            "	[ObjectKey] [int] NOT NULL,\r\n" +
                                            "	[ObjectAttrName] [nvarchar](1000) NOT NULL,\r\n" +
                                            "	[ObjectAttrValue] [nvarchar](max) NOT NULL\r\n" +
                                            ") ON [PRIMARY]\r\n" +
                                            "END\r\n" +
                                            "";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectAttributes_Objects]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectAttributes]'))\r\n" +
                                            "ALTER TABLE [dbo].[ObjectAttributes]  WITH CHECK ADD  CONSTRAINT [FK_ObjectAttributes_Objects] FOREIGN KEY([RunKey], [ObjectKey])\r\n" +
                                            "REFERENCES [dbo].[Objects] ([RunKey], [ObjectKey])\r\n" +
                                            "IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectAttributes_Objects]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectAttributes]'))\r\n" +
                                            "ALTER TABLE [dbo].[ObjectAttributes] CHECK CONSTRAINT [FK_ObjectAttributes_Objects]\r\n" +
                                            "IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectDependencies_Objects]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectDependencies]'))\r\n" +
                                            "ALTER TABLE [dbo].[ObjectDependencies]  WITH CHECK ADD  CONSTRAINT [FK_ObjectDependencies_Objects] FOREIGN KEY([RunKey], [SrcObjectKey])\r\n" +
                                            "REFERENCES [dbo].[Objects] ([RunKey], [ObjectKey])\r\n" +
                                            "IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectDependencies_Objects]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectDependencies]'))\r\n" +
                                            "ALTER TABLE [dbo].[ObjectDependencies] CHECK CONSTRAINT [FK_ObjectDependencies_Objects]\r\n" +
                                            "IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectDependencies_Objects1]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectDependencies]'))\r\n" +
                                            "ALTER TABLE [dbo].[ObjectDependencies]  WITH CHECK ADD  CONSTRAINT [FK_ObjectDependencies_Objects1] FOREIGN KEY([RunKey], [TgtObjectKey])\r\n" +
                                            "REFERENCES [dbo].[Objects] ([RunKey], [ObjectKey])\r\n" +
                                            "IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ObjectDependencies_Objects1]') AND parent_object_id = OBJECT_ID(N'[dbo].[ObjectDependencies]'))\r\n" +
                                            "ALTER TABLE [dbo].[ObjectDependencies] CHECK CONSTRAINT [FK_ObjectDependencies_Objects1]\r\n" +
                                            "IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Objects_RunScan]') AND parent_object_id = OBJECT_ID(N'[dbo].[Objects]'))\r\n" +
                                            "ALTER TABLE [dbo].[Objects]  WITH CHECK ADD  CONSTRAINT [FK_Objects_RunScan] FOREIGN KEY([RunKey])\r\n" +
                                            "REFERENCES [dbo].[RunScan] ([RunKey])\r\n" +
                                            "IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Objects_RunScan]') AND parent_object_id = OBJECT_ID(N'[dbo].[Objects]'))\r\n" +
                                            "ALTER TABLE [dbo].[Objects] CHECK CONSTRAINT [FK_Objects_RunScan]\r\n" +
                                            "";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[Connections]'))\r\n" +
                                            "EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[Connections]\r\n" +
                                            "AS SELECT 1 AS Column1' \r\n" +
                                            "IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[SourceTables]'))\r\n" +
                                            "EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[SourceTables]\r\n" +
                                            "AS SELECT 1 AS Column1' \r\n" +
                                            "IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[ObjectRelationships]'))\r\n" +
                                            "EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[ObjectRelationships]\r\n" +
                                            "AS\r\n" +
                                            "SELECT 1 AS Column1' \r\n" +
                                            "IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[LineageMap]'))\r\n" +
                                            "EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[LineageMap]\r\n" +
                                            "AS\r\n" +
                                            "SELECT 1 AS Column1' \r\n" +
                                            "IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[TargetTables]'))\r\n" +
                                            "EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[TargetTables]\r\n" +
                                            "AS\r\n" +
                                            "SELECT 1 AS Column1' \r\n" +
                                            "IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[DataFlows]'))\r\n" +
                                            "EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[DataFlows]\r\n" +
                                            "AS\r\n" +
                                            "SELECT 1 AS Column1' \r\n" +
                                            "IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[WalkSources]'))\r\n" +
                                            "EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[WalkSources]\r\n" +
                                            "AS\r\n" +
                                            "SELECT 1 AS Column1' \r\n" +
                                            "IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[Packages]'))\r\n" +
                                            "EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[Packages]\r\n" +
                                            "AS\r\n" +
                                            "SELECT 1 AS Column1' \r\n" +
                                            "IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vAudit]'))\r\n" +
                                            "EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vAudit]\r\n" +
                                            "AS\r\n" +
                                            "SELECT 1 AS Column1' \r\n" +
                                            "IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[TableLineageMap]'))\r\n" +
                                            "EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[TableLineageMap]\r\n" +
                                            "AS\r\n" +
                                            "SELECT 1 AS Column1' \r\n" +
                                            "IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[ConnectionsMapping]'))\r\n" +
                                            "EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[ConnectionsMapping]\r\n" +
                                            "AS\r\n" +
                                            "SELECT 1 AS Column1' \r\n";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "ALTER VIEW [dbo].[TargetTables]\r\n" +
                                            "AS\r\n" +
                                            "SELECT\r\n" +
                                            "    Objects.RunKey,\r\n" +
                                            "    ObjectDependencies.DependencyType,\r\n" +
                                            "    Objects.ObjectKey,\r\n" +
                                            "    Objects.ObjectName,\r\n" +
                                            "    Objects.ObjectDesc,\r\n" +
                                            "    ObjectDependencies.SrcObjectKey AS TgtComponentKey,\r\n" +
                                            "    TargetObjects.ObjectName AS TargetComponentName,\r\n" +
                                            "    TargetObjects.ObjectDesc AS TargetComponentDesc,\r\n" +
                                            "    OD_DataFlow.SrcObjectKey AS DataFlowID,\r\n" +
                                            "    OD_DestConnection.SrcObjectKey AS DestinationConnectionID\r\n" +
                                            "FROM dbo.Objects\r\n" +
                                            "INNER JOIN dbo.ObjectDependencies AS ObjectDependencies\r\n" +
                                            "        ON Objects.ObjectKey = ObjectDependencies.TgtObjectKey\r\n" +
                                            "       AND Objects.RunKey = ObjectDependencies.RunKey\r\n" +
                                            "INNER JOIN dbo.Objects AS TargetObjects\r\n" +
                                            "        ON ObjectDependencies.SrcObjectKey = TargetObjects.ObjectKey\r\n" +
                                            "       AND Objects.RunKey = TargetObjects.RunKey\r\n" +
                                            "       AND ObjectDependencies.RunKey = TargetObjects.RunKey\r\n" +
                                            "INNER JOIN dbo.ObjectDependencies AS OD_DataFlow\r\n" +
                                            "        ON ObjectDependencies.SrcObjectKey = OD_DataFlow.TgtObjectKey\r\n" +
                                            "       AND ObjectDependencies.RunKey = OD_DataFlow.RunKey\r\n" +
                                            "INNER JOIN dbo.ObjectDependencies AS OD_DestConnection\r\n" +
                                            "        ON Objects.ObjectKey = OD_DestConnection.TgtObjectKey\r\n" +
                                            "       AND Objects.RunKey = OD_DestConnection.RunKey\r\n" +
                                            "WHERE ObjectDependencies.DependencyType = N'Map'\r\n" +
                                            "  AND Objects.ObjectTypeString = N'Table'\r\n" +
                                            "  AND OD_DataFlow.DependencyType = N'Containment'\r\n" +
                                            "  AND OD_DestConnection.DependencyType = N'Containment'\r\n";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "ALTER VIEW [dbo].[SourceTables]\r\n" +
                                            "AS\r\n" +
                                            "SELECT\r\n" +
                                            "    dbo.Objects.RunKey,\r\n" +
                                            "    dbo.Objects.ObjectKey,\r\n" +
                                            "    dbo.Objects.ObjectName,\r\n" +
                                            "    dbo.Objects.ObjectTypeString,\r\n" +
                                            "    dbo.Objects.ObjectDesc,\r\n" +
                                            "    dbo.ObjectDependencies.TgtObjectKey AS SrcComponentKey,\r\n" +
                                            "    SourceObjects.ObjectName AS SourceObjectsName,\r\n" +
                                            "    SourceObjects.ObjectDesc AS SourceObjectsDesc,\r\n" +
                                            "    OD_DataFlow.SrcObjectKey AS DataFlowID,\r\n" +
                                            "    OD_DestConnection.SrcObjectKey AS SourceConnectionID\r\n" +
                                            "FROM dbo.Objects\r\n" +
                                            "INNER JOIN dbo.ObjectDependencies\r\n" +
                                            "        ON dbo.Objects.ObjectKey = dbo.ObjectDependencies.SrcObjectKey\r\n" +
                                            "        AND dbo.Objects.RunKey = dbo.ObjectDependencies.RunKey\r\n" +
                                            "INNER JOIN dbo.ObjectDependencies AS OD_DataFlow\r\n" +
                                            "        ON dbo.ObjectDependencies.TgtObjectKey = OD_DataFlow.TgtObjectKey\r\n" +
                                            "        AND dbo.ObjectDependencies.RunKey = OD_DataFlow.RunKey\r\n" +
                                            "INNER JOIN dbo.Objects AS SourceObjects\r\n" +
                                            "        ON dbo.ObjectDependencies.TgtObjectKey = SourceObjects.ObjectKey\r\n" +
                                            "        AND dbo.ObjectDependencies.RunKey = SourceObjects.RunKey\r\n" +
                                            "INNER JOIN dbo.ObjectDependencies AS OD_DestConnection\r\n" +
                                            "        ON dbo.Objects.ObjectKey = OD_DestConnection.TgtObjectKey\r\n" +
                                            "        AND dbo.Objects.RunKey = OD_DestConnection.RunKey\r\n" +
                                            "WHERE dbo.ObjectDependencies.DependencyType = N'Map'\r\n" +
                                            "  AND dbo.Objects.ObjectTypeString = N'Table'\r\n" +
                                            "  AND OD_DataFlow.DependencyType = N'Containment'\r\n" +
                                            "  AND OD_DataFlow.DependencyType = OD_DestConnection.DependencyType";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "ALTER VIEW [dbo].[LineageMap]\r\n" +
                                            "AS\r\n" +
                                            "SELECT\r\n" +
                                            "	RunKey,\r\n" +
                                            "	SrcObjectKey,\r\n" +
                                            "	TgtObjectKey\r\n" +
                                            "FROM dbo.ObjectDependencies\r\n" +
                                            "WHERE DependencyType = N'Map'";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "ALTER VIEW [dbo].[WalkSources]\r\n" +
                                            "AS\r\n" +
                                            "WITH f(RunKey, osrc, tgt, lvl, objecttype) \r\n" +
                                            "AS \r\n" +
                                            "(SELECT  Objects.RunKey,    dbo.SourceTables.ObjectKey\r\n" +
                                            "	, dbo.SourceTables.SrcComponentKey\r\n" +
                                            "	, 0 AS Expr1\r\n" +
                                            "	, dbo.Objects.ObjectTypeString\r\n" +
                                            "FROM dbo.SourceTables \r\n" +
                                            "INNER JOIN dbo.Objects \r\n" +
                                            "	ON dbo.SourceTables.ObjectKey = dbo.Objects.ObjectKey\r\n" +
                                            "	AND SourceTables.RunKey = Objects.RunKey\r\n" +
                                            "UNION ALL\r\n" +
                                            "SELECT  Objects_1.RunKey,    f_2.osrc\r\n" +
                                            "	, dbo.LineageMap.TgtObjectKey\r\n" +
                                            "	, f_2.lvl + 1 AS Expr1\r\n" +
                                            "	, Objects_1.ObjectTypeString\r\n" +
                                            "FROM         f AS f_2 \r\n" +
                                            "INNER JOIN dbo.LineageMap \r\n" +
                                            "	ON f_2.tgt = dbo.LineageMap.SrcObjectKey \r\n" +
                                            "INNER JOIN dbo.Objects AS Objects_1 \r\n" +
                                            "	ON dbo.LineageMap.TgtObjectKey = Objects_1.ObjectKey\r\n" +
                                            "	AND LineageMap.RunKey = Objects_1.RunKey\r\n" +
                                            "WHERE     (NOT (f_2.osrc = f_2.tgt)))\r\n" +
                                            "SELECT   RunKey,   osrc, tgt, lvl, objecttype\r\n" +
                                            "FROM         f AS f_1";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "ALTER VIEW [dbo].[ObjectRelationships]\r\n" +
                                            "AS\r\n" +
                                            "SELECT\r\n" +
                                            "    RunKey,\r\n" +
                                            "    SrcObjectKey AS ParentObjectKey,\r\n" +
                                            "    TgtObjectKey AS ChildObjectKey\r\n" +
                                            "FROM dbo.ObjectDependencies\r\n" +
                                            "WHERE DependencyType = N'Containment'";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "ALTER VIEW [dbo].[Packages]\r\n" +
                                            "AS\r\n" +
                                            "SELECT \r\n" +
                                            "	Objects.RunKey,\r\n" +
                                            "	Objects.ObjectKey AS PackageID, \r\n" +
                                            "	Objects.ObjectName AS PackageName,\r\n" +
                                            "	Objects.ObjectDesc AS PackageDesc,\r\n" +
                                            "	PackageProperties.PackageLocation,\r\n" +
                                            "	PackageProperties.PackageGUID\r\n" +
                                            "FROM [dbo].[Objects],\r\n" +
                                            "	(SELECT \r\n" +
                                            "		RunKey,\r\n" +
                                            "		PackageProperties.ObjectKey,\r\n" +
                                            "		[PackageLocation],\r\n" +
                                            "		[PackageGUID]\r\n" +
                                            "	FROM dbo.ObjectAttributes \r\n" +
                                            "	PIVOT (\r\n" +
                                            "		MIN (ObjectAttrValue) \r\n" +
                                            "		FOR ObjectAttrName \r\n" +
                                            "		IN ([PackageLocation], [PackageGUID])\r\n" +
                                            "		) AS PackageProperties\r\n" +
                                            "	) AS PackageProperties\r\n" +
                                            "WHERE [Objects].ObjectKey = PackageProperties.ObjectKey \r\n" +
                                            "AND [Objects].RunKey = PackageProperties.RunKey\r\n" +
                                            "AND [Objects].ObjectTypeString = N'SSIS Package'";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "ALTER VIEW [dbo].[Connections]\r\n" +
                                            "AS\r\n" +
                                            "SELECT \r\n" +
                                            "	[Objects].[RunKey],\r\n" +
                                            "	[Objects].ObjectKey AS ConnectionID,\r\n" +
                                            "	[Objects].ObjectName AS ConnectionName,\r\n" +
                                            "	[Objects].ObjectDesc AS ConnectionDesc,\r\n" +
                                            "	ConnectionString,\r\n" +
                                            "	ConnectionProperties.[Server],\r\n" +
                                            "	ConnectionProperties.[Database]\r\n" +
                                            "FROM [dbo].[Objects] \r\n" +
                                            "INNER JOIN\r\n" +
                                            "	(SELECT\r\n" +
                                            "		RunKey,\r\n" +
                                            "		ConnectionProperties.ObjectKey,\r\n" +
                                            "		ConnectionString,\r\n" +
                                            "		[Server],\r\n" +
                                            "		[Database]\r\n" +
                                            "		FROM [dbo].[ObjectAttributes] \r\n" +
                                            "		PIVOT \r\n" +
                                            "			(\r\n" +
                                            "				MIN(ObjectAttrValue) FOR ObjectAttrName \r\n" +
                                            "					IN (ConnectionString, [Server], [Database])\r\n" +
                                            "			) AS ConnectionProperties\r\n" +
                                            "	) AS ConnectionProperties\r\n" +
                                            "	ON [Objects].ObjectKey = ConnectionProperties.ObjectKey\r\n" +
                                            "	AND [Objects].RunKey = ConnectionProperties.RunKey\r\n" +
                                            "INNER JOIN dbo.LookupConnectionID\r\n" +
                                            "	ON ConnectionGUID = [Objects].ObjectTypeString";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "ALTER VIEW [dbo].[TableLineageMap]\r\n" +
                                            "AS\r\n" +
                                            "SELECT\r\n" +
                                            "    dbo.WalkSources.RunKey,\r\n" +
                                            "    dbo.SourceTables.ObjectKey AS SourceTableObjectKey,\r\n" +
                                            "    dbo.SourceTables.ObjectName AS SourceTable,\r\n" +
                                            "    srel.ParentObjectKey AS SourceConnectionKey,\r\n" +
                                            "    sconn.ConnectionName AS SourceConnectionName,\r\n" +
                                            "    sconn.ConnectionString AS SourceConnectionString,\r\n" +
                                            "    sconn.[Server] AS SourceServer,\r\n" +
                                            "    sconn.[Database] AS SourceDatabase,\r\n" +
                                            "    dbo.SourceTables.SrcComponentKey AS SourceComponentKey,\r\n" +
                                            "    dbo.TargetTables.ObjectName AS TargetTable,\r\n" +
                                            "    dbo.TargetTables.TgtComponentKey AS TargetComponentKey,\r\n" +
                                            "    trel.ParentObjectKey AS TargetConnectionKey,\r\n" +
                                            "    tconn.ConnectionName AS TargetConnectionName,\r\n" +
                                            "    tconn.ConnectionString AS TargetConnectionString,\r\n" +
                                            "    tconn.[Server] AS TargetServer,\r\n" +
                                            "    tconn.[Database] AS TargetDatabase,\r\n" +
                                            "    dfrel.ParentObjectKey AS DataFlowKey,\r\n" +
                                            "    dbo.Packages.PackageName,\r\n" +
                                            "    dbo.Packages.PackageDesc,\r\n" +
                                            "    dbo.Packages.PackageLocation,\r\n" +
                                            "    dbo.Packages.PackageGUID\r\n" +
                                            "FROM dbo.WalkSources\r\n" +
                                            "INNER JOIN dbo.SourceTables\r\n" +
                                            "        ON dbo.WalkSources.osrc = dbo.SourceTables.ObjectKey\r\n" +
                                            "        AND dbo.WalkSources.RunKey = dbo.SourceTables.RunKey\r\n" +
                                            "INNER JOIN dbo.TargetTables\r\n" +
                                            "        ON dbo.WalkSources.tgt = dbo.TargetTables.ObjectKey\r\n" +
                                            "        AND dbo.WalkSources.RunKey = dbo.TargetTables.RunKey\r\n" +
                                            "INNER JOIN dbo.ObjectRelationships AS srel\r\n" +
                                            "        ON dbo.SourceTables.ObjectKey = srel.ChildObjectKey\r\n" +
                                            "        AND dbo.SourceTables.RunKey = srel.RunKey\r\n" +
                                            "INNER JOIN dbo.ObjectRelationships AS trel\r\n" +
                                            "\r\n" +
                                            "       ON dbo.TargetTables.ObjectKey = trel.ChildObjectKey\r\n" +
                                            "        AND dbo.TargetTables.RunKey = trel.RunKey\r\n" +
                                            "INNER JOIN dbo.ObjectRelationships AS dfrel\r\n" +
                                            "        ON dbo.TargetTables.TgtComponentKey = dfrel.ChildObjectKey\r\n" +
                                            "        AND dbo.TargetTables.RunKey = dfrel.RunKey\r\n" +
                                            "INNER JOIN dbo.ObjectRelationships AS pkgrel\r\n" +
                                            "        ON dfrel.ParentObjectKey = pkgrel.ChildObjectKey\r\n" +
                                            "        AND dfrel.RunKey = pkgrel.RunKey\r\n" +
                                            "INNER JOIN dbo.Packages\r\n" +
                                            "        ON pkgrel.ParentObjectKey = dbo.Packages.PackageID\r\n" +
                                            "        AND pkgrel.RunKey = dbo.Packages.RunKey\r\n" +
                                            "INNER JOIN dbo.Connections AS sconn\r\n" +
                                            "        ON srel.ParentObjectKey = sconn.ConnectionID\r\n" +
                                            "        AND srel.RunKey = sconn.RunKey\r\n" +
                                            "INNER JOIN dbo.Connections AS tconn\r\n" +
                                            "        ON trel.ParentObjectKey = tconn.ConnectionID\r\n" +
                                            "        AND trel.RunKey = tconn.RunKey";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "ALTER VIEW [dbo].[DataFlows]\r\n" +
                                            "AS\r\n" +
                                            "SELECT\r\n" +
                                            "    dbo.Objects.RunKey,\r\n" +
                                            "    dbo.Objects.ObjectKey,\r\n" +
                                            "    dbo.Objects.ObjectName,\r\n" +
                                            "    dbo.Objects.ObjectDesc,\r\n" +
                                            "    dbo.ObjectDependencies.SrcObjectKey AS PackageID\r\n" +
                                            "FROM dbo.Objects\r\n" +
                                            "INNER JOIN dbo.ObjectDependencies\r\n" +
                                            "        ON dbo.Objects.ObjectKey = dbo.ObjectDependencies.TgtObjectKey\r\n" +
                                            "        AND dbo.Objects.RunKey = dbo.ObjectDependencies.RunKey\r\n" +
                                            "WHERE dbo.Objects.ObjectTypeString IN (\r\n" +
                                            "	N'{C3BF9DC1-4715-4694-936F-D3CFDA9E42C5}', \r\n" +
                                            "	N'{E3CFBEA8-1F48-40D8-91E1-2DEDC1EDDD56}'\r\n" +
                                            ")\r\n" +
                                            "  AND dbo.ObjectDependencies.DependencyType = N'Containment'";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "ALTER VIEW [dbo].[ConnectionsMapping]\r\n" +
                                            "AS\r\n" +
                                            "SELECT DISTINCT \r\n" +
                                            "    srel.ParentObjectKey AS SourceConnectionID,\r\n" +
                                            "    trel.ParentObjectKey AS TargetConnectionID\r\n" +
                                            "FROM dbo.WalkSources\r\n" +
                                            "INNER JOIN dbo.SourceTables\r\n" +
                                            "        ON dbo.WalkSources.osrc = dbo.SourceTables.ObjectKey\r\n" +
                                            "        AND dbo.WalkSources.RunKey = dbo.SourceTables.RunKey\r\n" +
                                            "INNER JOIN dbo.TargetTables\r\n" +
                                            "        ON dbo.WalkSources.tgt = dbo.TargetTables.ObjectKey\r\n" +
                                            "        AND dbo.WalkSources.RunKey = dbo.TargetTables.RunKey\r\n" +
                                            "INNER JOIN dbo.ObjectRelationships AS srel\r\n" +
                                            "        ON dbo.SourceTables.ObjectKey = srel.ChildObjectKey\r\n" +
                                            "        AND srel.RunKey = dbo.SourceTables.RunKey\r\n" +
                                            "INNER JOIN dbo.ObjectRelationships AS trel\r\n" +
                                            "        ON dbo.TargetTables.ObjectKey = trel.ChildObjectKey\r\n" +
                                            "        AND dbo.TargetTables.RunKey = trel.RunKey";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "INSERT INTO dbo.Version\r\n" +
                                            "(VersionID, InstallDate)\r\n" +
                                            "VALUES\r\n" +
                                            "(4, GETDATE())";
                    sqlCommand.ExecuteNonQuery();
                    ////                sqlCommand.CommandText = "";
                    ////                sqlCommand.ExecuteNonQuery();
                }
#endregion
                dbVersion = 4;
            }

            if (dbVersion == 4)
            {
#region dbVersion 4
                // Apply corrected WalkSources View
                using (SqlCommand sqlAlterCommand = repositoryConnection.CreateCommand())
                {
                    sqlAlterCommand.CommandText = "SET ANSI_NULLS ON";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "SET QUOTED_IDENTIFIER ON";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "ALTER VIEW [dbo].[WalkSources]\r\n" +
                                                "AS\r\n" +
                                                "WITH WalkSourceCTE(RunKey, osrc, tgt, lvl, objecttype, ParentString) \r\n" +
                                                "AS \r\n" +
                                                "(\r\n" +
                                                "	SELECT  Objects.RunKey\r\n" +
                                                "		, dbo.SourceTables.ObjectKey\r\n" +
                                                "		, dbo.SourceTables.SrcComponentKey\r\n" +
                                                "		, 0 AS Expr1\r\n" +
                                                "		, dbo.Objects.ObjectTypeString\r\n" +
                                                "		, CAST(',' + CAST(dbo.SourceTables.ObjectKey as varchar(14)) + ',' AS VARCHAR(2000)) AS ParentString\r\n" +
                                                "	FROM dbo.SourceTables \r\n" +
                                                "	INNER JOIN dbo.Objects \r\n" +
                                                "		ON dbo.SourceTables.ObjectKey = dbo.Objects.ObjectKey\r\n" +
                                                "		AND SourceTables.RunKey = Objects.RunKey\r\n" +
                                                "	UNION ALL\r\n" +
                                                "	SELECT  Objects.RunKey\r\n" +
                                                "		, WalkSourceCTE.osrc\r\n" +
                                                "		, dbo.LineageMap.TgtObjectKey\r\n" +
                                                "		, WalkSourceCTE.lvl + 1 AS Expr1\r\n" +
                                                "		, Objects.ObjectTypeString\r\n" +
                                                "		, CAST(WalkSourceCTE.ParentString +  CAST(WalkSourceCTE.tgt as varchar(14)) + ',' AS VARCHAR(2000)) AS ParentString\r\n" +
                                                "	FROM         WalkSourceCTE\r\n" +
                                                "	INNER JOIN dbo.LineageMap \r\n" +
                                                "		ON WalkSourceCTE.tgt = dbo.LineageMap.SrcObjectKey \r\n" +
                                                "		AND WalkSourceCTE.RunKey = dbo.LineageMap.RunKey\r\n" +
                                                "	INNER JOIN dbo.Objects\r\n" +
                                                "		ON dbo.LineageMap.TgtObjectKey = Objects.ObjectKey\r\n" +
                                                "		AND LineageMap.RunKey = Objects.RunKey\r\n" +
                                                "	WHERE NOT ((WalkSourceCTE.osrc = WalkSourceCTE.tgt)\r\n" +
                                                "	OR CHARINDEX(',' + CAST(WalkSourceCTE.tgt AS VARCHAR(40)) + ',', WalkSourceCTE.ParentString) > 0)\r\n" +
                                                ")\r\n" +
                                                "    \r\n" +
                                                "SELECT   RunKey,   osrc, tgt, lvl, objecttype\r\n" +
                                                "FROM         WalkSourceCTE\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "INSERT INTO dbo.Version\r\n" +
                                            "(VersionID, InstallDate)\r\n" +
                                            "VALUES\r\n" +
                                            "(5, GETDATE())";
                    sqlAlterCommand.ExecuteNonQuery();
                }
#endregion
                dbVersion = 5;
            }
            if (dbVersion == 5)
            {
#region dbVersion 5
                // New Procedures
                using (SqlCommand sqlAlterCommand = repositoryConnection.CreateCommand())
                {
                    sqlAlterCommand.CommandText = "SET ANSI_NULLS ON";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "SET QUOTED_IDENTIFIER ON";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveRunIDs]') AND type in (N'P', N'PC'))\r\n";
                    sqlAlterCommand.CommandText += "EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveRunIDs] AS SELECT 1')";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description:	Retrieves the list of Run's\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "ALTER PROCEDURE [dbo].[usp_RetrieveRunIDs]\r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN\r\n";
                    sqlAlterCommand.CommandText += "SET NOCOUNT ON;\r\n";
                    sqlAlterCommand.CommandText += "SELECT [RunKey] ,CONVERT(NVARCHAR(40), [RunDate], 120) + CHAR(9) + [RunCommand] FROM [dbo].[RunScan]\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fn_IntCSVSplit]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))\r\n";
                    sqlAlterCommand.CommandText += "DROP FUNCTION [dbo].[fn_IntCSVSplit]";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description:	Retrieves a table of integers from a csv string\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText = "CREATE FUNCTION [dbo].[fn_IntCSVSplit]\r\n";
                    sqlAlterCommand.CommandText += "( @RowData NVARCHAR(MAX) )\r\n";
                    sqlAlterCommand.CommandText += "RETURNS @RtnValue TABLE \r\n";
                    sqlAlterCommand.CommandText += "( Data INT ) \r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN \r\n";
                    sqlAlterCommand.CommandText += "    DECLARE @Iterator INT\r\n";
                    sqlAlterCommand.CommandText += "    DECLARE @WorkString NVARCHAR(MAX)\r\n";
                    sqlAlterCommand.CommandText += "    SET @Iterator = 1\r\n";
                    sqlAlterCommand.CommandText += "    DECLARE @FoundIndex INT\r\n";
                    sqlAlterCommand.CommandText += "    SET @FoundIndex = CHARINDEX(',',@RowData)\r\n";
                    sqlAlterCommand.CommandText += "    WHILE (@FoundIndex>0)\r\n";
                    sqlAlterCommand.CommandText += "    BEGIN\r\n";
                    sqlAlterCommand.CommandText += "		SET @WorkString = LTRIM(RTRIM(SUBSTRING(@RowData, 1, @FoundIndex - 1)))\r\n";
                    sqlAlterCommand.CommandText += "		IF ISNUMERIC(@WorkString) = 1\r\n";
                    sqlAlterCommand.CommandText += "		BEGIN\r\n";
                    sqlAlterCommand.CommandText += "			INSERT INTO @RtnValue (data) VALUES (@WorkString)\r\n";
                    sqlAlterCommand.CommandText += "		END\r\n";
                    sqlAlterCommand.CommandText += "		ELSE\r\n";
                    sqlAlterCommand.CommandText += "		BEGIN\r\n";
                    sqlAlterCommand.CommandText += "			INSERT INTO @RtnValue (data) VALUES(NULL)\r\n";
                    sqlAlterCommand.CommandText += "		END\r\n";
                    sqlAlterCommand.CommandText += "        SET @RowData = SUBSTRING(@RowData, @FoundIndex + 1,LEN(@RowData))\r\n";
                    sqlAlterCommand.CommandText += "        SET @Iterator = @Iterator + 1\r\n";
                    sqlAlterCommand.CommandText += "        SET @FoundIndex = CHARINDEX(',', @RowData)\r\n";
                    sqlAlterCommand.CommandText += "    END\r\n";
                    sqlAlterCommand.CommandText += "    IF ISNUMERIC(LTRIM(RTRIM(@RowData))) = 1\r\n";
                    sqlAlterCommand.CommandText += "    BEGIN\r\n";
                    sqlAlterCommand.CommandText += "        INSERT INTO @RtnValue (Data) SELECT LTRIM(RTRIM(@RowData))\r\n";
                    sqlAlterCommand.CommandText += "    END\r\n";
                    sqlAlterCommand.CommandText += "    RETURN\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveObjects]') AND type in (N'P', N'PC'))\r\n";
                    sqlAlterCommand.CommandText += "EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveObjects] AS SELECT 1')";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description:	Retrieves the list of Objects\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "ALTER PROCEDURE [dbo].[usp_RetrieveObjects]\r\n";
                    sqlAlterCommand.CommandText += "	@RunList nvarchar(max)\r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN\r\n";
                    sqlAlterCommand.CommandText += "	SET NOCOUNT ON;\r\n";
                    sqlAlterCommand.CommandText += "		SELECT [ObjectKey], [ObjectName], [Objects].[ObjectTypeString], [ObjectTypes].[ObjectTypeName], [RunKey]\r\n";
                    sqlAlterCommand.CommandText += "		FROM [dbo].[Objects] \r\n";
                    sqlAlterCommand.CommandText += "		LEFT OUTER JOIN [dbo].[ObjectTypes]\r\n";
                    sqlAlterCommand.CommandText += "			ON [Objects].[ObjectTypeString] = [ObjectTypes].[ObjectTypeKey]\r\n";
                    sqlAlterCommand.CommandText += "		INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter\r\n";
                    sqlAlterCommand.CommandText += "			ON Filter.Data = [RunKey]\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveLineageMap]') AND type in (N'P', N'PC'))\r\n";
                    sqlAlterCommand.CommandText += "EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveLineageMap] AS SELECT 1')";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description:	Retrieves the list of LineageMap\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "ALTER PROCEDURE [dbo].[usp_RetrieveLineageMap]\r\n";
                    sqlAlterCommand.CommandText += "	@RunList nvarchar(max)\r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN\r\n";
                    sqlAlterCommand.CommandText += "	SET NOCOUNT ON;\r\n";
                    sqlAlterCommand.CommandText += "		SELECT [SrcObjectKey], [TgtObjectKey], [DependencyType] \r\n";
                    sqlAlterCommand.CommandText += "		FROM [dbo].[ObjectDependencies]\r\n";
                    sqlAlterCommand.CommandText += "		INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter\r\n";
                    sqlAlterCommand.CommandText += "			ON Filter.Data = [RunKey]\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveObjectDetails]') AND type in (N'P', N'PC'))\r\n";
                    sqlAlterCommand.CommandText += "EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveObjectDetails] AS SELECT 1')";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description:	Retrieves the ObjectDetails\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "ALTER PROCEDURE [dbo].[usp_RetrieveObjectDetails]\r\n";
                    sqlAlterCommand.CommandText += "	@RunList nvarchar(max)\r\n";
                    sqlAlterCommand.CommandText += ",	@ObjectKey INT\r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN\r\n";
                    sqlAlterCommand.CommandText += "	SET NOCOUNT ON;\r\n";
                    sqlAlterCommand.CommandText += "		SELECT [ObjectTypeString], [ObjectDesc] \r\n";
                    sqlAlterCommand.CommandText += "		FROM [dbo].[Objects]\r\n";
                    sqlAlterCommand.CommandText += "		INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter\r\n";
                    sqlAlterCommand.CommandText += "			ON Filter.Data = [RunKey]\r\n";
                    sqlAlterCommand.CommandText += "			AND ObjectKey = @ObjectKey\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveObjectTypes]') AND type in (N'P', N'PC'))\r\n";
                    sqlAlterCommand.CommandText += "EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveObjectTypes] AS SELECT 1')";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description:	Retrieves the ObjectTypes\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "ALTER PROCEDURE [dbo].[usp_RetrieveObjectTypes]\r\n";
                    sqlAlterCommand.CommandText += "	@ObjectTypeKey NVARCHAR(255)\r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN\r\n";
                    sqlAlterCommand.CommandText += "	SET NOCOUNT ON;\r\n";
                    sqlAlterCommand.CommandText += "		SELECT [ObjectTypeName]\r\n";
                    sqlAlterCommand.CommandText += "		FROM [dbo].[ObjectTypes] \r\n";
                    sqlAlterCommand.CommandText += "		WHERE ObjectTypeKey = @ObjectTypeKey\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveObjectAttributes]') AND type in (N'P', N'PC'))\r\n";
                    sqlAlterCommand.CommandText += "EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveObjectAttributes] AS SELECT 1')";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description:	Retrieves the ObjectAttributes\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "ALTER PROCEDURE [dbo].[usp_RetrieveObjectAttributes]\r\n";
                    sqlAlterCommand.CommandText += "	@RunList nvarchar(max)\r\n";
                    sqlAlterCommand.CommandText += ",	@ObjectKey INT\r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN\r\n";
                    sqlAlterCommand.CommandText += "	SET NOCOUNT ON;\r\n";
                    sqlAlterCommand.CommandText += "		SELECT [ObjectAttrName], [ObjectAttrValue]\r\n";
                    sqlAlterCommand.CommandText += "		FROM [dbo].[ObjectAttributes] \r\n";
                    sqlAlterCommand.CommandText += "		INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter\r\n";
                    sqlAlterCommand.CommandText += "			ON Filter.Data = [RunKey]\r\n";
                    sqlAlterCommand.CommandText += "			AND ObjectKey = @ObjectKey\r\n";
                    sqlAlterCommand.CommandText += "		ORDER BY [ObjectAttrName];\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveContainedTargetDependencies]') AND type in (N'P', N'PC'))\r\n";
                    sqlAlterCommand.CommandText += "EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveContainedTargetDependencies] AS SELECT 1')";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description:	Retrieves the Contained Target Dependencies\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "ALTER PROCEDURE [dbo].[usp_RetrieveContainedTargetDependencies]\r\n";
                    sqlAlterCommand.CommandText += "	@RunList nvarchar(max)\r\n";
                    sqlAlterCommand.CommandText += ",	@TgtObjectKey INT\r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN\r\n";
                    sqlAlterCommand.CommandText += "	SET NOCOUNT ON;\r\n";
                    sqlAlterCommand.CommandText += "	SELECT [SrcObjectKey] \r\n";
                    sqlAlterCommand.CommandText += "	FROM [dbo].[ObjectDependencies] \r\n";
                    sqlAlterCommand.CommandText += "	INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter\r\n";
                    sqlAlterCommand.CommandText += "		ON Filter.Data = [RunKey]\r\n";
                    sqlAlterCommand.CommandText += "		AND [DependencyType] = 'Containment' \r\n";
                    sqlAlterCommand.CommandText += "		AND [TgtObjectKey] = @TgtObjectKey\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveSSASObjects]') AND type in (N'P', N'PC'))\r\n";
                    sqlAlterCommand.CommandText += "EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveSSASObjects] AS SELECT 1')";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description:	Retrieves the SSAS Objects\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "ALTER PROCEDURE [dbo].[usp_RetrieveSSASObjects]\r\n";
                    sqlAlterCommand.CommandText += "	@RunList nvarchar(max)\r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN\r\n";
                    sqlAlterCommand.CommandText += "	SET NOCOUNT ON;\r\n";
                    sqlAlterCommand.CommandText += "	SELECT [ObjectKey], [ObjectName]\r\n";
                    sqlAlterCommand.CommandText += "	FROM [dbo].[Objects]\r\n";
                    sqlAlterCommand.CommandText += "	INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter\r\n";
                    sqlAlterCommand.CommandText += "		ON Filter.Data = [RunKey]\r\n";
                    sqlAlterCommand.CommandText += "		AND [ObjectTypeString] = 'Ssas.Analysis Server'\r\n";
                    sqlAlterCommand.CommandText += "	ORDER BY [ObjectName]\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveSQLSObjects]') AND type in (N'P', N'PC'))\r\n";
                    sqlAlterCommand.CommandText += "EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveSQLSObjects] AS SELECT 1')";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description:	Retrieves the SQL Server Objects\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "ALTER PROCEDURE [dbo].[usp_RetrieveSQLSObjects]\r\n";
                    sqlAlterCommand.CommandText += "	@RunList nvarchar(max)\r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN\r\n";
                    sqlAlterCommand.CommandText += "	SET NOCOUNT ON;\r\n";
                    sqlAlterCommand.CommandText += "	SELECT  ConnectionID, ISNULL([Server], ConnectionName) + ISNULL('.' + [Database], '') as DisplayName\r\n";
                    sqlAlterCommand.CommandText += "	FROM  [dbo].[Connections]\r\n";
                    sqlAlterCommand.CommandText += "	INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter\r\n";
                    sqlAlterCommand.CommandText += "		ON Filter.Data = [RunKey]\r\n";
                    sqlAlterCommand.CommandText += "	ORDER BY DisplayName\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveSSRSObjects]') AND type in (N'P', N'PC'))\r\n";
                    sqlAlterCommand.CommandText += "EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveSSRSObjects] AS SELECT 1')";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description:	Retrieves the SSRS Objects\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "ALTER PROCEDURE [dbo].[usp_RetrieveSSRSObjects]\r\n";
                    sqlAlterCommand.CommandText += "	@RunList nvarchar(max)\r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN\r\n";
                    sqlAlterCommand.CommandText += "	SET NOCOUNT ON;\r\n";
                    sqlAlterCommand.CommandText += "	SELECT [ObjectKey], [ObjectName]\r\n";
                    sqlAlterCommand.CommandText += "	FROM [dbo].[Objects]\r\n";
                    sqlAlterCommand.CommandText += "	INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter\r\n";
                    sqlAlterCommand.CommandText += "		ON Filter.Data = [RunKey]\r\n";
                    sqlAlterCommand.CommandText += "		AND [ObjectTypeString] = 'ReportServer'\r\n";
                    sqlAlterCommand.CommandText += "	ORDER BY [ObjectName]\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveFileObjects]') AND type in (N'P', N'PC'))\r\n";
                    sqlAlterCommand.CommandText += "EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveFileObjects] AS SELECT 1')";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description:	Retrieves the File Server Objects\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "ALTER PROCEDURE [dbo].[usp_RetrieveFileObjects]\r\n";
                    sqlAlterCommand.CommandText += "	@RunList nvarchar(max)\r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN\r\n";
                    sqlAlterCommand.CommandText += "	SET NOCOUNT ON;\r\n";
                    sqlAlterCommand.CommandText += "	SELECT [ObjectKey], [ObjectName]\r\n";
                    sqlAlterCommand.CommandText += "	FROM [dbo].[Objects]\r\n";
                    sqlAlterCommand.CommandText += "	INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter\r\n";
                    sqlAlterCommand.CommandText += "		ON Filter.Data = [RunKey]\r\n";
                    sqlAlterCommand.CommandText += "		AND [ObjectTypeString] = 'Machine'\r\n";
                    sqlAlterCommand.CommandText += "	ORDER BY [ObjectName]\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveSSISObjects]') AND type in (N'P', N'PC'))\r\n";
                    sqlAlterCommand.CommandText += "EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveSSISObjects] AS SELECT 1')";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description:	Retrieves the SSIS Objects\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "ALTER PROCEDURE [dbo].[usp_RetrieveSSISObjects]\r\n";
                    sqlAlterCommand.CommandText += "	@RunList nvarchar(max)\r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN\r\n";
                    sqlAlterCommand.CommandText += "	SET NOCOUNT ON;\r\n";
                    sqlAlterCommand.CommandText += "	SELECT [PackageID], [PackageLocation] \r\n";
                    sqlAlterCommand.CommandText += "	FROM [dbo].[Packages]\r\n";
                    sqlAlterCommand.CommandText += "	INNER JOIN [dbo].[fn_IntCSVSplit](@RunList) AS Filter\r\n";
                    sqlAlterCommand.CommandText += "		ON Filter.Data = [RunKey]\r\n";
                    sqlAlterCommand.CommandText += "	ORDER BY [PackageID]\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RetrieveContained]') AND type in (N'P', N'PC'))\r\n";
                    sqlAlterCommand.CommandText += "EXEC ('CREATE PROCEDURE [dbo].[usp_RetrieveContained] AS SELECT 1')";
                    sqlAlterCommand.ExecuteNonQuery();
                    sqlAlterCommand.CommandText = "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "-- Author:		Keith Martin\r\n";
                    sqlAlterCommand.CommandText += "-- Create date: 2011-11-16\r\n";
                    sqlAlterCommand.CommandText += "-- Description: Retrieves the Children of this Containment Object\r\n";
                    sqlAlterCommand.CommandText += "-- =============================================\r\n";
                    sqlAlterCommand.CommandText += "ALTER PROCEDURE [dbo].[usp_RetrieveContained]\r\n";
                    sqlAlterCommand.CommandText += "	@SrcObjectKey INT\r\n";
                    sqlAlterCommand.CommandText += "AS\r\n";
                    sqlAlterCommand.CommandText += "BEGIN\r\n";
                    sqlAlterCommand.CommandText += "	SET NOCOUNT ON;\r\n";
                    sqlAlterCommand.CommandText += "	SELECT DISTINCT TgtObjectKey, ObjectName, ISNULL(ObjectTypes.ObjectTypeName, ObjectTypeString) as ObjectTypeString \r\n";
                    sqlAlterCommand.CommandText += "	FROM [dbo].[ObjectDependencies]\r\n";
                    sqlAlterCommand.CommandText += "			INNER JOIN [dbo].[Objects] \r\n";
                    sqlAlterCommand.CommandText += "				ON ObjectKey = TgtObjectKey\r\n";
                    sqlAlterCommand.CommandText += "				AND [DependencyType] = 'Containment' \r\n";
                    sqlAlterCommand.CommandText += "				AND SrcObjectKey = @SrcObjectKey\r\n";
                    sqlAlterCommand.CommandText += "	LEFT OUTER JOIN [dbo].[ObjectTypes] \r\n";
                    sqlAlterCommand.CommandText += "		ON ObjectTypes.ObjectTypeKey = ObjectTypeString \r\n";
                    sqlAlterCommand.CommandText += "	ORDER BY ObjectTypeString, ObjectName\r\n";
                    sqlAlterCommand.CommandText += "END\r\n";
                    sqlAlterCommand.ExecuteNonQuery();


                    sqlAlterCommand.CommandText = "INSERT INTO dbo.Version\r\n" +
                                            "(VersionID, InstallDate)\r\n" +
                                            "VALUES\r\n" +
                                            "(6, GETDATE())";
                    sqlAlterCommand.ExecuteNonQuery();
                }
#endregion
                dbVersion = 6;
            }
            if (dbVersion == 6)
            {
                // Add SQL 2012 entries to allow display of relational sources.
#region dbVersion 6
                using (SqlCommand sqlCommand = repositoryConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM [dbo].[LookupConnectionID] WHERE [ConnectionGUID] = N'{32808317-2AFC-4E1C-A03A-9F8477A3BDBA}') BEGIN\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{32808317-2AFC-4E1C-A03A-9F8477A3BDBA}', N'ODBC 2012')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{3269FBD7-897B-4CDF-8988-2E1A24B10FBB}', N'OLEDB 2012')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{68FFA586-FFA5-41DC-8EDE-13102087EF33}', N'ADO 2012')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{39CAF1E8-6582-4C31-A5C6-405A8661EEC1}', N'ADO.Net 2012')\r\n"+
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{E3D5D606-997B-4EF6-90AD-43483A788CC3}', N'MSOLAP 2012')\r\n"+
                        "END;";
                    sqlCommand.ExecuteNonQuery();

                    dbVersion = 7;

                    sqlCommand.CommandText = String.Format("INSERT INTO dbo.Version\r\n" +
                                            "(VersionID, InstallDate)\r\n" +
                                            "VALUES\r\n" +
                                            "({0}, GETDATE())", dbVersion);
                    sqlCommand.ExecuteNonQuery();


                }
#endregion
            }
            if (dbVersion == 7)
            {
                // Add SQL 2014 entries to allow display of relational sources.
#region dbVersion 7
                using (SqlCommand sqlCommand = repositoryConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "IF NOT EXISTS (SELECT 1 FROM [dbo].[LookupConnectionID] WHERE [ConnectionGUID] = N'{1818FF09-AF4D-4EA8-8C9D-0AB43B5775E5}') BEGIN\r\n" + 
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{1818FF09-AF4D-4EA8-8C9D-0AB43B5775E5}', N'ODBC 2014')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{F3F3005C-C3CB-4C61-B2A9-056035E4D8F2}', N'OLEDB 2014')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{FFEDAAC9-D6BD-4E6B-90AB-D4D296B5096A}', N'ADO 2014')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{D5353B56-34DA-4C97-AC94-722B91013E89}', N'ADO.Net 2014')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{05B2302B-4C20-43FD-92B3-3A067A037436}', N'MSOLAP 2014')\r\n" +
                        "END;";
                    sqlCommand.ExecuteNonQuery();

                    dbVersion = 8;
                    sqlCommand.CommandText = String.Format("INSERT INTO dbo.Version\r\n" +
                                            "(VersionID, InstallDate)\r\n" +
                                            "VALUES\r\n" +
                                            "({0}, GETDATE())", dbVersion);
                    sqlCommand.ExecuteNonQuery();

                }

#endregion
            }
            if (dbVersion == 8)
            {
                // Add SQL 2016 and 2017 entries to allow display of relational sources.
#region dbVersion 8
                using (SqlCommand sqlCommand = repositoryConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{1EABA520-CAC6-4134-ACC1-C8B2ED261247}', N'ODBC 2016')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{96A2155A-6C39-4F46-B5A4-EC0B63FA0655}', N'OLEDB 2016')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{4696FBDD-C1BC-4009-AABD-D2AE31E24F0D}', N'ADO 2016')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{5C9E27BC-DF64-48F1-855E-92EF415C638C}', N'ADO.Net 2016')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{F4D9470A-E60A-4BAD-ACC6-2E3AA759E0BD}', N'MSOLAP 2016')\r\n";
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{3D17B9AD-F94B-47C8-92F8-AD713A7BA732}', N'ODBC 2017')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{5802D1B1-DCFC-4F1E-8ACD-388327A21A9C}', N'OLEDB 2017')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{9886B70F-B8DD-49C3-BD50-85D9B6A88A72}', N'ADO 2017')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{58297D74-0AC9-4659-B8EE-D5B38CAA686F}', N'ADO.Net 2017')\r\n" +
                        "INSERT [dbo].[LookupConnectionID] ([ConnectionGUID], [ConnectionDescription]) VALUES (N'{D3102C96-CC8A-409D-A94C-2175F8D16FE0}', N'MSOLAP 2017')\r\n";
                    sqlCommand.ExecuteNonQuery();

                    dbVersion = 9;
                    sqlCommand.CommandText = String.Format("INSERT INTO dbo.Version\r\n" +
                                            "(VersionID, InstallDate)\r\n" +
                                            "VALUES\r\n" +
                                            "({0}, GETDATE())", dbVersion);
                    sqlCommand.ExecuteNonQuery();

                }

#endregion

                // Dont forget to update _dbVersion, or the new data won't be committed.
            }
        }

        public void LoadExisingRepository()
        {
            using (SqlCommand sqlCommand = new SqlCommand("SELECT COALESCE(MAX(ObjectKey), 0) + 1 FROM [dbo].[Objects]", repositoryConnection))
            {
                using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
                {
                    sqlReader.Read();
                    objectTable.Columns["ObjectKey"].AutoIncrementSeed = System.Convert.ToInt32(sqlReader[0]);
                }
            }
            using (SqlCommand sqlCommand = new SqlCommand("SELECT COALESCE(MAX(RunKey), 0) + 1 FROM [dbo].[RunScan]", repositoryConnection))
            {
                using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
                {
                    sqlReader.Read();
                    RunKeyValue = System.Convert.ToInt32(sqlReader[0]);
                    runScanTable.Columns["RunKey"].AutoIncrementSeed = System.Convert.ToInt32(sqlReader[0]);
                }
            }
            using (SqlCommand sqlCommand = new SqlCommand("SELECT [ObjectTypeKey], [ObjectTypeName], [ObjectTypeDesc], [ObjectMetaType], [Domain] FROM [dbo].[ObjectTypes]", repositoryConnection))
            {
                using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        this.AddObjectType(sqlReader[4].ToString(), sqlReader[0].ToString(), sqlReader[1].ToString(), sqlReader[2].ToString());
                    }
                }
            }
            // Mark all the records as unchanged.
            objectTypesTable.AcceptChanges();
            // This is no longer required, as the bulk load will only add NEW records.
            //using (SqlCommand sqlCommand = new SqlCommand("truncate table ObjectTypes", repositoryConnection))
            //{
            //    sqlCommand.ExecuteNonQuery();
            //}
        }

        public void DeleteExistingRepository()
        {
            using (SqlCommand sqlCommand = repositoryConnection.CreateCommand())
            {
                sqlCommand.CommandText = "truncate table dbo.ObjectAttributes";
                sqlCommand.ExecuteNonQuery();

                sqlCommand.CommandText = "truncate table dbo.ObjectDependencies";
                sqlCommand.ExecuteNonQuery();

                sqlCommand.CommandText = "delete from dbo.Objects";
                sqlCommand.ExecuteNonQuery();

                sqlCommand.CommandText = "delete from dbo.RunScan";
                sqlCommand.ExecuteNonQuery();

                sqlCommand.CommandText = "truncate table ObjectTypes";
                sqlCommand.ExecuteNonQuery();
            }
        }

        public void InitialiseRepository(string commandLine)
        {
            // adds an object and returns the ID
            DataRow row = runScanTable.NewRow();
            row["RunDate"] = DateTime.Now;
            row["RunCommand"] = commandLine;
            runScanTable.Rows.Add(row);
        }

        /// <summary>
        /// Add a new object to the repository
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="objectType"></param>
        /// <param name="parentObjectID"></param>
        /// <returns></returns>
        public int AddObject(string name, string description, string objectType, int parentObjectID)
        {
            // adds an object and returns the ID
            DataRow row = objectTable.NewRow();

            row["RunKey"] = this.runKeyValue;
            row["ObjectName"] = name;
            row["ObjectTypeString"] = objectType;
            row["ObjectDesc"] = description;

            objectTable.Rows.Add(row);

            // the ObjectKey is an identity column and should be incremented automatically.
            int objectID = (int)row["ObjectKey"];

            // add containment 
            AddObjectContainment(parentObjectID, objectID);

            return objectID;
        }

        /// <summary>
        /// adds the type of object to the repository
        /// </summary>
        /// <param name="objectType"></param>
        public void AddObjectType(string domain, string objectType)
        {
            AddObjectType(domain, objectType, objectType, string.Empty);
        }

        /// <summary>
        /// adds the type of object to the repository
        /// </summary>
        /// <param name="objectType"></param>
        public void AddObjectType(string domain, string objectTypeKey, string objectTypeName)
        {
            AddObjectType(domain, objectTypeKey, objectTypeName, string.Empty);
        }

        /// <summary>
        /// Adds the type of object to the repository
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public void AddObjectType(string domain, string objectType, string name, string description)
        {
            if (!IsTypeDefined(objectType))
            {
                DataRow row = objectTypesTable.NewRow();

                row["ObjectTypeID"] = objectType;
                row["ObjectTypeName"] = name;
                row["ObjectTypeDesc"] = description;
                row["Domain"] = domain;

                objectTypesTable.Rows.Add(row);
            }
        }

        public bool IsTypeDefined(string typeID)
        {
            DataRow[] rows = this.objectTypesTable.Select(string.Format("ObjectTypeID = '{0}'", typeID));
            Debug.Assert(rows.Length == 0 || rows.Length == 1);

            return (rows.Length > 0);
        }

        /// <summary>
        ///  adds a relationship between objects that's not containment or lineage map
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void AddUseDependency(int from, int to)
        {
            AddDependency(from, to, DependencyTypes.Use);
        }

        /// <summary>
        /// Adds a containment relationship between specified objects
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        private void AddObjectContainment(int parent, int child)
        {
            AddDependency(parent, child, DependencyTypes.Containment);
        }

        /// <summary>
        /// Adds a data mapping between objects
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void AddMapping(int from, int to)
        {
            AddDependency(from, to, DependencyTypes.Lineage);
        }

        /// <summary>
        /// adds a row to the dependencies table
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        private void AddDependency(int from, int to, string type)
        {
            DataRow row = objectDependenciesTable.NewRow();

            row["RunKey"] = this.runKeyValue;
            row["SrcObjectKey"] = from;
            row["TgtObjectKey"] = to;
            row["DependencyType"] = type;

            if (!DoesDependencyExist(from, to, type))
            {
                objectDependenciesTable.Rows.Add(row);
            }
        }

        /// <summary>
        ///  returns whether there's a mapping between the source and target objects
        /// todo: perf hit?
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool DoesMappingExist(int from, int to)
        {
            return DoesDependencyExist(from, to, DependencyTypes.Lineage);
        }

        /// <summary>
        ///  returns whether there's a mapping between the source and target objects
        /// todo: perf hit?
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool DoesDependencyExist(int from, int to, string dependencyType)
        {
            DataRow[] rows = objectDependenciesTable.Select(string.Format("RunKey = '{0}' AND SrcObjectKey = '{1}' AND TgtObjectKey = '{2}' AND DependencyType = '{3}'", this.RunKeyValue, from, to, dependencyType));

            return (rows.Length > 0);
        }

        /// <summary>
        /// Adds attributes for an object.
        /// </summary>
        /// <param name="objectID"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        public void AddAttribute(int objectID, string attributeName, string attributeValue)
        {
            if (!string.IsNullOrEmpty(attributeName) && !string.IsNullOrEmpty(attributeValue))
            {
                DataRow row = objectAttributesTable.NewRow();

                row["RunKey"] = this.runKeyValue;
                row["ObjectKey"] = objectID;
                row["ObjectAttrName"] = attributeName;
                row["ObjectAttrValue"] = attributeValue;

                objectAttributesTable.Rows.Add(row);
            }
        }

        /// <summary>
        /// Return IDs of objects that have specified name and type
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="type">type</param>
        /// <returns>Array of object IDs that satisfy the condition</returns>
        private int[] GetNamedObjects(string name, string type)
        {
            DataRow[] rows = objectTable.Select(string.Format("ObjectTypeString = '{0}' AND ObjectName = '{1}'", type, name.Replace("'", "''")));

            int[] ids = new int[rows.Length];
            for (int i = 0; i < ids.Length; ++i)
            {
                ids[i] = (int)rows[i]["ObjectKey"];
            }

            return ids;
        }

        /// <summary>
        /// Return ID of objects that have specified name and type
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="type">type</param>
        /// <returns>Object ID or -1</returns>
        private int GetNamedObject(string name, string type)
        {
            int[] ids = GetNamedObjects(name, type);
            if (ids.Length > 0)
            {
                Debug.Assert(ids.Length == 1);
                return ids[0];
            }
            else
            {
                return -1;
            }
        }


        /// <summary>
        /// Returns the connection string that is associated with a particular Connection ID
        /// </summary>
        /// <param name="connectionID">The ID for the connection that you want a string about.</param>
        /// <returns></returns>
        public string RetrieveConnectionString(int connectionID)
        {
            DataRow[] rows = objectAttributesTable.Select(string.Format("ObjectAttrName = 'ConnectionString' AND ObjectKey = '{0}' ", connectionID));
            if (rows.Length > 0)
            {
                Debug.Assert(rows.Length == 1); // should be only one because we're keeping them unique.
                return (string)rows[0]["ObjectAttrValue"];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to return the database name from the connection string that the connectionID points at..
        /// </summary>
        /// <param name="connectionID"></param>
        /// <returns>the database name, or an empty string.</returns>
        public string RetrieveDatabaseNameFromConnectionID(int connectionID)
        {
            object existing;
            string dbName = string.Empty;
            String connectionString = RetrieveConnectionString(connectionID);
            OleDbConnectionStringBuilder csBuilder = (OleDbConnectionStringBuilder)GetConnectionStringBuilder(connectionString);
            if (csBuilder == null)
            {
                csBuilder = (OleDbConnectionStringBuilder)GetConnectionStringBuilder(string.Empty);
            }
            if (csBuilder.TryGetValue(Repository.ConnectionStringProperties.InitialCatalog, out existing))
                dbName = (String)existing;
            else if (csBuilder.TryGetValue(Repository.ConnectionStringProperties.Database, out existing))
                dbName = (String)existing;
            return dbName;
        }

        /// <summary>
        /// returns ID of the connection with specified connection string. -1  if there's no connection with
        /// specificed connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public int GetConnection(string connectionString)
        {
            int connectionID = -1; // assume no connection will be found

            // if a connection with the same string exists, return it.
            DataRow[] rows = objectAttributesTable.Select(string.Format("ObjectAttrName = 'ConnectionString' AND ObjectAttrValue = '{0}' ", connectionString.Replace("'", "''")));
            if (rows.Length > 0)
            {
                Debug.Assert(rows.Length == 1); // should be only one because we're keeping them unique.
                connectionID = (int)rows[0]["ObjectKey"];
            }
            else
            {
                connectionID = GetOleDbCanonicalConnection(connectionString);
            }

            return connectionID;
        }

        private int GetOleDbCanonicalConnection(string connectionString)
        {
            int connectionID = -1;

            try
            {
                DbConnectionStringBuilder targetConnectionStringBuilder = GetConnectionStringBuilder(connectionString);

                if (targetConnectionStringBuilder != null)
                {
                    // get a list of existing connection strings
                    DataRow[] rows = objectAttributesTable.Select(string.Format("ObjectAttrName = 'ConnectionString'"));
                    foreach (DataRow row in rows)
                    {
                        try
                        {
                            DbConnectionStringBuilder currentConnectionStringBuilder = GetConnectionStringBuilder(row["ObjectAttrValue"].ToString());

                            if (currentConnectionStringBuilder != null)
                            {
                                if (databaseNameOnlyCompare)
                                {
                                    // todo: does username matter for the relational database?                                
                                    //if (CompareProviders(targetConnectionStringBuilder, currentConnectionStringBuilder)
                                    //    && CompareDatabases(targetConnectionStringBuilder, currentConnectionStringBuilder)
                                    //    )
                                    // And drop the Provider name, as this isn't there for the SQLDBEnumerator...
                                    if (CompareDatabases(targetConnectionStringBuilder, currentConnectionStringBuilder))
                                    {
                                        connectionID = (int)row["ObjectKey"];
                                        break;
                                    }
                                }
                                else
                                {
                                    // todo: does username matter for the relational database?                                
                                    if (CompareProviders(targetConnectionStringBuilder, currentConnectionStringBuilder)
                                        && CompareServers(targetConnectionStringBuilder, currentConnectionStringBuilder)
                                        && CompareDatabases(targetConnectionStringBuilder, currentConnectionStringBuilder)
                                        )
                                    {
                                        connectionID = (int)row["ObjectKey"];
                                        break;
                                    }
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                // ignore
            }

            return connectionID;
        }

        private bool CompareDatabases(DbConnectionStringBuilder builder1, DbConnectionStringBuilder builder2)
        {
            string db1 = GetDatabase(builder1);
            string db2 = GetDatabase(builder2);
            if (db1 == null)
                return false;
            if (db2 == null)
                return false;
            if (databasePrefixExclusions.Count > 0)
            {
                foreach (string dbPrefix in databasePrefixExclusions)
                {
                    if (db1.StartsWith(dbPrefix))
                    {
                        db1 = db1.Substring(dbPrefix.Length);
                    }
                    if (db2.StartsWith(dbPrefix))
                    {
                        db2 = db2.Substring(dbPrefix.Length);
                    }
                }
            }

            return (db1.ToLower() == db2.ToLower());
        }

        /// <summary>
        ///  parses the database out of connection string by understanding the different ways providers store
        ///  this information.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public string GetDatabase(DbConnectionStringBuilder builder)
        {
            object ob = null;

            builder.TryGetValue(ConnectionStringProperties.InitialCatalog, out ob);
            if ((ob == null) || (string.IsNullOrEmpty(ob.ToString())))
            {
                builder.TryGetValue(ConnectionStringProperties.Database, out ob);
            }

            string database = null;

            if (ob != null)
                database = ob.ToString();

            return database;
        }


        public DbConnectionStringBuilder GetConnectionStringBuilder(string connectionString)
        {
            DbConnectionStringBuilder builder = null;

            // try to create an OledbConnectionString
            try
            {
                builder = new OleDbConnectionStringBuilder(connectionString);

                // if we reach here, we were able to parse it 
            }
            catch (System.Exception)
            {
            }

            return builder;
        }

        /// <summary>
        ///  returns True if the providers in both connection strings match
        /// </summary>
        /// <param name="builder1"></param>
        /// <param name="builder2"></param>
        /// <returns></returns>
        private bool CompareProviders(DbConnectionStringBuilder builder1, DbConnectionStringBuilder builder2)
        {
            bool providersMatch = false;
            object ob = null;

            if (builder1.TryGetValue(ConnectionStringProperties.Provider, out ob))
            {
                string provider1 = ob.ToString();

                if (builder2.TryGetValue(ConnectionStringProperties.Provider, out ob))
                {
                    string provider2 = ob.ToString();
                    if (provider1.ToLower() == provider2.ToLower())
                    {
                        providersMatch = true;
                    }
                    else
                    {
                        if ((provider1.StartsWith("SQLNCLI") || (provider1.StartsWith("SQLOLEDB")))
                            && (provider2.StartsWith("SQLNCLI") || provider2.StartsWith("SQLOLEDB")))
                        {
                            providersMatch = true;
                        }
                    }
                }
            }

            return providersMatch;
        }

        /// <summary>
        ///  compares if the servers in the two connection strings are the same.
        /// </summary>
        /// <param name="builder1"></param>
        /// <param name="builder2"></param>
        /// <returns></returns>
        private bool CompareServers(DbConnectionStringBuilder builder1, DbConnectionStringBuilder builder2)
        {
            bool matched = false;
            string dataSource1 = GetServer(builder1);
            string dataSource2 = GetServer(builder2);

            if (dataSource1.ToLower() == dataSource2.ToLower())
            {
                matched = true;
            }
            else
            {
                if (dataSource1 == "." || (string.Compare(dataSource1, "localhost", true) == 0) || (string.Compare(dataSource1, "(local)", true) == 0))
                {
                    if (dataSource2 == "." || (string.Compare(dataSource2, "localhost", true) == 0) || (string.Compare(dataSource2, "(local)", true) == 0))
                    {
                        matched = true;
                    }
                }
            }

            return matched;
        }

        /// <summary>
        /// Tries to get the server out of the connection strings by looking for common ways of specifying them.
        /// </summary>
        /// <param name="builder1"></param>
        /// <returns></returns>
        public string GetServer(DbConnectionStringBuilder builder1)
        {
            object ob = null;
            string dataSource = null;
            builder1.TryGetValue(ConnectionStringProperties.DataSource, out ob);
            if ((ob == null) || (string.IsNullOrEmpty(ob.ToString())))
                builder1.TryGetValue(ConnectionStringProperties.Server, out ob);
            if ((ob == null) || (string.IsNullOrEmpty(ob.ToString())))
                builder1.TryGetValue(ConnectionStringProperties.Location, out ob);

            if (ob != null)
            {
                dataSource = ob.ToString();
            }

            return dataSource;
        }

        /// <summary>
        /// Gets the ID for a table, and creates it if it doesn't exist.  Also sets the description on the table.
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="tableName"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        public int GetTable(int connectionID, string tableName, string Description)
        {
            int tableID = -1; // assume no table will be found

            // get the tables that correspond to this connection
            DataRow[] connectionChildren = objectDependenciesTable.Select(string.Format("SrcObjectKey = '{0}' AND DependencyType = '{1}'", connectionID, DependencyTypes.Containment));

            // see if any table has our name
            foreach (DataRow row in connectionChildren)
            {
                DataRow[] relationalTableRows = objectTable.Select(string.Format("ObjectKey = '{0}' AND ObjectTypeString = '{1}' AND ObjectName = '{2}'", row["TgtObjectKey"], RelationalEnumerator.ObjectTypes.Table, tableName.Replace("'","''")));

                if (relationalTableRows.Length > 0)
                {
                    Debug.Assert(relationalTableRows.Length == 1); // should be only one table in a connection with the same name
                    tableID = (int)relationalTableRows[0]["ObjectKey"];
                    // Attempt to update the description, if there is one provided, and it's different to what's already there.
                    if (!string.IsNullOrEmpty(Description))
                    {
                        String strCurrentDescription = (string)relationalTableRows[0]["ObjectDesc"];
                        if (Description.ToLower() != strCurrentDescription.ToLower())
                        {
                            relationalTableRows[0]["ObjectDesc"] = Description;
                        }
                    }
                    break;
                }
            }

            if (tableID == -1)
            {
                tableID = AddObject(tableName, Description, RelationalEnumerator.ObjectTypes.Table, connectionID);
            }

            return tableID;
        }

        /// <summary>
        /// Gets the id for the table, and creates it if it doesn't exist.
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public int GetTable(int connectionID, string tableName)
        {
            return GetTable(connectionID, tableName, string.Empty);
        }

        /// <summary>
        /// Searches for an existing ID for a Procedure, and if not found, adds a new entry.
        /// </summary>
        /// <param name="connectionID">The connection that this procedure should be found in</param>
        /// <param name="procName">The name of the procedure</param>
        /// <returns>the ID that belongs to the procedure</returns>
        public int GetProcedure(int connectionID, string procName)
        {
            int procID = -1; // assume no procedure will be found

            // get the procedures that correspond to this connection
            DataRow[] connectionChildren = objectDependenciesTable.Select(string.Format("SrcObjectKey = '{0}' AND DependencyType = '{1}'", connectionID, DependencyTypes.Containment));

            // see if any procedure has our name
            foreach (DataRow row in connectionChildren)
            {
                DataRow[] relationalProcedureRows = objectTable.Select(string.Format("ObjectKey = '{0}' AND ObjectTypeString = '{1}' AND ObjectName = '{2}'", row["TgtObjectKey"], RelationalEnumerator.ObjectTypes.Procedure, procName.Replace("'","''")));

                if (relationalProcedureRows.Length > 0)
                {
                    Debug.Assert(relationalProcedureRows.Length == 1); // should be only one procedure in a connection with the same name
                    procID = (int)relationalProcedureRows[0]["ObjectKey"];
                    break;
                }
            }

            if (procID == -1)
            {
                procID = AddObject(procName, string.Empty, RelationalEnumerator.ObjectTypes.Procedure, connectionID);
            }

            return procID;
        }

        /// <summary>
        /// Searches for an existing ID for a Function, and if not found, adds a new entry, with a description.
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="funcName"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        public int GetFunction(int connectionID, string funcName, string Description)
        {
            int funcID = -1; // assume no Function will be found

            // get the Functions that correspond to this connection
            DataRow[] connectionChildren = objectDependenciesTable.Select(string.Format("SrcObjectKey = '{0}' AND DependencyType = '{1}'", connectionID, DependencyTypes.Containment));

            // see if any Function has our name
            foreach (DataRow row in connectionChildren)
            {
                DataRow[] relationalFunctionRows = objectTable.Select(string.Format("ObjectKey = '{0}' AND ObjectTypeString = '{1}' AND ObjectName = '{2}'", row["TgtObjectKey"], RelationalEnumerator.ObjectTypes.Function, funcName.Replace("'", "''")));

                if (relationalFunctionRows.Length > 0)
                {
                    Debug.Assert(relationalFunctionRows.Length == 1); // should be only one Function in a connection with the same name
                    funcID = (int)relationalFunctionRows[0]["ObjectKey"];
                    break;
                }
            }

            if (funcID == -1)
            {
                funcID = AddObject(funcName, Description, RelationalEnumerator.ObjectTypes.Function, connectionID);
            }

            return funcID;
        }

        /// <summary>
        /// Searches for an existing ID for a Function, and if not found, adds a new entry.
        /// </summary>
        /// <param name="connectionID">The connection that this Function should be found in</param>
        /// <param name="funcName">The name of the Function</param>
        /// <returns>the ID that belongs to the Function</returns>
        public int GetFunction(int connectionID, string funcName)
        {
            return GetFunction(connectionID, funcName, string.Empty);
        }

        /// <summary>
        /// Returns the column id for column which belongs to something.
        /// </summary>
        /// <param name="parentID">The RepositoryID for the parent of this column</param>
        /// <param name="columnName">The name of the column as known by this parent</param>
        /// <returns>The ID of the column in the Repository</returns>
        public int GetColumn(int parentID, string columnName)
        {
            int columnID = -1;  // assume that the column hasn't been added yet

            // get the columns that correspond to the table
            DataRow[] connectionChildren = objectDependenciesTable.Select(string.Format("SrcObjectKey = '{0}' AND DependencyType = '{1}'", parentID, DependencyTypes.Containment));

            // see if any table has our name
            foreach (DataRow row in connectionChildren)
            {
                DataRow[] relationalTableRows = objectTable.Select(string.Format("ObjectKey = '{0}' AND ObjectTypeString = '{1}' AND ObjectName = '{2}'", row["TgtObjectKey"], ColumnEnumerator.ObjectTypes.Column, columnName.Replace("'", "''")));

                if (relationalTableRows.Length > 0)
                {
                    Debug.Assert(relationalTableRows.Length == 1); // should be only one table in a connection with the same name
                    columnID = (int)relationalTableRows[0]["ObjectKey"];
                    break;
                }
            }

            if (columnID == -1)
            {
                columnID = AddObject(columnName, string.Empty, ColumnEnumerator.ObjectTypes.Column, parentID);
            }

            return columnID;
        }

        /// <summary>
        /// Get object corresponding to the specified Report Name
        /// </summary>
        /// <param name="reportName">The name of the report</param>
        /// <param name="description">The description of the report</param>
        /// <param name="reportServerID">The ID of the reporting server</param>
        /// <returns></returns>
        public int GetReport(string reportName, string description, int reportServerID)
        {
            int reportID = -1;

            // get the objects that correspond to this reportServer
            DataRow[] reportChildren = objectDependenciesTable.Select(string.Format("SrcObjectKey = '{0}' AND DependencyType = '{1}'", reportServerID, DependencyTypes.Containment));

            // see if any report has our name
            foreach (DataRow row in reportChildren)
            {
                DataRow[] reportRows = objectTable.Select(string.Format("ObjectKey = '{0}' AND ObjectTypeString = '{1}' AND ObjectName = '{2}'", row["TgtObjectKey"], ReportEnumerator.ObjectTypes.Report, reportName.Replace("'", "''")));

                if (reportRows.Length > 0)
                {
                    Debug.Assert(reportRows.Length == 1); // should be only one Report in a Server with the same name
                    reportID = (int)reportRows[0]["ObjectKey"];
                    break;
                }
            }

            if (reportID == -1)
            {
                reportID = AddObject(reportName, description, ReportEnumerator.ObjectTypes.Report, reportServerID);
            }

            return reportID;

        }

        /// <summary>
        /// Get object corresponding to specified file.
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="hostMachineName">Host where the File Name points to the file</param>
        /// <returns></returns>
        public int GetFile(string fileName, string hostMachineName)
        {
            string fileMachineName = hostMachineName;
            if (fileName.StartsWith(@"\\"))
            {
                fileMachineName = fileName.Substring(2);
                if (fileMachineName.IndexOf('\\') > 0)
                    fileMachineName = fileMachineName.Substring(0, fileMachineName.IndexOf('\\'));
            }

            fileMachineName = fileMachineName.ToUpper(System.Globalization.CultureInfo.InvariantCulture);

            // find machine object
            int machineID = GetNamedObject(fileMachineName, FileEnumerator.ObjectTypes.Machine);
            if (machineID < 0)
            {
                // create if does not exist
                machineID = AddObject(fileMachineName, "Computer", FileEnumerator.ObjectTypes.Machine, 0);
            }

            // get all the files and check machine ownership
            int[] files = GetNamedObjects(fileName, FileEnumerator.ObjectTypes.File);
            foreach (int fileCandidate in files)
            {
                if (DoesDependencyExist(machineID, fileCandidate, DependencyTypes.Containment))
                    return fileCandidate;
            }

            // Create the file record
            int fileID = AddObject(fileName, "File", FileEnumerator.ObjectTypes.File, machineID);
            Debug.Assert(DoesDependencyExist(machineID, fileID, DependencyTypes.Containment));

            return fileID;
        }

        internal int GetDataSet(string sdsName, int reportServerID)
        {
            int dataSetID = -1;

            // get the objects that correspond to this reportServer
            DataRow[] reportChildren = objectDependenciesTable.Select(string.Format("SrcObjectKey = '{0}' AND DependencyType = '{1}'", reportServerID, DependencyTypes.Containment));

            // see if any report has our name
            foreach (DataRow row in reportChildren)
            {
                DataRow[] dataSetRows = objectTable.Select(string.Format("ObjectKey = '{0}' AND ObjectTypeString = '{1}' AND ObjectName = '{2}'", row["TgtObjectKey"], ReportEnumerator.ObjectTypes.DataSet, sdsName.Replace("'", "''")));

                if (dataSetRows.Length > 0)
                {
                    Debug.Assert(dataSetRows.Length == 1); // should be only one Shared Data Set in a Server with the same name
                    dataSetID = (int)dataSetRows[0]["ObjectKey"];
                    break;
                }
            }

            if (dataSetID == -1)
            {
                dataSetID = AddObject(sdsName, string.Empty, ReportEnumerator.ObjectTypes.DataSet, reportServerID);
            }

            return dataSetID;
        }
    }
}
