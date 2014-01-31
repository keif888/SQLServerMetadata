using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Msagl.GraphViewerGdi
{
  internal class ViewInfoHolder
  {
    internal ViewInfoHolder() { }
    internal ViewInfoHolder(ViewInfo vi) { this.viewInfo = vi; }
    internal ViewInfo viewInfo;
    internal ViewInfoHolder prev;
    internal ViewInfoHolder next;
  }

}
