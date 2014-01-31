using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;
//using Microsoft.Data.Schema.ScriptDom;
//using Microsoft.Data.Schema.ScriptDom.Sql;
using System.IO;


namespace TSQLParser
{
    public class SqlStatement
    {
        private TSql100Parser parser;
        private TSqlFragment fragment;
        private bool reParseSQL;
        private IList<ParseError> _parseErrors;
        private Dictionary<string, Identifier> _identifiers;

        public bool quotedIdentifiers { get; set; }
        // public string sqlString { get; set; }  // Removed as ParseString should have this as a parameter...

        public List<string> parseErrors
        {
            get {
                List<string> result = new List<string>();
                if (_parseErrors != null)
                {
                    foreach (ParseError parseError in _parseErrors)
                    {
                        result.Add(String.Format("{0} {1}", parseError.Number, parseError.Message));
                    }
                }
                return result; 
            }
        }

        

        public SqlStatement()
        {
            quotedIdentifiers = true;
            fragment = null;
            _parseErrors = null;
            reParseSQL = true;
            // sqlString = string.Empty;
            parser = new TSql100Parser(quotedIdentifiers);
            _identifiers = new Dictionary<string, Identifier>();
        }


        public bool ParseReader(TextReader sqlFile)
        {
            // This sucks, but TextReader's are one way.
            string fileContent = sqlFile.ReadToEnd();
            if (ParseString(fileContent))
            {
                using (StringReader sr = new StringReader(fileContent))
                {
                    IList<ParseError> errors = null; 
                    //IList<TSqlParserToken> tokens = parser.GetTokenStream(sr, errors);
                    //ChildObjectName con = parser.ParseChildObjectName(sr, out errors);
                    StatementList sl = parser.ParseStatementList(sr, out errors);
                }
            }
            return ParseString(fileContent);
        }

        public bool ParseString(string sqlString)
        {
            int offsetIncrement = 0;
            while (reParseSQL)
            {
                reParseSQL = false;
                using (StringReader sr = new StringReader(sqlString))
                {
                    fragment = parser.Parse(sr, out _parseErrors);
                }
                if (_parseErrors != null && _parseErrors.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var error in _parseErrors)
                    {
                        sb.AppendLine(error.Message);
                        sb.AppendLine("offset " + error.Offset.ToString());
                        if (error.Number == 0010)  // What is TSP0010 now?
                        {
                            if (sqlString.Substring(error.Offset + offsetIncrement, 1) == "?")
                            {
                                reParseSQL = true;
                                sqlString = sqlString.Substring(0, error.Offset + offsetIncrement) + "@Special19695Guff " + sqlString.Substring(error.Offset + offsetIncrement + 1, sqlString.Length - error.Offset - 1 - offsetIncrement);
                                offsetIncrement += 17;
                            }
                        }
                    }
                    if (!reParseSQL)
                    {
                        return false;
//                        throw new ArgumentException("InvalidSQLScript", sb.ToString());
                    }
                }
            }
            reParseSQL = true;
            if (fragment is TSqlScript)
            {
                TSqlScript sqlScript = (TSqlScript)fragment;
                foreach (TSqlBatch sqlBatch in sqlScript.Batches)
                {
                    findIdentifiers(sqlBatch);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<string> getTableNames()
        {
            List<string> tableNames = new List<string>();
            if (fragment is TSqlScript)
            {
                foreach (Identifier foundID in _identifiers.Values)
                {
                    if (foundID.IdentifierType == Identifier.IdentifierEnum.Table)
                    {
                        tableNames.Add(foundID.ToString());
                    }
                }
            }
            else
            {
                throw new Exception("Unhandled Fragment Type " + fragment.GetType().FullName);
            }
            return tableNames;
        }

        public List<string> getTableNames(bool forceSchemaQualified)
        {
            List<string> tableNames = new List<string>();
            if (fragment is TSqlScript)
            {
                foreach (Identifier foundID in _identifiers.Values)
                {
                    if (foundID.IdentifierType == Identifier.IdentifierEnum.Table)
                    {
                        tableNames.Add(foundID.ToString(forceSchemaQualified));
                    }
                }
            }
            else
            {
                throw new Exception("Unhandled Fragment Type " + fragment.GetType().FullName);
            }
            return tableNames;
        }


        public List<string> getProcedureNames()
        {
            List<string> procNames = new List<string>();
            if (fragment is TSqlScript)
            {
                foreach (Identifier foundID in _identifiers.Values)
                {
                    if (foundID.IdentifierType == Identifier.IdentifierEnum.Procedure)
                    {
                        procNames.Add(foundID.ToString());
                    }
                }
            }
            else
            {
                throw new Exception("Unhandled Fragment Type " + fragment.GetType().FullName);
            }
            return procNames;
        }


        public List<string> getProcedureNames(bool forceSchemaQualified)
        {
            List<string> procNames = new List<string>();
            if (fragment is TSqlScript)
            {
                foreach (Identifier foundID in _identifiers.Values)
                {
                    if (foundID.IdentifierType == Identifier.IdentifierEnum.Procedure)
                    {
                        procNames.Add(foundID.ToString(forceSchemaQualified));
                    }
                }
            }
            else
            {
                throw new Exception("Unhandled Fragment Type " + fragment.GetType().FullName);
            }
            return procNames;
        }


        public List<string> getFunctionNames()
        {
            List<string> funcNames = new List<string>();
            if (fragment is TSqlScript)
            {
                foreach (Identifier foundID in _identifiers.Values)
                {
                    if (foundID.IdentifierType == Identifier.IdentifierEnum.Function)
                    {
                        funcNames.Add(foundID.ToString());
                    }
                }
            }
            else
            {
                throw new Exception("Unhandled Fragment Type " + fragment.GetType().FullName);
            }
            return funcNames;
        }


        public List<string> getFunctionNames(bool forceSchemaQualified)
        {
            List<string> funcNames = new List<string>();
            if (fragment is TSqlScript)
            {
                foreach (Identifier foundID in _identifiers.Values)
                {
                    if (foundID.IdentifierType == Identifier.IdentifierEnum.Function)
                    {
                        funcNames.Add(foundID.ToString(forceSchemaQualified));
                    }
                }
            }
            else
            {
                throw new Exception("Unhandled Fragment Type " + fragment.GetType().FullName);
            }
            return funcNames;
        }
        #region findIdentifiers

        private void findIdentifiers(TSqlBatch sqlBatch)
        {
            foreach (TSqlStatement sqlStatement in sqlBatch.Statements)
            {
                findIdentifiers(sqlStatement);
            }
        }

        private void findIdentifiers(TSqlStatement sqlStatement)
        {
            // ToDo: Finish handling of the different TSqlStatements below.  And do the drill through.
            switch (sqlStatement.GetType().FullName)
            {
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AddSignatureStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterApplicationRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAssemblyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAsymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAuthorizationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterBrokerPriorityStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterCertificateStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterCertificateStatementKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterCreateEndpointStatementBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterCreateServiceStatementBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterCredentialStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterCryptographicProviderStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseAddFileGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseAddFileStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseAuditSpecificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseCollateStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseEncryptionKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseModifyFileGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseModifyFileStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseModifyNameStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseRebuildLogStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseRemoveFileGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseRemoveFileStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseSetStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterEndpointStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterEventSessionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterEventSessionStatementType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterFullTextCatalogStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterFullTextIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterFullTextStopListStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterFunctionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterLoginAddDropCredentialStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterLoginEnableDisableStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterLoginOptionsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterLoginStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterMessageTypeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterPartitionFunctionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterPartitionSchemeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterQueueStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterRemoteServiceBindingStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterResourceGovernorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterResourcePoolStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterRouteStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterSchemaStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterServerAuditSpecificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterServerAuditStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterServerConfigurationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterServiceMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterServiceStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterSymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableAddTableElementStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableAlterColumnStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableChangeTrackingModificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableConstraintModificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableDropTableElementStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableRebuildStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableSetStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableSwitchStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableTriggerModificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTriggerStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterUserStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterViewStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterWorkloadGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterXmlSchemaCollectionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ApplicationRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AssemblyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditSpecificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupCertificateStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupDatabaseStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupRestoreMasterKeyStatementBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupServiceMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupTransactionLogStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BeginConversationTimerStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BeginDialogStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BeginEndBlockStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BeginTransactionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BreakStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BrokerPriorityStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BulkInsertBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BulkInsertStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CertificateStatementBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CheckpointStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CloseCursorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CloseMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CloseSymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CommitTransactionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ContinueStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateAggregateStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateApplicationRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateAssemblyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateAsymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateBrokerPriorityStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateCertificateStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateContractStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateCredentialStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateCryptographicProviderStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateDatabaseAuditSpecificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateDatabaseEncryptionKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateDatabaseStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateDefaultStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateEndpointStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateEventNotificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateEventSessionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateFullTextCatalogStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateFullTextIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateFullTextStopListStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateFunctionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateLoginStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateMessageTypeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreatePartitionFunctionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreatePartitionSchemeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateProcedureStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateQueueStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateRemoteServiceBindingStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateResourcePoolStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateRouteStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateRuleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateSchemaStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateServerAuditSpecificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateServerAuditStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateServiceStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateSpatialIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateStatisticsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateSymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateSynonymStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateTableStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateTriggerStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateTypeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateTypeTableStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateTypeUddtStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateTypeUdtStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateUserStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateViewStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateWorkloadGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateXmlIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateXmlSchemaCollectionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CredentialStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CursorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DatabaseEncryptionKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DataModificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DbccStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeallocateCursorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeclareTableStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeclareVariableStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DenyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DenyStatement80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropAggregateStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropApplicationRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropAssemblyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropAsymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropBrokerPriorityStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropCertificateStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropChildObjectsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropContractStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropCredentialStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropCryptographicProviderStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropDatabaseAuditSpecificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropDatabaseEncryptionKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropDatabaseStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropDefaultStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropEndpointStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropEventNotificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropEventSessionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropFullTextCatalogStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropFullTextIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropFullTextStopListStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropFunctionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropLoginStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropMessageTypeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropObjectsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropPartitionFunctionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropPartitionSchemeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropProcedureStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropQueueStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropRemoteServiceBindingStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropResourcePoolStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropRouteStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropRuleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropSchemaStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropServerAuditSpecificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropServerAuditStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropServiceStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropSignatureStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropStatisticsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropSymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropSynonymStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropTableStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropTriggerStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropTypeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropUnownedObjectStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropUserStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropViewStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropWorkloadGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropXmlSchemaCollectionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EnableDisableTriggerStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndConversationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventSessionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteAsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FetchCursorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextCatalogStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GetConversationGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GoToStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GrantStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GrantStatement80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InsertBulkStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InvalidSelectStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InvalidStatementList":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InvalidTSqlStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.KillQueryNotificationSubscriptionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.KillStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.KillStatsJobStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LabelStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LineNoStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MergeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MessageTypeStatementBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MoveConversationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenCursorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenSymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PredicateSetStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PrintStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProcedureStatementBody":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProcedureStatementBodyBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QueueStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RaiseErrorLegacyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RaiseErrorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ReadTextStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ReceiveStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ReconfigureStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RemoteServiceBindingStatementBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ResourcePoolStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RestoreMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RestoreServiceMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RestoreStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RestoreStatementKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ReturnStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RevertStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RevokeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RevokeStatement80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RollbackTransactionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RouteStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SaveTransactionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecurityStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecurityStatementBody80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SendStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ServerAuditStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetCommandStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetErrorLevelStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetIdentityInsertStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetOffsetsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetOnOffStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetRowCountStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetStatisticsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetTextSizeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetTransactionIsolationLevelStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetUserStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetVariableStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ShutdownStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SignatureStatementBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.StatementWithCommonTableExpressionsAndXmlNamespaces":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TextModificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TransactionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TriggerStatementBody":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TruncateTableStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TryCatchStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UpdateStatisticsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UpdateTextStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UserStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UseStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WaitForStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WaitForSupportedStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WhileStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WorkloadGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WriteTextStatement":
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.IfStatement":
//                    if (((IfStatement)sqlStatement).Predicate != null)
//                        findIdentifiers(((IfStatement)sqlStatement).Predicate);
                    if (((IfStatement)sqlStatement).ThenStatement != null)
                        findIdentifiers(((IfStatement)sqlStatement).ThenStatement);
                    if (((IfStatement)sqlStatement).ElseStatement != null)
                        findIdentifiers(((IfStatement)sqlStatement).ElseStatement);
                    break;
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SelectStatement":
                    findIdentifiers((SelectStatement)sqlStatement);
                    break;
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InsertStatement":
                    findIdentifiers((InsertStatement)sqlStatement);
                    break;
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UpdateStatement":
                    findIdentifiers((UpdateStatement)sqlStatement);
                    break;
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeleteStatement":
                    findIdentifiers((DeleteStatement)sqlStatement);
                    break;
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteStatement":
                    findIdentifiers((ExecuteStatement)sqlStatement);
                    break;
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterProcedureStatement":
                    findIdentifiers(((AlterProcedureStatement)sqlStatement).ProcedureReference.Name);
                    findIdentifiers(((AlterProcedureStatement)sqlStatement).StatementList);
                    break;
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FunctionStatementBody":
                    findIdentifiers(((FunctionStatementBody)sqlStatement).StatementList);
                    break;
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeclareCursorStatement":
                    findIdentifiers(((DeclareCursorStatement)sqlStatement).CursorDefinition.Select);
                    break;
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ViewStatementBody":
                    findIdentifiers(((ViewStatementBody)sqlStatement).SelectStatement);
                    break;

                default:
                    throw new Exception("Unhandled Statement Type in findIdentifiers(TSqlStatement sqlStatement) " + sqlStatement.GetType().FullName);
            }
        }

        private void findIdentifiers(StatementList statementList)
        {
            foreach (TSqlStatement statement in statementList.Statements)
            {
                findIdentifiers(statement);
            }
        }

        //private void findIdentifiers(ExecuteStatement execStatement)
        //{
        //    switch (execStatement.ExecuteSpecification.GetType().FullName)// .ExecutableEntity.GetType().FullName)
        //    {
        //        case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecutableProcedureReference":
        //            findIdentifiers(((ProcedureReference)((ExecutableProcedureReference)execStatement.ExecutableEntity).ProcedureReference).Name, Identifier.IdentifierEnum.Procedure);
        //            break;
        //        case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecutableStringList":
        //            foreach (Literal stringLiteral in ((ExecutableStringList)execStatement.ExecutableEntity).Strings)
        //            {
        //                findIdentifiers(stringLiteral);
        //            }
        //            break;
        //    }
        //}

        private void findIdentifiers(SelectStatement selectStatement)
        {
            if (selectStatement is StatementWithCtesAndXmlNamespaces) // StatementWithCommonTableExpressionsAndXmlNamespaces)
            {
                if (selectStatement.WithCtesAndXmlNamespaces != null) // .WithCommonTableExpressionsAndXmlNamespaces != null)
                {
                    //findIdentifiers(selectStatement.WithCtesAndXmlNamespaces);
                }
            }
            else
            {
                throw new Exception("Unhandled Statement Type in findIdentifiers(ExecuteStatement execStatement) " + selectStatement.GetType().FullName);
            }
            findIdentifiers(selectStatement.QueryExpression);
        }

        //private void findIdentifiers(WithCtesAndXmlNamespaces withCommonTableExpressionsAndXmlNamespaces)
        //{
        //    if (withCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions != null)
        //    {
        //        foreach (CommonTableExpression cte in withCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions)
        //        {
        //            findIdentifiers(cte.QueryExpression) // .Subquery.QueryExpression);
        //            Identifier foundID = new Identifier(string.Empty, string.Empty, string.Empty, cte.ExpressionName.Value, Identifier.IdentifierEnum.CommonTableExpression);
        //            if (!_identifiers.ContainsKey(foundID.ToString()))
        //            {
        //                _identifiers.Add(foundID.ToString(), foundID);
        //            }
        //            else
        //            {
        //                _identifiers[foundID.ToString()].IdentifierType = Identifier.IdentifierEnum.CommonTableExpression;
        //            }
        //        }
        //    }
        //    if (withCommonTableExpressionsAndXmlNamespaces.XmlNamespaces != null)
        //    {
        //        throw new Exception("Unhandled Statement Type in findIdentifiers(WithCommonTableExpressionsAndXmlNamespaces withCommonTableExpressionsAndXmlNamespaces) " + withCommonTableExpressionsAndXmlNamespaces.GetType().FullName);
        //    }

        //}

        #region QueryExpression
        private void findIdentifiers(QueryExpression queryExpression)
        {
            if (queryExpression is QuerySpecification)
            {
                findIdentifiers((QuerySpecification)queryExpression);
            }
            else if (queryExpression is BinaryQueryExpression)
            {
                findIdentifiers((BinaryQueryExpression)queryExpression);
            }
//            else if (queryExpression is QueryParenthesis)
//            {
//                findIdentifiers((QueryParenthesis)queryExpression);
//            }
            else
            {
                throw new Exception("Unhandled Statement Type in findIdentifiers(QueryExpression queryExpression) " + queryExpression.GetType().FullName);
            }
        }

        private void findIdentifiers(QuerySpecification querySpecification)
        {
            foreach (TableReference tableSource in querySpecification.FromClause.TableReferences)
            {
                findIdentifiers(tableSource);
            }
            if (querySpecification.WhereClause != null)
            {
                findIdentifiers(querySpecification.WhereClause);
            }
        }

        //private void findIdentifiers(QueryParenthesis queryParenthesis)
        //{
        //    findIdentifiers(queryParenthesis.QueryExpression);
        //}

        //private void findIdentifiers(BinaryQueryExpression queryExpression)
        //{
        //    findIdentifiers(queryExpression.FirstQueryExpression);
        //    findIdentifiers(queryExpression.SecondQueryExpression);
        //}
        #endregion

        //private void findIdentifiers(InsertStatement insertStatement)
        //{
        //    if (insertStatement.InsertSource is SelectStatement)
        //    {
        //        findIdentifiers((SelectStatement)insertStatement.InsertSource);
        //    }
        //    else if (insertStatement.InsertSource is ValuesInsertSource)
        //    {
        //        // Nothing to do here, as Values can't have Identifiers!
        //    }
        //    else if (insertStatement.InsertSource is ExecuteStatement)
        //    {
        //        findIdentifiers((ExecuteStatement)insertStatement.InsertSource);
        //    }
        //    else
        //    {
        //        throw new Exception("Unhandled Statement Type in findIdentifiers(InsertStatement insertStatement) " + insertStatement.GetType().FullName);
        //    }
        //    findIdentifiers(insertStatement.Target);

        //}

        //private void findIdentifiers(UpdateStatement updateStatement)
        //{
        //    if (updateStatement.WithCommonTableExpressionsAndXmlNamespaces != null)
        //    {
        //        findIdentifiers(updateStatement.WithCommonTableExpressionsAndXmlNamespaces);
        //    }

        //    if (updateStatement.Target != null)
        //    {
        //        findIdentifiers(updateStatement.Target);
        //    }

        //    foreach (TableSource tableSource in updateStatement.FromClauses)
        //    {
        //        findIdentifiers(tableSource);
        //    }
        //}

        //private void findIdentifiers(DeleteStatement deleteStatement)
        //{
        //    if (deleteStatement.WithCommonTableExpressionsAndXmlNamespaces != null)
        //    {
        //        findIdentifiers(deleteStatement.WithCommonTableExpressionsAndXmlNamespaces);
        //    }

        //    if (deleteStatement.Target != null)
        //    {
        //        findIdentifiers(deleteStatement.Target);
        //    }

        //    foreach (TableSource tableSource in deleteStatement.FromClauses)
        //    {
        //        findIdentifiers(tableSource);
        //    }

        //    if (deleteStatement.WhereClause != null)
        //    {
        //        findIdentifiers(deleteStatement.WhereClause);
        //    }
        //}

        private void findIdentifiers(WhereClause whereClause)
        {
            findIdentifiers(whereClause.SearchCondition);
        }

        private void findIdentifiers(BooleanExpression booleanExpression)
        {
            if (booleanExpression is BooleanComparisonExpression)
            {
                findIdentifiers((booleanExpression as BooleanComparisonExpression).FirstExpression);
                findIdentifiers((booleanExpression as BooleanComparisonExpression).SecondExpression);
            }
            else
            {
                throw new Exception("Unhandled Statement Type in findIdentifiers(BooleanExpression booleanExpression) " + booleanExpression.GetType().FullName);
            }
        }

        private void findIdentifiers(ScalarExpression scalarExpression)
        {
            throw new NotImplementedException();
        }

        //private void findIdentifiers(Expression expression)
        //{
            //if (expression is BinaryExpression)
            //{
            //    findIdentifiers(((BinaryExpression)expression).FirstExpression);
            //    findIdentifiers(((BinaryExpression)expression).SecondExpression);
            //}
            //else if (expression is ExistsPredicate)
            //{
            //    findIdentifiers(((ExistsPredicate)expression).Subquery.QueryExpression);
            //}
            //else if (expression is ExtractFromExpression)
            //{
            //    findIdentifiers(((ExtractFromExpression)expression).Expression);
            //}
            //else if (expression is InPredicate)
            //{
            //    if (((InPredicate)expression).Expression != null)
            //        findIdentifiers(((InPredicate)expression).Expression);
            //    if (((InPredicate)expression).Subquery != null)
            //        findIdentifiers(((InPredicate)expression).Subquery);
            //}
            //else if (expression is PrimaryExpression)
            //{
            //    findIdentifiers((PrimaryExpression)expression);
            //}
            //else if (expression is QueryExpression)
            //{
            //    findIdentifiers((QueryExpression)expression);
            //}
            //else if (expression is SubqueryComparisonPredicate)
            //{
            //    if (((SubqueryComparisonPredicate)expression).Expression != null)
            //        findIdentifiers(((SubqueryComparisonPredicate)expression).Expression);
            //    if (((SubqueryComparisonPredicate)expression).Subquery != null)
            //        findIdentifiers(((SubqueryComparisonPredicate)expression).Subquery);
            //}
            //else if (expression is TernaryExpression)
            //{
            //    if (((TernaryExpression)expression).FirstExpression != null)
            //        findIdentifiers(((TernaryExpression)expression).FirstExpression);
            //    if (((TernaryExpression)expression).SecondExpression != null)
            //        findIdentifiers(((TernaryExpression)expression).SecondExpression);
            //    if (((TernaryExpression)expression).ThirdExpression != null)
            //        findIdentifiers(((TernaryExpression)expression).ThirdExpression);
            //}
            //else if (expression is TSEqualCall)
            //{
            //    if (((TSEqualCall)expression).FirstExpression != null)
            //        findIdentifiers(((TSEqualCall)expression).FirstExpression);
            //    if (((TSEqualCall)expression).SecondExpression != null)
            //        findIdentifiers(((TSEqualCall)expression).SecondExpression);
            //}
            //else if (expression is UnaryExpression)
            //{
            //    if (((UnaryExpression)expression).Expression != null)
            //        findIdentifiers(((UnaryExpression)expression).Expression);
            //}
            ////else if (expression is UpdateCall)  // Not sure what this actually is
            ////{
            ////    findIdentifiers(((UpdateCall)expression).Identifier);
            ////}
            //else
        //    {
        //        // NOP
        //        // All the expressions that could return a table identifier have been handled above.
        //        switch (expression.GetType().FullName)
        //        {
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.EventDeclarationCompareFunctionParameter":
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextPredicate":
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.IdentityFunction":
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.InvalidExpression":
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.LikePredicate":
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.OdbcConvertSpecification":
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.SourceDeclaration":
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.UpdateCall":
        //                break;
        //            default: throw new Exception("Unhandled Statement Type in findIdentifiers(Expression expression) " + expression.GetType().FullName);
        //        }

        //    }
        //}

        /// <summary>
        /// Find identifiers in a PrimaryExpression
        /// </summary>
        /// <param name="expression">The PrimaryExpression to analyse</param>
        //private void findIdentifiers(PrimaryExpression expression)
        //{
        //    if (expression is FunctionCall)
        //    {
        //        findIdentifiers((FunctionCall)expression);
        //    }
        //    else if (expression is Subquery)
        //    {
        //        findIdentifiers(((Subquery)expression).QueryExpression);
        //    }
        //    else if (expression is CaseExpression)
        //    {
        //        findIdentifiers((CaseExpression)expression);
        //    }
        //    else if (expression is ParenthesisExpression)
        //    {
        //        findIdentifiers(((ParenthesisExpression)expression).Expression);
        //    }
        //    else
        //    {
        //        switch (expression.GetType().FullName)
        //        {
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.CastCall":
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.CoalesceExpression":
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.Column":  // Need to handle this in the future to store Columns!
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.ConvertCall":
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.Literal":
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.NullIfExpression":
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.UserDefinedTypePropertyAccess":
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.LeftFunctionCall":  // Can't find a name in this object
        //            case "Microsoft.SqlServer.TransactSql.ScriptDom.RightFunctionCall":  // Can't find a name in this object
        //                break;
        //            default: throw new Exception("Unhandled Statement Type in findIdentifiers(PrimaryExpression expression) " + expression.GetType().FullName);
        //        }
        //    }
        //    // Should handle Functions to save these as Identifiers...
        //}

        //private void findIdentifiers(CaseExpression caseExpression)
        //{
        //    if (caseExpression.ElseExpression != null)
        //    {
        //        findIdentifiers(caseExpression.ElseExpression);
        //    }

        //    if (caseExpression.InputExpression != null)
        //    {
        //        findIdentifiers(caseExpression.InputExpression);
        //    }
        //    foreach (WhenClause whenClause in caseExpression.WhenClauses)
        //    {
        //        if (whenClause.ThenExpression != null)
        //            findIdentifiers(whenClause.ThenExpression);
        //        if (whenClause.WhenExpression != null)
        //            findIdentifiers(whenClause.WhenExpression);
        //    }
        //}


        //private void findIdentifiers(FunctionCall functionCall)
        //{
        //    Identifier foundID;

        //    if (functionCall.CallTarget != null)
        //    {
        //        foundID = new Identifier(string.Empty, string.Empty, ((IdentifiersCallTarget)functionCall.CallTarget).Identifiers[0].Value, functionCall.FunctionName.Value, Identifier.IdentifierEnum.Function);
        //    }
        //    else
        //    {
        //        foundID = new Identifier(String.Empty, String.Empty, String.Empty, functionCall.FunctionName.Value, Identifier.IdentifierEnum.Function);
        //    }
        //    if (!_identifiers.ContainsKey(foundID.ToString()))
        //    {
        //        _identifiers.Add(foundID.ToString(), foundID);
        //    }
        //}

        private void findIdentifiers(NamedTableReference tableSource)
        {
            findIdentifiers(tableSource.SchemaObject);
        }

        private void findIdentifiers(TableReference tableSource)
        {
            switch (tableSource.GetType().FullName)
            {
                case "Microsoft.SqlServer.TransactSql.ScriptDom.NamedTableReference":
                    findIdentifiers((tableSource as NamedTableReference).SchemaObject);
                    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.JoinParenthesis":
                //    findIdentifiers(((JoinParenthesis)tableSource).Join);
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.OdbcQualifiedJoin":
                //    findIdentifiers(((OdbcQualifiedJoin)tableSource).TableSource);
                //    break;
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QualifiedJoin":
                    findIdentifiers(((QualifiedJoin)tableSource).FirstTableReference);
                    findIdentifiers(((QualifiedJoin)tableSource).SecondTableReference);
                    findIdentifiers(((QualifiedJoin)tableSource).SearchCondition);
                    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.UnqualifiedJoin":
                //    findIdentifiers(((UnqualifiedJoin)tableSource).FirstTableSource);
                //    findIdentifiers(((UnqualifiedJoin)tableSource).SecondTableSource);
                //    break;

                //case "Microsoft.SqlServer.TransactSql.ScriptDom.TableSourceWithAlias":
                //    // NOP (Not interested in the Alias)
                //    break;
                //// The following are all based on TableSourceWithAlias
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.AdhocTableSource":
                //    // ToDo:  Handle This?
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.BuiltInFunctionTableSource":
                //    // findIdentifiers(((BuiltInFunctionTableSource)tableSource).Name);
                //    // Not interested in Builtin Functions!
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextTableSource":
                //    findIdentifiers(((FullTextTableSource)tableSource).TableName);
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.InternalOpenRowset":
                //    // NOP (No useful data)
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenQueryTableSource":
                //    findIdentifiers((OpenQueryTableSource)tableSource);
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenRowsetTableSource":
                //    findIdentifiers(((OpenRowsetTableSource)tableSource).Query);
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenXmlTableSource":
                //    findIdentifiers(((OpenXmlTableSource)tableSource).TableName);
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.PivotedTableSource":
                //    findIdentifiers(((PivotedTableSource)tableSource).TableSource);
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.UnpivotedTableSource":
                //    findIdentifiers(((UnpivotedTableSource)tableSource).TableSource);
                //    break;

                //case "Microsoft.SqlServer.TransactSql.ScriptDom.TableSourceWithAliasAndColumns":
                //    // See Below
                //    break;
                //// The following are all based on TableSourceWithAliasAndColumns
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.BulkOpenRowset":
                //    // NOP (Filename)
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.ChangeTableChangesTableSource":
                //    findIdentifiers(((ChangeTableChangesTableSource)tableSource).Target);
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.ChangeTableVersionTableSource":
                //    findIdentifiers(((ChangeTableVersionTableSource)tableSource).Target);
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.DataModificationStatementTableSource":
                //    findIdentifiers(((DataModificationStatementTableSource)tableSource).Statement);
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.InlineDerivedTable":
                //    // NOP
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.QueryDerivedTable":
                //    findIdentifiers(((QueryDerivedTable)tableSource).Subquery);
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.SchemaObjectTableSource":
                //    findIdentifiers(((SchemaObjectTableSource)tableSource).SchemaObject);
                //    break;
                //case "Microsoft.SqlServer.TransactSql.ScriptDom.VariableTableSource":
                //    // Not intested in Variables...
                //    break;


                default:
                    throw new Exception("Unhandled Statement Type in findIdentifiers(TableSource tableSource) " + tableSource.GetType().FullName);
            }
        }



        //private void findIdentifiers(DataModificationTarget dataTarget)
        //{
        //    switch (dataTarget.GetType().FullName)
        //    {
        //        case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenRowsetDataModificationTarget":
        //            findIdentifiers(((OpenRowsetDataModificationTarget)dataTarget).OpenRowset);
        //            break;
        //        case "Microsoft.SqlServer.TransactSql.ScriptDom.SchemaObjectDataModificationTarget":
        //            findIdentifiers(((SchemaObjectDataModificationTarget)dataTarget).SchemaObject, Identifier.IdentifierEnum.Table);
        //            break;
        //        case "Microsoft.SqlServer.TransactSql.ScriptDom.VariableDataModificationTarget":
        //            // NOP Not interested in Variables.
        //            break;
        //        default:
        //            throw new Exception("Unhandled Statement Type in findIdentifiers(DataModificationTarget dataTarget) " + dataTarget.GetType().FullName);
        //    }
        //}


        //private void findIdentifiers(SchemaObjectTableSource schemaObjectTableSource)
        //{
        //    if (schemaObjectTableSource.ParametersUsed)
        //        findIdentifiers(schemaObjectTableSource.SchemaObject, Identifier.IdentifierEnum.Function);
        //    else
        //        findIdentifiers(schemaObjectTableSource.SchemaObject, Identifier.IdentifierEnum.Table);
        //}

        //private void findIdentifiers(Microsoft.SqlServer.TransactSql.ScriptDom.Identifier identifier)
        //{
        //    Identifier foundID;
        //    foundID = new Identifier(string.Empty, string.Empty, string.Empty, identifier.Value, Identifier.IdentifierEnum.Function);
        //}

        private void findIdentifiers(SchemaObjectName schemaObject)
        {
            Identifier foundID;
            switch (schemaObject.Identifiers.Count)
            {
                case 4: foundID = new Identifier(schemaObject.ServerIdentifier.Value, schemaObject.DatabaseIdentifier.Value, schemaObject.SchemaIdentifier.Value, schemaObject.BaseIdentifier.Value, Identifier.IdentifierEnum.Table);
                    break;
                case 3: foundID = new Identifier(string.Empty, schemaObject.DatabaseIdentifier.Value, schemaObject.SchemaIdentifier.Value, schemaObject.BaseIdentifier.Value, Identifier.IdentifierEnum.Table);
                    break;
                case 2: foundID = new Identifier(string.Empty, string.Empty, schemaObject.SchemaIdentifier.Value, schemaObject.BaseIdentifier.Value, Identifier.IdentifierEnum.Table);
                    break;
                default: foundID = new Identifier(string.Empty, string.Empty, string.Empty, schemaObject.BaseIdentifier.Value, Identifier.IdentifierEnum.Table);
                    break;
            }
            if (!_identifiers.ContainsKey(foundID.ToString()))
            {
                _identifiers.Add(foundID.ToString(), foundID);
            }
        }

        private void findIdentifiers(SchemaObjectName schemaObject, Identifier.IdentifierEnum objectType)
        {
            Identifier foundID;
            switch (schemaObject.Identifiers.Count)
            {
                case 4: foundID = new Identifier(schemaObject.ServerIdentifier.Value, schemaObject.DatabaseIdentifier.Value, schemaObject.SchemaIdentifier.Value, schemaObject.BaseIdentifier.Value, objectType);
                    break;
                case 3: foundID = new Identifier(string.Empty, schemaObject.DatabaseIdentifier.Value, schemaObject.SchemaIdentifier.Value, schemaObject.BaseIdentifier.Value, objectType);
                    break;
                case 2: foundID = new Identifier(string.Empty, string.Empty, schemaObject.SchemaIdentifier.Value, schemaObject.BaseIdentifier.Value, objectType);
                    break;
                default: foundID = new Identifier(string.Empty, string.Empty, string.Empty, schemaObject.BaseIdentifier.Value, objectType);
                    break;
            }
            if (!_identifiers.ContainsKey(foundID.ToString()))
            {
                _identifiers.Add(foundID.ToString(), foundID);
            }
        }
        #endregion
    }
}
