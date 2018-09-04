using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSQLParser
{
    public class Identifier
    {
        public enum IdentifierEnum
        {
             Table
            ,Procedure
            ,Function
            ,View
            ,CommonTableExpression
            ,Unknown
            ,Alias
        }

        public string ServerIdentifier { get; private set; }
        public string DatabaseIdentifier { get; private set; }
        public string SchemaIdentifier { get; private set; }
        public string BaseIdentifier { get; private set; }
        public IdentifierEnum IdentifierType { get; set; }
        public int Qualifiers { get; private set; }

        //private List<Column> _columns;
        //public List<string> Columns {
        //    get {
        //        List<string> result = new List<string>();
        //        foreach(Column column in _columns)
        //        {
        //            result.Add(column.ToString());
        //        }
        //        return result;
        //    }
        //}

        public Identifier()
        {
            Qualifiers = 0;
            ServerIdentifier = string.Empty;
            DatabaseIdentifier = string.Empty;
            SchemaIdentifier = string.Empty;
            BaseIdentifier = string.Empty;
            IdentifierType = IdentifierEnum.Unknown; 
        }

        public Identifier(string serverIdentifier, string databaseIdentifier, string schemaIdentifier, string baseIdentifier, IdentifierEnum identifierType)
        {
            Qualifiers = 0;
            ServerIdentifier = serverIdentifier;
            DatabaseIdentifier = databaseIdentifier;
            SchemaIdentifier = schemaIdentifier;
            BaseIdentifier = baseIdentifier;
            IdentifierType = identifierType;
            if (ServerIdentifier != string.Empty)
            {
                Qualifiers++;
            }
            if (DatabaseIdentifier != string.Empty)
            {
                Qualifiers++;
            }
            if (SchemaIdentifier != string.Empty)
            {
                Qualifiers++;
            }
            if (BaseIdentifier != string.Empty)
            {
                Qualifiers++;
            }
            else
            {
                throw new Exception("baseIdentifier can NOT be an empty string");
            }
        }

        public override string ToString()
        {
            string result = string.Empty;
            if (ServerIdentifier != string.Empty)
            {
                result += "[" + ServerIdentifier + "].";
            }
            if (DatabaseIdentifier != string.Empty)
            {
                result += "[" + DatabaseIdentifier + "].";
            }
            else if (result != string.Empty)
            {
                result += ".";
            }
            if (SchemaIdentifier != string.Empty)
            {
                result += "[" + SchemaIdentifier + "].";
            }
            else if (result != string.Empty)
            {
                result += ".";
            }
            result += "[" + BaseIdentifier + "]";
            return result;
        }

        public string ToString(bool forceSchemaQualified)
        {
            string result = string.Empty;
            if (ServerIdentifier != string.Empty)
            {
                result += "[" + ServerIdentifier + "].";
            }
            if (DatabaseIdentifier != string.Empty)
            {
                result += "[" + DatabaseIdentifier + "].";
            }
            else if (result != string.Empty)
            {
                result += ".";
            }
            if (SchemaIdentifier != string.Empty)
            {
                result += "[" + SchemaIdentifier + "].";
            }
            else if (result != string.Empty)
            {
                if (forceSchemaQualified)
                {
                    result += "[dbo].";
                }
                else
                {
                    result += ".";
                }
            }
            else
            {
                if (forceSchemaQualified)
                {
                    result += "[dbo].";
                }
            }
            result += "[" + BaseIdentifier + "]";
            return result;
        }    

        public string ToString(bool forceSchemaQualified, bool asIdentfier)
        {
            if (asIdentfier)
                return string.Format("{0}|{1}", this.IdentifierType, this.ToString(forceSchemaQualified));
            else
                return this.ToString(forceSchemaQualified);
        }
    }
}
