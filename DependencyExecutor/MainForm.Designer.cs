namespace DependencyExecutor
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tbRepository = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.MaskedTextBox();
            this.tbUser = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbSQL = new System.Windows.Forms.RadioButton();
            this.rbWindows = new System.Windows.Forms.RadioButton();
            this.tbDatabase = new System.Windows.Forms.TextBox();
            this.tbSQLServer = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbScanOptions = new System.Windows.Forms.TabPage();
            this.cbEnableReportingServices = new System.Windows.Forms.CheckBox();
            this.cbMatchOnDB = new System.Windows.Forms.CheckBox();
            this.cbClearDatabase = new System.Windows.Forms.CheckBox();
            this.cbEnableDatabase = new System.Windows.Forms.CheckBox();
            this.cbEnableAS = new System.Windows.Forms.CheckBox();
            this.cbEnableSSIS = new System.Windows.Forms.CheckBox();
            this.cbEnableSQL = new System.Windows.Forms.CheckBox();
            this.cbBatchMode = new System.Windows.Forms.CheckBox();
            this.cbRecurse = new System.Windows.Forms.CheckBox();
            this.tbSSISFolders = new System.Windows.Forms.TabPage();
            this.btSSISDelete = new System.Windows.Forms.Button();
            this.btSSISAdd = new System.Windows.Forms.Button();
            this.lbSSIS = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbSSISFolder = new System.Windows.Forms.TextBox();
            this.tbFileFolders = new System.Windows.Forms.TabPage();
            this.btFileDelete = new System.Windows.Forms.Button();
            this.btFileAdd = new System.Windows.Forms.Button();
            this.lbFolders = new System.Windows.Forms.ListBox();
            this.btBrowse = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.tbFileFolder = new System.Windows.Forms.TextBox();
            this.tbDatabases = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tbPasswordDB = new System.Windows.Forms.MaskedTextBox();
            this.tbUserDB = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbSQLDB = new System.Windows.Forms.RadioButton();
            this.rbWindowsDB = new System.Windows.Forms.RadioButton();
            this.tbDatabaseDB = new System.Windows.Forms.TextBox();
            this.tbSQLServerDB = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.btDeleteDB = new System.Windows.Forms.Button();
            this.btAddDB = new System.Windows.Forms.Button();
            this.lbConnectionDB = new System.Windows.Forms.ListBox();
            this.tbReports = new System.Windows.Forms.TabPage();
            this.tbReportPath = new System.Windows.Forms.TextBox();
            this.tbReportServer = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.btDeleteReport = new System.Windows.Forms.Button();
            this.btAddReport = new System.Windows.Forms.Button();
            this.lbReportServer = new System.Windows.Forms.ListBox();
            this.tbAnalysisServers = new System.Windows.Forms.TabPage();
            this.label13 = new System.Windows.Forms.Label();
            this.tbASDatabase = new System.Windows.Forms.TextBox();
            this.btDeleteAS = new System.Windows.Forms.Button();
            this.btAddAS = new System.Windows.Forms.Button();
            this.lbASServers = new System.Windows.Forms.ListBox();
            this.label12 = new System.Windows.Forms.Label();
            this.tbASServer = new System.Windows.Forms.TextBox();
            this.tbNameOverides = new System.Windows.Forms.TabPage();
            this.btDeletePrefix = new System.Windows.Forms.Button();
            this.btAddPrefix = new System.Windows.Forms.Button();
            this.lbDBPrefix = new System.Windows.Forms.ListBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tbDBPrefix = new System.Windows.Forms.TextBox();
            this.tbOutput = new System.Windows.Forms.TabPage();
            this.tbResults = new System.Windows.Forms.TextBox();
            this.btView = new System.Windows.Forms.Button();
            this.btAnalyze2005 = new System.Windows.Forms.Button();
            this.btAnalyze2008 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbCommandLine = new System.Windows.Forms.TextBox();
            this.toolTipControl = new System.Windows.Forms.ToolTip(this.components);
            this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.btAnalyze2012 = new System.Windows.Forms.Button();
            this.tcMain.SuspendLayout();
            this.tbRepository.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tbScanOptions.SuspendLayout();
            this.tbSSISFolders.SuspendLayout();
            this.tbFileFolders.SuspendLayout();
            this.tbDatabases.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tbReports.SuspendLayout();
            this.tbAnalysisServers.SuspendLayout();
            this.tbNameOverides.SuspendLayout();
            this.tbOutput.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tbRepository);
            this.tcMain.Controls.Add(this.tbScanOptions);
            this.tcMain.Controls.Add(this.tbSSISFolders);
            this.tcMain.Controls.Add(this.tbFileFolders);
            this.tcMain.Controls.Add(this.tbDatabases);
            this.tcMain.Controls.Add(this.tbReports);
            this.tcMain.Controls.Add(this.tbAnalysisServers);
            this.tcMain.Controls.Add(this.tbNameOverides);
            this.tcMain.Controls.Add(this.tbOutput);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcMain.Location = new System.Drawing.Point(0, 0);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(719, 390);
            this.tcMain.TabIndex = 0;
            // 
            // tbRepository
            // 
            this.tbRepository.Controls.Add(this.label4);
            this.tbRepository.Controls.Add(this.label3);
            this.tbRepository.Controls.Add(this.tbPassword);
            this.tbRepository.Controls.Add(this.tbUser);
            this.tbRepository.Controls.Add(this.groupBox1);
            this.tbRepository.Controls.Add(this.tbDatabase);
            this.tbRepository.Controls.Add(this.tbSQLServer);
            this.tbRepository.Controls.Add(this.label2);
            this.tbRepository.Controls.Add(this.label1);
            this.tbRepository.Location = new System.Drawing.Point(4, 22);
            this.tbRepository.Name = "tbRepository";
            this.tbRepository.Padding = new System.Windows.Forms.Padding(3);
            this.tbRepository.Size = new System.Drawing.Size(711, 364);
            this.tbRepository.TabIndex = 0;
            this.tbRepository.Text = "Repository";
            this.tbRepository.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 165);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 138);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "UserID";
            // 
            // tbPassword
            // 
            this.tbPassword.Enabled = false;
            this.tbPassword.Location = new System.Drawing.Point(99, 162);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(171, 20);
            this.tbPassword.TabIndex = 6;
            this.toolTipControl.SetToolTip(this.tbPassword, "The password for the user above.");
            this.tbPassword.UseSystemPasswordChar = true;
            // 
            // tbUser
            // 
            this.tbUser.Enabled = false;
            this.tbUser.Location = new System.Drawing.Point(99, 135);
            this.tbUser.Name = "tbUser";
            this.tbUser.Size = new System.Drawing.Size(171, 20);
            this.tbUser.TabIndex = 5;
            this.toolTipControl.SetToolTip(this.tbUser, "The SQL Login to use to connect to the SQL Server.");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbSQL);
            this.groupBox1.Controls.Add(this.rbWindows);
            this.groupBox1.Location = new System.Drawing.Point(3, 59);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(267, 69);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Authentication";
            // 
            // rbSQL
            // 
            this.rbSQL.AutoSize = true;
            this.rbSQL.Location = new System.Drawing.Point(96, 44);
            this.rbSQL.Name = "rbSQL";
            this.rbSQL.Size = new System.Drawing.Size(117, 17);
            this.rbSQL.TabIndex = 1;
            this.rbSQL.Text = "SQL Authentication";
            this.toolTipControl.SetToolTip(this.rbSQL, "Checked for SQL Authentication");
            this.rbSQL.UseVisualStyleBackColor = true;
            this.rbSQL.CheckedChanged += new System.EventHandler(this.rbSQL_CheckedChanged);
            // 
            // rbWindows
            // 
            this.rbWindows.AutoSize = true;
            this.rbWindows.Checked = true;
            this.rbWindows.Location = new System.Drawing.Point(96, 20);
            this.rbWindows.Name = "rbWindows";
            this.rbWindows.Size = new System.Drawing.Size(140, 17);
            this.rbWindows.TabIndex = 0;
            this.rbWindows.TabStop = true;
            this.rbWindows.Text = "Windows Authentication";
            this.toolTipControl.SetToolTip(this.rbWindows, "Checked for Windows Authentication");
            this.rbWindows.UseVisualStyleBackColor = true;
            // 
            // tbDatabase
            // 
            this.tbDatabase.Location = new System.Drawing.Point(99, 33);
            this.tbDatabase.Name = "tbDatabase";
            this.tbDatabase.Size = new System.Drawing.Size(171, 20);
            this.tbDatabase.TabIndex = 3;
            this.toolTipControl.SetToolTip(this.tbDatabase, "Enter the database name which holds the repository.  If this is the first executi" +
        "on, then an empty database is required, all tables will be created for you.");
            // 
            // tbSQLServer
            // 
            this.tbSQLServer.Location = new System.Drawing.Point(99, 6);
            this.tbSQLServer.Name = "tbSQLServer";
            this.tbSQLServer.Size = new System.Drawing.Size(171, 20);
            this.tbSQLServer.TabIndex = 2;
            this.toolTipControl.SetToolTip(this.tbSQLServer, "Enter the SQL Server where the Repository exists.");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Database";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "SQL Server";
            // 
            // tbScanOptions
            // 
            this.tbScanOptions.Controls.Add(this.cbEnableReportingServices);
            this.tbScanOptions.Controls.Add(this.cbMatchOnDB);
            this.tbScanOptions.Controls.Add(this.cbClearDatabase);
            this.tbScanOptions.Controls.Add(this.cbEnableDatabase);
            this.tbScanOptions.Controls.Add(this.cbEnableAS);
            this.tbScanOptions.Controls.Add(this.cbEnableSSIS);
            this.tbScanOptions.Controls.Add(this.cbEnableSQL);
            this.tbScanOptions.Controls.Add(this.cbBatchMode);
            this.tbScanOptions.Controls.Add(this.cbRecurse);
            this.tbScanOptions.Location = new System.Drawing.Point(4, 22);
            this.tbScanOptions.Name = "tbScanOptions";
            this.tbScanOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tbScanOptions.Size = new System.Drawing.Size(611, 364);
            this.tbScanOptions.TabIndex = 1;
            this.tbScanOptions.Text = "Scan Options";
            this.tbScanOptions.UseVisualStyleBackColor = true;
            // 
            // cbEnableReportingServices
            // 
            this.cbEnableReportingServices.AutoSize = true;
            this.cbEnableReportingServices.Location = new System.Drawing.Point(9, 171);
            this.cbEnableReportingServices.Name = "cbEnableReportingServices";
            this.cbEnableReportingServices.Size = new System.Drawing.Size(123, 17);
            this.cbEnableReportingServices.TabIndex = 8;
            this.cbEnableReportingServices.Text = "Enumerate Reports?";
            this.cbEnableReportingServices.UseVisualStyleBackColor = true;
            this.cbEnableReportingServices.CheckedChanged += new System.EventHandler(this.cbEnableReportingServices_CheckedChanged);
            // 
            // cbMatchOnDB
            // 
            this.cbMatchOnDB.AutoSize = true;
            this.cbMatchOnDB.Location = new System.Drawing.Point(9, 125);
            this.cbMatchOnDB.Name = "cbMatchOnDB";
            this.cbMatchOnDB.Size = new System.Drawing.Size(181, 17);
            this.cbMatchOnDB.TabIndex = 7;
            this.cbMatchOnDB.Text = "Match on Database Name Only?";
            this.toolTipControl.SetToolTip(this.cbMatchOnDB, "Enable to change the matching for connections to be purely database name.");
            this.cbMatchOnDB.UseVisualStyleBackColor = true;
            this.cbMatchOnDB.CheckedChanged += new System.EventHandler(this.cbMatchOnDB_CheckedChanged);
            // 
            // cbClearDatabase
            // 
            this.cbClearDatabase.AutoSize = true;
            this.cbClearDatabase.Location = new System.Drawing.Point(9, 371);
            this.cbClearDatabase.Name = "cbClearDatabase";
            this.cbClearDatabase.Size = new System.Drawing.Size(105, 17);
            this.cbClearDatabase.TabIndex = 6;
            this.cbClearDatabase.Text = "Clear Database?";
            this.toolTipControl.SetToolTip(this.cbClearDatabase, "If you are really sure, this will remove all the data from the database tables.");
            this.cbClearDatabase.UseVisualStyleBackColor = true;
            // 
            // cbEnableDatabase
            // 
            this.cbEnableDatabase.AutoSize = true;
            this.cbEnableDatabase.Location = new System.Drawing.Point(9, 148);
            this.cbEnableDatabase.Name = "cbEnableDatabase";
            this.cbEnableDatabase.Size = new System.Drawing.Size(137, 17);
            this.cbEnableDatabase.TabIndex = 5;
            this.cbEnableDatabase.Text = "Enumerate Databases?";
            this.toolTipControl.SetToolTip(this.cbEnableDatabase, "Do you want to analyse databases directly?");
            this.cbEnableDatabase.UseVisualStyleBackColor = true;
            this.cbEnableDatabase.CheckedChanged += new System.EventHandler(this.cbEnableDatabase_CheckedChanged);
            // 
            // cbEnableAS
            // 
            this.cbEnableAS.AutoSize = true;
            this.cbEnableAS.Location = new System.Drawing.Point(9, 102);
            this.cbEnableAS.Name = "cbEnableAS";
            this.cbEnableAS.Size = new System.Drawing.Size(168, 17);
            this.cbEnableAS.TabIndex = 4;
            this.cbEnableAS.Text = "Enumerate Analysis Services?";
            this.toolTipControl.SetToolTip(this.cbEnableAS, "Do you want to scan Analysis Services servers?");
            this.cbEnableAS.UseVisualStyleBackColor = true;
            this.cbEnableAS.CheckedChanged += new System.EventHandler(this.cbEnableAS_CheckedChanged);
            // 
            // cbEnableSSIS
            // 
            this.cbEnableSSIS.AutoSize = true;
            this.cbEnableSSIS.Checked = true;
            this.cbEnableSSIS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEnableSSIS.Location = new System.Drawing.Point(9, 53);
            this.cbEnableSSIS.Name = "cbEnableSSIS";
            this.cbEnableSSIS.Size = new System.Drawing.Size(161, 17);
            this.cbEnableSSIS.TabIndex = 3;
            this.cbEnableSSIS.Text = "Enumerate SSIS Packages?";
            this.toolTipControl.SetToolTip(this.cbEnableSSIS, resources.GetString("cbEnableSSIS.ToolTip"));
            this.cbEnableSSIS.UseVisualStyleBackColor = true;
            this.cbEnableSSIS.CheckedChanged += new System.EventHandler(this.cbEnableSSIS_CheckedChanged);
            // 
            // cbEnableSQL
            // 
            this.cbEnableSQL.AutoSize = true;
            this.cbEnableSQL.Location = new System.Drawing.Point(9, 76);
            this.cbEnableSQL.Name = "cbEnableSQL";
            this.cbEnableSQL.Size = new System.Drawing.Size(191, 17);
            this.cbEnableSQL.TabIndex = 2;
            this.cbEnableSQL.Text = "Enumerate Packages within SSIS?";
            this.toolTipControl.SetToolTip(this.cbEnableSQL, "When checked, the Integration Services on the SQL Server will be checked for Pack" +
        "ages.\r\nCombine this with SSIS Folders to control what is collected.  \r\nRecursion" +
        " is automatic on these.");
            this.cbEnableSQL.UseVisualStyleBackColor = true;
            this.cbEnableSQL.CheckedChanged += new System.EventHandler(this.cbEnableSQL_CheckedChanged);
            // 
            // cbBatchMode
            // 
            this.cbBatchMode.AutoSize = true;
            this.cbBatchMode.Checked = true;
            this.cbBatchMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbBatchMode.Location = new System.Drawing.Point(9, 30);
            this.cbBatchMode.Name = "cbBatchMode";
            this.cbBatchMode.Size = new System.Drawing.Size(90, 17);
            this.cbBatchMode.TabIndex = 1;
            this.cbBatchMode.Text = "Batch Mode?";
            this.toolTipControl.SetToolTip(this.cbBatchMode, "Do you want the command window to show, and make you hit the Space Bar?");
            this.cbBatchMode.UseVisualStyleBackColor = true;
            // 
            // cbRecurse
            // 
            this.cbRecurse.AutoSize = true;
            this.cbRecurse.Checked = true;
            this.cbRecurse.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRecurse.Location = new System.Drawing.Point(9, 7);
            this.cbRecurse.Name = "cbRecurse";
            this.cbRecurse.Size = new System.Drawing.Size(109, 17);
            this.cbRecurse.TabIndex = 0;
            this.cbRecurse.Text = "Recurse Folders?";
            this.toolTipControl.SetToolTip(this.cbRecurse, "When checked the Analysis will recurse to all sub folders on the file system for " +
        "SSIS analysis.");
            this.cbRecurse.UseVisualStyleBackColor = true;
            // 
            // tbSSISFolders
            // 
            this.tbSSISFolders.Controls.Add(this.btSSISDelete);
            this.tbSSISFolders.Controls.Add(this.btSSISAdd);
            this.tbSSISFolders.Controls.Add(this.lbSSIS);
            this.tbSSISFolders.Controls.Add(this.label5);
            this.tbSSISFolders.Controls.Add(this.tbSSISFolder);
            this.tbSSISFolders.Location = new System.Drawing.Point(4, 22);
            this.tbSSISFolders.Name = "tbSSISFolders";
            this.tbSSISFolders.Padding = new System.Windows.Forms.Padding(3);
            this.tbSSISFolders.Size = new System.Drawing.Size(611, 364);
            this.tbSSISFolders.TabIndex = 2;
            this.tbSSISFolders.Text = "SSIS Folders";
            this.tbSSISFolders.UseVisualStyleBackColor = true;
            // 
            // btSSISDelete
            // 
            this.btSSISDelete.Location = new System.Drawing.Point(495, 330);
            this.btSSISDelete.Name = "btSSISDelete";
            this.btSSISDelete.Size = new System.Drawing.Size(75, 23);
            this.btSSISDelete.TabIndex = 5;
            this.btSSISDelete.Text = "Delete";
            this.btSSISDelete.UseVisualStyleBackColor = true;
            this.btSSISDelete.Click += new System.EventHandler(this.btSSISDelete_Click);
            // 
            // btSSISAdd
            // 
            this.btSSISAdd.Location = new System.Drawing.Point(50, 331);
            this.btSSISAdd.Name = "btSSISAdd";
            this.btSSISAdd.Size = new System.Drawing.Size(75, 23);
            this.btSSISAdd.TabIndex = 4;
            this.btSSISAdd.Text = "Add";
            this.btSSISAdd.UseVisualStyleBackColor = true;
            this.btSSISAdd.Click += new System.EventHandler(this.btSSISAdd_Click);
            // 
            // lbSSIS
            // 
            this.lbSSIS.FormattingEnabled = true;
            this.lbSSIS.Location = new System.Drawing.Point(50, 34);
            this.lbSSIS.Name = "lbSSIS";
            this.lbSSIS.Size = new System.Drawing.Size(521, 290);
            this.lbSSIS.Sorted = true;
            this.lbSSIS.TabIndex = 3;
            this.toolTipControl.SetToolTip(this.lbSSIS, "The list of folders within Integration Services to recursively look for Packages " +
        "in.");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(36, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Folder";
            // 
            // tbSSISFolder
            // 
            this.tbSSISFolder.Location = new System.Drawing.Point(50, 7);
            this.tbSSISFolder.Name = "tbSSISFolder";
            this.tbSSISFolder.Size = new System.Drawing.Size(521, 20);
            this.tbSSISFolder.TabIndex = 0;
            // 
            // tbFileFolders
            // 
            this.tbFileFolders.Controls.Add(this.btFileDelete);
            this.tbFileFolders.Controls.Add(this.btFileAdd);
            this.tbFileFolders.Controls.Add(this.lbFolders);
            this.tbFileFolders.Controls.Add(this.btBrowse);
            this.tbFileFolders.Controls.Add(this.label6);
            this.tbFileFolders.Controls.Add(this.tbFileFolder);
            this.tbFileFolders.Location = new System.Drawing.Point(4, 22);
            this.tbFileFolders.Name = "tbFileFolders";
            this.tbFileFolders.Padding = new System.Windows.Forms.Padding(3);
            this.tbFileFolders.Size = new System.Drawing.Size(611, 364);
            this.tbFileFolders.TabIndex = 3;
            this.tbFileFolders.Text = "File Folders";
            this.tbFileFolders.UseVisualStyleBackColor = true;
            // 
            // btFileDelete
            // 
            this.btFileDelete.Location = new System.Drawing.Point(495, 330);
            this.btFileDelete.Name = "btFileDelete";
            this.btFileDelete.Size = new System.Drawing.Size(75, 23);
            this.btFileDelete.TabIndex = 11;
            this.btFileDelete.Text = "Delete";
            this.btFileDelete.UseVisualStyleBackColor = true;
            this.btFileDelete.Click += new System.EventHandler(this.btFileDelete_Click);
            // 
            // btFileAdd
            // 
            this.btFileAdd.Location = new System.Drawing.Point(50, 331);
            this.btFileAdd.Name = "btFileAdd";
            this.btFileAdd.Size = new System.Drawing.Size(75, 23);
            this.btFileAdd.TabIndex = 10;
            this.btFileAdd.Text = "Add";
            this.btFileAdd.UseVisualStyleBackColor = true;
            this.btFileAdd.Click += new System.EventHandler(this.btFileAdd_Click);
            // 
            // lbFolders
            // 
            this.lbFolders.FormattingEnabled = true;
            this.lbFolders.Location = new System.Drawing.Point(50, 34);
            this.lbFolders.Name = "lbFolders";
            this.lbFolders.Size = new System.Drawing.Size(521, 290);
            this.lbFolders.TabIndex = 9;
            // 
            // btBrowse
            // 
            this.btBrowse.Location = new System.Drawing.Point(580, 7);
            this.btBrowse.Name = "btBrowse";
            this.btBrowse.Size = new System.Drawing.Size(25, 23);
            this.btBrowse.TabIndex = 8;
            this.btBrowse.Text = "...";
            this.btBrowse.UseVisualStyleBackColor = true;
            this.btBrowse.Click += new System.EventHandler(this.btBrowse_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Folder";
            // 
            // tbFileFolder
            // 
            this.tbFileFolder.Location = new System.Drawing.Point(50, 7);
            this.tbFileFolder.Name = "tbFileFolder";
            this.tbFileFolder.Size = new System.Drawing.Size(521, 20);
            this.tbFileFolder.TabIndex = 6;
            // 
            // tbDatabases
            // 
            this.tbDatabases.Controls.Add(this.label7);
            this.tbDatabases.Controls.Add(this.label8);
            this.tbDatabases.Controls.Add(this.tbPasswordDB);
            this.tbDatabases.Controls.Add(this.tbUserDB);
            this.tbDatabases.Controls.Add(this.groupBox2);
            this.tbDatabases.Controls.Add(this.tbDatabaseDB);
            this.tbDatabases.Controls.Add(this.tbSQLServerDB);
            this.tbDatabases.Controls.Add(this.label9);
            this.tbDatabases.Controls.Add(this.label10);
            this.tbDatabases.Controls.Add(this.btDeleteDB);
            this.tbDatabases.Controls.Add(this.btAddDB);
            this.tbDatabases.Controls.Add(this.lbConnectionDB);
            this.tbDatabases.Location = new System.Drawing.Point(4, 22);
            this.tbDatabases.Name = "tbDatabases";
            this.tbDatabases.Padding = new System.Windows.Forms.Padding(3);
            this.tbDatabases.Size = new System.Drawing.Size(611, 364);
            this.tbDatabases.TabIndex = 4;
            this.tbDatabases.Text = "Databases";
            this.tbDatabases.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 168);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "Password";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 141);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(40, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "UserID";
            // 
            // tbPasswordDB
            // 
            this.tbPasswordDB.Enabled = false;
            this.tbPasswordDB.Location = new System.Drawing.Point(99, 165);
            this.tbPasswordDB.Name = "tbPasswordDB";
            this.tbPasswordDB.Size = new System.Drawing.Size(171, 20);
            this.tbPasswordDB.TabIndex = 17;
            this.toolTipControl.SetToolTip(this.tbPasswordDB, "The password for the user above.");
            this.tbPasswordDB.UseSystemPasswordChar = true;
            // 
            // tbUserDB
            // 
            this.tbUserDB.Enabled = false;
            this.tbUserDB.Location = new System.Drawing.Point(99, 138);
            this.tbUserDB.Name = "tbUserDB";
            this.tbUserDB.Size = new System.Drawing.Size(171, 20);
            this.tbUserDB.TabIndex = 16;
            this.toolTipControl.SetToolTip(this.tbUserDB, "The SQL Login to use to connect to the SQL Server.");
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rbSQLDB);
            this.groupBox2.Controls.Add(this.rbWindowsDB);
            this.groupBox2.Location = new System.Drawing.Point(3, 62);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(267, 69);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Authentication";
            // 
            // rbSQLDB
            // 
            this.rbSQLDB.AutoSize = true;
            this.rbSQLDB.Location = new System.Drawing.Point(96, 44);
            this.rbSQLDB.Name = "rbSQLDB";
            this.rbSQLDB.Size = new System.Drawing.Size(117, 17);
            this.rbSQLDB.TabIndex = 1;
            this.rbSQLDB.Text = "SQL Authentication";
            this.toolTipControl.SetToolTip(this.rbSQLDB, "Checked for SQL Authentication");
            this.rbSQLDB.UseVisualStyleBackColor = true;
            this.rbSQLDB.CheckedChanged += new System.EventHandler(this.rbSQLDB_CheckedChanged);
            // 
            // rbWindowsDB
            // 
            this.rbWindowsDB.AutoSize = true;
            this.rbWindowsDB.Checked = true;
            this.rbWindowsDB.Location = new System.Drawing.Point(96, 20);
            this.rbWindowsDB.Name = "rbWindowsDB";
            this.rbWindowsDB.Size = new System.Drawing.Size(140, 17);
            this.rbWindowsDB.TabIndex = 0;
            this.rbWindowsDB.TabStop = true;
            this.rbWindowsDB.Text = "Windows Authentication";
            this.toolTipControl.SetToolTip(this.rbWindowsDB, "Checked for Windows Authentication");
            this.rbWindowsDB.UseVisualStyleBackColor = true;
            // 
            // tbDatabaseDB
            // 
            this.tbDatabaseDB.Location = new System.Drawing.Point(99, 36);
            this.tbDatabaseDB.Name = "tbDatabaseDB";
            this.tbDatabaseDB.Size = new System.Drawing.Size(171, 20);
            this.tbDatabaseDB.TabIndex = 14;
            this.toolTipControl.SetToolTip(this.tbDatabaseDB, "Enter the database name which holds the repository.  If this is the first executi" +
        "on, then an empty database is required, all tables will be created for you.");
            // 
            // tbSQLServerDB
            // 
            this.tbSQLServerDB.Location = new System.Drawing.Point(99, 9);
            this.tbSQLServerDB.Name = "tbSQLServerDB";
            this.tbSQLServerDB.Size = new System.Drawing.Size(171, 20);
            this.tbSQLServerDB.TabIndex = 13;
            this.toolTipControl.SetToolTip(this.tbSQLServerDB, "Enter the SQL Server where the Repository exists.");
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 39);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 13);
            this.label9.TabIndex = 12;
            this.label9.Text = "Database";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(8, 12);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(62, 13);
            this.label10.TabIndex = 11;
            this.label10.Text = "SQL Server";
            // 
            // btDeleteDB
            // 
            this.btDeleteDB.Location = new System.Drawing.Point(495, 330);
            this.btDeleteDB.Name = "btDeleteDB";
            this.btDeleteDB.Size = new System.Drawing.Size(75, 23);
            this.btDeleteDB.TabIndex = 10;
            this.btDeleteDB.Text = "Delete";
            this.btDeleteDB.UseVisualStyleBackColor = true;
            this.btDeleteDB.Click += new System.EventHandler(this.btDeleteDB_Click);
            // 
            // btAddDB
            // 
            this.btAddDB.Location = new System.Drawing.Point(50, 331);
            this.btAddDB.Name = "btAddDB";
            this.btAddDB.Size = new System.Drawing.Size(75, 23);
            this.btAddDB.TabIndex = 9;
            this.btAddDB.Text = "Add";
            this.btAddDB.UseVisualStyleBackColor = true;
            this.btAddDB.Click += new System.EventHandler(this.btAddDB_Click);
            // 
            // lbConnectionDB
            // 
            this.lbConnectionDB.FormattingEnabled = true;
            this.lbConnectionDB.Location = new System.Drawing.Point(50, 190);
            this.lbConnectionDB.Name = "lbConnectionDB";
            this.lbConnectionDB.Size = new System.Drawing.Size(521, 134);
            this.lbConnectionDB.Sorted = true;
            this.lbConnectionDB.TabIndex = 8;
            this.toolTipControl.SetToolTip(this.lbConnectionDB, "The list of folders within Integration Services to recursively look for Packages " +
        "in.");
            // 
            // tbReports
            // 
            this.tbReports.Controls.Add(this.tbReportPath);
            this.tbReports.Controls.Add(this.tbReportServer);
            this.tbReports.Controls.Add(this.label14);
            this.tbReports.Controls.Add(this.label15);
            this.tbReports.Controls.Add(this.btDeleteReport);
            this.tbReports.Controls.Add(this.btAddReport);
            this.tbReports.Controls.Add(this.lbReportServer);
            this.tbReports.Location = new System.Drawing.Point(4, 22);
            this.tbReports.Name = "tbReports";
            this.tbReports.Padding = new System.Windows.Forms.Padding(3);
            this.tbReports.Size = new System.Drawing.Size(611, 364);
            this.tbReports.TabIndex = 8;
            this.tbReports.Text = "Reports";
            this.tbReports.UseVisualStyleBackColor = true;
            // 
            // tbReportPath
            // 
            this.tbReportPath.Location = new System.Drawing.Point(99, 36);
            this.tbReportPath.Name = "tbReportPath";
            this.tbReportPath.Size = new System.Drawing.Size(471, 20);
            this.tbReportPath.TabIndex = 16;
            this.tbReportPath.Text = "/";
            this.toolTipControl.SetToolTip(this.tbReportPath, "Enter the path on the Reporting Server that you wish to enumerate.\r\nFor Example:\r" +
        "\n/ = root of the reporting server");
            // 
            // tbReportServer
            // 
            this.tbReportServer.Location = new System.Drawing.Point(99, 9);
            this.tbReportServer.Name = "tbReportServer";
            this.tbReportServer.Size = new System.Drawing.Size(471, 20);
            this.tbReportServer.TabIndex = 15;
            this.tbReportServer.Text = "http://localhost/reportserver";
            this.toolTipControl.SetToolTip(this.tbReportServer, "Enter the reporting services URL here.\r\nIt should be like:\r\nhttp://localhost/Repo" +
        "rtServer\r\nSharepoint Integration capabilities are unknown!");
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(8, 39);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(29, 13);
            this.label14.TabIndex = 19;
            this.label14.Text = "Path";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(8, 12);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(73, 13);
            this.label15.TabIndex = 18;
            this.label15.Text = "Report Server";
            // 
            // btDeleteReport
            // 
            this.btDeleteReport.Location = new System.Drawing.Point(495, 330);
            this.btDeleteReport.Name = "btDeleteReport";
            this.btDeleteReport.Size = new System.Drawing.Size(75, 23);
            this.btDeleteReport.TabIndex = 18;
            this.btDeleteReport.Text = "Delete";
            this.btDeleteReport.UseVisualStyleBackColor = true;
            this.btDeleteReport.Click += new System.EventHandler(this.btDeleteReport_Click);
            // 
            // btAddReport
            // 
            this.btAddReport.Location = new System.Drawing.Point(50, 331);
            this.btAddReport.Name = "btAddReport";
            this.btAddReport.Size = new System.Drawing.Size(75, 23);
            this.btAddReport.TabIndex = 17;
            this.btAddReport.Text = "Add";
            this.btAddReport.UseVisualStyleBackColor = true;
            this.btAddReport.Click += new System.EventHandler(this.btAddReport_Click);
            // 
            // lbReportServer
            // 
            this.lbReportServer.FormattingEnabled = true;
            this.lbReportServer.Location = new System.Drawing.Point(50, 73);
            this.lbReportServer.Name = "lbReportServer";
            this.lbReportServer.Size = new System.Drawing.Size(521, 251);
            this.lbReportServer.Sorted = true;
            this.lbReportServer.TabIndex = 19;
            this.toolTipControl.SetToolTip(this.lbReportServer, "The list of folders within Integration Services to recursively look for Packages " +
        "in.");
            // 
            // tbAnalysisServers
            // 
            this.tbAnalysisServers.Controls.Add(this.label13);
            this.tbAnalysisServers.Controls.Add(this.tbASDatabase);
            this.tbAnalysisServers.Controls.Add(this.btDeleteAS);
            this.tbAnalysisServers.Controls.Add(this.btAddAS);
            this.tbAnalysisServers.Controls.Add(this.lbASServers);
            this.tbAnalysisServers.Controls.Add(this.label12);
            this.tbAnalysisServers.Controls.Add(this.tbASServer);
            this.tbAnalysisServers.Location = new System.Drawing.Point(4, 22);
            this.tbAnalysisServers.Name = "tbAnalysisServers";
            this.tbAnalysisServers.Padding = new System.Windows.Forms.Padding(3);
            this.tbAnalysisServers.Size = new System.Drawing.Size(611, 364);
            this.tbAnalysisServers.TabIndex = 5;
            this.tbAnalysisServers.Text = "Analysis Servers";
            this.tbAnalysisServers.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(288, 10);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(53, 13);
            this.label13.TabIndex = 12;
            this.label13.Text = "Database";
            // 
            // tbASDatabase
            // 
            this.tbASDatabase.Location = new System.Drawing.Point(347, 7);
            this.tbASDatabase.Name = "tbASDatabase";
            this.tbASDatabase.Size = new System.Drawing.Size(224, 20);
            this.tbASDatabase.TabIndex = 11;
            this.toolTipControl.SetToolTip(this.tbASDatabase, "An optional database on the Analysis Server.");
            // 
            // btDeleteAS
            // 
            this.btDeleteAS.Location = new System.Drawing.Point(495, 330);
            this.btDeleteAS.Name = "btDeleteAS";
            this.btDeleteAS.Size = new System.Drawing.Size(75, 23);
            this.btDeleteAS.TabIndex = 10;
            this.btDeleteAS.Text = "Delete";
            this.btDeleteAS.UseVisualStyleBackColor = true;
            this.btDeleteAS.Click += new System.EventHandler(this.btDeleteAS_Click);
            // 
            // btAddAS
            // 
            this.btAddAS.Location = new System.Drawing.Point(50, 331);
            this.btAddAS.Name = "btAddAS";
            this.btAddAS.Size = new System.Drawing.Size(75, 23);
            this.btAddAS.TabIndex = 9;
            this.btAddAS.Text = "Add";
            this.btAddAS.UseVisualStyleBackColor = true;
            this.btAddAS.Click += new System.EventHandler(this.btAddAS_Click);
            // 
            // lbASServers
            // 
            this.lbASServers.FormattingEnabled = true;
            this.lbASServers.Location = new System.Drawing.Point(50, 34);
            this.lbASServers.Name = "lbASServers";
            this.lbASServers.Size = new System.Drawing.Size(521, 290);
            this.lbASServers.Sorted = true;
            this.lbASServers.TabIndex = 8;
            this.toolTipControl.SetToolTip(this.lbASServers, "The list of folders within Integration Services to recursively look for Packages " +
        "in.");
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(8, 10);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(38, 13);
            this.label12.TabIndex = 7;
            this.label12.Text = "Server";
            // 
            // tbASServer
            // 
            this.tbASServer.Location = new System.Drawing.Point(50, 7);
            this.tbASServer.Name = "tbASServer";
            this.tbASServer.Size = new System.Drawing.Size(224, 20);
            this.tbASServer.TabIndex = 6;
            this.toolTipControl.SetToolTip(this.tbASServer, "The Analysis Server");
            // 
            // tbNameOverides
            // 
            this.tbNameOverides.Controls.Add(this.btDeletePrefix);
            this.tbNameOverides.Controls.Add(this.btAddPrefix);
            this.tbNameOverides.Controls.Add(this.lbDBPrefix);
            this.tbNameOverides.Controls.Add(this.label11);
            this.tbNameOverides.Controls.Add(this.tbDBPrefix);
            this.tbNameOverides.Location = new System.Drawing.Point(4, 22);
            this.tbNameOverides.Name = "tbNameOverides";
            this.tbNameOverides.Padding = new System.Windows.Forms.Padding(3);
            this.tbNameOverides.Size = new System.Drawing.Size(611, 364);
            this.tbNameOverides.TabIndex = 6;
            this.tbNameOverides.Text = "Name Overrides";
            this.tbNameOverides.UseVisualStyleBackColor = true;
            // 
            // btDeletePrefix
            // 
            this.btDeletePrefix.Location = new System.Drawing.Point(495, 330);
            this.btDeletePrefix.Name = "btDeletePrefix";
            this.btDeletePrefix.Size = new System.Drawing.Size(75, 23);
            this.btDeletePrefix.TabIndex = 10;
            this.btDeletePrefix.Text = "Delete";
            this.btDeletePrefix.UseVisualStyleBackColor = true;
            this.btDeletePrefix.Click += new System.EventHandler(this.btDeletePrefix_Click);
            // 
            // btAddPrefix
            // 
            this.btAddPrefix.Location = new System.Drawing.Point(50, 331);
            this.btAddPrefix.Name = "btAddPrefix";
            this.btAddPrefix.Size = new System.Drawing.Size(75, 23);
            this.btAddPrefix.TabIndex = 9;
            this.btAddPrefix.Text = "Add";
            this.btAddPrefix.UseVisualStyleBackColor = true;
            this.btAddPrefix.Click += new System.EventHandler(this.btAddPrefix_Click);
            // 
            // lbDBPrefix
            // 
            this.lbDBPrefix.FormattingEnabled = true;
            this.lbDBPrefix.Location = new System.Drawing.Point(50, 34);
            this.lbDBPrefix.Name = "lbDBPrefix";
            this.lbDBPrefix.Size = new System.Drawing.Size(521, 290);
            this.lbDBPrefix.Sorted = true;
            this.lbDBPrefix.TabIndex = 8;
            this.toolTipControl.SetToolTip(this.lbDBPrefix, "The list of folders within Integration Services to recursively look for Packages " +
        "in.");
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 10);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(33, 13);
            this.label11.TabIndex = 7;
            this.label11.Text = "Prefix";
            // 
            // tbDBPrefix
            // 
            this.tbDBPrefix.Location = new System.Drawing.Point(50, 7);
            this.tbDBPrefix.Name = "tbDBPrefix";
            this.tbDBPrefix.Size = new System.Drawing.Size(521, 20);
            this.tbDBPrefix.TabIndex = 6;
            this.toolTipControl.SetToolTip(this.tbDBPrefix, "Enter the prefix that will be removed from database names.\r\nThe removal is done w" +
        "hen comparing names during analysis.");
            // 
            // tbOutput
            // 
            this.tbOutput.Controls.Add(this.tbResults);
            this.tbOutput.Location = new System.Drawing.Point(4, 22);
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.Padding = new System.Windows.Forms.Padding(3);
            this.tbOutput.Size = new System.Drawing.Size(611, 364);
            this.tbOutput.TabIndex = 7;
            this.tbOutput.Text = "Output";
            this.tbOutput.UseVisualStyleBackColor = true;
            // 
            // tbResults
            // 
            this.tbResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbResults.Location = new System.Drawing.Point(3, 3);
            this.tbResults.Multiline = true;
            this.tbResults.Name = "tbResults";
            this.tbResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbResults.Size = new System.Drawing.Size(605, 358);
            this.tbResults.TabIndex = 0;
            // 
            // btView
            // 
            this.btView.Location = new System.Drawing.Point(3, 3);
            this.btView.Name = "btView";
            this.btView.Size = new System.Drawing.Size(171, 23);
            this.btView.TabIndex = 9;
            this.btView.Text = "View Results";
            this.btView.UseVisualStyleBackColor = true;
            this.btView.Click += new System.EventHandler(this.btView_Click);
            this.btView.MouseHover += new System.EventHandler(this.btView_MouseHover);
            // 
            // btAnalyze2005
            // 
            this.btAnalyze2005.Location = new System.Drawing.Point(180, 3);
            this.btAnalyze2005.Name = "btAnalyze2005";
            this.btAnalyze2005.Size = new System.Drawing.Size(171, 23);
            this.btAnalyze2005.TabIndex = 10;
            this.btAnalyze2005.Text = "Analyse 2005";
            this.btAnalyze2005.UseVisualStyleBackColor = true;
            this.btAnalyze2005.Click += new System.EventHandler(this.btAnalyze2005_Click);
            this.btAnalyze2005.MouseHover += new System.EventHandler(this.btAnalyze2005_MouseHover);
            // 
            // btAnalyze2008
            // 
            this.btAnalyze2008.Location = new System.Drawing.Point(357, 3);
            this.btAnalyze2008.Name = "btAnalyze2008";
            this.btAnalyze2008.Size = new System.Drawing.Size(171, 23);
            this.btAnalyze2008.TabIndex = 11;
            this.btAnalyze2008.Text = "Analyse 2008";
            this.btAnalyze2008.UseVisualStyleBackColor = true;
            this.btAnalyze2008.Click += new System.EventHandler(this.btAnalyze2008_Click);
            this.btAnalyze2008.MouseHover += new System.EventHandler(this.btAnalyze2008_MouseHover);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btAnalyze2012);
            this.panel1.Controls.Add(this.tbCommandLine);
            this.panel1.Controls.Add(this.btAnalyze2008);
            this.panel1.Controls.Add(this.btView);
            this.panel1.Controls.Add(this.btAnalyze2005);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 390);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(719, 60);
            this.panel1.TabIndex = 1;
            // 
            // tbCommandLine
            // 
            this.tbCommandLine.Location = new System.Drawing.Point(4, 32);
            this.tbCommandLine.Name = "tbCommandLine";
            this.tbCommandLine.ReadOnly = true;
            this.tbCommandLine.Size = new System.Drawing.Size(711, 20);
            this.tbCommandLine.TabIndex = 12;
            // 
            // toolTipControl
            // 
            this.toolTipControl.ToolTipTitle = "Huh?";
            // 
            // folderBrowser
            // 
            this.folderBrowser.Description = "Browse for suitable folders";
            this.folderBrowser.ShowNewFolderButton = false;
            // 
            // btAnalyze2012
            // 
            this.btAnalyze2012.Location = new System.Drawing.Point(534, 3);
            this.btAnalyze2012.Name = "btAnalyze2012";
            this.btAnalyze2012.Size = new System.Drawing.Size(171, 23);
            this.btAnalyze2012.TabIndex = 13;
            this.btAnalyze2012.Text = "Analyse 2012";
            this.btAnalyze2012.UseVisualStyleBackColor = true;
            this.btAnalyze2012.Click += new System.EventHandler(this.btAnalyze2012_Click);
            this.btAnalyze2012.MouseHover += new System.EventHandler(this.btAnalyze2012_MouseHover);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 450);
            this.Controls.Add(this.tcMain);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(735, 488);
            this.MinimumSize = new System.Drawing.Size(735, 488);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Dependency Executor";
            this.tcMain.ResumeLayout(false);
            this.tbRepository.ResumeLayout(false);
            this.tbRepository.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tbScanOptions.ResumeLayout(false);
            this.tbScanOptions.PerformLayout();
            this.tbSSISFolders.ResumeLayout(false);
            this.tbSSISFolders.PerformLayout();
            this.tbFileFolders.ResumeLayout(false);
            this.tbFileFolders.PerformLayout();
            this.tbDatabases.ResumeLayout(false);
            this.tbDatabases.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tbReports.ResumeLayout(false);
            this.tbReports.PerformLayout();
            this.tbAnalysisServers.ResumeLayout(false);
            this.tbAnalysisServers.PerformLayout();
            this.tbNameOverides.ResumeLayout(false);
            this.tbNameOverides.PerformLayout();
            this.tbOutput.ResumeLayout(false);
            this.tbOutput.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tbRepository;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MaskedTextBox tbPassword;
        private System.Windows.Forms.TextBox tbUser;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbSQL;
        private System.Windows.Forms.RadioButton rbWindows;
        private System.Windows.Forms.TextBox tbDatabase;
        private System.Windows.Forms.TextBox tbSQLServer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tbScanOptions;
        private System.Windows.Forms.Button btView;
        private System.Windows.Forms.Button btAnalyze2008;
        private System.Windows.Forms.Button btAnalyze2005;
        private System.Windows.Forms.CheckBox cbRecurse;
        private System.Windows.Forms.ToolTip toolTipControl;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox cbEnableDatabase;
        private System.Windows.Forms.CheckBox cbEnableAS;
        private System.Windows.Forms.CheckBox cbEnableSSIS;
        private System.Windows.Forms.CheckBox cbEnableSQL;
        private System.Windows.Forms.CheckBox cbBatchMode;
        private System.Windows.Forms.CheckBox cbMatchOnDB;
        private System.Windows.Forms.CheckBox cbClearDatabase;
        private System.Windows.Forms.TabPage tbSSISFolders;
        private System.Windows.Forms.TabPage tbFileFolders;
        private System.Windows.Forms.TabPage tbDatabases;
        private System.Windows.Forms.TabPage tbAnalysisServers;
        private System.Windows.Forms.TabPage tbNameOverides;
        private System.Windows.Forms.TextBox tbCommandLine;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbSSISFolder;
        private System.Windows.Forms.Button btSSISDelete;
        private System.Windows.Forms.Button btSSISAdd;
        private System.Windows.Forms.ListBox lbSSIS;
        private System.Windows.Forms.Button btFileDelete;
        private System.Windows.Forms.Button btFileAdd;
        private System.Windows.Forms.ListBox lbFolders;
        private System.Windows.Forms.Button btBrowse;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbFileFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowser;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.MaskedTextBox tbPasswordDB;
        private System.Windows.Forms.TextBox tbUserDB;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbSQLDB;
        private System.Windows.Forms.RadioButton rbWindowsDB;
        private System.Windows.Forms.TextBox tbDatabaseDB;
        private System.Windows.Forms.TextBox tbSQLServerDB;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btDeleteDB;
        private System.Windows.Forms.Button btAddDB;
        private System.Windows.Forms.ListBox lbConnectionDB;
        private System.Windows.Forms.Button btDeletePrefix;
        private System.Windows.Forms.Button btAddPrefix;
        private System.Windows.Forms.ListBox lbDBPrefix;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tbDBPrefix;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox tbASDatabase;
        private System.Windows.Forms.Button btDeleteAS;
        private System.Windows.Forms.Button btAddAS;
        private System.Windows.Forms.ListBox lbASServers;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox tbASServer;
        private System.Windows.Forms.TabPage tbOutput;
        private System.Windows.Forms.TextBox tbResults;
        private System.Windows.Forms.CheckBox cbEnableReportingServices;
        private System.Windows.Forms.TabPage tbReports;
        private System.Windows.Forms.TextBox tbReportPath;
        private System.Windows.Forms.TextBox tbReportServer;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button btDeleteReport;
        private System.Windows.Forms.Button btAddReport;
        private System.Windows.Forms.ListBox lbReportServer;
        private System.Windows.Forms.Button btAnalyze2012;
    }
}

