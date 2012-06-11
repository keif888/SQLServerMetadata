///
/// Microsoft SQL Server 2005 Business Intelligence Metadata Reporting Samples
/// Dependency Analyzer Sample
/// 
/// Copyright (c) Microsoft Corporation.  All rights reserved.
///
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Samples.DependencyAnalyzer
{
    /// <summary>
    /// Responsible for enumerating file objects
    /// </summary>
    class FileEnumerator
    {
        /// <summary>
        /// link to the repository to write into
        /// </summary>
        private Repository repository;

        /// <summary>
		/// the types of objects in the file domain
        /// </summary>
        public class ObjectTypes
        {
            public const string File = "File";

            public const string Machine = "Machine";
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
			AddFileObjectType(ObjectTypes.File);
			AddFileObjectType(ObjectTypes.Machine);

            return true;
        }

        /// <summary>
		///  helper for adding types for File domain
        /// </summary>
        /// <param name="type"></param>
        private void AddFileObjectType(string type)
        {
			repository.AddObjectType(Repository.Domains.File, type);
        }
    }
}
