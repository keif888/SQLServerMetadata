using System;
using System.Drawing;
using Microsoft.Msagl.Drawing;
using BBox = Microsoft.Msagl.Splines.Rectangle;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
namespace Microsoft.Msagl.GraphViewerGdi {

    /// <summary>
    /// Summary description for Geometry.
    /// </summary>
    internal class Geometry: ObjectWithBox {
        internal DObject tag;

        internal override BBox Box { get { return bBox; } }

        internal BBox bBox;

        internal Geometry(DObject tag, BBox box) {
            this.tag = tag;
            this.bBox = box;
        }
        internal Geometry(DObject tag) {
            this.tag = tag;

            DNode dNode = tag as DNode;
            if (dNode != null)
                bBox = dNode.DrawingNode.BoundingBox;
            else {
                DLabel dLabel = tag as DLabel;
                if (dLabel != null)
                    bBox = dLabel.DrawingLabel.BoundingBox;
            }
            
        }
    }
}
