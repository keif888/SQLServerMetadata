using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Msagl.Splines;

namespace Microsoft.Msagl.GraphViewerGdi {
#if DEBUGGLEE
    /// <summary>
    /// draws curved for debugging purpoposes
    /// </summary>
    public class DisplayGLEEGraph {

        /// <summary>
        /// displays an array of curves
        /// </summary>
        /// <param name="curves"></param>
        static public void ShowCurves(params Microsoft.Msagl.Splines.ICurve[] curves) {
            Microsoft.Msagl.Drawing.Graph g = new Microsoft.Msagl.Drawing.Graph("");
            ShowCurvesWithColorsSet(curves, g);
        }

        private static void ShowCurvesWithColorsSet(IEnumerable<Microsoft.Msagl.Splines.ICurve> curves,
            Microsoft.Msagl.Drawing.Graph g) {
            AllocateDebugCurves(g);
            //   g.ShowControlPoints = true;

            Microsoft.Msagl.Splines.Rectangle graphBox = new Microsoft.Msagl.Splines.Rectangle();

            AddCurvesToGraph(curves, g);
            bool firstTime = true;
            foreach (Microsoft.Msagl.Splines.ICurve c0 in curves) {
                if (c0 != null) {
                    Microsoft.Msagl.Splines.Parallelogram b = c0.ParallelogramNodeOverICurve.Parallelogram;

                    for (int i = 0; i < 4; i++) {
                        if (firstTime) {
                            firstTime = false;
                            graphBox = new Rectangle(b.Vertex((VertexId)i));
                        }
                        graphBox.Add(b.Vertex((VertexId)i));
                    }
                }
            }

            Microsoft.Msagl.Point del = (graphBox.LeftBottom - graphBox.RightTop) / 10;
            graphBox.Add(graphBox.LeftBottom + del);
            graphBox.Add(graphBox.RightTop - del);
            GeometryGraph gg = new GeometryGraph();
            gg.BoundingBox = graphBox;
            g.GeometryGraph = gg;
            try {
                DisplayGraph(g);
            } catch(Exception e) {
                Console.WriteLine(e);

            }
        }

        /// <summary>
        /// display colorored curves
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="colors"></param>
        static public void ShowCurvesWithColors(IEnumerable<Splines.ICurve> curves, IEnumerable<string> colors) {

            Microsoft.Msagl.Drawing.Graph g = new Microsoft.Msagl.Drawing.Graph("");
            AddColorsToGraph(colors, g);
            ShowCurvesWithColorsSet(curves, g);
        }

        private static void AddColorsToGraph(IEnumerable<string> colors, Microsoft.Msagl.Drawing.Graph g) {
            g.DebugColors = new List<Microsoft.Msagl.Drawing.Color>();
            foreach (string s in colors) {
                if (s.ToLower() == "green")
                    g.DebugColors.Add(Microsoft.Msagl.Drawing.Color.Green);
                else if (s.ToLower() == "black")
                    g.DebugColors.Add(Microsoft.Msagl.Drawing.Color.Black);
                else if (s.ToLower() == "red")
                    g.DebugColors.Add(Microsoft.Msagl.Drawing.Color.Red);    
                else if(s.ToLower()=="blue") {
                    g.DebugColors.Add(Microsoft.Msagl.Drawing.Color.Blue);
                }
                
            }

        }

        private static void AllocateDebugCurves(Microsoft.Msagl.Drawing.Graph g) {
            g.DebugBezierCurves = new List<Microsoft.Msagl.Splines.CubicBezierSegment>();
            g.DebugBoxes = new List<Parallelogram>();
            g.DebugEllipses = new List<Ellipse>();
            g.DebugLines = new List<LineSegment>();
        }

        private static void AddCurvesToGraph(IEnumerable<ICurve> curves, Microsoft.Msagl.Drawing.Graph g)
        {
            int i = 0;
            foreach (Microsoft.Msagl.Splines.ICurve c0 in curves)
            {
                Microsoft.Msagl.Drawing.Color col = Microsoft.Msagl.Drawing.Color.Black;
                if (g.DebugColors != null)
                    col = g.DebugColors[i++];
                if (c0 != null)
                {
                    if (c0 is Microsoft.Msagl.Splines.Curve)
                    {

                        foreach (Microsoft.Msagl.Splines.ICurve cs in (c0 as Microsoft.Msagl.Splines.Curve).Segments)
                        {
                            if (cs is Microsoft.Msagl.Splines.CubicBezierSegment)
                            {
                                g.DebugBezierCurves.Add(cs as Microsoft.Msagl.Splines.CubicBezierSegment);
                            }
                            else if (cs is Microsoft.Msagl.Splines.LineSegment)
                            {
                                Microsoft.Msagl.Splines.LineSegment ls = cs as Microsoft.Msagl.Splines.LineSegment;
                                g.DebugLines.Add(ls);
                            } else if (cs is Microsoft.Msagl.Splines.Ellipse) {
                                g.DebugEllipses.Add(cs as Ellipse);
                            }
                            g.ColorDictionary[cs] = col;
                        }
                    }
                    else
                    {
                        g.ColorDictionary[c0] = Microsoft.Msagl.Drawing.Color.Blue;
                        if (c0 is Microsoft.Msagl.Splines.LineSegment)
                        {
                            Microsoft.Msagl.Splines.LineSegment ls = c0 as Microsoft.Msagl.Splines.LineSegment;
                            g.DebugLines.Add(ls);
                        }
                        else if (c0 is Microsoft.Msagl.Splines.CubicBezierSegment)
                        {
                            g.DebugBezierCurves.Add(c0 as Microsoft.Msagl.Splines.CubicBezierSegment);
                        }
                        else if (c0 is Microsoft.Msagl.Splines.Ellipse)
                            g.DebugEllipses.Add(c0 as Microsoft.Msagl.Splines.Ellipse);
                        else if (c0 is Microsoft.Msagl.Splines.BSpline)
                        {
                            Curve cc = ((Microsoft.Msagl.Splines.BSpline)c0).ToBezier();

                            foreach (CubicBezierSegment bseg in cc.Segments)
                                g.DebugBezierCurves.Add(bseg);

                        }
                        else
                        {
                            Polyline poly = c0 as Polyline;
                            g.ColorDictionary[poly] = Microsoft.Msagl.Drawing.Color.Brown;
                            g.debugPolylines.Add(poly); 
                        }
                    }
                }
            }
        }


        /// <summary>
        /// displays the database
        /// </summary>
        /// <param name="db"></param>
        /// <param name="curves"></param>
        static public void ShowDataBase(Database db,params Splines.ICurve[] curves) {

            Microsoft.Msagl.Drawing.Graph g = new Microsoft.Msagl.Drawing.Graph("");
            AllocateDebugCurves(g);

            Microsoft.Msagl.Splines.Rectangle graphBox = new Microsoft.Msagl.Splines.Rectangle(db.Anchors[0].LeftTop);
       
            List<ICurve> cl = new List<ICurve>(curves);
       
            foreach (Anchor a in db.Anchors) {
                graphBox.Add(a.LeftTop);
                graphBox.Add(a.RightBottom);
                cl.Add(a.PolygonalBoundary);
            }

            AddCurvesToGraph(cl, g);

            Microsoft.Msagl.Point del = (graphBox.LeftBottom - graphBox.RightTop) / 10;
            graphBox.Add(graphBox.LeftBottom + del);
            graphBox.Add(graphBox.RightTop - del);
            GeometryGraph gg = new GeometryGraph();
            gg.BoundingBox = graphBox;
            g.DataBase = db;
            g.GeometryGraph = gg;
            DisplayGraph(g);
            db.nodesToShow = null;
        }



        private static void DisplayGraph(Microsoft.Msagl.Drawing.Graph g) {
            System.Windows.Forms.Form f = new System.Windows.Forms.Form();
            Microsoft.Msagl.GraphViewerGdi.GViewer gviewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            gviewer.BuildHitTree = false;
            f.Controls.Add(gviewer);
            gviewer.Dock = System.Windows.Forms.DockStyle.Fill;
            gviewer.MouseMove += new System.Windows.Forms.MouseEventHandler(gviewer_MouseMove);
            gviewer.NeedToCalculateLayout = false;
            gviewer.MouseClick += new System.Windows.Forms.MouseEventHandler(gviewer_MouseClick);

            tt.Active = true;
            tt.ShowAlways = true;
         

            f.Size = new System.Drawing.Size(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width*3/4 , System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height*3/4 );
            f.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            f.TopLevel = true;
            gviewer.Graph = g;

            f.ShowDialog();
        }

        static void gviewer_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e) {
            float viewerX, viewerY;
            Microsoft.Msagl.GraphViewerGdi.GViewer gviewer = sender as Microsoft.Msagl.GraphViewerGdi.GViewer;
            if (gviewer != null) {
                gviewer.ScreenToSource(e.Location.X, e.Location.Y, out viewerX, out viewerY);
                System.Windows.Forms.MessageBox.Show(String.Format("{0} {1}", viewerX, viewerY));
            }
        }

        static System.Windows.Forms.ToolTip tt = new System.Windows.Forms.ToolTip();

        static void gviewer_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
            Microsoft.Msagl.GraphViewerGdi.GViewer gviewer = sender as Microsoft.Msagl.GraphViewerGdi.GViewer;
            float viewerX, viewerY;
            if (gviewer != null) {
                gviewer.ScreenToSource(e.Location.X, e.Location.Y, out viewerX, out viewerY);
                gviewer.SetToolTip(tt, new Point(viewerX, viewerY).ToString());
                tt.ShowAlways = true;
            }
        }
/// <summary>
/// 
/// </summary>
        public static void SetShowFunctions() {
            Microsoft.Msagl.LayoutAlgorithmSettings.Show = new Microsoft.Msagl.Show(Microsoft.Msagl.GraphViewerGdi.DisplayGLEEGraph.ShowCurves);
            Microsoft.Msagl.LayoutAlgorithmSettings.ShowDatabase = new Microsoft.Msagl.ShowDatabase(Microsoft.Msagl.GraphViewerGdi.DisplayGLEEGraph.ShowDataBase);
            Microsoft.Msagl.LayoutAlgorithmSettings.ShowWithColors = new ShowWithColors(Microsoft.Msagl.GraphViewerGdi.DisplayGLEEGraph.ShowCurvesWithColors);
        }
    }
#endif
}
