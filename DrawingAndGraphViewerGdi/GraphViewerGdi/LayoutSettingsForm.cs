using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// the form for the layout settings dialog
    /// </summary>
    internal partial class LayoutSettingsForm : Form {
        internal LayoutSettingsWrapper Wrapper {
            get { return this.PropertyGrid.SelectedObject as LayoutSettingsWrapper; }
        }
        /// <summary>
        /// constructor
        /// </summary>
        internal LayoutSettingsForm() {
            InitializeComponent();
            this.PropertyGrid.PropertySort = PropertySort.Alphabetical;
            this.DialogResult = DialogResult.Cancel;
        }

        private void okButton_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        
        
        
    }
}
