using TSQLParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TestTSQLParser
{
    
    
    /// <summary>
    ///This is a test class for IdentifierTest and is intended
    ///to contain all IdentifierTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IdentifierTest
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
        ///A test for Identifier Constructor
        ///</summary>
        [TestMethod()]
        public void IdentifierConstructorTest()
        {
            string serverIdentifier = "(local)";
            string databaseIdentifier = "master";
            string schemaIdentifier = "dbo";
            string baseIdentifier = "tables";
            Identifier.IdentifierEnum identifierType = Identifier.IdentifierEnum.Table;
            Identifier target = new Identifier(serverIdentifier, databaseIdentifier, schemaIdentifier, baseIdentifier, identifierType);
            Assert.AreEqual("[(local)].[master].[dbo].[tables]", target.ToString());
            Assert.AreEqual(Identifier.IdentifierEnum.Table, target.IdentifierType);
        }

        /// <summary>
        ///A test for Identifier Constructor
        ///</summary>
        [TestMethod()]
        public void IdentifierConstructorTest1()
        {
            Identifier target = new Identifier();
            Assert.AreEqual(String.Empty, target.BaseIdentifier);
            Assert.AreEqual(String.Empty, target.DatabaseIdentifier);
            Assert.AreEqual(String.Empty, target.SchemaIdentifier);
            Assert.AreEqual(String.Empty, target.ServerIdentifier);
            Assert.AreEqual(Identifier.IdentifierEnum.Unknown, target.IdentifierType);
        }

        /// <summary>
        ///A test for Identifier Constructor
        ///</summary>
        [TestMethod()]
        public void IdentifierConstructorTest2()
        {
            bool caught = false;
            string serverIdentifier = "(local)";
            string databaseIdentifier = "master";
            string schemaIdentifier = "dbo";
            string baseIdentifier = string.Empty;
            Identifier.IdentifierEnum identifierType = Identifier.IdentifierEnum.Table;
            try
            {
                Identifier target = new Identifier(serverIdentifier, databaseIdentifier, schemaIdentifier, baseIdentifier, identifierType);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("baseIdentifier can NOT be an empty string", ex.Message);
                caught = true;
            }
            Assert.AreEqual(true, caught);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest()
        {
            Identifier target = new Identifier();
            bool forceSchemaQualified = false;
            string expected = "[]";
            string actual;
            actual = target.ToString(forceSchemaQualified);
            Assert.AreEqual(expected, actual);
            expected = "[dbo].[]";
            forceSchemaQualified = true;
            actual = target.ToString(forceSchemaQualified);
            Assert.AreEqual(expected, actual);
            string serverIdentifier = "(local)";
            string databaseIdentifier = "master";
            string schemaIdentifier = "dbo";
            string baseIdentifier = "tables";
            Identifier.IdentifierEnum identifierType = Identifier.IdentifierEnum.Table;
            target = new Identifier(serverIdentifier, databaseIdentifier, schemaIdentifier, baseIdentifier, identifierType);
            expected = "[(local)].[master].[dbo].[tables]";
            actual = target.ToString(forceSchemaQualified);
            Assert.AreEqual(expected, actual);
            forceSchemaQualified = false;
            target = new Identifier(serverIdentifier, databaseIdentifier, string.Empty, baseIdentifier, identifierType);
            expected = "[(local)].[master]..[tables]";
            actual = target.ToString(forceSchemaQualified);
            Assert.AreEqual(expected, actual);
            forceSchemaQualified = true;
            target = new Identifier(serverIdentifier, databaseIdentifier, string.Empty, baseIdentifier, identifierType);
            expected = "[(local)].[master].[dbo].[tables]";
            actual = target.ToString(forceSchemaQualified);
            Assert.AreEqual(expected, actual);
            target = new Identifier(serverIdentifier, string.Empty, string.Empty, baseIdentifier, identifierType);
            expected = "[(local)]..[dbo].[tables]";
            actual = target.ToString(forceSchemaQualified);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest1()
        {
            Identifier target = new Identifier();
            string expected = "[]";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
            string serverIdentifier = "(local)";
            string databaseIdentifier = "master";
            string schemaIdentifier = "dbo";
            string baseIdentifier = "tables";
            Identifier.IdentifierEnum identifierType = Identifier.IdentifierEnum.Table;
            target = new Identifier(serverIdentifier, databaseIdentifier, schemaIdentifier, baseIdentifier, identifierType);
            expected = "[(local)].[master].[dbo].[tables]";
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(4, target.Qualifiers);
            target = new Identifier(serverIdentifier, databaseIdentifier, string.Empty, baseIdentifier, identifierType);
            expected = "[(local)].[master]..[tables]";
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
            target = new Identifier(serverIdentifier, databaseIdentifier, string.Empty, baseIdentifier, identifierType);
            expected = "[(local)].[master]..[tables]";
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(3, target.Qualifiers);
            target = new Identifier(serverIdentifier, string.Empty, string.Empty, baseIdentifier, identifierType);
            expected = "[(local)]...[tables]";
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(2, target.Qualifiers);
            target = new Identifier(string.Empty, string.Empty, string.Empty, baseIdentifier, identifierType);
            expected = "[tables]";
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(1, target.Qualifiers);
        }
    }
}
