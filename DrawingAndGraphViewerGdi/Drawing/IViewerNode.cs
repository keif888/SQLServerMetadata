using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// interface of a node that could be edited
    /// </summary>
    public interface IViewerNode:IViewerObject {
        /// <summary>
        /// the corresponding drawing node
        /// </summary>
        Node Node { get;}
        /// <summary>
        /// incomind editable edges
        /// </summary>
        IEnumerable<IViewerEdge> InEdges {get;}
        /// <summary>
        /// outgoing editable edges
        /// </summary>
        IEnumerable<IViewerEdge> OutEdges {get;}
        /// <summary>
        /// self editable edges
        /// </summary>
        IEnumerable<IViewerEdge> SelfEdges {get;}
       /// <summary>
       /// set color and fillcolor from the node attributes
       /// </summary>
        void SetStrokeFill();
    }
}
