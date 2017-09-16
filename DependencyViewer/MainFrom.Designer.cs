namespace DependencyViewer
{
    partial class MainFrom
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrom));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvObjectList = new System.Windows.Forms.TreeView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.graphViewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            this.cmGraph = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.locateObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lvObjectProperties = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Value = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel2 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbLayoutDirection = new System.Windows.Forms.ComboBox();
            this.btLayout = new System.Windows.Forms.Button();
            this.nbAfter = new System.Windows.Forms.NumericUpDown();
            this.nbBefore = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnLoad = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.cmGraph.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nbAfter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbBefore)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvObjectList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(771, 498);
            this.splitContainer1.SplitterDistance = 221;
            this.splitContainer1.TabIndex = 0;
            // 
            // tvObjectList
            // 
            this.tvObjectList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvObjectList.Location = new System.Drawing.Point(0, 0);
            this.tvObjectList.Name = "tvObjectList";
            this.tvObjectList.Size = new System.Drawing.Size(221, 498);
            this.tvObjectList.TabIndex = 0;
            this.tvObjectList.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvObjectList_BeforeExpand);
            this.tvObjectList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvObjectList_AfterSelect);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.graphViewer);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.lvObjectProperties);
            this.splitContainer2.Panel2.Controls.Add(this.panel2);
            this.splitContainer2.Size = new System.Drawing.Size(546, 498);
            this.splitContainer2.SplitterDistance = 365;
            this.splitContainer2.TabIndex = 0;
            // 
            // graphViewer
            // 
            this.graphViewer.ArrowheadLength = 10D;
            this.graphViewer.AsyncLayout = false;
            this.graphViewer.AutoScroll = true;
            this.graphViewer.BackwardEnabled = false;
            this.graphViewer.BuildHitTree = true;
            this.graphViewer.ContextMenuStrip = this.cmGraph;
            this.graphViewer.CurrentLayoutMethod = Microsoft.Msagl.GraphViewerGdi.LayoutMethod.SugiyamaScheme;
            this.graphViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphViewer.EdgeInsertButtonVisible = true;
            this.graphViewer.Enabled = false;
            this.graphViewer.FileName = "";
            this.graphViewer.ForwardEnabled = false;
            this.graphViewer.Graph = null;
            this.graphViewer.InsertingEdge = false;
            this.graphViewer.LayoutAlgorithmSettingsButtonVisible = true;
            this.graphViewer.LayoutEditingEnabled = true;
            this.graphViewer.Location = new System.Drawing.Point(0, 0);
            this.graphViewer.LooseOffsetForRouting = 0.25D;
            this.graphViewer.MouseHitDistance = 0.05D;
            this.graphViewer.Name = "graphViewer";
            this.graphViewer.NavigationVisible = true;
            this.graphViewer.NeedToCalculateLayout = true;
            this.graphViewer.OffsetForRelaxingInRouting = 0.6D;
            this.graphViewer.PaddingForEdgeRouting = 8D;
            this.graphViewer.PanButtonPressed = false;
            this.graphViewer.SaveAsImageEnabled = true;
            this.graphViewer.SaveAsMsaglEnabled = false;
            this.graphViewer.SaveButtonVisible = true;
            this.graphViewer.SaveGraphButtonVisible = true;
            this.graphViewer.SaveInVectorFormatEnabled = true;
            this.graphViewer.Size = new System.Drawing.Size(546, 365);
            this.graphViewer.TabIndex = 0;
            this.graphViewer.TightOffsetForRouting = 0.125D;
            this.graphViewer.ToolBarIsVisible = true;
            this.graphViewer.Transform = ((Microsoft.Msagl.Core.Geometry.Curves.PlaneTransformation)(resources.GetObject("graphViewer.Transform")));
            this.graphViewer.UndoRedoButtonsVisible = true;
            this.graphViewer.WindowZoomButtonPressed = false;
            this.graphViewer.ZoomF = 1D;
//            this.graphViewer.ZoomFraction = 0.5D;
//            this.graphViewer.ZoomWhenMouseWheelScroll = true;
            this.graphViewer.ZoomWindowThreshold = 0.05D;
            this.graphViewer.ObjectUnderMouseCursorChanged += new System.EventHandler<Microsoft.Msagl.Drawing.ObjectUnderMouseCursorChangedEventArgs>(this.graphViewer_ObjectUnderMouseCursorChanged);
            // 
            // cmGraph
            // 
            this.cmGraph.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.locateObjectToolStripMenuItem});
            this.cmGraph.Name = "cmGraph";
            this.cmGraph.Size = new System.Drawing.Size(148, 26);
            // 
            // locateObjectToolStripMenuItem
            // 
            this.locateObjectToolStripMenuItem.Name = "locateObjectToolStripMenuItem";
            this.locateObjectToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.locateObjectToolStripMenuItem.Text = "Locate Object";
            this.locateObjectToolStripMenuItem.Click += new System.EventHandler(this.locateObjectToolStripMenuItem_Click);
            // 
            // lvObjectProperties
            // 
            this.lvObjectProperties.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.Value});
            this.lvObjectProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvObjectProperties.Location = new System.Drawing.Point(0, 24);
            this.lvObjectProperties.Name = "lvObjectProperties";
            this.lvObjectProperties.Size = new System.Drawing.Size(546, 105);
            this.lvObjectProperties.TabIndex = 1;
            this.lvObjectProperties.UseCompatibleStateImageBehavior = false;
            this.lvObjectProperties.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Attribute";
            this.columnHeader1.Width = 200;
            // 
            // Value
            // 
            this.Value.Width = 285;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(546, 24);
            this.panel2.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Object Properties";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cbLayoutDirection);
            this.panel1.Controls.Add(this.btLayout);
            this.panel1.Controls.Add(this.nbAfter);
            this.panel1.Controls.Add(this.nbBefore);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnLoad);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 498);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(771, 45);
            this.panel1.TabIndex = 1;
            // 
            // cbLayoutDirection
            // 
            this.cbLayoutDirection.FormattingEnabled = true;
            this.cbLayoutDirection.Items.AddRange(new object[] {
            "Left to Right",
            "Right to Left",
            "Top Down",
            "Bottom Up"});
            this.cbLayoutDirection.Location = new System.Drawing.Point(389, 11);
            this.cbLayoutDirection.Name = "cbLayoutDirection";
            this.cbLayoutDirection.Size = new System.Drawing.Size(121, 21);
            this.cbLayoutDirection.TabIndex = 6;
            this.cbLayoutDirection.Text = "Top Down";
            this.cbLayoutDirection.SelectedValueChanged += new System.EventHandler(this.cbLayoutDirection_SelectedValueChanged);
            // 
            // btLayout
            // 
            this.btLayout.Location = new System.Drawing.Point(308, 9);
            this.btLayout.Name = "btLayout";
            this.btLayout.Size = new System.Drawing.Size(75, 23);
            this.btLayout.TabIndex = 5;
            this.btLayout.Text = "reLayout";
            this.btLayout.UseVisualStyleBackColor = true;
            this.btLayout.Click += new System.EventHandler(this.btLayout_Click);
            // 
            // nbAfter
            // 
            this.nbAfter.Location = new System.Drawing.Point(241, 13);
            this.nbAfter.Name = "nbAfter";
            this.nbAfter.Size = new System.Drawing.Size(60, 20);
            this.nbAfter.TabIndex = 4;
            this.nbAfter.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nbAfter.ValueChanged += new System.EventHandler(this.nbAfter_ValueChanged);
            // 
            // nbBefore
            // 
            this.nbBefore.Location = new System.Drawing.Point(138, 13);
            this.nbBefore.Name = "nbBefore";
            this.nbBefore.Size = new System.Drawing.Size(60, 20);
            this.nbBefore.TabIndex = 3;
            this.nbBefore.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nbBefore.ValueChanged += new System.EventHandler(this.nbBefore_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(206, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "After";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(94, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Before";
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(13, 10);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(75, 23);
            this.btnLoad.TabIndex = 0;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // MainFrom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(771, 543);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainFrom";
            this.Text = "Dependency Viewer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.cmGraph.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nbAfter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbBefore)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView tvObjectList;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private Microsoft.Msagl.GraphViewerGdi.GViewer graphViewer;
        private System.Windows.Forms.ListView lvObjectProperties;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader Value;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btLayout;
        private System.Windows.Forms.NumericUpDown nbAfter;
        private System.Windows.Forms.NumericUpDown nbBefore;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.ComboBox cbLayoutDirection;
        private System.Windows.Forms.ContextMenuStrip cmGraph;
        private System.Windows.Forms.ToolStripMenuItem locateObjectToolStripMenuItem;
    }
}

