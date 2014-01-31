using System;
using System.Drawing.Printing;
namespace Microsoft.Msagl.GraphViewerGdi
{
	/// <summary>
	/// Summary description for Printing.
	/// </summary>
	public class GraphPrinting: PrintDocument
	{
		GViewer gViewer;
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="viewer"></param>
		public GraphPrinting(GViewer viewer )
		{
			this.gViewer=viewer;
		}
    /// <summary>
    /// This methods "draws" to the printer
    /// </summary>
    /// <param name="e"></param>
		protected override void OnPrintPage(PrintPageEventArgs e)
		{
			base.OnPrintPage (e);
			gViewer.DotOnPaint(e.Graphics,true);
		}



	}
}
