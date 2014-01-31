using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing.Drawing2D;

namespace Microsoft.Msagl.GraphViewerGdi {
    internal partial class SaveInVectorFormatForm : Form {

        GViewer gViewer;

        internal SaveInVectorFormatForm(GViewer gViewerControl) {
            InitializeComponent();
            this.saveCurrentView.Checked = gViewerControl.SaveCurrentViewInImage;
            this.saveTotalView.Checked = !gViewerControl.SaveCurrentViewInImage;
            this.gViewer = gViewerControl;
            this.CancelButton = this.cancelButton;
            this.saveCurrentView.CheckedChanged += new EventHandler(saveCurrentView_CheckedChanged);
            this.toolTip.SetToolTip(this.saveInTextBox, "The default file format is Emf");
            this.Text = "Save in EMF of WMF format";
            this.saveInTextBox.Text = "*.emf";
        }

        void saveCurrentView_CheckedChanged(object sender, EventArgs e) {
            gViewer.SaveCurrentViewInImage = this.saveCurrentView.Checked;
        }


        private void browseButton_Click(object sender, EventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "EMF Files(*.emf)|*.emf|WMF Files(*.wmf)|*.wmf";
            saveFileDialog.OverwritePrompt = false;
            DialogResult dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK) {
                this.saveInTextBox.Text = saveFileDialog.FileName;
                this.okButton.Focus();//to enable hitting the OK button
            }
        }

        string FileName { get { return this.saveInTextBox.Text; } }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.String)")]
        private void okButton_Click(object sender, EventArgs e) {
            if (String.IsNullOrEmpty(saveInTextBox.Text)) {
                MessageBox.Show("File name is not set");
                return;
            }

            if (!(FileName.EndsWith(".emf", StringComparison.OrdinalIgnoreCase) || FileName.EndsWith(".wmf",StringComparison.OrdinalIgnoreCase))) 
                saveInTextBox.Text += ".emf";
        
            Cursor c = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try {

                int w = (int)Math.Ceiling(gViewer.SrcRect.Width);
                int h = (int)Math.Ceiling(gViewer.SrcRect.Height);

                DrawVectorGraphics(w, h);

            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
                this.Cursor = c;
                return;
            }
            this.Cursor = c;
            this.Close();
        }

        private void DrawGeneral(int w, int h, System.Drawing.Graphics graphics) {
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;


            if (saveCurrentView.Checked) {
                DrawCurrent(graphics);
            } else {
                DrawAll(w, h, graphics);
            }
        }

        private void DrawAll(int w, int h, System.Drawing.Graphics graphics) {
            //fill the whole image
            graphics.FillRectangle(new SolidBrush(Draw.MsaglColorToDrawingColor(gViewer.Graph.Attr.BackgroundColor)),
                   new RectangleF(0, 0, w, h));

            //calculate the transform
            double s = 1;
            Microsoft.Msagl.Drawing.Graph g = gViewer.Graph;
            double x = 0.5 * w - s * (g.Left + 0.5 * g.Width);
            double y = 0.5 * h + s * (g.Bottom + 0.5 * g.Height);

            using (graphics.Transform = new System.Drawing.Drawing2D.Matrix((float)s, 0, 0, (float)-s, (float)x, (float)y)) {
               Draw.DrawPrecalculatedLayoutObject(graphics, gViewer.DGraph);
            }
        }

        private void DrawCurrent(System.Drawing.Graphics graphics) {
           using (Matrix currentTransform = gViewer.CurrentTransform) {
              graphics.Transform = currentTransform;
              graphics.FillRectangle(new SolidBrush(Draw.MsaglColorToDrawingColor(gViewer.Graph.Attr.BackgroundColor)),
                                     gViewer.SrcRect);
              graphics.Clip = new Region(gViewer.SrcRect);
              Draw.DrawPrecalculatedLayoutObject(graphics, gViewer.DGraph);
           }
        }

        void DrawVectorGraphics(int w, int h) {
            Graphics graphics = this.CreateGraphics();
            IntPtr ipHdc = graphics.GetHdc();

            //Create a new empty metafile from the memory stream 

            Stream outputStream = File.OpenWrite(FileName);
            Metafile MetafileToDisplay = new Metafile(outputStream, ipHdc, EmfType.EmfOnly);

            //Now that we have a loaded metafile, we get rid of that Graphics object

            graphics.ReleaseHdc(ipHdc);

            graphics.Dispose();

            //Reload the graphics object with the newly created metafile.

            graphics = Graphics.FromImage(MetafileToDisplay);


            DrawGeneral(w, h, graphics);

            //Get rid of the graphics object that we created 

            graphics.Dispose();
            outputStream.Close();


        }
    }
}
