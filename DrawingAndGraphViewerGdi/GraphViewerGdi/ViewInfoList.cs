using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Msagl.GraphViewerGdi
{
  internal class ViewInfosList
  {
    internal void Reset() {
       current.prev = null;
       current.next = null;
       current.viewInfo = null;
    }

    internal bool IsThereMoreThanOnePreviousViewInfo() {
       if (current == null) { return false; }
       if (current.prev == null) { return false; }
       // Need to make sure that the viewInfo of the prev/next is also not null
       // this situation happens at the beginning when doing prev/prev on 3 zoom on the graph.
       if (current.prev.viewInfo== null) { return false; }
       return true;
    }
    internal bool IsThereMoreThanOneNextViewInfo() {
       if (current == null) { return false; }
       if (current.next == null) { return false; }
       // Need to make sure that the viewInfo of the prev/next is also not null
       // this situation happens at the beginning when doing prev/prev on 3 zoom on the graph.
       if (current.next.viewInfo == null) { return false; }
       return true;
    }

    ViewInfoHolder current = new ViewInfoHolder();

    internal ViewInfo CurrentView { get { return current.viewInfo; } }

    internal void AddNewViewInfo(ViewInfo viewInfo)
    {

      if (current.viewInfo == null || current.viewInfo != viewInfo)
      {

        //                    Log.W(viewInfo);

        ViewInfoHolder n = new ViewInfoHolder(viewInfo);
        current.next = n;
        n.prev = current;
        current = n;
      }

    }

    internal bool BackwardAvailable
    {
      get { return current.prev != null && current.prev.viewInfo != null; }
    }

    internal void Forward()
    {
      if (ForwardAvailable)
        current = current.next;
    }
    internal void Backward()
    {
      if (BackwardAvailable)
        current = current.prev;
    }

    internal bool ForwardAvailable
    {
      get { return current.next != null && current.next.viewInfo != null; }
    }
  }
}
