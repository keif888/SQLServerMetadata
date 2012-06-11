///
/// Microsoft SQL Server 2005 Business Intelligence Metadata Reporting Samples
/// Dependency Analyzer Sample
/// 
/// Copyright (c) Microsoft Corporation.  All rights reserved.
///
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Samples.DependencyAnalyzer
{
    internal static class NativeMethods
    {
        [DllImport("ole32", PreserveSig = true)]
        internal static extern uint CLSIDFromProgID(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lplpszProgID,
           [Out, MarshalAs(UnmanagedType.Struct)] out Guid clsid //CLSID for which the ProgID is requested
          );
    }
}
