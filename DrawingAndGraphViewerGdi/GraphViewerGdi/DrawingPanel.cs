using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Microsoft.Msagl.Drawing;
using BBox = Microsoft.Msagl.Splines.Rectangle;
using P2=Microsoft.Msagl.Point;
using System.Collections.Generic;

namespace Microsoft.Msagl.GraphViewerGdi {


    /// <summary>
    /// this class serves as a drawing panel for GViewer
    /// </summary>
    internal class DrawingPanel : Control {
        System.Drawing.Color rubberRectColor = System.Drawing.Color.Black;
        FrameStyle rubberRectStyle = FrameStyle.Dashed;
        System.Drawing.Rectangle rubberRect;

        int mouseDownHVal;
        int mouseDownVVal;

        System.Drawing.Point mouseUpPoint;
        bool needToEraseRubber;

        public bool NeedToEraseRubber {
            get { return needToEraseRubber; }
            set { needToEraseRubber = value; }
        }

        internal System.Drawing.Point mouseDownPoint;
        GViewer gViewer;

        internal GViewer GViewer {
            get { return gViewer; }
            set { gViewer = value; }
        }

        internal bool zoomWindow;

        protected bool m_ZoomEnabled = true;
        public bool ZoomEnabled {
            get { return m_ZoomEnabled; }
        }

        internal DrawingPanel() { }

        internal void SetDoubleBuffering() {
            // the magic calls for invoking doublebuffering
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.gViewer != null && this.gViewer.Graph != null && this.gViewer.Graph.GeometryGraph != null)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; 
                this.gViewer.DotOnPaint(e.Graphics, false);
            }
            base.OnPaint(e); // Filippo Polo 13/11/07; if I don't do this, onpaint events won't be invoked
            gViewer.RaisePaintEvent(e);
        }
        
        System.Windows.Forms.MouseButtons currentPressedButton;

        protected override void OnMouseDown(MouseEventArgs e) {

            base.OnMouseDown(e);
            MsaglMouseEventArgs iArgs = CreateMouseEventArgs(e);
            gViewer.RaiseMouseDownEvent(iArgs);
            if (!iArgs.Handled) {
                currentPressedButton = e.Button;
                if (currentPressedButton == System.Windows.Forms.MouseButtons.Left)
                    if (this.ClientRectangle.Contains(PointToClient(Control.MousePosition))) {
                        this.mouseDownPoint = new System.Drawing.Point(e.X, e.Y);
                    if (this.MouseDraggingMode != DraggingMode.Pan && ZoomEnabled)
                            zoomWindow = true;
                        else {
                            mouseDownHVal = gViewer.HVal;
                            mouseDownVVal = gViewer.VVal;
                        }
                    }
            }
        }
        


        void DrawXORFrame() {
            ControlPaint.DrawReversibleFrame(rubberRect, rubberRectColor, rubberRectStyle);
            NeedToEraseRubber = !NeedToEraseRubber;
        }

        protected override void OnMouseUp(MouseEventArgs args) {
            base.OnMouseUp(args);
            MsaglMouseEventArgs iArgs = CreateMouseEventArgs(args);
            gViewer.RaiseMouseUpEvent(iArgs);
            if (NeedToEraseRubber)
                DrawXORFrame();

            if (!iArgs.Handled) {
                if (gViewer.OriginalGraph != null && MouseDraggingMode == DraggingMode.WindowZoom) {
                    System.Drawing.Point p = mouseDownPoint;
                    double f = Math.Max(Math.Abs(p.X - args.X), Math.Abs(p.Y - args.Y)) / GViewer.dpi;
                    if (f > gViewer.ZoomWindowThreshold && zoomWindow) {
                        mouseUpPoint = new System.Drawing.Point(args.X, args.Y);
                        if (ClientRectangle.Contains(mouseUpPoint)) {
                            System.Drawing.Rectangle r = GViewer.RectFromPoints(mouseDownPoint, mouseUpPoint);
                            r.Intersect(gViewer.DestRect);
                            if (GViewer.ModifierKeyWasPressed() == false) {
                                mouseDownPoint.X = r.Left;
                                mouseDownPoint.Y = r.Top;
                                mouseUpPoint.X = r.Right;
                                mouseUpPoint.Y = r.Bottom;
                                P2 p1 = gViewer.ScreenToSource(mouseDownPoint);
                                P2 p2 = gViewer.ScreenToSource(mouseUpPoint);
                                double sc = Math.Min((double)ClientRectangle.Width / r.Width,
                                    (double)ClientRectangle.Height / (double)r.Height);
                                P2 center = 0.5f * (p1 + p2);
                                gViewer.Zoom(center.X, center.Y, sc * gViewer.ZoomF);
                            } 
                        }
                    }

                }
            }
            zoomWindow =  false;
        }


        protected override void OnMouseMove(MouseEventArgs args) {
            MsaglMouseEventArgs iArgs = CreateMouseEventArgs(args);
            gViewer.RaiseMouseMoveEvent(iArgs);
            if (!iArgs.Handled) {
                if (gViewer.Graph != null) {
                    SetCursor(args);
                    object old = gViewer.selectedDObject;
                    if (MouseDraggingMode == DraggingMode.Pan)
                        ProcessPan(args);
                    else if (zoomWindow) //the user is holding the left button
                            DrawZoomWindow(args);
                        else
                            HitIfBBNodeIsNotNull(args);
                }
            }
        }

       
        private void HitIfBBNodeIsNotNull(MouseEventArgs args) {
            if (this.gViewer.BBNode != null)
                this.gViewer.Hit(args);
        }



        static MsaglMouseEventArgs CreateMouseEventArgs(MouseEventArgs args) {
            return new ViewerMouseEventArgs(args);
        }

      
        private void SetCursor(MouseEventArgs args) {
            Cursor cur;
            if (this.MouseDraggingMode == DraggingMode.Pan) {
                if (args.Button == System.Windows.Forms.MouseButtons.Left)
                    cur= gViewer.panGrabCursor;
                else
                    cur = gViewer.panOpenCursor;
            } else
                cur = gViewer.originalCursor;

            if (cur != this.Cursor)
                this.Cursor = cur;
        }

      

        private void DrawZoomWindow(MouseEventArgs args) {
            mouseUpPoint.X = args.X;
            mouseUpPoint.Y = args.Y;

            if (this.NeedToEraseRubber) 
                DrawXORFrame();
           
            if (ClientRectangle.Contains(PointToClient(Control.MousePosition))) {
                rubberRect = GViewer.RectFromPoints(PointToScreen(mouseDownPoint), PointToScreen(mouseUpPoint));
                DrawXORFrame();
            }
        }

        private void ProcessPan(MouseEventArgs args) {
            if (ClientRectangle.Contains(args.X, args.Y)) {
                if (args.Button == System.Windows.Forms.MouseButtons.Left) {
                    double dx = (double)(args.X - mouseDownPoint.X);
                    double dy = (double)(args.Y - mouseDownPoint.Y);
                    dx /= gViewer.LocalScale;
                    dy /= gViewer.LocalScale; //map it to real coord
                    int dh = gViewer.ScaleFromSrcXToScroll(dx);
                    int dv = gViewer.ScaleFromSrcYToScroll(dy);
                    gViewer.HVal = mouseDownHVal - dh;
                    gViewer.VVal = mouseDownVVal + dv;
                    gViewer.Invalidate();
                } else
                    GViewer.Hit(args);
            }
        }

        internal DraggingMode MouseDraggingMode {
            get {
                if (gViewer.panButton.Pushed)
                    return DraggingMode.Pan;
                else if (gViewer.windowZoomButton.Pushed)
                    return DraggingMode.WindowZoom;
                return DraggingMode.Default;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            gViewer.OnKey(e);
            base.OnKeyUp(e);
        }

        System.Drawing.Point startOfRubberLine;

        internal System.Drawing.Point StartOfRubberLine {
            get { return startOfRubberLine; }
        }
        System.Drawing.Point endOfRubberLine;

        internal System.Drawing.Point EndOfRubberLine {
            get { return endOfRubberLine; }
        }

        internal void DrawRubberLine(MsaglMouseEventArgs args) {
            if (NeedToEraseRubber)
                DrawRubberLine();
            endOfRubberLine = PointToScreen(new System.Drawing.Point(args.X, args.Y));
            DrawRubberLine();
        }

        private void DrawRubberLine() {
            ControlPaint.DrawReversibleLine(StartOfRubberLine, EndOfRubberLine, this.rubberRectColor);
            NeedToEraseRubber = !NeedToEraseRubber;
        }

        internal void StopDrawRubberLine() {
            if (NeedToEraseRubber)
                DrawRubberLine();
        }

        internal void StartDrawingRubberLine(System.Drawing.Point point) {
            this.startOfRubberLine = PointToScreen(point);
        }
    }
}
