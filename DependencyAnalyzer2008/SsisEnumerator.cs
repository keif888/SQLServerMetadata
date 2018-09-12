#if SQL2012
#define SQLGT2008
#endif
#if SQL2014
#define SQLGT2008
#endif
///
/// Microsoft SQL Server 2008 Business Intelligence Metadata Reporting Samples
/// Dependency Analyzer Sample
/// 
/// Copyright (c) Microsoft Corporation.  All rights reserved.
///
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.IO;


using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using DTR = Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System.Collections.Specialized;
using TSQLParser;
#if SQL2005
using IDTSComponentMetaData = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData90;
using IDTSPipeline = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPipeline90;
using IDTSInput = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInput90;
using IDTSInputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInputColumn90;
using IDTSOutput = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutput90;
using IDTSOutputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutputColumn90;
using IDTSRuntimeConnection = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSRuntimeConnection90;
using IDTSPath = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPath90;
using IDTSCustomProperty = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSCustomProperty90;
#else
using IDTSComponentMetaData = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData100;
using IDTSInput = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInput100;
using IDTSInputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInputColumn100;
using IDTSOutput = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutput100;
using IDTSOutputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutputColumn100;
using IDTSRuntimeConnection = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSRuntimeConnection100;
using IDTSPath = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPath100;
using IDTSCustomProperty = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSCustomProperty100;
#endif
#if SQL2008
using IDTSPipeline = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPipeline100;
#endif
#if SQL2012
using IDTSPipeline = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPipeline100;
#endif
#if SQL2014
using IDTSPipeline = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPipeline100;
#endif
#if SQL2016
using IDTSPipeline = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPipeline130;
#endif
#if SQL2017
using IDTSPipeline = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSPipeline130;
#endif

using Pre2012PackageInfo = Microsoft.SqlServer.Dts.Runtime.PackageInfo;

#if SQLGT2008
using Microsoft.SqlServer.Management.IntegrationServices;
using Post2012PackageInfo = Microsoft.SqlServer.Management.IntegrationServices.PackageInfo;
#endif


namespace Microsoft.Samples.DependencyAnalyzer
{
    /// <summary>
    ///  Enumerates Integration Services objects and the relationships between them. The result of this enumeration is persisted
    ///  in a repository.
    /// </summary>
    class SSISEnumerator
    {
        // 5 access modes used by ole db adapters
        enum AccessMode : int
        {
            AM_OPENROWSET = 0,
            AM_OPENROWSET_VARIABLE = 1,
            AM_SQLCOMMAND = 2,
            AM_SQLCOMMAND_VARIABLE = 3,
            AM_OPENROWSET_FASTLOAD_VARIABLE = 4
        }

        enum SqlStatementSourceType : int
        {
            DirectInput = 1,
            FileConnection = 2,
            Variable = 3
        }

        private Application app;
        private Repository repository;

        private const string dtsxPattern = "*.dtsx";
        private const string ispacPattern = "*.ispac";
        private bool threePartNames;
        private string[] packagePasswords;
        private static StringBuilder outputFromSSISBuilder = new StringBuilder();

        /// <summary>
        /// Different component Class IDs that we understand about
        /// </summary>
        private class ClassIDs
        {
#if SQL2005
            internal const string OleDbSource = "{2C0A8BE5-1EDC-4353-A0EF-B778599C65A0}";
            internal const string ExcelSource = "{B551FCA8-23BD-4719-896F-D8F352A5283C}";
            internal const string FlatFileSource = "{90C7770B-DE7C-435E-880E-E718C92C0573}";
            internal const string RawFileSource = "{E2568105-9550-4F71-A638-B7FE42E66930}";
            internal const string XmlSource = "Microsoft.SqlServer.Dts.Pipeline.XmlSourceAdapter, Microsoft.SqlServer.XmlSrc, Version=9.0.242.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

            internal const string OleDbDestination = "{E2568105-9550-4F71-A638-B7FE42E66922}";
            internal const string SqlDestination = "{5244B484-7C76-4026-9A01-00928EA81550}";
            internal const string FlatFileDest = "{A1DF9F6D-8EE4-4EF0-BB2E-D526130D7A7B}";
            internal const string RawFileDest = "{E2568105-9550-4F71-A638-B7FE42E66933}";

            internal const string Lookup = "{0FB4AABB-C027-4440-809A-1198049BF117}";
            internal const string FuzzyLookup = "{9F4EB4D4-AD71-496D-B70B-31ECE1139884}";

            internal const string ManagedComponentWrapper = "{bf01d463-7089-41ee-8f05-0a6dc17ce633}";

            internal const string DerivedColumn = "{9CF90BF0-5BCC-4C63-B91D-1F322DC12C26}";
            internal const string MultipleHash = "Martin.SQLServer.Dts.MultipleHash, MultipleHash2005, Version=1.0.0.0, Culture=neutral, PublicKeyToken=51c551904274ab44";
            internal const string KimballSCD = "MouldingAndMillwork.SSIS.KimballMethodSCD, KimballMethodSCD90, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8b0551303405e96c";
            internal const string OLEDBCommand = "{C60ACAD1-9BE8-46B3-87DA-70E59EADEA46}";
#endif
#if SQL2008
            internal const string OleDbSource = "{BCEFE59B-6819-47F7-A125-63753B33ABB7}";
            internal const string ExcelSource = "{A4B1E1C8-17F3-46C8-AAD0-34F0C6FE42DE}";
            internal const string FlatFileSource = "{5ACD952A-F16A-41D8-A681-713640837664}";
            internal const string RawFileSource = "{51DC0B24-7421-45C3-B4AB-9481A683D91D}";
            internal const string XmlSource = "Microsoft.SqlServer.Dts.Pipeline.XmlSourceAdapter, Microsoft.SqlServer.XmlSrc, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

            internal const string OleDbDestination = "{5A0B62E8-D91D-49F5-94A5-7BE58DE508F0}";
            internal const string SqlDestination = "{E4B61516-847B-4BDF-9CC6-1968A2D43E73}";
            internal const string FlatFileDest = "{D658C424-8CF0-441C-B3C4-955E183B7FBA}";
            internal const string RawFileDest = "{485E7329-8754-42B4-AA5B-29C5DA09CAD5}";
            internal const string ExcelDestination = "{C9269E28-EBDE-4DED-91EB-0BF42842F9F4}";

            internal const string Lookup = "{27648839-180F-45E6-838D-AFF53DF682D2}";
            internal const string FuzzyLookup = "{5056651F-F227-4978-94DF-53CDF9E8CCB6}";

            internal const string ManagedComponentWrapper = "{2E42D45B-F83C-400F-8D77-61DDE6A7DF29}";

            internal const string DerivedColumn = "{2932025B-AB99-40F6-B5B8-783A73F80E24}";
            internal const string MultipleHash = "Martin.SQLServer.Dts.MultipleHash, MultipleHash2008, Version=1.0.0.0, Culture=neutral, PublicKeyToken=51c551904274ab44";
            internal const string KimballSCD = "MouldingAndMillwork.SSIS.KimballMethodSCD, KimballMethodSCD100, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8b0551303405e96c";
            internal const string OLEDBCommand = "{8E61C8F6-C91D-43B6-97EB-3423C06571CC}";
#endif
#if SQL2012
            internal const string OleDbSource = "{165A526D-D5DE-47FF-96A6-F8274C19826B}";
            internal const string ExcelSource = "{8C084929-27D1-479F-9641-ABB7CDADF1AC}";
            internal const string FlatFileSource = "{D23FD76B-F51D-420F-BBCB-19CBF6AC1AB4}";
            internal const string RawFileSource = "{480C7D5A-CE63-405C-B338-3C7F26560EE3}";
            internal const string XmlSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}"; //"Microsoft.SqlServer.Dts.Pipeline.XmlSourceAdapter, Microsoft.SqlServer.XmlSrc, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

            internal const string OleDbDestination = "{4ADA7EAA-136C-4215-8098-D7A7C27FC0D1}";
            internal const string SqlDestination = "{F452EAF3-5EF0-43F1-8067-09DDF0BC6316}";
            internal const string FlatFileDest = "{8DA75FED-1B7C-407D-B2AD-2B24209CCCA4}";
            internal const string RawFileDest = "{04762BB6-892F-4EE6-AD46-9CEB0A7EC7A2}";
            internal const string ExcelDestination = "{1F5D5712-2FBA-4CB9-A95A-86C1F336E1DA}";

            internal const string Lookup = "{671046B0-AA63-4C9F-90E4-C06E0B710CE3}";
            internal const string FuzzyLookup = "{E4A5F949-EC93-45AB-8B36-B52936257EC2}";

            internal const string ManagedComponentWrapper = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}"; //Script Component? "{2E42D45B-F83C-400F-8D77-61DDE6A7DF29}";

            internal const string DerivedColumn = "{49928E82-9C4E-49F0-AABE-3812B82707EC}";
            internal const string MultipleHash = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}"; // "Martin.SQLServer.Dts.MultipleHash, MultipleHash2012, Version=1.0.0.0, Culture=neutral, PublicKeyToken=51c551904274ab44";
            internal const string KimballSCD = "MouldingAndMillwork.SSIS.KimballMethodSCD, KimballMethodSCD110, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8b0551303405e96c";
            internal const string OLEDBCommand = "{93FFEC66-CBC8-4C7F-9C6A-CB1C17A7567D}";

            //internal const string ADONetSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string CDCSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string DataMiningModel = "{3D9FFAE9-B89B-43D9-80C8-B97D2740C746}";
            //internal const string DataReaderDestination = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string DimensionProcessing = "{2C2F0891-3AAA-4865-A676-D7476FE4CE90}";
            //internal const string ODBCDestination = "{074B8736-CD73-40A5-822E-888215AF57DA}";
            //internal const string ODBCSource = "{A77F5655-A006-443A-9B7E-90B6BD55CB84}";
            //internal const string PartitionProcessing = "{DA510FB7-E3A8-4D96-9F59-55E15E67FE3D}";
            //internal const string RecordSetDestination = "{C457FD7E-CE98-4C4B-AEFE-F3AE0044F181}";
            //internal const string SQLServerCompactDestination = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";


#endif
#if SQL2014
            internal const string OleDbSource = "{C8D886B3-1825-4FCC-94DE-C3F108986E21}";
            internal const string ExcelSource = "{9F5C585F-2F02-4622-B273-F75D52419D4A}";
            internal const string FlatFileSource = "{C4D48377-EFD6-4C95-9A0B-049219453431}";
            internal const string RawFileSource = "{480C7D5A-CE63-405C-B338-3C7F26560EE3}";
            internal const string XmlSource = "{33D831DE-5DCF-48F0-B431-4D327B9E785D}"; // same as Managed Component Wrapper. //"Microsoft.SqlServer.Dts.Pipeline.XmlSourceAdapter, Microsoft.SqlServer.XmlSrc, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

            internal const string OleDbDestination = "{4ADA7EAA-136C-4215-8098-D7A7C27FC0D1}";
            internal const string SqlDestination = "{E6CBD480-2E7E-4419-A475-07A015FA2FF6}";
            internal const string FlatFileDest = "{FD4FFB90-EECF-4B5A-A3A7-DE2E1FA8052C}";
            internal const string RawFileDest = "{04762BB6-892F-4EE6-AD46-9CEB0A7EC7A2}";
            internal const string ExcelDestination = "{90E2E609-1207-4CB0-A8CE-CC7B8CFE2510}";

            internal const string Lookup = "{9345248B-9709-4C04-90C1-0853F8B68EE8}";
            internal const string FuzzyLookup = "{AD9B9B83-DB60-4188-B57D-93C5155DFACC}";

            internal const string ManagedComponentWrapper = "{33D831DE-5DCF-48F0-B431-4D327B9E785D}"; //Script Component? "{2E42D45B-F83C-400F-8D77-61DDE6A7DF29}";

            internal const string DerivedColumn = "{18E9A11B-7393-47C5-9D47-687BE04A6B09}";
            internal const string MultipleHash = "Martin.SQLServer.Dts.MultipleHash, MultipleHash2014, Version=1.0.0.0, Culture=neutral, PublicKeyToken=51c551904274ab44";
            internal const string KimballSCD = "MouldingAndMillwork.SSIS.KimballMethodSCD, KimballMethodSCD120, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8b0551303405e96c";
            internal const string OLEDBCommand = "{93FFEC66-CBC8-4C7F-9C6A-CB1C17A7567D}";

            //internal const string ADONetSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string CDCSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string DataMiningModel = "{3D9FFAE9-B89B-43D9-80C8-B97D2740C746}";
            //internal const string DataReaderDestination = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string DimensionProcessing = "{2C2F0891-3AAA-4865-A676-D7476FE4CE90}";
            //internal const string ODBCDestination = "{074B8736-CD73-40A5-822E-888215AF57DA}";
            //internal const string ODBCSource = "{A77F5655-A006-443A-9B7E-90B6BD55CB84}";
            //internal const string PartitionProcessing = "{DA510FB7-E3A8-4D96-9F59-55E15E67FE3D}";
            //internal const string RecordSetDestination = "{C457FD7E-CE98-4C4B-AEFE-F3AE0044F181}";
            //internal const string SQLServerCompactDestination = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";


#endif
#if SQL2016
            internal const string OleDbSource = "{657B7EBE-0A54-4C0E-A80E-7A5BD9886C25}";
            internal const string ExcelSource = "{99E18AC3-6E6A-4BF9-AFB2-93D139E6CAAF}";
            internal const string FlatFileSource = "{E32A96E1-7118-4CB5-A338-AC69E815CB13}";
            internal const string RawFileSource = "{036C7941-58DF-4CD3-BF13-E5373258D001}";
            internal const string XmlSource = "{4F885D04-B578-47B7-94A0-DE9C7DA25EE2}";

            internal const string OleDbDestination = "{7B729B0A-4EA5-4A0D-871A-B6E7618E9CFB}";
            internal const string SqlDestination = "{8AA67354-E39A-4951-8F52-D80D057E7BDC}";
            internal const string FlatFileDest = "{44152846-E5A6-4EDB-8B8E-7072209A7662}";
            internal const string RawFileDest = "{953B7879-214C-4AB8-AA79-AEA630F35C46}";
            internal const string ExcelDestination = "{EDFD5EC4-D128-423D-B962-B586F33F0125}";

            internal const string Lookup = "{EDED6488-9C19-496D-9F1D-99E1DB55AF77}";
            internal const string FuzzyLookup = "{760D7EC0-AEC6-4BEA-9CC5-E591884B6ED5}";

            internal const string ManagedComponentWrapper = "{4F885D04-B578-47B7-94A0-DE9C7DA25EE2}"; //Script Component? "{2E42D45B-F83C-400F-8D77-61DDE6A7DF29}";

            internal const string DerivedColumn = "{A2034F5D-D283-4218-A4AF-AE11AD34B886}";
            internal const string MultipleHash = "Martin.SQLServer.Dts.MultipleHash, MultipleHash2016, Version=1.0.0.0, Culture=neutral, PublicKeyToken=51c551904274ab44";
            internal const string KimballSCD = "MouldingAndMillwork.SSIS.KimballMethodSCD, KimballMethodSCD130, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8b0551303405e96c";
            internal const string OLEDBCommand = "{97720936-CE0C-4D46-8CEC-89977421806C}";

            //internal const string ADONetSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string CDCSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string DataMiningModel = "{3D9FFAE9-B89B-43D9-80C8-B97D2740C746}";
            //internal const string DataReaderDestination = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string DimensionProcessing = "{2C2F0891-3AAA-4865-A676-D7476FE4CE90}";
            //internal const string ODBCDestination = "{074B8736-CD73-40A5-822E-888215AF57DA}";
            //internal const string ODBCSource = "{A77F5655-A006-443A-9B7E-90B6BD55CB84}";
            //internal const string PartitionProcessing = "{DA510FB7-E3A8-4D96-9F59-55E15E67FE3D}";
            //internal const string RecordSetDestination = "{C457FD7E-CE98-4C4B-AEFE-F3AE0044F181}";
            //internal const string SQLServerCompactDestination = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";


#endif
#if SQL2017
            internal const string OleDbSource = "{B1E5F848-DF04-4E9B-83EA-1D7D9FAFEF4C}";
            internal const string ExcelSource = "{E5E85051-985D-4599-9FC9-AFA26099D5C4}";
            internal const string FlatFileSource = "{84CD7B6C-16B8-4D90-AC2D-17ED3E4B1155}";
            internal const string RawFileSource = "{682BA583-31D4-47D3-99B9-54018E35C093}";
            internal const string XmlSource = "{8DC69D45-2AD5-40C6-AAEC-25722F92D6FC}"; //"Microsoft.SqlServer.Dts.Pipeline.XmlSourceAdapter, Microsoft.SqlServer.XmlSrc, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

            internal const string OleDbDestination = "{FF6D802E-67E2-439D-9CBC-695EB22CDA4A}";
            internal const string SqlDestination = "{85B8BCCA-5D6A-49A6-8C9B-B1AE7B8DD5E6}";
            internal const string FlatFileDest = "{0667D728-1E5F-4BB6-863D-B29D3F405706}";
            internal const string RawFileDest = "{8E8B85F7-B7D4-4E21-BBDF-8090E532BBF9}";
            internal const string ExcelDestination = "{AE305C05-EB10-4C73-A293-4CF3A2B15169}";

            internal const string Lookup = "{728A46CB-3AB1-4141-B8C9-BA93A3901FBC}";
            internal const string FuzzyLookup = "{BC1E1984-30EB-43A6-81C1-748B3FDFA286}";

            internal const string ManagedComponentWrapper = "{8DC69D45-2AD5-40C6-AAEC-25722F92D6FC}"; //Script Component? "{2E42D45B-F83C-400F-8D77-61DDE6A7DF29}";

            internal const string DerivedColumn = "{692A88CF-7641-45B7-8E01-7BEE602D40EE}";
            internal const string MultipleHash = "Martin.SQLServer.Dts.MultipleHash, MultipleHash2014, Version=1.0.0.0, Culture=neutral, PublicKeyToken=51c551904274ab44";
            internal const string KimballSCD = "MouldingAndMillwork.SSIS.KimballMethodSCD, KimballMethodSCD100, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8b0551303405e96c";
            internal const string OLEDBCommand = "{1E21FD10-A6DF-4280-A583-4E3A87002286}";

            //internal const string ADONetSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string CDCSource = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string DataMiningModel = "{3D9FFAE9-B89B-43D9-80C8-B97D2740C746}";
            //internal const string DataReaderDestination = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";
            //internal const string DimensionProcessing = "{2C2F0891-3AAA-4865-A676-D7476FE4CE90}";
            //internal const string ODBCDestination = "{074B8736-CD73-40A5-822E-888215AF57DA}";
            //internal const string ODBCSource = "{A77F5655-A006-443A-9B7E-90B6BD55CB84}";
            //internal const string PartitionProcessing = "{DA510FB7-E3A8-4D96-9F59-55E15E67FE3D}";
            //internal const string RecordSetDestination = "{C457FD7E-CE98-4C4B-AEFE-F3AE0044F181}";
            //internal const string SQLServerCompactDestination = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";


#endif

        }

        private const string objectTypePackage = "SSIS Package";

        private const string attributePackageLocation = "PackageLocation";
        private const string attributePackageID = "PackageGUID";

        private OleDbConnectionStringBuilder connectionStringBuilder = new OleDbConnectionStringBuilder();

        /// <summary>
        /// information about registered pipeline components
        /// </summary>
        private PipelineComponentInfos pipelineComponentInfos = null;

        /// <summary>
        ///  information about registered connections
        /// </summary>
        private ConnectionInfos connectionInfos = null;

        /// <summary>
        /// information about registered tasks
        /// </summary>
        private TaskInfos taskInfos = null;

        private Dictionary<string, string> connectionTypeToIDMap = null;

        public SSISEnumerator()
        {
        }

        public bool Initialize(Repository repository)
        {
            bool success;

            try
            {
                this.repository = repository;

                // lets handle the infos enumeration right here.
                app = new Application();

                EnumerateInfos(app);

                success = true;
                threePartNames = false;

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(string.Format("Could not initialize the SSIS Packages Enumerator: {0}", ex.Message));
                success = false;
            }

            return success;
        }

        /// <summary>
        ///  enumerates all packages stored off in SQL Server database
        /// </summary>
        /// <param name="sqlConnectionString"></param>
        public void EnumerateSqlPackages(string server, string user, string pwd, string[] rootFolders, bool storeThreePartNames, string[] storePackagePasswords)
        {
            bool recurseSubFolders = true;

            threePartNames = storeThreePartNames;
            packagePasswords = storePackagePasswords;
            try
            {
                Queue<string> folders = new Queue<string>();

                #region SQL Hosted Packages

                foreach (string folderName in rootFolders)
                {
                    folders.Enqueue(folderName);
                }

                if (folders.Count == 0)
                {
                    folders.Enqueue("\\"); // the root folder
                }

                do
                {
                    string folder = folders.Dequeue();

                    PackageInfos packageInfos = app.GetPackageInfos(folder, server, user, pwd);
                    foreach (Pre2012PackageInfo packageInfo in packageInfos)
                    {
                        string location = packageInfo.Folder + "\\" + packageInfo.Name;
                        if (packageInfo.Flags == DTSPackageInfoFlags.Folder)
                        {
                            folders.Enqueue(location);
                        }
                        else
                        {
                            Debug.Assert(packageInfo.Flags == DTSPackageInfoFlags.Package);

                            try
                            {
                                Console.Write(string.Format("Loading SQL package '{0}'... ", location));
                                using (Package package = app.LoadFromSqlServer(location, server, user, pwd, null))
                                {
                                    EnumeratePackage(package, location);
                                }
                                Console.WriteLine("Completed Successfully.");
                            }
                            catch (Microsoft.SqlServer.Dts.Runtime.DtsRuntimeException dtsEx)
                            {
                                if (dtsEx.Message.Contains("The package is encrypted with a password"))
                                {
                                    try
                                    {
                                        // The package was encrypted.  Try to decrypt the sucker!
                                        using (Package package = LoadPackage(location, server, user, pwd, null))
                                        {
                                            if (package != null)
                                                EnumeratePackage(package, location);
                                            else
                                                Console.WriteLine(string.Format("Unable to decrypt package {0} with passwords provided.", location));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(string.Format("Error occurred: '{0}'", ex.Message));
                                        repository.Rollback();
                                        Console.WriteLine("This package has been rolled back.");
                                    }
                                    finally
                                    {
                                        // Commit each package as completed to reduce instance of data loss due to unexpected failures, and associated rollback.
                                        repository.Commit();
                                    }
                                }
                                else
                                {
                                    Console.WriteLine(string.Format("Error occurred: '{0}'", dtsEx.Message));
                                    repository.Rollback();
                                    Console.WriteLine("This package has been rolled back.");
                                }
                            }
                            catch (System.Exception ex2)
                            {
                                Console.WriteLine(string.Format("Error occurred: '{0}'", ex2.Message));
                                repository.Rollback();
                                Console.WriteLine("This package has been rolled back.");
                            }
                            finally
                            {
                                // Commit each package, as we have per package rollbacks now.
                                repository.Commit();
                            }
                        }
                    }
                } while (folders.Count > 0);
                #endregion

#if SQLGT2008
                #region SQL Catalog Hosted Packages

                foreach (string folderName in rootFolders)
                {
                    folders.Enqueue(folderName);
                }

                if (folders.Count == 0)
                {
                    folders.Enqueue("\\"); // the root folder
                }

                do
                {
                    string folder = folders.Dequeue();
                    string connectionString = @"Data Source={0};Initial Catalog=master;";

                    if (String.IsNullOrEmpty(user))
                    {
                        connectionString += "Integrated Security=SSPI;";
                    }
                    else
                    {
                        connectionString += String.Format("User ID={0};Password={1};", user, pwd);
                    }
                    System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(String.Format(connectionString, server));
                    connection.Open();
                    IntegrationServices integrationServices = new IntegrationServices(connection);

                    string tempFolder = System.IO.Path.GetTempPath() + @"\SSISMD" + Guid.NewGuid().ToString();
                    if (!System.IO.Directory.Exists(tempFolder))
                    {
                        System.IO.Directory.CreateDirectory(tempFolder);
                    }
                    System.IO.DirectoryInfo tempDirectory = new System.IO.DirectoryInfo(tempFolder);

                    // Strip the leading \ as SSISDB doesn't have a leading \...
                    if (folder[0] == '\\')
                    {
                        folder = folder.Substring(1, folder.Length - 1);
                    }

                    foreach (Catalog catalog in integrationServices.Catalogs)
                    {
                        if (folder.Length == 0 && recurseSubFolders)
                        {
                            foreach (CatalogFolder catalogFolder in catalog.Folders)
                            {
                                foreach (ProjectInfo project in catalogFolder.Projects)
                                {
                                    EnumerateProjectPackages(project, tempDirectory, server);
                                }
                            }
                        }
                        else if (catalog.Folders.Contains(folder))
                        {
                            CatalogFolder catalogFolder = catalog.Folders[folder];
                            foreach (ProjectInfo project in catalogFolder.Projects)
                            {
                                EnumerateProjectPackages(project, tempDirectory, server);
                            }
                        }
                    }
                    connection.Close();
                    if (tempDirectory.Exists)
                        tempDirectory.Delete(true);

                    // Commit each folder as completed to reduce instance of data loss due to unexpected failures.
                    repository.Commit();
                } while (folders.Count > 0);
                #endregion
#endif

                #region SSIS Hosted Packages
                foreach (string folderName in rootFolders)
                {
                    folders.Enqueue(folderName);
                }

                if (folders.Count == 0)
                {
                    folders.Enqueue("\\"); // the root folder
                }

                do
                {
                    string folder = folders.Dequeue();
                    // String the instance name from the server string, as SSIS doesn't support multiple instance.
                    if (server.Contains("\\"))
                    {
                        server = server.Substring(0,server.IndexOf('\\'));
                    }
                    PackageInfos packageInfos = app.GetDtsServerPackageInfos(folder, server);
                    foreach (Pre2012PackageInfo packageInfo in packageInfos)
                    {
                        string location = packageInfo.Folder + (packageInfo.Folder == "\\" ? "" : "\\") + packageInfo.Name;
                        if (packageInfo.Flags == DTSPackageInfoFlags.Folder)
                        {
                            folders.Enqueue(location);
                        }
                        else
                        {
                            Debug.Assert(packageInfo.Flags == DTSPackageInfoFlags.Package);

                            try
                            {
                                Console.Write(string.Format("Loading SSIS package '{0}'... ", location));
                                using (Package package = app.LoadFromDtsServer(location, server, null))
                                {
                                    EnumeratePackage(package, location);
                                }
                                Console.WriteLine("Completed Successfully.");
                            }
                            catch (Microsoft.SqlServer.Dts.Runtime.DtsRuntimeException dtsEx)
                            {
                                if (dtsEx.Message.Contains("The package is encrypted with a password"))
                                {
                                    try
                                    {
                                        // The package was encrypted.  Try to decrypt the sucker!
                                        using (Package package = LoadPackage(location, server, null))
                                        {
                                            if (package != null)
                                                EnumeratePackage(package, location);
                                            else
                                                Console.WriteLine(string.Format("Unable to decrypt package {0} with passwords provided.", location));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(string.Format("Error occurred: '{0}'", ex.Message));
                                        repository.Rollback();
                                        Console.WriteLine("This package has been rolled back.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine(string.Format("Error occurred: '{0}'", dtsEx.Message));
                                    repository.Rollback();
                                    Console.WriteLine("This package has been rolled back.");
                                }
                            }
                            catch (System.Exception ex2)
                            {
                                Console.WriteLine(string.Format("Error occurred: '{0}'", ex2.Message));
                                repository.Rollback();
                                Console.WriteLine("This package has been rolled back.");
                            }
                            finally
                            {
                                // Commit each package as completed to reduce instance of data loss due to unexpected failures, and associated rollback.
                                repository.Commit();
                            }
                        }
                    }
                } while (folders.Count > 0);
                #endregion


            
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(string.Format("Error enumerating packages on SQL Server '{0}': {1}", server, ex.Message));
                Console.WriteLine(string.Format("Stack Trace :{0}", ex.StackTrace));
                if (ex.InnerException != null)
                {
                    Console.WriteLine(string.Format("Inner Exception is {0}", ex.InnerException.Message));
                    Console.WriteLine(string.Format("Inner Exception Stack Trace :{0}", ex.InnerException.StackTrace));
                }
            }
        }


#if SQLGT2008
        /// <summary>
        /// Using an SSIS catalog project, get all the SSIS packages that are contained, and parse them.
        /// </summary>
        /// <param name="project">SSIS Catalog Project</param>
        /// <param name="tempDirectory">Directory to use for temporary files</param>
        private void EnumerateProjectPackages(ProjectInfo project, DirectoryInfo tempDirectory, String server)
        {
            string projectNameFile = tempDirectory.FullName + @"\" + project.Name + ".ispac";
            String locationName = server + @"\" + project.Parent.Parent.Name + @"\" + project.Parent.Name + @"\" + project.Name;

            // Write the project content to a zip file
            System.IO.File.WriteAllBytes(projectNameFile, project.GetProjectBytes());

            // Load the project content.
            EnumerateIntegrationServicePack(projectNameFile, locationName);

            // Cleanup
            try
            {
                System.IO.File.Delete(projectNameFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error {0} occurred whilst attempting to cleanup temporary files.\r\n{1}\r\n{2}\r\nWith stack trace {3}.", ex.Message, projectNameFile, tempDirectory.FullName + @"\" + project.Name, ex.StackTrace));
            }
        }

        /// <summary>
        /// Loads the ispac file that is referenced by name, and stores an overidable location (to handle file system of SSIS store)
        /// </summary>
        /// <param name="integrationServicePack">the file name and location on the file system</param>
        /// <param name="locationName">the logical location that the package came from.  Either the SSIS Server, or file system</param>
        private void EnumerateIntegrationServicePack(string integrationServicePack, string locationName)
        {
            try
            {
                // Load the project content.
                using (Project ssisProject = Project.OpenProject(integrationServicePack))
                {
                    // Force the loading of the connections, so they are in memory when the packages are accessed later.
                    // This seems to be needed for SQL 2012, but not later versions.
                    foreach (ConnectionManagerItem cmi in ssisProject.ConnectionManagerItems)
                    {
                        Console.WriteLine(string.Format("Discovered connection manager {0}", cmi.ConnectionManager.Name));
                    }
                    // Parse each and every package in the project.
                    foreach (PackageItem pi in ssisProject.PackageItems)
                    {
                        try
                        {
                            Console.Write(string.Format("Processing Project package '{0}'... ", pi.Package.Name));
                            EnumeratePackage(pi.Package, locationName + @"\" + pi.Package.Name);
                            Console.WriteLine("Completed Successfully.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("Error {0} occurred whilst attempting to handle ispac {1}\r\nWith stack trace {2}.", ex.Message, integrationServicePack, ex.StackTrace));
                            repository.Rollback();
                            Console.WriteLine("This package has been rolled back.");
                        }
                        finally
                        {
                            // Commit each package as completed to reduce instance of data loss due to unexpected failures, and associated rollback.
                            repository.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error {0} occurred whilst attempting to handle ispac {1}\r\nWith stack trace {2}.", ex.Message, integrationServicePack, ex.StackTrace));
            }
        }

#endif

        /// <summary>
        /// Enumerates all packages that're in a directory and all sub directories if user asked us to.
        /// </summary>
        /// <param name="rootFolders"></param>
        /// <param name="recurseSubFolders"></param>
        public void EnumerateFileSystemPackages(string[] rootFolders, bool recurseSubFolders, bool storeThreePartNames, string[] storePackagePasswords)
        {
            threePartNames = storeThreePartNames;
            packagePasswords = storePackagePasswords;
            foreach (string rootFolder in rootFolders)
            {
                if (System.IO.Directory.Exists(rootFolder) == false)
                {
                    throw new Exception(string.Format("Root package folder '{0}' doesn't exist.", rootFolder));
                }

                EnumeratePackages(rootFolder, dtsxPattern, recurseSubFolders, null);
#if SQLGT2008
                EnumerateProjects(rootFolder, ispacPattern, recurseSubFolders, null);
#endif
            }
        }

#if SQLGT2008

        /// <summary>
        /// Enumerates all the projects (ispac) in the rootFolder.
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="pattern"></param>
        /// <param name="recurseSubFolders"></param>
        /// <param name="p"></param>
        private void EnumerateProjects(string rootFolder, string pattern, bool recurseSubFolders, string locationName)
        {
            string[] filesToInspect;
            Console.Write("Enumerating projects...");
            filesToInspect = System.IO.Directory.GetFiles(rootFolder, pattern, (recurseSubFolders) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
            Console.WriteLine("done.");

            foreach (string projectFileName in filesToInspect)
            {
                if (String.IsNullOrEmpty(locationName))
                    EnumerateIntegrationServicePack(projectFileName, projectFileName);
                else
                    EnumerateIntegrationServicePack(projectFileName, locationName);
                // Commit each project as completed to reduce instance of data loss due to unexpected failures.
                repository.Commit();
            }
        }
#endif

        /// <summary>
        /// Enumerates all the packages in the rootFolder.  Will attempt to determine if there is a folder of Project Deployment
        /// pacakges, and handle these appropriately.
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="pattern"></param>
        /// <param name="recurseSubFolders"></param>
        /// <param name="locationName"></param>
        private void EnumeratePackages(string rootFolder, string pattern, bool recurseSubFolders, string locationName)
        {
            string[] filesToInspect;
            string sqlVersion = string.Empty;
#if SQLGT2008
            string executionLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileInfo currentExecutable = new FileInfo(executionLocation);
#endif
#if SQL2012
            sqlVersion = "SQL2012";
#endif
#if SQL2014
            sqlVersion = "SQL2014";
#endif
#if SQL2016
            sqlVersion = "SQL2016";
#endif
#if SQL2017
            sqlVersion = "SQL2017";
#endif

            Console.Write("Enumerating parameters and connection managers...");
            string[] configsToAttach = System.IO.Directory.GetFiles(rootFolder, "*.params", (recurseSubFolders) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
            string[] connectionsToAttach = System.IO.Directory.GetFiles(rootFolder, "*.conmgr", (recurseSubFolders) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
            Console.WriteLine("done.");

            if ((configsToAttach.Length > 0) || (connectionsToAttach.Length > 0))
            {
                // Assume that we are in Project Deployment Mode.
                // But this has to check each folder, as there may be a mix of project and package deployment based on folder locations :-(
                Console.WriteLine("Parameters or connection managers detected.  Switch to hybrid project mode...");
                Console.Write("Enumerate directories...");
                List<string> directoriesToEnumerate = new List<string>();
                directoriesToEnumerate.Add(rootFolder);
                if (recurseSubFolders)
                {
                    string[] foldersToEnumerate = System.IO.Directory.GetDirectories(rootFolder, "*.*", (recurseSubFolders) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
                    foreach (string folderPath in foldersToEnumerate)
                        directoriesToEnumerate.Add(folderPath);
                }
                Console.WriteLine("done.");

                foreach (string folderPath in directoriesToEnumerate)
                {
                    Console.Write(string.Format("Enumerate directory {0} for .params and .conmgr...", folderPath));
                    configsToAttach = System.IO.Directory.GetFiles(folderPath, "*.params", System.IO.SearchOption.TopDirectoryOnly);
                    connectionsToAttach = System.IO.Directory.GetFiles(folderPath, "*.conmgr", System.IO.SearchOption.TopDirectoryOnly);
                    Console.WriteLine("done.");
                    if ((configsToAttach.Length > 0) || (connectionsToAttach.Length > 0))
                    {
#if SQLGT2008
                        string tempFolder = System.IO.Path.GetTempPath() + @"\SSISMD" + Guid.NewGuid().ToString();
                        if (!System.IO.Directory.Exists(tempFolder))
                        {
                            System.IO.Directory.CreateDirectory(tempFolder);
                        }
                        System.IO.DirectoryInfo tempDirectory = new System.IO.DirectoryInfo(tempFolder);
                        try
                        {
                            Console.Write(string.Format("Enumerate directory {0} for .dtproj...", folderPath));
                            filesToInspect = System.IO.Directory.GetFiles(rootFolder, "*.dtproj", System.IO.SearchOption.TopDirectoryOnly);
                            Console.WriteLine("done.");
                            foreach (string projectFileName in filesToInspect)
                            {
                                // We need to shell out to the SSISProjectBuilder as it needs different DLL's to the analyser.
                                Console.WriteLine(String.Format("Generating ispac file from dtproj file {0}", projectFileName));
                                FileInfo projectFileInfo = new FileInfo(projectFileName);
                                string ispacFileName = string.Format(@"{0}\{1}.ispac", tempDirectory.FullName, projectFileInfo.Name);
                                // /d:"D:\keith\Documents\visual studio 2015\Projects\Integration Services 2016 ProjectMode\Integration Services 2016 ProjectMode\Integration Services 2016 ProjectMode.dtproj" /i:"D:\Test\SSISProjectBuilder.ispac" /v:SQL2016
                                Process ssisProjectBuilder = new Process();

                                ssisProjectBuilder.StartInfo.FileName = string.Format(@"{0}\..\SSISProjectBuilder\SSISProjectBuilder.exe", currentExecutable.DirectoryName);
                                ssisProjectBuilder.StartInfo.UseShellExecute = false;
                                ssisProjectBuilder.StartInfo.RedirectStandardOutput = true;
                                ssisProjectBuilder.StartInfo.RedirectStandardInput = true;
                                ssisProjectBuilder.StartInfo.Arguments = string.Format("/d:\"{0}\" /i:\"{1}\" /v:{2}", projectFileName, ispacFileName, sqlVersion);
                                ssisProjectBuilder.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                                {
                                    if (!String.IsNullOrEmpty(e.Data))
                                    {
                                        outputFromSSISBuilder.Append(String.Format("\n{0}", e.Data));
                                    }
                                });

                                ssisProjectBuilder.Start();
                                ssisProjectBuilder.BeginOutputReadLine();
                                if (ssisProjectBuilder.WaitForExit(300000))  // Wait for 5 minutes for the process to exit.  If a package build takes longer than this there must be a problem.
                                {
                                    Console.WriteLine(outputFromSSISBuilder);

                                    if (ssisProjectBuilder.ExitCode == 0)
                                    {
                                        ssisProjectBuilder.Close();
                                        EnumerateIntegrationServicePack(ispacFileName, projectFileName);
                                    }
                                    else
                                    {
                                        Console.WriteLine(String.Format("Error: SSISProjectBuilder.exe exited with code {0}.\r\nProject file is not being processed.", ssisProjectBuilder.ExitCode));
                                        ssisProjectBuilder.Close();
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Warning! SSISProjectBuilder has not exited in 5 minutes.");
                                    Console.WriteLine(outputFromSSISBuilder);
                                    Console.WriteLine("Warning! SSISProjectBuilder is now being killed off.");
                                    if (!ssisProjectBuilder.HasExited)
                                    {
                                        ssisProjectBuilder.Kill();
                                        if (ssisProjectBuilder.WaitForExit(3000))
                                        {
                                            Console.WriteLine("SSISProjectBuilder has been killed.\r\nThis project file will not be processed.");
                                            ssisProjectBuilder.Close();
                                        }
                                        else
                                        {
                                            throw new Exception("SSISProjectBuilder unable to be killed");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("SSISProjectBuilder has exited.\r\nThis project file will not be processed.");
                                        ssisProjectBuilder.Close();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("The exception \r\n{0}\r\nwas raised with stack trace{1}", ex.Message, ex.StackTrace));
                        }
                        finally
                        {
                            // Cleanup temporary files etc.
                            tempDirectory.Delete(true);
                        }
#else
                        Console.WriteLine(string.Format("Directory {0} has connection manager or configuration, which is not compatible with this build of DependencyAnalyzer...", folderPath));
#endif
                    }
                    else
                    {
                        Console.Write(string.Format("Enumerate directory {0} for .dtsx...", folderPath));
                        filesToInspect = System.IO.Directory.GetFiles(rootFolder, pattern, System.IO.SearchOption.TopDirectoryOnly);
                        Console.WriteLine("done.");

                        foreach (string packageFileName in filesToInspect)
                        {
                            EnumerateFilePackage(packageFileName, locationName);
                        }
                    }
                    // Commit each folder as completed to reduce instance of data loss due to unexpected failures.
                    repository.Commit();
                }
                // Remove the temporary directory and it's contents.
                //tempDirectory.Delete(true);
            }
            else
            { 

                Console.Write("Enumerating packages...");
                filesToInspect = System.IO.Directory.GetFiles(rootFolder, pattern, (recurseSubFolders) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
                Console.WriteLine("done.");

                foreach (string packageFileName in filesToInspect)
                {
                    EnumerateFilePackage(packageFileName, locationName);
                }
                // Commit each folder as completed to reduce instance of data loss due to unexpected failures.
                repository.Commit();
            }
        }

        /// <summary>
        /// Enumerates the specific package, whilst checking for encryption
        /// </summary>
        /// <param name="packageFileName"></param>
        /// <param name="locationName"></param>
        private void EnumerateFilePackage(string packageFileName, string locationName)
        {
            try
            {
                Console.Write(string.Format("Loading file package '{0}'... ", packageFileName));

                // load the package
                using (Package package = app.LoadPackage(packageFileName, null))
                {
                    if (String.IsNullOrEmpty(locationName))
                        EnumeratePackage(package, packageFileName);
                    else
                        EnumeratePackage(package, locationName + @"\" + package.Name);
                }

                Console.WriteLine("Completed Successfully.");
            }
            catch (Microsoft.SqlServer.Dts.Runtime.DtsRuntimeException dtsEx)
            {
                if (dtsEx.Message.Contains("The package is encrypted with a password"))
                {
                    try
                    {
                        // The package was encrypted.  Try to decrypt the sucker!
                        using (Package package = LoadPackage(packageFileName))
                        {
                            if (package != null)
                            {
                                if (String.IsNullOrEmpty(locationName))
                                    EnumeratePackage(package, packageFileName);
                                else
                                    EnumeratePackage(package, locationName + @"\" + package.Name);
                            }
                            else
                                Console.WriteLine(string.Format("Unable to decrypt package {0} with passwords provided.", packageFileName));
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(string.Format("Error {0} occurred whilst attempting to load package {1}\r\nWith stack trace {2}.", ex.Message, packageFileName, ex.StackTrace));
                        repository.Rollback();
                        Console.WriteLine("This package has been rolled back.");
                    }
                    finally
                    {
                        // Commit each package as completed to reduce instance of data loss due to unexpected failures, and associated rollback.
                        repository.Commit();
                    }
                }
                else
                {
                    Console.WriteLine(string.Format("Error {0} occurred whilst attempting to load package {1}\r\nWith stack trace {2}.", dtsEx.Message, packageFileName, dtsEx.StackTrace));
                    repository.Rollback();
                    Console.WriteLine("This package has been rolled back.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error {0} occurred whilst attempting to load package {1}\r\nWith stack trace {2}.", ex.Message, packageFileName, ex.StackTrace));
                repository.Rollback();
                Console.WriteLine("This package has been rolled back.");
            }
            finally
            {
                // Commit each package as completed to reduce instance of data loss due to unexpected failures, and associated rollback.
                repository.Commit();
            }
        }


        /// <summary>
        /// Attempt to load an ssis package utilising provided passwords.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="SSISServer"></param>
        /// <param name="SSISUser"></param>
        /// <param name="SSISPwd"></param>
        /// <param name="Events"></param>
        /// <returns></returns>
        private Package LoadPackage(String location, String SSISServer, String SSISUser, String SSISPwd, IDTSEvents Events)
        {
            Package package = null;
            int attempts = 1;
            foreach (String password in packagePasswords)
            {
                try
                {
                    app.PackagePassword = password;
                    package = app.LoadFromSqlServer(location, SSISServer, SSISUser, SSISPwd, Events);
                    Console.WriteLine(string.Format("Password attempt {0} succeeded for package {1}", attempts++, location));
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Password attempt {0} failed for package {1} with message {2}", attempts++, location, ex.Message));
                    package = null;
                }
            }
            return package;
        }

        /// <summary>
        /// Attempt to load an ssis package utilising provided passwords.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="SSISServer"></param>
        /// <param name="SSISUser"></param>
        /// <param name="SSISPwd"></param>
        /// <param name="Events"></param>
        /// <returns></returns>
        private Package LoadPackage(String location, String SSISServer, IDTSEvents Events)
        {
            Package package = null;
            int attempts = 1;
            foreach (String password in packagePasswords)
            {
                try
                {
                    app.PackagePassword = password;
                    package = app.LoadFromDtsServer(location, SSISServer, Events);
                    Console.WriteLine(string.Format("Password attempt {0} succeeded for package {1}", attempts++, location));
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Password attempt {0} failed for package {1} with message {2}", attempts++, location, ex.Message));
                    package = null;
                }
            }
            return package;
        }


        /// <summary>
        ///  Attempt to load an ssis package utilising provided passwords.
        /// </summary>
        /// <param name="packageFileName"></param>
        /// <returns></returns>
        private Package LoadPackage(string packageFileName)
        {
            Package package = null;
            int attempts = 1;
            foreach (String password in packagePasswords)
            {
                try
                {
                    app.PackagePassword = password;
                    package = app.LoadPackage(packageFileName, null);
                    Console.WriteLine(string.Format("Password attempt {0} succeeded for package {1}", attempts++, packageFileName));
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Password attempt {0} failed for package {1} with message {2}", attempts++, packageFileName, ex.Message));
                    package = null;
                }
            }
            return package;
        }

        private void EnumerateInfos(Application app)
        {
            Console.Write("Enumerating registered SSIS Data Flow components...");
            pipelineComponentInfos = app.PipelineComponentInfos;
            foreach (PipelineComponentInfo pipelineComponentInfo in pipelineComponentInfos)
            {
                repository.AddObjectType(Repository.Domains.SSIS, pipelineComponentInfo.ID, pipelineComponentInfo.Name, pipelineComponentInfo.Description);
            }
            Console.WriteLine("Done");

            Console.Write("Enumerating registered SSIS Connection Managers...");

            connectionInfos = app.ConnectionInfos;
            connectionTypeToIDMap = new Dictionary<string, string>(connectionInfos.Count);
            foreach (ConnectionInfo connectionInfo in connectionInfos)
            {
                connectionTypeToIDMap.Add(connectionInfo.ConnectionType, connectionInfo.ID);
                repository.AddObjectType(Repository.Domains.SSIS, connectionInfo.ID, connectionInfo.Name, connectionInfo.Description);
            }

            Console.WriteLine("Done");

            Console.Write("Enumerating registered SSIS Tasks...");

            taskInfos = app.TaskInfos;
            foreach (TaskInfo taskInfo in taskInfos)
            {
                repository.AddObjectType(Repository.Domains.SSIS, taskInfo.ID, taskInfo.Name, taskInfo.Description);
            }

            Console.WriteLine("Done");
        }

        /// <summary>
        /// recurses through all dtsContainers looking for any pipelines...
        /// Now handles ForEachLoop, ForLoop and Sequence Containers
        /// Disabled Tasks are not tested...
        /// </summary>
        /// <param name="package">The SSIS Package that is being investigated</param>
        /// <param name="location">The text string of the location of this package</param>
        /// <param name="packageRepositoryID">The internal ID for this package</param>
        /// <param name="dtsContainer">The Container that is to be assessed</param>
        private void EnumerateTask(Package package, string location, int packageRepositoryID, DtsContainer dtsContainer)
        {
            if (!dtsContainer.Disable)
            {
                if (dtsContainer is TaskHost)
                {
                    TaskHost taskHost = dtsContainer as TaskHost;

                    IDTSPipeline pipeline = taskHost.InnerObject as IDTSPipeline;

                    if (pipeline != null)
                    {
                        // this is a data flow task
                        packageRepositoryID = InspectDataFlow(pipeline, packageRepositoryID, package, taskHost, location);
                    }
                    else if (dtsContainer.CreationName.Contains("Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask.ExecuteSQLTask"))
                    {
                        EnumerateSqlTask(package, location, packageRepositoryID, taskHost);
                    }
                }
                else if (dtsContainer is ForEachLoop)
                {
                    ForEachLoop felContainer = dtsContainer as ForEachLoop;
                    foreach (DtsContainer innerDtsContainer in felContainer.Executables)
                    {
                        EnumerateTask(package, location, packageRepositoryID, innerDtsContainer);
                    }
                }
                else if (dtsContainer is ForLoop)
                {
                    ForLoop flContainer = dtsContainer as ForLoop;
                    foreach (DtsContainer innerDtsContainer in flContainer.Executables)
                    {
                        EnumerateTask(package, location, packageRepositoryID, innerDtsContainer);
                    }
                }
                else if (dtsContainer is Sequence)
                {
                    Sequence sContainer = dtsContainer as Sequence;
                    foreach (DtsContainer innerDtsContainer in sContainer.Executables)
                    {
                        EnumerateTask(package, location, packageRepositoryID, innerDtsContainer);
                    }
                }
            }
        }

        /// <summary>
        /// Handles parsing an SQLTask object, and finding what is used within it.
        /// </summary>
        /// <param name="package">The SSIS Package that is being investigated</param>
        /// <param name="location">The text string of the location of this package</param>
        /// <param name="packageRepositoryID">The internal ID for this package</param>
        /// <param name="taskHost">The SQL Task that is to be investigated.</param>
        private void EnumerateSqlTask(Package package, string location, int packageRepositoryID, TaskHost taskHost)
        {
            int tableID, procID, funcID;
            if (packageRepositoryID == -1)
            {
                // add this package to the repository
                packageRepositoryID = AddObject(package.Name, package.Description, objectTypePackage, repository.RootRepositoryObjectID);
                repository.AddAttribute(packageRepositoryID, attributePackageLocation, location);
                repository.AddAttribute(packageRepositoryID, attributePackageID, package.ID);
            }
            // Add this task to the repository
            int sqlTaskRepositoryObjectID = AddObject(taskHost.Name, taskHost.Description, taskHost.CreationName, packageRepositoryID);
            
            if (taskHost.Properties.Contains("SqlStatementSource") & taskHost.Properties.Contains("SqlStatementSourceType") & taskHost.Properties.Contains("Connection"))
            {
                string queryDefinition = string.Empty;

                SqlStatementSourceType stmtSource = (SqlStatementSourceType)taskHost.Properties["SqlStatementSourceType"].GetValue(taskHost);
                switch (stmtSource)
                {
                    case SqlStatementSourceType.DirectInput:
                        queryDefinition = taskHost.Properties["SqlStatementSource"].GetValue(taskHost).ToString();
                        repository.AddAttribute(sqlTaskRepositoryObjectID, Repository.Attributes.QueryDefinition, queryDefinition);
                        break;
                    case SqlStatementSourceType.FileConnection:
                        break;
                    case SqlStatementSourceType.Variable:
                        queryDefinition = GetVariable(package, taskHost, taskHost.Properties["SqlStatementSource"].GetValue(taskHost).ToString());
                        repository.AddAttribute(sqlTaskRepositoryObjectID, Repository.Attributes.QueryDefinition, queryDefinition);
                        break;
                    default:
                        throw new Exception(string.Format("Invalid Sql Statement Source Type {0}.", stmtSource));
                }
                Microsoft.SqlServer.Dts.Runtime.ConnectionManager connectionManager = package.Connections[taskHost.Properties["Connection"].GetValue(taskHost).ToString()];

                    string connectionManagerType = connectionManager.CreationName;
                    int connectionID = repository.GetConnection(connectionManager.ConnectionString);

                    string serverName = null;
                    if (connectionID == -1)
                    {
                        connectionID = CreateConnection(connectionManager, out serverName);
                    }

                    if (!string.IsNullOrEmpty(queryDefinition))
                    {
                        try
                        {
                            SqlStatement toBeParsed = new SqlStatement();

                            //toBeParsed.sqlString = statement;
                            toBeParsed.quotedIdentifiers = true;
                            if (toBeParsed.ParseString(queryDefinition))
                            {
                                foreach (string tableName in toBeParsed.getTableNames(true))
                                {
                                    if (threePartNames)
                                    {
                                        String dbName = repository.RetrieveDatabaseNameFromConnectionID(connectionID);
                                        tableID = repository.GetTable(connectionID, String.Format("[{0}].{1}", dbName, tableName));
                                    }
                                    else
                                        tableID = repository.GetTable(connectionID, tableName);
                                    repository.AddMapping(tableID, sqlTaskRepositoryObjectID);
                                }
                                foreach (string procedureName in toBeParsed.getProcedureNames(true))
                                {
                                    if (threePartNames)
                                    {
                                        String dbName = repository.RetrieveDatabaseNameFromConnectionID(connectionID);
                                        procID = repository.GetProcedure(connectionID, String.Format("[{0}].{1}", dbName, procedureName));
                                    }
                                    else
                                        procID = repository.GetProcedure(connectionID, procedureName);
                                    repository.AddMapping(procID, sqlTaskRepositoryObjectID);
                                }
                                foreach (string funcName in toBeParsed.getFunctionNames(true))
                                {
                                    if (threePartNames)
                                    {
                                        String dbName = repository.RetrieveDatabaseNameFromConnectionID(connectionID);
                                        funcID = repository.GetFunction(connectionID, String.Format("[{0}].{1}", dbName, funcName));
                                    }
                                    else
                                        funcID = repository.GetFunction(connectionID, funcName);
                                    repository.AddMapping(funcID, sqlTaskRepositoryObjectID);
                                }
                            }
                            else
                            {
                                string errorMessage = "The following messages where generated whilst parsing the sql statement\r\n" + queryDefinition + "\r\n";
                                foreach (string error in toBeParsed.parseErrors)
                                {
                                    errorMessage += error + "\r\n";
                                }
                                Console.WriteLine(errorMessage);
                            }
                        }
                        catch (System.Exception err)
                        {
                            Console.WriteLine("The exception \r\n{0}\r\nwas raised against query \r\n{1}\r\nPlease report to https://github.com/keif888/SQLServerMetadata/issues\r\n", err.Message, queryDefinition);
                        }
                    }
            }
            //ExecuteSQLTask sqlTask = taskHost.InnerObject as ExecuteSQLTask;
            //string queryDefinition = sqlTask.SqlStatementSource;
            //// add the query definition attribute if we have one
            //if (queryDefinition != null)
            //{
            //    repository.AddAttribute(sqlTaskRepositoryObjectID, Repository.Attributes.QueryDefinition, queryDefinition);
            //}
        }

        /// <summary>
        /// handles loading information about a package to the repository
        /// </summary>
        /// <param name="package"></param>
        private void EnumeratePackage(Package package, string location)
        {
            // add this package to the repository
            int packageRepositoryID = AddObject(package.Name, package.Description, objectTypePackage, repository.RootRepositoryObjectID);
            repository.AddAttribute(packageRepositoryID, attributePackageLocation, location);
            repository.AddAttribute(packageRepositoryID, attributePackageID, package.ID);

            // loop through all data flow tasks
            foreach (DtsContainer dtsContainer in package.Executables)
            {
                EnumerateTask(package, location, packageRepositoryID, dtsContainer);
            }

            //foreach (Executable executable in package.Executables)
            //{
            //    if (executable is TaskHost)
            //    {
            //        if ((executable as TaskHost).InnerObject is ExecuteSQLTask)
            //        {
            //            EnumerateSqlTask(package, location, packageRepositoryID, (executable as TaskHost));
            //        }
            //    }
            //}
        }

        /// <summary>
        /// adds interesting components to the repository. If at least one interesting object exists in the data flow task 
        /// the data flow task is also added to the repository. If at least one such interesting data flow task exists in the
        /// package and the package hasn't been created in the repository as yet, it will be created as well. What's returned
        /// is the package repository ID.
        /// </summary>
        /// <param name="packageRepositoryID"></param>
        /// <param name="package"></param>
        /// <param name="taskHost"></param>
        /// <returns></returns>
        private int InspectDataFlow(IDTSPipeline pipeline, int packageRepositoryID, Package package, TaskHost taskHost, string packageLocation)
        {
            int dataFlowRepositoryObjectID = -1;

            Dictionary<int, int> componentIDToSourceRepositoryObjectMap = new Dictionary<int, int>();
            Dictionary<int, int> componentIDToRepositoryObjectMap = new Dictionary<int, int>();

            // go through all components looking for sources or destinations
            foreach (IDTSComponentMetaData component in pipeline.ComponentMetaDataCollection)
            {
                EnumerateDataFlowComponent(ref packageRepositoryID, package, taskHost, packageLocation, ref dataFlowRepositoryObjectID, ref componentIDToSourceRepositoryObjectMap, ref componentIDToRepositoryObjectMap, component);
            }

            // after all the objects have been added, traverse their mappings
            if (componentIDToSourceRepositoryObjectMap.Count > 0)
            {
                EnumerateMappings(pipeline, dataFlowRepositoryObjectID, componentIDToSourceRepositoryObjectMap, componentIDToRepositoryObjectMap);
            }

            return packageRepositoryID;
        }

        private void EnumerateDataFlowComponent(ref int packageRepositoryID, Package package, TaskHost taskHost, string packageLocation, ref int dataFlowRepositoryObjectID, ref Dictionary<int, int> componentIDToSourceRepositoryObjectMap, ref Dictionary<int, int> componentIDToRepositoryObjectMap, IDTSComponentMetaData component)
        {
            string objectType;
            DTSPipelineComponentType dataFlowComponentType;
            string domain, tableOrViewName, queryDefinition;
            bool tableOrViewSource;
            string kimballPropertyValue;
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            XmlReader objXMLText;

            // if we have a source, note it as the starting point and add it to the repository
            if (IsComponentInteresting(package, taskHost, component, out objectType, out dataFlowComponentType, out domain, out tableOrViewName, out tableOrViewSource, out queryDefinition))
            {
                // if the data flow object itself hasn't been created as  yet, do so now
                if (dataFlowRepositoryObjectID == -1)
                {
                    // if the package itself doesn't exist either, create it now
                    if (packageRepositoryID == -1)
                    {
                        // add this package to the repository
                        packageRepositoryID = AddObject(package.Name, package.Description, objectTypePackage, repository.RootRepositoryObjectID);
                        repository.AddAttribute(packageRepositoryID, attributePackageLocation, packageLocation);
                        repository.AddAttribute(packageRepositoryID, attributePackageID, package.ID);
                    }

                    dataFlowRepositoryObjectID = AddObject(taskHost.Name, taskHost.Description, taskHost.CreationName, packageRepositoryID);
                }

                // add the component to the repository
                int componentRepositoryID = AddObject(component.Name, component.Description, objectType, dataFlowRepositoryObjectID);

                Debug.Assert(componentRepositoryID > 0);

                if (dataFlowComponentType == DTSPipelineComponentType.SourceAdapter)
                {
                    componentIDToSourceRepositoryObjectMap.Add(component.ID, componentRepositoryID);
                }

                componentIDToRepositoryObjectMap.Add(component.ID, componentRepositoryID);

                // add the query definition attribute if we have one
                if (queryDefinition != null)
                {
                    repository.AddAttribute(componentRepositoryID, Repository.Attributes.QueryDefinition, queryDefinition);


                }
                if (component.ComponentClassID == ClassIDs.OLEDBCommand)
                {
                    GetStringComponentPropertyValue(component, "SqlCommand");
                }
                else if (component.ComponentClassID == ClassIDs.DerivedColumn)
                {
                    foreach (IDTSInput localInput in component.InputCollection)
                    {
                        if (localInput.IsAttached)
                        {
                            foreach (IDTSInputColumn localIColumn in localInput.InputColumnCollection)
                            {
                                if (localIColumn.CustomPropertyCollection.Count == 2)
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale) + " Expression " +
                                        localIColumn.CustomPropertyCollection["FriendlyExpression"].Value != null ? localIColumn.CustomPropertyCollection["FriendlyExpression"].Value.ToString() : "Not Available"
                                        );
                                }
                                else
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale) + " Expression (See Ouput Column)");
                                }
                                //repository.AddObject(localIColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                            }
                        }
                    }
                    foreach (IDTSOutput localOutput in component.OutputCollection)
                    {
                        if (localOutput.IsAttached)
                        {
                            foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                            {
                                if (localOColumn.CustomPropertyCollection.Count == 2)
                                {
                                    repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale) + " Expression " +
                                        localOColumn.CustomPropertyCollection["FriendlyExpression"].Value != null ? localOColumn.CustomPropertyCollection["FriendlyExpression"].Value.ToString() : "Not Available"
                                        );
                                }
                                //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                                //ToDo: Add connection from Input to Output Column.
                            }
                        }
                    }
                }
                else if (component.ComponentClassID == ClassIDs.Lookup)
                {
                    foreach (IDTSInput localInput in component.InputCollection)
                    {
                        if (localInput.IsAttached)
                        {
                            foreach (IDTSInputColumn localIColumn in localInput.InputColumnCollection)
                            {
                                if (localIColumn.CustomPropertyCollection.Count == 2)
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale) + " Reference Column [" + 
                                        localIColumn.CustomPropertyCollection["JoinToReferenceColumn"].Value != null ? localIColumn.CustomPropertyCollection["JoinToReferenceColumn"].Value.ToString() : "Not Available"   // Address Issue #13.  Although why a lookup component wouldn't have a column to reference I don't know.
                                        + "]");
                                }
                                else
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale) + " Expression (See Ouput Column)");
                                }
                                //repository.AddObject(localIColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                            }
                        }
                    }
                    foreach (IDTSOutput localOutput in component.OutputCollection)
                    {
                        if (localOutput.IsAttached)
                        {
                            foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                            {
                                if (localOColumn.CustomPropertyCollection.Count == 1)
                                {
                                    repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale) + " Return Column [" + 
                                        localOColumn.CustomPropertyCollection["CopyFromReferenceColumn"].Value != null ? localOColumn.CustomPropertyCollection["CopyFromReferenceColumn"].Value.ToString() : "Not Available"  // Make sure that Issue #13 can't happen here!
                                        + "]");
                                }
                                //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                            }
                        }
                    }
                }
                else if (component.ComponentClassID == ClassIDs.FuzzyLookup)
                {
                    foreach (IDTSInput localInput in component.InputCollection)
                    {
                        if (localInput.IsAttached)
                        {
                            foreach (IDTSInputColumn localIColumn in localInput.InputColumnCollection)
                            {
                                if (localIColumn.CustomPropertyCollection.Count == 5)
                                {
                                    if (localIColumn.CustomPropertyCollection["JoinToReferenceColumn"].Value != null)
                                    {
                                        repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale) + " Reference Column [" +
                                            localIColumn.CustomPropertyCollection["JoinToReferenceColumn"].Value != null ? localIColumn.CustomPropertyCollection["JoinToReferenceColumn"].Value.ToString() : "Not Available"
                                            + "]");
                                    }
                                    else
                                    {
                                        repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale));
                                    }
                                }
                                else
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale) + " Expression (See Ouput Column)");
                                }
                                //repository.AddObject(localIColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                            }
                        }
                    }
                    foreach (IDTSOutput localOutput in component.OutputCollection)
                    {
                        if (localOutput.IsAttached)
                        {
                            foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                            {
                                if (localOColumn.CustomPropertyCollection.Count == 3)
                                {
                                    if (localOColumn.CustomPropertyCollection["CopyFromReferenceColumn"].Value != null)
                                    {
                                        repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale) + " Return Column [" +
                                            localOColumn.CustomPropertyCollection["CopyFromReferenceColumn"].Value != null ? localOColumn.CustomPropertyCollection["CopyFromReferenceColumn"].Value.ToString() : "Not Available"
                                            + "]");
                                    }
                                    else
                                    {
                                        repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale));
                                    }
                                }
                                else
                                {
                                    repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale));
                                }
                                //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                            }
                        }
                    }
                }
                else if (component.ComponentClassID == ClassIDs.ManagedComponentWrapper)
                {
                    if (component.CustomPropertyCollection["UserComponentTypeName"].Value.ToString() == ClassIDs.MultipleHash)
                    {
                        foreach (IDTSInput localInput in component.InputCollection)
                        {
                            if (localInput.IsAttached)
                            {
                                foreach (IDTSInputColumn localIColumn in localInput.InputColumnCollection)
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale));
                                    //repository.AddObject(localIColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                                }
                            }
                        }
                        foreach (IDTSOutput localOutput in component.OutputCollection)
                        {
                            if (localOutput.IsAttached)
                            {
                                foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                                {
                                    if (localOColumn.CustomPropertyCollection.Count == 2)
                                    {
                                        string localResults = "";
                                        if (localOColumn.CustomPropertyCollection["InputColumnLineageIDs"].Value.ToString().Length > 0)  // If this fails then the use of the component is not valid, and the package is corrupt.
                                        {
                                            foreach (string localIDs in localOColumn.CustomPropertyCollection["InputColumnLineageIDs"].Value.ToString().Split(','))
                                            {
                                                string withoutHash = localIDs;
                                                if (localIDs[0] == '#')
                                                {
                                                    withoutHash = localIDs.Substring(1);
                                                }
                                                if (localResults.Length == 0)
                                                {
                                                    localResults = component.InputCollection[0].InputColumnCollection.GetInputColumnByLineageID(System.Convert.ToInt32(withoutHash)).Name;
                                                }
                                                else
                                                {
                                                    localResults += ", " + component.InputCollection[0].InputColumnCollection.GetInputColumnByLineageID(System.Convert.ToInt32(withoutHash)).Name;
                                                }
                                            }
                                        }
                                        repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale) + " Generated From " + localResults);
                                    }
                                    //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                                }
                            }
                        }
                    }
                    else if (component.CustomPropertyCollection["UserComponentTypeName"].Value.ToString() == ClassIDs.KimballSCD)
                    {
                        foreach (IDTSInput localInput in component.InputCollection)
                        {
                            if (localInput.IsAttached)
                            {
                                foreach (IDTSInputColumn localIColumn in localInput.InputColumnCollection)
                                {
                                    repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale));
                                    //repository.AddObject(localIColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                                }
                            }
                        }

                        foreach (IDTSOutput localOutput in component.OutputCollection)
                        {
                            if (localOutput.IsAttached)
                            {
                                kimballPropertyValue = string.Empty;
                                switch (localOutput.Name)
                                {
                                    case "New":
                                        kimballPropertyValue = component.CustomPropertyCollection["New Output Columns"].Value.ToString();
                                        break;
                                    case "Unchanged":
                                        kimballPropertyValue = component.CustomPropertyCollection["Unchanged Output Columns"].Value.ToString();
                                        break;
                                    case "Deleted":
                                        kimballPropertyValue = component.CustomPropertyCollection["Deleted Output Columns"].Value.ToString();
                                        break;
                                    case "Updated SCD1":
                                        kimballPropertyValue = component.CustomPropertyCollection["Update SCD1 Columns"].Value.ToString();
                                        break;
                                    case "Auditing":
                                        kimballPropertyValue = component.CustomPropertyCollection["Audit Columns"].Value.ToString();
                                        break;
                                    case "Expired SCD2":
                                        kimballPropertyValue = component.CustomPropertyCollection["Expired SCD2 Columns"].Value.ToString();
                                        break;
                                    case "Invalid Input":
                                        break;
                                    case "New SCD2":
                                        kimballPropertyValue = component.CustomPropertyCollection["New SCD2 Columns"].Value.ToString();
                                        break;
                                    default:
                                        break;
                                }
                                string localResults = "Unknown.";
                                if (kimballPropertyValue != string.Empty)
                                {
                                    objXMLText = XmlReader.Create(new System.IO.StringReader(kimballPropertyValue), settings);
                                    objXMLText.Read();
                                    foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                                    {
                                        string strLineageID;
                                        string strMappedToExistingDimensionLineageID;

                                        objXMLText.ReadToFollowing("OutputColumnDefinition");
                                        do
                                        {
                                            localResults = "Unknown.";
                                            strLineageID = objXMLText.GetAttribute("LineageID");
                                            strMappedToExistingDimensionLineageID = objXMLText.GetAttribute("MappedToExistingDimensionLineageID");
                                            if (localOColumn.LineageID == System.Convert.ToInt32(strLineageID))
                                            {
                                                foreach (IDTSInput localInput in component.InputCollection)
                                                {
                                                    IDTSInputColumn temp;
                                                    try
                                                    {
                                                        temp = localInput.InputColumnCollection.GetInputColumnByLineageID(System.Convert.ToInt32(strMappedToExistingDimensionLineageID));
                                                        localResults = "[" + localInput.Name + "] Column [" + temp.Name + "].";
                                                        break;
                                                    }
                                                    catch (Exception)
                                                    {
                                                    }
                                                }
                                                break;
                                            }
                                        }
                                        while (objXMLText.ReadToFollowing("OutputColumnDefinition"));

                                        repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale) + " Generated From " + localResults);
                                        //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                                    }
                                }
                                else
                                {
                                    foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                                    {
                                        repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale) + " Generated From " + localResults);
                                        //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        AddColumnNameAttributes(component, componentRepositoryID);
                    }
                }
                else
                {
                    // Add generic column attribute information here.
                    AddColumnNameAttributes(component, componentRepositoryID);
                }
                EnumerateComponentConnections(package, component, objectType, dataFlowComponentType, tableOrViewName, tableOrViewSource, queryDefinition, componentRepositoryID);
            }
        }

        private string FormatColumnDescription(string lName, Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType lType, int lLength, int lPrecision, int lScale)
        {
            string lTemp = "Column [" + lName + "] of Type [" + lType.ToString() + "]";
            switch (lType)
            {
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_BYREF_DECIMAL:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_BYREF_NUMERIC:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DECIMAL:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_NUMERIC:
                    lTemp += " Precision " + lPrecision.ToString() + " Scale " + lScale.ToString();
                    break;
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_BYTES:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_IMAGE:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_NTEXT:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_STR:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_TEXT:
                case Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR:
                    lTemp += " Length " + lLength;
                    break;
                default:
                    break;
            }
            return lTemp;
        }

        private void AddColumnNameAttributes(IDTSComponentMetaData component, int componentRepositoryID)
        {
            foreach (IDTSInput localInput in component.InputCollection)
            {
                if (localInput.IsAttached)
                {
                    foreach (IDTSInputColumn localIColumn in localInput.InputColumnCollection)
                    {
                        repository.AddAttribute(componentRepositoryID, localInput.Name + " [" + localIColumn.Name + "] [ID: " + localIColumn.ID.ToString() + "]", "From [" + localIColumn.UpstreamComponentName + "] " + FormatColumnDescription(localIColumn.Name, localIColumn.DataType, localIColumn.Length, localIColumn.Precision, localIColumn.Scale));
                        //repository.AddObject(localIColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                    }
                }
            }
            foreach (IDTSOutput localOutput in component.OutputCollection)
            {
                if (localOutput.IsAttached)
                {
                    foreach (IDTSOutputColumn localOColumn in localOutput.OutputColumnCollection)
                    {
                        repository.AddAttribute(componentRepositoryID, localOutput.Name + " [" + localOColumn.Name + "] [ID: " + localOColumn.ID.ToString() + "]", FormatColumnDescription(localOColumn.Name, localOColumn.DataType, localOColumn.Length, localOColumn.Precision, localOColumn.Scale));
                        //repository.AddObject(localOColumn.Name, "", ColumnEnumerator.ObjectTypes.Column, componentRepositoryID);
                    }
                }
            }
        }


        private void AddTableMappings(DTSPipelineComponentType dataFlowComponentType, bool tableOrViewSource, int componentRepositoryID, int tableRepositoryID)
        {
            if (tableRepositoryID != -1)
            {
                // add the mapping between the source table and the source component
                if ((dataFlowComponentType == DTSPipelineComponentType.SourceAdapter)
                    || ((dataFlowComponentType == DTSPipelineComponentType.Transform) && (tableOrViewSource = true)))
                {
                    repository.AddMapping(tableRepositoryID, componentRepositoryID);
                }
                else if ((dataFlowComponentType == DTSPipelineComponentType.DestinationAdapter)
                    || ((dataFlowComponentType == DTSPipelineComponentType.Transform) && (tableOrViewSource == false)))
                {
                    // add the mapping from destination transform to destination table
                    repository.AddMapping(componentRepositoryID, tableRepositoryID);
                }
            }
        }


        private void ParseTSqlStatement(string statement, int connectionID, DTSPipelineComponentType dataFlowComponentType, bool tableOrViewSource, int componentRepositoryID)
        {
            SqlStatement toBeParsed = new SqlStatement();
            int tableID, procID, funcID;
            //toBeParsed.sqlString = statement;
            toBeParsed.quotedIdentifiers = true;
            if (toBeParsed.ParseString(statement))
            {
                foreach (string tableName in toBeParsed.getTableNames(true))
                {
                    if (threePartNames)
                    {
                        String dbName = repository.RetrieveDatabaseNameFromConnectionID(connectionID);
                        tableID = repository.GetTable(connectionID, String.Format("[{0}].{1}", dbName, tableName));
                    }
                    else
                        tableID = repository.GetTable(connectionID, tableName);
                    AddTableMappings(dataFlowComponentType, tableOrViewSource, componentRepositoryID, tableID);
                }
                foreach (string procedureName in toBeParsed.getProcedureNames(true))
                {
                    if (threePartNames)
                    {
                        String dbName = repository.RetrieveDatabaseNameFromConnectionID(connectionID);
                        procID = repository.GetProcedure(connectionID, String.Format("[{0}].{1}", dbName, procedureName));
                    }
                    else
                        procID = repository.GetProcedure(connectionID, procedureName);
                    AddTableMappings(dataFlowComponentType, tableOrViewSource, componentRepositoryID, procID);
                }
                foreach (string funcName in toBeParsed.getFunctionNames(true))
                {
                    if (threePartNames)
                    {
                        String dbName = repository.RetrieveDatabaseNameFromConnectionID(connectionID);
                        funcID = repository.GetFunction(connectionID, String.Format("[{0}].{1}", dbName, funcName));
                    }
                    else
                        funcID = repository.GetFunction(connectionID, funcName);
                    AddTableMappings(dataFlowComponentType, tableOrViewSource, componentRepositoryID, funcID);
                }
            }
            else
            {
                string errorMessage = "The following messages where generated whilst parsing the sql statement\r\n" + statement + "\r\n";
                foreach (string error in toBeParsed.parseErrors)
                {
                    errorMessage += error + "\r\n";
                }
                Console.WriteLine(errorMessage);
            }
        }

        private void EnumerateComponentConnections(Package package, IDTSComponentMetaData component, string objectType,
            DTSPipelineComponentType dataFlowComponentType, string tableOrViewName, bool tableOrViewSource, string queryDefinition, int componentRepositoryID)
        {
            foreach (IDTSRuntimeConnection runtimeConnection in component.RuntimeConnectionCollection)
            {
                // todo: what happens if this connection isn't available anymore
                if (package.Connections.Contains(runtimeConnection.ConnectionManagerID))
                {
                    Microsoft.SqlServer.Dts.Runtime.ConnectionManager connectionManager = package.Connections[runtimeConnection.ConnectionManagerID];

                    string connectionManagerType = connectionManager.CreationName;
                    int connectionID = repository.GetConnection(connectionManager.ConnectionString);

                    string serverName = null;
                    if (connectionID == -1)
                    {
                        connectionID = CreateConnection(connectionManager, out serverName);
                    }

                    if (!string.IsNullOrEmpty(queryDefinition))
                    {
                        try
                        {
                            ParseTSqlStatement(queryDefinition, connectionID, dataFlowComponentType, tableOrViewSource, componentRepositoryID);
                        }
                        catch (System.Exception err)
                        {
                            Console.WriteLine("The exception \r\n{0}\r\nwas raised against query \r\n{1}\r\nPlease report to https://github.com/keif888/SQLServerMetadata/issues\r\n", err.Message, queryDefinition);
                        }

                        /*
                                                var statements = new List<IStatement> { };
                                                try
                                                {
                                                    statements = ParserFactory.Execute(queryDefinition);
                                                }
                                                catch (System.Exception err)
                                                {
                                                    Console.WriteLine("The exception \r\n{0}\r\nwas raised against query \r\n{1}\r\nPlease report to https://github.com/keif888/SQLServerMetadata/issues\r\n", err.Message, queryDefinition);
                                                }
                                                foreach (IStatement statement in statements)
                                                {
                                                    if (statement is Laan.Sql.Parser.Entities.SelectStatement)
                                                    {
                                                        ParseLaanSqlStatement(statement, connectionID, dataFlowComponentType, tableOrViewSource, componentRepositoryID, String.Empty);
                                                    }
                                                    if (statement is Laan.Sql.Parser.Entities.WithStatement)
                                                    {
                                                        ParseLaanSqlStatement((statement as Laan.Sql.Parser.Entities.WithStatement).CTERetrieve, connectionID, dataFlowComponentType, tableOrViewSource, componentRepositoryID, (statement as Laan.Sql.Parser.Entities.WithStatement).CTETable.Value);
                                                        ParseLaanSqlStatement((statement as Laan.Sql.Parser.Entities.WithStatement).CTESelect, connectionID, dataFlowComponentType, tableOrViewSource, componentRepositoryID, (statement as Laan.Sql.Parser.Entities.WithStatement).CTETable.Value);
                                                    }
                                                }
                         */
                    }
                    else
                    {
                        int tableRepositoryID = -1;
                        if (connectionManagerType == "ADO" || connectionManagerType == "OLEDB" ||
                            connectionManagerType == "ADO.NET" || connectionManagerType == "ODBC")
                        {
                            if (!string.IsNullOrEmpty(tableOrViewName))
                            {
                                // Convert the table name into a select statement, so we can use TSQL parsing.
                                queryDefinition = String.Format("SELECT * FROM {0}", tableOrViewName);
                                ParseTSqlStatement(queryDefinition, connectionID, dataFlowComponentType, tableOrViewSource, componentRepositoryID);
                            }
                        }
                        else if (connectionManagerType == "FILE" || connectionManagerType == "FLATFILE" ||
                            connectionManagerType == "EXCEL")
                        {
                            // add the table to the repository for each distinct connection
                            tableRepositoryID = repository.GetFile(connectionManager.ConnectionString, "localhost");
                            AddTableMappings(dataFlowComponentType, tableOrViewSource, componentRepositoryID, tableRepositoryID);
                        }
                    }
                }
            }


            // special case some components that don't use connection managers
            if (objectType == ClassIDs.XmlSource)
            {
                string xmlFile = GetStringComponentPropertyValue(component, "XMLData");
                int fileID = repository.GetFile(xmlFile, "localhost");
                repository.AddMapping(fileID, componentRepositoryID);

                xmlFile = GetStringComponentPropertyValue(component, "XMLSchemaDefinition");
                fileID = repository.GetFile(xmlFile, "localhost");
                repository.AddMapping(fileID, componentRepositoryID);
            }
            else if (objectType == ClassIDs.RawFileSource)
            {
                string rawFile = GetStringComponentPropertyValue(component, "FileName");
                if (rawFile == null)
                {
                    rawFile = GetStringComponentPropertyValue(component, "FileNameVariable");
                }
                int fileID = repository.GetFile(rawFile, "localhost");
                repository.AddMapping(fileID, componentRepositoryID);
            }
            else if (objectType == ClassIDs.RawFileDest)
            {
                string rawFile = GetStringComponentPropertyValue(component, "FileName");
                if (rawFile == null)
                {
                    rawFile = GetStringComponentPropertyValue(component, "FileNameVariable");
                }
                int fileID = repository.GetFile(rawFile, "localhost");
                repository.AddMapping(componentRepositoryID, fileID);
            }
            // add all the Columns as Attributes (just for fun?)
            // AddColumnNameAttributes(component, componentRepositoryID);
        }

        private int CreateConnection(Microsoft.SqlServer.Dts.Runtime.ConnectionManager connectionManager, out string serverName)
        {
            // todo: is the root object of the connection the root repository object ID?
            string connectionType = connectionManager.CreationName;
            if (connectionType.IndexOf(':') > 0)
                connectionType = connectionType.Substring(0, connectionType.IndexOf(':'));

            int connectionID = AddConnectionObject(connectionManager.Name, connectionManager.Description, connectionType, repository.RootRepositoryObjectID);

            // add the attributes of the connection as well
            repository.AddAttribute(connectionID, Repository.Attributes.ConnectionString, connectionManager.ConnectionString);

            // get the server name/initial catalog, etc. 
            string initialCatalog;

            GetConnectionAttributes(connectionManager, out serverName, out initialCatalog);

            if (string.IsNullOrEmpty(serverName) == false)
            {
                repository.AddAttribute(connectionID, Repository.Attributes.ConnectionServer, serverName);
            }

            if (string.IsNullOrEmpty(initialCatalog) == false)
            {
                repository.AddAttribute(connectionID, Repository.Attributes.ConnectionDatabase, initialCatalog);
            }
            return connectionID;
        }

        private void GetConnectionAttributes(Microsoft.SqlServer.Dts.Runtime.ConnectionManager connectionManager, out string serverName, out string initialCatalog)
        {
            serverName = null;
            initialCatalog = null;

            try
            {
                if ((connectionManager.CreationName == "OLEDB") || (connectionManager.CreationName == "EXCEL"))
                {
                    connectionStringBuilder.Clear();

                    connectionStringBuilder.ConnectionString = connectionManager.ConnectionString;

                    serverName = connectionStringBuilder.DataSource;

                    object outValue;
                    connectionStringBuilder.TryGetValue("Initial Catalog", out outValue);

                    if (outValue != null)
                    {
                        initialCatalog = outValue.ToString();
                    }
                }
                else
                {
                    if (connectionManager.CreationName == "FLATFILE")
                    {
                        serverName = connectionManager.ConnectionString;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(string.Format("Error occurred: Could not completely parse connection string for '{0}': {1}", connectionManager.Name, ex.Message));
            }
        }

        /// <summary>
        ///  enumerate all mappings that exist between a group of source and target components and write them out to the repository.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="dataFlowRepositoryObjectID"></param>
        /// <param name="componentIDToSourceRepositoryObjectMap"></param>
        /// <param name="componentIDToTargetRepositoryObjectMap"></param>
        private void EnumerateMappings(IDTSPipeline pipeline, int dataFlowRepositoryObjectID,
            Dictionary<int, int> componentIDToSourceRepositoryObjectMap,
            Dictionary<int, int> componentIDToRepositoryObjectMap)
        {
            Debug.Assert(dataFlowRepositoryObjectID > 0);
            Debug.Assert(componentIDToSourceRepositoryObjectMap.Count > 0);
            Debug.Assert(componentIDToRepositoryObjectMap.Count > 0);

            Dictionary<int, bool> outputsAlreadyInvestigated = new Dictionary<int, bool>(); // key: outputID

            foreach (int sourceComponentID in componentIDToSourceRepositoryObjectMap.Keys)
            {
                EnumerateMappingForSource(pipeline, dataFlowRepositoryObjectID, sourceComponentID,
                    componentIDToRepositoryObjectMap, outputsAlreadyInvestigated);
            }
        }

        /// <summary>
        /// Enumerate all mappings that exist between a source and a list of target components and write them out to the repository. 
        /// If there are multiple mappings between a source and target only one will be written out.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="dataFlowRepositoryObjectID"></param>
        /// <param name="sourceComponentID"></param>
        /// <param name="componentIDToTargetRepositoryObjectMap"></param>
        private void EnumerateMappingForSource(IDTSPipeline pipeline, int dataFlowRepositoryObjectID,
            int sourceComponentID, Dictionary<int, int> componentIDToRepositoryObjectMap,
            Dictionary<int, bool> outputsAlreadyInvestigated)
        {
            // get the component
            IDTSComponentMetaData componentMetadata = pipeline.ComponentMetaDataCollection.GetObjectByID(sourceComponentID);
            Debug.Assert(componentMetadata != null);

            Queue<IDTSOutput> outputsToInvestigate = new Queue<IDTSOutput>();

            // go through each output of the source.
            foreach (IDTSOutput output in componentMetadata.OutputCollection)
            {
                outputsToInvestigate.Enqueue(output);
            }

            while (outputsToInvestigate.Count > 0)
            {
                IDTSOutput output = outputsToInvestigate.Dequeue();

                int outputID = output.ID;

                if (outputsAlreadyInvestigated.ContainsKey(outputID) == false)
                {
                    outputsAlreadyInvestigated.Add(outputID, true);

                    // find out whethere the next component has one of the source IDs
                    IDTSComponentMetaData connectedComponent = GetConnectedComponentID(pipeline, output);
                    if (connectedComponent != null)
                    {
                        int connectedComponentID = connectedComponent.ID;

                        // if the target component doesn't exist in the repository already, add it now
                        int targetObjectRepositoryID = -1;
                        if (componentIDToRepositoryObjectMap.ContainsKey(connectedComponentID))
                        {
                            targetObjectRepositoryID = componentIDToRepositoryObjectMap[connectedComponentID];
                        }
                        else
                        {
                            targetObjectRepositoryID = AddObject(connectedComponent.Name, connectedComponent.Description, connectedComponent.ComponentClassID, dataFlowRepositoryObjectID);
                            componentIDToRepositoryObjectMap.Add(connectedComponentID, targetObjectRepositoryID);
                        }

                        Debug.Assert(componentIDToRepositoryObjectMap.ContainsKey(output.Component.ID));
                        int sourceObjectRepositoryID = componentIDToRepositoryObjectMap[output.Component.ID];

                        // create a mapping if it doesn't exist already
                        if (repository.DoesMappingExist(sourceObjectRepositoryID, targetObjectRepositoryID) == false)
                        {
                            repository.AddMapping(sourceObjectRepositoryID, targetObjectRepositoryID);
                        }

                        // otherwise add all outputs from the connected component as well
                        foreach (IDTSOutput outputToTraverse in connectedComponent.OutputCollection)
                        {
                            outputsToInvestigate.Enqueue(outputToTraverse);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Traverses the output->input path (if there is one) and reports the component of the input.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        private IDTSComponentMetaData GetConnectedComponentID(IDTSPipeline pipeline, IDTSOutput output)
        {
            int outputID = output.ID;

            foreach (IDTSPath path in pipeline.PathCollection)
            {
                if (path.StartPoint.ID == outputID)
                {
                    // got it
                    return path.EndPoint.Component;
                }
            }

            // this output is dangling, not connected to anything else
            return null;
        }

        /// <summary>
        /// if the component is interesting, return that fact and the object type
        /// </summary>
        /// <param name="package">ssis package that contains the component to inspect</param>
        /// <param name="taskHost">ssis task that contains the component to inspect</param>
        /// <param name="component">component to inspect</param>
        /// <param name="objectTypeName">object type name</param>
        /// <param name="componentType">Source/Destination/Transform</param>
        /// <param name="domain">Repository.Domains.Relational or Repository.Domains.File</param>
        /// <param name="tableOrViewName">table or view for relational domain</param>
        /// <param name="tableOrViewSource">Whether the table/view (if specified) is a source or a destination</param>
        /// <returns></returns>
        private bool IsComponentInteresting(Package package, TaskHost taskHost, IDTSComponentMetaData component,
            out string objectTypeName,
            out DTSPipelineComponentType componentType,
            out string domain,
            out string tableOrViewName,
            out bool tableOrViewSource,
            out string queryDefinition)
        {
            objectTypeName = component.ComponentClassID;
            domain = null;
            tableOrViewName = null;
            queryDefinition = null;
            tableOrViewSource = false;

            // for managed components, the type name is stored in a custom property
            if (string.Compare(objectTypeName, ClassIDs.ManagedComponentWrapper, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                objectTypeName = component.CustomPropertyCollection["UserComponentTypeName"].Value.ToString();
            }

            if (pipelineComponentInfos.Contains(objectTypeName) == false)
            {
                //                throw new Exception(string.Format("Unknown component type encountered: {0}, {1}", objectTypeName, component.Name));
                Console.WriteLine(string.Format("Unknown component type encountered (ignoring): {0}, {1}", objectTypeName, component.Name));
                componentType = DTSPipelineComponentType.Transform;
                return false;
            }

            PipelineComponentInfo pipelineComponentInfo = pipelineComponentInfos[objectTypeName];

            Debug.Assert(pipelineComponentInfo != null);

            componentType = pipelineComponentInfo.ComponentType;

            string componentClassID = component.ComponentClassID;

            if ((string.Equals(componentClassID, ClassIDs.OleDbSource))
                 || (string.Equals(componentClassID, ClassIDs.ExcelSource))
                 || (string.Equals(componentClassID, ClassIDs.OleDbDestination)))
            {
                domain = Repository.Domains.Relational;
                GetOleDbComponentsInfo(package, taskHost, component, ref tableOrViewName, ref queryDefinition);
                tableOrViewSource = !(string.Equals(componentClassID, ClassIDs.OleDbDestination));
            }
            else if (string.Equals(componentClassID, ClassIDs.SqlDestination))
            {
                domain = Repository.Domains.Relational;
                tableOrViewName = GetStringComponentPropertyValue(component, "BulkInsertTableName");
                tableOrViewSource = false;
            }
            else if ((string.Equals(componentClassID, ClassIDs.FlatFileSource))
                     || (string.Equals(componentClassID, ClassIDs.FlatFileDest)))
            {
                domain = Repository.Domains.File;

                // flat file connection is always 'interesting', but does not use table name, which was nulled out earlier anyways
            }
            else if (string.Equals(componentClassID, ClassIDs.Lookup))
            {
                domain = Repository.Domains.Relational;
                queryDefinition = GetLookupInfo(component);
            }
            else if (string.Equals(componentClassID, ClassIDs.FuzzyLookup))
            {
                domain = Repository.Domains.Relational;
                tableOrViewName = GetFuzzyLookupInfo(component);
                tableOrViewSource = true;
            }
            else if (string.Equals(componentClassID, ClassIDs.OLEDBCommand))
            {
                queryDefinition = GetLookupInfo(component);
            }

            return (objectTypeName != null);
        }

        private static string GetFuzzyLookupInfo(IDTSComponentMetaData component)
        {
            return GetStringComponentPropertyValue(component, "ReferenceTableName");
        }

        /// <summary>
        ///  returns information about the query used by the lookup/fuzzy lookup components
        /// </summary>
        /// <param name="component"></param>
        /// <param name="queryDefinition"></param>
        private static string GetLookupInfo(IDTSComponentMetaData component)
        {
            return GetStringComponentPropertyValue(component, "SqlCommand");
        }

        private static string GetStringComponentPropertyValue(IDTSComponentMetaData component, string propertyName)
        {
            IDTSCustomProperty prop = null;
            try
            {
                prop = component.CustomPropertyCollection[propertyName];
            }
            catch
            {
                // No Action here.
            }
            string value = null;

            if (prop != null && prop.Value is string)
            {
                value = prop.Value.ToString();
            }

            return value;
        }

        /// <summary>
        ///  returns information about OleDb source, destination and Excel source.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="tableOrViewName"></param>
        /// <param name="queryDefinition"></param>
        private static void GetOleDbComponentsInfo(Package package, TaskHost taskHost, IDTSComponentMetaData component, ref string tableOrViewName, ref string queryDefinition)
        {
            IDTSCustomProperty prop = component.CustomPropertyCollection["AccessMode"];
            string strVariableName;
            tableOrViewName = "";
            queryDefinition = "";

            if (prop != null && prop.Value is int)
            {
                int accessMode = (int)prop.Value;

                switch (accessMode)
                {
                    case (int)AccessMode.AM_OPENROWSET:
                        tableOrViewName = GetStringComponentPropertyValue(component, "OpenRowset");
                        break;

                    case (int)AccessMode.AM_OPENROWSET_VARIABLE:
                        strVariableName = GetStringComponentPropertyValue(component, "OpenRowsetVariable");
                        tableOrViewName = GetVariable(package, taskHost, strVariableName);
                        break;

                    case (int)AccessMode.AM_SQLCOMMAND:
                        queryDefinition = GetStringComponentPropertyValue(component, "SqlCommand");
                        break;

                    case (int)AccessMode.AM_SQLCOMMAND_VARIABLE:
                        strVariableName = GetStringComponentPropertyValue(component, "SqlCommandVariable");
                        if (strVariableName == null)
                        {
                            tableOrViewName = GetStringComponentPropertyValue(component, "OpenRowset");
                        }
                        else
                        {
                            queryDefinition = GetVariable(package, taskHost, strVariableName);
                        }
                        break;

                    case (int)AccessMode.AM_OPENROWSET_FASTLOAD_VARIABLE:
                        strVariableName = GetStringComponentPropertyValue(component, "OpenRowsetVariable");
                        if (strVariableName == null)
                        {
                            tableOrViewName = string.Empty;
                            queryDefinition = string.Empty;
                            Console.WriteLine("Unexpected setup for OLEDB Fast Load from Variable.  Table details not collected.\r\nPlease report to https://github.com/keif888/SQLServerMetadata/issues\r\nWith the SSIS Package if possible.");
                        }
                        else
                        {
                            tableOrViewName = GetVariable(package, taskHost, strVariableName);
                        }
                        break;

                    default:
                        throw new Exception(string.Format("Invalid access mode {0}.", accessMode));
                }
            }
        }

        /// <summary>
        /// Retrieves the value for a variable from either the taskHost or the package
        /// </summary>
        /// <param name="package">the SSIS package that is being analysed</param>
        /// <param name="taskHost">the container within the SSIS package that holds the item using the variable</param>
        /// <param name="strVariableName">the name of the variable that is to be found.</param>
        /// <returns></returns>
        private static string GetVariable(Package package, TaskHost taskHost, string strVariableName)
        {
            if (strVariableName != null)
            {
                if (taskHost.Variables.Contains(strVariableName))
                    return taskHost.Variables[strVariableName].Value.ToString();
                else if (package.Variables.Contains(strVariableName))
                    return package.Variables[strVariableName].Value.ToString();
                else
                    return null;
            }
            else
                return null;
        }

        private int AddConnectionObject(string name, string description, string type, int parentObjectID)
        {
            string id = connectionTypeToIDMap[type];
            return repository.AddObject(name, description, id, parentObjectID);
        }

        /// <summary>
        /// Add a new object to the repository
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="objectType"></param>
        /// <param name="parentObjectID"></param>
        /// <returns></returns>
        private int AddObject(string name, string description, string objectType, int parentObjectID)
        {
            if (!repository.IsTypeDefined(objectType))
            {
                // object type might be a progid instead of a clsid. We need to use clsids if one exists
                bool isGuid = true;
                Guid guid;
                isGuid = Guid.TryParse(objectType, out guid);

                if (isGuid == false)
                {
                    try
                    {
                        uint result = NativeMethods.CLSIDFromProgID(objectType, out guid);
                        if (result == 0)
                        {
                            objectType = guid.ToString("B").ToUpper();
                        }
                    }
                    catch (System.Exception)
                    {
                        // ignore, use the original objectType
                    }
                }
            }

            return repository.AddObject(name, description, objectType, parentObjectID);
        }
    }



    public class PackageEventHandler : DefaultEvents
    {
        public List<string> eventMessages;
        public PackageEventHandler()
        {
            eventMessages = new List<string>();
        }

        public override bool OnError(DtsObject source, int errorCode, string subComponent, string description, string helpFile, int helpContext, string idofInterfaceWithError)
        {
            HandleEvent("Error", errorCode, subComponent, description);
            return base.OnError(source, errorCode, subComponent, description, helpFile, helpContext, idofInterfaceWithError);
        }

        public override void OnInformation(DtsObject source, int informationCode, string subComponent, string description, string helpFile, int helpContext, string idofInterfaceWithError, ref bool fireAgain)
        {
            HandleEvent("Information", informationCode, subComponent, description);
            base.OnInformation(source, informationCode, subComponent, description, helpFile, helpContext, idofInterfaceWithError, ref fireAgain);
        }

        public override void OnWarning(DtsObject source, int warningCode, string subComponent, string description, string helpFile, int helpContext, string idofInterfaceWithError)
        {
            HandleEvent("Warning", warningCode, subComponent, description);
            base.OnWarning(source, warningCode, subComponent, description, helpFile, helpContext, idofInterfaceWithError);
        }

        private void HandleEvent(string type, int errorCode, string subComponent, string description)
        {
            eventMessages.Add(String.Format("[{0}] {1}: {2}: {3}", type, errorCode, subComponent, description));
        }

    }
}
