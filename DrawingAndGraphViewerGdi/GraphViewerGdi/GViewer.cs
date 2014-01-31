using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Splines;
using BBox = Microsoft.Msagl.Splines.Rectangle;
using DrawingGraph = Microsoft.Msagl.Drawing.Graph;
using GeometryEdge = Microsoft.Msagl.Edge;
using GeometryNode = Microsoft.Msagl.Node;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using System.Collections.Generic;
using P2=Microsoft.Msagl.Point;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace Microsoft.Msagl.GraphViewerGdi {

    internal enum DraggingMode {
        Default,
        Pan,
        WindowZoom,
    };

    /// <summary>
    /// Summary description for DOTViewer.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    sealed public partial class GViewer : System.Windows.Forms.UserControl, IViewer {
        #region Support for layout editing
        DrawingLayoutEditor drawingLayoutEditor;

        internal DrawingLayoutEditor DrawingLayoutEditor {
            get { return drawingLayoutEditor; }
            set { drawingLayoutEditor = value; }
        }
        Dictionary<DObject, Microsoft.Msagl.Drawing.Color> decoratedObjsColors = new Dictionary<DObject, Microsoft.Msagl.Drawing.Color>();
        private ToolBarButton undoButton;
        private ToolBarButton redoButton;
        //internal ToolBarButton windowZoomButton;
        internal ToolBarButton windowZoomButton;
        internal ToolBarButton panButton;
        private ToolBarButton layoutSettingsButton;
        Dictionary<DObject, Microsoft.Msagl.Drawing.Color> decoratedObjsFillColors = new Dictionary<DObject, Microsoft.Msagl.Drawing.Color>();
        static bool mouseAndKeysAnalyserForDragToggle(
                                    Microsoft.Msagl.Drawing.ModifierKeys modifierKeys,
                                    Microsoft.Msagl.Drawing.MouseButtons mouseButtons,
                                    bool dragging) {
            return (!dragging && LeftButtonIsPressed(mouseButtons))
                || (dragging && LeftButtonIsPressed(mouseButtons) &&
                ((modifierKeys & Microsoft.Msagl.Drawing.ModifierKeys.Control) == Microsoft.Msagl.Drawing.ModifierKeys.Control ||
                ((modifierKeys & Microsoft.Msagl.Drawing.ModifierKeys.Shift) == Microsoft.Msagl.Drawing.ModifierKeys.Shift)));
        }

        static bool LeftButtonIsPressed(Microsoft.Msagl.Drawing.MouseButtons mouseButtons) {
            return (mouseButtons & Microsoft.Msagl.Drawing.MouseButtons.Left) == Microsoft.Msagl.Drawing.MouseButtons.Left;
        }

        void edgeDecorator(Microsoft.Msagl.Drawing.IViewerEdge iedge) {
            Microsoft.Msagl.GraphViewerGdi.DEdge dEdge = iedge as Microsoft.Msagl.GraphViewerGdi.DEdge;
            DrawingEdge edge = dEdge.DrawingObject as DrawingEdge;
            decoratedObjsColors[dEdge] = edge.Attr.Color;
            edge.Attr.Color = DrawingLayoutEditor.SelectedEntityColor;
        }

        void edgeDeDecorator(Microsoft.Msagl.Drawing.IViewerEdge iedge) {
            Microsoft.Msagl.GraphViewerGdi.DEdge dEdge = iedge as Microsoft.Msagl.GraphViewerGdi.DEdge;
            DrawingEdge edge = dEdge.DrawingObject as DrawingEdge;
            edge.Attr.Color = decoratedObjsColors[dEdge];
            decoratedObjsColors.Remove(dEdge);
        }

        void nodeDecorator(Microsoft.Msagl.Drawing.IViewerNode inode) {
            DNode dNode = inode as DNode;
            DrawingNode node = dNode.DrawingObject as DrawingNode;
            this.decoratedObjsColors[dNode] = node.Attr.Color;
            this.decoratedObjsFillColors[dNode] = node.Attr.FillColor;
            node.Attr.FillColor = this.DrawingLayoutEditor.SelectedEntityColor;
            node.Attr.Color = this.DrawingLayoutEditor.SelectedEntityColor;
            this.Invalidate(inode);
        }

        void nodeDeDecorator(Microsoft.Msagl.Drawing.IViewerNode inode) {
            DNode dNode = inode as DNode;
            DrawingNode node = dNode.DrawingObject as DrawingNode;
            node.Attr.Color = this.decoratedObjsColors[dNode];
            node.Attr.FillColor = this.decoratedObjsFillColors[dNode];
            this.Invalidate(inode);
        }

        void ClearLayoutEditor() {
            if (this.DrawingLayoutEditor != null) 
                DisableDrawingLayoutEditor();
            InitDrawingLayoutEditor();
        }

        #endregion

  
        private void UnconditionalHit(MouseEventArgs args) {
            System.Drawing.Point point = args != null ? new System.Drawing.Point(args.X, args.Y) : DrawingPanel.PointToClient(MousePosition);
            
            object old = selectedDObject;
            if (bBNode == null && this.DGraph != null)
                bBNode = this.DGraph.BBNode;
            if (bBNode != null) {

                Geometry geometry = bBNode.Hit(ScreenToSource(point), GetHitSlack());
                if (geometry == null)
                    selectedDObject = null;
                else 
                    selectedDObject = geometry.tag;
              
                if (old != selectedDObject)
                    SetSelectedObject(selectedDObject);
            }
        }

    /// <summary>
    /// The event raised after changing the graph
    /// </summary>

        public event EventHandler GraphChanged;

        /// <summary>
        /// It should be physically on the screen by one tenth of an inch
        /// </summary>
        /// <returns></returns> 
        double GetHitSlack() {
            double inchSlack = this.MouseHitDistance;
            double slackInPoints = dpi * inchSlack;
            return slackInPoints / CurrentScale;
        }

        void DrawingPanel_MouseClick(object sender, MouseEventArgs e) {
            this.OnMouseClick(e);
        }

        double CurrentScale {
            get {
                if (!scaledDown) {
                    return LocalScale;
                } else {
                    return LocalScale * scaleDownCoefficient;
                }
            }
        }

        internal static bool ModifierKeyWasPressed() {
            return Control.ModifierKeys == Keys.Control || Control.ModifierKeys == Keys.Shift;
        }

        private void InvalidatePanel() {
            this.panel.Invalidate();
        }

        private System.Drawing.Rectangle CreateRectangleForEdge(DEdge dedge) {
            BBox box = dedge.Edge.Attr.GeometryEdge.BoundingBox;
            InflateRectWithCornerRadius(dedge, ref box);
            AddLabelBox(dedge.Label, ref box);
            AddArrows(dedge, ref box);
            Point ddd = new Point(-1000, 1000);
            return CreateScreenRectFromTwoCornersInTheSource(box.LeftTop+ddd, box.RightBottom-ddd);
        }

        static void AddLabelBox(DLabel dLabel, ref Microsoft.Msagl.Splines.Rectangle box) {
            if (dLabel != null) {
                box.Add(dLabel.DrawingLabel.BoundingBox.LeftTop);
                box.Add(dLabel.DrawingLabel.BoundingBox.RightBottom);
            }
        }

        static void AddArrows(DEdge dedge, ref Microsoft.Msagl.Splines.Rectangle box) {
            AddArrowAtStart(dedge.DrawingEdge, ref box);
            AddArrowAtEnd(dedge.DrawingEdge, ref box);
        }

        static private void AddArrowAtEnd(DrawingEdge edge, ref Microsoft.Msagl.Splines.Rectangle box) {
            if (edge.Attr.ArrowAtTarget)
                AddArrowToBox(edge.Attr.EdgeCurve.End, edge.Attr.ArrowAtTargetPosition, edge.Attr.LineWidth, ref box);

        }


        static void AddArrowAtStart(DrawingEdge edge, ref Microsoft.Msagl.Splines.Rectangle box) {
            if (edge.Attr.ArrowAtSource)
                AddArrowToBox(edge.Attr.EdgeCurve.Start, edge.Attr.ArrowAtSourcePosition, edge.Attr.LineWidth, ref box);
        }

        static void AddArrowToBox(Microsoft.Msagl.Point start, Microsoft.Msagl.Point end, int width, ref Microsoft.Msagl.Splines.Rectangle box) {
            //it does not hurt to add a larger piece 
            P2 dir = (end - start).Rotate(Math.PI / 2);

            box.Add(end + dir);
            box.Add(end - dir);
            box.Add(start + dir);
            box.Add(start - dir);

            box.Left -= width;
            box.Top += width;
            box.Right += width;
            box.Bottom -= width;
        }



        void InflateRectWithCornerRadius(DEdge dedge, ref BBox box) {
            if (dedge.SelectedForEditing) {
                double del = this.UnderlyingPolylineCircleRadius;
                box.Left -= 2 * del;
                box.Top += 2 * del;
                box.Right += 2 * del;
                box.Bottom -= 2 * del;
            }
        }

        static Microsoft.Msagl.Splines.Rectangle GetBBox(GeometryEdge edge) {

            BBox box = new BBox(edge.Curve.Start);


            if (edge.UnderlyingPolyline != null) {
                foreach (P2 p in edge.UnderlyingPolyline)
                    box.Add(p);
            }


            Curve c = edge.Curve as Curve;
            if (c != null) {
                foreach (ICurve seg in c.Segments) {
                    CubicBezierSegment cub = seg as CubicBezierSegment;
                    if (cub != null) {
                        box.Add(cub.B(1));
                        box.Add(cub.B(2));
                        box.Add(cub.B(3));
                    } else {
                        LineSegment ls = seg as LineSegment;
                        if (ls != null)
                            box.Add(ls.End);
                        else
                            throw new InvalidOperationException();
                    }
                }
            } else {
                CubicBezierSegment cub = edge.Curve as CubicBezierSegment;
                if (cub != null) {
                    box.Add(cub.B(1));
                    box.Add(cub.B(2));
                    box.Add(cub.B(3));
                } else
                    throw new InvalidOperationException();
            }
            InflateBoxByLabel(edge, ref box);
            return box;
        }

        static void InflateBoxByLabel(GeometryEdge gEdge, ref BBox box) {
            if (gEdge.Label != null) {
                P2 p = new P2(gEdge.Label.Width / 2, gEdge.Label.Height / 2);
                box.Add(gEdge.Label.Center + p);
                box.Add(gEdge.Label.Center - p);
            }
        }

        private System.Drawing.Rectangle CreateRectangleForNode(Microsoft.Msagl.Drawing.Node node) {
            double del = 3 * node.Attr.LineWidth;
            Microsoft.Msagl.Point p = new Microsoft.Msagl.Point(-del, del);
            return CreateScreenRectFromTwoCornersInTheSource(node.Attr.GeometryNode.BoundingBox.LeftTop + p, node.Attr.GeometryNode.BoundingBox.RightBottom - p);
        }

        private System.Drawing.Rectangle CreateScreenRectFromTwoCornersInTheSource(P2 leftTop, P2 rightBottom) {
            PointF[] pts = new PointF[] { Pf(leftTop), Pf(rightBottom) };

            using (Matrix currentTransform = CurrentTransform) {
               currentTransform.TransformPoints(pts);
            }

            return System.Drawing.Rectangle.FromLTRB(
                (int)Math.Floor(pts[0].X-1),
                (int)Math.Floor(pts[0].Y)-1,
                (int)Math.Ceiling(pts[1].X)+1,
                (int)Math.Ceiling(pts[1].Y)+1);

        }

        static PointF Pf(P2 p2) {
            return new PointF((float)p2.X, (float)p2.Y);
        }



        private System.Drawing.Rectangle CreateRectangle(BBox box) {
           using (Matrix m = this.CurrentTransform) {
              System.Drawing.PointF[] points = new System.Drawing.PointF[]{
                new System.Drawing.PointF((float)box.Left,(float)box.Top),
                new System.Drawing.PointF((float)box.Right,(float)box.Bottom)};

              m.TransformPoints(points);

              return System.Drawing.Rectangle.FromLTRB((int)points[0].X, (int)points[0].Y, (int)points[1].X, (int)points[1].Y);
           }
        }


        void ScrollHandler(object o, ScrollEventArgs args) {
            //    if(args.Type==  ScrollEventType.EndScroll)
            panel.Invalidate();
        }
        /// <summary>
        /// Maps a point from the screen to the graph surface
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public P2 ScreenToSource(System.Drawing.Point point) {
           using (System.Drawing.Drawing2D.Matrix m = CurrentTransform) {
              m.Invert();
              PointF[] pf = new PointF[] {new PointF(point.X, point.Y)};
              m.TransformPoints(pf);
              return new P2(pf[0].X, pf[0].Y);
           }
        }
        /// <summary>
        /// maps a point from the screen to the graph surface
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public P2 ScreenToSource(P2 screenPoint) {
           using (System.Drawing.Drawing2D.Matrix m = CurrentTransform) {
              m.Invert();
              PointF[] pf = new PointF[] {new PointF((float) screenPoint.X, (float) screenPoint.Y)};
              m.TransformPoints(pf);
              return new P2(pf[0].X, pf[0].Y);
           }
        }

        internal P2 ScreenToSource(int x, int y) {
            return ScreenToSource(new System.Drawing.Point(x, y));
        }

        static PointF Mult(System.Drawing.Drawing2D.Matrix m, PointF p) {
            PointF[] pf = new PointF[] { new PointF(p.X, p.Y) };
            m.TransformPoints(pf);
            return new PointF(pf[0].X, pf[0].Y);
        }



        static int Int(double f) { return (int)(f + 0.5); }




        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
                if (ToolTip != null) {
                    ToolTip.RemoveAll();
                    try {
                        ToolTip.Dispose();
                    } catch { throw; }
                    ToolTip = null;
                }
                if (layoutWaitHandle != null)
                    layoutWaitHandle.Close();
                if (panGrabCursor != null)
                    panGrabCursor.Dispose();
                if (panOpenCursor != null)
                    panOpenCursor.Dispose();
            }
            base.Dispose(disposing);
        }
        private System.Windows.Forms.ImageList imageList;
        private ToolBar toolbar;
        internal System.Windows.Forms.ToolBarButton zoomin;
        internal System.Windows.Forms.ToolBarButton zoomout;
        //internal System.Windows.Forms.ToolBarButton panButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ToolBarButton backwardButton;
        private System.Windows.Forms.ToolBarButton forwardButton;
        private ToolBarButton saveButton;
        private System.Windows.Forms.HScrollBar hScrollBar;
        private System.Windows.Forms.VScrollBar vScrollBar;
        private System.Windows.Forms.ToolBarButton print;
        private ToolBarButton openButton;


        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GViewer));
            this.hScrollBar = new System.Windows.Forms.HScrollBar();
            this.vScrollBar = new System.Windows.Forms.VScrollBar();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.toolbar = new System.Windows.Forms.ToolBar();
            this.zoomin = new System.Windows.Forms.ToolBarButton();
            this.zoomout = new System.Windows.Forms.ToolBarButton();
            this.windowZoomButton = new System.Windows.Forms.ToolBarButton();
            this.panButton = new System.Windows.Forms.ToolBarButton();
            this.backwardButton = new System.Windows.Forms.ToolBarButton();
            this.forwardButton = new System.Windows.Forms.ToolBarButton();
            this.saveButton = new System.Windows.Forms.ToolBarButton();
            this.undoButton = new System.Windows.Forms.ToolBarButton();
            this.redoButton = new System.Windows.Forms.ToolBarButton();
            this.openButton = new System.Windows.Forms.ToolBarButton();
            this.print = new System.Windows.Forms.ToolBarButton();
            this.layoutSettingsButton = new System.Windows.Forms.ToolBarButton();
            this.SuspendLayout();
            // 
            // hScrollBar
            // 
            this.hScrollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScrollBar.Location = new System.Drawing.Point(0, 562);
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.Size = new System.Drawing.Size(624, 16);
            this.hScrollBar.TabIndex = 0;
            this.hScrollBar.Visible = false;
            // 
            // vScrollBar
            // 
            this.vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar.Location = new System.Drawing.Point(608, 0);
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.Size = new System.Drawing.Size(16, 562);
            this.vScrollBar.TabIndex = 1;
            this.vScrollBar.Visible = false;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "");
            this.imageList.Images.SetKeyName(1, "");
            this.imageList.Images.SetKeyName(2, "zoom.bmp");
            this.imageList.Images.SetKeyName(3, "");
            this.imageList.Images.SetKeyName(4, "");
            this.imageList.Images.SetKeyName(5, "");
            this.imageList.Images.SetKeyName(6, "");
            this.imageList.Images.SetKeyName(7, "");
            this.imageList.Images.SetKeyName(8, "");
            this.imageList.Images.SetKeyName(9, "undo.bmp");
            this.imageList.Images.SetKeyName(10, "redo.bmp");
            this.imageList.Images.SetKeyName(11, "");
            this.imageList.Images.SetKeyName(12, "openfolderHS.png");
            this.imageList.Images.SetKeyName(13, "disabledUndo.bmp");
            this.imageList.Images.SetKeyName(14, "disabledRedo.bmp");
            this.imageList.Images.SetKeyName(15, "layoutMethodBlue.ico");
            // 
            // toolBar
            // 
            this.toolbar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.zoomin,
            this.zoomout,
            this.windowZoomButton,
            this.panButton,
            this.backwardButton,
            this.forwardButton,
            this.saveButton,
            this.undoButton,
            this.redoButton,
            this.openButton,
            this.print,
            this.layoutSettingsButton});
            this.toolbar.ButtonSize = new System.Drawing.Size(22, 23);
            this.toolbar.DropDownArrows = true;
            this.toolbar.ImageList = this.imageList;
            this.toolbar.Location = new System.Drawing.Point(0, 0);
            this.toolbar.Name = "toolBar";
            this.toolbar.ShowToolTips = true;
            this.toolbar.Size = new System.Drawing.Size(608, 28);
            this.toolbar.TabIndex = 2;
            this.toolbar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar_ButtonClick);
            // 
            // zoomin
            // 
            this.zoomin.ImageIndex = 0;
            this.zoomin.Name = "zoomin";
            this.zoomin.ToolTipText = "Zoom In";
            // 
            // zoomout
            // 
            this.zoomout.ImageIndex = 1;
            this.zoomout.Name = "zoomout";
            this.zoomout.ToolTipText = "Zoom Out";
            // 
            // windowZoomButton
            // 
            this.windowZoomButton.ImageKey = "zoom.bmp";
            this.windowZoomButton.Name = "zoomModeToggleButton";
            this.windowZoomButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this.windowZoomButton.ToolTipText = "Zoom in to the rectangle";
            // 
            // panButton
            // 
            this.panButton.ImageIndex = 3;
            this.panButton.Name = "pan";
            this.panButton.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this.panButton.ToolTipText = "Pan";
            // 
            // backwardButton
            // 
            this.backwardButton.ImageIndex = 6;
            this.backwardButton.Name = "backwardButton";
            this.backwardButton.ToolTipText = "Backward";
            // 
            // forwardButton
            // 
            this.forwardButton.ImageIndex = 4;
            this.forwardButton.Name = "forwardButton";
            // 
            // save
            // 
            this.saveButton.ImageIndex = 8;
            this.saveButton.Name = "save";
            this.saveButton.ToolTipText = "Save the graph or the drawing";
            // 
            // undoButton
            // 
            this.undoButton.ImageIndex = 9;
            this.undoButton.Name = "undoButton";
            // 
            // redoButton
            // 
            this.redoButton.ImageIndex = 10;
            this.redoButton.Name = "redoButton";
            // 
            // openButton
            // 
            this.openButton.ImageIndex = 12;
            this.openButton.Name = "openButton";
            this.openButton.ToolTipText = "Load a graph from a \".msagl\" file";
            // 
            // print
            // 
            this.print.ImageIndex = 11;
            this.print.Name = "print";
            this.print.ToolTipText = "Print the current view";
            // 
            // sugiyamaMethodButton
            // 
            this.layoutSettingsButton.ImageIndex = 15;
            this.layoutSettingsButton.Name = "layoutMethodButton";
            this.layoutSettingsButton.Style = System.Windows.Forms.ToolBarButtonStyle.PushButton;
            // 
            // GViewer
            // 
            this.AutoScroll = true;
            this.Controls.Add(this.toolbar);
            this.Controls.Add(this.vScrollBar);
            this.Controls.Add(this.hScrollBar);
            this.Name = "GViewer";
            this.Size = new System.Drawing.Size(624, 578);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion







        ViewInfo CurrentViewInfo() {
            ViewInfo viewInfo = new ViewInfo();

            viewInfo.hLargeChange = this.HLargeChange;
            viewInfo.vLargeChange = this.VLargeChange;

            viewInfo.hVal = this.HVal;
            viewInfo.vVal = this.VVal;

            viewInfo.scaledDown = this.scaledDown;
            viewInfo.scaledDownCoefficient = this.scaleDownCoefficient;
            viewInfo.vScrollBarIsViz = this.VScrVis;
            viewInfo.hScrollBarIsViz = this.HScrVis;


            viewInfo.leftMouseButtonWasPressed = Control.MouseButtons == System.Windows.Forms.MouseButtons.Left;
            viewInfo.zoomF = this.ZoomF;
            return viewInfo;

        }

        void HandleViewInfoList() {
            if (storeViewInfo) {
                ViewInfo currentViewInfo = CurrentViewInfo();
                listOfViewInfos.AddNewViewInfo(currentViewInfo);
            } else
                storeViewInfo = true;


            this.BackwardEnabled = this.listOfViewInfos.BackwardAvailable;
            this.ForwardEnabled = this.listOfViewInfos.ForwardAvailable;

        }



        internal void DotOnPaint(Graphics g, bool forPrinting) {

            if (PanelHeight < minimalSizeToDraw || PanelWidth < minimalSizeToDraw)
                return;
            if (wasMinimized == true) {
                wasMinimized = false;
                panel.Invalidate();
            }

            if (this.OriginalGraph != null) {
                CalcRects();
                HandleViewInfoList();
                if (forPrinting == false) {
                    g.FillRectangle(outsideAreaBrush, this.ClientRectangle);
                    using (SolidBrush sb = new SolidBrush(Draw.MsaglColorToDrawingColor(OriginalGraph.Attr.BackgroundColor))) {
                        g.FillRectangle(sb, destRect);
                    }
                }
                using (Matrix m = CurrentTransform) {
                    g.Transform = m;
                }
                g.Clip = new Region(SrcRect);
                if (DGraph == null)
                    return;
                DGraph.DrawGraph(g);

                //some info is known only after the first drawing

                if (this.bBNode == null && BuildHitTree
#if DEBUGGLEE
 && (
                  dGraph.DrawingGraph.DebugBezierCurves == null &&
        dGraph.DrawingGraph.DebugBoxes == null &&
        dGraph.DrawingGraph.DebugLines == null &&
        dGraph.DrawingGraph.DebugEllipses == null)
#endif
) {
                    DGraph.BuildBBHierarchy();
                    bBNode = DGraph.BbNode;
                }

            } else
                g.FillRectangle(Brushes.Gray, this.ClientRectangle);


            g.Transform.Reset();
        }


        static internal System.Drawing.Rectangle RectFromPoints(System.Drawing.Point p1, System.Drawing.Point p2) {
            return new System.Drawing.Rectangle(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y),
              Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.String)")]
        internal void Zoom(double cx, double cy, double val) {
            if (HLargeChange == 1 || VLargeChange == 1) {
                MessageBox.Show("Cannot zoom in anymore");
                return;
            }

            ZoomF = val;

            double hw = ScaleFromScrollToSrcX(HLargeChange);
            HVal = ScaleFromSrcXToScroll(cx - hw * 0.5f - OriginalGraph.Left);
            hw = ScaleFromScrollToSrcY(VLargeChange);
            VVal = ScaleFromSrcYToScroll(cy - hw * 0.5f - OriginalGraph.Height - OriginalGraph.Bottom);
            panel.Invalidate();
        }

        bool buildHitTree = true;
        /// <summary>
        /// support for mouse selection 
        /// </summary>
        public bool BuildHitTree {
            get { return buildHitTree; }
            set { buildHitTree = value; }
        }

        bool wasMinimized;
        /// <summary>
        /// Handle Resize event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            Control f1 = this.Parent;
            while (true) {
                if (f1 == null)
                    break;
                if (f1 is Form)
                    break;
                f1 = f1.Parent;
            }

            Form ff = f1 as Form;
            if (ff != null) {
                if (ff.WindowState == FormWindowState.Minimized) {
                    wasMinimized = true;
                }
            }

            if (wasMinimized) {
                return;
            }

            InitScale();
            ZoomF = ZoomF;
            if (panel != null)
                panel.Invalidate();
        }

        DGraph dGraph;

        internal DGraph DGraph {
            get { return dGraph; }
            set { dGraph = value; }
        }

        Graph originalGraph;

        internal Graph OriginalGraph {
            get { return originalGraph; }
            set {
                originalGraph = value;
                //this.nodeDragger = new DrawingNodeDragger(originalGraph);
            }
        }

        bool needToCalculateLayout = true;
        /// <summary>
        /// If set to false no layout is calculated. It is presumed that the layout is precalculated.
        /// </summary>
        public bool NeedToCalculateLayout {
            get { return needToCalculateLayout; }
            set { needToCalculateLayout = value; }
        }

        bool asyncLayout;


        /// <summary>
        /// Setting the Graph property shows the graph in the control
        /// </summary>
        /// 
        public Graph Graph {
            get { return OriginalGraph; }
            set {
                if (value != null) {
                    if (!asyncLayout) {
                        OriginalGraph = value;
                        
                        DGraph = null;
                        if (NeedToCalculateLayout) {
                            OriginalGraph.GeometryGraph = null;
                            LayoutAndCreateDGraph();
                        } else {
                            DGraph = DGraph.CreateDGraphFromPrecalculatedDrawingGraph(OriginalGraph);
                        }
                        InitiateDrawing();
                    } else {
                        SetGraphAsync(value);
                    }
                } else {
                    OriginalGraph = null;
                    DGraph = null;
                    this.DrawingPanel.Invalidate();
                }
                if (this.GraphChanged != null)
                    this.GraphChanged(this, null);
            }
        }

       

        #region Asynchronous Layout

        // wrwg: added this for asynchronous layouting




        /// <summary>
        /// An event which can be subscribed to get notification of layout progress.
        /// </summary>
        public event EventHandler<LayoutProgressEventArgs> AsyncLayoutProgress;

        // The thread running the layout process
        System.Threading.Thread layoutThread;

        // A wait handle for ensuring layouting has started
        System.Threading.EventWaitHandle layoutWaitHandle =
            new System.Threading.EventWaitHandle(false,
                                                 System.Threading.EventResetMode.AutoReset);

        /// <summary>
        /// Whether asynchronous layouting is enabled. Defaults to false.
        /// </summary>
        /// <remarks>
        /// If you set this property to true, setting the <see cref="Graph"/> property
        /// will work asynchronously by starting a thread which does the layout and 
        /// displaying. The coarse progress of layouting can be observed with 
        /// <see cref="AsyncLayoutProgress"/>, and layouting can be aborted with 
        /// <see cref="AbortAsyncLayout"/>.
        /// </remarks>
        public bool AsyncLayout {
            get { return asyncLayout; }
            set { asyncLayout = value; }
        }

        /// <summary>
        /// Abort an asynchronous layout activity.
        /// </summary>
        public void AbortAsyncLayout() {
            if (layoutThread != null) {
                layoutThread.Abort();
                layoutThread = null;
            }
        }

        // Is called from Graph setter.
        void SetGraphAsync(Graph value) {

            if (layoutThread != null) {
                layoutThread.Abort();
                layoutThread = null;
            }
            layoutThread = new System.Threading.Thread(
                delegate() {
                    LayoutProgressEventArgs args = new LayoutProgressEventArgs(LayoutProgress.LayingOut, null);
                    lock (value) {
                        try {
                            layoutWaitHandle.Set();
                            OriginalGraph = value;
                            if (NeedToCalculateLayout) {
                                if (AsyncLayoutProgress != null)
                                    AsyncLayoutProgress(this, args);
                                LayoutAndCreateDGraph();
                            } else {
                                DGraph = new DGraph(OriginalGraph);
                            }
                            this.Invoke(
                                (Invoker)
                                delegate() {
                                    if (AsyncLayoutProgress != null) {
                                        args.progress = LayoutProgress.Rendering;
                                        AsyncLayoutProgress(this, args);
                                    }
                                    InitiateDrawing();
                                    if (AsyncLayoutProgress != null) {
                                        args.progress = LayoutProgress.Finished;
                                        AsyncLayoutProgress(this, args);
                                    }
                                });
                        } catch (System.Threading.ThreadAbortException) {
                            if (AsyncLayoutProgress != null) {
                                args.progress = LayoutProgress.Aborted;
                                AsyncLayoutProgress(this, args);
                            }
                            // rethrown automatically
                        } 
                        //catch (Exception e) {
                        //    // must not leak through any exception, otherwise appl. terminates
                        //    if (AsyncLayoutProgress != null) {
                        //        args.progress = LayoutProgress.Aborted;
                        //        args.diagnostics = e.ToString();
                        //        AsyncLayoutProgress(this, args);
                        //    }
                        //}
                        layoutThread = null;
                    }
                });
            // Before we start the thread, ensure the control is created.
            // Otherwise Invoke inside of the thread might fail.
            this.CreateControl();
            layoutThread.Start();
            // Wait until the layout thread has started.
            // If we don't do this, there is a chance that the thread is aborted
            // before we are in the try-context which catches the AbortException, 
            // and we wouldn't get the abortion notification.
            layoutWaitHandle.WaitOne();
        }

        delegate void Invoker();


        #endregion

        private void InitiateDrawing() {
            InitScale();

            VLargeChange = HLargeChange = scrollMax;

            SizeScrollBars();

            ZoomF = 1;

            CalcRects();

            this.bBNode = null;//to initiate new calculation

            panel.Invalidate();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void LayoutAndCreateDGraph() {
#if DEBUGGLEE
            if (System.Environment.GetEnvironmentVariable("debugglee") == "on") {
                Stream stream = File.Open("c:/tmp/currentGraph", FileMode.Create);
                BinaryFormatter bformatter = new BinaryFormatter();
                try {
                    Console.WriteLine("Writing current graph");
                    bformatter.Serialize(stream, Graph);
                } catch (System.Runtime.Serialization.SerializationException e) {
                    Console.WriteLine(e.ToString());
                    MessageBox.Show(e.ToString());
                }
                stream.Close();
            }

#endif

            bool needOpaqueColorsForNode = false;
            switch (this.CurrentLayoutMethod) {
                case LayoutMethod.SugiyamaScheme:
                    if (!(OriginalGraph.LayoutAlgorithmSettings is SugiyamaLayoutSettings))
                        OriginalGraph.LayoutAlgorithmSettings = sugiyamaSettings;
                    break;
                case LayoutMethod.MDS:
                    needOpaqueColorsForNode = true;
                    if (!(OriginalGraph.LayoutAlgorithmSettings is Microsoft.Msagl.Mds.MdsLayoutSettings))
                        OriginalGraph.LayoutAlgorithmSettings = mdsLayoutSettings;
                    break;
                case LayoutMethod.Ranking:
                    needOpaqueColorsForNode = true;
                    if (!(OriginalGraph.LayoutAlgorithmSettings is Microsoft.Msagl.Mds.RankingLayoutSettings))
                        OriginalGraph.LayoutAlgorithmSettings = rankingSettings;
                    break;
            }

            OriginalGraph.CreateGeometryGraph();
            GeometryGraph gleeGraph = OriginalGraph.GeometryGraph;
            if (needOpaqueColorsForNode)
                MakeNodesOpaque();
            DGraph = DGraph.CreateDGraphAndGeometryInfo(OriginalGraph, gleeGraph);
            gleeGraph.CalculateLayout();
            TransferGeometryFromMsaglGraphToGraph(gleeGraph);
        }

        private void MakeNodesOpaque() {
            Microsoft.Msagl.Drawing.Color defaultColor=CreateDefaultOpaqueColor();
            foreach (Microsoft.Msagl.Drawing.Node node in this.OriginalGraph.NodeMap.Values)
                if (NodeFillcolorIsNotOpaque(node))
                    node.Attr.FillColor = defaultColor;
            
        }

        static bool NodeFillcolorIsNotOpaque(Microsoft.Msagl.Drawing.Node node) {
            return node.Attr.FillColor.A == 0;
        }

        private Microsoft.Msagl.Drawing.Color CreateDefaultOpaqueColor() {
            Microsoft.Msagl.Drawing.Color color = this.OriginalGraph.Attr.BackgroundColor;
            if (color.A == 0)
                color.A = byte.MaxValue;
            return color;
        }
        /// <summary>
        /// 
        /// </summary>
        public void ClearBoundingBoxHierarchy() {
            bBNode = null;
        }

        private BBNode bBNode;

        internal BBNode BBNode {
            get {
                if (bBNode == null) {
                    DGraph.BuildBBHierarchy();
                    bBNode = DGraph.BbNode;
                }
                return bBNode;
            }
            set { bBNode = value; }
        }
        /// <summary>
        /// Brings in to the view the object of the group
        /// </summary>
        /// <param name="graphElements"></param>
        public void ShowGroup(object[] graphElements) {
            BBox bb = BBoxOfObjs(graphElements);

            ShowBBox(bb);
        }

        /// <summary>
        /// Changes the view in a way that the group is at the center
        /// </summary>
        /// <param name="graphElements"></param>
        public void CenterToGroup(params object[] graphElements) {
            BBox bb = BBoxOfObjs(graphElements);

            if (!bb.IsEmpty)
                CenterToPoint(0.5f * (bb.LeftTop + bb.RightBottom));
        }


        static BBox BBoxOfObjs(object[] objs) {
            BBox bb = new BBox(0, 0, 0, 0);
            bool boxIsEmpty = true;

            foreach (object o in objs) {
                Microsoft.Msagl.Drawing.Node node = o as Microsoft.Msagl.Drawing.Node;
                BBox objectBB = new BBox(0, 0, 0, 0);
                if (node != null)
                    objectBB = node.BoundingBox;
                else {
                    Microsoft.Msagl.Drawing.Edge edge = o as Microsoft.Msagl.Drawing.Edge;
                    if (edge != null)
                        objectBB = edge.BoundingBox;
                    else
                        continue;
                }

                if (boxIsEmpty) {
                    bb = objectBB;
                    boxIsEmpty = false;
                } else
                    bb.Add(objectBB);
            }


            return bb;
        }

        /// <summary>
        /// Make the bounding box fully visible
        /// </summary>
        /// <param name="bb"></param>
        public void ShowBBox(BBox bb) {
            if (bb.IsEmpty == false) {

                double sc = Math.Min(this.OriginalGraph.Width / bb.Width,
                  this.OriginalGraph.Height / bb.Height);

                P2 center = 0.5 * (bb.LeftTop + bb.RightBottom);

                Zoom(center.X, center.Y, sc);

            }
        }

        /// <summary>
        /// Pans the view by vector (x,y)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        public void Pan(double x, double y) {
            HVal += ScaleFromSrcXToScroll(x);
            VVal += ScaleFromSrcYToScroll(y);
            panel.Invalidate();
        }

        /// <summary>
        /// Pans the view by vector point
        /// </summary>
        /// <param name="point"></param>
        public void Pan(P2 point) {
            Pan(point.X, point.Y);
        }
        /// <summary>
        /// centers the view to the point (x,y)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        public void CenterToPoint(double x, double y) {
            Zoom(x, y, ZoomF);
            //int cx = ScaleFromSrcXToScroll(x);
            //int cy = ScaleFromSrcYToScroll(y);

            //int curCx = (int)(hVal + hLargeChangeF * 0.5f + 0.5f);
            //int curCy = (int)(vVal + vLargeChangeF * 0.5f + 0.5f);


            //hVal += cx - curCx;
            //vVal += cy - curCy;

            //panel.Invalidate();
        }
        /// <summary>
        /// Centers the view to the point p
        /// </summary>
        /// <param name="point"></param>
        public void CenterToPoint(P2 point) {
            CenterToPoint(point.X, point.Y);
        }
        /// <summary>
        /// Finds the object under point (x,y) where x,y are given in the window coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        public object GetObjectAt(int x, int y) {
            Geometry g = this.BBNode.Hit(ScreenToSource(new System.Drawing.Point(x, y)), GetHitSlack());
            if (g == null)
                return null;

            return g.tag;

        }
        /// <summary>
        /// Finds the object under point p where p is given in the window coordinates
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public object GetObjectAt(System.Drawing.Point point) {
            return GetObjectAt(point.X, point.Y);
        }
        /// <summary>
        /// Zooms in
        /// </summary>
        public void ZoomInPressed() {
            ZoomF *= ZoomFactor();
        }
        /// <summary>
        /// Zooms out
        /// </summary>
        public void ZoomOutPressed() {
            ZoomF /= ZoomFactor();
        }

        double ZoomFactor() { return 1.0f + zoomFraction; }

        private void toolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e) {
            if (e.Button == this.zoomin)
                ZoomInPressed();
            else if (e.Button == this.zoomout)
                ZoomOutPressed();
            else if (e.Button == this.backwardButton)
                BackwardButtonPressed();
            else if (e.Button == this.forwardButton)
                ForwardButtonPressed();
            else if (e.Button == this.saveButton)
                SaveButtonPressed();
            else if (e.Button == this.print)
                PrintButtonPressed();
            else if (e.Button == this.openButton)
                OpenButtonPressed();
            else if (e.Button == this.undoButton)
                UndoButtonPressed();
            else if (e.Button == this.redoButton)
                RedoButtonPressed();
            else if (e.Button == this.windowZoomButton)
                WindowZoomButtonIsPressed();
            else if (e.Button == this.panButton)
                PanButtonIsPressed();
            else if (e.Button == this.layoutSettingsButton)
                LayoutSettingsIsClicked();
            
        }

   
        private void LayoutSettingsIsClicked()
        {
               LayoutSettingsForm layoutSettingsForm = new LayoutSettingsForm();
                LayoutSettingsWrapper wrapper = new LayoutSettingsWrapper();
                wrapper.LayoutTypeHasChanged += new EventHandler(OnLayoutTypeChange);
                wrapper.LayoutMethod = CurrentLayoutMethod;
                switch (CurrentLayoutMethod) {
                    case LayoutMethod.SugiyamaScheme:
                        wrapper.LayoutSettings = sugiyamaSettings;
                        break;
                    case LayoutMethod.MDS:
                        wrapper.LayoutSettings = mdsLayoutSettings;
                        break;
                    case LayoutMethod.Ranking:
                        wrapper.LayoutSettings = rankingSettings;
                        break;
                }
                layoutSettingsForm.PropertyGrid.SelectedObject = wrapper;
                LayoutAlgorithmSettings backup=this.Graph!=null?this.Graph.LayoutAlgorithmSettings.Clone():null;
                if (layoutSettingsForm.ShowDialog() == DialogResult.OK)
                    this.Graph = Graph; //recalculate the layout
                else
                    if (this.Graph != null)
                        this.Graph.LayoutAlgorithmSettings = backup;
                   
         
        }

        

       

        void OnLayoutTypeChange(object o, EventArgs args) {
            LayoutSettingsWrapper wrapper = o as LayoutSettingsWrapper;
            switch (wrapper.LayoutMethod) {
                case LayoutMethod.SugiyamaScheme:
                    wrapper.LayoutSettings = sugiyamaSettings;
                    break;
                case LayoutMethod.MDS:
                    wrapper.LayoutSettings = mdsLayoutSettings;
                    break;
                case LayoutMethod.Ranking:
                    wrapper.LayoutSettings = rankingSettings;
                    break;
                case LayoutMethod.UseSettingsOfTheGraph:
                    if (Graph != null)
                        wrapper.LayoutSettings = this.Graph.LayoutAlgorithmSettings;
                    else
                        wrapper.LayoutSettings = null;
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);//cannot be here
                    break;
            }
            this.CurrentLayoutMethod = wrapper.LayoutMethod;
            if (Graph != null)
                this.Graph.LayoutAlgorithmSettings = wrapper.LayoutSettings;
        }

        string windowZoomButtonToolTipText = "Zoom in by dragging a rectangle";
        string windowZoomButtonDisabledToolTipText = "Zoom in by dragging a rectangle, is disabled now";
        string panButtonToolTipText = "Pan";
        string panButtonDisabledToolTipText = "Pan, is disabled now";

        private void PanButtonIsPressed() {
            if (panButton.Pushed) {
                panButton.ToolTipText = panButtonToolTipText;
                windowZoomButton.Pushed = false;
                windowZoomButton.ToolTipText = windowZoomButtonDisabledToolTipText;
            } else 
                panButton.ToolTipText = panButtonToolTipText;
        }

        private void WindowZoomButtonIsPressed() {
            if (windowZoomButton.Pushed) {
                windowZoomButton.ToolTipText = windowZoomButtonToolTipText;
                panButton.Pushed = false;
                panButton.ToolTipText = panButtonDisabledToolTipText;
            } else
                windowZoomButton.ToolTipText = windowZoomButtonToolTipText;
        }

        private void RedoButtonPressed() {
            if (this.DrawingLayoutEditor != null && this.DrawingLayoutEditor.CanRedo)
                DrawingLayoutEditor.Redo();
        }

        private void UndoButtonPressed() {
            if (this.DrawingLayoutEditor != null && this.DrawingLayoutEditor.CanUndo)
                DrawingLayoutEditor.Undo();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void OpenButtonPressed() {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = "MSAGL Files(*.msagl)|*.msagl"; ;
            try {
                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    this.NeedToCalculateLayout = false;
                    this.Graph = Graph.Read(openFileDialog.FileName);
                }
            } catch (Exception e) {
                MessageBox.Show(e.Message);
                throw;
            } finally {
                this.NeedToCalculateLayout = true;
            }

        }
        /// <summary>
        /// Raises a dialog of saving the drawing image to a file
        /// </summary>
        public void SaveButtonPressed() {
            if (Graph == null) {
                return;
            }

            ContextMenu contextMenu = new ContextMenu(CreateSaveMenuItems());
            contextMenu.Show(this, new System.Drawing.Point(this.toolbar.Left + 100, toolbar.Bottom), LeftRightAlignment.Right);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.Windows.Forms.MenuItem.#ctor(System.String)")]
        private MenuItem[] CreateSaveMenuItems() {
            List<MenuItem> menuItems = new List<MenuItem>();
            MenuItem menuItem;
            if (SaveAsMsaglEnabled) {
                menuItems.Add(menuItem = new MenuItem("Save graph"));
                menuItem.Click += new EventHandler(saveGraph_click);
            }
            if (SaveAsImageEnabled) {
                menuItems.Add(menuItem = new MenuItem("Save in bitmap format"));
                menuItem.Click += new EventHandler(saveImage_click);
            }
            if (SaveInVectorFormatEnabled) {
                menuItems.Add(menuItem = new MenuItem("Save in vector format"));
                menuItem.Click += new EventHandler(saveInVectorGraphicsFormat_click);
            }
            return menuItems.ToArray();
        }

        void saveInVectorGraphicsFormat_click(object sender, EventArgs e) {
            SaveInVectorFormatForm saveForm = new SaveInVectorFormatForm(this);
            saveForm.ShowDialog();

        }

        void saveImage_click(object sender, EventArgs e) {
            SaveViewAsImageForm saveViewForm = new SaveViewAsImageForm(this);
            saveViewForm.ShowDialog();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        void saveGraph_click(object sender, EventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "MSAGL Files(*.msagl)|*.msagl";
            try {
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    if (this.GraphSavingStarted != null)
                        this.GraphSavingStarted(this, null);
                    this.Graph.Write(saveFileDialog.FileName);
                    if (this.GraphSavingEnded != null)
                        this.GraphSavingEnded(this, null);
                }

            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Navigates forward in the view history
        /// </summary>
        public void ForwardButtonPressed() {
            if (listOfViewInfos.ForwardAvailable)
                if (listOfViewInfos.CurrentView.leftMouseButtonWasPressed) {
                    while (listOfViewInfos.ForwardAvailable) {
                        listOfViewInfos.Forward();
                        this.SetViewFromViewInfo(listOfViewInfos.CurrentView);

                        if (listOfViewInfos.CurrentView.leftMouseButtonWasPressed == false)
                            break;

                    }
                } else {
                    listOfViewInfos.Forward();
                    this.SetViewFromViewInfo(listOfViewInfos.CurrentView);
                    /*
                              if(listOfViewInfos.CurrentView.leftMouseButtonWasPressed) {
                                  while( listOfViewInfos.ForwardAvailable ) {
                                      listOfViewInfos.Forward();
                                      this.SetViewFromViewInfo(listOfViewInfos.CurrentView);
							
                                      if(listOfViewInfos.CurrentView.leftMouseButtonWasPressed==false)
                                          break;
												
                                  }
                              }
                              */

                }

        }
        /// <summary>
        /// Navigates backward in the view history
        /// </summary>
        public void BackwardButtonPressed() {
            if (listOfViewInfos.BackwardAvailable)
                if (listOfViewInfos.CurrentView.leftMouseButtonWasPressed) {
                    while (listOfViewInfos.BackwardAvailable) {
                        listOfViewInfos.Backward();
                        this.SetViewFromViewInfo(listOfViewInfos.CurrentView);

                        if (listOfViewInfos.CurrentView.leftMouseButtonWasPressed == false)
                            break;

                    }
                } else {
                    listOfViewInfos.Backward();
                    this.SetViewFromViewInfo(listOfViewInfos.CurrentView);
                }

        }

        /// <summary>
        /// Prints the graph.
        /// </summary>
        public void PrintButtonPressed() {
            GraphPrinting p = new GraphPrinting(this);
            PrintDialog pd = new PrintDialog();
            pd.Document = p;
            if (pd.ShowDialog() == DialogResult.OK)
                p.Print();
        }

        /// <summary>
        /// Controls the pan button.
        /// </summary>
        public bool PanButtonPressed {
            get {
                return panButton.Pushed;
            }
            set {
                panButton.Pushed = value;
            }
        }

        private void panel_Click(object sender, EventArgs e) {
            this.OnClick(e);
        }

        /// <summary>
        /// Reacts on some pressed keys
        /// </summary>
        /// <param name="e"></param>
        public void OnKey(KeyEventArgs e) {
            if (e == null)
                return;
            if (e.KeyData == (Keys)262181) {
                if (this.backwardButton.Enabled) {
                    this.BackwardButtonPressed();
                }
            } else if (e.KeyData == (Keys)262183)//key==Keys.BrowserForward)
      {
                if (this.forwardButton.Enabled)
                    this.ForwardButtonPressed();
            }

        }


        static void TransferGeometryFromMsaglGraphToGraph(GeometryGraph gleeGraph) {

            foreach (GeometryEdge gleeEdge in gleeGraph.Edges) {
                DrawingEdge drawingEdge = gleeEdge.UserData as DrawingEdge;
                drawingEdge.Attr.GeometryEdge = gleeEdge;
            }

            foreach (GeometryNode gleeNode in gleeGraph.NodeMap.Values) {
                DrawingNode drawingNode = gleeNode.UserData as DrawingNode;
                drawingNode.Attr.GeometryNode = gleeNode;
            }
        }

        double zoomWindowThreshold = 0.05; //inches
        /// <summary>
        /// If the mininal side of the zoom window is shorter than the threshold then zoom 
        /// does not take place
        /// </summary>
        public double ZoomWindowThreshold {
            get { return zoomWindowThreshold; }
            set { zoomWindowThreshold = value; }
        }

        double mouseHitDistance = 0.05;
        /// <summary>
        /// SelectedObject can be detected if the distance in inches between it and 
        /// the cursor is less than MouseHitDistance
        /// </summary>
        public double MouseHitDistance {
            get { return mouseHitDistance; }
            set { mouseHitDistance = value; }
        }



        #region IGraphViewer Members
        /// <summary>
        /// calcualates the layout and returns the object ready to be drawn
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public object CalculateLayout(Graph graph) {
            this.OriginalGraph = graph as Graph;
            LayoutAndCreateDGraph();
            return this.DGraph;
        }
        /// <summary>
        /// sets a tool tip
        /// </summary>
        /// <param name="toolTip"></param>
        /// <param name="tip"></param>
        public void SetToolTip(ToolTip toolTip, string tip) {
            if (toolTip != null)
                toolTip.SetToolTip(DrawingPanel, tip);
        }

        /// <summary>
        /// Just uses the passed object to draw the graph. The method expects DGraph as the argument
        /// </summary>
        /// <param name="entityContainingLayout"></param>
        public void SetCalculatedLayout(object entityContainingLayout) {
            ClearLayoutEditor();
            this.DGraph = entityContainingLayout as DGraph;
            if (this.DGraph != null) {
                this.OriginalGraph = DGraph.DrawingGraph;
                if (this.GraphChanged != null)
                    this.GraphChanged(this, null);
                InitiateDrawing();
            }
        }
        /// <summary>
        /// Returns layouted Microsoft.Msagl.Drawing.Graph
        /// </summary>
        public Graph GraphWithLayout {
            get { return DGraph.DrawingGraph; }
        }

        /// <summary>
        /// maps screen coordinates to viewer coordinates
        /// </summary>
        /// <param name="screenX"></param>
        /// <param name="screenY"></param>
        /// <param name="viewerX"></param>
        /// <param name="viewerY"></param>
        public void ScreenToSource(float screenX, float screenY, out float viewerX, out float viewerY) {
            P2 p = ScreenToSource(new System.Drawing.Point((int)screenX, (int)screenY));
            viewerX = (float)p.X;
            viewerY = (float)p.Y;
        }
        /// <summary>
        /// pans the drawing on deltaX, deltaY in the drawing coords
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        public void Pan(float deltaX, float deltaY) {
            Pan(new Microsoft.Msagl.Point(deltaX, deltaY));
        }

        #endregion



        static bool Intersect(Geometry geometry, ref Microsoft.Msagl.Splines.Rectangle box) {
            //here we know that the box of "geometry" and "box" do intersect.
            Line line = geometry as Line;
            if (line != null)
                return Intersect(line, ref box);
            return true;
        }

        static bool Intersect(Line line, ref Microsoft.Msagl.Splines.Rectangle box) {
            P2 d = line.end - line.start;
            double min, max;
            double s = d * line.start;
            double f = d * line.end;
            System.Diagnostics.Debug.Assert(s <= f);
            ProjectBoxOnDir(d, ref box, out min, out max);
            if (min > f)
                return false;
            if (max < s)
                return false;

            d = d.Rotate(Math.PI / 2);
            ProjectBoxOnDir(d, ref box, out min, out max);

            f = d * line.start;
            if (min > f)
                return false;
            if (max < f)
                return false;

            return true;

        }

        static void ProjectBoxOnDir(Microsoft.Msagl.Point d, ref Microsoft.Msagl.Splines.Rectangle box, out double min, out double max) {
            min = max = box.LeftTop * d;
            UpdateMinMax(ref min, ref max, box.RightTop * d);
            UpdateMinMax(ref min, ref max, box.LeftBottom * d);
            UpdateMinMax(ref min, ref max, box.RightBottom * d);
        }

        private static void UpdateMinMax(ref double min, ref double max, double f) {
            if (f < min)
                min = f;
            else if (f > max)
                max = f;
        }


        #region IEditViewer Members

        /// <summary>
        /// returns the object under the cursor
        /// </summary>
        public Microsoft.Msagl.Drawing.IViewerObject ObjectUnderMouseCursor {
            get {
                if (MousePositonWhenSetSelectedObject != MousePosition) 
                    UnconditionalHit(null);
                return selectedDObject;
            }
        }

        //Microsoft.Msagl.Drawing.DrawingObject DObjectToDrawingObject(DObject drObj) {
        //    return this.DGraph.drawingObjectsToDObjects[drObj as DrawingObject] as Microsoft.Msagl.Drawing.IDraggableObject;            
        //}

        /// <summary>
        /// The radius of a circle around a underlying polyline corner
        /// </summary>
        public double UnderlyingPolylineCircleRadius {
            get { return this.DpiX * 0.05 / this.CurrentScale; }
        }

        /// <summary>
        /// Forces redraw of draggableObject
        /// </summary>
        /// <param name="editObj"></param>
        public void Invalidate(Microsoft.Msagl.Drawing.IViewerObject editObj) {
            this.panel.Invalidate(CreateRectangleToInvalidateAndForceEdgeGeometryPathRecalculation(editObj));
        }

        private System.Drawing.Rectangle CreateRectangleToInvalidateAndForceEdgeGeometryPathRecalculation(Microsoft.Msagl.Drawing.IViewerObject editObj) {
            DNode dNode = editObj as DNode;
            if (dNode != null)
                return CreateRectangleForNode(dNode.Node);
            DEdge dEdge = editObj as DEdge;
            if (dEdge != null) {
                dEdge.GraphicsPath = null;
                return CreateRectangleForEdge(dEdge);
            } else {
                DLabel edgeLabel = editObj as DLabel;
                if (edgeLabel != null) {
                    return CreateRectangleForLabel(edgeLabel);
                } else return CreateRectangleForDGraph(editObj as DGraph);
            }
        }

        private System.Drawing.Rectangle CreateRectangleForDGraph(DGraph dGraphParameter) {
            return CreateScreenRectFromTwoCornersInTheSource(
                dGraphParameter.DrawingGraph.BoundingBox.LeftTop,
                dGraphParameter.DrawingGraph.BoundingBox.RightBottom);
        }

        private System.Drawing.Rectangle CreateRectangleForLabel(DLabel edgeLabel) {
            Microsoft.Msagl.Splines.Rectangle box = edgeLabel.DrawingLabel.BoundingBox;
            if (edgeLabel.MarkedForDragging) {
                P2 delta = new Microsoft.Msagl.Point(1, 1);
                box.Add(edgeLabel.DrawingLabel.GeometryLabel.AttachmentSegmentEnd + delta);
                box.Add(edgeLabel.DrawingLabel.GeometryLabel.AttachmentSegmentEnd - delta);
            }
            return CreateScreenRectFromTwoCornersInTheSource(
                box.LeftTop,
                box.RightBottom);
        }
        /// <summary>
        /// return ModifierKeys
        /// </summary>
        ModifierKeys IViewer.ModifierKeys {
            get {
                switch (ModifierKeys) {
                    case Keys.Control:
                    case Keys.ControlKey: return Microsoft.Msagl.Drawing.ModifierKeys.Control;
                    case Keys.Shift:
                    case Keys.ShiftKey: return Microsoft.Msagl.Drawing.ModifierKeys.Shift;
                    case Keys.Alt: return Microsoft.Msagl.Drawing.ModifierKeys.Alt;
                    default: return Microsoft.Msagl.Drawing.ModifierKeys.None;
                }
            }
        }

        /// <summary>
        /// Maps a screen point to the graph surface point
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Microsoft.Msagl.Point ScreenToSource(MsaglMouseEventArgs e) {
            if (e != null)
                return ScreenToSource(e.X, e.Y);
            return new Microsoft.Msagl.Point();
        }
        /// <summary>
        /// enumerates over all draggable entities
        /// </summary>
        public IEnumerable<Microsoft.Msagl.Drawing.IViewerObject> Entities {
            get {
                if (this.DGraph != null) {
                    foreach (Microsoft.Msagl.Drawing.IViewerObject obj in this.DGraph.Entities)
                        yield return obj;
                }
            }
        }
        /// <summary>
        /// number of dots per inch horizontally
        /// </summary>
        public double DpiX {
            get { return dpix; }
        }
        /// <summary>
        /// number of dots per inch vertically
        /// </summary>
        public double DpiY {
            get { return dpiy; }
        }

        #endregion

        #region IEditViewer explicit Members
        EventHandler<MsaglMouseEventArgs> iEditViewerMouseDown;
        event EventHandler<MsaglMouseEventArgs> IViewer.MouseDown {
            add { iEditViewerMouseDown += value; }
            remove { iEditViewerMouseDown -= value; }
        }

        EventHandler<MsaglMouseEventArgs> iEditViewerMouseMove;

        event EventHandler<MsaglMouseEventArgs> IViewer.MouseMove {
            add { iEditViewerMouseMove += value; }
            remove { iEditViewerMouseMove -= value; }
        }

        EventHandler<MsaglMouseEventArgs> iEditViewerMouseUp;
        event EventHandler<MsaglMouseEventArgs> IViewer.MouseUp {
            add { this.iEditViewerMouseUp += value; }
            remove { this.iEditViewerMouseUp -= value; }
        }

     
        #endregion

        internal void RaiseMouseMoveEvent(MsaglMouseEventArgs iArgs) {
            if (this.iEditViewerMouseMove != null)
                iEditViewerMouseMove(this, iArgs);
        }


        internal void RaiseMouseDownEvent(MsaglMouseEventArgs iArgs) {
            if (this.iEditViewerMouseDown != null)
                iEditViewerMouseDown(this, iArgs);

        }

        internal void RaiseMouseUpEvent(MsaglMouseEventArgs iArgs) {
            if (this.iEditViewerMouseUp != null)
                iEditViewerMouseUp(this, iArgs);
        }


        /// <summary>
        /// A method of IEditViewer
        /// </summary>
        /// <param name="changedObjects"></param>
        public void OnDragEnd(IEnumerable<Microsoft.Msagl.Drawing.IViewerObject> changedObjects) {
            this.DGraph.UpdateBBoxHierarchy(changedObjects);
        }

        private IEnumerable<DrawingObject> GetDrawingObjs(IEnumerable<Microsoft.Msagl.Drawing.IViewerObject> changedObjects) {
            foreach (Microsoft.Msagl.Drawing.IViewerObject d in changedObjects)
                yield return d.DrawingObject;
        }


        /// <summary>
        /// Forces redraw of the object
        /// </summary>
        /// <param name="editObj"></param>
        public void InvalidateBeforeTheChange(Microsoft.Msagl.Drawing.IViewerObject editObj) {
            this.Invalidate(editObj);
        }

        void IViewer.Invalidate() {
            this.ZoomF = ZoomF;
        }


        #region IEditViewer Members

        /// <summary>
        /// This event is raised before the file saving
        /// </summary>
        public event EventHandler GraphSavingStarted;

        /// <summary>
        /// This even is raised after graph saving
        /// </summary>
        public event EventHandler GraphSavingEnded;

        double visibleWidth = 0.05; //inches

        /// <summary>
        /// The scale dependent width of an edited curve that should be clearly visible.
        /// Used in the default entity editing.
        /// </summary>
        public double LineThicknessForEditing {
            get { return this.DpiX * visibleWidth / this.CurrentScale; }
        }

       
     
        /// <summary>
        /// Undoes the last edit action
        /// </summary>
        public void Undo() {
            if (this.DrawingLayoutEditor.CanUndo)
                this.DrawingLayoutEditor.Undo();
        }

        /// <summary>
        /// redoes the last undo
        /// </summary>
        public void Redo() {
            if (this.DrawingLayoutEditor.CanRedo)
                this.DrawingLayoutEditor.Redo();
        }

   
        /// <summary>
        /// returns true if an undo is available
        /// </summary>
        /// <returns></returns>
        public bool CanUndo() {
            return DrawingLayoutEditor.CanUndo;
        }

        /// <summary>
        /// returns true is a redo is available
        /// </summary>
        /// <returns></returns>
        public bool CanRedo() {
            return DrawingLayoutEditor.CanRedo;
        }

   

       /// <summary>
        /// Pops up a pop up menu with a menu item for each couple, the string is the title and the delegate is the callback
        /// </summary>
        /// <param name="menuItems"></param>
        public void PopupMenus( params Couple<string,DelegateReturningVoid>[] menuItems){
            ContextMenu contextMenu = new ContextMenu();
            foreach(Couple<string,DelegateReturningVoid> menuItem in menuItems)
                contextMenu.MenuItems.Add(CreateMenuItem(menuItem.First, menuItem.Second));
            contextMenu.Show(this, PointToClient(Control.MousePosition));
        }

        static MenuItem CreateMenuItem(string title, DelegateReturningVoid voidDelegate) {
            MenuItem menuItem = new MenuItem();
            menuItem.Text = title;
            menuItem.Click+= delegate(object sender, EventArgs e){voidDelegate();};
            return menuItem;
        }


        #endregion

        #region IViewer Members

        /// <summary>
        /// adding a node to the graph with the undo support
        /// The node boundary curve should have (0,0) as its internal point.
        /// The curve will be moved the the node center.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="registerForUndo"></param>
      
        public void AddNode(Microsoft.Msagl.Drawing.IViewerNode node, bool registerForUndo) {
            DNode dNode=node as DNode;
            DrawingNode drawingNode = dNode.DrawingNode;
           
            IViewer viewer = this as IViewer;
            
            this.DGraph.NodeMap[drawingNode.Id] = dNode;
            this.Graph.AddNode(drawingNode);
            this.Graph.GeometryGraph.AddNode(drawingNode.Attr.GeometryNode);
            
            foreach (DEdge e in dNode.outEdges) {
                e.Target.inEdges.Add(e);
                e.Target.DrawingNode.AddInEdge(e.DrawingEdge);
                e.Target.DrawingNode.Attr.GeometryNode.AddInEdge(e.DrawingEdge.Attr.GeometryEdge);
            }
            foreach (DEdge e in dNode.inEdges) {
                e.Source.outEdges.Add(e);
                e.Source.DrawingNode.AddOutEdge(e.DrawingEdge);
                e.Source.DrawingNode.Attr.GeometryNode.AddOutEdge(e.DrawingEdge.Attr.GeometryEdge);
            }

            viewer.Invalidate(node);
            foreach (DEdge e in Edges(dNode)) {
                DGraph.Edges.Add(e);
                Graph.Edges.Add(e.DrawingEdge);
                Graph.GeometryGraph.Edges.Add(e.DrawingEdge.Attr.GeometryEdge);
                viewer.Invalidate(e);
            }

            if (registerForUndo) {
                drawingLayoutEditor.RegisterNodeAdditionForUndo(node);
                this.Graph.GeometryGraph.ExtendBoundingBox(drawingNode.BoundingBox.LeftTop);
                this.Graph.GeometryGraph.ExtendBoundingBox(drawingNode.BoundingBox.RightBottom);
                drawingLayoutEditor.CurrentAction.GraphBoundingBoxAfter = this.Graph.BoundingBox;
            }
            this.BBNode = null;
            viewer.Invalidate();
        }

       

     
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="source"></param>
        ///// <param name="target"></param>
        ///// <param name="registerForUndo"></param>
        ///// <returns></returns>
        //public Microsoft.Msagl.Drawing.Edge AddEdge(Microsoft.Msagl.Drawing.Node source, Microsoft.Msagl.Drawing.Node target, bool registerForUndo) {
        //    System.Diagnostics.Debug.Assert(this.Graph.FindNode(source.Id) == source);
        //    System.Diagnostics.Debug.Assert(this.Graph.FindNode(target.Id) == target);

        //    Microsoft.Msagl.Drawing.Edge edge = this.Graph.AddEdge(source.Id, target.Id);
        //    edge.Label = new Microsoft.Msagl.Drawing.Label();
        //    Microsoft.Msagl.Edge geometryEdge = edge.Attr.GeometryEdge = new Microsoft.Msagl.Edge();
        //    geometryEdge.Parent = this.Graph.GeometryGraph;

        //    Microsoft.Msagl.Point a = source.Attr.GeometryNode.Center;
        //    Microsoft.Msagl.Point b = target.Attr.GeometryNode.Center;
        //    if (source == target) {
        //        Site start = new Site(a);
        //        Site end = new Site(b);
        //        Microsoft.Msagl.Point mid1 = source.Attr.GeometryNode.Center;
        //        mid1.X += (source.Attr.GeometryNode.BoundingBox.Width / 3 * 2);
        //        Microsoft.Msagl.Point mid2 = mid1;
        //        mid1.Y -= source.Attr.GeometryNode.BoundingBox.Height / 2;
        //        mid2.Y += source.Attr.GeometryNode.BoundingBox.Height / 2;
        //        Site mid1s = new Site(mid1);
        //        Site mid2s = new Site(mid2);
        //        start.Next = mid1s;
        //        mid1s.Previous = start;
        //        mid1s.Next = mid2s;
        //        mid2s.Previous = mid1s;
        //        mid2s.Next = end;
        //        end.Previous = mid2s;
        //        geometryEdge.UnderlyingPolyline = new UnderlyingPolyline(start);
        //        geometryEdge.Curve = geometryEdge.UnderlyingPolyline.CreateCurve();
        //    } else {
        //        Site start = new Site(a);
        //        Site end = new Site(b);
        //        Site mids = new Site(a * 0.5 + b * 0.5);
        //        start.Next = mids;
        //        mids.Previous = start;
        //        mids.Next = end;
        //        end.Previous = mids;
        //        geometryEdge.UnderlyingPolyline = new UnderlyingPolyline(start);
        //        geometryEdge.Curve = geometryEdge.UnderlyingPolyline.CreateCurve();
        //    }

        //    geometryEdge.Source = edge.SourceNode.Attr.GeometryNode;
        //    geometryEdge.Target = edge.TargetNode.Attr.GeometryNode;
        //    geometryEdge.ArrowheadLength = edge.Attr.ArrowheadLength;
        //    if (!Routing.TrimSplineAndCalculateArrowheads(geometryEdge, geometryEdge.Curve, true))
        //        Routing.CreateBigEnoughSpline(geometryEdge);
            
        //    AddEdge(edge, registerForUndo);
        //    return edge;
           
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="registerForUndo"></param>
        public void AddEdge(Microsoft.Msagl.Drawing.IViewerEdge edge, bool registerForUndo) {
            
            
            if (registerForUndo) drawingLayoutEditor.RegisterEdgeAdditionForUndo(edge);

            DEdge dEdge = edge as DEdge;
            DrawingEdge drawingEdge = edge.DrawingObject as DrawingEdge;
            GeometryEdge geomEdge = drawingEdge.Attr.GeometryEdge;

            //the edge has to be disconnected from the graph
            System.Diagnostics.Debug.Assert(this.DGraph.Edges.Contains(dEdge) == false);
            System.Diagnostics.Debug.Assert(this.Graph.Edges.Contains(drawingEdge) == false);
            System.Diagnostics.Debug.Assert(this.Graph.GeometryGraph.Edges.Contains(geomEdge) == false);

            this.DGraph.Edges.Add(dEdge);
            this.Graph.Edges.Add(drawingEdge);
            this.Graph.GeometryGraph.Edges.Add(geomEdge);
            this.Graph.GeometryGraph.ExtendBoundingBox(drawingEdge.Attr.GeometryEdge.Curve.BBox.LeftTop);
            this.Graph.GeometryGraph.ExtendBoundingBox(drawingEdge.Attr.GeometryEdge.Curve.BBox.RightBottom);
            
            if (registerForUndo) drawingLayoutEditor.CurrentAction.GraphBoundingBoxAfter = this.Graph.BoundingBox;

         
            this.BBNode = null;
            DNode source = edge.Source as DNode;
            DNode target = edge.Target as DNode;
            //the edge has to be disconnected from the graph
            System.Diagnostics.Debug.Assert(source.outEdges.Contains(dEdge) == false);
            System.Diagnostics.Debug.Assert(target.inEdges.Contains(dEdge) == false);
            System.Diagnostics.Debug.Assert(source.selfEdges.Contains(dEdge) == false);



            if (source != target) {
                source.AddOutEdge(dEdge);
                target.AddInEdge(dEdge);

                source.DrawingNode.AddOutEdge(drawingEdge);
                target.DrawingNode.AddInEdge(drawingEdge);

                source.DrawingNode.Attr.GeometryNode.AddOutEdge(geomEdge);
                target.DrawingNode.Attr.GeometryNode.AddInEdge(geomEdge);

            } else {
                source.AddSelfEdge(dEdge);
                source.DrawingNode.AddSelfEdge(drawingEdge);
                source.DrawingNode.Attr.GeometryNode.AddSelfEdge(geomEdge);
            }

            this.DGraph.BbNode = null;
            this.DGraph.BuildBBHierarchy();

            this.Invalidate();

        }

        IEnumerable<DEdge> Edges(DNode dNode) {
            foreach (DEdge de in dNode.OutEdges)
                yield return de;
            foreach (DEdge de in dNode.InEdges)
                yield return de; 
            foreach (DEdge de in dNode.SelfEdges)
                yield return de;
        }

        /// <summary>
        /// removes a node from the graph with the undo support
        /// </summary>
        /// <param name="node"></param>
        /// <param name="registerForUndo"></param>
        public void RemoveNode(Microsoft.Msagl.Drawing.IViewerNode node, bool registerForUndo) {
            
            
            if (registerForUndo)
                drawingLayoutEditor.RegisterNodeForRemoval(node);

            RemoveNodeFromAllGraphs(node);
            
            this.BBNode = null;
            this.DGraph.BbNode = null;
            this.DGraph.BuildBBHierarchy();

            this.Invalidate();
        }

        /// <summary>
        /// makes the node unreachable
        /// </summary>
        /// <param name="node"></param>
        private void RemoveNodeFromAllGraphs(Microsoft.Msagl.Drawing.IViewerNode node) {
            DrawingNode drawingNode = node.DrawingObject as DrawingNode;

            this.DGraph.NodeMap.Remove(drawingNode.Id);
            this.Graph.NodeMap.Remove(drawingNode.Id);
            this.Graph.GeometryGraph.NodeMap.Remove(drawingNode.Id);

            foreach (DEdge de in Edges(node as DNode)) {
                this.DGraph.Edges.Remove(de);
                this.Graph.Edges.Remove(de.DrawingEdge);
                this.Graph.GeometryGraph.Edges.Remove(de.DrawingEdge.Attr.GeometryEdge);
            }

            foreach (DEdge de in node.OutEdges){
                de.Target.inEdges.Remove(de);
                de.Target.DrawingNode.RemoveInEdge(de.DrawingEdge);
                de.Target.DrawingNode.Attr.GeometryNode.RemoveInEdge(de.DrawingEdge.Attr.GeometryEdge);
            }

            foreach (DEdge de in node.InEdges){
                de.Source.outEdges.Remove(de);
                de.Source.DrawingNode.RemoveOutEdge(de.DrawingEdge);
                de.Source.DrawingNode.Attr.GeometryNode.RemoveOutEdge(de.DrawingEdge.Attr.GeometryEdge);
            }

        }

        /// <summary>
        /// removes an edge from the graph with the undo support
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="registerForUndo"></param>
        public void RemoveEdge(Microsoft.Msagl.Drawing.IViewerEdge edge, bool registerForUndo) {
            DEdge de = edge as Microsoft.Msagl.GraphViewerGdi.DEdge;

            if (registerForUndo) 
                drawingLayoutEditor.RegisterEdgeRemovalForUndo(edge);
            
            this.Graph.RemoveEdge(de.DrawingEdge);


            this.DGraph.Edges.Remove(de);
            if (de.Source != de.Target) {
                de.Source.RemoveOutEdge(de);
                de.Target.RemoveInEdge(de);
            } else
                de.Source.RemoveSelfEdge(de);


            this.BBNode = null;
            this.DGraph.BbNode = null;
            this.DGraph.BuildBBHierarchy();

            this.Invalidate();
        }

        #endregion

        /// <summary>
        /// Sets the size of the node to something appropriate to the label it has to display.
        /// </summary>
        /// <param name="node">The node to be resized</param>
        public void ResizeNodeToLabel(Microsoft.Msagl.Drawing.Node node) {
            double width = 0;
            double height = 0;
            string label = node.Label.Text;
            if (String.IsNullOrEmpty(label) == false) {
                using (Font f = new Font(node.Label.FontName, node.Label.FontSize)) {
                    StringMeasure.MeasureWithFont(label, f, out width, out height);
                }
                node.Label.Size = new Microsoft.Msagl.Drawing.Size((float)width, (float)height);
                width += 2 * node.Attr.LabelMargin;
                height += 2 * node.Attr.LabelMargin;
                if (width < this.Graph.Attr.MinNodeWidth)
                    width = this.Graph.Attr.MinNodeWidth;
                if (height < this.Graph.Attr.MinNodeHeight)
                    height = this.Graph.Attr.MinNodeHeight;

                Microsoft.Msagl.Point originalCenter = node.Attr.GeometryNode.Center;
                node.Attr.GeometryNode.BoundaryCurve = Microsoft.Msagl.Drawing.NodeBoundaryCurves.GetNodeBoundaryCurve(node, width, height).Translate(originalCenter);
                node.Attr.GeometryNode.Center = originalCenter;

                ArrayList edges = new ArrayList();
                foreach (Microsoft.Msagl.Drawing.Edge e in node.OutEdges)
                    edges.Add(e);
                foreach (Microsoft.Msagl.Drawing.Edge e in node.InEdges)
                    edges.Add(e);
                foreach (Microsoft.Msagl.Drawing.Edge e in node.SelfEdges)
                    edges.Add(e);

                foreach (Microsoft.Msagl.Drawing.Edge e in edges) {
                    Msagl.Edge geometryEdge = e.Attr.GeometryEdge;
                    geometryEdge.Curve = geometryEdge.UnderlyingPolyline.CreateCurve();
                    if (!Curve.TrimSplineAndCalculateArrowheads(geometryEdge, geometryEdge.Curve, true))
                        Curve.CreateBigEnoughSpline(geometryEdge);
                }

                this.BBNode = null;
                this.DGraph.BbNode = null;
                this.DGraph.BuildBBHierarchy();
                this.Invalidate();
            }
        }



        internal void RaisePaintEvent(PaintEventArgs e) {
            base.OnPaint(e);
        }

        #region IViewer Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public void StartDrawingRubberLine(MsaglMouseEventArgs args) {
            this.panel.StartDrawingRubberLine(new System.Drawing.Point(args.X, args.Y));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public void DrawRubberLine(MsaglMouseEventArgs args) {
            panel.DrawRubberLine(args);
        }
        /// <summary>
        /// 
        /// </summary>
        public void StopDrawingRubberLine() {
            panel.StopDrawRubberLine();
        }

        #endregion



        #region IViewer Members

/// <summary>
/// creates a detached edge from the existing nodes
/// </summary>
/// <param name="source"></param>
/// <param name="target"></param>
/// <returns></returns>
        public IViewerEdge CreateEdge(Microsoft.Msagl.Drawing.Node source, Microsoft.Msagl.Drawing.Node target) {
            System.Diagnostics.Debug.Assert(this.Graph.FindNode(source.Id) == source);
            System.Diagnostics.Debug.Assert(this.Graph.FindNode(target.Id) == target);

            Microsoft.Msagl.Drawing.Edge edge = new DrawingEdge(source, target,Connection.Disconnected);
            edge.Label = new Microsoft.Msagl.Drawing.Label();
            Microsoft.Msagl.Edge geometryEdge = edge.Attr.GeometryEdge = new Microsoft.Msagl.Edge();
            geometryEdge.Parent = this.Graph.GeometryGraph;

            Microsoft.Msagl.Point a = source.Attr.GeometryNode.Center;
            Microsoft.Msagl.Point b = target.Attr.GeometryNode.Center;
            if (source == target) {
                Site start = new Site(a);
                Site end = new Site(b);
                Microsoft.Msagl.Point mid1 = source.Attr.GeometryNode.Center;
                mid1.X += (source.Attr.GeometryNode.BoundingBox.Width / 3 * 2);
                Microsoft.Msagl.Point mid2 = mid1;
                mid1.Y -= source.Attr.GeometryNode.BoundingBox.Height / 2;
                mid2.Y += source.Attr.GeometryNode.BoundingBox.Height / 2;
                Site mid1s = new Site(mid1);
                Site mid2s = new Site(mid2);
                start.Next = mid1s;
                mid1s.Previous = start;
                mid1s.Next = mid2s;
                mid2s.Previous = mid1s;
                mid2s.Next = end;
                end.Previous = mid2s;
                geometryEdge.UnderlyingPolyline = new UnderlyingPolyline(start);
                geometryEdge.Curve = geometryEdge.UnderlyingPolyline.CreateCurve();
            } else {
                Site start = new Site(a);
                Site end = new Site(b);
                Site mids = new Site(a * 0.5 + b * 0.5);
                start.Next = mids;
                mids.Previous = start;
                mids.Next = end;
                end.Previous = mids;
                geometryEdge.UnderlyingPolyline = new UnderlyingPolyline(start);
                geometryEdge.Curve = geometryEdge.UnderlyingPolyline.CreateCurve();
            }

            geometryEdge.Source = edge.SourceNode.Attr.GeometryNode;
            geometryEdge.Target = edge.TargetNode.Attr.GeometryNode;
            geometryEdge.ArrowheadLength = edge.Attr.ArrowheadLength;
            if (!Curve.TrimSplineAndCalculateArrowheads(geometryEdge, geometryEdge.Curve, true))
                Curve.CreateBigEnoughSpline(geometryEdge);
           
            DEdge dEdge = new DEdge(this.DGraph.NodeMap[edge.SourceNode.Id], this.DGraph.NodeMap[edge.TargetNode.Id], edge, Connection.Disconnected);
            if (edge.Label != null) 
                dEdge.Label = new DLabel(dEdge, new Microsoft.Msagl.Drawing.Label());
            return dEdge;
    
        }

        /// <summary>
        /// gets the visual graph
        /// </summary>
        public IViewerGraph ViewerGraph {
            get { return this.DGraph; }
        }

        /// <summary>
        /// creates a viewer node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public IViewerNode CreateNode(Microsoft.Msagl.Drawing.Node node) {
            return DGraph.CreateDNodeAndSetNodeBoundaryCurve(this.Graph,
                this.DGraph, node.Attr.GeometryNode, node);
        }

        #endregion

        #region IViewer Members

        /// <summary>
        /// sets the edge label
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="label"></param>
        public void SetEdgeLabel(DrawingEdge edge, Microsoft.Msagl.Drawing.Label label) {
            //find the edge first
            DEdge de=null;
            foreach (DEdge dEdge in this.DGraph.Edges) {
                if (dEdge.DrawingEdge == edge) {
                    de = dEdge;
                    break;
                }
            }
            System.Diagnostics.Debug.Assert(de != null);
            edge.Label = label;
            double w, h;
            DGraph.CreateDLabel(de, label, out w, out h);
            edge.Attr.GeometryEdge.Label = label.GeometryLabel;
            ICurve curve = edge.Attr.GeometryEdge.Curve;
            label.GeometryLabel.Center = curve[(curve.ParStart+curve.ParEnd)/2];
            label.GeometryLabel.Parent = edge.Attr.GeometryEdge;
            this.BBNode = this.DGraph.BbNode = null;
            this.Invalidate();
        }

        #endregion
    }




#if DEBUGLOG
  internal class Lo
  {
    static StreamWriter sw = null;

    static internal void W(string s)
    {
      if (sw == null)
      {
        sw = new StreamWriter("c:\\gdiviewerlog");
      }
      sw.WriteLine(s);
      sw.Flush();

    }
    static internal void W(object o)
    {
      if (sw == null)
      {
        sw = new StreamWriter("c:\\gdiviewerlog");
      }
      sw.WriteLine(o.ToString());
      sw.Flush();

    }

  }
#endif
}





