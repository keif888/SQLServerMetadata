using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DependencyViewer
{
    public partial class RunSelection : Form
    {
        private Dictionary<int, string> RunIDs = new Dictionary<int, string>();
        private Dictionary<int, string> SelectedRunIDs = new Dictionary<int, string>();

        public RunSelection()
        {
            InitializeComponent();
        }

        public void LoadItems(Dictionary<int, string> runIDs)
        {
            this.RunIDs = runIDs;
            lbRuns.Items.Clear();

            foreach (KeyValuePair<int, string> runText in this.RunIDs)
            {
                lbRuns.Items.Add(runText.Value);
            }
        }

        public Dictionary<int, string> GetItems()
        {
            return SelectedRunIDs;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SelectedRunIDs.Clear();
            foreach (KeyValuePair<int, string> runText in this.RunIDs)
            {
                foreach (string selectedData in lbRuns.SelectedItems)
                {
                    if (selectedData == runText.Value)
                    {
                        SelectedRunIDs.Add(runText.Key, runText.Value);
                        break;
                    }
                }
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
