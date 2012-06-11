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
	/// Responsible for enumerating relational objects
	/// </summary>
	class RelationalEnumerator
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
			public const string Database = "Database";
			public const string Table = "Table";
			public const string Function = "Function";
			public const string Procedure = "Procedure";
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
			AddRelationalObjectType(RelationalEnumerator.ObjectTypes.Database);
			AddRelationalObjectType(RelationalEnumerator.ObjectTypes.Table);
			AddRelationalObjectType(RelationalEnumerator.ObjectTypes.Function);
			AddRelationalObjectType(RelationalEnumerator.ObjectTypes.Procedure);

			return true;
		}

		/// <summary>
		///  helper for adding types for Relational domain
		/// </summary>
		/// <param name="type"></param>
		private void AddRelationalObjectType(string type)
		{
			repository.AddObjectType(Repository.Domains.Relational, type);
		}
	}
}
