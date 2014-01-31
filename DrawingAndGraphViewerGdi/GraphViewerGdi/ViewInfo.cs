using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Msagl.GraphViewerGdi
{
  internal class ViewInfo
  {
    public override int GetHashCode() { return 0; }
 
      public override bool Equals(object obj)
    {
      ViewInfo vi = obj as ViewInfo;
      if ((vi as object) == null)
        return false;
    return
      hVal == vi.hVal
      &&
      vVal == vi.vVal
      &&
      hLargeChange == vi.hLargeChange
      &&
      vLargeChange == vi.vLargeChange
      &&
      hScrollBarIsViz == vi.hScrollBarIsViz
      &&
      vScrollBarIsViz == vi.vScrollBarIsViz
      &&
      scaledDown == vi.scaledDown
      &&
      this.scaledDownCoefficient == vi.scaledDownCoefficient
        && this.zoomF == vi.zoomF;
    }

 


    internal bool leftMouseButtonWasPressed;

    public static bool operator !=(ViewInfo c1, ViewInfo c2)
    {
      if ((c1 as object) == null)
        return (c2 as object) != null;

      return !c1.Equals(c2);
    }

    public static bool operator ==(ViewInfo c1, ViewInfo c2)
    {
      if ((c1 as object) == null)
        return c2 as object == null;
      return c1.Equals(c2);
    }


      internal double zoomF;
    internal bool scaledDown;
    internal double scaledDownCoefficient;

    internal int hVal, vVal;
    internal int hLargeChange, vLargeChange;
    internal bool hScrollBarIsViz, vScrollBarIsViz;
  }

}
