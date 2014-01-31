using Microsoft.Msagl.Splines;
using GeomEdge = Microsoft.Msagl.Edge;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// undoes/redoes edge editing when dragging the smoothed polyline corner
    /// </summary>
    public class EdgeDragUndoRedoAction: UndoRedoDragAction {
        GeomEdge editedEdge;
      
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="editedEdgePar"></param>
        public EdgeDragUndoRedoAction(GeomEdge editedEdgePar):base((GeometryGraph)editedEdgePar.Parent) {
            this.editedEdge = editedEdgePar;
            this.AddRestoreData(editedEdgePar, (EdgeRestoreData) editedEdgePar.GetRestoreData());
        }
        /// <summary>
        /// undoes the editing
        /// </summary>
        public override void Undo() {
            EdgeRestoreData erd = (EdgeRestoreData)GetRestoreData(editedEdge);
            GeometryGraphEditor.DragEdgeWithSite(new Point(0, 0), editedEdge, erd, erd.Site, erd.InitialSitePosition);
        }

        /// <summary>
        /// redoes the editing
        /// </summary>
        public override void Redo() {
            EdgeRestoreData erd = (EdgeRestoreData)GetRestoreData(editedEdge);
            GeometryGraphEditor.DragEdgeWithSite(Delta, editedEdge, erd, erd.Site, erd.InitialSitePosition);
        }
    }
}