using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// A class representing a drawn label
    /// </summary>
    public sealed class DLabel : DObject, Microsoft.Msagl.Drawing.IViewerObject {
        Font font;
        /// <summary>
        /// gets the font of the label
        /// </summary>
        public Font Font {
            get { return font; }
            set { font = value; }
        }

        DObject parent;
/// <summary>
/// the object that label belongs to
/// </summary>
        public DObject Parent {
            get { return parent; }
            set { parent = value; }
        }
     
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="parentPar"></param>
        /// <param name="label"></param>
        public DLabel(DObject parentPar, Microsoft.Msagl.Drawing.Label label) {
            this.parent = parentPar;
            this.DrawingLabel = label;
            ((IHavingDLabel)parent).Label = this;
            Font = new Font(DrawingLabel.FontName, DrawingLabel.FontSize);
        }

        /// <summary>
        /// delivers the underlying label object
        /// </summary>
        override public Microsoft.Msagl.Drawing.DrawingObject DrawingObject {
            get { return DrawingLabel; }
        }

        internal override float DashSize() {
            return 1;//it is never used
        }

        Microsoft.Msagl.Drawing.Label drawingLabel;
        /// <summary>
        /// gets or set the underlying drawing label
        /// </summary>
        public Microsoft.Msagl.Drawing.Label DrawingLabel {
            get { return drawingLabel; }
            set { drawingLabel = value; }
        }


    }
}
