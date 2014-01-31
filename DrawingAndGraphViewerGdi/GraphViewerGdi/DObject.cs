using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// A base class for objects that a drawn by GViewer
    /// </summary>
    public abstract class DObject:ObjectWithBox, Microsoft.Msagl.Drawing.IViewerObject {
    /// <summary>
/// get the underlying drawing object
/// </summary>
        public abstract Microsoft.Msagl.Drawing.DrawingObject DrawingObject {
            get;
        }

        /// <summary>
        /// Dash pattern.
        /// </summary>
        private float[] dashPatternArray;

        internal float[] DashPatternArray {
            get { return dashPatternArray; }
            set { dashPatternArray = value; }
        }

        internal abstract float DashSize();

        private BBNode bbNode;

        internal BBNode BbNode {
            get { return bbNode; }
            set { bbNode = value; }
        }

        override internal Microsoft.Msagl.Splines.Rectangle Box { get { return BbNode.Box; } }

        bool markedForDragging;
        /// <summary>
        /// Implements a property of an interface IEditViewer
        /// </summary>
        public bool MarkedForDragging {
            get {
                return markedForDragging;
            }
            set {
                markedForDragging = value;
                if (value) {
                    if (MarkedForDraggingEvent != null)
                        MarkedForDraggingEvent(this, null);
                } else {
                    if (UnmarkedForDraggingEvent != null)
                        UnmarkedForDraggingEvent(this, null);
                }
            }
        }

        /// <summary>
        /// raised when the entity is marked for dragging
        /// </summary>
        public event EventHandler MarkedForDraggingEvent;

        /// <summary>
        /// raised when the entity is unmarked for dragging
        /// </summary>
        public event EventHandler UnmarkedForDraggingEvent;
    }
}
