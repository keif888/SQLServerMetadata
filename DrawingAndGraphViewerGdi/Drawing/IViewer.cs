using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Msagl.Splines;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// the interface for the viewer which is able to edit a graph layout
    /// </summary>
    public interface IViewer {
        /// <summary>
        /// signalling the mouse down event
        /// </summary>
        event EventHandler<MsaglMouseEventArgs> MouseDown;
        /// <summary>
        /// signalling the mouse move event
        /// </summary>
        event EventHandler<MsaglMouseEventArgs> MouseMove;

        /// <summary>
        /// signalling the mouse up event
        /// </summary>
        event EventHandler<MsaglMouseEventArgs> MouseUp;

        /// <summary>
        /// Returns the object under the cursor and null if there is none
        /// </summary>
        IViewerObject ObjectUnderMouseCursor { get;}
        /// <summary>
        /// forcing redraw of the object
        /// </summary>
        /// <param name="editObj"></param>
        void Invalidate(IViewerObject editObj);


        /// <summary>
        /// invalidates everything
        /// </summary>
        void Invalidate();
        /// <summary>
        /// is raised after the graph is changed
        /// </summary>
        event EventHandler GraphChanged;
        /// <summary>
        /// forcing redraw of the object
        /// </summary>
        /// <param name="editObj"></param>
        void InvalidateBeforeTheChange(IViewerObject editObj);

        /// <summary>
        /// returns modifier keys; control, shift, or alt are pressed at the moments
        /// </summary>
        ModifierKeys ModifierKeys { get;}

        /// <summary>
        /// maps a point in screen coordinates to the point in the graph surface
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        Point ScreenToSource(MsaglMouseEventArgs e);

        /// <summary>
        /// maps a point in screen coordinates to the point in the graph surface
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        Point ScreenToSource(Point screenPoint);

        /// <summary>
        /// gets all entities which can be dragged
        /// </summary>
        IEnumerable<IViewerObject> Entities { get;}
        /// <summary>
        /// number of dots per inch in x direction
        /// </summary>
        double DpiX { get;}

        /// <summary>
        /// number of dots per inch in y direction
        /// </summary>
        double DpiY { get;}
        /// <summary>
        /// this method should be called on the end of the dragging
        /// </summary>
        /// <param name="changedObjects"></param>
        void OnDragEnd(IEnumerable<IViewerObject> changedObjects);

        /// <summary>
        /// The scale dependent width of an edited curve that should be clearly visible.
        /// Used in the default entity editing.
        /// </summary>
        double LineThicknessForEditing { get;}

         /// <summary>
        /// enables and disables the default editing of the viewer
        /// </summary>
        bool LayoutIsEditable {   get;  }


        /// <summary>
        /// Pops up a pop up menu with a menu item for each couple, the string is the title and the delegate is the callback
        /// </summary>
        /// <param name="menuItems"></param>
        void PopupMenus( params Couple<string,DelegateReturningVoid>[] menuItems);
        
        /// <summary>
        /// The radius of the circle drawn around a polyline corner
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Polyline")]
        double UnderlyingPolylineCircleRadius { get;}
        /// <summary>
        /// gets or sets the graph
        /// </summary>
        Graph Graph { get; set; }
        /// <summary>
        /// prepare to draw the rubber line
        /// </summary>
        /// <param name="args"></param>
        void StartDrawingRubberLine(MsaglMouseEventArgs args);
        /// <summary>
        /// draw the rubber line to the current mouse position
        /// </summary>
        /// <param name="args"></param>
        void DrawRubberLine(MsaglMouseEventArgs args);

        /// <summary>
        /// stop drawing the rubber line
        /// </summary>
        void StopDrawingRubberLine();

     /// <summary>
     /// add an edge to the viewer graph
     /// </summary>
     /// <param name="edge"></param>
     /// <param name="registerForUndo"></param>
     /// <returns></returns>
        void AddEdge(IViewerEdge edge, bool registerForUndo);

        /// <summary>
        /// adds a node to the viewer graph
        /// </summary>
        /// <param name="node"></param>
        /// <param name="registerForUndo"></param>
        void AddNode(IViewerNode node, bool registerForUndo);

        /// <summary>
        /// removes an edge from the graph
        /// </summary>
        /// <param name="edge"></param>
///<param name="registerForUndo"></param>
        void RemoveEdge(IViewerEdge edge, bool registerForUndo);
        /// <summary>
        /// deletes node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="registerForUndo"></param>
        void RemoveNode(IViewerNode node, bool registerForUndo);

        /// <summary>
        /// creates a detached edge between existing nodes
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        IViewerEdge CreateEdge(Node source, Node target);

        /// <summary>
        /// sets the edge label
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="label"></param>
        void SetEdgeLabel(Edge edge,Label label);
        
        /// <summary>
        /// gets the viewer graph
        /// </summary>
        IViewerGraph ViewerGraph {
            get;
        }
/// <summary>
/// creates a viewer node
/// </summary>
/// <param name="node"></param>
/// <returns></returns>
        IViewerNode CreateNode(Node node);
    }
}
