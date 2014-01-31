 using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// reads a drawing graph from a stream
    /// </summary>
    public class GraphReader {
        Stream stream;
        Graph graph = new Graph();
        XmlReader xmlReader;

        internal GraphReader(Stream streamP) {
            stream = streamP;
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreWhitespace = true;
            readerSettings.IgnoreComments = true;
            xmlReader = XmlReader.Create(stream, readerSettings);
        }
        
        /// <summary>
        /// Reads the graph from a file
        /// </summary>
        /// <returns></returns>
        internal Graph Read() {
            System.Globalization.CultureInfo currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            try {
            ReadGraph();
            } finally { System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture; }
            return graph;
        }

        private void ReadGraph() {
            MoveToContent();
            CheckToken(Tokens.MsaglGraph);
            XmlRead();
            ReadAttr();
            ReadLabel(graph);
            ReadNodes();
            ReadEdges();
            ReadGeomGraph();
        }

        private void ReadLabel(DrawingObject parent) {
            CheckToken(Tokens.Label);
            bool hasLabel = !this.xmlReader.IsEmptyElement;
            if (hasLabel) {
                XmlRead();
                Label label = new Label();
                label.Text = ReadStringElement(Tokens.Text);
                label.FontName = ReadStringElement(Tokens.FontName);
                label.FontColor = ReadColorElement(Tokens.FontColor);
                label.FontSize = ReadIntElement(Tokens.FontSize);
                label.Width = ReadDoubleElement(Tokens.Width);
                label.Height = ReadDoubleElement(Tokens.Height);
                ((ILabeledObject)parent).Label = label;

                ReadEndElement();
            } else
                xmlReader.Skip();
        }

        private void ReadGeomGraph() {
            if (ReadBooleanElement(Tokens.GeometryGraphIsPresent)) {
                GeometryGraphReader ggr=new GeometryGraphReader();
                ggr.SetXmlReader(this.xmlReader);
                GeometryGraph geomGraph = ggr.Read();
                BindTheGraphs(this.graph, geomGraph);
            }
            

        }

        static void BindTheGraphs(Graph drawingGraph,GeometryGraph geomGraph) {
            drawingGraph.GeometryGraph = geomGraph;

            foreach (KeyValuePair<string, Microsoft.Msagl.Node> kv in geomGraph.NodeMap) {
                Microsoft.Msagl.Drawing.Node drawingNode = drawingGraph.NodeMap[kv.Key] as Microsoft.Msagl.Drawing.Node;
                drawingNode.Attr.GeometryNode = kv.Value;
                if (drawingNode.Label != null)
                    drawingNode.Label.GeometryLabel = kv.Value.Label;
            }
            
            //geom edges have to appear in the same order as drawing edges
            IEnumerator<Edge> edgeEnum = drawingGraph.Edges.GetEnumerator();
            IEnumerator<Microsoft.Msagl.Edge> geomEdgeEnum = geomGraph.Edges.GetEnumerator();
            while (edgeEnum.MoveNext()) {
                geomEdgeEnum.MoveNext();
                edgeEnum.Current.Attr.GeometryEdge = geomEdgeEnum.Current;
                if (edgeEnum.Current.Label != null)
                    edgeEnum.Current.Label.GeometryLabel = geomEdgeEnum.Current.Label;
            }

            drawingGraph.LayoutAlgorithmSettings = geomGraph.LayoutAlgorithmSettings;
        }

        private void ReadEdges() {
            CheckToken(Tokens.Edges);
            XmlRead();
            while (TokenIs(Tokens.Edge))
                ReadEdge();
            ReadEndElement();
        }

        private void ReadEdge() {
            CheckToken(Tokens.Edge);
            XmlRead();
            Edge edge=graph.AddEdge(ReadStringElement(Tokens.SourceNodeID), ReadStringElement(Tokens.TargetNodeID));
            edge.Attr = new EdgeAttr();
            ReadEdgeAttr(edge.Attr);
            ReadLabel(edge);
            ReadEndElement();
        }

        private void ReadEdgeAttr(EdgeAttr edgeAttr) {
            CheckToken(Tokens.EdgeAttribute);
            XmlRead();
            ReadBaseAttr(edgeAttr);
            edgeAttr.Separation = ReadIntElement(Tokens.EdgeSeparation);
            edgeAttr.Weight = ReadIntElement(Tokens.Weight);
            edgeAttr.ArrowheadAtSource = (ArrowStyle)Enum.Parse(typeof(ArrowStyle), ReadStringElement(Tokens.ArrowStyle));
            edgeAttr.ArrowheadAtTarget = (ArrowStyle)Enum.Parse(typeof(ArrowStyle), ReadStringElement(Tokens.ArrowStyle));
            edgeAttr.ArrowheadLength =(float) ReadDoubleElement(Tokens.ArrowheadLength);
            ReadEndElement();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "token")]
        private int ReadIntElement(Tokens token) {
            CheckToken(token);
            return ReadElementContentAsInt();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "token")]
        private string ReadStringElement(Tokens token) {
            CheckToken(token);
            return ReadElementContentAsString();
        }


        private int ReadElementContentAsInt() {
            return xmlReader.ReadElementContentAsInt();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "token")]
        private bool ReadBooleanElement(Tokens token) {
            CheckToken(token);
            return ReadElementContentAsBoolean();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "token")]
        private double ReadDoubleElement(Tokens token) {
            CheckToken(token);
            return ReadElementContentAsDouble();
        }

        private void ReadNodes() {
            CheckToken(Tokens.Nodes);
            XmlRead();
            while (TokenIs(Tokens.Node))
                ReadNode();
            ReadEndElement();
        }

        private void ReadNode() {
            CheckToken(Tokens.Node);
            XmlRead();
            NodeAttr nodeAttr = new NodeAttr();
            ReadNodeAttr(nodeAttr);
            Node node = graph.AddNode(nodeAttr.Id);
            node.Attr = nodeAttr;
            ReadLabel(node);      
            ReadEndElement();
        }

        private void ReadNodeAttr(NodeAttr na) {
            CheckToken(Tokens.NodeAttribute); 
            XmlRead();
            ReadBaseAttr(na as AttributeBase);
            na.FillColor = ReadColorElement(Tokens.Fillcolor);
            na.LabelMargin=ReadIntElement(Tokens.LabelMargin);
            na.Padding=ReadDoubleElement(Tokens.Padding);
            na.Shape = (Shape) Enum.Parse(typeof(Shape), ReadStringElement(Tokens.Shape));
            na.XRadius=ReadDoubleElement(Tokens.XRad);
            na.YRadius=ReadDoubleElement(Tokens.YRad);
            ReadEndElement();
       
        }

        private void ReadBaseAttr(AttributeBase baseAttr) {
            CheckToken(Tokens.BaseAttr);
            XmlRead();
            ReadStyles(baseAttr);
            baseAttr.Color = ReadColorElement(Tokens.Color);
            baseAttr.LineWidth = ReadIntElement(Tokens.LineWidth);
            baseAttr.Id = ReadStringElement(Tokens.ID);
            ReadEndElement();
        }

        private void ReadStyles(AttributeBase baseAttr) {
            CheckToken(Tokens.Styles);
            XmlRead();
            bool haveStyles = false;
            while (TokenIs(Tokens.Style)) {
                baseAttr.AddStyle((Style)Enum.Parse(typeof(Style), ReadStringElement(Tokens.Style)));
                haveStyles = true;
            }
            if (haveStyles)
                ReadEndElement();
        }

        private void ReadEndElement() {
            xmlReader.ReadEndElement();
        }

        private string ReadElementContentAsString() {
            return xmlReader.ReadElementContentAsString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)"), System.Diagnostics.Conditional("DEBUGGLEE")]
        private void CheckToken(Tokens t) {
            if (!xmlReader.IsStartElement(t.ToString())) {
                throw new InvalidDataException(String.Format("expecting {0}", t));
            }

        }

        private bool TokenIs(Tokens t) {
            return xmlReader.IsStartElement(t.ToString());
        }


        private void ReadAttr() {
            CheckToken(Tokens.GraphAttribute);
            XmlRead();
            ReadBaseAttr(graph.Attr);
            ReadMinNodeHeight();
            ReadMinNodeWidth();
            ReadAspectRatio();
            ReadBorder();
            graph.Attr.BackgroundColor = ReadColorElement(Tokens.BackgroundColor);
            graph.Attr.Margin = ReadDoubleElement(Tokens.Margin);
            graph.Attr.OptimizeLabelPositions = ReadBooleanElement(Tokens.OptimizeLabelPositions);
            graph.Attr.NodeSeparation = ReadDoubleElement(Tokens.NodeSeparation);
            graph.Attr.LayerDirection = (LayerDirection)Enum.Parse(typeof(LayerDirection), ReadStringElement(Tokens.LayerDirection));
            graph.Attr.LayerSeparation = ReadDoubleElement(Tokens.LayerSeparation);
            ReadEndElement();
        }

        private void ReadBorder() {
            this.graph.Attr.Border = ReadIntElement(Tokens.Border);
        }

        private void ReadMinNodeWidth() {
            this.graph.Attr.MinNodeWidth = ReadDoubleElement(Tokens.MinNodeWidth);
        }

        private void ReadMinNodeHeight() {
            this.graph.Attr.MinNodeHeight = ReadDoubleElement(Tokens.MinNodeHeight);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "token")]
        Color ReadColorElement(Tokens token) {
            CheckToken(token);
            XmlRead();
            Color c = ReadColor();
            ReadEndElement();
            return c;
        }

        Color ReadColor() {
            CheckToken(Tokens.Color);
            XmlRead();
            Color c = new Color(ReadByte(Tokens.A), ReadByte(Tokens.R), ReadByte(Tokens.G), ReadByte(Tokens.B));
            ReadEndElement();
            return c;
        }

        private byte ReadByte(Tokens token) {
            return (byte)ReadIntElement(token);
        }

        private void ReadAspectRatio() {
            CheckToken(Tokens.AspectRatio);
            graph.Attr.AspectRatio = ReadElementContentAsDouble();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Convert.ToBoolean(System.String)")]
        private bool ReadElementContentAsBoolean() {
            return Convert.ToBoolean(xmlReader.ReadElementContentAsString());
        }

     
        private double ReadElementContentAsDouble() {
            return xmlReader.ReadElementContentAsDouble();
        }

        private void MoveToContent() {
            xmlReader.MoveToContent();
        }

        private void XmlRead() {
            xmlReader.Read();
        }
    }
}
