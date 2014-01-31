using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Msagl.Splines;
using GeomNode = Microsoft.Msagl.Node;
using GeomEdge = Microsoft.Msagl.Edge;
using GeomLabel = Microsoft.Msagl.Label;
namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// the editor of a graph layout
    /// </summary>
    public class GeometryGraphEditor {

        /// <summary>
        /// signals that there is a change in the undo/redo list
        /// There are four possibilities: Undo(Redo) becomes available (unavailable)
        /// </summary>
        public event EventHandler ChangeInUndoRedoList;


        UndoRedoActionsList undoRedoActionsList=new UndoRedoActionsList();

        /// <summary>
        /// return the current undo action
        /// </summary>
        public UndoRedoAction CurrentUndoAction {get{return undoRedoActionsList.currentUndo;}}

        /// <summary>
        /// return the current redo action
        /// </summary>
        public UndoRedoAction CurrentRedoAction { get { return undoRedoActionsList.currentRedo; } }

        bool graphBoundingBoxGetsExtended;
/// <summary>
/// Will be set to true if an entity was dragged out of the graph bounding box
/// </summary>
        public bool GraphBoundingBoxGetsExtended {
            get { return graphBoundingBoxGetsExtended; }
            internal set { graphBoundingBoxGetsExtended = value; }
        }

        GeometryGraph graph;

        /// <summary>
        /// Current graph under editing
        /// </summary>
        public GeometryGraph Graph {
            get { return graph; }
            set { 
                graph = value;
                this.Clear();
                RaiseChangeInUndoList();
            }
        }


        Set<GeometryObject> objectsToDrag = new Set<GeometryObject>();
        Set<GeomEdge> edgesDraggedForTheSource = new Set<GeomEdge>();
        Set<GeomEdge> edgesDraggedForTheTarget = new Set<GeomEdge>();

        GeomEdge editedEdge;

       
        /// <summary>
        /// The edge data of the edge selected for editing
        /// </summary>

        internal GeomEdge EditedEdge {
            get { return editedEdge; }
            set { editedEdge = value; }
        }

        static internal void DragNode(GeomNode node, Point delta, NodeRestoreData restoreData) {
            node.BoundaryCurve = restoreData.BoundaryCurve.Translate(delta);
            node.Center = restoreData.Center + delta;
        }

        static internal void DragLabel(GeomLabel label, Point delta, Point dragStartCenter) {
            label.Center = dragStartCenter + delta;
            GeomEdge edge = label.Parent as GeomEdge;
            if (edge != null) {
                CalculateAttachedSegmentEnd(label, edge);
                label.AttachmentSegmentStart = label.Center;
                if (!Curve.Close(label.AttachmentSegmentEnd, label.Center))
                    foreach (IntersectionInfo x in Curve.CurveCurveIntersect(CreateLabelBoundary(label),
                        new LineSegment(label.AttachmentSegmentEnd, label.Center), false)) {
                        label.AttachmentSegmentStart = x.IntersectionPoint;
                        break;
                    }
            }
        }

        static ICurve CreateLabelBoundary(GeomLabel label) {
            double w = label.Width / 2;
            double h = label.Height / 2;
            Curve curve = new Curve();
            Curve.AddLineSegment(curve, label.Center.X - w, label.Center.Y - h, label.Center.X - w, label.Center.Y + h);
            Curve.ContinueWithLineSegment(curve, label.Center.X + w, label.Center.Y + h);
            Curve.ContinueWithLineSegment(curve, label.Center.X + w, label.Center.Y - h);
            Curve.CloseCurve(curve);
            return curve;
        }

       static void CalculateAttachedSegmentEnd(GeomLabel label, GeomEdge edge) {
            label.AttachmentSegmentEnd = edge.Curve[edge.Curve.ClosestParameter(label.Center)];

       }
        /// <summary>
        /// drags elements by the delta
        /// </summary>
        /// <param name="delta"></param>
        public void Drag(Point delta) {
            GraphBoundingBoxGetsExtended = false;
            if (delta.X != 0 || delta.Y != 0) {
                if (EditedEdge == null) {
                    foreach (GeometryObject geomObj in objectsToDrag) {
                        GeomNode node = geomObj as GeomNode;
                        if (node != null)
                            DragNode(node,delta, CurrentUndoAction.GetRestoreData(node) as NodeRestoreData);
                        else {
                            GeomEdge edge = geomObj as GeomEdge;
                            if (edge != null)
                                TranslateEdge(edge, delta, CurrentUndoAction.GetRestoreData(edge) as EdgeRestoreData);
                            else {
                                GeomLabel label = geomObj as GeomLabel;
                                if (label != null)
                                    DragLabel(label, delta, (CurrentUndoAction.GetRestoreData(label) as LabelRestoreData).Center);
                                else
                                    throw new NotImplementedException();
                            }
                        }

                        UpdateGraphBoundingBoxWithCheck(geomObj);
                    }

                    DragEdgesWithSource(delta);
                    DragEdgesWithTarget(delta);


                } else if (EditedEdge != null) {
                    DragEdgeEdit(delta);
                    UpdateGraphBoundingBoxWithCheck(EditedEdge);
                }
                                
            }
        }

        private void DragEdgesWithTarget(Point delta) {
            foreach (GeomEdge edge in this.edgesDraggedForTheTarget) {
                EdgeRestoreData edgeRestoreData = CurrentUndoAction.GetRestoreData(edge) as EdgeRestoreData;
                DragEdgeWithTarget(delta, edge, edgeRestoreData);
                UpdateGraphBoundingBoxWithCheck(edge);
            }
        }

        private void DragEdgesWithSource(Point delta) {
            foreach (GeomEdge edge in this.edgesDraggedForTheSource) {
                EdgeRestoreData edgeRestoreData = CurrentUndoAction.GetRestoreData(edge) as EdgeRestoreData;
                DragEdgeWithSource(delta, edge, edgeRestoreData);
                UpdateGraphBoundingBoxWithCheck(edge);
            }
        }

        static internal void DragEdgeWithSource(Point delta, GeomEdge edge, EdgeRestoreData edgeRestoreData) {
            double closenessToLine = Math.Min(1.0, (delta.Length / edgeRestoreData.DistanceWhenEdgeIsTurningToLine));

            Point lineStart = edgeRestoreData.Polyline.LastSite.Point;
            Point lineEnd = edgeRestoreData.Polyline.HeadSite.Point + delta;
            
            edge.UnderlyingPolyline.HeadSite.Point=lineEnd;

            Point lineVectorBetweenToPolylinePoints = (lineEnd - lineStart)/edgeRestoreData.NumberOfPolylineSegments;

            Site liveSite = edge.UnderlyingPolyline.LastSite;
            Site restoreSite = edgeRestoreData.Polyline.LastSite;
            
            for (int i = 1; i < edgeRestoreData.NumberOfPolylineSegments; i++) {
                Point linePoint=lineStart+ ((double)i)*lineVectorBetweenToPolylinePoints;
                liveSite = liveSite.Previous;
                restoreSite = restoreSite.Previous;
                liveSite.Point = closenessToLine * linePoint + (1 - closenessToLine) * restoreSite.Point;
            }
            
            CreateCurveOnChangedPolyline(delta, edge, edgeRestoreData);
        }

        static internal void DragEdgeWithTarget(Point delta, GeomEdge edge, EdgeRestoreData edgeRestoreData) {
            
            double closenessToLine = Math.Min(1.0, (delta.Length / edgeRestoreData.DistanceWhenEdgeIsTurningToLine));

            Point lineStart = edgeRestoreData.Polyline.HeadSite.Point;
            Point lineEnd = edgeRestoreData.Polyline.LastSite.Point + delta;

            edge.UnderlyingPolyline.LastSite.Point = lineEnd;

            Point lineVectorBetweenToPolylinePoints = (lineEnd - lineStart) / edgeRestoreData.NumberOfPolylineSegments;

            Site liveSite = edge.UnderlyingPolyline.HeadSite;
            Site restoreSite = edgeRestoreData.Polyline.HeadSite;

            for (int i = 1; i < edgeRestoreData.NumberOfPolylineSegments; i++) {
                Point linePoint = lineStart + ((double)i) * lineVectorBetweenToPolylinePoints;
                liveSite = liveSite.Next;
                restoreSite = restoreSite.Next;
                liveSite.Point = closenessToLine * linePoint + (1 - closenessToLine) * restoreSite.Point;
            }

            CreateCurveOnChangedPolyline(delta, edge, edgeRestoreData);
        }


        private void DragEdgeEdit(Point delta) {
            EdgeRestoreData edgeRestoreData = CurrentUndoAction.GetRestoreData(this.EditedEdge) as EdgeRestoreData;

            DragEdgeWithSite(delta, EditedEdge, edgeRestoreData, edgeRestoreData.Site,
                             edgeRestoreData.InitialSitePosition);
        }

       

     
        static internal void DragEdge(Point delta, GeomEdge e, EdgeRestoreData edgeRestoreData, Set<GeometryObject> objectsMarkedToDrag) {
            Site site = null;

            Point point=new Point(0,0);
            if (objectsMarkedToDrag.Contains(e.Source)) {
                if (!objectsMarkedToDrag.Contains(e.Target)) {
                    site = e.UnderlyingPolyline.HeadSite;
                    point = edgeRestoreData.Polyline.HeadSite.Point;
                }
            } else {
                site = e.UnderlyingPolyline.LastSite;
                point = edgeRestoreData.Polyline.LastSite.Point;
            }

            if (site == null)
                TranslateEdge(e, delta, edgeRestoreData);
            else 
                DragEdgeWithSite(delta, e, edgeRestoreData, site, point );
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="delta">delta of the drag</param>
        /// <param name="e">the modified edge</param>
        /// <param name="edgeRestoreData"></param>
        /// <param name="site"></param>
        /// <param name="previousSitePosition"></param>
        static internal void DragEdgeWithSite(Point delta, GeomEdge e, EdgeRestoreData edgeRestoreData, Site site, Point previousSitePosition) {
            site.Point = previousSitePosition + delta;
            CreateCurveOnChangedPolyline(delta, e, edgeRestoreData);
        }

        private static void CreateCurveOnChangedPolyline(Point delta, GeomEdge e, EdgeRestoreData edgeRestoreData) {
            Curve curve = e.UnderlyingPolyline.CreateCurve();
            if (!Curve.TrimSplineAndCalculateArrowheads(e, curve, false))
                Curve.CreateBigEnoughSpline(e);
            if (delta.X != 0 || delta.Y != 0)
                DragLabelOfTheEdge(e, edgeRestoreData, curve);
        }
    

        static private void DragLabelOfTheEdge(GeomEdge e ,EdgeRestoreData edgeRestoreData, Curve untrimmedCurve) {
            if (e.Label != null) {
                e.Label.Center = untrimmedCurve[edgeRestoreData.LabelAttachmentParameter] + 
                    edgeRestoreData.LabelOffsetFromTheAttachmentPoint;                
            }
        }
///// <summary>
///// Creates a curve by using the underlying polyline
///// </summary>
///// <param name="underlyingPoly"></param>
///// <returns></returns>
//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Polyline")]
//        static public Curve CreateCurveFromEdgeUnderlyingPolyline(UnderlyingPolyline underlyingPoly) {
//            Curve curve = new Curve();
//            Site a = underlyingPoly.HeadSite;//the corner start
//            Site b; //the corner origin
//            Site c;//the corner other end

//            while (SmoothedPolyline.FindCorner(a, out b, out c)) {
//                CubicBezierSegment seg = SmoothedPolyline.CreateBezierSeg(b.bezierSegmentShrinkCoefficient, a, b, c);
//                if (curve.Segments.Count == 0) {
//                    if (!Curve.Close(a.Point, seg.Start))
//                        Curve.AddLineSeg(curve, a.Point, seg.Start);
//                    } else if (!Curve.Close(curve.End, seg.Start))
//                    Routing.ContinueWithLineSeg(curve, seg.Start);
//                curve.AddSegment(seg);
//                a = b;
//            }

//            System.Diagnostics.Debug.Assert(a.Next.Next == null);

//            if (curve.Segments.Count == 0) {
//                if (!Curve.Close(a.Point, a.Next.Point)) {
//                    Curve.AddLineSeg(curve, a.Point, a.Next.Point);
//                } else {
//                    double w=5;
//                    curve.Segments.Add(new CubicBezierSegment(a.Point, a.Point+new Point(w,w),a.Point+new Point(-w,w), b.Point));
//                }
//            } else if (!Curve.Close(curve.End, a.Next.Point))
//                Routing.ContinueWithLineSeg(curve, a.Next.Point);

//            return curve;
//        }

        static private void TranslateEdge(GeomEdge e, Point delta, EdgeRestoreData edgeRestoreData) {
            e.Curve = edgeRestoreData.Curve.Translate(delta);
            if (e.UnderlyingPolyline != null)
                for (Site s = e.UnderlyingPolyline.HeadSite, s0 = edgeRestoreData.Polyline.HeadSite; s != null; s = s.Next, s0 = s0.Next)
                    s.Point = s0.Point + delta;

            if (e.ArrowheadAtSource)
                e.ArrowheadAtSourcePosition = edgeRestoreData.ArrowheadAtSourcePosition + delta;
            if (e.ArrowheadAtTarget)
                e.ArrowheadAtTargetPosition = edgeRestoreData.ArrowheadAtTargetPosition + delta;

            if (e.Label != null)
                e.Label.Center = edgeRestoreData.LabelCenter + delta;

        }

      
        /// <summary>
        /// enumerates over the nodes chosen for dragging
        /// </summary>
        public IEnumerable<GeometryObject> ObjectsToDrag { get { return this.objectsToDrag; } }
       /// <summary>
       /// returns true if "undo" is available
       /// </summary>
        public bool CanUndo { get { return undoRedoActionsList.currentUndo != null; } }
        /// <summary>
        /// returns true if "redo" is available
        /// </summary>
        public bool CanRedo { get { return undoRedoActionsList.currentRedo != null; } }
       
        ///// <summary>
        ///// preparation
        ///// </summary>
        //public void PrepareForEditing(object userData) {
        //    UndoRedoAction action=this.undoRedoActionsList.AddAction(CreateUndoRedoAction(userData));
        //    action.GraphBoundingBoxBefore = action.Graph.BoundingBox;
        //}

/// <summary>
/// prepares for node dragging
/// </summary>
/// <param name="affectedObjects">LayoutEditor just keeps these object for the client convenience</param>
/// <param name="markedObjects">markedObjects will be dragged</param>
/// <returns></returns>
        public UndoRedoAction PrepareForObjectDragging(Set<object> affectedObjects, IEnumerable<GeometryObject> markedObjects) {
            this.EditedEdge = null;
            ClearDraggedSets();
            CalculateDragSetsAndSubscribeToLayoutChangedEvent(markedObjects);
            UndoRedoAction action = CreateEditUndoRedoAction(affectedObjects);
            return InsertToListAndFixTheBox(action);
        }

        private void ClearDraggedSets() {
            this.objectsToDrag.Clear();
            this.edgesDraggedForTheSource.Clear();
            this.edgesDraggedForTheTarget.Clear();
        }

        private void CalculateDragSetsAndSubscribeToLayoutChangedEvent(IEnumerable<GeometryObject> markedObjects) {
            foreach (GeometryObject geometryObject in markedObjects) {
                this.objectsToDrag.Insert(geometryObject);                
                GeomEdge edge = geometryObject as GeomEdge;
                if (edge != null) {
                    objectsToDrag.Insert(edge.Source);
                    objectsToDrag.Insert(edge.Target);
                }
            }
            CalculateDragSetsForEdges();
        }

        void UpdateGraphBoundingBoxWithCheck(GeometryObject geomObj) {
            Rectangle bBox = geomObj.BoundingBox;
            {
                GeomEdge edge = geomObj as GeomEdge;
                if(edge!=null && edge.Label!=null)
                    bBox.Add(edge.Label.BoundingBox);
            }
            Point p = new Point(-graph.Margins, graph.Margins);

            GraphBoundingBoxGetsExtended |= graph.ExtendBoundingBoxWithCheck(bBox.LeftTop + p) || graph.ExtendBoundingBoxWithCheck(bBox.RightBottom - p);

        }

        private void CalculateDragSetsForEdges() {
            foreach (GeometryObject geomObj in objectsToDrag.Clone()) {
                GeomNode node = geomObj as GeomNode;
                if (node != null)
                    AssignEdgesOfNodeToEdgeDragSets(node);    
            }
        }

        private void AssignEdgesOfNodeToEdgeDragSets(GeomNode node) {
            foreach (GeomEdge edge in node.SelfEdges)
                objectsToDrag.Insert(edge);
            foreach (GeomEdge edge in node.InEdges)
                if (objectsToDrag.Contains(edge.Source))
                    objectsToDrag.Insert(edge);
                else
                    this.edgesDraggedForTheTarget.Insert(edge);

            foreach (GeomEdge edge in node.OutEdges)
                if (objectsToDrag.Contains(edge.Target))
                    objectsToDrag.Insert(edge);
                else
                    this.edgesDraggedForTheSource.Insert(edge);


        }

        internal UndoRedoAction InsertToListAndFixTheBox(UndoRedoAction action) {
            this.undoRedoActionsList.AddAction(action);
            action.GraphBoundingBoxBefore = action.Graph.BoundingBox;
            RaiseChangeInUndoList();
            return action;
        }

        private void RaiseChangeInUndoList() {
            if (this.ChangeInUndoRedoList != null)
                ChangeInUndoRedoList(this, null);
        }

     /// <summary>
     /// preparing for an edge corner dragging
     /// </summary>
     /// <param name="affectedObjects"></param>
     /// <param name="geometryEdge"></param>
     /// <param name="site"></param>
     /// <returns></returns>
        public UndoRedoAction PrepareForEdgeCornerDragging(Set<object> affectedObjects,
            GeomEdge geometryEdge, Site site) {
            this.EditedEdge = geometryEdge;
            UndoRedoAction edgeDragUndoRedoAction = (EdgeDragUndoRedoAction)CreateEdgeEditUndoRedoAction(affectedObjects);
            EdgeRestoreData edgeRestoreDate = (EdgeRestoreData)edgeDragUndoRedoAction.GetRestoreData(geometryEdge);
            edgeRestoreDate.Site = site;
            return InsertToListAndFixTheBox(edgeDragUndoRedoAction);
        }

        /// <summary>
        /// prepares for the polyline corner removal
        /// </summary>
        /// <param name="affectedEdge"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Polyline")]
        public UndoRedoAction PrepareForPolylineCornerRemoval(object affectedEdge, Site site) {
            SiteRemoveUndoAction action = new SiteRemoveUndoAction(EditedEdge);
            action.RemovedSite = site;
            action.AffectedObjects = new Set<object>();
            action.AffectedObjects.Insert(affectedEdge);
            return InsertToListAndFixTheBox(action);
        }

       /// <summary>
       /// prepare for polyline corner insertion
       /// </summary>
       /// <param name="affectedObj">edited objects</param>
       /// <param name="site">the site to insert</param>
       /// <returns></returns>
        internal UndoRedoAction PrepareForPolylineCornerInsertion(object affectedObj, Site site) {
            SiteInsertUndoAction action = new SiteInsertUndoAction(EditedEdge);
            action.InsertedSite = site;
            action.AffectedObjects = new Set<object>();
            action.AffectedObjects.Insert(affectedObj);
            return InsertToListAndFixTheBox(action);

        }


        private UndoRedoAction CreateEdgeEditUndoRedoAction(Set<object> affectedObjs) {
            EdgeDragUndoRedoAction action = new EdgeDragUndoRedoAction(this.EditedEdge);
            action.AffectedObjects = affectedObjs;
            return action;
        }

        private UndoRedoAction CreateEditUndoRedoAction(Set<object> affectedObjs) {
            ObjectDragUndoRedoAction action = new ObjectDragUndoRedoAction(this.objectsToDrag);
            action.AffectedObjects=affectedObjs;
            foreach (GeometryObject msaglObject in this.ChangedElements()) 
                action.AddRestoreData(msaglObject, msaglObject.GetRestoreData());
    
            return action;
        }  

        private IEnumerable<GeometryObject> ChangedElements() {
            if (this.EditedEdge == null) {

                foreach (GeometryObject obj in ObjectsToDrag) {
                    GeomNode node = obj as GeomNode;
                    if (node != null) {
                        yield return node;
                        foreach (GeomEdge e in node.OutEdges)
                            yield return e;
                        foreach (GeomEdge e in node.SelfEdges)
                            yield return e;
                        foreach (GeomEdge e in node.InEdges)
                            yield return e;
                    } else {
                        GeomEdge edge = obj as GeomEdge;
                        if (edge != null) {
                            yield return edge;
                            yield return edge.Source;
                            foreach (GeomEdge e in edge.Source.Edges)
                                yield return e;
                            yield return edge.Target;
                            foreach (GeomEdge e in edge.Target.Edges)
                                yield return e;
                        } else {
                            GeomLabel label = obj as GeomLabel;
                            if (label != null)
                                yield return label;
                            else
                                throw new NotImplementedException();
                        }
                    }
                }
            } else
                yield return this.EditedEdge;
        }

        /// <summary>
        /// Undoes the last editing. 
        /// </summary>
        public void Undo() {
            if (CanUndo) {
                undoRedoActionsList.currentUndo.Undo();
                undoRedoActionsList.currentRedo = undoRedoActionsList.currentUndo;
                undoRedoActionsList.currentUndo = undoRedoActionsList.currentUndo.Previous;
                RaiseChangeInUndoList();
            }
        }

        /// <summary>
        /// redo the dragging
        /// </summary>
        public void Redo() {  
            if (CanRedo) {
                undoRedoActionsList.currentRedo.Redo();
                undoRedoActionsList.currentUndo = undoRedoActionsList.currentRedo;
                undoRedoActionsList.currentRedo = undoRedoActionsList.currentRedo.Next;
                RaiseChangeInUndoList();
            }
           
        }

        /// <summary>
        /// clear the editor
        /// </summary>
        public void Clear() {
            this.objectsToDrag = new Set<GeometryObject>();
            this.edgesDraggedForTheSource.Clear();
            this.edgesDraggedForTheTarget.Clear();
            this.undoRedoActionsList = new UndoRedoActionsList();
            this.EditedEdge = null;
        }

        /// <summary>
        /// gets the enumerator pointing to the polyline corner before the point
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        static public Site GetPreviousSite(GeomEdge edge, Point point) {
            Site prevSite = edge.UnderlyingPolyline.HeadSite;
            Site nextSite = prevSite.Next;
            do {
                if (BetweenSites(prevSite, nextSite, point))
                    return prevSite;
                prevSite = nextSite;
                nextSite = nextSite.Next;
                
            } 
            while( nextSite!=null);
            return null;
        }

        static private bool BetweenSites(Site prevSite, Site nextSite, Point point) {
            double par = Point.ClosestParameterOnLineSegment(point, prevSite.Point, nextSite.Point);
            return par > 0.1 && par < 0.9;
        }

        /// <summary>
        /// insert a polyline corner
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="point">the point to insert the corner</param>
        /// <param name="siteBeforeInsertion"></param>
        ///<param name="affectedEntity">an object to be stored in the undo action</param>
        public void InsertSite(GeomEdge edge, Point point, Site siteBeforeInsertion, object affectedEntity) {
            this.EditedEdge = edge;
            //creating the new site
            Site first = siteBeforeInsertion;
            Site second = first.Next;
            Site s=new Site(first, point, second);
            PrepareForPolylineCornerInsertion(affectedEntity, s);
            
            //just to recalc everything in a correct way
            DragEdgeWithSite(new Point(0, 0), edge, CurrentUndoAction.GetRestoreData(edge) as EdgeRestoreData, s, point);
        }

        /// <summary>
        /// deletes the polyline corner
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="site"></param>
        /// <param name="userData">an object to be stored in the unde action</param>
        public void DeleteSite(GeomEdge edge,  Site site, object userData) {
            this.EditedEdge = edge;
            PrepareForPolylineCornerRemoval(userData, site);
            site.Previous.Next=site.Next;//removing the site from the list
            site.Next.Previous=site.Previous;
            //just to recalc everything in a correct way
            DragEdgeWithSite(new Point(0, 0), edge, CurrentUndoAction.GetRestoreData(edge) as EdgeRestoreData, site.Previous, site.Previous.Point);
        }

        /// <summary>
        /// finds the polyline corner near the mouse position
        /// </summary>
        /// <param name="underlyingPolyline"></param>
        /// <param name="mousePoint"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Polyline")]
        static public Site FindCornerForEdit(UnderlyingPolyline underlyingPolyline, Point mousePoint, double tolerance) {
            Site site = underlyingPolyline.HeadSite.Next;
            tolerance *= tolerance; //square the tolerance

            do {
                if(site.Previous==null || site.Next==null)
                    continue; //don't return the first and the last corners
                Point diff = mousePoint - site.Point;
                if (diff * diff <= tolerance)
                    return site;

                site = site.Next;
            }
            while (site.Next != null) ;
            return null;
        }

        /// <summary>
        /// creates a "tight" bounding box
        /// </summary>
        /// <param name="affectedEntity">the object corresponding to the graph</param>
        /// <param name="geometryGraph"></param>
        public void FitGraphBoundingBox(object affectedEntity, GeometryGraph geometryGraph) {
            if (geometryGraph != null) {
                UndoRedoAction uAction=new UndoRedoAction(geometryGraph);
                uAction.Graph=geometryGraph;
                undoRedoActionsList.AddAction(uAction);
                Rectangle r = new Rectangle();
                foreach (GeomNode n in geometryGraph.NodeMap.Values) { r = n.BoundingBox; break; }
                foreach (GeomNode n in geometryGraph.NodeMap.Values) { r.Add(n.BoundingBox); }
                foreach (GeomEdge e in geometryGraph.Edges) {
                    r.Add(e.BoundingBox);
                    if (e.Label != null)
                        r.Add(e.Label.BoundingBox);
                }


                r.Left -= geometryGraph.Margins;
                r.Top += geometryGraph.Margins;
                r.Bottom -= geometryGraph.Margins;
                r.Right += geometryGraph.Margins;
                uAction.AffectedObjects = new Set<object>();
                uAction.AffectedObjects.Insert(affectedEntity);
                uAction.GraphBoundingBoxAfter=geometryGraph.BoundingBox = r;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delta"></param>
        public void OnDragEnd(Point delta) {
            UndoRedoDragAction action = (UndoRedoDragAction)CurrentUndoAction;
            action.Delta = delta;
            action.GraphBoundingBoxAfter = action.Graph.BoundingBox;
        }
    }
}
