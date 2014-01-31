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


    /// <summary>
    /// viewer of directed graphs
    /// </summary>
    partial class GViewer : System.Windows.Forms.UserControl, IViewer {


        static int scrollMax = 0xFFFF;
        double scrollMaxF = scrollMax;
        /// <summary>
        /// 
        /// xLong = GrapWidth/ClientRect.Width>GraphHeight/ClientRect.Height
        /// </summary>
        bool xLong;
        double scaleDownCoefficient;
        /// <summary>
        /// if scaledDown is true - take the scale from the zoomCoeff 
        /// </summary>
        bool scaledDown;
        double scaleMin = 0.10f;
        internal double zoomFraction = 0.5f;
        /// <summary>
        /// Sets or gets the fraction on which the zoom value changes in every zoom out or zoom in
        /// </summary>
        public double ZoomFraction {
            get { return zoomFraction; }
            set {
                if (value > 0.9)
                    value = 0.9f;
                else if (value < 0.0001)
                    value = 0.0001f;
                zoomFraction = value;

            }
        }

        double localScale;

        public double LocalScale {
            get { return localScale; }
            set { localScale = value; }
        }


        /*
         * (s, 0,a)(srcRect.X)= (destRect.Left,destRect.Top)
         * (0,-s,b)(srcRect.Y)
         * a=destRect.Left-s*srcRect.Left
         * b=destRect.Bottom + srcRect.Bottom * s
         * */
        internal System.Drawing.Drawing2D.Matrix CurrentTransform {
            get {
                float s = (float)destRect.Width / (float)SrcRect.Width;
                return new System.Drawing.Drawing2D.Matrix(s, 0.0f, 0.0f, -s,
                  (float)(destRect.X - SrcRect.X * s),
                  (float)(destRect.Bottom + SrcRect.Y * s));
            }
        }

        System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(0, 0, 0, 0);
        RectangleF srcRect = new RectangleF(0, 0, 0, 0);

        internal RectangleF SrcRect {
            get { return srcRect; }
            set { srcRect = value; }
        }


        double HLargeChangeF {
            get { return this.hScrollBar.LargeChange; }
            set { HLargeChange = Int(value); }
        }

        double VLargeChangeF {
            get { return this.vScrollBar.LargeChange; }
            set { VLargeChange = Int(value); }
        }

        static int BoundToScroll(int i) {
            if (i > scrollMax)
                i = scrollMax;
            if (i < 0)
                i = 0;

            return i;
        }

        internal int VLargeChange {
            get { return this.vScrollBar.LargeChange; }
            set { this.vScrollBar.LargeChange = BoundToScroll(value); }
        }

        internal int HLargeChange {
            get { return this.hScrollBar.LargeChange; }
            set { this.hScrollBar.LargeChange = BoundToScroll(value); }
        }

        /// <summary>
        /// positions and largeValue of scroll bars define srcRect in the graph coordinates
        /// </summary>		
        internal int HVal {
            get { return this.hScrollBar.Value; }
            set {
                this.hScrollBar.Value = Math.Max(0, Math.Min(value, hScrollBar.Maximum + 1 - HLargeChange));
            }
        }
        /// <summary>
        /// positions and largeValue of scroll bars define srcRect in the graph coordinates
        /// </summary>
        internal int VVal {
            get { return vScrollBar.Value; }
            set {
                vScrollBar.Value = Math.Max(0, Math.Min(value, vScrollBar.Maximum + 1 - VLargeChange));

            }
        }

        double HValF {
            get { return (double)this.hScrollBar.Value; }
            set { this.HVal = BoundToScroll(Int(value)); }
        }
        double VValF {
            get { return (double)VVal; }
            set { VVal = BoundToScroll(Int(value)); }
        }
        /// <summary>
        /// The width of the current graph
        /// </summary>
        public double GraphWidth {
            get { return this.OriginalGraph.Width; }
        }

        /// <summary>
        /// The height of the current graph
        /// </summary>

        public double GraphHeight {
            get { return this.OriginalGraph.Height; }
        }

        /// <summary>
        /// Gets or sets the zoom factor
        /// </summary>
        public double ZoomF {
            get {
                if (scaledDown == false) {
                    if (this.xLong) {
                        return scrollMaxF / HLargeChangeF;
                    } else
                        return scrollMaxF / VLargeChangeF;
                } else
                    return scaleDownCoefficient;
            }
            set {
                if (OriginalGraph == null)
                    return;
                if (value < scaleMin || value > scrollMaxF) {
                    //MessageBox.Show("the zoom value is out of range ")
                    return;
                }
                if (value >= 1) {
                    scaledDown = false;
                    if (value == 1)
                        ZoomWithOne();
                    else if (xLong)
                        ZoomXLong(value);
                    else
                        ZoomYLong(value);
                    #region The old version
                    //double sc = value * ScaleLocal;

                    //hScrollBar.Visible = ((double)ClientRectangle.Width) / sc < GraphWidth;
                    //vScrollBar.Visible = ((double)ClientRectangle.Height) / sc < GraphHeight;

                    //double hCenter = hValF + HLargeChangeF / 2.0f;
                    //double vCenter = vValF + VLargeChangeF / 2.0f;

                    //if (hScrollBar.Visible) {

                    //    HLargeChangeF = scrollMaxF * ((((double)PanelWidth) / sc) / GraphWidth);
                    //    if (HLargeChange == scrollMax)
                    //        hScrollBar.Visible = false;
                    //    hValF = hCenter - HLargeChangeF / 2.0f;

                    //} else {
                    //    hScrollBar.LargeChange = scrollMax;
                    //    hScrollBar.Value = 0;
                    //}

                    //if (vScrollBar.Visible) {

                    //    VLargeChangeF = scrollMaxF * ((((double)PanelHeight) / sc) / GraphHeight);
                    //    vValF = vCenter - VLargeChangeF / 2.0f;
                    //    if (VLargeChange == scrollMax)
                    //        vScrollBar.Visible = false;
                    //} else {
                    //    VLargeChange = scrollMax;
                    //    VVal = 0;
                    //}
                    #endregion

                } else {
                    vScrollBar.Visible = false;
                    hScrollBar.Visible = false;
                    scaledDown = true;
                    scaleDownCoefficient = value;
                }

                if( ZoomFactorChanged != null) {
                   ZoomFactorChanged(this, EventArgs.Empty);
                }
                panel.Invalidate();
            }

        }

        internal event EventHandler ZoomFactorChanged;

        private void ZoomYLong(double val) {
            double gh = GraphHeight / val;

            vScrollBar.Visible = true;
            double vCenter = VValF + VLargeChangeF / 2.0f;

            VLargeChangeF = scrollMaxF / val;
            VValF = vCenter - VLargeChangeF / 2.0f;
            double dx = vScrollBar.Width;
            double dy = hScrollBar.Height;
            double pw = PanelWidth;
            double ph = PanelHeight;
            double gw = GraphWidth;
            /* Do we need a horizontal scrollbar?
             Let y be a free horizontal space such that we can use the whole graph width
            */
            double y = ph - (pw - dx) * gh / gw;
            if (y <= dy) {
                if (y < 0)
                    y = 0;
                hScrollBar.Visible = false;
                HLargeChange = scrollMax;
                HVal = 0;
                LocalScale = (ph - y) / gh;
            } else {
                hScrollBar.Visible = true;
                double hCenter = HValF + HLargeChangeF / 2.0f;
                LocalScale = (ph - dy) / gh;
                HLargeChangeF = scrollMax * ((pw - dx) / LocalScale / gw);
                HValF = vCenter - HLargeChangeF / 2.0f;
            }
        }

        private void ZoomXLong(double val) {
            double gw = GraphWidth / val;

            hScrollBar.Visible = true;
            double hCenter = HValF + HLargeChangeF / 2.0f;

            HLargeChangeF = scrollMaxF / val;
            HValF = hCenter - HLargeChangeF / 2.0f;
            double dx = vScrollBar.Width;
            double dy = hScrollBar.Height;
            double pw = PanelWidth;
            double ph = PanelHeight;
            double gh = GraphHeight;
            /* Do we need a vertical scrollbar?
             Let x be a free horizontal space such that we can use the whole graph height
             we have sc*gw=pw-x, sx=(pw-x)/gw; (pw-x)/gw*gh=ph-dy;
             pw-x=(ph-dy)/gh*gw; x=pw-(ph-dy)/gh*gw;
            */
            double x = pw - (ph - dy) / gh * gw;
            if (x <= dx) {
                if (x < 0)
                    x = 0;
                vScrollBar.Visible = false;
                VLargeChange = scrollMax;
                VVal = 0;
                LocalScale = (pw - x) / gw;
            } else {
                vScrollBar.Visible = true;
                double vCenter = VValF + VLargeChangeF / 2.0f;
                LocalScale = (pw - dx) / gw;
                VLargeChangeF = scrollMax * ((ph - dy) / LocalScale) / gh;
                VValF = vCenter - VLargeChangeF / 2.0f;
            }
        }

        private void ZoomWithOne() {
            this.HLargeChange = scrollMax;
            this.VLargeChange = scrollMax;
            HValF = VValF = 0;
            vScrollBar.Visible = hScrollBar.Visible = false;
            InitScale();

        }

        System.Drawing.Rectangle DrawingRect {
            get {
                return panel.ClientRectangle;
            }
        }

        void CalcDestRect() {
            double scaleNow = CurrentScale;

            destRect.Width = Math.Max(1, Math.Min(PanelWidth, Int(SrcRect.Width * scaleNow)));
            destRect.Height = Math.Max(1, Math.Min(PanelHeight, Int(SrcRect.Height * scaleNow)));
            destRect.X = 0;
            destRect.Y = 0;

            int dw = this.DrawingPanel.ClientRectangle.Width;
            if (destRect.Width < dw) {
                double shift = (dw - destRect.Width) / 2.0f;
                destRect.X = Int(shift);
            }

            int dh = this.DrawingPanel.ClientRectangle.Height;
            if (destRect.Height < dh) {
                double shift = (dh - destRect.Height) / 2.0f;
                destRect.Y = Int(shift);
            }

        }


        void CalcRects() {
            if (this.OriginalGraph != null) {
                CalcSrcRect();
                CalcDestRect();
            }
        }

        private void CalcSrcRect() {
            if (scaledDown == false) {
                double k = this.OriginalGraph.Width / scrollMaxF;

                srcRect.Width = (float)Math.Min(this.OriginalGraph.Width, k * this.HLargeChangeF);
                srcRect.X = (float)(k * HValF) + (float)OriginalGraph.Left;

                k = OriginalGraph.Height / scrollMaxF;
                srcRect.Y = (float)OriginalGraph.Height + (float)ScaleFromScrollToSrcY(VVal + this.VLargeChange) + (float)OriginalGraph.Bottom;
                srcRect.Height = (float)Math.Min(this.OriginalGraph.Height, k * this.VLargeChangeF);
            } else {
                srcRect.X = (float)OriginalGraph.Left;
                srcRect.Y = (float)OriginalGraph.Height + (float)ScaleFromScrollToSrcY(this.vScrollBar.Maximum) + (float)OriginalGraph.Bottom;
                srcRect.Width = (float)GraphWidth; srcRect.Height = (float)GraphHeight;
            }
        }


        void InitScale() {
            if (this.OriginalGraph != null) {
                double scaleX = ((double)PanelWidth) / ((double)this.OriginalGraph.Width);
                double scaleY = ((double)PanelHeight) / ((double)this.OriginalGraph.Height);

                LocalScale = (this.xLong = scaleX < scaleY) ? scaleX : scaleY;
            }
        }


        private void SizeScrollBars() {
            hScrollBar.SetBounds(0, ClientRectangle.Height - hScrollBar.Height, ClientRectangle.Width - vScrollBar.Width, hScrollBar.Height);
            vScrollBar.SetBounds(ClientRectangle.Right - vScrollBar.Width, 0, vScrollBar.Width, ClientRectangle.Height - hScrollBar.Height);
        }

        internal int PanelWidth {
            get {
                return this.panel.ClientRectangle.Width + (vScrollBar.Visible ? vScrollBar.Width : 0);
            }
        }

        internal int PanelHeight {
            get {
                return this.panel.ClientRectangle.Height + (hScrollBar.Visible ? hScrollBar.Height : 0);
            }
        }
        bool HScrVis {
            get {
                return hScrollBar.Visible;
            }
        }

        bool VScrVis {

            get {
                return vScrollBar.Visible;

            }
        }

        internal int ScaleFromSrcXToScroll(double x) {
            double k = scrollMaxF / this.OriginalGraph.Width;
            return Int(k * x);
        }

        internal double ScaleFromScrollToSrcX(int x) {
            double k = scrollMaxF / this.OriginalGraph.Width;
            return ((double)x) / k;
        }

        double ScaleFromScrollToSrcY(int x) {
            double k = this.OriginalGraph.Height / scrollMaxF;
            return (float)(-k * x);
        }

        internal int ScaleFromSrcYToScroll(double y) {
            double k = this.OriginalGraph.Height / scrollMaxF;
            return Int(-y / k);
        }
        static internal double dpi = GetDotsPerInch();
        static double dpix;

        static double dpiy;
        static double GetDotsPerInch() {
            Graphics g = (new Form()).CreateGraphics();
            return Math.Max(dpix = g.DpiX, dpiy = g.DpiY);
        }



        bool saveCurrentViewInImage;
        /// <summary>
        /// capturing the previous user's choice of which veiw to save
        /// </summary>
        internal bool SaveCurrentViewInImage {
            get { return saveCurrentViewInImage; }
            set { saveCurrentViewInImage = value; }
        }


        private DrawingPanel panel;

        /// <summary>
        /// The panel containing GViewer object
        /// </summary>
        public Control DrawingPanel { get { return panel; } }


        /// <summary>
        /// The ViewInfo gives all info needed for setting the view
        /// </summary>
        protected override void OnPaint(PaintEventArgs e) {
            panel.Invalidate();
        }

        void SetViewFromViewInfo(ViewInfo viewInfo) {
            this.scaledDown = viewInfo.scaledDown;
            this.scaleDownCoefficient = viewInfo.scaledDownCoefficient;

            this.VLargeChange = viewInfo.vLargeChange;
            this.HLargeChange = viewInfo.hLargeChange;

            this.HVal = viewInfo.hVal;
            this.VVal = viewInfo.vVal;

            this.hScrollBar.Visible = viewInfo.hScrollBarIsViz;
            this.vScrollBar.Visible = viewInfo.vScrollBarIsViz;

            this.storeViewInfo = false;
            this.ZoomF = viewInfo.zoomF;
            this.panel.Invalidate();
        }

        /// <summary>
        /// Gets or sets the forward and backward buttons visibility
        /// </summary>
        public bool NavigationVisible {
            get { return this.forwardButton.Visible; }
            set {
                this.forwardButton.Visible = value;
                this.backwardButton.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets the save button visibility
        /// </summary>
        public bool SaveButtonVisible {
            get { return this.saveButton.Visible; }
            set { this.saveButton.Visible = value; }
        }

        /// <summary>
        /// The event raised when the graph object under the mouse cursor changes
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// The rectangle for drawing
        /// </summary>
        internal System.Drawing.Rectangle DestRect { get { return destRect; } set { destRect = value; } }

        #region Members



        ViewInfosList listOfViewInfos = new ViewInfosList();


        internal void ResetListOfViewInfos() {
           listOfViewInfos.Reset();
        }
        internal bool IsThereMoreThanOnePreviousViewInfo() {
           return listOfViewInfos.IsThereMoreThanOnePreviousViewInfo();
        }
        internal bool IsThereMoreThanOneNextViewInfo() {
           return listOfViewInfos.IsThereMoreThanOneNextViewInfo();
        }

        bool storeViewInfo = true;
        internal Cursor panOpenCursor;
        internal Cursor panGrabCursor;
        internal Cursor originalCursor;




        static int minimalSizeToDraw = 10;

        System.Drawing.Brush imageBackgroungBrush = System.Drawing.Brushes.White;

        System.Drawing.Brush ImageBackgroungBrush {
            get { return imageBackgroungBrush; }
            set { imageBackgroungBrush = value; }
        }

        System.Drawing.Brush outsideAreaBrush = System.Drawing.Brushes.LightGray;

        /// <summary>
        /// The color of the area outside of the graph.
        /// </summary>
        public System.Drawing.Brush OutsideAreaBrush {
            get { return outsideAreaBrush; }
            set { outsideAreaBrush = value; }
        }

        internal DObject selectedDObject;
        /// <summary>
        /// The object which is currently located under the mouse cursor
        /// </summary>
        public object SelectedObject {
            get {

                return selectedDObject != null ? selectedDObject.DrawingObject : null;
            }
        }

        System.Drawing.Point mousePositonWhenSetSelectedObject;

        internal System.Drawing.Point MousePositonWhenSetSelectedObject {
            get { return mousePositonWhenSetSelectedObject; }
            set { mousePositonWhenSetSelectedObject = value; }
        }

        internal void SetSelectedObject(object o) {
            selectedDObject = (DObject)o;

            MousePositonWhenSetSelectedObject = MousePosition;
            if (SelectionChanged != null)
                SelectionChanged(this, null);
        }




        internal System.Windows.Forms.ToolTip ToolTip {
            get { return toolTip1; }
            set { toolTip1 = value; }
        }




        //public static double LocationToFloat(int location) { return ((double)location) * LayoutAlgorithmSettings.PointSize; }

        //public static double LocationToFloat(string location) { return LocationToFloat(Int32.Parse(location)); }



        internal bool DestRectContainsPoint(System.Drawing.Point p) {
            return destRect.Contains(p);
        }

        #endregion

        enum ImageEnum {
            ZoomIn,
            ZoomOut,
            WindowZoom,
            Hand,
            Forward,
            ForwardDis,
            Backward,
            BackwardDis,
            Save,
            Undo, Redo, Print, Open, UndoDisabled, RedoDisabled
        }
        /// <summary>
        /// Enables or disables the forward button
        /// </summary>
        public bool ForwardEnabled {
            get { return forwardButton.ImageIndex == (int)ImageEnum.Forward; }

            set {
                forwardButton.ImageIndex = (int)(value ? ImageEnum.Forward : ImageEnum.ForwardDis);
            }
        }

        /// <summary>
        /// Enables or disables the backward button
        /// </summary>
        public bool BackwardEnabled {
            get { return backwardButton.ImageIndex == (int)ImageEnum.Backward; }

            set {
                backwardButton.ImageIndex = (int)(value ? ImageEnum.Backward : ImageEnum.BackwardDis);
            }
        }



        void ToolBarMouseMoved(object o, MouseEventArgs a) {
            this.Cursor = this.originalCursor;
        }

        void vScrollBar_MouseEnter(object o, EventArgs a) {
            ToolBarMouseMoved(null, null);
        }
        /// <summary>
        /// Tightly fit the bounding box around the graph
        /// </summary>
        public void FitGraphBoundingBox() {
            if (this.DrawingLayoutEditor != null) {
                if (this.Graph != null)
                    DrawingLayoutEditor.FitGraphBoundingBox(this.DGraph);
                Invalidate();
            }
        }
        /// <summary>
        /// Default constructor
        /// </summary>
        public GViewer() {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.BackwardEnabled = false;

            this.ForwardEnabled = false;

            this.vScrollBar.MouseEnter += new EventHandler(vScrollBar_MouseEnter);
            this.hScrollBar.MouseEnter += new EventHandler(vScrollBar_MouseEnter);
            this.toolbar.MouseMove += new System.Windows.Forms.MouseEventHandler(ToolBarMouseMoved);

            Assembly a = Assembly.GetExecutingAssembly();
            foreach (string r in a.GetManifestResourceNames()) {
                if (r.Contains("hmove.cur"))
                    this.panGrabCursor = new Cursor(a.GetManifestResourceStream(r));
                else if (r.Contains("oph.cur"))
                    this.panOpenCursor = new Cursor(a.GetManifestResourceStream(r));
            }

            this.originalCursor = this.Cursor;

            this.hScrollBar.Scroll += new ScrollEventHandler(ScrollHandler);
            this.vScrollBar.Scroll += new ScrollEventHandler(ScrollHandler);

            //sourceRect and scale , basically everything will be calculated through scrollbars
            //except the case when scaledDown is true
            //all visual info is derived from scroll bars

            hScrollBar.Minimum = vScrollBar.Minimum = 0;

            vScrollBar.Maximum = hScrollBar.Maximum = scrollMax;

            HLargeChange = VLargeChange = scrollMax;

            vScrollBar.SmallChange = hScrollBar.SmallChange = HLargeChange / 10;

            VVal = HVal = 0;

            this.panButton.Pushed = false;
            this.windowZoomButton.Pushed = false;
           
            this.layoutSettingsButton.ToolTipText="Configures the layout algorithm settings";

            this.undoButton.ToolTipText = "Undo layout editing";
            this.redoButton.ToolTipText = "Redo layout editing";
            this.forwardButton.ToolTipText = "Forward";
            panButton.ToolTipText = panButton.Pushed ? panButtonToolTipText : panButtonDisabledToolTipText;
            windowZoomButton.ToolTipText = windowZoomButton.Pushed ? windowZoomButtonToolTipText : windowZoomButtonDisabledToolTipText;

            InitDrawingLayoutEditor();

            toolbar.Invalidate();

            this.SuspendLayout();
            InitPanel();
            this.Controls.Clear();
            this.Controls.Add(this.panel);
            this.Controls.Add(this.toolbar);
            this.Controls.Add(this.vScrollBar);
            this.Controls.Add(this.hScrollBar);
            this.ResumeLayout();
        }

    
        private void InitPanel() {
            this.panel = new DrawingPanel();
            this.panel.TabIndex = 0;
            this.Controls.Add(panel);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;

            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(608, 534);
            this.panel.TabIndex = 0;
            panel.GViewer = this;
            panel.SetDoubleBuffering();
            panel.Click += new EventHandler(panel_Click);
            this.DrawingPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(DrawingPanel_MouseClick);
            this.DrawingPanel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(DrawingPanel_MouseDoubleClick);
            this.DrawingPanel.MouseCaptureChanged += new EventHandler(DrawingPanel_MouseCaptureChanged);
            this.DrawingPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(DrawingPanel_MouseDown);
            this.DrawingPanel.MouseEnter += new EventHandler(DrawingPanel_MouseEnter);
            this.DrawingPanel.MouseHover += new EventHandler(DrawingPanel_MouseHover);
            this.DrawingPanel.MouseLeave += new EventHandler(DrawingPanel_MouseLeave);
            this.DrawingPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(DrawingPanel_MouseMove);
            this.DrawingPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(DrawingPanel_MouseUp);
            this.DrawingPanel.MouseWheel += new System.Windows.Forms.MouseEventHandler(GViewer_MouseWheel);
            this.DrawingPanel.Move += new EventHandler(GViewer_Move);
            this.DrawingPanel.KeyDown += new KeyEventHandler(DrawingPanel_KeyDown);
            this.DrawingPanel.KeyPress += new KeyPressEventHandler(DrawingPanel_KeyPress);
            this.DrawingPanel.KeyUp += new KeyEventHandler(DrawingPanel_KeyUp);
            this.DrawingPanel.DoubleClick += new EventHandler(DrawingPanel_DoubleClick);
        }

        void DrawingPanel_DoubleClick(object sender, EventArgs e) {
            this.OnDoubleClick(e);
        }


        /// <summary>
        /// returns true if in the current state the layout can be edited
        /// </summary>
        public bool LayoutIsEditable {
            get {
                return !(panButton.Pushed || windowZoomButton.Pushed) && LayoutEditingEnabled;
            }
        }

        bool layoutEditingEnabled = true;
        /// <summary>
        /// enables or disables the layout editing
        /// </summary>
        public bool LayoutEditingEnabled {
            get { return layoutEditingEnabled; }
            set { layoutEditingEnabled = value; }
        }

        private void DisableDrawingLayoutEditor() {
            if (DrawingLayoutEditor != null) {
                this.DrawingLayoutEditor.DetachFromViewerEvents();
                this.DrawingLayoutEditor = null;
            }
        }

        private void InitDrawingLayoutEditor() {
            if (DrawingLayoutEditor == null) {
                this.DrawingLayoutEditor = new Microsoft.Msagl.Drawing.DrawingLayoutEditor(this);
                DrawingLayoutEditor.ChangeInUndoRedoList += new EventHandler(DrawingLayoutEditor_ChangeInUndoRedoList);
            }
            this.undoButton.ImageIndex = (int)ImageEnum.UndoDisabled;
            this.redoButton.ImageIndex = (int)ImageEnum.RedoDisabled;
        }


        void DrawingLayoutEditor_ChangeInUndoRedoList(object sender, EventArgs args) {
            if (undoButton.ImageIndex != UndoImageIndex())
                undoButton.ImageIndex = UndoImageIndex();
            if (redoButton.ImageIndex != RedoImageIndex())
                redoButton.ImageIndex = RedoImageIndex();
        }

        private int RedoImageIndex() {
            return (int)(DrawingLayoutEditor.CanRedo ? ImageEnum.Redo : ImageEnum.RedoDisabled);
        }

        private int UndoImageIndex() {
            return (int)(DrawingLayoutEditor.CanUndo ? ImageEnum.Undo : ImageEnum.UndoDisabled);
        }

        void drawingLayoutEditor_RedoDisabled(object sender, EventArgs e) {
            this.redoButton.ImageIndex = (int)ImageEnum.RedoDisabled;
        }

        void drawingLayoutEditor_RedoEnabled(object sender, EventArgs e) {
            this.redoButton.ImageIndex = (int)ImageEnum.Redo;
        }

        void drawingLayoutEditor_UndoDisabled(object sender, EventArgs e) {
            this.undoButton.ImageIndex = (int)ImageEnum.UndoDisabled;
        }

        void drawingLayoutEditor_UndoEnabled(object sender, EventArgs e) {
            this.undoButton.ImageIndex = (int)ImageEnum.Undo;
        }

        void DrawingPanel_KeyUp(object sender, KeyEventArgs e) {
            this.OnKeyUp(e);
        }

        void DrawingPanel_KeyPress(object sender, KeyPressEventArgs e) {
            this.OnKeyPress(e);
        }

        void DrawingPanel_KeyDown(object sender, KeyEventArgs e) {
            this.OnKeyDown(e);
        }

        void GViewer_Move(object sender, EventArgs e) {
            this.OnMove(e);
        }

        void GViewer_MouseWheel(object sender, MouseEventArgs e) {
            this.OnMouseWheel(e);
        }

        void DrawingPanel_MouseUp(object sender, MouseEventArgs e) {
            this.OnMouseUp(e);
        }

        void DrawingPanel_MouseMove(object sender, MouseEventArgs e) {
            this.OnMouseMove(e);
        }

        void DrawingPanel_MouseLeave(object sender, EventArgs e) {
            this.OnMouseLeave(e);
        }

        void DrawingPanel_MouseHover(object sender, EventArgs e) {
            this.OnMouseHover(e);
        }

        void DrawingPanel_MouseEnter(object sender, EventArgs e) {
            this.OnMouseEnter(e);
        }

        void DrawingPanel_MouseDown(object sender, MouseEventArgs e) {
            this.OnMouseDown(e);
        }

        void DrawingPanel_MouseCaptureChanged(object sender, EventArgs e) {
            this.OnMouseCaptureChanged(e);
        }

        void DrawingPanel_MouseDoubleClick(object sender, MouseEventArgs e) {
            this.OnMouseDoubleClick(e);
        }

        internal void Hit(MouseEventArgs args) {
            if (args.Button == System.Windows.Forms.MouseButtons.None)
                UnconditionalHit(args);
        }

        /// <summary>
        /// hides/shows the toolbar
        /// </summary>
        public bool ToolBarIsVisible {
            get {
                return this.Controls.Contains(toolbar);
            }
            set {
                if (value != ToolBarIsVisible) {
                    this.SuspendLayout();
                    if (value) {
                        Controls.Add(toolbar);
                        Controls.SetChildIndex(toolbar, 1);//it follows the panel
                    } else
                        Controls.Remove(toolbar);

                    this.ResumeLayout();
                }
            }
        }

        bool saveAsMsaglEnabled=true;
        /// <summary>
        /// If this property is set to true the control enables saving and loading of .MSAGL files
        /// Otherwise the "Load file" button and saving as .MSAGL file is disabled.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Msagl")]
        public bool SaveAsMsaglEnabled {
            get { return saveAsMsaglEnabled; }
            set {
                if (saveAsMsaglEnabled != value) {
                    openButton.Visible = value;
                    saveAsMsaglEnabled = value;
                }
            }
        }

        bool saveInVectorFormatEnabled=true;
        /// <summary>
        /// enables or disables saving the graph in a vector format
        /// </summary>
        public bool SaveInVectorFormatEnabled {
            get { return saveInVectorFormatEnabled; }
            set { saveInVectorFormatEnabled = value; }
        }

        bool saveAsImageEnabled=true;
        /// <summary>
        /// enables or disables saving the graph as an image
        /// </summary>
        public bool SaveAsImageEnabled {
            get { return saveAsImageEnabled; }
            set { saveAsImageEnabled = value; }
        }


        /// <summary>
        ///hides and shows the layout algorithm settings button
        /// </summary>
        public bool LayoutAlgorithmSettingsButtonVisible {
            get { return layoutSettingsButton.Visible; }
            set { layoutSettingsButton.Visible = value; }
        }
        /// <summary>
        /// hides and shows the "Save graph" button
        /// </summary>
        public bool SaveGraphButtonVisible {
            get { return saveButton.Visible; }
            set { saveButton.Visible = value; }
        }

        LayoutMethod currentLayoutMethod = LayoutMethod.SugiyamaScheme;
        /// <summary>
        /// sets the layout method of the viewer
        /// </summary>
        public LayoutMethod CurrentLayoutMethod
        {
            get { return currentLayoutMethod; }
            set { currentLayoutMethod = value; }
        }
        Microsoft.Msagl.SugiyamaLayoutSettings sugiyamaSettings = new Microsoft.Msagl.SugiyamaLayoutSettings();
        Microsoft.Msagl.Mds.MdsLayoutSettings mdsLayoutSettings = new Microsoft.Msagl.Mds.MdsLayoutSettings();
        Microsoft.Msagl.Mds.RankingLayoutSettings rankingSettings = new Microsoft.Msagl.Mds.RankingLayoutSettings();        

    }
}
