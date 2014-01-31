using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// An enumeration representing layout progress.
    /// </summary>
    public enum LayoutProgress {
        /// <summary>
        /// Layout phase.
        /// </summary>
        LayingOut,

        /// <summary>
        /// Rendering phase.
        /// </summary>
        Rendering,

        /// <summary>
        /// Finished.
        /// </summary>
        Finished,

        /// <summary>
        /// Aborted.
        /// </summary>
        Aborted,


    }
}
