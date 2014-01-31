using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// A class representing arguments of the layout progress.
    /// </summary>
    public class LayoutProgressEventArgs : EventArgs {
        internal LayoutProgress progress;
        internal string diagnostics;
        /// <summary>
        /// returning LayoutProgress
        /// </summary>
        public LayoutProgress Progress {
            get { return progress; }
        }
        /// <summary>
        /// Returning diagnostics (in case aborted because of a crash)
        /// </summary>
        public string Diagnostics {
            get { return diagnostics; }
        }

        internal LayoutProgressEventArgs(LayoutProgress progress, string diagnostics) {
            this.progress = progress;
            this.diagnostics = diagnostics;
        }
    }
}
