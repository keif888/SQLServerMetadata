using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using Microsoft.Msagl;
using Microsoft.Msagl.Splines;
using System.ComponentModel;
using P2=Microsoft.Msagl.Point;
using BBox = Microsoft.Msagl.Splines.Rectangle;
using DrawingGraph = Microsoft.Msagl.Drawing.Graph;
using GeometryEdge = Microsoft.Msagl.Edge;
using GeometryNode = Microsoft.Msagl.Node;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;
using DrawingNode = Microsoft.Msagl.Drawing.Node;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// Graph for drawing. Putting an instance of this class to property Microsoft.Msagl.Drawing of DOTViewer
    /// triggers the drawing.
    /// </summary>
    [Serializable]
    public class Graph : DrawingObject, ILabeledObject {
#if DEBUGGLEE
        [NonSerialized]
        Database dataBase;
/// <summary>
/// debug only
/// </summary>
        public Database DataBase {
            get { return dataBase; }
            set { dataBase = value; }
        }
#endif
        Label label;
        /// <summary>
        /// the label of the object
        /// </summary>
        public Label Label {
            get { return label; }
            set { label = value; }
        }

        LayoutAlgorithmSettings layoutAlgorithm=new SugiyamaLayoutSettings();
        
        /// <summary>
        /// the properties of the layout algorithm
        /// </summary>
        public LayoutAlgorithmSettings LayoutAlgorithmSettings {
            get{return layoutAlgorithm;}
            set{ layoutAlgorithm=value;}
        }

        static void WriteNodeCollection(TextWriter sw, IEnumerable nodeLabels) {
            int i = 0;

            sw.Write(" ");

            foreach (string s in nodeLabels) {
                sw.Write(s); sw.Write(" ");
                i = (i + 1) % 6;

                if (i == 0)
                    sw.WriteLine("");

            }
        }

        void WriteNodes(TextWriter sw) {
            sw.WriteLine("//nodes");
            foreach (Node node in this.nodeMap.Values)
                sw.WriteLine(node.ToString());

        }

        void WriteMinLayer(TextWriter sw) {
            if (MinLayer != null) {
                WriteLayer(sw, "min", MinLayer);
            }
        }
        void WriteMaxLayer(TextWriter sw) {
            if (MaxLayer != null) {
                WriteLayer(sw, "max", MaxLayer);
            }
        }
        void WriteSameLayers(TextWriter sw) {
            foreach (IEnumerable i in SameLayers)
                WriteLayer(sw, "same", i);
        }

        static void WriteLayer(TextWriter sw, string tag, IEnumerable layer) {
            sw.Write("{layer=" + tag);
            WriteNodeCollection(sw, layer);
            sw.WriteLine("}");
        }
        void WriteLayers(TextWriter sw) {
            WriteMinLayer(sw);
            WriteMaxLayer(sw);
            WriteSameLayers(sw);
        }


        /// <summary>
        /// Prints Microsoft.Msagl.Drawing in the DOT format - has side effects!
        /// </summary>
        /// <returns>String</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.IO.StringWriter.#ctor")]
        public override string ToString() {

       
            StringWriter sw = new StringWriter();

            sw.WriteLine("digraph \"" + this.Label + "\" {");

            this.WriteStms(sw);

            sw.WriteLine("}");

            sw.Close();

            return sw.ToString();

        }


        void WriteEdges(TextWriter tw) {
            foreach (Edge edge in this.Edges) {
                tw.WriteLine(edge.ToString());
            }
        }

        void WriteStms(TextWriter sw) {
            sw.WriteLine(attr.ToString(Label.Text));
            WriteNodes(sw);
            WriteLayers(sw);
            WriteEdges(sw);

        }

        /// <summary>
        /// Returns the bounding box of the graph
        /// </summary>
        public override Rectangle BoundingBox {
            get {
                return
                this.GeometryGraph != null ?
                    PumpByBorder(this.GeometryGraph.BoundingBox) : new Rectangle(0, 0, new Point(1, 1));
            }
        }

        private Rectangle PumpByBorder(Rectangle rectangle) {
            P2 del=new P2(this.Attr.Border, this.Attr.Border);
            return new Rectangle(rectangle.LeftBottom - del, rectangle.RightTop + del);
        }

        /// <summary>
        /// The graph attribute.
        /// </summary>
        internal GraphAttr attr;
        /// <summary>
        /// The graph attribute property
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Attr")]
        public GraphAttr Attr {
            get { return attr; }
            set { attr = value; }
        }
        /// <summary>
        /// the width of the graph
        /// </summary>
        public double Width {
            get {
                return this.GeometryGraph!=null?this.GeometryGraph.Width + this.Attr.Border:1;
            }

        }

        /// <summary>
        /// the height of the graph
        /// </summary>
        public double Height {
            get {
                return this.GeometryGraph!=null?this.GeometryGraph.Height+this.Attr.Border:1;
            }
        }
        /// <summary>
        /// left of the graph
        /// </summary>
        public double Left {
            get {
                return this.GeometryGraph!=null?this.GeometryGraph.Left-this.Attr.Border:0;
            }

        }
/// <summary>
/// top of the graph
/// </summary>
        public double Top {
            get {
                return this.GeometryGraph!=null?this.GeometryGraph.Top + this.Attr.Border:1;
            }

        }
/// <summary>
/// bottom of the graph
/// </summary>
        public double Bottom {
            get {

                return this.GeometryGraph!=null?this.GeometryGraph.Bottom - this.Attr.Border:0;
            }

        }
        /// <summary>
        /// right of the graph
        /// </summary>
        public double Right {
            get {
                return this.GeometryGraph!=null?this.GeometryGraph.Right + this.Attr.Border:1;
            }

        }

#if DEBUGGLEE
        List<Node> history = new List<Node>();
/// <summary>
/// debug only visible
/// </summary>
        public ICollection<Node> History {
            get { return history; }
            set { history = (List<Node>)value; }
        }
#endif
        /// <summary>
        /// Creates a new node and returns it or returns the old node.
        /// If the node label is not set the id is used as the label.
        /// </summary>
        /// <param name="nodeId">is a key to the node in the Node's table</param>
        /// <returns></returns>
        public Node AddNode(string nodeId) {
            Node ret = nodeMap[nodeId] as Node;
            if (ret == null) {
                ret = new Node(nodeId);
                nodeMap[nodeId] = ret;
#if DEBUGGLEE
                history.Add(ret);
#endif
            }
            return ret;
        }

        /// <summary>
        /// adds a node to the graph
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(Node node) {
            if(nodeMap.ContainsKey(node.Id))
                return;

            nodeMap[node.Id]=node;
        }

        /// <summary>
        /// Number of nodes in the graph without counting the subgraphs.
        /// </summary>
        public int NodeCount {
            get { return nodeMap.Count; }
        }


        /// <summary>
        /// A lookup function.
        /// </summary>
        /// <param name="edgeId"></param>
        /// <returns></returns>
        public Edge EdgeById(string edgeId) {
            if (this.idToEdges == null || this.idToEdges.Count == 0) {
                foreach (Edge e in Edges)
                    if (e.Attr.Id != null)
                        idToEdges[e.Attr.Id] = e;

            }

            return idToEdges[edgeId] as Edge;
        }

        /// <summary>
        /// The number of dges in the graph.
        /// </summary>
        public int EdgeCount {
            get { return Edges.Count; }
        }

        /// <summary>
        /// Removes an edge, if the edge doesn't exist then nothing happens.
        /// </summary>
        /// <param name="edge">edge reference</param>
        virtual public void RemoveEdge(Edge edge)
        {
            if (edge == null)
                return;
            if (!Edges.Contains(edge))
                return;
            Node source = edge.SourceNode;
            Node target = edge.TargetNode;
            if (source != target)
            {
                source.RemoveOutEdge(edge);
                target.RemoveInEdge(edge);
            }
            else
                source.RemoveSelfEdge(edge);
            Edges.Remove(edge);
			GeometryGraph.RemoveEdge( edge.GeometryObject as Msagl.Edge );
        }

        /// <summary>
        /// Removes a node and all of its edges. If the node doesn't exist, nothing happens.
        /// </summary>
        /// <param name="node">node reference</param>
        virtual public void RemoveNode(Node node)
        {
            if (node == null || !NodeMap.ContainsKey(node.Id))
                return;
            ArrayList delendi = new ArrayList();
            foreach (Edge e in node.InEdges)
                delendi.Add(e);
            foreach (Edge e in node.OutEdges)
                delendi.Add(e);
            foreach (Edge e in node.SelfEdges)
                delendi.Add(e);
            foreach (Edge e in delendi)
                RemoveEdge(e);
            NodeMap.Remove(node.Id);
			GeometryGraph.RemoveNode( node.GeometryObject as Msagl.Node );
        }

        /// <summary>
        /// Always adds a new edge,if head or tail nodes don't exist they will be created
        /// </summary>
        /// <param name="source">source node id</param>
        /// <param name="edgeLabel">edge labe - can be null</param>
        /// <param name="target">target node id</param>
        /// <returns>Edge</returns>
        virtual public Edge AddEdge(string source, string edgeLabel, string target) {
            string l = edgeLabel;
            if (l == null)
                l = "";
            Edge edge = new Edge(source, l, target);
            AddPrecalculatedEdge(edge);
            return edge;
        }
        /// <summary>
        /// adds and edge object
        /// </summary>
        /// <param name="edge"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Precalculated")]
        public void AddPrecalculatedEdge(Edge edge) {
            Edges.Add(edge);

            edge.SourceNode = AddNode(edge.Source);
            edge.TargetNode = AddNode(edge.Target);
            

            if (edge.Source != edge.Target) {
                edge.SourceNode.AddOutEdge(edge);
                edge.TargetNode.AddInEdge(edge);
            } else
                edge.SourceNode.AddSelfEdge(edge);
        }

        /// <summary>
        /// A lookup function: searching recursively in the subgraphs
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public Node FindNode(string nodeId) {
            return nodeMap[nodeId] as Node;
        }

        /// <summary>
        /// Always adds a new edge,if head or tail nodes don't exist they will be created
        /// </summary>
        /// <param name="source">the source node id</param>
        /// <param name="target">the target node id</param>
        /// <returns>edge</returns>

        public virtual Edge AddEdge(string source, string target) {

            return AddEdge(source, null, target);
        }


        /// <summary>
        /// Nodes having minimal layering
        /// </summary>
        IEnumerable minLayer;
/// <summary>
/// for future use
/// </summary>
        public IEnumerable MinLayer {
            get { return minLayer; }
            set { minLayer = value; }
        }

        /// <summary>
        /// Nodes to have maximal layering
        /// </summary>
        IEnumerable maxLayer;
/// <summary>
/// for future use
/// </summary>
        public IEnumerable MaxLayer {
            get { return maxLayer; }
            set { maxLayer = value; }
        }


        /// <summary>
        /// Nodes having the same layering: different calls generate different groups with the same layer
        /// </summary>
        /// <param name="nodeLabels">collection of strings - node labels</param>
        public void AddSameLayer(IEnumerable nodeLabels) {
            SameLayers.Add(nodeLabels);
        }


  

        /// <summary>
        /// Collections of same layers nodes
        /// </summary>
        ArrayList sameLayers = new ArrayList();
/// <summary>
/// for future use
/// </summary>
        public ArrayList SameLayers {
            get { return sameLayers; }
        }

        /// <summary>
        /// Very strangely, but layouts look not so good if I use Dictionary ovet string, Node
        /// </summary>
        internal Hashtable nodeMap = new Hashtable();
        /// <summary>
        /// labels -> nodes 
        /// </summary>
        public Hashtable NodeMap {
            get { return nodeMap; }

        }
#if DEBUGGLEE
        /// <summary>
        /// visible only in debug
        /// </summary>
        /// <param name="nodeM"></param>
        public void InitNodeMap(Hashtable nodeM) {
            this.nodeMap = nodeM;
        }
#endif

        List<Edge> edges = new List<Edge>();

        /// <summary>
        /// The list of edges
        /// </summary>
        public ICollection<Edge> Edges {
            get { return edges; }
        }

        
        Hashtable idToEdges = new Hashtable();
        //map from EdgeInfo to Edge

        
      
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        string id;
        /// <summary>
        /// Graph constructor
        /// </summary>
        /// <param name="label">graph label</param>
        /// <param name="id">graph id</param>
        public Graph(string label, string id) {
            this.id = id;
            this.Label = new Label();
            this.Label.Text = label;
            InitAttributes();
        }

      /// <summary>
      /// constructor
      /// </summary>
        public Graph():this("") {
            
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="labelPar"></param>
        public Graph(string labelPar) {
            this.Label = new Label();
            this.id = this.Label.Text = labelPar;
            InitAttributes();
        }

        private void InitAttributes() {
            this.attr = new GraphAttr();
       //     CreateSelectedNodeAttr();
       //     CreateSelectedEdgeAttr();
        }


      

        bool directed = true;
/// <summary>
/// true is the graph is directed
/// </summary>
        public bool Directed {
            get { return directed; }
            set { directed = value; }
        }

        internal GeometryGraph geomGraph;

        /// <summary>
        /// underlying graph with pure geometry info
        /// </summary>
        public GeometryGraph GeometryGraph {
            get {
                return geomGraph;// != null ? geomGraph : geomGraph = CreateLayoutGraph.Create(this);
            }
            set { geomGraph = value; }
        }
/// <summary>
/// Creates the corresponding geometry graph
/// </summary>
        virtual public void CreateGeometryGraph() {
            this.GeometryGraph = CreateLayoutGraph.Create(this);
        }


#if DEBUGGLEE
        /// <summary>
        ///  field used for debug purposes only 
        /// </summary>
        List<Microsoft.Msagl.Splines.LineSegment> debugLines;

        List<Color> debugColors;
/// <summary>
/// debug only
/// </summary>
        public List<Color> DebugColors {
            get { return debugColors; }
            set { debugColors = value; }
        }

        Dictionary<object, Color> colorDictionary = new Dictionary<object, Color>();
/// <summary>
/// debug only
/// </summary>
        public Dictionary<object, Color> ColorDictionary {
            get { return colorDictionary; }
            set { colorDictionary = value; }
        }
/// <summary>
/// debug only
/// </summary>
        public ICollection<Microsoft.Msagl.Splines.LineSegment> DebugLines {
            get { return debugLines; }
            set { debugLines = (List<Microsoft.Msagl.Splines.LineSegment>)value; }
        }
        /// <summary>
        ///  field used for debug purposes only 
        /// </summary>
        List<Microsoft.Msagl.Splines.Ellipse> debugEllipses;
/// <summary>
/// debug only
/// </summary>
        public ICollection<Microsoft.Msagl.Splines.Ellipse> DebugEllipses {
            get { return debugEllipses; }
            set { debugEllipses = (List<Microsoft.Msagl.Splines.Ellipse>)value; }
        }
        /// <summary>
        ///  field used for debug purposes only 
        /// </summary>
        List<Microsoft.Msagl.Splines.CubicBezierSegment> debugBezierCurves;
/// <summary>
/// debug only
/// </summary>
        public ICollection<Microsoft.Msagl.Splines.CubicBezierSegment> DebugBezierCurves {
            get { return debugBezierCurves; }
            set { debugBezierCurves = (List<Microsoft.Msagl.Splines.CubicBezierSegment>)value; }
        }

/// <summary>
/// for debug only
/// </summary>
        public List<Microsoft.Msagl.Splines.Polyline> debugPolylines = new List<Polyline>();

        /// <summary>
        ///  field used for debug purposes only 
        /// </summary>
        List<Microsoft.Msagl.Splines.Parallelogram> debugBoxes;
/// <summary>
/// debug only
/// </summary>
        public ICollection<Microsoft.Msagl.Splines.Parallelogram> DebugBoxes {
            get { return debugBoxes; }
            set { debugBoxes = (List<Microsoft.Msagl.Splines.Parallelogram>)value; }
        }

        /// <summary>
        ///  field used for debug purposes only 
        /// </summary>
        bool showControlPoints;
/// <summary>
/// debug only
/// </summary>
        public bool ShowControlPoints {
            get { return showControlPoints; }
            set { showControlPoints = value; }
        }
#endif
/// <summary>
/// the geometry graph
/// </summary>
        public override GeometryObject GeometryObject {
            get { return this.GeometryGraph; }
        }

        const string fileExtension = ".msagl";

        /// <summary>
        /// Write the graph to a file
        /// </summary>
        /// <param name="fileName"></param>
        public void Write(string fileName) {
            if (fileName != null) {
                if (!fileName.EndsWith(fileExtension, true, new System.Globalization.CultureInfo("en-US")))
                    fileName += fileExtension;
                using (Stream stream = File.OpenWrite(fileName)) {
                    GraphWriter graphWriter = new GraphWriter(stream, this);
                    graphWriter.Write();
                }
            }
        }
        /// <summary>
        /// Reads the graph from a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Graph Read(string fileName) {
            using (Stream stream = File.OpenRead(fileName)) {
                GraphReader graphReader = new GraphReader(stream);
                return graphReader.Read();
            }

        }
    }
}
