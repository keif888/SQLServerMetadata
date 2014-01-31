using Microsoft.Msagl.Splines;
namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// undoes/redoes the corner dragging operation
    /// </summary>
    public abstract class UndoRedoDragAction: UndoRedoAction {

        internal UndoRedoDragAction(GeometryGraph graphPar):base(graphPar){

        }

        Point delta;
        /// <summary>
        /// the amount of the drag action
        /// </summary>
        public Point Delta {
            get { return delta; }
            set { delta = value; }
        }
    }
}