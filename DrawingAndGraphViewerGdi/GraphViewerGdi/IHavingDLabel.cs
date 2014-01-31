using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// an interface for objects having a label
    /// </summary>
    public interface IHavingDLabel {
        /// <summary>
        /// gets or sets the label
        /// </summary>
        DLabel Label {
            get;
            set;
        }
    }
}
