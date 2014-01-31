using System;
using P2=Microsoft.Msagl.Point;
using Rectangle = Microsoft.Msagl.Splines.Rectangle;
namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// Edge of Microsoft.Msagl.Drawing
    /// </summary>
    [Serializable]
    public class Edge : DrawingObject, ILabeledObject {

        Label label;
        /// <summary>
        /// the label of the object
        /// </summary>
        public Label Label {
            get { return label; }
            set { label = value; }
        }

/// <summary>
/// a shortcut to edge label
/// </summary>
        public string LabelText {
            get { return Label == null ? "" : Label.Text; }
            set {
                if (Label == null)
                    Label = new Label();
                Label.Text = value;
            }
        }

/// <summary>
/// the edge bounding box
/// </summary>
        override public Rectangle BoundingBox {
            get {

                if (this.attr.GeometryEdge == null)
                    return new Rectangle(0, 0, -1, -1);

                Rectangle bb = attr.EdgeCurve.BBox;

                if (Label != null)
                    bb.Add(Label.BoundingBox);
                
                if (this.attr.ArrowAtTarget)
                    bb.Add(this.attr.ArrowAtTargetPosition);

                if (this.attr.ArrowAtSource)
                    bb.Add(this.attr.ArrowAtSourcePosition);

                return bb;
            }
        }


        EdgeAttr attr;
        /// <summary>
        /// The edge attribute.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Attr")]
        public EdgeAttr Attr {
            get { return attr; }
            set { attr = value; }
        }
        /// <summary>
        /// The id of the edge source node.
        /// </summary>
        string source;
        /// <summary>
        /// The id of the edge target node
        /// </summary>
        string target;

        /// <summary>
        /// source id, label ,target id
        /// </summary>
        /// <param name="source"> cannot be null</param>
        /// <param name="labelText">label can be null</param>
        /// <param name="target">cannot be null</param>
        public Edge(string source, string labelText, string target) {
            if (String.IsNullOrEmpty(source) || String.IsNullOrEmpty(target))
                throw new InvalidOperationException("Creating an edge with null or empty source or target IDs");
            this.source = source;
            this.target = target;

            this.attr = new EdgeAttr();
            if(!String.IsNullOrEmpty(labelText))
                this.Label = new Label(labelText);
      
        }

        /// <summary>
        /// creates a detached edge
        /// </summary>
        /// <param name="sourceNode"></param>
        /// <param name="targetNode"></param>
        /// <param name="connection">controls is the edge will be connected to the graph</param>
        public Edge(Node sourceNode, Node targetNode, Connection connection)
            : this(sourceNode.Id, null, targetNode.Id) {
            this.SourceNode = sourceNode;
            this.TargetNode = targetNode;
            if (connection == Connection.Connected) {
                if (sourceNode == targetNode)
                    sourceNode.AddSelfEdge(this);
                else {
                    sourceNode.AddOutEdge(this);
                    targetNode.AddInEdge(this);
                }
            }
        }

        /// <summary>
        /// Head->Tail->Label.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return Utils.Quote(source) + " -> " + Utils.Quote(target) +(Label==null?"":"[" + Label.Text + "]");
        }
/// <summary>
/// the edge source node ID
/// </summary>
        public string Source {
            get { return source; }
        }
/// <summary>
/// the edge target node ID
/// </summary>
        public string Target {
            get { return target; }
        }

        private Node sourceNode;
        /// <summary>
        /// the edge source node
        /// </summary>
        public Node SourceNode {
            get { return sourceNode; }
            internal set { sourceNode = value; }
        }
        private Node targetNode;
        /// <summary>
        /// the edge target node
        /// </summary>
        public Node TargetNode {
            get { return targetNode; }
            internal set { targetNode = value; }
        }
/// <summary>
/// gets the corresponding geometry edge
/// </summary>
        public override GeometryObject GeometryObject {
            get { return this.Attr.GeometryEdge; }
        }
    }
}
