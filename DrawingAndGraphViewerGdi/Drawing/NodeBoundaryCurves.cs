using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Msagl.Drawing;
using P2 = Microsoft.Msagl.Point;
using Microsoft.Msagl.Splines;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// a helper class for creation of node boundary curves
    /// </summary>
    public sealed class NodeBoundaryCurves {
        NodeBoundaryCurves() { }

        /// <summary>
        /// a helper function to creat a node boundary curve 
        /// </summary>
        /// <param name="node">the node</param>
        /// <param name="width">the node width</param>
        /// <param name="height">the node height</param>
        /// <returns></returns>
        public static ICurve GetNodeBoundaryCurve(Node node, double width, double height) {
            if (node == null)
                throw new InvalidOperationException();
            NodeAttr nodeAttr = node.Attr;

            switch (nodeAttr.Shape) {
                case Shape.Ellipse:
                case Shape.DoubleCircle:
                    return Microsoft.Msagl.Splines.CurveFactory.CreateEllipse(width, height, new P2(0, 0));
                case Shape.Circle: {
                        double r = Math.Max(width / 2, height / 2);
                        return Microsoft.Msagl.Splines.CurveFactory.CreateEllipse(r, r, new P2(0, 0));
                    }

                case Shape.Box:
                    return Microsoft.Msagl.Splines.CurveFactory.CreateBox(width, height, nodeAttr.XRadius, nodeAttr.YRadius, new P2(0, 0));


                case Shape.Diamond:
                    return Microsoft.Msagl.Splines.CurveFactory.CreateDiamond(
                      width, height, new P2(0, 0));

                case Shape.House:
                    return Microsoft.Msagl.Splines.CurveFactory.CreateHouse(width, height, new P2());

                case Shape.InvHouse:
                    return Microsoft.Msagl.Splines.CurveFactory.CreateInvHouse(width, height, new P2());
                case Shape.Octagon:
                    return Microsoft.Msagl.Splines.CurveFactory.CreateOctagon(width, height, new P2());
#if DEBUG
                case Shape.TestShape:
                    return Microsoft.Msagl.Splines.CurveFactory.CreateTestShape(width, height);
#endif

                default: {
                        if (node.Attr.GeometryNode == null || node.Attr.GeometryNode.BoundaryCurve == null)
                            return new Microsoft.Msagl.Splines.Ellipse(
                              new P2(width / 2, 0), new P2(0, height / 2), new P2());
                        else
                            return ScaleCurveToDescribeAroundRectangle(node.Attr.GeometryNode.BoundaryCurve.Translate(-node.Attr.GeometryNode.Center), width, height);
                    }
            }
        }

        static ICurve ScaleCurveToDescribeAroundRectangle(ICurve curve, double width, double height) {
            //create the rectangle curve
            Curve rect = new Curve();
            Curve.AddLineSegment(rect, -width / 2, height / 2, width / 2, height / 2);
            Curve.ContinueWithLineSegment(rect,width / 2, -height / 2);
            Curve.ContinueWithLineSegment(rect, -width / 2, -height / 2);
            Curve.CloseCurve(rect);

            //find scale big enough that the scaled curve will not intersect the rectangle width, height
            double upScale = Math.Max(width / curve.BBox.Width, height / curve.BBox.Height) * 2;
            const int numberOfTries = 10;
            int i;
            for (i = 0; i < numberOfTries; i++)
                if (CurvesIntersect(rect, ScaledCurve(upScale,curve)))
                    upScale *= 2;
                else break;
           
            if (i == numberOfTries)
                return null;

            double downScale = Math.Min(width / curve.BBox.Width, height / curve.BBox.Height) / 2;
            System.Diagnostics.Debug.Assert(!CurvesIntersect(rect, ScaledCurve(downScale, curve)));
            
            //run binary search to find the right scale
            const double eps = 0.01;
            System.Diagnostics.Debug.Assert(upScale>downScale);
            while (upScale - downScale > eps) {
                double s = (upScale + downScale) / 2;
                if (CurvesIntersect(rect, ScaledCurve(s, curve)))
                    downScale = s;
                else
                    upScale = s;
            }
            return ScaledCurve((downScale + upScale) / 2, curve);
        }

        static bool CurvesIntersect(Curve rect, ICurve curve) {
            return Curve.CurveCurveIntersect(rect, curve, false).Count > 0;
        }

        static ICurve ScaledCurve(double s, ICurve curve) {
            PlaneTransformation t = new PlaneTransformation(s, 0, 0, 0, s, 0);
            return curve.Transform(t);
        }

    }
}
