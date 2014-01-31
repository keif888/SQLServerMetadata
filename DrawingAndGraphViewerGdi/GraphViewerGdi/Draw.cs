using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Microsoft.Msagl.Drawing;

using Microsoft.Msagl.Splines;
using BBox = Microsoft.Msagl.Splines.Rectangle;
using DrawingGraph = Microsoft.Msagl.Drawing.Graph;
using GeometryEdge = Microsoft.Msagl.Edge;
using GeometryNode = Microsoft.Msagl.Node;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using P2=Microsoft.Msagl.Point;


namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// exposes some drawing functionality
    /// </summary>
    sealed public class Draw {
        /// <summary>
        /// private constructor
        /// </summary>
        Draw() { }


        static PointF P2ToPointF(P2 p) { return new PointF((float)p.X, (float)p.Y); }

        static private double doubleCircleOffsetRatio = 0.9;

        internal static double DoubleCircleOffsetRatio {
            get { return doubleCircleOffsetRatio; }
        }


        internal static float dashSize = 0.05f;//inches
/// <summary>
/// A color converter
/// </summary>
/// <param name="gleeColor"></param>
/// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Msagl")]
        static public System.Drawing.Color MsaglColorToDrawingColor(Microsoft.Msagl.Drawing.Color gleeColor) {
            return System.Drawing.Color.FromArgb(gleeColor.A, gleeColor.R, gleeColor.G, gleeColor.B);
        }


       
        /// <summary>
        /// Drawing that can be performed on any Graphics object
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="precalculatedObject"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "precalculated"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Precalculated")]
        static public void DrawPrecalculatedLayoutObject(Graphics graphics, object precalculatedObject) {
            DGraph dg = precalculatedObject as DGraph;
            if (dg != null)
                dg.DrawGraph(graphics);
        }
      
#if DEBUGGLEE
        internal static bool DrawDebugStuff(Graphics g, DGraph graphToDraw, Pen myPen) {
            bool debugDrawing = false;

            if (graphToDraw.DrawingGraph.debugPolylines != null)
            {
                foreach (Polyline p in graphToDraw.DrawingGraph.debugPolylines)
                {
                    SetColor(graphToDraw, myPen, p);
                    if (p.Closed)
                        g.FillPolygon(new SolidBrush(myPen.Color), GetPolylinePoints(p));
                    else
                        g.DrawLines(myPen, GetPolylinePoints(p));
                 
                }

            }

            if (graphToDraw.DrawingGraph.DebugLines != null) {
                debugDrawing = true;
                foreach (LineSegment bs in graphToDraw.DrawingGraph.DebugLines) {
                    SetColor(graphToDraw, myPen, bs);
                    g.DrawLine(myPen, (float)bs.Start.X, (float)bs.Start.Y, (float)bs.End.X, (float)bs.End.Y);
                }
            }
            if (graphToDraw.DrawingGraph.DebugBezierCurves != null) {
                debugDrawing = true;
                foreach (CubicBezierSegment bs in graphToDraw.DrawingGraph.DebugBezierCurves) {
                    SetColor(graphToDraw, myPen, bs);
                    g.DrawBezier(myPen, (float)bs.B(0).X, (float)bs.B(0).Y,
                     (float)bs.B(1).X, (float)bs.B(1).Y,
                                (float)bs.B(2).X, (float)bs.B(2).Y,
                                (float)bs.B(3).X, (float)bs.B(3).Y);
                    if (graphToDraw.DrawingGraph.ShowControlPoints)
                        DrawControlPoints(g, bs);
                }
            }

            if (graphToDraw.DrawingGraph.DebugEllipses != null) {
                debugDrawing = true;

                foreach (Ellipse el in graphToDraw.DrawingGraph.DebugEllipses) {
                    SetColor(graphToDraw, myPen, el);
                    g.DrawEllipse(myPen, (float)(el.Center.X - el.AxisA.X), (float)(el.Center.Y - el.AxisB.Y), (float)(el.AxisA.X * 2), Math.Abs((float)el.AxisB.Y * 2));
                }
            }

            myPen.Color = System.Drawing.Color.Red;
            if (graphToDraw.DrawingGraph.DebugBoxes != null) {
                debugDrawing = true;
                
                foreach (Parallelogram pb in graphToDraw.DrawingGraph.DebugBoxes) {
                    SetColor(graphToDraw, myPen, pb);
                    PointF[] ps = new PointF[4];
                    for (int i = 0; i < 4; i++)
                        ps[i] = new PointF((float)pb.Vertex((VertexId)i).X, (float)pb.Vertex((VertexId)i).Y);
                    g.DrawPolygon(myPen, ps);
                }
            }
            return debugDrawing;
        }

        private static System.Drawing.Point[] GetPolylinePoints(Polyline p)
        {
            
            List<System.Drawing.Point> ret=new List<System.Drawing.Point>();
            foreach(Microsoft.Msagl.Point pnt in p){
                ret.Add(new System.Drawing.Point((int)pnt.X,(int)pnt.Y));
            }
            return ret.ToArray();
        }

        private static void SetColor(DGraph graphToDraw, Pen myPen, object bs)
        {
            Microsoft.Msagl.Drawing.Color color;
            if (graphToDraw.DrawingGraph.ColorDictionary.TryGetValue(bs, out color))
                myPen.Color = MsaglColorToDrawingColor(color);
            else
                myPen.Color = System.Drawing.Color.Blue;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString")]
        internal static void DrawDataBase(Graphics g, Pen myPen, DrawingGraph dg) {
            int i = 0;

                foreach (Anchor p in dg.DataBase.Anchors)
                    i = DrawAnchor(g, myPen, dg, i, p);

            
            myPen.Color = System.Drawing.Color.Blue;
            foreach (List<IntEdge> edges in dg.DataBase.Multiedges.Values)
                foreach (IntEdge e in edges)
                    foreach (LayerEdge le in e.LayerEdges) {
                        g.DrawLine(myPen, PointF(dg.DataBase.Anchors[le.Source].Origin),
                        PointF(dg.DataBase.Anchors[le.Target].Origin));
                    }
            
            
            myPen.Color = System.Drawing.Color.Red;
            if (dg.DataBase.nodesToShow == null)
                foreach (List<IntEdge> li in dg.DataBase.Multiedges.Values)
                    foreach (IntEdge ie in li)
                        if (ie.Edge.Curve is Curve) {
                            foreach (ICurve s in (ie.Edge.Curve as Curve).Segments) {
                                CubicBezierSegment bs = s as CubicBezierSegment;
                                if (bs != null) {


                                    g.DrawBezier(myPen, (float)bs.B(0).X, (float)bs.B(0).Y,
                                                   (float)bs.B(1).X, (float)bs.B(1).Y,
                                                   (float)bs.B(2).X, (float)bs.B(2).Y,
                                                   (float)bs.B(3).X, (float)bs.B(3).Y);

                                } else {
                                    LineSegment ls = s as LineSegment;
                                    g.DrawLine(myPen, (float)ls.Start.X, (float)ls.Start.Y,
                                                                       (float)ls.End.X, (float)ls.End.Y);

                                }
                            }

                            myPen.Color = System.Drawing.Color.Brown;
                            foreach(LineSegment ls in ie.Edge.UnderlyingPolyline.GetSegs())
                                g.DrawLine(myPen, (float)ls.Start.X, (float)ls.Start.Y,
                                                                    (float)ls.End.X, (float)ls.End.Y);
                            myPen.Color = System.Drawing.Color.Red;
                        }
        }
        

        

        private static int DrawAnchor(Graphics g, Pen myPen, DrawingGraph dg, int i, Anchor p) {

            string stringToShow = i.ToString() + " " + p.Id;

            DrawStringInRectCenter(g, Brushes.Blue, new Font(FontFamily.GenericSerif, 10), stringToShow, new RectangleF((float)p.Left, (float)p.Bottom, (float)p.RightAnchor + (float)p.LeftAnchor, (float)p.TopAnchor + (float)p.BottomAnchor));
            i++;
            return i;
        }

        private static void DrawControlPoints(Graphics g, CubicBezierSegment bs) {
            using (Pen pen = new Pen(System.Drawing.Color.Green, (float)(1 / 1000.0))) {
                //  pen.DashStyle = DashStyle.DashDot;
                g.DrawLine(pen, PointF(bs.B(0)), PointF(bs.B(1)));
                g.DrawLine(pen, PointF(bs.B(1)), PointF(bs.B(2)));
                g.DrawLine(pen, PointF(bs.B(2)), PointF(bs.B(3)));
            }
        }
#endif

      
        internal static void AddStyleForPen(DObject dObj, Pen myPen, Style style) {
            if (style == Style.Dashed) {

                myPen.DashStyle = DashStyle.Dash;

                if (dObj.DashPatternArray == null) {
                    float f = dObj.DashSize();
                    dObj.DashPatternArray = new float[] { f, f };
                }
                myPen.DashPattern = dObj.DashPatternArray;

                myPen.DashOffset = dObj.DashPatternArray[0];
            } else if (style == Style.Dotted) {
                myPen.DashStyle = DashStyle.Dash;
                if (dObj.DashPatternArray == null) {
                    float f = dObj.DashSize();
                    dObj.DashPatternArray = new float[] { 1, f };
                }
                myPen.DashPattern = dObj.DashPatternArray;
            }
        }

        internal static void DrawEdgeArrows(Graphics g, DrawingEdge edge, System.Drawing.Color edgeColor, Pen myPen) {
            ArrowAtTheEnd(g, edge, edgeColor, myPen);
            ArrawAtTheBeginning(g,  edge, edgeColor, myPen);
        }

        private static void ArrawAtTheBeginning(Graphics g, DrawingEdge edge, System.Drawing.Color edgeColor, Pen myPen) {
            if (edge.Attr.GeometryEdge != null && edge.Attr.ArrowAtSource)
                DrawArrowAtTheBeginningWithControlPoints(g, edge, edgeColor, myPen);
        }


        private static void DrawArrowAtTheBeginningWithControlPoints(Graphics g, DrawingEdge edge, System.Drawing.Color edgeColor, Pen myPen) {
            if (edge.Attr.ArrowheadAtSource == ArrowStyle.None)
                DrawLine(g, myPen, edge.Attr.EdgeCurve.Start,
                  edge.Attr.ArrowAtSourcePosition);
            else using (SolidBrush sb = new SolidBrush(edgeColor))
                    DrawArrow(g, sb, edge.Attr.EdgeCurve.Start,
                      edge.Attr.ArrowAtSourcePosition, edge.Attr.LineWidth, edge.Attr.ArrowheadAtSource);
        }

        private static void ArrowAtTheEnd(Graphics g, DrawingEdge edge, System.Drawing.Color edgeColor, Pen myPen) {
            if (edge.Attr.GeometryEdge != null && edge.Attr.ArrowAtTarget)
                DrawArrowAtTheEndWithControlPoints(g, edge, edgeColor, myPen);
        }

        private static void DrawArrowAtTheEndWithControlPoints(Graphics g, DrawingEdge edge, System.Drawing.Color edgeColor, Pen myPen) {
            if (edge.Attr.ArrowheadAtTarget == ArrowStyle.None)
                DrawLine(g, myPen, (P2)edge.Attr.EdgeCurve.End,
                  edge.Attr.ArrowAtTargetPosition);
            else using (SolidBrush sb = new SolidBrush(edgeColor))
                    DrawArrow(g, sb, (P2)edge.Attr.EdgeCurve.End,
                  edge.Attr.ArrowAtTargetPosition, edge.Attr.LineWidth, edge.Attr.ArrowheadAtTarget);
        }


        internal static void CreateGraphicsPath(DEdge dedge) {
            DrawingEdge edge = dedge.DrawingEdge;
            dedge.GraphicsPath = new GraphicsPath();

            Curve c = edge.Attr.EdgeCurve as Curve;
            if (c != null) {
                foreach (ICurve seg in c.Segments) {

                    CubicBezierSegment cubic = seg as CubicBezierSegment;
                    if (cubic != null)
                        dedge.GraphicsPath.AddBezier(PointF(cubic.B(0)), PointF(cubic.B(1)), PointF(cubic.B(2)), PointF(cubic.B(3)));
                    else {
                        LineSegment ls = seg as LineSegment;
                        dedge.GraphicsPath.AddLine(PointF(ls.Start), PointF(ls.End));
                    }
                }
            } else {
                LineSegment ls = edge.Attr.EdgeCurve as LineSegment;
                if (ls != null)
                    dedge.GraphicsPath.AddLine(PointF(ls.Start), PointF(ls.End));
                else {
                    CubicBezierSegment seg = (CubicBezierSegment)edge.Attr.EdgeCurve;
                    dedge.GraphicsPath.AddBezier(PointF(seg.B(0)), PointF(seg.B(1)), PointF(seg.B(2)), PointF(seg.B(3)));
                }
            }
        }        

        static bool NeedToFill(System.Drawing.Color fillColor) {
            return fillColor.A != 0; //the color is not transparent
        }

        static internal void DrawDoubleCircle(Graphics g, Pen pen, DNode dNode) {
            NodeAttr nodeAttr = dNode.DrawingNode.Attr;
            double x = (double)nodeAttr.Pos.X - (double)nodeAttr.Width / 2.0f;
            double y = (double)nodeAttr.Pos.Y - (double)nodeAttr.Height / 2.0f;
            if (NeedToFill(dNode.FillColor)) {
                g.FillEllipse(new SolidBrush(dNode.FillColor), (float)x, (float)y, (float)nodeAttr.Width, (float)nodeAttr.Height);
            }

            g.DrawEllipse(pen, (float)x, (float)y, (float)nodeAttr.Width, (float)nodeAttr.Height);
            float w = (float)nodeAttr.Width;
            float h = (float)nodeAttr.Height;
            float m = Math.Max(w, h);
            float coeff = (float)1.0 - (float)(DoubleCircleOffsetRatio);
            x += coeff * m / 2.0;
            y += coeff * m / 2.0;
            g.DrawEllipse(pen, (float)x, (float)y, w - coeff*m, h-coeff*m);

        }

        private static System.Drawing.Color FillColor(NodeAttr nodeAttr) {
            return MsaglColorToDrawingColor(nodeAttr.FillColor);
        }

        const double arrowAngle = 25.0;//degrees
        static internal void DrawArrow(Graphics g, Brush brush, P2 start, P2 end, int lineWidth, ArrowStyle arrowStyle) {

            switch (arrowStyle) {
                case ArrowStyle.NonSpecified:
                case ArrowStyle.Normal:

                    DrawNormalArrow(g, brush, ref start, ref end, lineWidth);
                    break;
                case ArrowStyle.Tee:
                    DrawTeeArrow(g, brush, ref start, ref end, lineWidth);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private static void DrawNormalArrow(Graphics g, Brush brush, ref P2 start, ref P2 end, int lineWidth) {
            PointF[] points;

            if (lineWidth == 1) {
                P2 dir = end - start;
                P2 h = dir;
                dir /= dir.Length;

                P2 s = new P2(-dir.Y, dir.X);

                s *= h.Length * ((float)Math.Tan(arrowAngle * 0.5f * (Math.PI / 180.0)));

                points = new PointF[] { P2ToPointF(start + s), P2ToPointF(end), P2ToPointF(start - s) };
            } else {

                P2 dir = end - start;
                P2 h = dir;
                dir /= dir.Length;
                P2 s = new P2(-dir.Y, dir.X);
                float w = (float)(0.5f * lineWidth);
                P2 s0 = w * s;
                double al = arrowAngle * 0.5f * (double)(Math.PI / 180.0);
                s *= h.Length * ((float)Math.Tan(al));
                s += s0;

                points = new PointF[] { P2ToPointF(start + s), P2ToPointF(start - s), P2ToPointF(end - s0), P2ToPointF(end + s0) };
                P2 center = end - dir * w * (float)Math.Tan(al);
                double rad = w / (double)Math.Cos(al);
                g.FillEllipse(brush,
                              (float)center.X - (float)rad,
                              (float)center.Y - (float)rad,
                              2.0f * (float)rad,
                              2.0f * (float)rad);
            }


            g.FillPolygon(brush, points);
        }

        private static void DrawTeeArrow(Graphics g, Brush brush, ref P2 start, ref P2 end, int lineWidth) {
            double lw = lineWidth == -1 ? 1 : lineWidth;
            using (Pen p = new Pen(brush, (float)lw)) {
                g.DrawLine(p, P2ToPointF(start), P2ToPointF(end));
                P2 dir = end - start;
                P2 h = dir;
                dir /= dir.Length;

                P2 s = new P2(-dir.Y, dir.X);

                s *= 2 * h.Length * ((float)Math.Tan(arrowAngle * 0.5f * (Math.PI / 180.0)));
                s += (1 + lw) * s.Normalize();

                g.DrawLine(p, P2ToPointF(start + s), P2ToPointF(start - s));
            }

        }

        static internal void DrawLine(Graphics g, Pen pen, P2 start, P2 end) {

            g.DrawLine(pen, P2ToPointF(start), P2ToPointF(end));

        }


        static internal void DrawBox(Graphics g, Pen pen, DNode dNode) {
            NodeAttr nodeAttr = dNode.DrawingNode.Attr;
            if (nodeAttr.XRadius == 0 || nodeAttr.YRadius == 0) {
                double x = (double)nodeAttr.Pos.X - (double)nodeAttr.Width / 2.0f;
                double y = (double)nodeAttr.Pos.Y - (double)nodeAttr.Height / 2.0f;

                if (NeedToFill(dNode.FillColor)) {
                    System.Drawing.Color fc = FillColor(nodeAttr);
                    g.FillRectangle(new SolidBrush(fc), (float)x, (float)y, (float)nodeAttr.Width, (float)nodeAttr.Height);
                }

                g.DrawRectangle(pen, (float)x, (float)y, (float)nodeAttr.Width, (float)nodeAttr.Height);
            } else {
                float width = (float)nodeAttr.Width;
                float height = (float)nodeAttr.Height;
                float xRadius = (float)nodeAttr.XRadius;
                float yRadius = (float)nodeAttr.YRadius;
                using (GraphicsPath path = new GraphicsPath()) {
                    FillTheGraphicsPath(nodeAttr, width, height, ref xRadius, ref yRadius, path);

                    if (NeedToFill(dNode.FillColor)) {
                        g.FillPath(new SolidBrush(dNode.FillColor), path);
                    }


                    g.DrawPath(pen, path);
                }
            }
        }

        private static void FillTheGraphicsPath(NodeAttr nodeAttr, float width, float height, ref float xRadius, ref float yRadius, GraphicsPath path) {
            float w = (float)(width / 2);
            if (xRadius > w)
                xRadius = w;
            float h = (float)(height / 2);
            if (yRadius > h)
                yRadius = h;
            float x = (float)nodeAttr.Pos.X;
            float y = (float)nodeAttr.Pos.Y;
            float ox = w - xRadius;
            float oy = h - yRadius;
            float top = y + h;
            float bottom = y - h;
            float left = x - w;
            float right = x + w;

            const float PI = 180;
            if (ox > 0)
                path.AddLine(x - ox, bottom, x + ox, bottom);
            path.AddArc(x + ox - xRadius, y - oy - yRadius, 2 * xRadius, 2 * yRadius, 1.5f * PI, 0.5f * PI);

            if (oy > 0)
                path.AddLine(right, y - oy, right, y + oy);
            path.AddArc(x + ox - xRadius, y + oy - yRadius, 2 * xRadius, 2 * yRadius, 0, 0.5f * PI);
            if (ox > 0)
                path.AddLine(x + ox, top, x - ox, top);
            path.AddArc(x - ox - xRadius, y + oy - yRadius, 2 * xRadius, 2 * yRadius, 0.5f * PI, 0.5f * PI);
            if (oy > 0)
                path.AddLine(left, y + oy, left, y - oy);
            path.AddArc(x - ox - xRadius, y - oy - yRadius, 2 * xRadius, 2 * yRadius, PI, 0.5f * PI);
        }


        static internal void DrawDiamond(Graphics g, Pen pen, DNode dNode) {
            NodeAttr nodeAttr = dNode.DrawingNode.Attr;
            double w2 = (double)nodeAttr.Width / 2.0f;
            double h2 = (double)nodeAttr.Height / 2.0f;
            double cx = (double)nodeAttr.Pos.X;
            double cy = (double)nodeAttr.Pos.Y;
            System.Drawing.PointF[] ps = new System.Drawing.PointF[]
	  {
																	 
		  new PointF((float)cx-(float)w2,(float)cy),new PointF((float)cx,(float)cy+(float)h2),new PointF((float)cx+(float)w2,(float)cy),new PointF((float)cx,(float)cy-(float)h2)
	  };

            if (NeedToFill(dNode.FillColor)) {
                System.Drawing.Color fc = FillColor(nodeAttr);
                g.FillPolygon(new SolidBrush(fc), ps);

            }

            g.DrawPolygon(pen, ps);



        }

        static internal void DrawEllipse(Graphics g, Pen pen, DNode dNode) {
            NodeAttr nodeAttr=dNode.DrawingNode.Attr;
            double x = (double)nodeAttr.Pos.X - (double)nodeAttr.Width / 2.0f;
            double y = (double)nodeAttr.Pos.Y - (double)nodeAttr.Height / 2.0f;

            if (NeedToFill(dNode.FillColor)) {
                g.FillEllipse(new SolidBrush(dNode.FillColor), (float)x, (float)y, (float)nodeAttr.Width, (float)nodeAttr.Height);
            }
            if (nodeAttr.Shape == Shape.Point) {
                g.FillEllipse(new SolidBrush(pen.Color), (float)x, (float)y, (float)nodeAttr.Width, (float)nodeAttr.Height);
            }

            g.DrawEllipse(pen, (float)x, (float)y, (float)nodeAttr.Width, (float)nodeAttr.Height);

        }

        //static internal void DrawGraphBBox(Graphics g,DGraph graphToDraw)
        //{

        //  foreach( Style style in graphToDraw.DrawingGraph.GraphAttr.Styles)
        //  {
        //    if(style==Style.Filled)
        //    {
        //      BBox bb=graphToDraw.DrawingGraph.GraphAttr.BoundingBox;

        //      g.FillRectangle(
        //        new SolidBrush(System.Drawing.Color.LightSteelBlue),
        //        (float)bb.LeftTop.X,(float)bb.LeftTop.Y,(float)bb.RightBottom.X-(float)bb.LeftTop.X,-(float)bb.RightBottom.Y+(float)bb.RightBottom.Y);

        //      return;
        //    }
        //  }

        //  if(!(graphToDraw.DrawingGraph.GraphAttr.Backgroundcolor.A==0))
        //  {
        //    BBox bb=graphToDraw.DrawingGraph.GraphAttr.BoundingBox;

        //    SolidBrush brush=new SolidBrush((MsaglColorToDrawingColor( graphToDraw.DrawingGraph.GraphAttr.Backgroundcolor)));

        //    if(!bb.IsEmpty)
        //      g.FillRectangle(brush, 
        //      (float)	bb.LeftTop.X,(float)bb.LeftTop.Y,(float)bb.RightBottom.X-(float)bb.LeftTop.X,-(float)bb.LeftTop.Y+(float)bb.RightBottom.Y);
        //  }

        //}


//don't know what to do about the throw-catch block
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        static internal void DrawLabel(Graphics g, DLabel label) {
            if (label == null)
                return;

            try {
                using (SolidBrush sb = new SolidBrush(MsaglColorToDrawingColor(label.DrawingLabel.FontColor))) {
                    DrawStringInRectCenter(g, sb,
                    label.Font, label.DrawingLabel.Text,
                                                           new RectangleF((float)label.DrawingLabel.Left, (float)label.DrawingLabel.Bottom,
                                                                          (float)label.DrawingLabel.Size.Width, (float)label.DrawingLabel.Size.Height));
                }
            } catch { }

            if (label.MarkedForDragging) {
                Pen pen = new Pen(MsaglColorToDrawingColor(label.DrawingLabel.FontColor));
                pen.DashStyle = DashStyle.Dot;
                DrawLine(g, pen, label.DrawingLabel.GeometryLabel.AttachmentSegmentStart,
                    label.DrawingLabel.GeometryLabel.AttachmentSegmentEnd);
            }
        }

        static void DrawStringInRectCenter(Graphics g, Brush brush, Font f, string s, RectangleF r /*, double rectLineWidth*/) {
            if (String.IsNullOrEmpty(s))
                return;

            using (System.Drawing.Drawing2D.Matrix m = g.Transform) {

                using (System.Drawing.Drawing2D.Matrix saveM = m.Clone()) {

                    //rotate the label around its center
                    float c = (r.Bottom + r.Top) / 2;

                    using (System.Drawing.Drawing2D.Matrix m2 = new System.Drawing.Drawing2D.Matrix(1, 0, 0, -1, 0, 2 * c)) {
                        m.Multiply(m2);
                    }
                    g.Transform = m;
                    using (StringFormat stringFormat = StringFormat.GenericTypographic)
                    {
                        g.DrawString(s, f, brush, r.Left, r.Top, stringFormat);
                    }
                    g.Transform = saveM;
                }
            }

        }
 
        static internal PointF PointF(P2 p) { return new PointF((float)p.X, (float)p.Y); }


        internal static void DrawFromMsaglCurve(Graphics g, Pen pen, DNode dNode) {
            NodeAttr attr=dNode.DrawingNode.Attr;
            Curve c = (Curve)attr.GeometryNode.BoundaryCurve;
            GraphicsPath p = new GraphicsPath();
            foreach (ICurve seg in c.Segments)
                AddSegToPath(seg, ref p);

            if (NeedToFill(dNode.FillColor)) {
                g.FillPath(new SolidBrush(dNode.FillColor), p);
            }
            g.DrawPath(pen, p);
        }

        private static void AddSegToPath(ICurve seg, ref GraphicsPath p) {
            LineSegment line = seg as LineSegment;
            if (line != null)
                p.AddLine(PointF(line.Start), PointF(line.End));
            else {
                CubicBezierSegment cb = seg as CubicBezierSegment;
                p.AddBezier(PointF(cb.B(0)), PointF(cb.B(1)), PointF(cb.B(2)), PointF(cb.B(3)));
            }
        }
    }
}

