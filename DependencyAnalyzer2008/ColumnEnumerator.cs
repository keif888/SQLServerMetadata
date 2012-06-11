///
/// Microsoft SQL Server 2008 Business Intelligence Metadata Reporting
/// Dependency Analyzer
/// 
/// Copyright (c) Keith Martin.  All rights reserved.
///
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Samples.DependencyAnalyzer
{
    /// <summary>
    /// Responsible for enumerating relational objects
    /// </summary>
    class ColumnEnumerator
    {
        /// <summary>
        /// link to the repository to write into
        /// </summary>
        private Repository repository;

        /// <summary>
        /// the types of objects in the relational domain
        /// </summary>
        public class ObjectTypes
        {
            public const string Column = "Column";
        }

        /// <summary>
        /// called to initialize this enumerator. This method also registers the types in the
        /// relational domain.
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public bool Initialize(Repository repository)
        {
            this.repository = repository;

            // add types of objects to the object types table
            //AddColumnObjectType(ColumnEnumerator.ObjectTypes.Column);

            return true;
        }

        /// <summary>
        ///  helper for adding types for Relational domain
        /// </summary>
        /// <param name="type"></param>
        private void AddColumnObjectType(string type)
        {
            repository.AddObjectType(Repository.Domains.Relational, type);
        }
    }
}
