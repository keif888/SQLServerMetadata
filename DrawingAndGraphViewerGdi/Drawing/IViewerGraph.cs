using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// this interface represents a graph that is drawn by the viewer and can be edited by it
    /// </summary>
    public interface IViewerGraph {
        /// <summary>
        /// gets the drawing graph
        /// </summary>
        Graph DrawingGraph { get; }
    }
}
