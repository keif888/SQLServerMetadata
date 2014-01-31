using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// a base class for object that are drawn
    /// </summary>
    abstract public class ObjectWithBox {
        abstract internal Microsoft.Msagl.Splines.Rectangle Box { get; } 
    }
}
