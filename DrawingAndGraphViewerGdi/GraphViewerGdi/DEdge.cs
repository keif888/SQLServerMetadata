using Microsoft.Msagl.Drawing;
using System.Drawing.Drawing2D;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;
using System;

namespace Microsoft.Msagl.GraphViewerGdi
{
    /// <summary>
    /// it is a class holding Microsoft.Msagl.Drawing.Edge and Microsoft.Msagl.Edge
    /// </summary>
    public sealed class DEdge : DObject,IViewerEdge, IHavingDLabel, IEditableObject
    {
        
        Microsoft.Msagl.Drawing.Edge drawingEdge;
        /// <summary>
        /// the corresponding drawing edge
        /// </summary>
        public Microsoft.Msagl.Drawing.Edge DrawingEdge
        {
            get { return drawingEdge; }
            set { drawingEdge = value; }
        }

        float dashSize;

        internal DEdge(DNode source, DNode target, DrawingEdge drawingEdgeParam, Connection connection) {
            this.DrawingEdge = drawingEdgeParam;
            this.Source = source;
            this.Target = target;

            if (connection == Connection.Connected) {
                if (source == target)
                    source.AddSelfEdge(this);
                else {
                    source.AddOutEdge(this);
                    target.AddInEdge(this);
                }
            }

            if (drawingEdgeParam.Label != null)
                this.dLabel = new DLabel(this, drawingEdge.Label);
        }

        DNode source;

        internal DNode Source
        {
            get { return source; }
            set { source = value; }
        }

        DNode target;

        internal DNode Target
        {
            get { return target; }
            set { target = value; }
        }

        GraphicsPath graphicsPath;
        /// <summary>
        /// Can be set to GraphicsPath of GDI (
        /// </summary>
        internal GraphicsPath GraphicsPath
        {
            get { return graphicsPath; }
            set
            {
                graphicsPath = value;
            }
        }

        internal override float DashSize() {
            if (dashSize > 0)
                return dashSize;
            float w = DrawingEdge.Attr.LineWidth;
            float dashSizeInPoints = (float)(Draw.dashSize * GViewer.dpi);
            return dashSize = dashSizeInPoints / w;
        }

        /// <summary>
        /// Color of the edge
        /// </summary>
        public System.Drawing.Color Color {
            get { return Draw.MsaglColorToDrawingColor(this.DrawingEdge.Attr.Color); }
        }

        #region IDraggableEdge Members

        /// <summary>
        /// underlying Drawing edge
        /// </summary>
        public Microsoft.Msagl.Drawing.Edge Edge {
            get { return this.DrawingEdge; }
        }

        IViewerNode IViewerEdge.Source {
            get { return Source; }
        }

        IViewerNode IViewerEdge.Target {
            get { return Target; }
        }

        DLabel dLabel;

        /// <summary>
        /// keeps the pointer to the corresponding label
        /// </summary>
        public DLabel Label {
            get { return dLabel; }
            set { dLabel = value; }
        }
   
/// <summary>
/// The underlying DrawingEdge
/// </summary>
        override public DrawingObject DrawingObject {
            get { return this.DrawingEdge; }
        }

        double radiusOfPolylineCorner;
        /// <summary>
        ///the radius of circles drawin around polyline corners 
        /// </summary>
        public double RadiusOfPolylineCorner {
            get {
                return radiusOfPolylineCorner;
            }
            set {
                radiusOfPolylineCorner = value;
            }
        }

        #endregion

        #region IEditableObject Members

        bool selectedForEditing;
        
        /// <summary>
        /// is set to true then the edge should set up for editing
        /// </summary>
        public bool SelectedForEditing {
            get {
                return selectedForEditing;
            }
            set {
                selectedForEditing = value;
            }
        }

        #endregion
    }
}
