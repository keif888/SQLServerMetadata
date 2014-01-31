using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// provides an API for drawing in a image
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renderer")]
    public sealed class GraphRenderer {
        object layedOutGraph;

        Microsoft.Msagl.Drawing.Graph graph; 
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="drGraph"></param>
        public GraphRenderer(Microsoft.Msagl.Drawing.Graph drGraph) {
            this.graph = drGraph;
        }


        /// <summary>
        /// calulates the layout
        /// </summary>
        public void CalculateLayout() {
            using (GViewer gv=new GViewer())
            layedOutGraph = gv.CalculateLayout(this.graph);
        }

        /// <summary>
        /// renders the graph on the image
        /// </summary>
        /// <param name="image"></param>
        public void Render(Image image) {
            if (image != null)
                Render(Graphics.FromImage(image), 0, 0, image.Width, image.Height);
        }

        /// <summary>
        /// Renders the graph inside of the rectangle xleft,ytop, width, height
        /// </summary>
        /// <param name="graphics">the graphics object</param>
        /// <param name="left">left of the rectangle</param>
        /// <param name="top">top of the rectangle</param>
        /// <param name="width">width of the rectangle</param>
        /// <param name="height">height of the rectangle</param>
        public void Render(System.Drawing.Graphics graphics, int left, int top, int width, int height) {
            Render(graphics,new Rectangle(left,top,width,height));
        }
        /// <summary>
        /// Renders the graph inside of the rectangle
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="rect"></param>
        public void Render(System.Drawing.Graphics graphics, System.Drawing. Rectangle rect) {
            if (graphics != null) {
                if (layedOutGraph == null)
                    CalculateLayout();

                double s = Math.Min(rect.Width / graph.Width, rect.Height / graph.Height);
                double xoffset = rect.Left + 0.5 * rect.Width - s * (graph.Left + 0.5 * graph.Width);
                double yoffset = rect.Top + 0.5 * rect.Height + s * (graph.Bottom + 0.5 * graph.Height);
                using (SolidBrush sb = new SolidBrush(Draw.MsaglColorToDrawingColor(graph.Attr.BackgroundColor)))
                    graphics.FillRectangle(sb, rect);

                using (System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix((float)s, 0, 0, (float)-s, (float)xoffset, (float)yoffset))
                    graphics.Transform = m;

                Draw.DrawPrecalculatedLayoutObject(graphics, layedOutGraph);
            }
        }

    }
}
