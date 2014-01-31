using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Microsoft.Msagl.GraphViewerGdi {
    internal class LayoutSettingsWrapper {

        public EventHandler LayoutTypeHasChanged;

        LayoutMethod layoutMethod;

        [DisplayName("Layout method")]
        [Description("Sets the current layout method")]
        [DefaultValue(LayoutMethod.SugiyamaScheme)]
        public LayoutMethod LayoutMethod {
            get { return layoutMethod; }
            set {
                layoutMethod = value;
                if (LayoutTypeHasChanged != null)
                    LayoutTypeHasChanged(this, null);
            }
        }
        Microsoft.Msagl.LayoutAlgorithmSettings layoutSettings;

        public Microsoft.Msagl.LayoutAlgorithmSettings LayoutSettings {
            get { return layoutSettings; }
            set { layoutSettings = value; }
        }
    }
}
