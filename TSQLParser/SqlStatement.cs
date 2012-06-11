using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Schema.ScriptDom;
using Microsoft.Data.Schema.ScriptDom.Sql;
using System.IO;


namespace TSQLParser
{
    public class SqlStatement
    {
        private TSql100Parser parser;
        private IScriptFragment fragment;
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
                        result.Add(String.Format("{0} {1}", parseError.Identifier, parseError.Message));
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
                        if (error.Identifier == "TSP0010")
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
                case "Microsoft.Data.Schema.ScriptDom.Sql.AddSignatureStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterApplicationRoleStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterAssemblyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterAsymmetricKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterAuthorizationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterBrokerPriorityStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterCertificateStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterCertificateStatementKind":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterCreateEndpointStatementBase":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterCreateServiceStatementBase":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterCredentialStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterCryptographicProviderStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterDatabaseAddFileGroupStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterDatabaseAddFileStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterDatabaseAuditSpecificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterDatabaseCollateStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterDatabaseEncryptionKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterDatabaseModifyFileGroupStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterDatabaseModifyFileStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterDatabaseModifyNameStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterDatabaseRebuildLogStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterDatabaseRemoveFileGroupStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterDatabaseRemoveFileStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterDatabaseSetStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterDatabaseStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterEndpointStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterEventSessionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterEventSessionStatementType":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterFullTextCatalogStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterFullTextIndexStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterFullTextStopListStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterFunctionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterIndexStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterLoginAddDropCredentialStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterLoginEnableDisableStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterLoginOptionsStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterLoginStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterMasterKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterMessageTypeStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterPartitionFunctionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterPartitionSchemeStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterQueueStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterRemoteServiceBindingStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterResourceGovernorStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterResourcePoolStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterRoleStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterRouteStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterSchemaStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterServerAuditSpecificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterServerAuditStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterServerConfigurationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterServiceMasterKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterServiceStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterSymmetricKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterTableAddTableElementStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterTableAlterColumnStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterTableChangeTrackingModificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterTableConstraintModificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterTableDropTableElementStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterTableRebuildStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterTableSetStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterTableStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterTableSwitchStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterTableTriggerModificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterTriggerStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterUserStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterViewStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterWorkloadGroupStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterXmlSchemaCollectionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.ApplicationRoleStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AssemblyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.AuditSpecificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BackupCertificateStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BackupDatabaseStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BackupMasterKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BackupRestoreMasterKeyStatementBase":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BackupServiceMasterKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BackupStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BackupTransactionLogStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BeginConversationTimerStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BeginDialogStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BeginEndBlockStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BeginTransactionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BreakStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BrokerPriorityStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BulkInsertBase":
                case "Microsoft.Data.Schema.ScriptDom.Sql.BulkInsertStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CertificateStatementBase":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CheckpointStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CloseCursorStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CloseMasterKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CloseSymmetricKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CommitTransactionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.ContinueStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateAggregateStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateApplicationRoleStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateAssemblyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateAsymmetricKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateBrokerPriorityStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateCertificateStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateContractStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateCredentialStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateCryptographicProviderStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateDatabaseAuditSpecificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateDatabaseEncryptionKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateDatabaseStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateDefaultStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateEndpointStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateEventNotificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateEventSessionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateFullTextCatalogStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateFullTextIndexStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateFullTextStopListStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateFunctionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateIndexStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateLoginStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateMasterKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateMessageTypeStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreatePartitionFunctionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreatePartitionSchemeStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateProcedureStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateQueueStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateRemoteServiceBindingStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateResourcePoolStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateRoleStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateRouteStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateRuleStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateSchemaStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateServerAuditSpecificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateServerAuditStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateServiceStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateSpatialIndexStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateStatisticsStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateSymmetricKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateSynonymStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateTableStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateTriggerStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateTypeStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateTypeTableStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateTypeUddtStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateTypeUdtStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateUserStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateViewStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateWorkloadGroupStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateXmlIndexStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CreateXmlSchemaCollectionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CredentialStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.CursorStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DatabaseEncryptionKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DataModificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DbccStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DeallocateCursorStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DeclareTableStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DeclareVariableStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DenyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DenyStatement80":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropAggregateStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropApplicationRoleStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropAssemblyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropAsymmetricKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropBrokerPriorityStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropCertificateStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropChildObjectsStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropContractStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropCredentialStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropCryptographicProviderStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropDatabaseAuditSpecificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropDatabaseEncryptionKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropDatabaseStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropDefaultStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropEndpointStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropEventNotificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropEventSessionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropFullTextCatalogStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropFullTextIndexStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropFullTextStopListStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropFunctionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropIndexStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropLoginStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropMasterKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropMessageTypeStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropObjectsStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropPartitionFunctionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropPartitionSchemeStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropProcedureStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropQueueStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropRemoteServiceBindingStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropResourcePoolStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropRoleStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropRouteStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropRuleStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropSchemaStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropServerAuditSpecificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropServerAuditStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropServiceStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropSignatureStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropStatisticsStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropSymmetricKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropSynonymStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropTableStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropTriggerStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropTypeStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropUnownedObjectStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropUserStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropViewStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropWorkloadGroupStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.DropXmlSchemaCollectionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.EnableDisableTriggerStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.EndConversationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.EventSessionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.ExecuteAsStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.FetchCursorStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.FullTextCatalogStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.GetConversationGroupStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.GoToStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.GrantStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.GrantStatement80":
                case "Microsoft.Data.Schema.ScriptDom.Sql.IndexStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.InsertBulkStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.InvalidSelectStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.InvalidStatementList":
                case "Microsoft.Data.Schema.ScriptDom.Sql.InvalidTSqlStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.KillQueryNotificationSubscriptionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.KillStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.KillStatsJobStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.LabelStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.LineNoStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.MasterKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.MergeStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.MessageTypeStatementBase":
                case "Microsoft.Data.Schema.ScriptDom.Sql.MoveConversationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.OpenCursorStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.OpenMasterKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.OpenSymmetricKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.PredicateSetStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.PrintStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.ProcedureStatementBody":
                case "Microsoft.Data.Schema.ScriptDom.Sql.ProcedureStatementBodyBase":
                case "Microsoft.Data.Schema.ScriptDom.Sql.QueueStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.RaiseErrorLegacyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.RaiseErrorStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.ReadTextStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.ReceiveStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.ReconfigureStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.RemoteServiceBindingStatementBase":
                case "Microsoft.Data.Schema.ScriptDom.Sql.ResourcePoolStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.RestoreMasterKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.RestoreServiceMasterKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.RestoreStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.RestoreStatementKind":
                case "Microsoft.Data.Schema.ScriptDom.Sql.ReturnStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.RevertStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.RevokeStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.RevokeStatement80":
                case "Microsoft.Data.Schema.ScriptDom.Sql.RoleStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.RollbackTransactionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.RouteStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SaveTransactionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SecurityStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SecurityStatementBody80":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SendStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.ServerAuditStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SetCommandStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SetErrorLevelStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SetIdentityInsertStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SetOffsetsStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SetOnOffStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SetRowCountStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SetStatisticsStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SetTextSizeStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SetTransactionIsolationLevelStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SetUserStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SetVariableStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.ShutdownStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SignatureStatementBase":
                case "Microsoft.Data.Schema.ScriptDom.Sql.StatementWithCommonTableExpressionsAndXmlNamespaces":
                case "Microsoft.Data.Schema.ScriptDom.Sql.SymmetricKeyStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.TextModificationStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.TransactionStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.TriggerStatementBody":
                case "Microsoft.Data.Schema.ScriptDom.Sql.TruncateTableStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.TryCatchStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.UpdateStatisticsStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.UpdateTextStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.UserStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.UseStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.WaitForStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.WaitForSupportedStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.WhileStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.WorkloadGroupStatement":
                case "Microsoft.Data.Schema.ScriptDom.Sql.WriteTextStatement":
                    break;

                case "Microsoft.Data.Schema.ScriptDom.Sql.IfStatement":
                    if (((IfStatement)sqlStatement).Predicate != null)
                        findIdentifiers(((IfStatement)sqlStatement).Predicate);
                    if (((IfStatement)sqlStatement).ThenStatement != null)
                        findIdentifiers(((IfStatement)sqlStatement).ThenStatement);
                    if (((IfStatement)sqlStatement).ElseStatement != null)
                        findIdentifiers(((IfStatement)sqlStatement).ElseStatement);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.SelectStatement":
                    findIdentifiers((SelectStatement)sqlStatement);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.InsertStatement":
                    findIdentifiers((InsertStatement)sqlStatement);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.UpdateStatement":
                    findIdentifiers((UpdateStatement)sqlStatement);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.DeleteStatement":
                    findIdentifiers((DeleteStatement)sqlStatement);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.ExecuteStatement":
                    findIdentifiers((ExecuteStatement)sqlStatement);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.AlterProcedureStatement":
                    findIdentifiers(((AlterProcedureStatement)sqlStatement).ProcedureReference.Name);
                    findIdentifiers(((AlterProcedureStatement)sqlStatement).StatementList);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.FunctionStatementBody":
                    findIdentifiers(((FunctionStatementBody)sqlStatement).StatementList);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.DeclareCursorStatement":
                    findIdentifiers(((DeclareCursorStatement)sqlStatement).CursorDefinition.Select);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.ViewStatementBody":
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

        private void findIdentifiers(ExecuteStatement execStatement)
        {
            switch (execStatement.ExecutableEntity.GetType().FullName)
            {
                case "Microsoft.Data.Schema.ScriptDom.Sql.ExecutableProcedureReference":
                    findIdentifiers(((ProcedureReference)((ExecutableProcedureReference)execStatement.ExecutableEntity).ProcedureReference).Name, Identifier.IdentifierEnum.Procedure);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.ExecutableStringList":
                    foreach (Literal stringLiteral in ((ExecutableStringList)execStatement.ExecutableEntity).Strings)
                    {
                        findIdentifiers(stringLiteral);
                    }
                    break;
            }
        }

        private void findIdentifiers(SelectStatement selectStatement)
        {
            if (selectStatement is StatementWithCommonTableExpressionsAndXmlNamespaces)
            {
                if (selectStatement.WithCommonTableExpressionsAndXmlNamespaces != null)
                {
                    findIdentifiers(selectStatement.WithCommonTableExpressionsAndXmlNamespaces);
                }
            }
            else
            {
                throw new Exception("Unhandled Statement Type in findIdentifiers(ExecuteStatement execStatement) " + selectStatement.GetType().FullName);
            }
            findIdentifiers(selectStatement.QueryExpression);

        }

        private void findIdentifiers(WithCommonTableExpressionsAndXmlNamespaces withCommonTableExpressionsAndXmlNamespaces)
        {
            if (withCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions != null)
            {
                foreach (CommonTableExpression cte in withCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions)
                {
                    findIdentifiers(cte.Subquery.QueryExpression);
                    Identifier foundID = new Identifier(string.Empty, string.Empty, string.Empty, cte.ExpressionName.Value, Identifier.IdentifierEnum.CommonTableExpression);
                    if (!_identifiers.ContainsKey(foundID.ToString()))
                    {
                        _identifiers.Add(foundID.ToString(), foundID);
                    }
                    else
                    {
                        _identifiers[foundID.ToString()].IdentifierType = Identifier.IdentifierEnum.CommonTableExpression;
                    }
                }
            }
            if (withCommonTableExpressionsAndXmlNamespaces.XmlNamespaces != null)
            {
                throw new Exception("Unhandled Statement Type in findIdentifiers(WithCommonTableExpressionsAndXmlNamespaces withCommonTableExpressionsAndXmlNamespaces) " + withCommonTableExpressionsAndXmlNamespaces.GetType().FullName);
            }

        }

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
            else if (queryExpression is QueryParenthesis)
            {
                findIdentifiers((QueryParenthesis)queryExpression);
            }
            else
            {
                throw new Exception("Unhandled Statement Type in findIdentifiers(QueryExpression queryExpression) " + queryExpression.GetType().FullName);
            }
        }

        private void findIdentifiers(QuerySpecification querySpecification)
        {
            foreach (TableSource tableSource in querySpecification.FromClauses)
            {
                findIdentifiers(tableSource);
            }
            if (querySpecification.WhereClause != null)
            {
                findIdentifiers(querySpecification.WhereClause);
            }
        }

        private void findIdentifiers(QueryParenthesis queryParenthesis)
        {
            findIdentifiers(queryParenthesis.QueryExpression);
        }

        private void findIdentifiers(BinaryQueryExpression queryExpression)
        {
            findIdentifiers(queryExpression.FirstQueryExpression);
            findIdentifiers(queryExpression.SecondQueryExpression);
        }
        #endregion

        private void findIdentifiers(InsertStatement insertStatement)
        {
            if (insertStatement.InsertSource is SelectStatement)
            {
                findIdentifiers((SelectStatement)insertStatement.InsertSource);
            }
            else if (insertStatement.InsertSource is ValuesInsertSource)
            {
                // Nothing to do here, as Values can't have Identifiers!
            }
            else if (insertStatement.InsertSource is ExecuteStatement)
            {
                findIdentifiers((ExecuteStatement)insertStatement.InsertSource);
            }
            else
            {
                throw new Exception("Unhandled Statement Type in findIdentifiers(InsertStatement insertStatement) " + insertStatement.GetType().FullName);
            }
            findIdentifiers(insertStatement.Target);

        }

        private void findIdentifiers(UpdateStatement updateStatement)
        {
            if (updateStatement.WithCommonTableExpressionsAndXmlNamespaces != null)
            {
                findIdentifiers(updateStatement.WithCommonTableExpressionsAndXmlNamespaces);
            }

            if (updateStatement.Target != null)
            {
                findIdentifiers(updateStatement.Target);
            }

            foreach (TableSource tableSource in updateStatement.FromClauses)
            {
                findIdentifiers(tableSource);
            }
        }

        private void findIdentifiers(DeleteStatement deleteStatement)
        {
            if (deleteStatement.WithCommonTableExpressionsAndXmlNamespaces != null)
            {
                findIdentifiers(deleteStatement.WithCommonTableExpressionsAndXmlNamespaces);
            }

            if (deleteStatement.Target != null)
            {
                findIdentifiers(deleteStatement.Target);
            }

            foreach (TableSource tableSource in deleteStatement.FromClauses)
            {
                findIdentifiers(tableSource);
            }

            if (deleteStatement.WhereClause != null)
            {
                findIdentifiers(deleteStatement.WhereClause);
            }
        }

        private void findIdentifiers(WhereClause whereClause)
        {
            findIdentifiers(whereClause.SearchCondition);
        }

        private void findIdentifiers(Expression expression)
        {
            if (expression is BinaryExpression)
            {
                findIdentifiers(((BinaryExpression)expression).FirstExpression);
                findIdentifiers(((BinaryExpression)expression).SecondExpression);
            }
            else if (expression is ExistsPredicate)
            {
                findIdentifiers(((ExistsPredicate)expression).Subquery.QueryExpression);
            }
            else if (expression is ExtractFromExpression)
            {
                findIdentifiers(((ExtractFromExpression)expression).Expression);
            }
            else if (expression is InPredicate)
            {
                if (((InPredicate)expression).Expression != null)
                    findIdentifiers(((InPredicate)expression).Expression);
                if (((InPredicate)expression).Subquery != null)
                    findIdentifiers(((InPredicate)expression).Subquery);
            }
            else if (expression is PrimaryExpression)
            {
                findIdentifiers((PrimaryExpression)expression);
            }
            else if (expression is QueryExpression)
            {
                findIdentifiers((QueryExpression)expression);
            }
            else if (expression is SubqueryComparisonPredicate)
            {
                if (((SubqueryComparisonPredicate)expression).Expression != null)
                    findIdentifiers(((SubqueryComparisonPredicate)expression).Expression);
                if (((SubqueryComparisonPredicate)expression).Subquery != null)
                    findIdentifiers(((SubqueryComparisonPredicate)expression).Subquery);
            }
            else if (expression is TernaryExpression)
            {
                if (((TernaryExpression)expression).FirstExpression != null)
                    findIdentifiers(((TernaryExpression)expression).FirstExpression);
                if (((TernaryExpression)expression).SecondExpression != null)
                    findIdentifiers(((TernaryExpression)expression).SecondExpression);
                if (((TernaryExpression)expression).ThirdExpression != null)
                    findIdentifiers(((TernaryExpression)expression).ThirdExpression);
            }
            else if (expression is TSEqualCall)
            {
                if (((TSEqualCall)expression).FirstExpression != null)
                    findIdentifiers(((TSEqualCall)expression).FirstExpression);
                if (((TSEqualCall)expression).SecondExpression != null)
                    findIdentifiers(((TSEqualCall)expression).SecondExpression);
            }
            else if (expression is UnaryExpression)
            {
                if (((UnaryExpression)expression).Expression != null)
                    findIdentifiers(((UnaryExpression)expression).Expression);
            }
            //else if (expression is UpdateCall)  // Not sure what this actually is
            //{
            //    findIdentifiers(((UpdateCall)expression).Identifier);
            //}
            else
            {
                // NOP
                // All the expressions that could return a table identifier have been handled above.
                switch (expression.GetType().FullName)
                {
                    case "Microsoft.Data.Schema.ScriptDom.Sql.EventDeclarationCompareFunctionParameter":
                    case "Microsoft.Data.Schema.ScriptDom.Sql.FullTextPredicate":
                    case "Microsoft.Data.Schema.ScriptDom.Sql.IdentityFunction":
                    case "Microsoft.Data.Schema.ScriptDom.Sql.InvalidExpression":
                    case "Microsoft.Data.Schema.ScriptDom.Sql.LikePredicate":
                    case "Microsoft.Data.Schema.ScriptDom.Sql.OdbcConvertSpecification":
                    case "Microsoft.Data.Schema.ScriptDom.Sql.SourceDeclaration":
                    case "Microsoft.Data.Schema.ScriptDom.Sql.UpdateCall":
                        break;
                    default: throw new Exception("Unhandled Statement Type in findIdentifiers(Expression expression) " + expression.GetType().FullName);
                }

            }
        }

        /// <summary>
        /// Find identifiers in a PrimaryExpression
        /// </summary>
        /// <param name="expression">The PrimaryExpression to analyse</param>
        private void findIdentifiers(PrimaryExpression expression)
        {
            if (expression is FunctionCall)
            {
                findIdentifiers((FunctionCall)expression);
            }
            else if (expression is Subquery)
            {
                findIdentifiers(((Subquery)expression).QueryExpression);
            }
            else if (expression is CaseExpression)
            {
                findIdentifiers((CaseExpression)expression);
            }
            else if (expression is ParenthesisExpression)
            {
                findIdentifiers(((ParenthesisExpression)expression).Expression);
            }
            else
            {
                switch (expression.GetType().FullName)
                {
                    case "Microsoft.Data.Schema.ScriptDom.Sql.CastCall":
                    case "Microsoft.Data.Schema.ScriptDom.Sql.CoalesceExpression":
                    case "Microsoft.Data.Schema.ScriptDom.Sql.Column":  // Need to handle this in the future to store Columns!
                    case "Microsoft.Data.Schema.ScriptDom.Sql.ConvertCall":
                    case "Microsoft.Data.Schema.ScriptDom.Sql.Literal":
                    case "Microsoft.Data.Schema.ScriptDom.Sql.NullIfExpression":
                    case "Microsoft.Data.Schema.ScriptDom.Sql.UserDefinedTypePropertyAccess":
                    case "Microsoft.Data.Schema.ScriptDom.Sql.LeftFunctionCall":  // Can't find a name in this object
                    case "Microsoft.Data.Schema.ScriptDom.Sql.RightFunctionCall":  // Can't find a name in this object
                        break;
                    default: throw new Exception("Unhandled Statement Type in findIdentifiers(PrimaryExpression expression) " + expression.GetType().FullName);
                }
            }
            // Should handle Functions to save these as Identifiers...
        }

        private void findIdentifiers(CaseExpression caseExpression)
        {
            if (caseExpression.ElseExpression != null)
            {
                findIdentifiers(caseExpression.ElseExpression);
            }

            if (caseExpression.InputExpression != null)
            {
                findIdentifiers(caseExpression.InputExpression);
            }
            foreach (WhenClause whenClause in caseExpression.WhenClauses)
            {
                if (whenClause.ThenExpression != null)
                    findIdentifiers(whenClause.ThenExpression);
                if (whenClause.WhenExpression != null)
                    findIdentifiers(whenClause.WhenExpression);
            }
        }


        private void findIdentifiers(FunctionCall functionCall)
        {
            Identifier foundID;

            if (functionCall.CallTarget != null)
            {
                foundID = new Identifier(string.Empty, string.Empty, ((IdentifiersCallTarget)functionCall.CallTarget).Identifiers[0].Value, functionCall.FunctionName.Value, Identifier.IdentifierEnum.Function);
            }
            else
            {
                foundID = new Identifier(String.Empty, String.Empty, String.Empty, functionCall.FunctionName.Value, Identifier.IdentifierEnum.Function);
            }
            if (!_identifiers.ContainsKey(foundID.ToString()))
            {
                _identifiers.Add(foundID.ToString(), foundID);
            }
        }

        private void findIdentifiers(TableSource tableSource)
        {
            switch (tableSource.GetType().FullName)
            {
                case "Microsoft.Data.Schema.ScriptDom.Sql.JoinParenthesis":
                    findIdentifiers(((JoinParenthesis)tableSource).Join);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.OdbcQualifiedJoin":
                    findIdentifiers(((OdbcQualifiedJoin)tableSource).TableSource);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.QualifiedJoin":
                    findIdentifiers(((QualifiedJoin)tableSource).FirstTableSource);
                    findIdentifiers(((QualifiedJoin)tableSource).SecondTableSource);
                    findIdentifiers(((QualifiedJoin)tableSource).SearchCondition);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.UnqualifiedJoin":
                    findIdentifiers(((UnqualifiedJoin)tableSource).FirstTableSource);
                    findIdentifiers(((UnqualifiedJoin)tableSource).SecondTableSource);
                    break;

                case "Microsoft.Data.Schema.ScriptDom.Sql.TableSourceWithAlias":
                    // NOP (Not interested in the Alias)
                    break;
                    // The following are all based on TableSourceWithAlias
                case "Microsoft.Data.Schema.ScriptDom.Sql.AdhocTableSource":
                    // ToDo:  Handle This?
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.BuiltInFunctionTableSource":
                    // findIdentifiers(((BuiltInFunctionTableSource)tableSource).Name);
                    // Not interested in Builtin Functions!
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.FullTextTableSource":
                    findIdentifiers(((FullTextTableSource)tableSource).TableName);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.InternalOpenRowset":
                    // NOP (No useful data)
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.OpenQueryTableSource":
                    findIdentifiers((OpenQueryTableSource)tableSource);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.OpenRowsetTableSource":
                    findIdentifiers(((OpenRowsetTableSource)tableSource).Query);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.OpenXmlTableSource":
                    findIdentifiers(((OpenXmlTableSource)tableSource).TableName);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.PivotedTableSource":
                    findIdentifiers(((PivotedTableSource)tableSource).TableSource);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.UnpivotedTableSource":
                    findIdentifiers(((UnpivotedTableSource)tableSource).TableSource);
                    break;

                case "Microsoft.Data.Schema.ScriptDom.Sql.TableSourceWithAliasAndColumns":
                    // See Below
                    break;
                // The following are all based on TableSourceWithAliasAndColumns
                case "Microsoft.Data.Schema.ScriptDom.Sql.BulkOpenRowset":
                    // NOP (Filename)
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.ChangeTableChangesTableSource":
                    findIdentifiers(((ChangeTableChangesTableSource)tableSource).Target);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.ChangeTableVersionTableSource":
                    findIdentifiers(((ChangeTableVersionTableSource)tableSource).Target);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.DataModificationStatementTableSource":
                    findIdentifiers(((DataModificationStatementTableSource)tableSource).Statement);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.InlineDerivedTable":
                    // NOP
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.QueryDerivedTable":
                    findIdentifiers(((QueryDerivedTable)tableSource).Subquery);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.SchemaObjectTableSource":
                    findIdentifiers(((SchemaObjectTableSource)tableSource).SchemaObject);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.VariableTableSource":
                    // Not intested in Variables...
                    break;


                default:
                    throw new Exception("Unhandled Statement Type in findIdentifiers(TableSource tableSource) " + tableSource.GetType().FullName);
            }
        }



        private void findIdentifiers(DataModificationTarget dataTarget)
        {
            switch (dataTarget.GetType().FullName)
            {
                case "Microsoft.Data.Schema.ScriptDom.Sql.OpenRowsetDataModificationTarget":
                    findIdentifiers(((OpenRowsetDataModificationTarget)dataTarget).OpenRowset);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.SchemaObjectDataModificationTarget":
                    findIdentifiers(((SchemaObjectDataModificationTarget)dataTarget).SchemaObject, Identifier.IdentifierEnum.Table);
                    break;
                case "Microsoft.Data.Schema.ScriptDom.Sql.VariableDataModificationTarget":
                    // NOP Not interested in Variables.
                    break;
                default:
                    throw new Exception("Unhandled Statement Type in findIdentifiers(DataModificationTarget dataTarget) " + dataTarget.GetType().FullName);
            }
        }


        private void findIdentifiers(SchemaObjectTableSource schemaObjectTableSource)
        {
            if (schemaObjectTableSource.ParametersUsed)
                findIdentifiers(schemaObjectTableSource.SchemaObject, Identifier.IdentifierEnum.Function);
            else
                findIdentifiers(schemaObjectTableSource.SchemaObject, Identifier.IdentifierEnum.Table);
        }

        private void findIdentifiers(Microsoft.Data.Schema.ScriptDom.Sql.Identifier identifier)
        {
            Identifier foundID;
            foundID = new Identifier(string.Empty, string.Empty, string.Empty, identifier.Value, Identifier.IdentifierEnum.Function);
        }

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
