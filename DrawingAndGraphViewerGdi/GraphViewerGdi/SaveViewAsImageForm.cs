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
    internal partial class SaveViewAsImageForm : Form {

        GViewer gViewer;
        
        internal SaveViewAsImageForm(GViewer gViewerControl) {
            InitializeComponent();
            this.saveCurrentView.Checked = gViewerControl.SaveCurrentViewInImage;
            this.saveTotalView.Checked = !gViewerControl.SaveCurrentViewInImage;
            this.gViewer = gViewerControl;
            this.CancelButton = this.cancelButton;
            this.imageScale.TickStyle = TickStyle.Both;
            this.imageScale.TickFrequency = 5;
            this.imageScale.Minimum = 10;
            this.imageScale.Maximum = 100;
            this.imageScale.Value = this.imageScale.Minimum;
            SetScaleLabelTexts();
            this.imageScale.ValueChanged += new EventHandler(imageScale_ValueChanged);
            this.saveCurrentView.CheckedChanged += new EventHandler(saveCurrentView_CheckedChanged);
            this.toolTip.SetToolTip(this.saveInTextBox, "The default file format is JPG");
            this.saveInTextBox.Text = "*.jpg";
        }

        void saveCurrentView_CheckedChanged(object sender, EventArgs e) {
            SetScaleLabelTexts();
            gViewer.SaveCurrentViewInImage = this.saveCurrentView.Checked;
        }


        double ImageScale {
            get {
                double span = imageScale.Maximum - imageScale.Minimum;
                double l = (imageScale.Value- imageScale.Minimum)/span;
                return 1.0 +  l * 9;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        void imageScale_ValueChanged(object sender, EventArgs e) {
            this.toolTip.SetToolTip(this.imageScale, String.Format("Image scale is {0}",ImageScale));
            SetScaleLabelTexts();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        private void SetScaleLabelTexts() {
            int w, h;

            if (this.saveTotalView.Checked) {
                w = (int)Math.Ceiling(gViewer.Graph.Width * ImageScale);
                h = (int)Math.Ceiling(gViewer.Graph.Height * ImageScale);
            } else {
                w = (int)(gViewer.SrcRect.Width * ImageScale);
                h = (int)(gViewer.SrcRect.Height * ImageScale);
            }
            this.imageSizeLabel.Text = String.Format("Image size : {0} x {1}", w, h);
  
        }

        private void browseButton_Click(object sender, EventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPG Files(*.JPG)|*.JPG|BMP Files(*.BMP)|*.BMP|GIF Files(*.GIF)|*.GIF|Png Files(*.Png)|*.Png";
            saveFileDialog.OverwritePrompt = false;
            DialogResult dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK) {
                this.saveInTextBox.Text = saveFileDialog.FileName;
                this.okButton.Focus();//to enable hitting the OK button
            }
        }

        string FileName { get { return this.saveInTextBox.Text; } }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.String)")]
        private void okButton_Click(object sender, EventArgs e) {
            if (String.IsNullOrEmpty(saveInTextBox.Text)) {
                MessageBox.Show("File name is not set");
                return;
            }

            Cursor c = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try {

                int w = 0, h = 0;
                if (this.saveCurrentView.Checked) {
                    w = (int)Math.Ceiling(gViewer.SrcRect.Width * ImageScale);
                    h = (int)Math.Ceiling(gViewer.SrcRect.Height * ImageScale);
                } else {
                    w = (int)Math.Ceiling(gViewer.Graph.Width * ImageScale);
                    h = (int)Math.Ceiling(gViewer.Graph.Height * ImageScale);
                }

                Bitmap bitmap = null;
                if (GetFileNameExtension() == ".EMF" || GetFileNameExtension() == ".WMF") {
                    DrawVectorGraphics(w, h);
                } else {

                    bitmap = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
                    using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                        DrawGeneral(w, h, graphics);
                }

                AdjustFileName();
                if (bitmap != null)
                    bitmap.Save(this.saveInTextBox.Text);

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
            using(SolidBrush solidBrush = new SolidBrush(Draw.MsaglColorToDrawingColor(gViewer.Graph.Attr.BackgroundColor))) {
               //fill the whole image
               graphics.FillRectangle(solidBrush, new RectangleF(0, 0, w, h));
            }

            //calculate the transform
            double s = ImageScale;
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


        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
        void AdjustFileName() {
            string ext = GetFileNameExtension();
            if (ext == ".BMP" || ext == ".JPG" || ext == ".GIF" || ext == ".EMF" || ext == ".PNG" || ext == ".WMF") { //do nothing
            } else
                saveInTextBox.Text = saveInTextBox.Text.ToLower() + ".png";

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToUpper")]
        private string GetFileNameExtension() {
            return System.IO.Path.GetExtension(FileName).ToUpper();
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            this.Close();
        }

   }
}