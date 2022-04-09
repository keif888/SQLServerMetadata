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
        private TSql130Parser parser;
        private TSqlFragment fragment;
        private bool reParseSQL;
        private IList<ParseError> _parseErrors;
        private Dictionary<string, Identifier> _identifiers;

        public bool quotedIdentifiers { get; set; }
        // public string sqlString { get; set; }  // Removed as ParseString should have this as a parameter...

        public List<string> parseErrors
        {
            get
            {
                List<string> result = new List<string>();
                if (_parseErrors != null)
                {
                    foreach (ParseError parseError in _parseErrors)
                    {
                        result.Add(String.Format("Parsing Warning Number: {0}\r\nMessage: {1}\r\nLine: {2}\r\nOffset: {3}", parseError.Number, parseError.Message, parseError.Line, parseError.Offset));
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
            parser = new TSql130Parser(quotedIdentifiers);
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
                    offsetIncrement = 0;
                    foreach (var error in _parseErrors)
                    {
                        sb.AppendLine(error.Message);
                        sb.AppendLine("offset " + error.Offset.ToString());
                        if (error.Number == 46010)  // What is TSP0010 now?
                        {
                            if (sqlString.Substring(error.Offset + offsetIncrement, 1) == "?")
                            {
                                reParseSQL = true;
                                sqlString = sqlString.Substring(0, error.Offset + offsetIncrement) + "@Special19695Guff " + sqlString.Substring(error.Offset + offsetIncrement + 1, sqlString.Length - error.Offset - 1 - offsetIncrement);
                                offsetIncrement += 17;
                            }
                            else if (sqlString.Substring(error.Offset + offsetIncrement, 1) == "{")  //ToDo: Add valid handling for "{ call [core].[sp_update_data_source] (?, ?, ?, ?, ?) }", If it follows the pattern then strip {} and () replace call with exec.
                            {
                                reParseSQL = true;
                                sqlString = sqlString.Substring(0, error.Offset + offsetIncrement) + sqlString.Substring(error.Offset + offsetIncrement + 1, sqlString.Length - error.Offset - 1 - offsetIncrement);
                                offsetIncrement -= 1;
                            }
                            else if (sqlString.Substring(error.Offset + offsetIncrement, 1) == "}")
                            {
                                reParseSQL = true;
                                sqlString = sqlString.Substring(0, error.Offset + offsetIncrement) + sqlString.Substring(error.Offset + offsetIncrement + 1, sqlString.Length - error.Offset - 1 - offsetIncrement);
                                offsetIncrement -= 1;
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

                Dictionary<string, Identifier> tempIdentifiers = new Dictionary<string, Identifier>();
                foreach(Identifier item in _identifiers.Values)
                {
                    tempIdentifiers.Add(item.ToString(false, true), item);
                }

                    foreach (Identifier item in tempIdentifiers.Values)
                {
                    if (item.IdentifierType == Identifier.IdentifierEnum.Table)
                    {
                        if (_identifiers.ContainsKey(string.Format("{0}|{1}", Identifier.IdentifierEnum.Alias, item.ToString(false))))
                        {
                            _identifiers.Remove(item.ToString(false, true));
                        }
                        else if (_identifiers.ContainsKey(string.Format("{0}|{1}", Identifier.IdentifierEnum.CommonTableExpression, item.ToString(false))))
                        {
                            _identifiers.Remove(item.ToString(false, true));
                        }
                    }
                }

                return (parseErrors.Count == 0);
            }
            else
            {
                return false;
            }
        }

        public List<string> getTableNames()
        {
            return getTableNames(false);
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

        public List<string> getAliasNames()
        {
            return getAliasNames(false);
        }

        public List<string> getAliasNames(bool forceSchemaQualified)
        {
            List<string> aliasNames = new List<string>();
            if (fragment is TSqlScript)
            {
                foreach (Identifier foundID in _identifiers.Values)
                {
                    if (foundID.IdentifierType == Identifier.IdentifierEnum.Alias)
                    {
                        aliasNames.Add(foundID.ToString(forceSchemaQualified));
                    }
                }
            }
            else
            {
                throw new Exception("Unhandled Fragment Type " + fragment.GetType().FullName);
            }
            return aliasNames;
        }

        public List<string> getProcedureNames()
        {
            return getProcedureNames(false);
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
            return getFunctionNames(false);
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
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AddAlterFullTextIndexAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AddFileSpec":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AddMemberAlterRoleAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AddSearchPropertyListAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AddSignatureStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AdHocDataSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AdHocTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AffinityKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlgorithmKeyOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AllowConnectionsOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AllowConnectionsOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterApplicationRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAssemblyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAsymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAuthorizationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAvailabilityGroupAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAvailabilityGroupActionType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAvailabilityGroupActionTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAvailabilityGroupFailoverAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAvailabilityGroupFailoverOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAvailabilityGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterAvailabilityGroupStatementType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterBrokerPriorityStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterCertificateStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterCertificateStatementKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterColumnAlterFullTextIndexAction":
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
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterDatabaseTermination":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterEndpointStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterEventSessionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterEventSessionStatementType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterFederationKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterFederationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterFullTextCatalogAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterFullTextCatalogStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterFullTextIndexAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterFullTextIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterFullTextStopListStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterIndexType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterIndexTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterLoginAddDropCredentialStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterLoginEnableDisableStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterLoginOptionsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterLoginStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterMasterKeyOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterMessageTypeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterPartitionFunctionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterPartitionSchemeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterQueueStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterRemoteServiceBindingStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterResourceGovernorCommandType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterResourceGovernorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterResourcePoolStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterRoleAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterRouteStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterSchemaStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterSearchPropertyListStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterSequenceStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterServerAuditSpecificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterServerAuditStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterServerConfigurationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterServerRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterServiceMasterKeyOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterServiceMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterServiceStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterSymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableAddTableElementStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableAlterColumnOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableAlterColumnStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableChangeTrackingModificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableConstraintModificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableDropTableElement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableDropTableElementStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableFileTableNamespaceStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableRebuildStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableSetStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableSwitchStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableTriggerModificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTriggerStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterUserStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterViewStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterWorkloadGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterXmlSchemaCollectionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ApplicationRoleOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ApplicationRoleOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ApplicationRoleOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ApplicationRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AssemblyEncryptionSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AssemblyName":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AssemblyOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AssemblyOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AssemblyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AssignmentKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AssignmentSetClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AsymmetricKeyCreateLoginSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AttachMode":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AttachModeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditActionGroup":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditActionGroupReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditActionSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditEventGroupHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditEventTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditFailureActionType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditGuidAuditOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditSpecificationDetail":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditSpecificationPart":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditSpecificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditTarget":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditTargetKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditTargetOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuditTargetOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuthenticationEndpointProtocolOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuthenticationPayloadOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuthenticationProtocol":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuthenticationTypes":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AuthenticationTypesHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AutoCleanupChangeTrackingOptionDetail":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AvailabilityGroupOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AvailabilityGroupOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AvailabilityGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AvailabilityModeOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AvailabilityModeReplicaOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AvailabilityReplica":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AvailabilityReplicaOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AvailabilityReplicaOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.AvailabilityReplicaOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupCertificateStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupDatabaseStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupOptionsNoValueHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupOptionsWithValueHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupRestoreFileInfo":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupRestoreItemKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupRestoreMasterKeyStatementBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupServiceMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackupTransactionLogStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BackwardsCompatibleDropIndexClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BeginConversationTimerStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BeginDialogStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BeginTransactionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BinaryExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BinaryExpressionType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BinaryLiteral":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BinaryQueryExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BinaryQueryExpressionType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BooleanBinaryExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BooleanBinaryExpressionType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BooleanComparisonExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BooleanComparisonType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BooleanExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BooleanExpressionSnippet":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BooleanIsNullExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BooleanNotExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BooleanParenthesisExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BooleanTernaryExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BooleanTernaryExpressionType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BoundingBoxParameter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BoundingBoxParameterType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BoundingBoxParameterTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BoundingBoxSpatialIndexOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BreakStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BrokerPriorityParameter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BrokerPriorityParameterHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BrokerPriorityParameterSpecialType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BrokerPriorityParameterType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BrokerPriorityStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BrowseForClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BuiltInFunctionTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BulkInsertBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BulkInsertFlagOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BulkInsertIntOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BulkInsertOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BulkInsertOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BulkInsertStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BulkInsertStringOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.BulkOpenRowset":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CallTarget":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CaseExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CastCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CellsPerObjectSpatialIndexOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CertificateCreateLoginSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CertificateOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CertificateOptionKinds":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CertificateOptionKindsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CertificateStatementBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ChangeRetentionChangeTrackingOptionDetail":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ChangeTableChangesTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ChangeTableVersionTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ChangeTrackingDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ChangeTrackingFullTextIndexOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ChangeTrackingOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ChangeTrackingOptionDetail":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CharacterSetPayloadOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CheckConstraintDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CheckpointStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ChildObjectName":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CloseCursorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CloseMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CloseSymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CoalesceExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CodeGenerationSupporter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ColumnDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ColumnDefinitionBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ColumnReferenceExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ColumnStorageOptions":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ColumnType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ColumnWithSortOrder":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CommandOptions":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CommandSecurityElement80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CommitTransactionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CommonTableExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CompositeGroupingSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CompressionEndpointProtocolOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CompressionPartitionRange":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ComputeClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ComputeFunction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ComputeFunctionType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ComputeFunctionTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ConstraintDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ConstraintEnforcement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ContainmentDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ContainmentOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ContainmentOptionKindHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ContinueStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ContractMessage":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ConvertCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateAggregateStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateApplicationRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateAssemblyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateAsymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateAvailabilityGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateBrokerPriorityStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateCertificateStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateColumnStoreIndexStatement":
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
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateFederationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateFullTextCatalogStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateFullTextIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateFullTextStopListStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateLoginSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateLoginStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateMessageTypeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreatePartitionFunctionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreatePartitionSchemeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateQueueStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateRemoteServiceBindingStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateResourcePoolStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateRouteStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateRuleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateSchemaStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateSearchPropertyListStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateSelectiveXmlIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateSequenceStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateServerAuditSpecificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateServerAuditStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateServerRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateServiceStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateSpatialIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateStatisticsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateSymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateSynonymStatement":
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
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreationDispositionKeyOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CredentialStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CryptoMechanism":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CryptoMechanismType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CubeGroupingSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CursorDefaultDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CursorDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CursorId":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CursorOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CursorOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CursorOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CursorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DatabaseAuditAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DatabaseAuditActionGroupHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DatabaseAuditActionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DatabaseEncryptionKeyAlgorithm":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DatabaseEncryptionKeyAlgorithmHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DatabaseEncryptionKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DatabaseMirroringEndpointRole":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DatabaseOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DatabaseOptionKindHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DataCompressionLevel":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DataCompressionLevelHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DataCompressionOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DataModificationSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DataModificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DataModificationTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DataTypeReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DataTypeSequenceOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DbccCommand":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DbccCommandsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DbccJoinOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DbccNamedLiteral":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DbccOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DbccOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DbccOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DbccStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeallocateCursorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeclareTableVariableBody":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeclareTableVariableStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeclareVariableElement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DefaultConstraintDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DefaultLiteral":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeleteMergeAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeleteSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeleteUpdateAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DenyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DenyStatement80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeviceInfo":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeviceType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeviceTypesHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DialogOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DialogOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DiskStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DiskStatementOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DiskStatementOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DiskStatementOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DiskStatementType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropAggregateStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropAlterFullTextIndexAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropApplicationRoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropAssemblyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropAsymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropAvailabilityGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropBrokerPriorityStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropCertificateStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropChildObjectsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropClusteredConstraintMoveOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropClusteredConstraintOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropClusteredConstraintOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropClusteredConstraintStateOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropClusteredConstraintValueOption":
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
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropFederationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropFullTextCatalogStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropFullTextIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropFullTextStopListStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropFunctionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropIndexClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropIndexClauseBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropIndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropLoginStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropMemberAlterRoleAction":
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
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropSchemaBehavior":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropSchemaStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropSearchPropertyListAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropSearchPropertyListStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropSequenceStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropServerAuditSpecificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropServerAuditStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.DropServerRoleStatement":
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
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EnabledDisabledPayloadOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EnableDisableOptionType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EnableDisableOptionTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EnableDisableTriggerStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EncryptionAlgorithm":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EncryptionAlgorithmPreference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EncryptionAlgorithmsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EncryptionPayloadOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EncryptionSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndConversationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndpointAffinity":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndpointEncryptionSupport":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndpointEncryptionSupportHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndpointProtocol":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndpointProtocolOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndpointProtocolOptions":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndpointProtocolOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndpointProtocolsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndpointState":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndpointStateHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndpointType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EndpointTypesHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventDeclaration":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventDeclarationCompareFunctionParameter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventDeclarationSetParameter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventGroupContainer":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventNotificationEventGroup":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventNotificationEventType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventNotificationObjectScope":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventNotificationTarget":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventRetentionSessionOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventSessionEventRetentionModeType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventSessionEventRetentionModeTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventSessionMemoryPartitionModeType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventSessionMemoryPartitionModeTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventSessionObjectName":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventSessionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventTypeContainer":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.EventTypeGroupContainer":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecutableEntity":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecutableProcedureReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecutableStringList":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteAsClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteAsFunctionOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteAsOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteAsOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteAsProcedureOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteAsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteAsTriggerOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteContext":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteInsertSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteParameter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExistsPredicate":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExpressionCallTarget":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExpressionFlags":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExpressionGroupingSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExpressionWithSortOrder":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExtractFromExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FailoverActionOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FailoverModeOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FailoverModeReplicaOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FederationScheme":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FetchCursorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FetchOrientation":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FetchOrientationHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FetchType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileDeclaration":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileDeclarationOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileDeclarationOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileEncryptionSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileGroupDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileGroupOrPartitionScheme":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileGrowthFileDeclarationOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileNameFileDeclarationOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileStreamDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileStreamOnDropIndexOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileStreamOnTableOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileStreamRestoreOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileTableCollateFileNameTableOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileTableConstraintNameTableOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FileTableDirectoryTableOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FipsComplianceLevel":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FipsComplianceLevelHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ForceSeekTableHint":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ForClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ForeignKeyConstraintDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FromClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextCatalogAndFileGroup":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextCatalogOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextCatalogOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextCatalogStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextFunctionType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextIndexColumn":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextIndexOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextIndexOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextPredicate":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextStopListAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FunctionCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FunctionCallSetClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FunctionOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FunctionOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.FunctionReturnType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GeneralSetCommand":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GeneralSetCommandType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GeneralSetCommandTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GetConversationGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GlobalVariableExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GoToStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GrandTotalGroupingSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GrantStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GrantStatement80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GridParameter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GridParameterType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GridParameterTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GridsSpatialIndexOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GroupByClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GroupByOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GroupByOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GroupingSetsGroupingSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.GroupingSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.HadrAvailabilityGroupDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.HadrDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.HadrDatabaseOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.HavingClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IAuthorization":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ICollationSetter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.Identifier":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IdentifierCreateLoginOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IdentifierDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IdentifierLiteral":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IdentifierOrValueExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IdentifierPrincipalOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IdentifierSnippet":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IdentityFunctionCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IdentityOptions":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IdentityValueKeyOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IFileStreamSpecifier":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IIfCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ImportanceParameterHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ImportanceParameterType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IndexAffectingStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IndexExpressionOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IndexOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IndexOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IndexOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IndexStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IndexStateOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IndexTableHint":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InlineDerivedTable":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InlineResultSetDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InPredicate":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InsertBulkColumnDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InsertBulkStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InsertMergeAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InsertOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InsertSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InsertSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IntegerLiteral":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IntegerOptimizerHintHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.InternalOpenRowset":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IPasswordChangeOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IPv4":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.IsolationLevel":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.JoinHint":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.JoinHintHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.JoinParenthesisTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.JoinTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.KeyOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.KeyOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.KeySourceKeyOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.KeywordCasing":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.KillQueryNotificationSubscriptionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.KillStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.KillStatsJobStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LabelStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LeftFunctionCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LikePredicate":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LineNoStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ListenerIPEndpointProtocolOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.Literal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralAuditTargetOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralAvailabilityGroupOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralBulkInsertOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralEndpointProtocolOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralOptimizerHint":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralPayloadOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralPrincipalOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralRange":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralReplicaOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralSessionOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralStatisticsOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralTableHint":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LiteralType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LockEscalationMethod":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LockEscalationMethodHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LockEscalationTableOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.LoginTypePayloadOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MaxDispatchLatencySessionOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MaxLiteral":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MaxRolloverFilesAuditTargetOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MaxSizeAuditTargetOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MaxSizeDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MaxSizeFileDeclarationOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MemoryPartitionSessionOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MemoryUnit":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MemoryUnitsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MergeAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MergeActionClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MergeCondition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MergeSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MessageSender":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MessageTypeStatementBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MessageValidationMethod":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MessageValidationMethodsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MethodSpecifier":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MirrorToClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ModifyFileGroupOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ModifyFilegroupOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MoneyLiteral":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MoveConversationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MoveRestoreOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MoveToDropIndexOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MultiPartIdentifier":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.MultiPartIdentifierCallTarget":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.NamedTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.NameFileDeclarationOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.NextValueForExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.NonTransactedFileStreamAccess":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.NullableConstraintDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.NullIfExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.NullLiteral":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.NullNotNull":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.NumericLiteral":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OdbcConvertSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OdbcFunctionCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OdbcLiteral":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OdbcLiteralType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OdbcQualifiedJoinTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OffsetClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OnFailureAuditOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OnOffAssemblyOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OnOffAuditTargetOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OnOffDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OnOffDialogOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OnOffFullTextCatalogOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OnOffPrincipalOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OnOffRemoteServiceBindingOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OnOffSessionOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OnOffSimpleDbOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenCursorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenQueryTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenRowsetBulkHintOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenRowsetTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenSymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenXmlTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OptimizeForOptimizerHint":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OptimizerHint":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OptimizerHintKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OptionsHelper`1":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OptionsHelper`1+OptionInfo":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OptionState":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OrderBulkInsertOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OrderByClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OutputClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OutputIntoClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.OverClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PageVerifyDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PageVerifyDatabaseOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PageVerifyDbOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ParameterizationDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ParameterizedDataTypeReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ParameterlessCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ParameterlessCallType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ParameterModifier":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ParameterStyle":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ParenthesisExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ParseCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ParseError":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PartitionFunctionCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PartitionFunctionRange":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PartitionParameterType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PartitionSpecifier":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PartnerDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PartnerDatabaseOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PartnerDbOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PasswordAlterPrincipalOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PasswordCreateLoginSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PayloadOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PayloadOptionKinds":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.Permission":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PermissionSetAssemblyOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PermissionSetOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PermissionSetOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PhaseOneBatchException":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PhaseOneConstraintException":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PhaseOnePartialAstException":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PivotedTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PlanOptimizerHintHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PortsEndpointProtocolOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PortTypes":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PortTypesHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PredicateSetOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PredicateSetStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PrimaryExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PrimaryRoleReplicaOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PrincipalOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PrincipalOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PrincipalType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PrintStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.Privilege80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PrivilegeSecurityElement80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PrivilegeType80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProcedureOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProcedureOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProcedureOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProcedureParameter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProcedureReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProcedureReferenceName":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProcedureStatementBody":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProcedureStatementBodyBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProcessAffinityRange":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProcessAffinityType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProviderEncryptionSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ProviderKeyNameKeyOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.PseudoColumnHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QualifiedJoin":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QualifiedJoinType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QueryDerivedTable":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QueryExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QueryParenthesisExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QuerySpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QueueDelayAuditOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QueueExecuteAsOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QueueOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QueueOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QueueProcedureOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QueueStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QueueStateOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QueueValueOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.QuoteType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RaiseErrorLegacyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RaiseErrorOptions":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RaiseErrorOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RaiseErrorStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ReadOnlyForClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ReadTextStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RealLiteral":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ReceiveStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ReconfigureStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RecoveryDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RecoveryDatabaseOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RecoveryDbOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RemoteServiceBindingOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RemoteServiceBindingOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RemoteServiceBindingStatementBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RenameAlterRoleAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ResourcePoolAffinityHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ResourcePoolAffinitySpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ResourcePoolAffinityType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ResourcePoolParameter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ResourcePoolParameterHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ResourcePoolParameterType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ResourcePoolStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RestoreMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RestoreOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RestoreOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RestoreOptionNoValueHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RestoreOptionWithValueHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RestoreServiceMasterKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RestoreStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RestoreStatementKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RestoreStatementKindsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ResultColumnDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ResultSetDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ResultSetsExecuteOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ResultSetsOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ResultSetType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RetentionUnitHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ReturnStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RevertStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RevokeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RevokeStatement80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RightFunctionCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RolePayloadOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RoleStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RollbackTransactionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RollupGroupingSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RouteOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RouteOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RouteOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RouteStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.RowValue":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SaveTransactionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScalarExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScalarExpressionDialogOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScalarExpressionRestoreOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScalarExpressionSequenceOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScalarExpressionSnippet":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScalarFunctionReturnType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScalarSubquery":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SchemaDeclarationItem":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SchemaObjectFunctionTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SchemaObjectName":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SchemaObjectNameOrValueExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SchemaObjectNameSnippet":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SchemaObjectResultSetDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SchemaPayloadOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.AlignmentPoint":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.EmptyGenerator":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.IdentifierGenerator":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.KeywordGenerator":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.ScriptGeneratorSupporter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.ScriptWriter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.ScriptWriter+AlignmentPointData":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.ScriptWriter+NewLineElement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.ScriptWriter+ScriptWriterElement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.ScriptWriter+ScriptWriterElementType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.ScriptWriter+TokenWrapper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.Sql100ScriptGeneratorVisitor":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.Sql110ScriptGeneratorVisitor":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.Sql80ScriptGeneratorVisitor":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.Sql90ScriptGeneratorVisitor":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.SqlScriptGeneratorVisitor":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.SqlScriptGeneratorVisitor+ListGenerationOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.SqlScriptGeneratorVisitor+ListGenerationOption+SeparatorType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ScriptGenerator.TokenGenerator":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SearchedCaseExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SearchedWhenClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SearchPropertyListAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SearchPropertyListFullTextIndexOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecondaryRoleReplicaOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecondaryXmlIndexType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecondaryXmlIndexTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecurityElement80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecurityLoginOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecurityObjectKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecurityPrincipal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecurityStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecurityStatementBody80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecurityTargetObject":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecurityTargetObjectName":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SecurityUserClause80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SelectElement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SelectFunctionReturnType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SelectInsertSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SelectiveXmlIndexPromotedPath":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SelectScalarExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SelectSetVariable":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SelectStarExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SelectStatementSnippet":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SemanticFunctionType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SemanticTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SendStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SeparatorType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SequenceOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SequenceOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SequenceStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ServerAuditActionGroupHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ServerAuditStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ServiceBrokerOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ServiceBrokerOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ServiceContract":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SessionOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SessionOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SessionOptionUnitHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SessionTimeoutPayloadOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetCommand":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetCommandStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetErrorLevelStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetFipsFlaggerCommand":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetIdentityInsertStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetOffsets":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetOffsetsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetOnOffStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetOptions":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetRowCountStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetSearchPropertyListAlterFullTextIndexAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetStatisticsOptions":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetStatisticsOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetStatisticsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetStopListAlterFullTextIndexAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetTextSizeStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetTransactionIsolationLevelStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetUserStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SetVariableStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ShutdownStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SignableElementKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SignatureStatementBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SimpleAlterFullTextIndexAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SimpleAlterFullTextIndexActionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SimpleCaseExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SimpleDbOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SimpleWhenClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SizeFileDeclarationOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SoapMethod":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SoapMethodAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SoapMethodFormat":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SoapMethodFormatsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SoapMethodSchemas":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SortOrder":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SourceDeclaration":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SparseColumnOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SpatialIndexingSchemeType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SpatialIndexingSchemeTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SpatialIndexOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SpatialIndexRegularOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.Sql100ScriptGenerator":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.Sql110ScriptGenerator":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.Sql80ScriptGenerator":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.Sql90ScriptGenerator":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SqlCommandIdentifier":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SqlDataTypeOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SqlDataTypeReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SqlScriptGenerator":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SqlScriptGeneratorOptions":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SqlScriptGeneratorResource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SqlVersion":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SqlVersionFlags":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.StateAuditOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.StatementList":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.StatementListSnippet":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.StatementWithCtesAndXmlNamespaces":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.StatisticsOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.StatisticsOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.StatisticsOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.StopListFullTextIndexOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.StopRestoreOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.StringLiteral":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SubDmlFlags":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SubqueryComparisonPredicate":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SubqueryComparisonPredicateType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.SymmetricKeyStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableDataCompressionOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableElementType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableHint":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableHintKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableHintOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableHintsOptimizerHint":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableReferenceWithAlias":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableReferenceWithAliasAndColumns":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableSampleClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableSampleClauseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TableValuedFunctionReturnType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TargetDeclaration":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TargetRecoveryTimeDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TargetRecoveryTimeUnitHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TextModificationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ThrowStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TimeUnit":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TopRowFilter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TransactionStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TriggerAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TriggerActionType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TriggerEnforcement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TriggerEventGroupHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TriggerEventTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TriggerObject":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TriggerOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TriggerOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TriggerOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TriggerScope":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TriggerStatementBody":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TriggerType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TruncateTableStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TryCastCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TryCatchStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TryConvertCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TryParseCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSEqualCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql100LexerInternal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql100Parser":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql100ParserBaseInternal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql100ParserInternal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql110LexerInternal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql110Parser":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql110ParserBaseInternal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql110ParserInternal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql80LexerInternal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql80Parser":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql80ParserBaseInternal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql80ParserBaseInternal+ParserEntryPoint`1":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql80ParserInternal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql90LexerInternal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql90Parser":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql90ParserBaseInternal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSql90ParserInternal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlAuditEventGroupHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlAuditEventTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlBatch":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlConcreteFragmentVisitor":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlFragment":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlFragmentFactory":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlFragmentSnippet":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlFragmentVisitor":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlLexerBaseInternal":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlLexerBaseInternal+TokenKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlParseErrorException":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlParser":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlParser+ExtractSchemaObjectNameVisitor":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlParserResource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlParserToken":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlScript":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlStatementSnippet":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlTokenType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlTriggerEventGroupHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlTriggerEventTypeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlWhitespaceTokenFilter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.TSqlWhitespaceTokenFilter+TSqlParserTokenProxyWithIndex":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UnaryExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UnaryExpressionType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UniqueConstraintDefinition":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UniqueRowFilter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UnpivotedTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UnqualifiedJoin":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UnqualifiedJoinType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UpdateCall":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UpdateDeleteSpecificationBase":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UpdateForClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UpdateMergeAction":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UpdateSpecification":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UpdateStatisticsStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UpdateTextStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UseFederationStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UserDataTypeReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UserDefinedTypeCallTarget":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UserDefinedTypePropertyAccess":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UserLoginOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UserLoginOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UserLoginOptionType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UserOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UserRemoteServiceBindingOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UserStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UserType80":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.UseStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ValueExpression":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ValuesInsertSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.VariableMethodCallTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.VariableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.VariableTableReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.VariableValuePair":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ViewOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ViewOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.ViewOptionKind":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WaitForOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WaitForOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WaitForStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WaitForSupportedStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WhenClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WhereClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WhileStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WindowDelimiter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WindowDelimiterType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WindowFrameClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WindowFrameType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WindowsCreateLoginSource":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WithCtesAndXmlNamespaces":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WithinGroupClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WitnessDatabaseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WorkloadGroupImportanceParameter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WorkloadGroupParameter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WorkloadGroupParameterType":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WorkloadGroupResourceParameter":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WorkloadGroupResourceParameterHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WorkloadGroupStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WriteTextStatement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.WsdlPayloadOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.XmlDataTypeOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.XmlDataTypeOptionHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.XmlDataTypeReference":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.XmlForClause":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.XmlForClauseModeHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.XmlForClauseOption":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.XmlForClauseOptions":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.XmlForClauseOptionsHelper":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.XmlNamespaces":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.XmlNamespacesAliasElement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.XmlNamespacesDefaultElement":
                case "Microsoft.SqlServer.TransactSql.ScriptDom.XmlNamespacesElement":
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterFunctionStatement":
                    findIdentifiers(((AlterFunctionStatement)sqlStatement).Name, Identifier.IdentifierEnum.Function);
                    findIdentifiers(((AlterFunctionStatement)sqlStatement).StatementList);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterProcedureStatement":
                    findIdentifiers(((AlterProcedureStatement)sqlStatement).ProcedureReference.Name);
                    findIdentifiers(((AlterProcedureStatement)sqlStatement).StatementList);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.AlterTableStatement":
                    findIdentifiers(((AlterTableStatement)sqlStatement).SchemaObjectName);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.BeginEndBlockStatement":
                    findIdentifiers(((BeginEndBlockStatement)sqlStatement).StatementList);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateFunctionStatement":
                    findIdentifiers(((CreateFunctionStatement)sqlStatement).Name, Identifier.IdentifierEnum.Function);
                    findIdentifiers(((CreateFunctionStatement)sqlStatement).StatementList);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateProcedureStatement":
                    findIdentifiers(((CreateProcedureStatement)sqlStatement).ProcedureReference.Name, Identifier.IdentifierEnum.Procedure);
                    findIdentifiers(((CreateProcedureStatement)sqlStatement).StatementList);
                    break;
                case "Microsoft.SqlServer.TransactSql.ScriptDom.CreateTableStatement":
                    findIdentifiers(((CreateTableStatement)sqlStatement).SchemaObjectName);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeclareCursorStatement":
                    findIdentifiers(((DeclareCursorStatement)sqlStatement).CursorDefinition.Select);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeclareVariableStatement":
                    foreach (DeclareVariableElement declareVariableElement in ((DeclareVariableStatement)sqlStatement).Declarations)
                        if (declareVariableElement.Value != null)
                            findIdentifiers(declareVariableElement.Value);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.DeleteStatement":
                    DeleteStatement deleteStatement = sqlStatement as DeleteStatement;
                    if (deleteStatement.WithCtesAndXmlNamespaces != null)
                    {
                        foreach (CommonTableExpression cte in deleteStatement.WithCtesAndXmlNamespaces.CommonTableExpressions)
                        {
                            findIdentifiers(cte.ExpressionName, Identifier.IdentifierEnum.CommonTableExpression);
                            findIdentifiers(cte.QueryExpression);
                        }
                    }
                    findIdentifiers(deleteStatement.DeleteSpecification.WhereClause);
                    findIdentifiers(deleteStatement.DeleteSpecification.Target);
                    if (deleteStatement.DeleteSpecification.FromClause != null)
                    {
                        foreach (TableReference table in deleteStatement.DeleteSpecification.FromClause.TableReferences)
                        {
                            findIdentifiers(table);
                        }
                    }
                    if (deleteStatement.DeleteSpecification.OutputIntoClause != null)
                    {
                        findIdentifiers(deleteStatement.DeleteSpecification.OutputIntoClause.IntoTable);
                    }
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.ExecuteStatement":
                    findIdentifiers((ExecuteStatement)sqlStatement);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.FunctionStatementBody":
                    findIdentifiers(((FunctionStatementBody)sqlStatement).StatementList);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.IfStatement":
                    if (((IfStatement)sqlStatement).ThenStatement != null)
                        findIdentifiers(((IfStatement)sqlStatement).ThenStatement);
                    if (((IfStatement)sqlStatement).ElseStatement != null)
                        findIdentifiers(((IfStatement)sqlStatement).ElseStatement);
                    if (((IfStatement)sqlStatement).Predicate != null)
                        findIdentifiers(((IfStatement)sqlStatement).Predicate);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.InsertStatement":
                    InsertStatement insertStatement = sqlStatement as InsertStatement;
                    if (insertStatement.WithCtesAndXmlNamespaces != null)
                    {
                        foreach (CommonTableExpression cte in insertStatement.WithCtesAndXmlNamespaces.CommonTableExpressions)
                        {
                            findIdentifiers(cte.ExpressionName, Identifier.IdentifierEnum.CommonTableExpression);
                            findIdentifiers(cte.QueryExpression);
                        }
                    }
                    findIdentifiers(insertStatement.InsertSpecification.Target);
                    findIdentifiers(insertStatement.InsertSpecification.InsertSource);
                    if (insertStatement.InsertSpecification.OutputIntoClause != null)
                        findIdentifiers(insertStatement.InsertSpecification.OutputIntoClause.IntoTable);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.MergeStatement":
                    MergeStatement mergeStatement = sqlStatement as MergeStatement;
                    if (mergeStatement.WithCtesAndXmlNamespaces != null)
                    {
                        foreach (CommonTableExpression cte in mergeStatement.WithCtesAndXmlNamespaces.CommonTableExpressions)
                        {
                            findIdentifiers(cte.ExpressionName, Identifier.IdentifierEnum.CommonTableExpression);
                            findIdentifiers(cte.QueryExpression);
                        }
                    }
                    findIdentifiers(mergeStatement.MergeSpecification.Target);
                    findIdentifiers(mergeStatement.MergeSpecification.TableReference);
                    if (mergeStatement.MergeSpecification.OutputIntoClause != null)
                        findIdentifiers(mergeStatement.MergeSpecification.OutputIntoClause.IntoTable);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.SelectStatement":
                    SelectStatement selectStatement = sqlStatement as SelectStatement;
                    if (selectStatement.WithCtesAndXmlNamespaces != null)
                    {
                        foreach (CommonTableExpression cte in selectStatement.WithCtesAndXmlNamespaces.CommonTableExpressions)
                        {
                            findIdentifiers(cte.ExpressionName, Identifier.IdentifierEnum.CommonTableExpression);
                            findIdentifiers(cte.QueryExpression);
                        }
                    }
                    findIdentifiers(selectStatement.QueryExpression);
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.UpdateStatement":
                    UpdateStatement updateStatement = sqlStatement as UpdateStatement;
                    if (updateStatement.WithCtesAndXmlNamespaces != null)
                    {
                        foreach (CommonTableExpression cte in updateStatement.WithCtesAndXmlNamespaces.CommonTableExpressions)
                        {
                            findIdentifiers(cte.ExpressionName, Identifier.IdentifierEnum.CommonTableExpression);
                            findIdentifiers(cte.QueryExpression);
                        }
                    }
                    findIdentifiers(updateStatement.UpdateSpecification.WhereClause);
                    findIdentifiers(updateStatement.UpdateSpecification.Target);
                    if (updateStatement.UpdateSpecification.FromClause != null)
                    {
                        foreach (TableReference table in updateStatement.UpdateSpecification.FromClause.TableReferences)
                        {
                            findIdentifiers(table);
                        }
                    }
                    if (updateStatement.UpdateSpecification.OutputIntoClause != null)
                    {
                        findIdentifiers(updateStatement.UpdateSpecification.OutputIntoClause.IntoTable);
                    }
                    foreach (SetClause setClause in updateStatement.UpdateSpecification.SetClauses)
                    {
                        findIdentifiers(setClause);
                    }
                    break;

                case "Microsoft.SqlServer.TransactSql.ScriptDom.ViewStatementBody":
                    findIdentifiers(((ViewStatementBody)sqlStatement).SelectStatement);
                    break;

                default:
                    throw new Exception("Unhandled Statement Type in findIdentifiers(TSqlStatement sqlStatement) " + sqlStatement.GetType().FullName);
            }
        }

        private void findIdentifiers(InsertSource insertSource)
        {
            if (insertSource is SelectInsertSource)
            {
                SelectInsertSource selectInsertSource = insertSource as SelectInsertSource;
                findIdentifiers(selectInsertSource.Select);
            }
            else if (insertSource is ExecuteInsertSource)
            {
                findIdentifiers((insertSource as ExecuteInsertSource).Execute);
            }
        }

        private void findIdentifiers(ExecuteSpecification executeSpecification)
        {
            if (executeSpecification.ExecutableEntity != null)
            {
                if (executeSpecification.ExecutableEntity is ExecutableProcedureReference)
                {
                    ExecutableProcedureReference executableProcedureReference = executeSpecification.ExecutableEntity as ExecutableProcedureReference;
                    findIdentifiers(executableProcedureReference.ProcedureReference.ProcedureReference.Name, Identifier.IdentifierEnum.Procedure);
                }
                else
                {
                    ExecutableStringList executableStringList = executeSpecification.ExecutableEntity as ExecutableStringList;
                    foreach (ValueExpression ve in executableStringList.Strings)
                    {
                        findIdentifiers(ve);
                    }
                }
            }
        }

        private void findIdentifiers(SetClause setClause)
        {
            if (setClause is AssignmentSetClause)
            {
                AssignmentSetClause assSetClause = setClause as AssignmentSetClause;
                findIdentifiers(assSetClause.NewValue);
            }
            else
            {
                FunctionCallSetClause funcSetClause = setClause as FunctionCallSetClause;
                findIdentifiers(funcSetClause.MutatorFunction);
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
            if (execStatement.ExecuteSpecification != null)
            {
                if (execStatement.ExecuteSpecification.ExecutableEntity != null)
                {
                    if (execStatement.ExecuteSpecification.ExecutableEntity is ExecutableProcedureReference)
                    {
                        ExecutableProcedureReference executableProcedureReference = execStatement.ExecuteSpecification.ExecutableEntity as ExecutableProcedureReference;
                        findIdentifiers(executableProcedureReference.ProcedureReference.ProcedureReference.Name, Identifier.IdentifierEnum.Procedure);
                        foreach (ExecuteParameter parm in executableProcedureReference.Parameters)
                        {
                            //ToDo: Parse these
                            //findIdentifiers(parm);
                        }
                    }
                    else if (execStatement.ExecuteSpecification.ExecutableEntity is ExecutableStringList)
                    {
                        ExecutableStringList executableStringList = execStatement.ExecuteSpecification.ExecutableEntity as ExecutableStringList;

                        foreach (ValueExpression ve in executableStringList.Strings)
                        {
                            findIdentifiers(ve);
                        }
                        foreach (ExecuteParameter parm in executableStringList.Parameters)
                        {
                            //ToDo: Parse these
                            //findIdentifiers(parm);
                        }
                    }
                    else
                    {
                        throw new Exception("Unhandled Statement Type in findIdentifiers(ExecuteStatement execStatement) " + execStatement.ExecuteSpecification.ExecutableEntity.GetType().FullName);
                    }
                }
            }
        }

        private void findIdentifiers(SelectStatement selectStatement)
        {
            if (selectStatement is StatementWithCtesAndXmlNamespaces) // StatementWithCommonTableExpressionsAndXmlNamespaces)
            {
                if (selectStatement.WithCtesAndXmlNamespaces != null) // .WithCommonTableExpressionsAndXmlNamespaces != null)
                {
                    foreach (CommonTableExpression cte in selectStatement.WithCtesAndXmlNamespaces.CommonTableExpressions)
                    {
                        findIdentifiers(cte.ExpressionName, Identifier.IdentifierEnum.CommonTableExpression);
                        findIdentifiers(cte.QueryExpression);
                    }
                    //findIdentifiers(selectStatement.WithCtesAndXmlNamespaces);
                }
            }
            else
            {
                throw new Exception("Unhandled Statement Type in findIdentifiers(SelectStatement selectStatement) " + selectStatement.GetType().FullName);
            }
            findIdentifiers(selectStatement.QueryExpression);
        }

        #region QueryExpression
        private void findIdentifiers(QueryExpression queryExpression)
        {
            if (queryExpression != null)
            {
                if (queryExpression is QuerySpecification)
                {
                    findIdentifiers((QuerySpecification)queryExpression);
                    foreach (SelectElement selectElement in (queryExpression as QuerySpecification).SelectElements)
                    {
                        findIdentifiers(selectElement);
                    }
                }
                else if (queryExpression is BinaryQueryExpression)
                {
                    BinaryQueryExpression binaryQueryExpression = queryExpression as BinaryQueryExpression;

                    findIdentifiers(binaryQueryExpression.FirstQueryExpression);
                    findIdentifiers(binaryQueryExpression.SecondQueryExpression);
                }
                else if (queryExpression is QueryParenthesisExpression)
                {
                    QueryParenthesisExpression queryPE = queryExpression as QueryParenthesisExpression;
                    findIdentifiers(queryPE.QueryExpression);
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
        }

        private void findIdentifiers(SelectSetVariable selectSetVariable)
        {
            if (selectSetVariable.Expression != null)
            {
                findIdentifiers(selectSetVariable.Expression);
            }
        }

        private void findIdentifiers(SelectScalarExpression selectScalarExpression)
        {
            if (selectScalarExpression.Expression != null)
            {
                findIdentifiers(selectScalarExpression.Expression);
            }
            if (selectScalarExpression.ColumnName != null)
            {
                findIdentifiers(selectScalarExpression.ColumnName);
            }
        }

        private void findIdentifiers(IdentifierOrValueExpression identifierOrValueExpression)
        {
            if (identifierOrValueExpression.Identifier != null)
            {
                // This is the column name? identifierOrValueExpression.Identifier.Value
            }
            if (identifierOrValueExpression.ValueExpression != null)
            {
                findIdentifiers(identifierOrValueExpression.ValueExpression);
            }
        }

        private void findIdentifiers(QuerySpecification querySpecification)
        {
            if (querySpecification.FromClause != null)
                foreach (TableReference tableSource in querySpecification.FromClause.TableReferences)
                {
                    findIdentifiers(tableSource);
                }
            if (querySpecification.SelectElements != null)
                foreach (SelectElement element in querySpecification.SelectElements)
                {
                    findIdentifiers(element);
                }
            if (querySpecification.WhereClause != null)
            {
                findIdentifiers(querySpecification.WhereClause);
            }
        }


        private void findIdentifiers(SelectElement selectElement)
        {
            if (selectElement is SelectScalarExpression)
            {
                findIdentifiers(selectElement as SelectScalarExpression);
            }
            else if (selectElement is SelectSetVariable)
            {
                findIdentifiers(selectElement as SelectSetVariable);
            }
            else if (selectElement is SelectStarExpression)
            {
                // No Identifiers on a SELECT *...
            }
            else
            {
                throw new Exception("Unhandled Statement Type in findIdentifiers(SelectElement selectElement) " + selectElement.GetType().FullName);
            }
        }

        #endregion

        private void findIdentifiers(WhereClause whereClause)
        {
            if (whereClause != null)
                findIdentifiers(whereClause.SearchCondition);
        }

        private void findIdentifiers(BooleanExpression booleanExpression)
        {
            if (booleanExpression is BooleanComparisonExpression)
            {
                findIdentifiers((booleanExpression as BooleanComparisonExpression).FirstExpression);
                findIdentifiers((booleanExpression as BooleanComparisonExpression).SecondExpression);
            }
            else if (booleanExpression is BooleanBinaryExpression)
            {
                findIdentifiers((booleanExpression as BooleanBinaryExpression).FirstExpression);
                findIdentifiers((booleanExpression as BooleanBinaryExpression).SecondExpression);
            }
            //else if (booleanExpression is BooleanExpressionSnippet)
            //{
            //    findIdentifiers((booleanExpression as BooleanExpressionSnippet).);
            //    findIdentifiers((booleanExpression as BooleanExpressionSnippet).SecondExpression);
            //}
            else if (booleanExpression is BooleanIsNullExpression)
            {
                findIdentifiers((booleanExpression as BooleanIsNullExpression).Expression);
            }
            else if (booleanExpression is BooleanNotExpression)
            {
                findIdentifiers((booleanExpression as BooleanNotExpression).Expression);
            }
            else if (booleanExpression is BooleanParenthesisExpression)
            {
                findIdentifiers((booleanExpression as BooleanParenthesisExpression).Expression);
            }
            else if (booleanExpression is BooleanTernaryExpression)
            {
                findIdentifiers((booleanExpression as BooleanTernaryExpression).FirstExpression);
                findIdentifiers((booleanExpression as BooleanTernaryExpression).SecondExpression);
                findIdentifiers((booleanExpression as BooleanTernaryExpression).ThirdExpression);
            }
            else if (booleanExpression is EventDeclarationCompareFunctionParameter)
            {
                //foreach (Microsoft.SqlServer.TransactSql.ScriptDom.Identifier id in (booleanExpression as EventDeclarationCompareFunctionParameter).Name.MultiPartIdentifier.Identifiers)
                //    findIdentifiers(id.);
                findIdentifiers((booleanExpression as EventDeclarationCompareFunctionParameter).SourceDeclaration);
            }
            else if (booleanExpression is ExistsPredicate)
            {
                findIdentifiers((booleanExpression as ExistsPredicate).Subquery);
            }
            else if (booleanExpression is FullTextPredicate)
            {
                // No Table ID's here!
            }
            else if (booleanExpression is InPredicate)
            {
                findIdentifiers((booleanExpression as InPredicate).Expression);
                foreach (ScalarExpression expression in (booleanExpression as InPredicate).Values)
                    findIdentifiers(expression);
                if ((booleanExpression as InPredicate).Subquery != null)
                    findIdentifiers((booleanExpression as InPredicate).Subquery);
            }
            else if (booleanExpression is LikePredicate)
            {
                findIdentifiers((booleanExpression as LikePredicate).FirstExpression);
                findIdentifiers((booleanExpression as LikePredicate).SecondExpression);
            }
            else if (booleanExpression is SubqueryComparisonPredicate)
            {
                findIdentifiers((booleanExpression as SubqueryComparisonPredicate).Expression);
                findIdentifiers((booleanExpression as SubqueryComparisonPredicate).Subquery);
            }
            else if (booleanExpression is TSEqualCall)
            {
                findIdentifiers((booleanExpression as TSEqualCall).FirstExpression);
                findIdentifiers((booleanExpression as TSEqualCall).SecondExpression);
            }
            else if (booleanExpression is UpdateCall)
            {
                //ToDo: test this and find out what it is.
                //findIdentifiers((booleanExpression as UpdateCall).Identifier);
                //findIdentifiers((booleanExpression as UpdateCall).);
            }
            else
            {
                throw new Exception("Unhandled Statement Type in findIdentifiers(BooleanExpression booleanExpression) " + booleanExpression.GetType().FullName);
            }
        }

        private void findIdentifiers(ScalarExpression scalarExpression)
        {
            if (scalarExpression != null)
                switch (scalarExpression.GetType().FullName)
                {
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.BinaryLiteral":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.ColumnReferenceExpression":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.DefaultLiteral":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.GlobalVariableExpression":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.IdentifierLiteral":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.IdentityFunctionCall":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.IntegerLiteral":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.Literal":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.MaxLiteral":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.MoneyLiteral":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.NextValueForExpression":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.NullIfExpression":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.NullLiteral":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.NumericLiteral":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.OdbcConvertSpecification":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.OdbcFunctionCall":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.OdbcLiteral":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.ParameterlessCall":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.ParseCall":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.PartitionFunctionCall":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.PrimaryExpression":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.RealLiteral":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.RightFunctionCall":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.ScalarExpression":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.ScalarExpressionSnippet":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.SourceDeclaration":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.StringLiteral":  // ToDo: re-parse this!
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.TryCastCall":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.TryConvertCall":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.TryParseCall":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.UnaryExpression":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.UserDefinedTypePropertyAccess":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.ValueExpression":
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.VariableReference":
                        break;

                    case "Microsoft.SqlServer.TransactSql.ScriptDom.BinaryExpression":
                        findIdentifiers((scalarExpression as BinaryExpression).FirstExpression);
                        findIdentifiers((scalarExpression as BinaryExpression).SecondExpression);
                        break;

                    case "Microsoft.SqlServer.TransactSql.ScriptDom.CaseExpression":
                        findIdentifiers((scalarExpression as CaseExpression).ElseExpression);
                        break;

                    case "Microsoft.SqlServer.TransactSql.ScriptDom.CastCall":
                        findIdentifiers((scalarExpression as CastCall).Parameter);
                        break;

                    case "Microsoft.SqlServer.TransactSql.ScriptDom.CoalesceExpression":
                        foreach (ScalarExpression expression in (scalarExpression as CoalesceExpression).Expressions)
                        {
                            findIdentifiers(expression);
                        }
                        break;

                    case "Microsoft.SqlServer.TransactSql.ScriptDom.ConvertCall":
                        findIdentifiers((scalarExpression as ConvertCall).Parameter);
                        break;

                    case "Microsoft.SqlServer.TransactSql.ScriptDom.ExtractFromExpression":
                        findIdentifiers((scalarExpression as ExtractFromExpression).Expression);
                        break;

                    case "Microsoft.SqlServer.TransactSql.ScriptDom.FunctionCall":
                        FunctionCall functionCall = scalarExpression as FunctionCall;
                        //ToDo:  Handle Identifiers!  
                        if (functionCall.CallTarget != null)
                        {
                            SchemaObjectName son = new SchemaObjectName();
                            if (functionCall.CallTarget is MultiPartIdentifierCallTarget)
                            {
                                if ((functionCall.CallTarget as MultiPartIdentifierCallTarget).MultiPartIdentifier != null)
                                    foreach (Microsoft.SqlServer.TransactSql.ScriptDom.Identifier mpi in (functionCall.CallTarget as MultiPartIdentifierCallTarget).MultiPartIdentifier.Identifiers)
                                    {
                                        son.Identifiers.Add(mpi);
                                    }
                            }
                            else if (functionCall.CallTarget is ExpressionCallTarget)
                            {
                                if ((functionCall.CallTarget as ExpressionCallTarget).Expression != null)
                                {
                                    findIdentifiers((functionCall.CallTarget as ExpressionCallTarget).Expression);
                                }
                            }
                            son.Identifiers.Add(functionCall.FunctionName);
                            findIdentifiers(son, Identifier.IdentifierEnum.Function);
                        }
                        else
                        {
                            findIdentifiers(functionCall.FunctionName, Identifier.IdentifierEnum.Function);
                        }
                        foreach (ScalarExpression expression in functionCall.Parameters)
                        {
                            findIdentifiers(expression);
                        }
                        break;

                    case "Microsoft.SqlServer.TransactSql.ScriptDom.IIfCall":
                        findIdentifiers((scalarExpression as IIfCall).Predicate);
                        findIdentifiers((scalarExpression as IIfCall).ElseExpression);
                        findIdentifiers((scalarExpression as IIfCall).ThenExpression);
                        break;

                    case "Microsoft.SqlServer.TransactSql.ScriptDom.LeftFunctionCall":
                        foreach (ScalarExpression expression in (scalarExpression as LeftFunctionCall).Parameters)
                        {
                            findIdentifiers(expression);
                        }
                        break;

                    case "Microsoft.SqlServer.TransactSql.ScriptDom.ParenthesisExpression":
                        findIdentifiers((scalarExpression as ParenthesisExpression).Expression);
                        break;

                    case "Microsoft.SqlServer.TransactSql.ScriptDom.ScalarSubquery":
                        findIdentifiers((scalarExpression as ScalarSubquery).QueryExpression);
                        break;

                    case "Microsoft.SqlServer.TransactSql.ScriptDom.SearchedCaseExpression":
                        SearchedCaseExpression searchedCaseExpression = scalarExpression as SearchedCaseExpression;
                        findIdentifiers(searchedCaseExpression.ElseExpression);
                        foreach (WhenClause whenClause in searchedCaseExpression.WhenClauses)
                        {
                            findIdentifiers(whenClause.ThenExpression);
                        }
                        break;

                    case "Microsoft.SqlServer.TransactSql.ScriptDom.SimpleCaseExpression":
                        SimpleCaseExpression simpleCaseExpression = scalarExpression as SimpleCaseExpression;
                        findIdentifiers(simpleCaseExpression.InputExpression);
                        findIdentifiers(simpleCaseExpression.ElseExpression);
                        foreach (WhenClause whenClause in simpleCaseExpression.WhenClauses)
                        {
                            findIdentifiers(whenClause.ThenExpression);
                        }
                        break;

                    default:
                        throw new Exception("Unhandled Statement Type in findIdentifiers(ScalarExpression scalarExpression) " + scalarExpression.GetType().FullName);
                };
        }


        private void findIdentifiers(TableReference tableSource)
        {
            if (tableSource != null)
            {
                switch (tableSource.GetType().FullName)
                {
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.NamedTableReference":
                        // These are sometimes CTE's.  How do you tell the difference?
                        findIdentifiers((tableSource as NamedTableReference).SchemaObject);
                        if ((tableSource as NamedTableReference).Alias != null)
                            findIdentifiers((tableSource as NamedTableReference).Alias);
                        // Ignore these as they are CTE names...
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.JoinParenthesisTableReference":
                        findIdentifiers(((JoinParenthesisTableReference)tableSource).Join);
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.OdbcQualifiedJoinTableReference":
                        findIdentifiers(((OdbcQualifiedJoinTableReference)tableSource).TableReference);
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.QualifiedJoin":
                        findIdentifiers(((QualifiedJoin)tableSource).FirstTableReference);
                        findIdentifiers(((QualifiedJoin)tableSource).SecondTableReference);
                        findIdentifiers(((QualifiedJoin)tableSource).SearchCondition);
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.UnqualifiedJoin":
                        findIdentifiers(((UnqualifiedJoin)tableSource).FirstTableReference);
                        findIdentifiers(((UnqualifiedJoin)tableSource).SecondTableReference);
                        break;

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
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.FullTextTableReference":
                        findIdentifiers(((FullTextTableReference)tableSource).TableName);
                        break;
                    //case "Microsoft.SqlServer.TransactSql.ScriptDom.InternalOpenRowset":
                    //    // NOP (No useful data)
                    //    break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenQueryTableReference":
                        findIdentifiers((OpenQueryTableReference)tableSource);
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenRowsetTableReference":
                        findIdentifiers(((OpenRowsetTableReference)tableSource).Query);
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.OpenXmlTableReference":
                        findIdentifiers(((OpenXmlTableReference)tableSource).TableName);
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.PivotedTableReference":
                        findIdentifiers(((PivotedTableReference)tableSource).TableReference);
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.UnpivotedTableReference":
                        findIdentifiers(((UnpivotedTableReference)tableSource).TableReference);
                        break;

                    //case "Microsoft.SqlServer.TransactSql.ScriptDom.TableSourceWithAliasAndColumns":
                    //    // See Below
                    //    break;
                    //// The following are all based on TableSourceWithAliasAndColumns
                    //case "Microsoft.SqlServer.TransactSql.ScriptDom.BulkOpenRowset":
                    //    // NOP (Filename)
                    //    break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.ChangeTableChangesTableReference":
                        findIdentifiers(((ChangeTableChangesTableReference)tableSource).Target);
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.ChangeTableVersionTableReference":
                        findIdentifiers(((ChangeTableVersionTableReference)tableSource).Target);
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.DataModificationTableReference":
                        findIdentifiers(((DataModificationTableReference)tableSource).DataModificationSpecification.Target);
                        if (((DataModificationTableReference)tableSource).DataModificationSpecification is MergeSpecification specification)
                        {
                            if (specification.TableReference != null)
                                findIdentifiers(specification.TableReference);
                        }
                        if (((DataModificationTableReference)tableSource).DataModificationSpecification.OutputIntoClause != null)
                        {
                            findIdentifiers(((DataModificationTableReference)tableSource).DataModificationSpecification.OutputIntoClause.IntoTable);
                        }
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.InlineDerivedTable":
                        // NOP
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.QueryDerivedTable":
                        findIdentifiers(((QueryDerivedTable)tableSource).QueryExpression);
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.SchemaObjectFunctionTableReference":
                        findIdentifiers(((SchemaObjectFunctionTableReference)tableSource).SchemaObject);
                        break;
                    case "Microsoft.SqlServer.TransactSql.ScriptDom.VariableTableReference":
                        // Not intested in Variables...
                        break;
                    //case "Microsoft.SqlServer.TransactSql.ScriptDom.VariableTableSource":
                    //    // Not intested in Variables...
                    //    break;


                    default:
                        throw new Exception("Unhandled Statement Type in findIdentifiers(TableSource tableSource) " + tableSource.GetType().FullName);
                }
            }
        }

        /// <summary>
        /// Finds alias' when they show up.
        /// </summary>
        /// <param name="aliasSource"></param>
        private void findIdentifiers(Microsoft.SqlServer.TransactSql.ScriptDom.Identifier aliasSource)
        {
            Identifier foundID = new Identifier(string.Empty, string.Empty, string.Empty, aliasSource.Value, Identifier.IdentifierEnum.Alias);
            if (!_identifiers.ContainsKey(foundID.ToString(false, true)))
            {
                _identifiers.Add(foundID.ToString(false, true), foundID);
            }
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
            if (!_identifiers.ContainsKey(foundID.ToString(false, true)))
            {
                _identifiers.Add(foundID.ToString(false, true), foundID);
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
            if (!_identifiers.ContainsKey(foundID.ToString(false, true)))
            {
                _identifiers.Add(foundID.ToString(false, true), foundID);
            }
        }

        private void findIdentifiers(Microsoft.SqlServer.TransactSql.ScriptDom.Identifier objectName, Identifier.IdentifierEnum objectType)
        {
            Identifier foundID = new Identifier(string.Empty, string.Empty, string.Empty, objectName.Value, objectType);
            if (!_identifiers.ContainsKey(foundID.ToString(false, true)))
            {
                _identifiers.Add(foundID.ToString(false, true), foundID);
            }
        }
        #endregion
    }
}
