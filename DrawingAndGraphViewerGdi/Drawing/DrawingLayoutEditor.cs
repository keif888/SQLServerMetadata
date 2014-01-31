using System;
using System.Collections.Generic;
using Point = Microsoft.Msagl.Point;
namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// The usage of this delegate is 
    /// a) when dragging is false
    /// to find out if a combination of mouse buttons and pressed 
    /// modifier keys signals that the current selected entity should be added 
    /// (removed) to (from) the dragging group
    /// b) if the dragging is true to find out if we are selecting objects with the rectangle 
    /// </summary>
    /// <param name="modifierKeys"></param>
    /// <param name="mouseButtons"></param>
    /// <param name="dragging"></param>
    /// <returns></returns>
    public delegate bool MouseAndKeysAnalyzer(ModifierKeys modifierKeys, MouseButtons mouseButtons, bool dragging);
    /// <summary>
    /// a delegate type with IDraggableNode as a parameter
    /// </summary>
    /// <param name="obj"></param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Draggable")]
    public delegate void DelegateForIDraggableObject(IViewerObject obj);
    /// <summary>
    /// a delegate type with IDraggableEdge as a parameter
    /// </summary>
    /// <param name="edge"></param>
    public delegate void DelegateForEdge(IViewerEdge edge);
    /// <summary>
    /// creates a new node to insert
    /// </summary>
    /// <returns></returns>
    public delegate Node NewNodeFactory();

    /// <summary>
    /// a delegate type with IDraggableLabel as a parameter
    /// </summary>
    /// <param name="edge"></param>
    public delegate void DelegateForEdgeLabel(IViewerObject edge);
    /// <summary>
    /// type of a polyline corner for insertion or deletion
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Polyline")]
    public enum PolylineCornerType {
        /// <summary>
        /// a corner to insert
        /// </summary>
        PreviousCornerForInsertion,
        /// <summary>
        /// a corner to delete
        /// </summary>
        CornerToDelete
    }
    /// <summary>
    /// A delegate with no parameters and void return type
    /// </summary>
    public delegate void DelegateReturningVoid();

    /// <summary>
    /// Provides  graph nodes dragging functionality at the moment
    /// </summary>
    public class DrawingLayoutEditor {

        Graph graph;

        /// <summary>
        /// current graph of under editin
        /// </summary>
        public Graph Graph {
            get { return graph; }
            set {
                graph = value;
                if (graph != null)
                    this.geomGraphEditor.Graph = graph.GeometryGraph;
            }
        }

        Dictionary<IViewerObject, DelegateReturningVoid> decoratorRemovalsDict = new Dictionary<IViewerObject, DelegateReturningVoid>();
        /// <summary>
        /// signals that there is a change in the undo/redo list
        /// There are four possibilities: Undo(Redo) becomes available (unavailable)
        /// </summary>
        public event EventHandler ChangeInUndoRedoList;

        byte transparencyOfselectedEntityColor = 100;

        /// <summary>
        /// Sets or gets the transparency of the selected entity color: the range is from 0, opaque, to 255, transparent.
        /// </summary>
        public byte TransparencyOfSelectedEntityColor {
            get { return transparencyOfselectedEntityColor; }
            set { transparencyOfselectedEntityColor = value; }
        }

        Color selectedEntityColor;

        /// <summary>
        /// Sets or gets the color of the selected entity.
        /// </summary>
        public Color SelectedEntityColor {
            get { return selectedEntityColor; }
            set { selectedEntityColor = value; }
        }

        Color selectedNodeBoundaryColor = Color.DarkBlue;

        /// <summary>
        /// Sets or gets the color of the selected node boundary
        /// </summary>
        public Color SelectedNodeBoundaryColor {
            get { return selectedNodeBoundaryColor; }
            set { selectedNodeBoundaryColor = value; }
        }

        IViewerObject activeDraggedObject;

        internal IViewerObject ActiveDraggedObject {
            get { return activeDraggedObject; }
            set { activeDraggedObject = value; }
        }

        Site polylineVertex;

        internal Site PolylineVertex {
            get { return polylineVertex; }
            set { polylineVertex = value; }
        }


        IViewerEdge selectedEdge;
        /// <summary>
        /// the current selected edge
        /// </summary>
        public IViewerEdge SelectedEdge {
            get { return selectedEdge; }
            set { selectedEdge = value; }
        }

        double mouseMoveThreshold = 0.05;

        /// <summary>
        /// If the distance between the mouse down point and the mouse up point is greater than the threshold 
        /// then we have a mouse move. Otherwise we have a click.
        /// </summary>
        public double MouseMoveThreshold {
            get { return mouseMoveThreshold; }
            set { mouseMoveThreshold = value; }
        }

        IViewer viewer;
        MouseAndKeysAnalyzer toggleEntityPredicate;

        /// <summary>
        /// the delegate to decide if an entity is dragged or we just zoom in the viewer
        /// </summary>
        public MouseAndKeysAnalyzer ToggleEntityPredicate {
            get { return toggleEntityPredicate; }
            set { toggleEntityPredicate = value; }
        }

        
        bool dragging;
        bool Dragging {
            get { return dragging; }
            set { dragging = value; }
        }

        GeometryGraphEditor geomGraphEditor = new GeometryGraphEditor();
        Point mouseDownSourcePoint;
        Point mouseDownScreenPoint;


        Set<IViewerObject> dragGroup = new Set<IViewerObject>();

        MouseButtons pressedMouseButtons;
        /// <summary>
        /// current pressed mouse buttons
        /// </summary>
        public Microsoft.Msagl.Drawing.MouseButtons PressedMouseButtons {
            get { return pressedMouseButtons; }
            set { pressedMouseButtons = value; }
        }
   
        DelegateForIDraggableObject decorateNodeForDragging;
        /// <summary>
        /// a delegate to decorate a node for dragging
        /// </summary>
        public DelegateForIDraggableObject DecorateObjectForDragging {
            get { return decorateNodeForDragging; }
            set { decorateNodeForDragging = value; }
        }
        DelegateForEdge decorateEdgeForDragging;
        /// <summary>
        /// a delegate decorate an edge for editing
        /// </summary>
        public DelegateForEdge DecorateEdgeForDragging {
            get { return decorateEdgeForDragging; }
            set { decorateEdgeForDragging = value; }
        }

        DelegateForEdgeLabel decorateLabelForDragging;
        /// <summary>
        /// a delegate decorate a label for editing
        /// </summary>
        public DelegateForEdgeLabel DecorateEdgeLabelForDragging {
            get { return decorateLabelForDragging; }
            set { decorateLabelForDragging = value; }
        }


        DelegateForIDraggableObject removeNodeDraggingDecorations;

        /// <summary>
        /// a delegate to remove node decorations
        /// </summary>
        public DelegateForIDraggableObject RemoveObjDraggingDecorations {
            get { return removeNodeDraggingDecorations; }
            set { removeNodeDraggingDecorations = value; }
        }

        DelegateForEdge removeEdgeDraggingDecorations;
        /// <summary>
        /// a delegate to remove edge decorations
        /// </summary>
        public DelegateForEdge RemoveEdgeDraggingDecorations {
            get { return removeEdgeDraggingDecorations; }
            set { removeEdgeDraggingDecorations = value; }
        }
     
        /// <summary>
        /// A simplified constructor. This constructor does not set entity decorators. There will be no entity appearence change for editing.
        /// The reaction on mouse will be handles be a method of DrawingLayoutEditor
        /// </summary>
        /// <param name="viewerPar">the viewer that the editor communicates with</param>
        public DrawingLayoutEditor(IViewer viewerPar) {
            this.viewer = viewerPar;
            this.viewer.MouseDown += new EventHandler<MsaglMouseEventArgs>(viewer_MouseDown);
            this.viewer.MouseMove += new EventHandler<MsaglMouseEventArgs>(viewer_MouseMove);
            this.viewer.MouseUp += new EventHandler<MsaglMouseEventArgs>(viewer_MouseUp);
            this.ToggleEntityPredicate = delegate(ModifierKeys modifierKeys, MouseButtons mouseButtons, bool draggingParameter) { return LeftButtonIsPressed(mouseButtons); };
            this.nodeInsertPredicate = delegate(ModifierKeys modifierKeys, MouseButtons mouseButtons, bool draggingParameter) { return MiddleButtonIsPressed(mouseButtons) && draggingParameter == false; };
            this.DecorateObjectForDragging = new DelegateForIDraggableObject(this.TheDefaultObjectDecorator);
            this.RemoveObjDraggingDecorations = new DelegateForIDraggableObject(this.TheDefaultObjectDecoratorRemover);
            this.DecorateEdgeForDragging = new DelegateForEdge(this.TheDefaultEdgeDecoratorStub);
            this.DecorateEdgeLabelForDragging = new DelegateForEdgeLabel(this.TheDefaultEdgeLabelDecoratorStub);

            this.RemoveEdgeDraggingDecorations = new DelegateForEdge(this.TheDefaultEdgeDecoratorStub);
      
            this.viewer.GraphChanged += new EventHandler(viewer_GraphChanged);
            geomGraphEditor.ChangeInUndoRedoList += new EventHandler(layoutEditor_ChangeInUndoRedoList); 

        }

        
        void viewer_GraphChanged(object sender, EventArgs e) {
            this.graph = viewer.Graph;
            if (graph != null && graph.GeometryGraph != null)
                this.geomGraphEditor.Graph = graph.GeometryGraph;
        }

      
        /// <summary>
        /// Unsubscibes from the viewer events
        /// </summary>
        public void DetachFromViewerEvents() {
            this.viewer.MouseDown -= viewer_MouseDown;
            this.viewer.MouseMove -= viewer_MouseMove;
            this.viewer.MouseUp -= viewer_MouseUp;
            this.viewer.GraphChanged -= viewer_GraphChanged;
            geomGraphEditor.ChangeInUndoRedoList -= layoutEditor_ChangeInUndoRedoList;
        }


        void layoutEditor_ChangeInUndoRedoList(object sender, EventArgs e) {
            if (this.ChangeInUndoRedoList != null)
                ChangeInUndoRedoList(this, null);
        }

  
        void TheDefaultObjectDecorator(Microsoft.Msagl.Drawing.IViewerObject obj) {
            this.viewer.InvalidateBeforeTheChange(obj);
            IViewerNode node = obj as IViewerNode;
            if (node != null) {
                Node drawingNode = node.Node;
                double w = drawingNode.Attr.LineWidth;
                decoratorRemovalsDict[node] = (delegate() { drawingNode.Attr.LineWidth = (int)w; });
                drawingNode.Attr.LineWidth = (int)Math.Max(viewer.LineThicknessForEditing, w * 2);
            } else {
                IViewerEdge edge = obj as IViewerEdge;
                if (edge != null) {
                    Edge drawingEdge = edge.Edge;
                    double w = drawingEdge.Attr.LineWidth;
                    decoratorRemovalsDict[edge] = (delegate() { drawingEdge.Attr.LineWidth = (int)w; });
                    drawingEdge.Attr.LineWidth = (int)Math.Max(viewer.LineThicknessForEditing, w * 2);
                }            }
            this.viewer.Invalidate(obj);

        }

        void TheDefaultObjectDecoratorRemover(Microsoft.Msagl.Drawing.IViewerObject obj) {
            this.viewer.InvalidateBeforeTheChange(obj);
            DelegateReturningVoid v;
            if (decoratorRemovalsDict.TryGetValue(obj, out v)) {
                decoratorRemovalsDict[obj](); //call the delegate
                decoratorRemovalsDict.Remove(obj);
                this.viewer.Invalidate(obj);
            }
        }

        void TheDefaultEdgeDecoratorStub(Microsoft.Msagl.Drawing.IViewerEdge edge) { }

        void TheDefaultEdgeLabelDecoratorStub(Microsoft.Msagl.Drawing.IViewerObject label) { }

        MouseAndKeysAnalyzer nodeInsertPredicate;

        /// <summary>
        /// The method analysing keys and mouse buttons to decide if we are inserting a node
        /// </summary>
        public MouseAndKeysAnalyzer NodeInsertPredicate {
            get { return nodeInsertPredicate; }
            set { nodeInsertPredicate = value; }
        }

        static bool LeftButtonIsPressed(Microsoft.Msagl.Drawing.MouseButtons mouseButtons) {
            return (mouseButtons & Microsoft.Msagl.Drawing.MouseButtons.Left) == Microsoft.Msagl.Drawing.MouseButtons.Left;
        }

        static bool MiddleButtonIsPressed(Microsoft.Msagl.Drawing.MouseButtons mouseButtons) {
            return (mouseButtons & Microsoft.Msagl.Drawing.MouseButtons.Middle) == Microsoft.Msagl.Drawing.MouseButtons.Middle;
        }


        private bool MouseDownPointAndMouseUpPointsAreFarEnough(Microsoft.Msagl.Drawing.MsaglMouseEventArgs e) {
            int x = e.X; int y = e.Y;
            double dx = (mouseDownScreenPoint.X - x) / viewer.DpiX;
            double dy = (mouseDownScreenPoint.Y - y) / viewer.DpiY;
            return Math.Sqrt(dx * dx + dy * dy) > this.MouseMoveThreshold / 3;
        }

        private void AnalyzeLeftMouseButtonClick() {
            bool modifierKeyIsPressed = ModifierKeyIsPressed();
            IViewerObject obj = viewer.ObjectUnderMouseCursor;
            if (obj != null) {
                IViewerEdge editableEdge = obj as IViewerEdge;
                if (editableEdge != null)
                    SwitchToEdgeEditing(editableEdge);
                else {
                    if (obj.MarkedForDragging)
                        UnselectObjectForDragging(obj);
                    else {
                        if (!modifierKeyIsPressed)
                            UnselectEverything();
                        SelectObjectForDragging(obj);
                    }
                    UnselectEdge();
                }
            }
        }

        private bool ModifierKeyIsPressed() {
            bool modifierKeyWasUsed = (viewer.ModifierKeys & ModifierKeys.Control) == ModifierKeys.Control
                    || (viewer.ModifierKeys & ModifierKeys.Shift) == ModifierKeys.Shift;
            return modifierKeyWasUsed;
        }

        private void SwitchToEdgeEditing(IViewerEdge edge) {
            UnselectEverything();
            this.viewer.InvalidateBeforeTheChange(edge);
            this.SelectedEdge = edge;
            (edge as IEditableObject).SelectedForEditing = true;
            edge.RadiusOfPolylineCorner = this.viewer.UnderlyingPolylineCircleRadius;
            this.DecorateEdgeForDragging(edge);
            this.viewer.Invalidate(edge);
        }


        private IEnumerable<IViewerNode> Nodes() {
            foreach (IViewerObject o in viewer.Entities) {
                IViewerNode n = o as IViewerNode;
                if (n != null)
                    yield return n;
            }
        }

        private void SelectObjectForDragging(IViewerObject obj) {
            if (obj.MarkedForDragging == false) {
                obj.MarkedForDragging = true;
                dragGroup.Insert(obj);
                this.DecorateObjectForDragging(obj);
            }
        }

        private void UnselectObjectForDragging(IViewerObject obj) {
                UnselectWithoutRemovingFromDragGroup(obj);
                dragGroup.Remove(obj);
        }

        private void UnselectWithoutRemovingFromDragGroup(IViewerObject obj) {
            obj.MarkedForDragging = false;
            this.RemoveObjDraggingDecorations(obj);
        }

        private void UnselectEverything() {
            foreach (IViewerObject obj in this.dragGroup) {
                viewer.Invalidate(obj);
                UnselectWithoutRemovingFromDragGroup(obj);
            }
            dragGroup.Clear();
            UnselectEdge();
        }

        private void UnselectEdge() {
            if (this.SelectedEdge != null) {
                viewer.InvalidateBeforeTheChange(SelectedEdge);
                (SelectedEdge as IEditableObject).SelectedForEditing = false;
                this.removeEdgeDraggingDecorations(SelectedEdge);
                viewer.Invalidate(SelectedEdge);
                SelectedEdge = null;
            }
        }

        IEnumerable<IViewerEdge> Edges(IViewerNode node) {
            foreach (IViewerEdge edge in node.SelfEdges)
                yield return edge;
            foreach (IViewerEdge edge in node.OutEdges)
                yield return edge;
            foreach (IViewerEdge edge in node.InEdges)
                yield return edge;
        }

        Set<object> affectedObjects;
        Set<object> GetSetOfAffectedObjs() {
            if (affectedObjects != null)
                return affectedObjects;
            else
            {
                affectedObjects = new Set<object>();
                if (SelectedEdge != null)
                    affectedObjects.Insert(SelectedEdge);
                else
                    foreach (IViewerObject draggableObj in this.dragGroup)
                        affectedObjects += GetSetOfAffectedObjs(draggableObj);
                
                return affectedObjects;
            }

        }

        private Set<object> GetSetOfAffectedObjs(IViewerObject draggableObj) {
            Set<object> ret = new Set<object>();
            ret.Insert(draggableObj);
            IViewerNode node = draggableObj as IViewerNode;
            if (node != null)
                foreach (IViewerEdge edge in Edges(node))
                    ret.Insert(edge);
            else {
                IViewerEdge edge = draggableObj as IViewerEdge;
                if (edge != null) {
                    ret += GetSetOfAffectedObjs(edge.Source);
                    ret += GetSetOfAffectedObjs(edge.Target);
                } else {
                    IViewerObject label = draggableObj as IViewerObject;
                    if (label != null)
                        ret.Insert(draggableObj);
                }
            }
            return ret;
        }

        private void InvalidateAffectedObjs() {
            foreach (IViewerObject o in this.GetSetOfAffectedObjs()) 
                viewer.Invalidate(o);
            if (geomGraphEditor.GraphBoundingBoxGetsExtended)
                viewer.Invalidate();
        }

        private void InvalidateAffectedObjsBeforeEdit() {
            foreach (IViewerObject o in this.GetSetOfAffectedObjs())
                viewer.InvalidateBeforeTheChange(o);
        }


        Point GetDelta(MsaglMouseEventArgs e) {
            return viewer.ScreenToSource(e) - mouseDownSourcePoint;
        }

        bool leftButtonIsPressed;

        bool LeftMouseButtonWasPressed {
            get { return leftButtonIsPressed; }
            set { leftButtonIsPressed = value; }
        }

        bool middleButtonIsPressed;

        bool MiddleMouseButtonWasPressed {
            get { return middleButtonIsPressed; }
            set { middleButtonIsPressed = value; }
        }

        IViewerNode sourceOfInsertedEdge;

        internal IViewerNode SourceOfInsertedEdge {
            get { return sourceOfInsertedEdge; }
            set { sourceOfInsertedEdge = value; }
        }

        void viewer_MouseDown(object sender, MsaglMouseEventArgs e) {
            if (viewer.LayoutIsEditable) {
                PressedMouseButtons = GetPressedButtons(e);
                mouseDownSourcePoint = viewer.ScreenToSource(e);
                mouseDownScreenPoint = new Point(e.X, e.Y);
                if (e.LeftButtonIsPressed) {
                    LeftMouseButtonWasPressed = true;
                    if (!(viewer.ObjectUnderMouseCursor is IViewerEdge))
                        ActiveDraggedObject = viewer.ObjectUnderMouseCursor;
                    if (ActiveDraggedObject != null)
                        e.Handled = true;
                    if (this.SelectedEdge != null)
                        this.CheckIfDraggingPolylineVertex(e);
                } else if (e.RightButtonIsPressed) {
                    if (this.SelectedEdge != null)
                        ProcessRightClickOnSelectedEdge(e);
                } else if (e.MiddleButtonIsPressed && viewer.ObjectUnderMouseCursor is IViewerNode) {
                    SourceOfInsertedEdge = viewer.ObjectUnderMouseCursor as IViewerNode;
                    UnselectEverything();
                    MiddleMouseButtonWasPressed = true;
                    viewer.StartDrawingRubberLine(e);
                } 
            }
        }

        IEnumerable<IViewerObject> AffectedObjsEnum() {
            foreach (IViewerObject o in this.GetSetOfAffectedObjs())
                yield return o;
        }

        void viewer_MouseMove(object sender, MsaglMouseEventArgs e) {
            if (viewer.LayoutIsEditable) {
                if (e.LeftButtonIsPressed && (ActiveDraggedObject != null || this.PolylineVertex != null)) {
                    if (!Dragging) {
                        if (this.MouseDownPointAndMouseUpPointsAreFarEnough(e)) {
                            Dragging = true;
                            //first time we are in Dragging mode
                            if (this.PolylineVertex != null)
                                geomGraphEditor.PrepareForEdgeCornerDragging(GetSetOfAffectedObjs(), 
                                    SelectedEdge.DrawingObject.GeometryObject as Microsoft.Msagl.Edge, PolylineVertex);
                            else if (ActiveDraggedObject != null) {
                                UnselectEdge();
                                if (!ActiveDraggedObject.MarkedForDragging)
                                    UnselectEverything();
                                SelectObjectForDragging(ActiveDraggedObject);
                                geomGraphEditor.PrepareForObjectDragging(GetSetOfAffectedObjs(), DraggedGeomObjects());
                            }
                        }
                    }

                    if (Dragging) {
                        InvalidateAffectedObjsBeforeEdit();
                        geomGraphEditor.Drag(GetDelta(e));
                        InvalidateAffectedObjs();
                        e.Handled = true;
                    }
                } else if (MiddleMouseButtonWasPressed) {
                    viewer.DrawRubberLine(e);
                }

            }
        }

        private IEnumerable<GeometryObject> DraggedGeomObjects() {
            foreach (IViewerObject draggObj in this.dragGroup)
                yield return draggObj.DrawingObject.GeometryObject;
        }

        void viewer_MouseUp(object sender, MsaglMouseEventArgs args) {
            if (viewer.LayoutIsEditable) {
                bool click = !MouseDownPointAndMouseUpPointsAreFarEnough(args);
                if (click && LeftMouseButtonWasPressed) {
                    if (viewer.ObjectUnderMouseCursor is IViewerObject) {
                        AnalyzeLeftMouseButtonClick();
                        args.Handled = true;
                    } else
                        UnselectEverything();
                } else if (Dragging) {
                    viewer.OnDragEnd(AffectedObjsEnum());
                    args.Handled = true;
                    this.geomGraphEditor.OnDragEnd(viewer.ScreenToSource(args) - this.mouseDownSourcePoint);
                } else if (LeftMouseButtonWasPressed) {
                    if (ToggleEntityPredicate(viewer.ModifierKeys, PressedMouseButtons, true) && (viewer.ModifierKeys & ModifierKeys.Shift) != ModifierKeys.Shift)
                        SelectEntitiesForDraggingWithRectangle(args);
                    // else SpreadGroupForMDS(args);
                } else if (MiddleMouseButtonWasPressed) {
                    viewer.StopDrawingRubberLine();
                    IViewerNode targetNode = viewer.ObjectUnderMouseCursor as IViewerNode;
                    if (targetNode != null) {
                        IViewerEdge edge = viewer.CreateEdge(SourceOfInsertedEdge.DrawingObject as Node, targetNode.DrawingObject as Node);
                        this.viewer.AddEdge(edge, true);
                    }
                }
            }


            Dragging = false;
            PolylineVertex = null;
            ActiveDraggedObject = null;
            affectedObjects = null;
            LeftMouseButtonWasPressed = MiddleMouseButtonWasPressed = false;

        }

        //private void SpreadGroupForMDS(MsaglMouseEventArgs args) {
        //    if ((viewer.ModifierKeys & Microsoft.Msagl.Drawing.ModifierKeys.Shift) == Microsoft.Msagl.Drawing.ModifierKeys.Shift) {
        //        Microsoft.Msagl.Splines.Rectangle rect =
        //            new Microsoft.Msagl.Splines.Rectangle(this.mouseDownSourcePoint, viewer.ScreenToSource(args));

        //        double radius = (rect.LeftTop - rect.RightBottom).Length / 2;
        //        Set<IViewerNode> iSet = new Set<IViewerNode>();
        //        Set<Microsoft.Msagl.Node> setToSpread = new Set<Microsoft.Msagl.Node>();

        //        GeometryGraph graph = null;
        //        foreach (IViewerNode node in Nodes()) {

        //            if (rect.Intersect(node.Node.BoundingBox)) {
        //                viewer.InvalidateBeforeTheChange(node);
        //                foreach (IViewerEdge edge in node.SelfEdges)
        //                    viewer.Invalidate(edge);
        //                iSet.Insert(node);
        //                setToSpread.Insert(node.Node.Attr.GeometryNode);
        //                graph = node.Node.Attr.GeometryNode.Parent as GeometryGraph;
        //            }
        //        }

        //        foreach (IViewerNode node in Nodes()) {
        //            viewer.Invalidate(node);
        //            foreach (IViewerEdge edge in node.OutEdges)
        //                viewer.Invalidate(edge);

        //            foreach (IViewerEdge edge in node.SelfEdges)
        //                viewer.Invalidate(edge);
        //        }

        //        if (graph != null) {
        //            System.Console.WriteLine("xxx");
        //            Microsoft.Msagl.Mds.MdsGraphLayout graphLayout = new Microsoft.Msagl.Mds.MdsGraphLayout();
        //            graphLayout.GroupSpread(graph, setToSpread);
        //        }

        //        foreach (IViewerNode node in Nodes()) {
        //            viewer.Invalidate(node);
        //            foreach (IViewerEdge edge in node.OutEdges)
        //                viewer.Invalidate(edge);

        //            foreach (IViewerEdge edge in node.SelfEdges)
        //                viewer.Invalidate(edge);

        //        }
        //        viewer.Invalidate(); //remove it later
        //        args.Handled = true;
        //    }
        //}

        private void SelectEntitiesForDraggingWithRectangle(MsaglMouseEventArgs args) {
            Microsoft.Msagl.Splines.Rectangle rect =
                new Microsoft.Msagl.Splines.Rectangle(this.mouseDownSourcePoint, viewer.ScreenToSource(args));

            foreach (IViewerNode node in Nodes())
                if (rect.Intersect(node.Node.BoundingBox))
                    SelectObjectForDragging(node);

            args.Handled = true;
        }

        Point mouseRightButtonDownPoint;

        Couple<Site, Microsoft.Msagl.Drawing.PolylineCornerType> cornerInfo;

        private void ProcessRightClickOnSelectedEdge(MsaglMouseEventArgs e) {
            mouseRightButtonDownPoint = viewer.ScreenToSource(e);

            cornerInfo = AnalyzeInsertOrDeletePolylineCorner(mouseRightButtonDownPoint, this.SelectedEdge.RadiusOfPolylineCorner);

            if (cornerInfo == null)
                return;

            e.Handled = true;

            Couple<string, DelegateReturningVoid> edgeRemoveCouple = new Couple<string, DelegateReturningVoid>("Remove edge",
               delegate() { this.viewer.RemoveEdge(this.SelectedEdge, true); });

            if (cornerInfo.Second == Microsoft.Msagl.Drawing.PolylineCornerType.PreviousCornerForInsertion)
                viewer.PopupMenus(new Couple<string, DelegateReturningVoid>("Insert polyline corner", new DelegateReturningVoid(this.InsertPolylineCorner)), edgeRemoveCouple);
            else if (cornerInfo.Second == Microsoft.Msagl.Drawing.PolylineCornerType.CornerToDelete)
                viewer.PopupMenus(new Couple<string, DelegateReturningVoid>("Delete polyline corner", new DelegateReturningVoid(this.DeleteCorner)), edgeRemoveCouple);
           
        }

       




        void CheckIfDraggingPolylineVertex(MsaglMouseEventArgs e) {
            if (SelectedEdge != null) {
                Site site = this.SelectedEdge.Edge.Attr.GeometryEdge.UnderlyingPolyline.HeadSite;
                do {
                    if (MouseScreenPointIsCloseEnoughToVertex(site.Point,
                            SelectedEdge.RadiusOfPolylineCorner + ((double)SelectedEdge.Edge.Attr.LineWidth) / 2.0)) {
                        this.PolylineVertex = site;
                        e.Handled = true;
                        break;
                    }
                    site = site.Next;
                }
                while (site != null);
            }
        }

        private bool MouseScreenPointIsCloseEnoughToVertex(Point point, double radius) {
            return (point - this.mouseDownSourcePoint).Length < radius;
        }


        static MouseButtons GetPressedButtons(MsaglMouseEventArgs e) {
            MouseButtons ret = MouseButtons.None;
            if (e.LeftButtonIsPressed)
                ret |= MouseButtons.Left;
            if (e.MiddleButtonIsPressed)
                ret |= MouseButtons.Middle;
            if (e.RightButtonIsPressed)
                ret |= MouseButtons.Right;
            return ret;
        }

        delegate void InvalidateDelegate(IViewerObject editablObj);

        //under development
        /// <summary>
        /// Undoes the editing
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void Undo() {
            if (geomGraphEditor.CanUndo) {
                UndoRedoAction action = geomGraphEditor.CurrentUndoAction;
                InvalidateActionEditedElems(action, new InvalidateDelegate(viewer.InvalidateBeforeTheChange));
                geomGraphEditor.Undo();
                InvalidateActionEditedElems(action, new InvalidateDelegate(viewer.Invalidate));
                viewer.OnDragEnd(ActionAffectedObjs(action.AffectedObjects));
                if (action.GraphBoundingBoxHasChanged)
                    viewer.Invalidate();

            }
        }


        IEnumerable<IViewerObject> ActionAffectedObjs(Set<Object> affected) {
            if (affected != null)
                foreach (object o in affected)
                    yield return (IViewerObject)o;
        }


        static void InvalidateActionEditedElems(UndoRedoAction action, InvalidateDelegate invalidateDelegate) {
            if (action.AffectedObjects != null)
                foreach (IViewerObject obj in action.AffectedObjects)
                    invalidateDelegate(obj);
        }

     

        //under development
        /// <summary>
        /// Redoes the editing
        /// </summary>
        public void Redo() {
            if (geomGraphEditor.CanRedo) {
                UndoRedoAction action = geomGraphEditor.CurrentRedoAction;

                InvalidateActionEditedElems(action, new InvalidateDelegate(viewer.InvalidateBeforeTheChange));
                geomGraphEditor.Redo();
                InvalidateActionEditedElems(action, new InvalidateDelegate(viewer.Invalidate));

                viewer.OnDragEnd(ActionAffectedObjs(action.AffectedObjects));
                if (action.GraphBoundingBoxHasChanged)
                    viewer.Invalidate();
            }
        }
        /// <summary>
        /// Clear the editor
        /// </summary>
        public void Clear() {
            UnselectEverything();
        }
        /// <summary>
        /// Finds a corner to delete or insert
        /// </summary>
        /// <param name="point"></param>
        /// <param name="tolerance"></param>
        /// <returns>null if a corner is not found</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Polyline"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Couple<Site, PolylineCornerType> AnalyzeInsertOrDeletePolylineCorner(Point point, double tolerance) {
            if (this.SelectedEdge == null)
                return null;

            tolerance += this.SelectedEdge.Edge.Attr.LineWidth;

            Site corner = GeometryGraphEditor.GetPreviousSite(SelectedEdge.Edge.Attr.GeometryEdge, point);
            if (corner != null)
                return new Couple<Site, PolylineCornerType>(corner, PolylineCornerType.PreviousCornerForInsertion);

            corner = GeometryGraphEditor.FindCornerForEdit(this.SelectedEdge.Edge.Attr.GeometryEdge.UnderlyingPolyline, point, tolerance);
            if (corner != null)
                return new Couple<Site, PolylineCornerType>(corner, PolylineCornerType.CornerToDelete);

            return null;
        }


        //public IEnumerator<Point> GetPreviousPolylineCorner( Point point) {
        //    return LayoutEditor.GetPreviousSite(SelectedEdge.Edge.Attr.GeometryEdge, point);
        //}
        /// <summary>
        /// insert a polyline corner at the point befor the prevCorner
        /// </summary>
        /// <param name="point"></param>
        /// <param name="previousCorner"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Polyline")]
        public void InsertPolylineCorner(Point point, Site previousCorner) {
            this.viewer.InvalidateBeforeTheChange(SelectedEdge);
            this.geomGraphEditor.InsertSite(this.SelectedEdge.Edge.Attr.GeometryEdge, point, previousCorner, SelectedEdge);
            this.viewer.Invalidate(SelectedEdge);
        }

        void InsertPolylineCorner() {
            this.viewer.InvalidateBeforeTheChange(SelectedEdge);
            this.geomGraphEditor.InsertSite(this.SelectedEdge.Edge.Attr.GeometryEdge, 
                this.mouseRightButtonDownPoint, this.cornerInfo.First, SelectedEdge);
            this.viewer.Invalidate(SelectedEdge);
        }
        /// <summary>
        /// delete the polyline corner, shortcut it.
        /// </summary>
        /// <param name="corner"></param>
        public void DeleteCorner(Site corner) {
            this.viewer.InvalidateBeforeTheChange(SelectedEdge);
            this.geomGraphEditor.DeleteSite(SelectedEdge.Edge.Attr.GeometryEdge, corner, SelectedEdge);
            this.viewer.Invalidate(SelectedEdge);
            this.viewer.OnDragEnd(new IViewerObject[] { SelectedEdge });
        }

        void DeleteCorner() {
            this.viewer.InvalidateBeforeTheChange(SelectedEdge);
            this.geomGraphEditor.DeleteSite(SelectedEdge.Edge.Attr.GeometryEdge, this.cornerInfo.First, SelectedEdge);
            this.viewer.Invalidate(SelectedEdge);
            this.viewer.OnDragEnd(new IViewerObject[] { SelectedEdge });
        }

        /// <summary>
        /// create a tight bounding box for the graph
        /// </summary>
        /// <param name="graphToFit"></param>
        public void FitGraphBoundingBox(IViewerObject graphToFit) {
            if (graphToFit != null) {
                geomGraphEditor.FitGraphBoundingBox( graphToFit ,graphToFit.DrawingObject.GeometryObject as GeometryGraph);
                this.viewer.Invalidate();
            }
        }
       
        /// <summary>
        /// returns true if Undo is available
        /// </summary>
        public bool CanUndo {
            get { return this.geomGraphEditor.CanUndo; }
        }
        /// <summary>
        /// return true if Redo is available
        /// </summary>
        public bool CanRedo {
            get { return this.geomGraphEditor.CanRedo; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void RegisterNodeAdditionForUndo(IViewerNode node) {
            AddNodeUndoAction undoAction = new AddNodeUndoAction(this.graph, this.viewer, node);
            geomGraphEditor.InsertToListAndFixTheBox(undoAction);
        }

        /// <summary>
        /// current undo action
        /// </summary>
        public UndoRedoAction CurrentAction {
            get { return geomGraphEditor.CurrentUndoAction; }
        }

        /// <summary>
        /// registers the edge addition for undo
        /// </summary>
        /// <param name="edge"></param>
        public void RegisterEdgeAdditionForUndo(IViewerEdge edge) {
            geomGraphEditor.InsertToListAndFixTheBox(new AddEdgeUndoAction(this.viewer, edge));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        public void RegisterEdgeRemovalForUndo(IViewerEdge edge) {
            geomGraphEditor.InsertToListAndFixTheBox(new RemoveEdgeUndoAction(this.graph, this.viewer, edge));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void RegisterNodeForRemoval(IViewerNode node) {
            geomGraphEditor.InsertToListAndFixTheBox(new RemoveNodeUndoAction(this.viewer, node));
        }
    }
}
