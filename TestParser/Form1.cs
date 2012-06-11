using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TSQLParser;
using System.IO;

namespace TestParser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void parseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlStatement target = new SqlStatement();
            bool actual;
            StringReader sr = new StringReader(tbInput.Text);
            actual = target.ParseReader(sr);
            if (actual)
            {
                List<string> lstractual;
                lstractual = target.getTableNames();
                tbTarget.Text = "Table Names\r\n";
                foreach (string tableName in lstractual)
                {
                    tbTarget.Text += tableName + "\r\n";
                }
                tbTarget.Text += "\r\nProcedure Names\r\n";
                lstractual = target.getProcedureNames();
                foreach (string tableName in lstractual)
                {
                    tbTarget.Text += tableName + "\r\n";
                }
                tbTarget.Text += "\r\nFunction Names\r\n";
                lstractual = target.getFunctionNames();
                foreach (string tableName in lstractual)
                {
                    tbTarget.Text += tableName + "\r\n";
                }

            }
            else
            {
                tbTarget.Text = "Failed to Parse.\r\n";
                foreach (string error in target.parseErrors)
                {
                    tbTarget.Text += error + "\r\n";
                }
            }
        }
    }
}
