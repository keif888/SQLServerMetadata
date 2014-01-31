using System;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using BBox = Microsoft.Msagl.Splines.Rectangle;
using P2=Microsoft.Msagl.Point;

namespace Microsoft.Msagl.GraphViewerGdi
{
    /// <summary>
    /// Summary description for BBNode.
    /// </summary>
    internal class BBNode
    {
        internal BBNode l;
        internal BBNode r;
        internal BBNode parent;
        internal BBox bBox;
        internal Geometry geometry;
        internal BBox Box
        {
            get
            {
                if (geometry != null)
                    return geometry.bBox;

                return bBox;
            }
        }

      

        internal BBNode() { }

        //when we check for inclusion we expand the box by slack
        internal Geometry Hit(P2 p, double slack)
        {

            if (l == null)
                if (Box.Contains(p, slack))
                {
                    Line line = geometry as Line;

                    if (line != null)
                    {
                        if (Tessellator.DistToSegm(p, line.start, line.end) < slack + line.LineWidth / 2)
                            return line;
                        return null;

                    }
                    else if (Box.Contains(p))
                        return geometry;

                    return null;
                }
                else
                    return null;

            if (l.Box.Contains(p, slack))
            {
                Geometry g = l.Hit(p, slack);
                if (g != null)
                {
                    return g;
                }
            }

            if (r.Box.Contains(p, slack))
            {
                Geometry g = r.Hit(p, slack);
                if (g != null)
                    return g;
            }

            return null;
        }
    }
}
