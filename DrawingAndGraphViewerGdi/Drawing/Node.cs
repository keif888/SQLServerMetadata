using System;
using Rectangle = Microsoft.Msagl.Splines.Rectangle;
using System.Collections.Generic;

namespace Microsoft.Msagl.Drawing {

    /// <summary>
    /// If this delegate is not null and returns true then no node rendering is done by the viewer, the delegate is supposed to do the job.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="graphics"></param>
    public delegate bool DelegateToOverrideNodeRendering(Node node, object graphics);

    /// <summary>
    /// Node of the Microsoft.Msagl.Drawing.
    /// </summary>
    [Serializable]
    public class Node : DrawingObject, ILabeledObject {

        Label label;
        /// <summary>
        /// the label of the object
        /// </summary>
        public Label Label {
            get { return label; }
            set { label = value; }
        }

/// <summary>
/// A delegate to draw node
/// </summary>
        DelegateToOverrideNodeRendering drawNodeDelegate;
        /// <summary>
        /// If this delegate is not null and returns true then no node rendering is done
        /// </summary>
        public DelegateToOverrideNodeRendering DrawNodeDelegate {
            get { return drawNodeDelegate; }
            set { drawNodeDelegate = value; }
        }


/// <summary>
/// gets the node bounding box
/// </summary>
        override public Rectangle BoundingBox {
            get { return this.Attr.GeometryNode.BoundingBox; }
        }

        /// <summary>
        /// Attribute controling the node drawing.
        /// </summary>
        NodeAttr attr;
/// <summary>
/// gets or sets the node attribute
/// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Attr")]
        public NodeAttr Attr {
            get { return attr; }
            set { attr = value; }
        }
        /// <summary>
        /// Creates a Node instance
        /// </summary>
        /// <param name="id">node name</param>
        public Node(string id) {
            if (String.IsNullOrEmpty(id)) 
                throw new InvalidOperationException("creating a node with a null or empty id");

            this.Label = new Label();
            
            this.Attr = new NodeAttr();
            this.attr.Id = id;
            this.Label.Text = id; //one can change the label later
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "o")]
        public int CompareTo(object o) {
            Node n = o as Node;
            if (n == null)
                throw new InvalidOperationException();
            return String.Compare(this.Attr.Id, n.Attr.Id, StringComparison.Ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return Utils.Quote(Label.Text) + "[" + Attr.ToString() + "]";
        }
/// <summary>
/// the node ID
/// </summary>
        public string Id {
            get {
                return this.attr.Id;
            }
            set {
                attr.Id = value;
            }
        }

        List<Edge> outEdges=new List<Edge>();
            /// <summary>
            /// Enumerates over outgoing edges of the node
            /// </summary>
        public IEnumerable<Edge> OutEdges{ get{return outEdges;}} 

        List<Edge> inEdges=new List<Edge>();
            
        /// <summary>
        /// enumerates over the node incoming edges
        /// </summary>
        public IEnumerable<Edge> InEdges{ get{return inEdges;}}

        List<Edge> selfEdges=new List<Edge>();
            
        /// <summary>
        /// enumerates over the node self edges
        /// </summary>
        public IEnumerable<Edge> SelfEdges{ get{return selfEdges;}}
/// <summary>
/// add an incoming edge to the node
/// </summary>
/// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "e")]
        public void AddInEdge(Edge e){
            inEdges.Add(e);
        }

        /// <summary>
        /// adds and outcoming edge to the node
        /// </summary>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "e")]
        public void AddOutEdge(Edge e){
            outEdges.Add(e);
        }

        /// <summary>
        /// adds a self edge to the node
        /// </summary>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "e")]
        public void AddSelfEdge(Edge e){
            selfEdges.Add(e);
        }

        /// <summary>
        /// Removes an in-edge from the node's edge list (this won't remove the edge from the graph).
        /// </summary>
        /// <param name="edge">The edge to be removed</param>
        public void RemoveInEdge(Edge edge)
        {
            inEdges.Remove(edge);
        }

        /// <summary>
        /// Removes an out-edge from the node's edge list (this won't remove the edge from the graph).
        /// </summary>
        /// <param name="edge">The edge to be removed</param>
        public void RemoveOutEdge(Edge edge)
        {
            outEdges.Remove(edge);
        }

        /// <summary>
        /// Removes a self-edge from the node's edge list (this won't remove the edge from the graph).
        /// </summary>
        /// <param name="edge">The edge to be removed</param>
        public void RemoveSelfEdge(Edge edge)
        {
            selfEdges.Remove(edge);
        }

/// <summary>
/// gets the geometry node
/// </summary>
        public override GeometryObject GeometryObject {
            get { return this.attr.GeometryNode; }
        }
        /// <summary>
        /// a shortcut to the node label text
        /// </summary>

        public string LabelText {
            get { return Label!=null?Label.Text:""; }
            set {
                if(Label==null)
                    Label=new Label();
                Label.Text = value; 
            }
        }

/// <summary>
/// enumerates over all edges
/// </summary>
        public IEnumerable<Edge> Edges {
            get {
                foreach (Edge e in InEdges)
                    yield return e;
                foreach (Edge e in OutEdges)
                    yield return e;
                foreach (Edge e in SelfEdges)
                    yield return e;
            }
        }
    }
}
