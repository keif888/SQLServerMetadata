///
/// SQL Server 2008 Business Intelligence Metadata
/// 
/// Copyright (c) Keith Martin.  All rights reserved.
///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.DependencyAnalyzer
{
    /// <summary>
    /// Responsible for enumerating report objects.
    /// </summary>
    class ReportEnumerator
    {
        /// <summary>
        /// link to the repository to write into
        /// </summary>
        private Repository repository;

        /// <summary>
        /// the types of objects in the Report domain
        /// </summary>
        public class ObjectTypes
        {
            public const string ReportServer = "ReportServer";
            public const string Report = "Report";
            public const string DataSet = "DataSet";
        }

        /// <summary>
        /// called to initialize this enumerator. This method also registers the types in the
        /// file domain.
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public bool Initialize(Repository repository)
        {
            this.repository = repository;

            // add types of objects to the object types table
            AddReportObjectType(ObjectTypes.ReportServer);
            AddReportObjectType(ObjectTypes.Report);
            AddReportObjectType(ObjectTypes.DataSet);

            return true;
        }

        /// <summary>
        ///  helper for adding types for Report domain
        /// </summary>
        /// <param name="type"></param>
        private void AddReportObjectType(string type)
        {
            repository.AddObjectType(Repository.Domains.SSRS, type);
        }
    }
}
