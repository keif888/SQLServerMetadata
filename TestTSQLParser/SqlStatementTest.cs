using TSQLParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
//using Microsoft.Data.Schema.ScriptDom.Sql;
using Microsoft.SqlServer.TransactSql.ScriptDom;
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
            string sqlString = @"SELECT DISTINCT UX002_ExtractId, 
DATEADD(day, CAST(SUBSTRING(UX002_ExtractId, 5, 3) AS INT) - 1, DATEADD(year, CAST(SUBSTRING(UX002_ExtractId, 3, 2) AS INT), cast('2000-01-01' +' '+Convert(varchar,getdate(),8) AS datetime) ) ) AS ProcessDate
FROM dbo.RolloutOutfile
ORDER BY
DATEADD(day, CAST(SUBSTRING(UX002_ExtractId, 5, 3) AS INT) - 1, DATEADD(year, CAST(SUBSTRING(UX002_ExtractId, 3, 2) AS INT), cast('2000-01-01' +' '+Convert(varchar,getdate(),8) AS datetime)))";
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
            string sqlString = @"-- Create the recursive CTE to find all of Bonnie's ancestors.
                                WITH Generation (ID) AS
                                (
                                -- First anchor member returns Bonnie's mother.
                                    SELECT Mother 
                                    FROM dbo.Person
                                    WHERE Name = 'Bonnie'
                                UNION
                                -- Second anchor member returns Bonnie's father.
                                    SELECT Father 
                                    FROM dbo.Person
                                    WHERE Name = 'Bonnie'
                                UNION ALL
                                -- First recursive member returns male ancestors of the previous generation.
                                    SELECT Person.Father
                                    FROM Generation, Person
                                    WHERE Generation.ID=Person.ID
                                UNION ALL
                                -- Second recursive member returns female ancestors of the previous generation.
                                    SELECT Person.Mother
                                    FROM Generation, dbo.Person
                                    WHERE Generation.ID=Person.ID
                                )
                                SELECT Person.ID, Person.Name, Person.Mother, Person.Father
                                FROM Generation, dbo.Person
                                WHERE Generation.ID = Person.ID;";
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
            string sqlString = @"WITH Parts(AssemblyID, ComponentID, PerAssemblyQty, EndDate, ComponentLevel) AS
                                (
                                    SELECT b.ProductAssemblyID, b.ComponentID, b.PerAssemblyQty,
                                        b.EndDate, 0 AS ComponentLevel
                                    FROM Production.BillOfMaterials AS b
                                    WHERE b.ProductAssemblyID = 800
                                          AND b.EndDate IS NULL
                                    UNION ALL
                                    SELECT bom.ProductAssemblyID, bom.ComponentID, p.PerAssemblyQty,
                                        bom.EndDate, ComponentLevel + 1
                                    FROM Production.BillOfMaterials AS bom 
                                        INNER JOIN Parts AS p
                                        ON bom.ProductAssemblyID = p.ComponentID
                                        AND bom.EndDate IS NULL
                                )
                                UPDATE Production.BillOfMaterials
                                SET PerAssemblyQty = c.PerAssemblyQty * 2
                                FROM Production.BillOfMaterials AS c
                                JOIN Parts AS d ON c.ProductAssemblyID = d.AssemblyID
                                WHERE d.ComponentLevel = 0;";
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
            Assert.AreEqual("Error Number: 46010\r\nMessage: Incorrect syntax near select.\r\nLine: 1\r\nOffset: 25", actual[0]);
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
        public void execStatement()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "EXEC [dbo].[usp_GetData];";
            target.ParseString(sqlString);
            actual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(expected.Count, actual.Count);
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
            string sqlString = @"SELECT CASE WHEN Col1 = 'A' THEN 'B' 
    ELSE 'C' END 
FROM [dbo].[TestTable] 
WHERE CASE WHEN Col1 = 'A' THEN 'B' 
    ELSE 'C' 
END = 'C';";
            expected.Add("[dbo].[TestTable]");
            target.ParseString(sqlString);
            actual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }

        [TestMethod()]
        public void basicDeleteStatement()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            List<string> expected = new List<string>();
            List<string> actual;
            string sqlString = "DELETE FROM [dbo].[TestTable];";
            expected.Add("[dbo].[TestTable]");
            target.ParseString(sqlString);
            actual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
        }

        [TestMethod()]
        public void fullOuterJoinStatement()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            string sqlString = @"SELECT p1.PLAYERID,
    f1.PLAYERNAME,
    p2.PLAYERID,
    f2.PLAYERNAME
FROM   PLAYER f1,
        PLAYER f2,
        PLAYS p1
FULL OUTER JOIN PLAYS p2
	ON p1.PLAYERID < p2.PLAYERID
	AND p1.TEAMID = p2.TEAMID
GROUP  BY p1.PLAYERID,
    f1.PLAYERID,
    p2.PLAYERID,
    f2.PLAYERID
HAVING Count(p1.PLAYERID) = Count(*)
    AND Count(p2.PLAYERID) = Count(*)
    AND p1.PLAYERID = f1.PLAYERID
    AND p2.PLAYERID = f2.PLAYERID; ";
            target.ParseString(sqlString);
            List<string> lstrexpected = new List<string>();
            List<string> lstractual;
            lstrexpected.Add("[PLAYER]");
            lstrexpected.Add("[PLAYS]");
            lstractual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            for (int i = 0; i < lstractual.Count; i++)
            {
                Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
            }
        }

        [TestMethod()]
        public void fullMultiSubSelectStatement()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            string sqlString = @"select * from player where player_id in 
(
 select set2.player_id orig
 from
 (select count(*) count,b.player_id , nvl(sum(a.team_id+ascii(team_name)),0) team_value
   from plays a, player b , team c
   where a.player_id=b.player_id
    and a.team_id = c.team_id
   group by b.player_id) set1,
(select count(*) count,b.player_id , nvl(sum(a.team_id+ascii(team_name)),0) team_value
   from plays a, player b , team c
   where a.player_id=b.player_id
    and a.team_id = c.team_id
   group by b.player_id) set2
       where set1.count=set2.count and set1.team_value=set2.team_value
  and set1.player_id<>set2.player_id
)";
            target.ParseString(sqlString);
            List<string> lstrexpected = new List<string>();
            List<string> lstractual;
            lstrexpected.Add("[player]");
            lstrexpected.Add("[plays]");
            lstrexpected.Add("[team]");
            lstractual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            for (int i = 0; i < lstractual.Count; i++)
            {
                Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
            }
        }

        [TestMethod()]
        public void createTableStatement()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            string sqlString = "CREATE TABLE [dbo].[TestMe] ([Col1] NVARCHAR(max));";
            target.ParseString(sqlString);
            List<string> lstrexpected = new List<string>();
            List<string> lstractual;
            lstrexpected.Add("[dbo].[TestMe]");
            lstractual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            for (int i = 0; i < lstractual.Count; i++)
            {
                Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
            }
        }

        [TestMethod()]
        public void createFunctionStatement()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            string sqlString = @"CREATE FUNCTION dbo.Triple(@Input int)
       RETURNS int
AS
BEGIN;
  DECLARE @Result int;
  SET @Result = @Input * 3;
  RETURN @Result;
END;
go";
            target.ParseString(sqlString);
            List<string> lstrexpected = new List<string>();
            List<string> lstractual;
            lstractual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            for (int i = 0; i < lstractual.Count; i++)
            {
                Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
            }
            lstrexpected.Add("[dbo].[Triple]");
            lstractual = target.getFunctionNames(forceSchemaQualified);
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            for (int i = 0; i < lstractual.Count; i++)
            {
                Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
            }
        }

        [TestMethod()]
        public void selectFromTableAndFunctionStatement()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            string sqlString = @"SELECT DataVal, dbo.Triple(DataVal) AS Triple
FROM   dbo.LargeTable;";
            target.ParseString(sqlString);
            List<string> lstrexpected = new List<string>();
            List<string> lstractual;
            lstrexpected.Add("[dbo].[LargeTable]");
            lstractual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            for (int i = 0; i < lstractual.Count; i++)
            {
                Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
            }
            lstrexpected.Clear();
            lstrexpected.Add("[dbo].[Triple]");
            lstractual = target.getFunctionNames(forceSchemaQualified);
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            for (int i = 0; i < lstractual.Count; i++)
            {
                Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
            }
        }

        [TestMethod()]
        public void CreateTableWithFollowingInsert()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            string sqlString = @"CREATE TABLE dbo.LargeTable
  (KeyVal int NOT NULL PRIMARY KEY,
   DataVal int NOT NULL CHECK (DataVal BETWEEN 1 AND 10)
  );

WITH Digits
AS (SELECT d FROM (VALUES (0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) AS d(d))
INSERT INTO dbo.LargeTable (KeyVal, DataVal)
SELECT 10000 * tt.d + 1000 * st.d
     + 100 * h.d + 10 * t.d + s.d + 1,
       10 * RAND(CHECKSUM(NEWID())) + 1
FROM   Digits AS s,  Digits AS t,  Digits AS h,
       Digits AS st, Digits AS tt;";
            target.ParseString(sqlString);
            List<string> lstrexpected = new List<string>();
            List<string> lstractual;
            lstrexpected.Add("[dbo].[LargeTable]");
            lstractual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            for (int i = 0; i < lstractual.Count; i++)
            {
                Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
            }
        }

        [TestMethod()]
        public void CrossApplyStatement()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            string sqlString = @"SELECT *
FROM
  orders o
  CROSS APPLY (
    SELECT *  FROM
       customers c
    WHERE
       o.customerid = c.customerid) AS c
WHERE
  c.companyname = 'Around the Horn';";
            target.ParseString(sqlString);
            List<string> lstrexpected = new List<string>();
            List<string> lstractual;
            lstrexpected.Add("[orders]");
            lstrexpected.Add("[customers]");
            lstractual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            for (int i = 0; i < lstractual.Count; i++)
            {
                Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
            }
        }

        [TestMethod()]
        public void CrossApplyWithExceptStatement()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            string sqlString = @"SELECT *
FROM
    orders o
    CROSS APPLY (SELECT *
                 FROM
                     customers c
                 WHERE
                     o.customerid = c.customerid) AS c
WHERE
    c.companyname = 'Around the Horn'
 
EXCEPT
SELECT *
FROM
    specialorders o
    JOIN specialcustomers c
        ON o.customerid = c.customerid
WHERE
    c.companyname = 'Around the Horn';";
            target.ParseString(sqlString);
            List<string> lstrexpected = new List<string>();
            List<string> lstractual;
            lstrexpected.Add("[orders]");
            lstrexpected.Add("[customers]");
            lstrexpected.Add("[specialorders]");
            lstrexpected.Add("[specialcustomers]");
            lstractual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            for (int i = 0; i < lstractual.Count; i++)
            {
                Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
            }
        }

         [TestMethod()]
        public void BasicCursorStatement()
        {
            SqlStatement target = new SqlStatement();
            bool forceSchemaQualified = false;
            string sqlString = @"declare @CustId nchar(5);
	declare @RowNum int;
	declare CustList cursor for
	select top 5 CustomerID from Northwind.dbo.Customers;
	OPEN CustList;
	FETCH NEXT FROM CustList 
	INTO @CustId;
	set @RowNum = 0 ;
	WHILE @@FETCH_STATUS = 0
	BEGIN
	  set @RowNum = @RowNum + 1;
	  print cast(@RowNum as char(1)) + ' ' + @CustId;
	  FETCH NEXT FROM CustList 
	    INTO @CustId;
	END
	CLOSE CustList;
	DEALLOCATE CustList;";
            target.ParseString(sqlString);
            List<string> lstrexpected = new List<string>();
            List<string> lstractual;
            lstrexpected.Add("[Northwind].[dbo].[Customers]");
            lstractual = target.getTableNames(forceSchemaQualified);
            Assert.AreEqual(lstrexpected.Count, lstractual.Count);
            for (int i = 0; i < lstractual.Count; i++)
            {
                Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
            }
        }

         [TestMethod()]
         public void BasicWithStatement()
         {
             SqlStatement target = new SqlStatement();
             bool forceSchemaQualified = false;
             string sqlString = @"-- Define the CTE expression name and column list.
WITH Sales_CTE (SalesPersonID, SalesOrderID, SalesYear)
AS
-- Define the CTE query.
(
    SELECT SalesPersonID, SalesOrderID, YEAR(OrderDate) AS SalesYear
    FROM Sales.SalesOrderHeader
    WHERE SalesPersonID IS NOT NULL
)
-- Define the outer query referencing the CTE name.
SELECT SalesPersonID, COUNT(SalesOrderID) AS TotalSales, SalesYear
FROM Sales_CTE
GROUP BY SalesYear, SalesPersonID
ORDER BY SalesPersonID, SalesYear;";
             target.ParseString(sqlString);
             List<string> lstrexpected = new List<string>();
             List<string> lstractual;
             lstrexpected.Add("[Sales].[SalesOrderHeader]");
             lstractual = target.getTableNames(forceSchemaQualified);
             Assert.AreEqual(lstrexpected.Count, lstractual.Count);
             for (int i = 0; i < lstractual.Count; i++)
             {
                 Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
             }
         }

         [TestMethod()]
         public void MultipleWithStatement()
         {
             SqlStatement target = new SqlStatement();
             bool forceSchemaQualified = false;
             string sqlString = @"WITH Sales_CTE (SalesPersonID, TotalSales, SalesYear)
AS
-- Define the first CTE query.
(
    SELECT SalesPersonID, SUM(TotalDue) AS TotalSales, YEAR(OrderDate) AS SalesYear
    FROM Sales.SalesOrderHeader
    WHERE SalesPersonID IS NOT NULL
       GROUP BY SalesPersonID, YEAR(OrderDate)

)
,   -- Use a comma to separate multiple CTE definitions.

-- Define the second CTE query, which returns sales quota data by year for each sales person.
Sales_Quota_CTE (BusinessEntityID, SalesQuota, SalesQuotaYear)
AS
(
       SELECT BusinessEntityID, SUM(SalesQuota)AS SalesQuota, YEAR(QuotaDate) AS SalesQuotaYear
       FROM Sales.SalesPersonQuotaHistory
       GROUP BY BusinessEntityID, YEAR(QuotaDate)
)

-- Define the outer query by referencing columns from both CTEs.
SELECT SalesPersonID
  , SalesYear
  , FORMAT(TotalSales,'C','en-us') AS TotalSales
  , SalesQuotaYear
  , FORMAT (SalesQuota,'C','en-us') AS SalesQuota
  , FORMAT (TotalSales -SalesQuota, 'C','en-us') AS Amt_Above_or_Below_Quota
FROM Sales_CTE
JOIN Sales_Quota_CTE ON Sales_Quota_CTE.BusinessEntityID = Sales_CTE.SalesPersonID
                    AND Sales_CTE.SalesYear = Sales_Quota_CTE.SalesQuotaYear
ORDER BY SalesPersonID, SalesYear;";
             target.ParseString(sqlString);
             List<string> lstrexpected = new List<string>();
             List<string> lstractual;
             lstrexpected.Add("[Sales].[SalesOrderHeader]");
             lstrexpected.Add("[Sales].[SalesPersonQuotaHistory]");
             lstractual = target.getTableNames(forceSchemaQualified);
             Assert.AreEqual(lstrexpected.Count, lstractual.Count);
             for (int i = 0; i < lstractual.Count; i++)
             {
                 Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
             }
         }

         [TestMethod()]
         public void multipleAnchorAndRecursiveMembers()
         {
             SqlStatement target = new SqlStatement();
             bool forceSchemaQualified = false;
             string sqlString = @"-- Create the recursive CTE to find all of Bonnie's ancestors.
WITH Generation (ID) AS
(
-- First anchor member returns Bonnie's mother.
    SELECT Mother 
    FROM dbo.Person
    WHERE Name = 'Bonnie'
UNION
-- Second anchor member returns Bonnie's father.
    SELECT Father 
    FROM dbo.Person
    WHERE Name = 'Bonnie'
UNION ALL
-- First recursive member returns male ancestors of the previous generation.
    SELECT Person.Father
    FROM Generation, Person
    WHERE Generation.ID=Person.ID
UNION ALL
-- Second recursive member returns female ancestors of the previous generation.
    SELECT Person.Mother
    FROM Generation, dbo.Person
    WHERE Generation.ID=Person.ID
)
SELECT Person.ID, Person.Name, Person.Mother, Person.Father
FROM Generation, dbo.Person
WHERE Generation.ID = Person.ID;";
             target.ParseString(sqlString);
             List<string> lstrexpected = new List<string>();
             List<string> lstractual;
             lstrexpected.Add("[dbo].[Person]");
             lstrexpected.Add("[Person]");  // One of the calls to Person isn't Qualified.  Dodgy MS Example! http://msdn.microsoft.com/en-us/library/ms175972.aspx
             lstractual = target.getTableNames(forceSchemaQualified);
             Assert.AreEqual(lstrexpected.Count, lstractual.Count);
             for (int i = 0; i < lstractual.Count; i++)
             {
                 Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
             }
         }

         [TestMethod()]
         public void createProcedure()
         {
             SqlStatement target = new SqlStatement();
             bool forceSchemaQualified = false;
             string sqlString = @"USE AdventureWorks2012;
GO
IF OBJECT_ID ( 'HumanResources.uspGetEmployees', 'P' ) IS NOT NULL 
    DROP PROCEDURE HumanResources.uspGetEmployees;
GO
CREATE PROCEDURE HumanResources.uspGetEmployees 
    @LastName nvarchar(50), 
    @FirstName nvarchar(50) 
AS 

    SET NOCOUNT ON;
    SELECT FirstName, LastName, Department
    FROM HumanResources.vEmployeeDepartmentHistory
    WHERE FirstName = @FirstName AND LastName = @LastName;
GO";
             target.ParseString(sqlString);
             List<string> lstrexpected = new List<string>();
             List<string> lstractual;
             lstrexpected.Add("[HumanResources].[vEmployeeDepartmentHistory]");
             lstractual = target.getTableNames(forceSchemaQualified);
             Assert.AreEqual(lstrexpected.Count, lstractual.Count);
             for (int i = 0; i < lstractual.Count; i++)
             {
                 Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
             }
             lstrexpected.Clear();
             lstrexpected.Add("[HumanResources].[uspGetEmployees]");
             lstractual = target.getProcedureNames(forceSchemaQualified);
             Assert.AreEqual(lstrexpected.Count, lstractual.Count);
             for (int i = 0; i < lstractual.Count; i++)
             {
                 Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
             }
         }

         [TestMethod()]
         public void caseSubSelectStatement()
         {
             SqlStatement target = new SqlStatement();
             bool forceSchemaQualified = false;
             string sqlString = @"DECLARE @reportDate datetime
DECLARE @startDateMonthly datetime
DECLARE @protocolID int

SET @reportDate = '2/1/2007'
SET @startDateMonthly = GETDATE()
SET @protocolID = 152

DECLARE @EnrollmentGoal int
DECLARE @NoUSSites int
DECLARE @NoSites int

SELECT @EnrollmentGoal = intEnrollmentGoal, @NoUSSites = intNoUSSites, @NoSites = intNoSites
FROM tblProtocol WHERE intProtocolID = @protocolID 

SELECT
Year(am.theDate)
,DATENAME(month,am.theDate) as TheMonth
,am.theDate

,(SELECT intTransactionID FROM tblEnrollment e2 
	INNER JOIN tblSiteAlias
    ON (e2.intAliasID = tblSiteAlias.intAliasID
    and tblSiteAlias.intProtocolID = @protocolID)
WHERE YEAR(am.theDate) = YEAR(e2.dtmReportedDate)
          AND MONTH(am.theDate) = MONTH(e2.dtmReportedDate)) AS TESTER

,CASE(
SELECT intTransactionID FROM tblEnrollment e2 
	INNER JOIN tblSiteAlias
    ON (e2.intAliasID = tblSiteAlias.intAliasID
    and tblSiteAlias.intProtocolID = @protocolID)
WHERE YEAR(am.theDate) = YEAR(e2.dtmReportedDate)
          AND MONTH(am.theDate) = MONTH(e2.dtmReportedDate))
      WHEN NULL THEN 

(
SELECT TOP (1) tblEnrollment.intEnrollment FROM tblEnrollment 
	INNER JOIN tblSiteAlias ON tblEnrollment.intAliasID = tblSiteAlias.intAliasID
		WHERE (tblSiteAlias.intProtocolID = @protocolID) 
		AND (tblEnrollment.dtmReportedDate < am.theDate)
		AND (tblEnrollment.intEnrollment > 0)
ORDER BY tblEnrollment.dtmReportedDate DESC
)

      ELSE 
(
SELECT TOP 1 IsNull(SUM(intEnrollment),0) FROM tblEnrollment e2 
	INNER JOIN tblSiteAlias
    ON (e2.intAliasID = tblSiteAlias.intAliasID
    and tblSiteAlias.intProtocolID = @protocolID)
WHERE YEAR(am.theDate) = YEAR(e2.dtmReportedDate)
          AND MONTH(am.theDate) = MONTH(e2.dtmReportedDate)
)
END as TotalEnrollment

, IsNull(((CONVERT(REAL, @NoUSSites)/@NoSites) * @EnrollmentGoal),0) as USGoal
, @EnrollmentGoal as [Enrollment Goal]
FROM ALLMONTHS AS am LEFT JOIN tblEnrollment e
     ON (     YEAR(am.theDate) = YEAR(e.dtmReportedDate)
          AND MONTH(am.theDate) = MONTH(e.dtmReportedDate)
        )
WHERE am.theDate > @reportDate and am.theDate <= @startDateMonthly
GROUP BY Year(am.theDate),Month(am.theDate),DATENAME(month,am.theDate), am.theDate
ORDER BY Year(am.theDate) ASC,
Month(am.theDate) ASC;";
             target.ParseString(sqlString);
             List<string> lstrexpected = new List<string>();
             List<string> lstractual;
             lstrexpected.Add("[tblProtocol]");
             lstrexpected.Add("[tblEnrollment]");
             lstrexpected.Add("[tblSiteAlias]");
             lstrexpected.Add("[ALLMONTHS]");
             lstractual = target.getTableNames(forceSchemaQualified);
             Assert.AreEqual(lstrexpected.Count, lstractual.Count);
             for (int i = 0; i < lstractual.Count; i++)
             {
                 Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
             }
         }


         [TestMethod()]
         public void execSelectStatement()
         {
             SqlStatement target = new SqlStatement();
             bool forceSchemaQualified = false;
             string sqlString = @"EXEC ('SELECT * FROM sys.tables');";  // ToDo: Handle String Literals for re-parsing.
             target.ParseString(sqlString);
             List<string> lstrexpected = new List<string>();
             List<string> lstractual;
             //lstrexpected.Add("[sys].[tables]");
             lstractual = target.getTableNames(forceSchemaQualified);
             Assert.AreEqual(lstrexpected.Count, lstractual.Count);
             for (int i = 0; i < lstractual.Count; i++)
             {
                 Assert.IsTrue(lstractual.Contains(lstrexpected[i]), String.Format("Value {0} is missing", lstrexpected[i]));
             }
         }

    }
}
