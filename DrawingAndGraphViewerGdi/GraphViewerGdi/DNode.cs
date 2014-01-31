using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Msagl.Drawing;
using GLEEEdge=Microsoft.Msagl.Edge;
using DrawingEdge=Microsoft.Msagl.Drawing.Edge;
using GLEENode=Microsoft.Msagl.Node;
using DrawingNode=Microsoft.Msagl.Drawing.Node;
namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// it is a class holding Microsoft.Msagl.Drawing.Node and Microsoft.Msagl.Node
    /// </summary>
    public class DNode : DObject, IViewerNode, IHavingDLabel {

        DLabel label;
        /// <summary>
        /// gets / sets the rendered label of the object
        /// </summary>
        public DLabel Label {
            get { return label; }
            set { label = value; }
        }

        Microsoft.Msagl.Drawing.Node drawingNode;
        internal List<DEdge> outEdges = new List<DEdge>();
        internal List<DEdge> inEdges = new List<DEdge>();
        internal List<DEdge> selfEdges = new List<DEdge>();

        internal DNode(DrawingNode drawingNodeParam) {
            this.DrawingNode = drawingNodeParam;
        }


        internal void AddOutEdge(DEdge edge) {
            outEdges.Add(edge);
        }

        internal void AddInEdge(DEdge edge) {
            inEdges.Add(edge);
        }

        internal void AddSelfEdge(DEdge edge) {
            selfEdges.Add(edge);
        }

  
        /// <summary>
        /// return the color of a node
        /// </summary>
        public System.Drawing.Color Color {
            get { return Draw.MsaglColorToDrawingColor(this.DrawingNode.Attr.Color); }
  
        }
        
   
        /// <summary>
        /// Fillling color of a node
        /// </summary>
        public System.Drawing.Color FillColor {
            get { return Draw.MsaglColorToDrawingColor(this.DrawingNode.Attr.FillColor); }
        }

        /// <summary>
        /// the corresponding drawing node
        /// </summary>
        public Microsoft.Msagl.Drawing.Node DrawingNode {
            get { return drawingNode; }
            set { drawingNode = value;}
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetStrokeFill() {}

       
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Microsoft.Msagl.Node GeometryNode {
            get { return this.DrawingNode.Attr.GeometryNode; }
        }
       
        float dashSize;
        internal override float DashSize() {
            if (dashSize > 0)
                return dashSize;
            float w = DrawingNode.Attr.LineWidth;
            if (w < 0)
                w = 1;
            float dashSizeInPoints = (float)(Draw.dashSize * GViewer.dpi);
            return dashSize = dashSizeInPoints / w;
        }

        #region IDraggableNode Members

        /// <summary>
        /// returns the corresponding DrawingNode
        /// </summary>
        public Microsoft.Msagl.Drawing.Node Node {
            get { return this.DrawingNode; }
        }

        /// <summary>
        /// return incoming edges
        /// </summary>
        public IEnumerable<IViewerEdge> InEdges {
            get {
                foreach (DEdge e in this.inEdges)
                    yield return e;
            }
        }

        /// <summary>
        /// returns outgoing edges
        /// </summary>
        public IEnumerable<IViewerEdge> OutEdges {
            get {
                foreach (DEdge e in this.outEdges)
                    yield return e;
            }
        }

        /// <summary>
        /// returns self edges
        /// </summary>
        public IEnumerable<IViewerEdge> SelfEdges {
            get {
                foreach (DEdge e in this.selfEdges)
                    yield return e;
            }
        }

        #endregion

        #region IDraggableObject Members

        /// <summary>
        /// returns the corresponding drawing object
        /// </summary>
        override public DrawingObject DrawingObject {
            get { return this.DrawingNode; }
        }

        #endregion

        internal void RemoveOutEdge(DEdge de) {
            outEdges.Remove(de);
        }

        internal void RemoveInEdge(DEdge de) {
            inEdges.Remove(de);
        }

        internal void RemoveSelfEdge(DEdge de) {
            selfEdges.Remove(de);
        }
    }
}
