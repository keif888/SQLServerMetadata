using TSQLParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.Data.Schema.ScriptDom.Sql;
using System.Collections.Generic;

namespace TestTSQLParser
{
    
    
    /// <summary>
    ///This is a test class for SqlStatementTest and is intended
    ///to contain all SqlStatementTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SqlStatementTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for SqlStatement Constructor
        ///</summary>
        [TestMethod()]
        public void SqlStatementConstructorTest()
        {
            SqlStatement target = new SqlStatement();
            // Assert.AreEqual(string.Empty, string sqlString);
            Assert.AreEqual(true, target.quotedIdentifiers);
            Assert.AreEqual(0, target.parseErrors.Count);
        }

        /// <summary>
        ///A test for ParseString
        ///</summary>
        [TestMethod()]
        public void ParseStringTest_Empty()
        {
            SqlStatement target = new SqlStatement();
            bool expected = true;
            bool actual;
            actual = target.ParseString(string.Empty);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ParseString
        ///</summary>
        [TestMethod()]
        public void ParseStringTest_Basic()
        {
            SqlStatement target = new SqlStatement();
            bool expected = true;
            bool actual;
            // string sqlString = "select * from sys.tables;";
            actual = target.ParseString("select * from sys.tables;");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ParseString
        ///</summary>
        [TestMethod()]
        public void ParseStringTest_Complex1()
        {
            SqlStatement target = new SqlStatement();
            bool expected = true;
            bool actual;
            string sqlString = "SELECT DISTINCT UX002_ExtractId, \r\n" +
"DATEADD(day, CAST(SUBSTRING(UX002_ExtractId, 5, 3) AS INT) - 1, DATEADD(year, CAST(SUBSTRING(UX002_ExtractId, 3, 2) AS INT), cast('2000-01-01' +' '+Convert(varchar,getdate(),8) AS datetime) ) ) AS ProcessDate \r\n" +
"FROM dbo.RolloutOutfile \r\n" +
"ORDER BY \r\n" +
"DATEADD(day, CAST(SUBSTRING(UX002_ExtractId, 5, 3) AS INT) - 1, DATEADD(year, CAST(SUBSTRING(UX002_ExtractId, 3, 2) AS INT), cast('2000-01-01' +' '+Convert(varchar,getdate(),8) AS datetime))) \r\n";
            actual = target.ParseString(sqlString);
            Assert.AreEqual(expected, actual);
            List<string> lstrexpected = new List<string>();
            List<string> lstractual;
            lstrexpected.Add("[dbo].[RolloutOutfile]");
            lstractual = target.getTableNames();
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            Assert.AreEqual(lstrexpected[0], lstractual[0]);
        }

        /// <summary>
        ///A test for ParseString
        ///</summary>
        [TestMethod()]
        public void ParseStringTest_Complex2()
        {
            SqlStatement target = new SqlStatement();
            bool expected = true;
            bool actual;
            string sqlString = "-- Create the recursive CTE to find all of Bonnie's ancestors.\r\n" +
                                "WITH Generation (ID) AS\r\n" +
                                "(\r\n" +
                                "-- First anchor member returns Bonnie's mother.\r\n" +
                                "    SELECT Mother \r\n" +
                                "    FROM dbo.Person\r\n" +
                                "    WHERE Name = 'Bonnie'\r\n" +
                                "UNION\r\n" +
                                "-- Second anchor member returns Bonnie's father.\r\n" +
                                "    SELECT Father \r\n" +
                                "    FROM dbo.Person\r\n" +
                                "    WHERE Name = 'Bonnie'\r\n" +
                                "UNION ALL\r\n" +
                                "-- First recursive member returns male ancestors of the previous generation.\r\n" +
                                "    SELECT Person.Father\r\n" +
                                "    FROM Generation, Person\r\n" +
                                "    WHERE Generation.ID=Person.ID\r\n" +
                                "UNION ALL\r\n" +
                                "-- Second recursive member returns female ancestors of the previous generation.\r\n" +
                                "    SELECT Person.Mother\r\n" +
                                "    FROM Generation, dbo.Person\r\n" +
                                "    WHERE Generation.ID=Person.ID\r\n" +
                                ")\r\n" +
                                "SELECT Person.ID, Person.Name, Person.Mother, Person.Father\r\n" +
                                "FROM Generation, dbo.Person\r\n" +
                                "WHERE Generation.ID = Person.ID;";
            actual = target.ParseString(sqlString);
            Assert.AreEqual(expected, actual);
            List<string> lstrexpected = new List<string>();
            List<string> lstractual;
            lstrexpected.Add("[dbo].[Person]");
            lstrexpected.Add("[Person]");
            lstractual = target.getTableNames();
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            for (int i = 0; i < lstractual.Count; i++)
            {
                Assert.AreEqual(lstrexpected[i], lstractual[i]);
            }
        }


        /// <summary>
        ///A test for ParseString
        ///</summary>
        [TestMethod()]
        public void ParseStringTest_Complex3()
        {
            SqlStatement target = new SqlStatement();
            bool expected = true;
            bool actual;
            string sqlString = "WITH Parts(AssemblyID, ComponentID, PerAssemblyQty, EndDate, ComponentLevel) AS\r\n" +
                                "(\r\n" +
                                "    SELECT b.ProductAssemblyID, b.ComponentID, b.PerAssemblyQty,\r\n" +
                                "        b.EndDate, 0 AS ComponentLevel\r\n" +
                                "    FROM Production.BillOfMaterials AS b\r\n" +
                                "    WHERE b.ProductAssemblyID = 800\r\n" +
                                "          AND b.EndDate IS NULL\r\n" +
                                "    UNION ALL\r\n" +
                                "    SELECT bom.ProductAssemblyID, bom.ComponentID, p.PerAssemblyQty,\r\n" +
                                "        bom.EndDate, ComponentLevel + 1\r\n" +
                                "    FROM Production.BillOfMaterials AS bom \r\n" +
                                "        INNER JOIN Parts AS p\r\n" +
                                "        ON bom.ProductAssemblyID = p.ComponentID\r\n" +
                                "        AND bom.EndDate IS NULL\r\n" +
                                ")\r\n" +
                                "UPDATE Production.BillOfMaterials\r\n" +
                                "SET PerAssemblyQty = c.PerAssemblyQty * 2\r\n" +
                                "FROM Production.BillOfMaterials AS c\r\n" +
                                "JOIN Parts AS d ON c.ProductAssemblyID = d.AssemblyID\r\n" +
                                "WHERE d.ComponentLevel = 0;";
            actual = target.ParseString(sqlString);
            Assert.AreEqual(expected, actual);
            List<string> lstrexpected = new List<string>();
            List<string> lstractual;
            lstrexpected.Add("[Production].[BillOfMaterials]");
            lstractual = target.getTableNames();
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            for (int i = 0; i < lstractual.Count; i++)
            {
                Assert.AreEqual(lstrexpected[i], lstractual[i]);
            }
        }



        /// <summary>
        ///A test for ParseString
        ///</summary>
        [TestMethod()]
        public void ParseStringTest_Bad0()
        {
            SqlStatement target = new SqlStatement();
            bool expected = false;
            bool actual;
            string sqlString = "select * from sys.tables select;";
            actual = target.ParseString(sqlString);
            Assert.AreEqual(expected, actual);
        }


        /// <summary>
        ///A test for parseErrors
        ///</summary>
        [TestMethod()]
        public void parseErrorsTest()
        {
            SqlStatement target = new SqlStatement();
            List<string> actual;
            actual = target.parseErrors;
            Assert.AreEqual(0, actual.Count);
        }

        /// <summary>
        ///A test for parseErrors
        ///</summary>
        [TestMethod()]
        public void parseErrorsTest1()
        {
            SqlStatement target = new SqlStatement();
            List<string> actual;
            string sqlString = "select * from sys.tables select;";
            target.ParseString(sqlString);
            actual = target.parseErrors;
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("TSP0010 Incorrect syntax near select.", actual[0]);
        }


        /// <summary>
        ///A test for quotedIdentifiers
        ///</summary>
        [TestMethod()]
        public void quotedIdentifiersTest_false()
        {
            SqlStatement target = new SqlStatement();
            bool expected = false;
            bool actual;
            target.quotedIdentifiers = expected;
            actual = target.quotedIdentifiers;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for quotedIdentifiers
        ///</summary>
        [TestMethod()]
        public void quotedIdentifiersTest_true()
        {
            SqlStatement target = new SqlStatement();
            bool expected = true;
            bool actual;
            target.quotedIdentifiers = expected;
            actual = target.quotedIdentifiers;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for getTableNames
        ///</summary>
        [TestMethod()]
        public void getTableNamesTest()
        {
            SqlStatement target = new SqlStatement();
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "select * from sys.tables;";
            expected.Add("[sys].[tables]");
            target.ParseString(sqlString);
            actual = target.getTableNames();
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }

        /// <summary>
        ///A test for getTableNames
        ///</summary>
        [TestMethod()]
        public void getTableNamesTest2()
        {
            SqlStatement target = new SqlStatement();
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "select * from sys.tables inner join sys.columns on tables.id = columns.id;";
            expected.Add("[sys].[tables]");
            expected.Add("[sys].[columns]");
            target.ParseString(sqlString);
            actual = target.getTableNames();
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }


        /// <summary>
        ///A test for findTableNames
        ///</summary>
        [TestMethod()]
        public void findTableNamesTest_schemaObject1()
        {
            SqlStatement target = new SqlStatement();
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "select * from [tables];";
            expected.Add("[tables]");
            target.ParseString(sqlString);
            actual = target.getTableNames();
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }

        /// <summary>
        ///A test for findTableNames
        ///</summary>
        [TestMethod()]
        public void findTableNamesTest_schemaObject2()
        {
            SqlStatement target = new SqlStatement();
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "select * from sys.tables;";
            expected.Add("[sys].[tables]");
            target.ParseString(sqlString);
            actual = target.getTableNames();
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }

        /// <summary>
        ///A test for findTableNames
        ///</summary>
        [TestMethod()]
        public void findTableNamesTest_schemaObject3()
        {
            SqlStatement target = new SqlStatement();
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "select * from testdb.sys.tables;";
            expected.Add("[testdb].[sys].[tables]");
            target.ParseString(sqlString);
            actual = target.getTableNames();
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }

        /// <summary>
        ///A test for findTableNames
        ///</summary>
        [TestMethod()]
        public void findTableNamesTest_schemaObject4()
        {
            SqlStatement target = new SqlStatement();
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "select * from [(local)\\sql2008R2].testdb.sys.tables;";
            expected.Add("[(local)\\sql2008R2].[testdb].[sys].[tables]");
            target.ParseString(sqlString);
            actual = target.getTableNames();
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }

        /// <summary>
        ///A test for parameters (?)
        ///</summary>
        [TestMethod()]
        public void findTableNamesTest_schemaObject5()
        {
            SqlStatement target = new SqlStatement();
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "select * from sys.tables where A=?;";
            expected.Add("[sys].[tables]");
            target.ParseString(sqlString);
            actual = target.getTableNames();
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }

        /// <summary>
        ///A test for getTableNames
        ///</summary>
        [TestMethod()]
        public void getTableNamesTest1()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "select * from tables where A=?;";
            expected.Add("[tables]");
            target.ParseString(sqlString);
            actual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
            forceSchemaQualified = true;
            actual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreNotEqual(expected[0], actual[0]);
        }

        [TestMethod()]
        public void insertStatementTestValues()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "INSERT INTO [dbo].[TestTable] (Col1, Col2) VALUES ('A','B');";
            expected.Add("[dbo].[TestTable]");
            target.ParseString(sqlString);
            actual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }

        [TestMethod()]
        public void insertStatementTestSelect()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "INSERT INTO [dbo].[TestTable] (Col1, Col2) SELECT Col1, Col2 FROM [dbo].[SourceTable];";
            expected.Add("[dbo].[TestTable]");
            expected.Add("[dbo].[SourceTable]");
            target.ParseString(sqlString);
            actual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(true, actual.Contains(expected[0]));
            Assert.AreEqual(true, actual.Contains(expected[1]));
        }

        [TestMethod()]
        public void insertStatementTestExec()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "INSERT INTO [dbo].[TestTable] (Col1, Col2) EXEC [dbo].[usp_GetData];";
            expected.Add("[dbo].[TestTable]");
            target.ParseString(sqlString);
            actual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
            expected.Clear();
            expected.Add("[dbo].[usp_GetData]");
            actual = target.getProcedureNames(forceSchemaQualified);
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }


        [TestMethod()]
        public void caseExpressionSimple()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "SELECT Col1 FROM [dbo].[TestTable] WHERE CASE Col1 WHEN 'A' THEN 'B' ELSE 'C' END = 'C';";
            expected.Add("[dbo].[TestTable]");
            target.ParseString(sqlString);
            actual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }

        [TestMethod()]
        public void caseExpressionSearched()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "SELECT CASE WHEN Col1 = 'A' THEN 'B' ELSE 'C' END FROM [dbo].[TestTable] WHERE CASE WHEN Col1 = 'A' THEN 'B' ELSE 'C' END = 'C';";
            expected.Add("[dbo].[TestTable]");
            target.ParseString(sqlString);
            actual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }
    
    }
}
