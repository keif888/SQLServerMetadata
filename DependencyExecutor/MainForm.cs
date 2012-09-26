using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace DependencyExecutor
{
    public partial class MainForm : Form
    {

        private readonly BackgroundWorker invokeAnalyser2008;
        private readonly BackgroundWorker invokeAnalyser2005;
        private readonly BackgroundWorker invokeAnalyser2012;
        private Process analyse;

        public MainForm()
        {
            InitializeComponent();

            tcMain.TabPages.Remove(tbSSISFolders);
            tcMain.TabPages.Remove(tbAnalysisServers);
            tcMain.TabPages.Remove(tbDatabases);
            tcMain.TabPages.Remove(tbNameOverides);
            tcMain.TabPages.Remove(tbReports);

            invokeAnalyser2005 = new BackgroundWorker();
            invokeAnalyser2005.DoWork += invokeAnalyser2005_DoWork;
            invokeAnalyser2008 = new BackgroundWorker();
            invokeAnalyser2008.DoWork += invokeAnalyser2008_DoWork;
            invokeAnalyser2012 = new BackgroundWorker();
            invokeAnalyser2012.DoWork += invokeAnalyser2012_DoWork;
        }

        private void rbSQL_CheckedChanged(object sender, EventArgs e)
        {
            if (rbSQL.Checked)
            {
                tbPassword.Enabled = true;
                tbUser.Enabled = true;
            }
            else
            {
                tbPassword.Enabled = false;
                tbUser.Enabled = false;
            }
        }

        private void btView_Click(object sender, EventArgs e)
        {
            if ((tbDatabase.Text != string.Empty) && (tbSQLServer.Text != string.Empty))
            {
                string executable = System.Windows.Forms.Application.StartupPath + @"\..\Dependency Viewer\DependencyViewer.exe";

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = executable;
                startInfo.Arguments = string.Format("/depDb:\"{0}\"", buildConnectionString());
                this.WindowState = FormWindowState.Minimized;
                tbCommandLine.Text = string.Format("{0} {1}", executable, startInfo.Arguments);
                try
                {
                    Process viewer = Process.Start(startInfo);
                    viewer.WaitForExit();
                }
                catch (Exception err)
                {
                    this.WindowState = FormWindowState.Normal;
                    MessageBox.Show(err.Message, "Failure when launching the Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.WindowState = FormWindowState.Normal;
                }
            }
        }

        private void cbEnableSQL_CheckedChanged(object sender, EventArgs e)
        {
            if (cbEnableSQL.Checked)
            {
                tcMain.TabPages.Add(tbSSISFolders);
            }
            else
            {
                tcMain.TabPages.Remove(tbSSISFolders);
            }
        }

        private void cbEnableSSIS_CheckedChanged(object sender, EventArgs e)
        {
            if (cbEnableSSIS.Checked)
            {
                tcMain.TabPages.Add(tbFileFolders);
                cbEnableSQL.Enabled = true;
            }
            else
            {
                tcMain.TabPages.Remove(tbFileFolders);
                cbEnableSQL.Checked = false;
                cbEnableSQL.Enabled = false;
            }
        }

        private void cbEnableAS_CheckedChanged(object sender, EventArgs e)
        {
            if (cbEnableAS.Checked)
            {
                tcMain.TabPages.Add(tbAnalysisServers);
            }
            else
            {
                tcMain.TabPages.Remove(tbAnalysisServers);
            }
        }

        private void cbEnableDatabase_CheckedChanged(object sender, EventArgs e)
        {
            if (cbEnableDatabase.Checked)
            {
                tcMain.TabPages.Add(tbDatabases);
            }
            else
            {
                tcMain.TabPages.Remove(tbDatabases);
            }
        }

        private void cbMatchOnDB_CheckedChanged(object sender, EventArgs e)
        {
            if (cbMatchOnDB.Checked)
            {
                tcMain.TabPages.Add(tbNameOverides);
            }
            else
            {
                tcMain.TabPages.Remove(tbNameOverides);
            }
        }

        private void btSSISAdd_Click(object sender, EventArgs e)
        {
            if (tbSSISFolder.Text != String.Empty)
            {
                lbSSIS.Items.Add(tbSSISFolder.Text);
            }
        }

        private void btSSISDelete_Click(object sender, EventArgs e)
        {
            if (lbSSIS.SelectedItems.Count > 0)
            {
                lbSSIS.Items.RemoveAt(lbSSIS.SelectedIndex);
            }
        }

        private void btFileAdd_Click(object sender, EventArgs e)
        {
            if (tbFileFolder.Text != string.Empty)
            {
                lbFolders.Items.Add(tbFileFolder.Text);
            }
        }

        private void btFileDelete_Click(object sender, EventArgs e)
        {
            if (lbFolders.SelectedItems.Count > 0)
            {
                lbFolders.Items.RemoveAt(lbFolders.SelectedIndex);
            }
        }

        private void btBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbFileFolder.Text = folderBrowser.SelectedPath;
            }
        }

        private string buildConnectionString()
        {
            string connectionString = string.Empty;
            if (rbWindows.Checked)
            {
                connectionString = string.Format("Server={0};database={1};Integrated Security=SSPI;", tbSQLServer.Text, tbDatabase.Text);
            }
            else
            {
                connectionString = string.Format("Server={0};database={1};User Id={2};password={3};", tbSQLServer.Text, tbDatabase.Text, tbUser.Text, tbPassword.Text);
            }

            return connectionString;
        }

        private string buildArguments()
        {
            string arguments = string.Format("/depDb:\"{0}\"", buildConnectionString());
            if (cbRecurse.Checked)
                arguments += " /r+";
            else
                arguments += " /r-";

            if (cbBatchMode.Checked)
                arguments += " /b+";
            else
                arguments += " /b-";

            if (cbEnableSSIS.Checked)
            {
                arguments += " /skipSSIS-";
                foreach (string fileFolder in lbFolders.Items)
                {
                    arguments += string.Format(" /f:\"{0}\"", fileFolder);
                }

                if (cbEnableSQL.Checked)
                {
                    arguments += " /skipSQL-";
                    foreach (string ssisFolder in lbSSIS.Items)
                    {
                        arguments += string.Format(" /sf:\"{0}\"", ssisFolder);
                    }
                }
                else
                    arguments += " /skipSQL+";
            }
            else
                arguments += " /skipSSIS+";

            if (cbEnableDatabase.Checked)
            {
                foreach (string dbConnectionString in lbConnectionDB.Items)
                {
                    arguments += string.Format(" /d2s:\"{0}\"", dbConnectionString);
                }
            }

            if (cbMatchOnDB.Checked)
            {
                arguments += " /matchDBOnly+";
                foreach (string dbPrefix in lbDBPrefix.Items)
                {
                    arguments += string.Format(" /dp:{0}", dbPrefix);
                }
            }
            else
                arguments += " /matchDBOnly-";

            if (cbEnableAS.Checked)
            {
                arguments += " /skipAS-";
                foreach (string asConnection in lbASServers.Items)
                {
                    arguments += string.Format(" /a:\"{0}\"", asConnection);
                }
            }
            else
                arguments += " /skipAS+";

            if (cbEnableReportingServices.Checked)
            {
                arguments += " /skipRS-";
                foreach (string rsConnection in lbReportServer.Items)
                {
                    arguments += string.Format(" /rpt:\"{0}\"", rsConnection);
                }
            }
            else
                arguments += " /skipRS+";

            return arguments;
        }

        #region Hovers
        private void btView_MouseHover(object sender, EventArgs e)
        {
            string executable = System.Windows.Forms.Application.StartupPath + @"\..\Dependency Viewer\DependencyViewer.exe";

            string arguments = string.Format("/depDb:\"{0}\"", buildConnectionString());
            tbCommandLine.Text = string.Format("\"{0}\" {1}", executable, arguments);
        }

        private void btAnalyze2005_MouseHover(object sender, EventArgs e)
        {
            string executable = System.Windows.Forms.Application.StartupPath + @"\..\Dependency Analyzer 2005\DependencyAnalyzer.exe";

            tbCommandLine.Text = string.Format("\"{0}\" {1}", executable, buildArguments());
        }

        private void btAnalyze2008_MouseHover(object sender, EventArgs e)
        {
            string executable = System.Windows.Forms.Application.StartupPath + @"\..\Dependency Analyzer 2008\DependencyAnalyzer.exe";

            tbCommandLine.Text = string.Format("\"{0}\" {1}", executable, buildArguments());
        }

        private void btAnalyze2012_MouseHover(object sender, EventArgs e)
        {
            string executable = System.Windows.Forms.Application.StartupPath + @"\..\Dependency Analyzer 2012\DependencyAnalyzer.exe";

            tbCommandLine.Text = string.Format("\"{0}\" {1}", executable, buildArguments());
        }
        #endregion

        private void rbSQLDB_CheckedChanged(object sender, EventArgs e)
        {
            if (rbSQLDB.Checked)
            {
                tbPasswordDB.Enabled = true;
                tbUserDB.Enabled = true;
            }
            else
            {
                tbPasswordDB.Enabled = false;
                tbUserDB.Enabled = false;
            }
        }

        private string buildDBConnectionString()
        {
            string connectionString = string.Empty;
            if (rbWindowsDB.Checked)
            {
                connectionString = string.Format("Server={0};Database={1};Integrated Security=SSPI;", tbSQLServerDB.Text, tbDatabaseDB.Text);
            }
            else
            {
                connectionString = string.Format("Server={0};Database={1};User Id={2};password={3};", tbSQLServerDB.Text, tbDatabaseDB.Text, tbUserDB.Text, tbPasswordDB.Text);
            }

            return connectionString;
        }

        private void btAddDB_Click(object sender, EventArgs e)
        {
            if ((tbSQLServerDB.Text != string.Empty) && (tbDatabaseDB.Text != string.Empty))
            {
                lbConnectionDB.Items.Add(buildDBConnectionString());
            }
        }

        private void btDeleteDB_Click(object sender, EventArgs e)
        {
            if (lbConnectionDB.SelectedItems.Count > 0)
            {
                lbConnectionDB.Items.RemoveAt(lbConnectionDB.SelectedIndex);
            }
        }

        private void btAddPrefix_Click(object sender, EventArgs e)
        {
            if (tbDBPrefix.Text != string.Empty)
                lbDBPrefix.Items.Add(tbDBPrefix.Text);
        }

        private void btDeletePrefix_Click(object sender, EventArgs e)
        {
            if (lbDBPrefix.SelectedItems.Count > 0)
                lbDBPrefix.Items.RemoveAt(lbDBPrefix.SelectedIndex);
        }

        private string buildASConnectionString()
        {
            string connectionString = string.Empty;
            if (tbASDatabase.Text == string.Empty)
            {
                connectionString = string.Format("Provider=msolap;Data Source={0};", tbASServer.Text);
            }
            else
            {
                connectionString = string.Format("Provider=msolap;Data Source={0};Initial Catalog={1};", tbASServer.Text, tbASDatabase.Text);
            }

            return connectionString;
        }

        private void btAddAS_Click(object sender, EventArgs e)
        {
            if (tbASServer.Text != String.Empty)
                lbASServers.Items.Add(buildASConnectionString());
        }

        private void btDeleteAS_Click(object sender, EventArgs e)
        {
            if (lbASServers.SelectedItems.Count > 0)
                lbASServers.Items.RemoveAt(lbASServers.SelectedIndex);
        }

        private void btAnalyze2005_Click(object sender, EventArgs e)
        {
            if ((tbDatabase.Text != string.Empty) && (tbSQLServer.Text != string.Empty))
            {
                tbResults.Clear();
                tcMain.SelectedTab = tbOutput;
                string executable = System.Windows.Forms.Application.StartupPath + @"\..\Dependency Analyzer 2005\DependencyAnalyzer.exe";
                tbCommandLine.Text = string.Format("\"{0}\" {1}", executable, buildArguments());

                btAnalyze2005.Enabled = false;
                btAnalyze2008.Enabled = false;
                btAnalyze2012.Enabled = false;
                btView.Enabled = false;
                invokeAnalyser2005.RunWorkerAsync();
            }
        }

        void invokeAnalyser2005_DoWork(object sender, DoWorkEventArgs e)
        {
            string executable = System.Windows.Forms.Application.StartupPath + @"\..\Dependency Analyser 2005\DependencyAnalyzer.exe";
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = executable;
            startInfo.Arguments = buildArguments();

            analyse = new Process();
            analyse.StartInfo.Arguments = buildArguments();
            analyse.StartInfo.FileName = executable;

            analyse.StartInfo.UseShellExecute = false;
            analyse.StartInfo.RedirectStandardOutput = true;
            analyse.StartInfo.RedirectStandardError = true;
            analyse.StartInfo.CreateNoWindow = true;
            analyse.StartInfo.ErrorDialog = true;

            analyse.OutputDataReceived += new DataReceivedEventHandler(analyse_OutputDataReceived);
            analyse.ErrorDataReceived += new DataReceivedEventHandler(analyse_OutputDataReceived);
            analyse.EnableRaisingEvents = true;
            analyse.Exited += new EventHandler(analyse_Exited);

            try
            {
                analyse.Start();
                analyse.BeginOutputReadLine();
                analyse.BeginErrorReadLine();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Failure when launching the Analyser", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btView.Invoke((MethodInvoker)delegate { btView.Enabled = true; });
                btAnalyze2005.Invoke((MethodInvoker)delegate { btAnalyze2005.Enabled = true; });
                btAnalyze2008.Invoke((MethodInvoker)delegate { btAnalyze2008.Enabled = true; });
                btAnalyze2012.Invoke((MethodInvoker)delegate { btAnalyze2012.Enabled = true; });
            }
        }



        private void btAnalyze2008_Click(object sender, EventArgs e)
        {
            if ((tbDatabase.Text != string.Empty) && (tbSQLServer.Text != string.Empty))
            {
                tbResults.Clear();
                tcMain.SelectedTab = tbOutput;
                string executable = System.Windows.Forms.Application.StartupPath + @"\..\Dependency Analyzer 2008\DependencyAnalyzer.exe";
                tbCommandLine.Text = string.Format("\"{0}\" {1}", executable, buildArguments());

                btAnalyze2005.Enabled = false;
                btAnalyze2008.Enabled = false;
                btAnalyze2012.Enabled = false;
                btView.Enabled = false;
                invokeAnalyser2008.RunWorkerAsync();
            }
        }



        void invokeAnalyser2008_DoWork(object sender, DoWorkEventArgs e)
        {
            string executable = System.Windows.Forms.Application.StartupPath + @"\..\Dependency Analyzer 2008\DependencyAnalyzer.exe";
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = executable;
            startInfo.Arguments = buildArguments();
            //this.WindowState = FormWindowState.Minimized;

            analyse = new Process();
            analyse.StartInfo.Arguments = buildArguments();
            analyse.StartInfo.FileName = executable;

            analyse.StartInfo.UseShellExecute = false;
            analyse.StartInfo.RedirectStandardOutput = true;
            analyse.StartInfo.RedirectStandardError = true;
            analyse.StartInfo.CreateNoWindow = true;
            analyse.StartInfo.ErrorDialog = true;

            analyse.OutputDataReceived += new DataReceivedEventHandler(analyse_OutputDataReceived);
            analyse.ErrorDataReceived += new DataReceivedEventHandler(analyse_OutputDataReceived);
            analyse.EnableRaisingEvents = true;
            analyse.Exited += new EventHandler(analyse_Exited);

            try
            {
                analyse.Start();
                analyse.BeginOutputReadLine();
                analyse.BeginErrorReadLine();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Failure when launching the Analyser", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btView.Invoke((MethodInvoker)delegate { btView.Enabled = true; });
                btAnalyze2005.Invoke((MethodInvoker)delegate { btAnalyze2005.Enabled = true; });
                btAnalyze2008.Invoke((MethodInvoker)delegate { btAnalyze2008.Enabled = true; });
                btAnalyze2012.Invoke((MethodInvoker)delegate { btAnalyze2012.Enabled = true; });
            }
        }

        void analyse_Exited(object sender, EventArgs e)
        {
            analyse.CancelErrorRead();
            analyse.CancelOutputRead();
            btView.Invoke((MethodInvoker)delegate { btView.Enabled = true; });
            btAnalyze2005.Invoke((MethodInvoker)delegate { btAnalyze2005.Enabled = true; });
            btAnalyze2008.Invoke((MethodInvoker)delegate { btAnalyze2008.Enabled = true; });
            btAnalyze2012.Invoke((MethodInvoker)delegate { btAnalyze2012.Enabled = true; });
        }

        void analyse_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string textMessage = e.Data;
            if (!string.IsNullOrEmpty(textMessage))
                tbResults.Invoke((MethodInvoker) delegate { tbResults.AppendText(textMessage + "\r\n"); });
        }

        private void btAddReport_Click(object sender, EventArgs e)
        {
            if ((tbReportServer.Text != string.Empty) && (tbReportPath.Text != string.Empty))
            {
                lbReportServer.Items.Add(string.Format("{0}?{1}", tbReportServer.Text, tbReportPath.Text));
            }
        }

        private void btDeleteReport_Click(object sender, EventArgs e)
        {
            if (lbReportServer.SelectedItems.Count > 0)
                lbReportServer.Items.RemoveAt(lbReportServer.SelectedIndex);
        }

        private void cbEnableReportingServices_CheckedChanged(object sender, EventArgs e)
        {
            if (cbEnableReportingServices.Checked)
            {
                tcMain.TabPages.Add(tbReports);
            }
            else
            {
                tcMain.TabPages.Remove(tbReports);
            }
        }

        private void btAnalyze2012_Click(object sender, EventArgs e)
        {
            if ((tbDatabase.Text != string.Empty) && (tbSQLServer.Text != string.Empty))
            {
                tbResults.Clear();
                tcMain.SelectedTab = tbOutput;
                string executable = System.Windows.Forms.Application.StartupPath + @"\..\Dependency Analyzer 2008\DependencyAnalyzer.exe";
                tbCommandLine.Text = string.Format("\"{0}\" {1}", executable, buildArguments());

                btAnalyze2005.Enabled = false;
                btAnalyze2008.Enabled = false;
                btAnalyze2012.Enabled = false;
                btView.Enabled = false;
                invokeAnalyser2012.RunWorkerAsync();
            }
        }

        void invokeAnalyser2012_DoWork(object sender, DoWorkEventArgs e)
        {
            string executable = System.Windows.Forms.Application.StartupPath + @"\..\Dependency Analyzer 2012\DependencyAnalyzer.exe";
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = executable;
            startInfo.Arguments = buildArguments();
            //this.WindowState = FormWindowState.Minimized;

            analyse = new Process();
            analyse.StartInfo.Arguments = buildArguments();
            analyse.StartInfo.FileName = executable;

            analyse.StartInfo.UseShellExecute = false;
            analyse.StartInfo.RedirectStandardOutput = true;
            analyse.StartInfo.RedirectStandardError = true;
            analyse.StartInfo.CreateNoWindow = true;
            analyse.StartInfo.ErrorDialog = true;

            analyse.OutputDataReceived += new DataReceivedEventHandler(analyse_OutputDataReceived);
            analyse.ErrorDataReceived += new DataReceivedEventHandler(analyse_OutputDataReceived);
            analyse.EnableRaisingEvents = true;
            analyse.Exited += new EventHandler(analyse_Exited);

            try
            {
                analyse.Start();
                analyse.BeginOutputReadLine();
                analyse.BeginErrorReadLine();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Failure when launching the Analyser", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btView.Invoke((MethodInvoker)delegate { btView.Enabled = true; });
                btAnalyze2005.Invoke((MethodInvoker)delegate { btAnalyze2005.Enabled = true; });
                btAnalyze2008.Invoke((MethodInvoker)delegate { btAnalyze2008.Enabled = true; });
                btAnalyze2012.Invoke((MethodInvoker)delegate { btAnalyze2012.Enabled = true; });
            }
        }

    }
}
