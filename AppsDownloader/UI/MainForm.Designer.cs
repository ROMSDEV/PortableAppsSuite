namespace AppsDownloader.UI
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("listViewGroup0", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("listViewGroup1", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("listViewGroup2", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("listViewGroup3", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup5 = new System.Windows.Forms.ListViewGroup("listViewGroup4", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup6 = new System.Windows.Forms.ListViewGroup("listViewGroup5", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup7 = new System.Windows.Forms.ListViewGroup("listViewGroup6", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup8 = new System.Windows.Forms.ListViewGroup("listViewGroup7", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup9 = new System.Windows.Forms.ListViewGroup("listViewGroup8", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup10 = new System.Windows.Forms.ListViewGroup("listViewGroup9", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup11 = new System.Windows.Forms.ListViewGroup("listViewGroup10", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup12 = new System.Windows.Forms.ListViewGroup("listViewGroup11", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup13 = new System.Windows.Forms.ListViewGroup("listViewGroup12", System.Windows.Forms.HorizontalAlignment.Left);
            this.checkDownload = new System.Windows.Forms.Timer(this.components);
            this.multiDownloader = new System.Windows.Forms.Timer(this.components);
            this.statusBar = new System.Windows.Forms.Panel();
            this.timeStatus = new System.Windows.Forms.Label();
            this.appStatusBorder = new System.Windows.Forms.Label();
            this.appStatus = new System.Windows.Forms.Label();
            this.fileStatusBorder = new System.Windows.Forms.Label();
            this.fileStatus = new System.Windows.Forms.Label();
            this.urlStatus = new System.Windows.Forms.Label();
            this.statusBarBorder = new System.Windows.Forms.Panel();
            this.downloadStateAreaPanel = new System.Windows.Forms.Panel();
            this.downloadProgress = new System.Windows.Forms.Panel();
            this.buttonArea = new System.Windows.Forms.Panel();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.okBtn = new System.Windows.Forms.Button();
            this.downloadReceived = new System.Windows.Forms.Label();
            this.downloadSpeed = new System.Windows.Forms.Label();
            this.settingsAreaBorder = new System.Windows.Forms.Panel();
            this.settingsArea = new System.Windows.Forms.Panel();
            this.highlightInstalledCheck = new System.Windows.Forms.CheckBox();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.showColorsCheck = new System.Windows.Forms.CheckBox();
            this.showGroupsCheck = new System.Windows.Forms.CheckBox();
            this.appsList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.appMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.appMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.appMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.appMenuItemSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.appMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.imgList = new System.Windows.Forms.ImageList(this.components);
            this.searchResultBlinker = new System.Windows.Forms.Timer(this.components);
            this.appsListBorder = new System.Windows.Forms.Panel();
            this.statusBar.SuspendLayout();
            this.downloadStateAreaPanel.SuspendLayout();
            this.buttonArea.SuspendLayout();
            this.settingsArea.SuspendLayout();
            this.appMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkDownload
            // 
            this.checkDownload.Interval = 10;
            this.checkDownload.Tick += new System.EventHandler(this.CheckDownload_Tick);
            // 
            // multiDownloader
            // 
            this.multiDownloader.Tick += new System.EventHandler(this.MultiDownloader_Tick);
            // 
            // statusBar
            // 
            this.statusBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(56)))), ((int)(((byte)(64)))));
            this.statusBar.Controls.Add(this.timeStatus);
            this.statusBar.Controls.Add(this.appStatusBorder);
            this.statusBar.Controls.Add(this.appStatus);
            this.statusBar.Controls.Add(this.fileStatusBorder);
            this.statusBar.Controls.Add(this.fileStatus);
            this.statusBar.Controls.Add(this.urlStatus);
            this.statusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBar.Location = new System.Drawing.Point(0, 625);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(792, 24);
            this.statusBar.TabIndex = 0;
            // 
            // timeStatus
            // 
            this.timeStatus.BackColor = System.Drawing.Color.Transparent;
            this.timeStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.timeStatus.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.timeStatus.ForeColor = System.Drawing.Color.SlateGray;
            this.timeStatus.Location = new System.Drawing.Point(230, 0);
            this.timeStatus.Name = "timeStatus";
            this.timeStatus.Size = new System.Drawing.Size(114, 24);
            this.timeStatus.TabIndex = 0;
            this.timeStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.timeStatus.TextChanged += new System.EventHandler(this.Status_TextChanged);
            // 
            // appStatusBorder
            // 
            this.appStatusBorder.BackColor = System.Drawing.Color.Black;
            this.appStatusBorder.Dock = System.Windows.Forms.DockStyle.Left;
            this.appStatusBorder.Location = new System.Drawing.Point(229, 0);
            this.appStatusBorder.Name = "appStatusBorder";
            this.appStatusBorder.Size = new System.Drawing.Size(1, 24);
            this.appStatusBorder.TabIndex = 0;
            this.appStatusBorder.Text = " 0 Apps found!";
            this.appStatusBorder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.appStatusBorder.Visible = false;
            // 
            // appStatus
            // 
            this.appStatus.BackColor = System.Drawing.Color.Transparent;
            this.appStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.appStatus.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appStatus.ForeColor = System.Drawing.Color.SlateGray;
            this.appStatus.Location = new System.Drawing.Point(115, 0);
            this.appStatus.Name = "appStatus";
            this.appStatus.Size = new System.Drawing.Size(114, 24);
            this.appStatus.TabIndex = 0;
            this.appStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.appStatus.TextChanged += new System.EventHandler(this.Status_TextChanged);
            // 
            // fileStatusBorder
            // 
            this.fileStatusBorder.BackColor = System.Drawing.Color.Black;
            this.fileStatusBorder.Dock = System.Windows.Forms.DockStyle.Left;
            this.fileStatusBorder.Location = new System.Drawing.Point(114, 0);
            this.fileStatusBorder.Name = "fileStatusBorder";
            this.fileStatusBorder.Size = new System.Drawing.Size(1, 24);
            this.fileStatusBorder.TabIndex = 0;
            this.fileStatusBorder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.fileStatusBorder.Visible = false;
            // 
            // fileStatus
            // 
            this.fileStatus.BackColor = System.Drawing.Color.Transparent;
            this.fileStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.fileStatus.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileStatus.ForeColor = System.Drawing.Color.SlateGray;
            this.fileStatus.Location = new System.Drawing.Point(0, 0);
            this.fileStatus.Name = "fileStatus";
            this.fileStatus.Size = new System.Drawing.Size(114, 24);
            this.fileStatus.TabIndex = 0;
            this.fileStatus.Text = " 0 Apps found!";
            this.fileStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.fileStatus.TextChanged += new System.EventHandler(this.Status_TextChanged);
            // 
            // urlStatus
            // 
            this.urlStatus.BackColor = System.Drawing.Color.Transparent;
            this.urlStatus.Dock = System.Windows.Forms.DockStyle.Right;
            this.urlStatus.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.urlStatus.ForeColor = System.Drawing.Color.SlateGray;
            this.urlStatus.Location = new System.Drawing.Point(584, 0);
            this.urlStatus.Name = "urlStatus";
            this.urlStatus.Size = new System.Drawing.Size(208, 24);
            this.urlStatus.TabIndex = 0;
            this.urlStatus.Text = "www.si13n7.com ";
            this.urlStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.urlStatus.TextChanged += new System.EventHandler(this.Status_TextChanged);
            this.urlStatus.Click += new System.EventHandler(this.UrlStatus_Click);
            // 
            // statusBarBorder
            // 
            this.statusBarBorder.BackColor = System.Drawing.Color.Black;
            this.statusBarBorder.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBarBorder.Location = new System.Drawing.Point(0, 624);
            this.statusBarBorder.Name = "statusBarBorder";
            this.statusBarBorder.Size = new System.Drawing.Size(792, 1);
            this.statusBarBorder.TabIndex = 1;
            // 
            // downloadStateAreaPanel
            // 
            this.downloadStateAreaPanel.BackColor = System.Drawing.Color.Gray;
            this.downloadStateAreaPanel.BackgroundImage = global::AppsDownloader.Properties.Resources.diagonal_pattern;
            this.downloadStateAreaPanel.Controls.Add(this.downloadProgress);
            this.downloadStateAreaPanel.Controls.Add(this.buttonArea);
            this.downloadStateAreaPanel.Controls.Add(this.downloadReceived);
            this.downloadStateAreaPanel.Controls.Add(this.downloadSpeed);
            this.downloadStateAreaPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.downloadStateAreaPanel.Location = new System.Drawing.Point(0, 556);
            this.downloadStateAreaPanel.Name = "downloadStateAreaPanel";
            this.downloadStateAreaPanel.Size = new System.Drawing.Size(792, 68);
            this.downloadStateAreaPanel.TabIndex = 0;
            // 
            // downloadProgress
            // 
            this.downloadProgress.BackColor = System.Drawing.Color.WhiteSmoke;
            this.downloadProgress.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.downloadProgress.Location = new System.Drawing.Point(26, 30);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.Size = new System.Drawing.Size(374, 10);
            this.downloadProgress.TabIndex = 0;
            this.downloadProgress.Visible = false;
            // 
            // buttonArea
            // 
            this.buttonArea.BackColor = System.Drawing.Color.Transparent;
            this.buttonArea.Controls.Add(this.cancelBtn);
            this.buttonArea.Controls.Add(this.okBtn);
            this.buttonArea.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonArea.Location = new System.Drawing.Point(584, 0);
            this.buttonArea.Name = "buttonArea";
            this.buttonArea.Size = new System.Drawing.Size(208, 68);
            this.buttonArea.TabIndex = 0;
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(103, 24);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 101;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // okBtn
            // 
            this.okBtn.Enabled = false;
            this.okBtn.Location = new System.Drawing.Point(3, 24);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(75, 23);
            this.okBtn.TabIndex = 100;
            this.okBtn.Text = "OK";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.OkBtn_Click);
            // 
            // downloadReceived
            // 
            this.downloadReceived.BackColor = System.Drawing.Color.Transparent;
            this.downloadReceived.ForeColor = System.Drawing.Color.PaleTurquoise;
            this.downloadReceived.Location = new System.Drawing.Point(26, 43);
            this.downloadReceived.Name = "downloadReceived";
            this.downloadReceived.Size = new System.Drawing.Size(374, 13);
            this.downloadReceived.TabIndex = 0;
            this.downloadReceived.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.downloadReceived.Visible = false;
            // 
            // downloadSpeed
            // 
            this.downloadSpeed.BackColor = System.Drawing.Color.Transparent;
            this.downloadSpeed.ForeColor = System.Drawing.Color.PaleTurquoise;
            this.downloadSpeed.Location = new System.Drawing.Point(26, 14);
            this.downloadSpeed.Name = "downloadSpeed";
            this.downloadSpeed.Size = new System.Drawing.Size(374, 13);
            this.downloadSpeed.TabIndex = 0;
            this.downloadSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.downloadSpeed.Visible = false;
            // 
            // settingsAreaBorder
            // 
            this.settingsAreaBorder.BackColor = System.Drawing.Color.Black;
            this.settingsAreaBorder.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.settingsAreaBorder.Location = new System.Drawing.Point(0, 555);
            this.settingsAreaBorder.Name = "settingsAreaBorder";
            this.settingsAreaBorder.Size = new System.Drawing.Size(792, 1);
            this.settingsAreaBorder.TabIndex = 3;
            // 
            // settingsArea
            // 
            this.settingsArea.BackColor = System.Drawing.Color.Transparent;
            this.settingsArea.Controls.Add(this.highlightInstalledCheck);
            this.settingsArea.Controls.Add(this.searchBox);
            this.settingsArea.Controls.Add(this.showColorsCheck);
            this.settingsArea.Controls.Add(this.showGroupsCheck);
            this.settingsArea.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.settingsArea.Location = new System.Drawing.Point(0, 523);
            this.settingsArea.Name = "settingsArea";
            this.settingsArea.Size = new System.Drawing.Size(792, 32);
            this.settingsArea.TabIndex = 0;
            // 
            // highlightInstalledCheck
            // 
            this.highlightInstalledCheck.AutoSize = true;
            this.highlightInstalledCheck.Checked = true;
            this.highlightInstalledCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.highlightInstalledCheck.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.highlightInstalledCheck.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.highlightInstalledCheck.Location = new System.Drawing.Point(313, 7);
            this.highlightInstalledCheck.Name = "highlightInstalledCheck";
            this.highlightInstalledCheck.Size = new System.Drawing.Size(127, 19);
            this.highlightInstalledCheck.TabIndex = 4;
            this.highlightInstalledCheck.Text = "Highlight Installed";
            this.highlightInstalledCheck.UseVisualStyleBackColor = true;
            this.highlightInstalledCheck.CheckedChanged += new System.EventHandler(this.HighlightInstalledCheck_CheckedChanged);
            // 
            // searchBox
            // 
            this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.searchBox.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.searchBox.Location = new System.Drawing.Point(565, 5);
            this.searchBox.Multiline = true;
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(218, 22);
            this.searchBox.TabIndex = 1;
            this.searchBox.TextChanged += new System.EventHandler(this.SearchBox_TextChanged);
            this.searchBox.Enter += new System.EventHandler(this.SearchBox_Enter);
            this.searchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchBox_KeyDown);
            // 
            // showColorsCheck
            // 
            this.showColorsCheck.AutoSize = true;
            this.showColorsCheck.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.showColorsCheck.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.showColorsCheck.Location = new System.Drawing.Point(150, 7);
            this.showColorsCheck.Name = "showColorsCheck";
            this.showColorsCheck.Size = new System.Drawing.Size(132, 19);
            this.showColorsCheck.TabIndex = 3;
            this.showColorsCheck.Text = "Show Group Colors";
            this.showColorsCheck.UseVisualStyleBackColor = true;
            this.showColorsCheck.CheckedChanged += new System.EventHandler(this.ShowColorsCheck_CheckedChanged);
            // 
            // showGroupsCheck
            // 
            this.showGroupsCheck.AutoSize = true;
            this.showGroupsCheck.Checked = true;
            this.showGroupsCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showGroupsCheck.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.showGroupsCheck.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.showGroupsCheck.Location = new System.Drawing.Point(14, 7);
            this.showGroupsCheck.Name = "showGroupsCheck";
            this.showGroupsCheck.Size = new System.Drawing.Size(100, 19);
            this.showGroupsCheck.TabIndex = 2;
            this.showGroupsCheck.Text = "Show Groups";
            this.showGroupsCheck.UseVisualStyleBackColor = true;
            this.showGroupsCheck.CheckedChanged += new System.EventHandler(this.ShowGroupsCheck_CheckedChanged);
            // 
            // appsList
            // 
            this.appsList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.appsList.CheckBoxes = true;
            this.appsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.appsList.ContextMenuStrip = this.appMenu;
            this.appsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.appsList.FullRowSelect = true;
            listViewGroup1.Header = "listViewGroup0";
            listViewGroup1.Name = "listViewGroup0";
            listViewGroup2.Header = "listViewGroup1";
            listViewGroup2.Name = "listViewGroup1";
            listViewGroup3.Header = "listViewGroup2";
            listViewGroup3.Name = "listViewGroup2";
            listViewGroup4.Header = "listViewGroup3";
            listViewGroup4.Name = "listViewGroup3";
            listViewGroup5.Header = "listViewGroup4";
            listViewGroup5.Name = "listViewGroup4";
            listViewGroup6.Header = "listViewGroup5";
            listViewGroup6.Name = "listViewGroup5";
            listViewGroup7.Header = "listViewGroup6";
            listViewGroup7.Name = "listViewGroup6";
            listViewGroup8.Header = "listViewGroup7";
            listViewGroup8.Name = "listViewGroup7";
            listViewGroup9.Header = "listViewGroup8";
            listViewGroup9.Name = "listViewGroup8";
            listViewGroup10.Header = "listViewGroup9";
            listViewGroup10.Name = "listViewGroup9";
            listViewGroup11.Header = "listViewGroup10";
            listViewGroup11.Name = "listViewGroup10";
            listViewGroup12.Header = "listViewGroup11";
            listViewGroup12.Name = "listViewGroup11";
            listViewGroup13.Header = "listViewGroup12";
            listViewGroup13.Name = "listViewGroup12";
            this.appsList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3,
            listViewGroup4,
            listViewGroup5,
            listViewGroup6,
            listViewGroup7,
            listViewGroup8,
            listViewGroup9,
            listViewGroup10,
            listViewGroup11,
            listViewGroup12,
            listViewGroup13});
            this.appsList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.appsList.LabelWrap = false;
            this.appsList.Location = new System.Drawing.Point(0, 0);
            this.appsList.MultiSelect = false;
            this.appsList.Name = "appsList";
            this.appsList.Size = new System.Drawing.Size(792, 523);
            this.appsList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.appsList.TabIndex = 0;
            this.appsList.TabStop = false;
            this.appsList.UseCompatibleStateImageBehavior = false;
            this.appsList.View = System.Windows.Forms.View.Details;
            this.appsList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.AppsList_ItemCheck);
            this.appsList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.AppsList_ItemChecked);
            this.appsList.Enter += new System.EventHandler(this.AppsList_Enter);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Application";
            this.columnHeader1.Width = 189;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Description";
            this.columnHeader2.Width = 270;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Version";
            this.columnHeader3.Width = 81;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Download Size";
            this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Installed Size";
            this.columnHeader5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Source";
            this.columnHeader6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader6.Width = 115;
            // 
            // appMenu
            // 
            this.appMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.appMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.appMenuItem1,
            this.appMenuItem2,
            this.appMenuItemSeparator1,
            this.appMenuItem3});
            this.appMenu.Name = "addMenu";
            this.appMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.appMenu.Size = new System.Drawing.Size(162, 76);
            this.appMenu.Opening += new System.ComponentModel.CancelEventHandler(this.AppMenu_Opening);
            // 
            // appMenuItem1
            // 
            this.appMenuItem1.ForeColor = System.Drawing.Color.Silver;
            this.appMenuItem1.Name = "appMenuItem1";
            this.appMenuItem1.Size = new System.Drawing.Size(161, 22);
            this.appMenuItem1.Text = "Check";
            this.appMenuItem1.Click += new System.EventHandler(this.AppMenuItem_Click);
            // 
            // appMenuItem2
            // 
            this.appMenuItem2.ForeColor = System.Drawing.Color.Silver;
            this.appMenuItem2.Name = "appMenuItem2";
            this.appMenuItem2.Size = new System.Drawing.Size(161, 22);
            this.appMenuItem2.Text = "Check All";
            this.appMenuItem2.Click += new System.EventHandler(this.AppMenuItem_Click);
            // 
            // appMenuItemSeparator1
            // 
            this.appMenuItemSeparator1.Name = "appMenuItemSeparator1";
            this.appMenuItemSeparator1.Size = new System.Drawing.Size(158, 6);
            // 
            // appMenuItem3
            // 
            this.appMenuItem3.ForeColor = System.Drawing.Color.Silver;
            this.appMenuItem3.Name = "appMenuItem3";
            this.appMenuItem3.Size = new System.Drawing.Size(161, 22);
            this.appMenuItem3.Text = "Open in Browser";
            this.appMenuItem3.Click += new System.EventHandler(this.AppMenuItem_Click);
            // 
            // imgList
            // 
            this.imgList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imgList.ImageSize = new System.Drawing.Size(16, 16);
            this.imgList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // searchResultBlinker
            // 
            this.searchResultBlinker.Interval = 300;
            this.searchResultBlinker.Tick += new System.EventHandler(this.SearchResultBlinker_Tick);
            // 
            // appsListBorder
            // 
            this.appsListBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.appsListBorder.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.appsListBorder.Location = new System.Drawing.Point(0, 522);
            this.appsListBorder.Name = "appsListBorder";
            this.appsListBorder.Size = new System.Drawing.Size(792, 1);
            this.appsListBorder.TabIndex = 4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(84)))), ((int)(((byte)(86)))));
            this.ClientSize = new System.Drawing.Size(792, 649);
            this.Controls.Add(this.appsListBorder);
            this.Controls.Add(this.appsList);
            this.Controls.Add(this.settingsArea);
            this.Controls.Add(this.settingsAreaBorder);
            this.Controls.Add(this.downloadStateAreaPanel);
            this.Controls.Add(this.statusBarBorder);
            this.Controls.Add(this.statusBar);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(760, 336);
            this.Name = "MainForm";
            this.Opacity = 0D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Apps Downloader";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.ResizeBegin += new System.EventHandler(this.MainForm_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
            this.statusBar.ResumeLayout(false);
            this.downloadStateAreaPanel.ResumeLayout(false);
            this.buttonArea.ResumeLayout(false);
            this.settingsArea.ResumeLayout(false);
            this.settingsArea.PerformLayout();
            this.appMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer checkDownload;
        private System.Windows.Forms.Timer multiDownloader;
        private System.Windows.Forms.Panel statusBar;
        private System.Windows.Forms.Panel statusBarBorder;
        private System.Windows.Forms.Panel downloadStateAreaPanel;
        private System.Windows.Forms.Panel settingsAreaBorder;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Label downloadReceived;
        private System.Windows.Forms.Label downloadSpeed;
        private System.Windows.Forms.Panel settingsArea;
        private System.Windows.Forms.ListView appsList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Label urlStatus;
        private System.Windows.Forms.Panel buttonArea;
        private System.Windows.Forms.Label fileStatus;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.CheckBox showGroupsCheck;
        private System.Windows.Forms.CheckBox showColorsCheck;
        private System.Windows.Forms.ImageList imgList;
        private System.Windows.Forms.Timer searchResultBlinker;
        private System.Windows.Forms.Panel downloadProgress;
        private System.Windows.Forms.CheckBox highlightInstalledCheck;
        private System.Windows.Forms.Panel appsListBorder;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ContextMenuStrip appMenu;
        private System.Windows.Forms.ToolStripMenuItem appMenuItem1;
        private System.Windows.Forms.ToolStripSeparator appMenuItemSeparator1;
        private System.Windows.Forms.ToolStripMenuItem appMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem appMenuItem3;
        private System.Windows.Forms.Label appStatusBorder;
        private System.Windows.Forms.Label appStatus;
        private System.Windows.Forms.Label fileStatusBorder;
        private System.Windows.Forms.Label timeStatus;
    }
}

