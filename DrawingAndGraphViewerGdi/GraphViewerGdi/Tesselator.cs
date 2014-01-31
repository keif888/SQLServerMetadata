using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Msagl.Drawing;
using BBox = Microsoft.Msagl.Splines.Rectangle;
using P2=Microsoft.Msagl.Point;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// Class building the node hierarchy 
    /// </summary>

    internal class Tessellator {

        /// <summary>
        /// a private constructor to avoid the instantiation
        /// </summary>
        Tessellator() {}

        internal static double DistToSegm(P2 p, P2 s, P2 e) {

            P2 l = e - s;
            double len = l.Length;
            if (len < Tessellator.epsilon)
                return (p - (0.5f * (s + e))).Length;
            P2 perp = new P2(-l.Y, l.X);
            perp /= len;
            return Math.Abs((p - s) * perp);

        }

        static bool WithinEpsilon(Microsoft.Msagl.Splines.ICurve bc, double start, double end) {
            int n = 3; //hack !!!!
            double d = (end - start) / n;
            P2 s = bc[start];
            P2 e = bc[end];

            return DistToSegm(bc[start + d], s, e) < epsilon
              &&
              DistToSegm(bc[start + d * (n - 1)], s, e) < epsilon;

        }

        internal static List<ObjectWithBox> TessellateCurve(DEdge dedge, double radiusForUnderlyingPolylineCorners ) {
            DrawingEdge edge = dedge.DrawingEdge;
            Microsoft.Msagl.Splines.ICurve bc=edge.Attr.EdgeCurve;
            double lineWidth=edge.Attr.LineWidth;
            List<ObjectWithBox> ret=new List<ObjectWithBox>();
            int n = 1;
            bool done;
            do {
                double d = (bc.ParEnd - bc.ParStart) / (double)n;
                done = true;
                if (n <= 64)//don't break a segment into more than 64 parts
                    for (int i = 0; i < n; i++) {
                        if (!WithinEpsilon(bc, d * i, d * (i + 1))) {
                            n *= 2;
                            done = false;
                            break;
                        }
                    }
            }
            while (!done);

            double del = (bc.ParEnd - bc.ParStart) / n;

            for (int j = 0; j < n; j++) {
                Line line = new Line(dedge, bc[del * (double)j], bc[del * (double)(j + 1)], lineWidth);
                ret.Add(line);
            }

            //if (dedge.Label != null)
            //    ret.Add(new LabelGeometry(dedge.Label, edge.Label.Left,
            //                              edge.Label.Bottom, new P2(edge.Label.Size.Width, edge.Label.Size.Height)));
            
            
            if (edge.Attr.ArrowAtTarget) 
                ret.Add(new Line(dedge, (P2)edge.Attr.EdgeCurve.End, edge.Attr.ArrowAtTargetPosition, edge.Attr.LineWidth));
                
            
            
            if (edge.Attr.ArrowAtSource) 
                ret.Add(
                        new Line(dedge, edge.Attr.EdgeCurve.Start, edge.Attr.ArrowAtSourcePosition, edge.Attr.LineWidth));

            if (radiusForUnderlyingPolylineCorners>0)
                AddUnderlyingPolylineTessellation(ret, dedge, radiusForUnderlyingPolylineCorners);

            return ret;
            
        }


        private static void AddUnderlyingPolylineTessellation(List<ObjectWithBox> list, DEdge edge, double radiusForUnderlyingPolylineCorners) {

            P2 rad=new P2(radiusForUnderlyingPolylineCorners,radiusForUnderlyingPolylineCorners);
            IEnumerator<P2> en = edge.DrawingEdge.Attr.GeometryEdge.UnderlyingPolyline.GetEnumerator();
            en.MoveNext();
            P2 p=en.Current;
            list.Add(new Geometry(edge, new BBox(p+rad, p-rad)));
            while(en.MoveNext()){
                list.Add(new Line(edge, p, p=en.Current, edge.DrawingEdge.Attr.LineWidth));
                 list.Add(new Geometry(edge, new BBox(p+rad, p-rad)));
            }
           
        }
       
        static double epsilon=LayoutAlgorithmSettings.PointSize / 10;
   
    }
}
