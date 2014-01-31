using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace Microsoft.Msagl.Drawing {
    internal class GraphWriter {

        Stream stream;

        XmlWriter xmlWriter;

        public XmlWriter XmlWriter {
            get { return xmlWriter; }
        }
        Graph graph;

        public GraphWriter(Stream streamPar, Graph graphP) {
            this.stream = streamPar;
            this.graph = graphP;
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriter = XmlWriter.Create(stream, xmlWriterSettings);
        }

        public GraphWriter() { }
        /// <summary>
        /// Writes the graph to a file
        /// </summary>
        public void Write() {
            System.Globalization.CultureInfo currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            try {
            Open();
            WriteGraphAttr(graph.Attr);
            WriteLabel(this.graph.Label);
            WriteNodes();
            WriteEdges();
            WriteGeometryGraph();
            Close();
            } 
            finally{
                //restore the culture
                System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }

        private void WriteLabel(Label label) {
            WriteStartElement(Tokens.Label);
            if (label != null && !String.IsNullOrEmpty(label.Text)) {
                WriteStringElement(Tokens.Text, label.Text);
                WriteStringElement(Tokens.FontName, label.FontName);
                WriteColorElement(Tokens.FontColor, label.FontColor);
                WriteStringElement(Tokens.FontSize,label.FontSize);
                WriteStringElement(Tokens.Width, label.Width);
                WriteStringElement(Tokens.Height, label.Height);
            }
            WriteEndElement();
        }

        private void WriteGraphAttr(GraphAttr graphAttr) {
            this.WriteStartElement(Tokens.GraphAttribute);
            WriteBaseAttr(graphAttr);
            this.WriteMinNodeHeight();
            this.WriteMinNodeWidth();
            this.WriteAspectRatio();
            this.WriteBorder();
            WriteColorElement(Tokens.BackgroundColor, graphAttr.BackgroundColor);
            WriteStringElement(Tokens.Margin, graphAttr.Margin);
            WriteStringElement(Tokens.OptimizeLabelPositions, graphAttr.OptimizeLabelPositions);
            WriteStringElement(Tokens.NodeSeparation, graphAttr.NodeSeparation);
            WriteStringElement(Tokens.LayerDirection, graphAttr.LayerDirection);
            WriteStringElement(Tokens.LayerSeparation, graphAttr.LayerSeparation);
            this.WriteEndElement();
        }

        private void WriteBorder() {
            WriteStringElement(Tokens.Border, this.graph.Attr.Border);
        }

        private void WriteMinNodeWidth() {
            WriteStringElement(Tokens.MinNodeWidth, this.graph.Attr.MinNodeWidth);
        }

        private void WriteMinNodeHeight() {
            WriteStringElement(Tokens.MinNodeHeight, this.graph.Attr.MinNodeHeight);
        }

    
        private Color WriteColorElement(Tokens t, Color c) {
            WriteStartElement(t);
            WriteColor(c);
            WriteEndElement();
            return c;
        }

        private void WriteColor(Color color) {
            WriteStartElement(Tokens.Color);
            WriteStringElement(Tokens.A, color.A);
            WriteStringElement(Tokens.R, color.R);
            WriteStringElement(Tokens.G, color.G);
            WriteStringElement(Tokens.B, color.B);
            WriteEndElement(); 
        }

        private void WriteGeometryGraph() {
            if (this.graph.geomGraph != null && this.graph.BoundingBox.Width > 0) {
                WriteStringElement(Tokens.GeometryGraphIsPresent, true);
                GeometryGraphWriter ggw = new GeometryGraphWriter();
                ggw.XmlWriter = XmlWriter;
                ggw.Stream = stream;
                ggw.Graph = this.graph.GeometryGraph;
                ggw.NeedToCloseXmlWriter = false;
                ggw.Write();
            } else
                WriteStringElement(Tokens.GeometryGraphIsPresent, false);
        }
       
        private void Open() {
            xmlWriter.WriteStartElement(Tokens.MsaglGraph.ToString());
        }

        private void Close() {
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
            xmlWriter.Close();
        }

        private void WriteEdges() {
            WriteStartElement(Tokens.Edges);
            foreach (Edge edge in graph.Edges)
                WriteEdge(edge);
            WriteEndElement();
        }

        private void WriteEdge(Edge edge) {
            WriteStartElement(Tokens.Edge);
            WriteStringElement(Tokens.SourceNodeID, edge.Source);
            WriteStringElement(Tokens.TargetNodeID, edge.Target);
            WriteEdgeAttr(edge.Attr);
            WriteLabel(edge.Label);
            WriteEndElement();
        }

        private void WriteEdgeAttr(EdgeAttr edgeAttr) {
            WriteStartElement(Tokens.EdgeAttribute);
            WriteBaseAttr(edgeAttr);
            WriteStringElement(Tokens.EdgeSeparation, edgeAttr.Separation);
            WriteStringElement(Tokens.Weight, edgeAttr.Weight);
            WriteStringElement(Tokens.ArrowStyle, edgeAttr.ArrowheadAtSource);
            WriteStringElement(Tokens.ArrowStyle, edgeAttr.ArrowheadAtTarget);
            WriteStringElement(Tokens.ArrowheadLength, edgeAttr.ArrowheadLength);
            WriteEndElement();
        }

     
        private void WriteNodes() {
            WriteStartElement(Tokens.Nodes);
            foreach (Node node in graph.NodeMap.Values)
                WriteNode(node);
            WriteEndElement();
        }

        private void WriteNode(Node node) {
            WriteStartElement(Tokens.Node);
            WriteNodeAttr(node.Attr);
            WriteLabel(node.Label);
            WriteEndElement();
        }

        private void WriteNodeAttr(NodeAttr na) {
            WriteStartElement(Tokens.NodeAttribute);
            WriteBaseAttr(na as AttributeBase);
            WriteColorElement(Tokens.Fillcolor, na.FillColor);
            WriteStringElement(Tokens.LabelMargin, na.LabelMargin);
            WriteStringElement(Tokens.Padding, na.Padding);
            WriteStringElement(Tokens.Shape, na.Shape);
            WriteStringElement(Tokens.XRad, na.XRadius);
            WriteStringElement(Tokens.YRad, na.YRadius);
            WriteEndElement();
        }

        private void WriteBaseAttr(AttributeBase baseAttr) {
            WriteStartElement(Tokens.BaseAttr);
            WriteStyles(baseAttr.Styles);
            WriteColorElement(Tokens.Color, baseAttr.Color);
            WriteStringElement(Tokens.LineWidth, baseAttr.LineWidth);
            WriteStringElement(Tokens.ID, baseAttr.Id==null?"":baseAttr.Id);
            WriteEndElement();
        }

        private void WriteStyles(IEnumerable<Style> styles) {
            WriteStartElement(Tokens.Styles);
            foreach (Style s in styles)
                WriteStringElement(Tokens.Style, s);
            WriteEndElement();
        }

        private void WriteAspectRatio() {
            WriteStringElement(Tokens.AspectRatio, graph.Attr.AspectRatio);
        }

        private void WriteEndElement() {
            xmlWriter.WriteEndElement();
        }

        private void WriteStartElement(Tokens t) {
            xmlWriter.WriteStartElement(t.ToString());
        }

        private void WriteStringElement(Tokens t, object s) {
            xmlWriter.WriteElementString(t.ToString(), s.ToString());
        }

    }
}
