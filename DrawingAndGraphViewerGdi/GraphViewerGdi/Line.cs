using System;
using Microsoft.Msagl.Drawing;
using BBox = Microsoft.Msagl.Splines.Rectangle;
using P2=Microsoft.Msagl.Point;

namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// 
    /// </summary>
    internal class Line : Geometry {
        internal P2 start, end;
        double lineWidth;

        internal double LineWidth {
            get { return lineWidth; }
        }
        internal Line(DObject tag, P2 start, P2 end, double lw)
            : base(tag) {
            lineWidth = lw;
            P2 dir = end - start;
            if (lineWidth < 0)
                lineWidth = 1;

            double len = dir.Length;
            if (len > Microsoft.Msagl.Splines.Curve.IntersectionEpsilon) {
                dir /= (len / (lineWidth / 2));
                dir = dir.Rotate(Math.PI / 2);
            } else {
                dir.X = 0;
                dir.Y = 0;
            }

            this.bBox = new Microsoft.Msagl.Splines.Rectangle(start + dir);
            this.bBox.Add(start - dir);
            this.bBox.Add(end + dir);
            this.bBox.Add(end - dir);
            this.start = start;
            this.end = end;

            if (this.bBox.LeftTop.X == this.bBox.RightBottom.X) {
                bBox.LeftTop = bBox.LeftTop + new P2(-0.05f, 0);
                bBox.RightBottom = bBox.RightBottom + new P2(0.05f, 0);
            }
            if (this.bBox.LeftTop.Y == this.bBox.RightBottom.Y) {
                bBox.LeftTop = bBox.LeftTop + new P2(0, -0.05f);
                bBox.RightBottom = bBox.RightBottom + new P2(0, 0.05f);
            }

        }
    }
}
