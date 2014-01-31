using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl;
using DrawingGraph=Microsoft.Msagl.Drawing.Graph;
using GeometryEdge=Microsoft.Msagl.Edge;
using GeometryNode=Microsoft.Msagl.Node;
using DrawingEdge=Microsoft.Msagl.Drawing.Edge;
using DrawingNode=Microsoft.Msagl.Drawing.Node;
using System.Windows.Forms;
using P2 = Microsoft.Msagl.Point;

namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// This yet another graph is needed to hold additional GDI specific data for drawing.
    /// It has a pointer to Microsoft.Msagl.Drawing.Graph and Microsoft.Msagl.Graph.
    /// It is passed to the drawing routine
    /// </summary>
    sealed internal class DGraph : DObject,  IViewerGraph {
    
        override public DrawingObject DrawingObject {
            get { return this.DrawingGraph; }
        }

        bool EdgeLabelsAreRendered {
            get { return this.DrawingGraph.LayoutAlgorithmSettings is SugiyamaLayoutSettings; }
        }

        List<DEdge> edges = new List<DEdge>();

        public List<DEdge> Edges {
            get { return edges; }
        }
        Dictionary<string, DNode> nodeMap = new Dictionary<string, DNode>();

        public Dictionary<string, DNode> NodeMap {
            get { return nodeMap; }
        }

        internal BBNode BBNode {
            get{
                if(BbNode==null)
                    BuildBBHierarchy();
                return BbNode;
            }
        }

        void AddNode(DNode dNode){
            nodeMap[dNode.DrawingNode.Id]=dNode;           
        }

        void AddEdge(DEdge dEdge){
            edges.Add(dEdge);
        }

        internal void UpdateBBoxHierarchy(IEnumerable<IViewerObject>  movedObjects){

            Set<DObject> changedObjects=new Set<DObject>();
            foreach (DObject dObj in movedObjects)
                foreach (DObject relatedObj in RelatedObjs(dObj))
                    changedObjects.Insert(relatedObj);

            
            foreach(DObject dObj in changedObjects){
                RebuildBBHierarchyUnderObject(dObj);
                InvalidateBBNodesAbove(dObj);
            }

            UpdateBoxes(BBNode);
        }


        static void RebuildBBHierarchyUnderObject(DObject dObj){
            BBNode oldNode=dObj.BbNode;
            BBNode newNode=BuildBBHierarchyUnderDObject(dObj);
            //now copy all fields, except the parent
            oldNode.l=newNode.l;
            oldNode.r=newNode.r;
            oldNode.geometry=newNode.geometry;
            oldNode.bBox=newNode.bBox;
        }

        static void InvalidateBBNodesAbove(DObject dObj){
            for(BBNode node=dObj.BbNode;node!=null;node=node.parent)
                node.bBox.Width=-1; //this will make the box empty
            
        }

        void UpdateBoxes(BBNode bbNode) {
            if (bbNode.Box.IsEmpty) {
                if (bbNode.geometry != null)
                    bbNode.bBox = bbNode.geometry.Box;
                else {
                    UpdateBoxes(bbNode.l);
                    UpdateBoxes(bbNode.r);
                    bbNode.bBox = bbNode.l.Box;
                    bbNode.bBox.Add(bbNode.r.Box);
                }
            }
        }
        
        IEnumerable<DObject> RelatedObjs(DObject dObj){
            yield return dObj;
            DNode dNode=dObj as DNode;
            if (dNode != null) {
                foreach (DEdge e in dNode.OutEdges)
                    yield return e;
                foreach (DEdge e in dNode.InEdges)
                    yield return e;
                foreach (DEdge e in dNode.SelfEdges)
                    yield return e;
            } else {
                DEdge dEdge = dObj as DEdge;
                if (dEdge != null) {
                    yield return dEdge.Source;
                    yield return dEdge.Target;
                    if (dEdge.Label != null)
                        yield return dEdge.Label;
                }
            }
        }
           
        internal void BuildBBHierarchy(){
            List<ObjectWithBox> objectsWithBox=new List<ObjectWithBox>();
 
            foreach(DObject dObject in Entities){
                dObject.BbNode=BuildBBHierarchyUnderDObject(dObject);
                objectsWithBox.Add(dObject);
            }

            BbNode=SpatialAlgorithm.CreateBBNodeOnGeometries(objectsWithBox);
            
        }


        internal IEnumerable<Microsoft.Msagl.Drawing.IViewerObject> Entities {
            get {
                foreach (DEdge dEdge in this.Edges) {
                    yield return dEdge;
                    if (dEdge.Label != null && this.drawingGraph.LayoutAlgorithmSettings is SugiyamaLayoutSettings)
                        yield return dEdge.Label;
                }

                foreach (DNode dNode in this.NodeMap.Values)
                    yield return dNode;
            }
        }
         
        static BBNode BuildBBHierarchyUnderDObject(DObject dObject){
            DNode dNode=dObject as DNode;
            if(dNode!=null)
                return BuildBBHierarchyUnderDNode(dNode);
            DEdge dedge=dObject as DEdge;
            if (dedge != null) {
                return BuildBBHierarchyUnderDEdge(dedge);          
            }
            DLabel dLabel=dObject as DLabel;
            if (dLabel != null)
                return BuildBBHierarchyUnderDLabel(dLabel);

            DGraph dGraph = dObject as DGraph;
            if (dGraph != null) {
                dGraph.BbNode.bBox = dGraph.DrawingGraph.BoundingBox;
                return dGraph.BbNode;
            }

            throw new InvalidOperationException();
        }

        static BBNode BuildBBHierarchyUnderDLabel(DLabel dLabel) {
            BBNode bbNode = new BBNode();
            bbNode.geometry = new Geometry(dLabel);
            bbNode.bBox = dLabel.DrawingLabel.BoundingBox;
            return bbNode; 
        }

        static BBNode BuildBBHierarchyUnderDNode(DNode dNode){
            BBNode bbNode=new BBNode();
            bbNode.geometry=new Geometry(dNode);
            bbNode.bBox = dNode.DrawingNode.BoundingBox;
            return bbNode;
        }
        
        static BBNode BuildBBHierarchyUnderDEdge(DEdge dEdge){
            List<ObjectWithBox> geometries=Tessellator.TessellateCurve(dEdge, dEdge.MarkedForDragging?dEdge.RadiusOfPolylineCorner:0);
            return SpatialAlgorithm.CreateBBNodeOnGeometries(geometries);
        }

        internal override float DashSize() {
            return 0; //not implemented
        }

   //     internal Dictionary<DrawingObject, DObject> drawingObjectsToDObjects = new Dictionary<DrawingObject, DObject>();

   

        DrawingGraph drawingGraph;

        public DrawingGraph DrawingGraph {
            get { return drawingGraph; }
            set { drawingGraph = value; }
        }


        internal void DrawGraph(Graphics g) {
           
            #region drawing of database for debugging only

#if DEBUGGLEE
            Pen myPen = new Pen(System.Drawing.Color.Blue, (float)(1 / 1000.0));
            DrawingGraph dg = this.DrawingGraph;
            if (dg.DataBase != null) {
                Draw.DrawDataBase(g, myPen, dg);
            }

            bool debugDrawing = Draw.DrawDebugStuff(g, this, myPen);
            if (debugDrawing)
                return;

#endif
            #endregion

            if (this.drawingGraph.Attr.Border > 0) 
                DrawGraphBorder(this.drawingGraph.Attr.Border,g);
            
            bool renderEdgeLabels = this.EdgeLabelsAreRendered;
            
            //we need to draw the edited edges last
            DEdge dEdgeSelectedForEditing = null;

            foreach (DEdge dEdge in Edges)
                if (!dEdge.SelectedForEditing)
                    DrawEdge(g, dEdge, renderEdgeLabels);
                else //there must be no more than one edge selected for editing
                    dEdgeSelectedForEditing = dEdge;

            foreach (DNode dnode in nodeMap.Values)
                DrawNode(g, dnode);
     

            //draw the selected edge
            if(dEdgeSelectedForEditing!=null){
                    DrawEdge(g, dEdgeSelectedForEditing, renderEdgeLabels);
                    DrawUnderlyingPolyline(g, dEdgeSelectedForEditing);
                }
        }

        static void DrawUnderlyingPolyline(Graphics g, DEdge editedEdge) {
            Microsoft.Msagl.UnderlyingPolyline points = editedEdge.DrawingEdge.Attr.GeometryEdge.UnderlyingPolyline;
            Pen pen = new Pen(editedEdge.Color, editedEdge.DrawingEdge.Attr.LineWidth);
            IEnumerator<P2> en = points.GetEnumerator();
            en.MoveNext();
            PointF p = P2P(en.Current);
            while (en.MoveNext())
                g.DrawLine(pen, p, p = P2P(en.Current));

            foreach (P2 p2 in points)
                DrawCircleAroungPolylineCorner(g, p2, pen,editedEdge.RadiusOfPolylineCorner);
        }

        static void DrawCircleAroungPolylineCorner(Graphics g, P2 p, Pen pen, double radius) {
            g.DrawEllipse(pen, (float)(p.X - radius), (float)(p.Y - radius),
                (float)(2 * radius), (float)(2 * radius));
        }

    
        static PointF P2P(P2 p) {
            return new PointF((float)p.X, (float)p.Y); 
        }

        private void DrawGraphBorder(int borderWidth, Graphics graphics) {
            using (Pen myPen = new Pen(Draw.MsaglColorToDrawingColor(this.drawingGraph.Attr.Color), (float)borderWidth)) 
                graphics.DrawRectangle(myPen,
                    (float)drawingGraph.Left, 
                    (float)drawingGraph.Bottom,
                    (float)drawingGraph.Width, 
                    (float)drawingGraph.Height);
            
        }

//don't know what to do about the try-catch block
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        static internal void DrawEdge(Graphics graphics, DEdge dEdge, bool drawLabel) {
            DrawingEdge drawingEdge = dEdge.DrawingEdge;
            if (drawingEdge.Attr.GeometryEdge == null)
                return;

            if (dEdge.GraphicsPath == null)
                Draw.CreateGraphicsPath(dEdge);

            EdgeAttr attr = drawingEdge.Attr;

            using (Pen myPen = new Pen(dEdge.Color, attr.LineWidth)) {
                foreach (Style style in attr.Styles) {
                    Draw.AddStyleForPen(dEdge, myPen, style);
                }

                try {
                    graphics.DrawPath(myPen, dEdge.GraphicsPath);
                } catch {
                    //  sometimes on Vista it's just throws an out of memory exception without any obvious reason
                }
                Draw.DrawEdgeArrows(graphics, drawingEdge, dEdge.Color, myPen);
            }
            if (drawLabel)
                Draw.DrawLabel(graphics, dEdge.Label);
        }
    


        internal static void DrawNode(Graphics g, Microsoft.Msagl.GraphViewerGdi.DNode dnode) {

            DrawingNode node = dnode.DrawingNode;
            if (node.DrawNodeDelegate != null) 
                if (node.DrawNodeDelegate(node, g))
                    return; //the client draws instead
            
            if (node.Attr.GeometryNode == null)//node comes with non-initilalized attribute - should not be drawn
                return;
            NodeAttr attr = node.Attr;
         
            using (Pen pen = new Pen( dnode.Color, (float)attr.LineWidth)) {

                foreach (Style style in attr.Styles)
                    Draw.AddStyleForPen(dnode, pen, style);
                switch (attr.Shape) {
                    case Shape.DoubleCircle:
                        Draw.DrawDoubleCircle(g, pen, dnode);
                        break;
                    case Shape.Box:
                        Draw.DrawBox(g, pen, dnode);
                        break;
                    case Shape.Diamond:
                        Draw.DrawDiamond(g, pen, dnode);
                        break;
                    case Shape.Point:
                        Draw.DrawEllipse(g, pen, dnode);
                        break;
                    case Shape.Plaintext: {
                            break;
                            //do nothing
                        }
                    case Shape.Octagon:
                    case Shape.House:
                    case Shape.InvHouse:
                    case Shape.DrawFromGeometry:
#if DEBUG
                    //case Shape.TestShape:
#endif
                        pen.EndCap = System.Drawing.Drawing2D.LineCap.Square;
                        Draw.DrawFromMsaglCurve(g, pen, dnode);
                        break;
                        
                    default:
                        Draw.DrawEllipse(g, pen, dnode);
                        break;
                }
            }
            Draw.DrawLabel(g, dnode.Label);
        }

        internal DGraph(DrawingGraph drawingGraph) {
            this.DrawingGraph = drawingGraph;
        }

       /// <summary>
       /// creates DGraph from a precalculated drawing graph
       /// </summary>
       /// <param name="drawingGraph"></param>
       /// <returns></returns>
        internal static DGraph CreateDGraphFromPrecalculatedDrawingGraph(DrawingGraph drawingGraph) {
            DGraph ret = new DGraph(drawingGraph);
            //create dnodes and node boundary curves
            foreach (DrawingNode drawingNode in drawingGraph.NodeMap.Values) {
                DNode dNode = new DNode(drawingNode);
                if (drawingNode.Label != null)
                    dNode.Label = new DLabel(dNode, drawingNode.Label);
                ret.AddNode(dNode);
            }

            foreach (DrawingEdge drawingEdge in drawingGraph.Edges) 
                ret.AddEdge(new DEdge(ret.GetNode(drawingEdge.SourceNode), ret.GetNode(drawingEdge.TargetNode), drawingEdge, Connection.Connected));                
            
            return ret;
        }

        internal static void CreateDLabel(DObject parent, Microsoft.Msagl.Drawing.Label label, out double width, out double height) {
            DLabel dLabel = new DLabel(parent, label);
            dLabel.Font = new Font(label.FontName, label.FontSize);
            StringMeasure.MeasureWithFont(label.Text, dLabel.Font, out width, out height);
            label.Width = width;
            label.Height = height;
        }

        internal static DGraph CreateDGraphAndGeometryInfo(DrawingGraph drawingGraph, GeometryGraph gleeGraph) {
            DGraph ret = new DGraph(drawingGraph);
            //create dnodes and glee node boundary curves
            foreach (GeometryNode geomNode in gleeGraph.NodeMap.Values) {
                DrawingNode drawingNode = geomNode.UserData as DrawingNode;
                CreateDNodeAndSetNodeBoundaryCurve(drawingGraph, ret, geomNode, drawingNode);
            }

            foreach (GeometryEdge gleeEdge in gleeGraph.Edges) {
                DEdge dEdge = new DEdge(ret.GetNode(gleeEdge.Source), ret.GetNode(gleeEdge.Target), gleeEdge.UserData as DrawingEdge, Connection.Connected);
                    ret.AddEdge(dEdge);
                    DrawingEdge drawingEdge=dEdge.Edge;
                    Microsoft.Msagl.Drawing.Label label = drawingEdge.Label;
               
                    if (label!=null) {
                        double width, height;
                        CreateDLabel(dEdge, label, out width, out height);
                    }
            }
            
            return ret;
        }

        internal static DNode CreateDNodeAndSetNodeBoundaryCurve(DrawingGraph drawingGraph, DGraph dGraph, GeometryNode geomNode, DrawingNode drawingNode) {
            double width = 0;
            double height = 0;
            DNode dNode = new DNode(drawingNode);
            dGraph.AddNode(dNode);
            Microsoft.Msagl.Drawing.Label label = drawingNode.Label;
            if (label != null) {
                CreateDLabel(dNode, label, out width, out height);
                width += 2 * dNode.DrawingNode.Attr.LabelMargin;
                height += 2 * dNode.DrawingNode.Attr.LabelMargin;
            }
            if (width < drawingGraph.Attr.MinNodeWidth)
                width = drawingGraph.Attr.MinNodeWidth;
            if (height < drawingGraph.Attr.MinNodeHeight)
                height = drawingGraph.Attr.MinNodeHeight;

            // Filippo Polo: I'm taking this out because I've modified the drawing of a double circle
            // so that it can be used with ellipses too.
            //if (drawingNode.Attr.Shape == Shape.DoubleCircle)
            //width = height = Math.Max(width, height) * Draw.DoubleCircleOffsetRatio;
            if (geomNode.BoundaryCurve == null)
                geomNode.BoundaryCurve = Microsoft.Msagl.Drawing.NodeBoundaryCurves.GetNodeBoundaryCurve(dNode.DrawingNode, width, height);
            return dNode;
        }


        
        
        private DNode GetNode(Node node) {
            return nodeMap[node.Id];
        }

        private DNode GetNode(DrawingNode node) {
            return nodeMap[node.Id];
        }


        
    }
}
