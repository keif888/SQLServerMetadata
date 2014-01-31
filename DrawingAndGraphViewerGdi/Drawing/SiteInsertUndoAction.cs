using Microsoft.Msagl.Splines;
using GeomEdge = Microsoft.Msagl.Edge;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// undoes/redoes edge editing when dragging the smoothed polyline corner
    /// </summary>
    public class SiteInsertUndoAction : UndoRedoAction {
        Site insertedSite;
        Point insertionPoint;
        Site prevSite;

        internal Site PrevSite {
            get { return prevSite; }
            set { prevSite = value; }
        }

        double siteK;

        /// <summary>
        /// k - the coefficient giving the start and the end spline points
        /// </summary>
        public double SiteK {
            get { return siteK; }
            set { siteK = value; }
        }

        /// <summary>
        /// The point where the new polyline corner was inserted
        /// </summary>
        public Point InsertionPoint {
            get { return insertionPoint; }
            set { insertionPoint = value; }
        }

        internal Site InsertedSite {
            get { return insertedSite; }
            set {
                insertedSite = value;
                this.InsertionPoint = insertedSite.Point;
                this.SiteK = insertedSite.BezierSegmentFitCoefficient;
                this.PrevSite = insertedSite.Previous;
            }
        }

        GeomEdge editedEdge;

        /// <summary>
        /// Constructor. At the moment of the constructor call the site should not be inserted yet
        /// </summary>
        /// <param name="edgeToEdit"></param>
        public SiteInsertUndoAction(GeomEdge edgeToEdit)
            : base((GeometryGraph)edgeToEdit.Parent) {
            this.editedEdge = edgeToEdit;
            this.AddRestoreData(editedEdge, (EdgeRestoreData)editedEdge.GetRestoreData());
        }
        /// <summary>
        /// undoes the editing
        /// </summary>
        public override void Undo() {
            Site prev = InsertedSite.Previous;
            Site next = InsertedSite.Next;
            prev.Next = next;
            next.Previous = prev;
            GeometryGraphEditor.DragEdgeWithSite(new Point(0, 0), editedEdge, (EdgeRestoreData)GetRestoreData(editedEdge), prev, prev.Point);
        }

        /// <summary>
        /// redoes the editing
        /// </summary>
        public override void Redo() {
            insertedSite = new Site(PrevSite, InsertionPoint, PrevSite.Next);
            insertedSite.BezierSegmentFitCoefficient = this.siteK;
            GeometryGraphEditor.DragEdgeWithSite(new Point(0, 0), editedEdge,
                (EdgeRestoreData)GetRestoreData(editedEdge), insertedSite, insertedSite.Point);
        }
    }
}