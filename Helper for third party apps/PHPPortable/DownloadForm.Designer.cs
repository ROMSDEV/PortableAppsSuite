namespace RunPHP
{
    partial class DownloadForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DownloadForm));
            this.CheckDownload = new System.Windows.Forms.Timer(this.components);
            this.ExtractDownload = new System.ComponentModel.BackgroundWorker();
            this.panel1 = new System.Windows.Forms.Panel();
            this.DLPercentage = new System.Windows.Forms.ProgressBar();
            this.DLLoaded = new System.Windows.Forms.Label();
            this.DLSpeed = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // CheckDownload
            // 
            this.CheckDownload.Tick += new System.EventHandler(this.CheckDownload_Tick);
            // 
            // ExtractDownload
            // 
            this.ExtractDownload.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ExtractDownload_DoWork);
            this.ExtractDownload.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.ExtractDownload_RunWorkerCompleted);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.DLPercentage);
            this.panel1.Controls.Add(this.DLLoaded);
            this.panel1.Controls.Add(this.DLSpeed);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(268, 106);
            this.panel1.TabIndex = 0;
            // 
            // DLPercentage
            // 
            this.DLPercentage.Location = new System.Drawing.Point(13, 42);
            this.DLPercentage.Name = "DLPercentage";
            this.DLPercentage.Size = new System.Drawing.Size(242, 23);
            this.DLPercentage.TabIndex = 7;
            // 
            // DLLoaded
            // 
            this.DLLoaded.BackColor = System.Drawing.Color.Transparent;
            this.DLLoaded.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DLLoaded.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.DLLoaded.Location = new System.Drawing.Point(13, 68);
            this.DLLoaded.Name = "DLLoaded";
            this.DLLoaded.Size = new System.Drawing.Size(242, 23);
            this.DLLoaded.TabIndex = 6;
            this.DLLoaded.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DLSpeed
            // 
            this.DLSpeed.BackColor = System.Drawing.Color.Transparent;
            this.DLSpeed.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DLSpeed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.DLSpeed.Location = new System.Drawing.Point(13, 16);
            this.DLSpeed.Name = "DLSpeed";
            this.DLSpeed.Size = new System.Drawing.Size(242, 23);
            this.DLSpeed.TabIndex = 5;
            this.DLSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DownloadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(268, 106);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DownloadForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Download";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DownloadForm_FormClosed);
            this.Load += new System.EventHandler(this.DownloadForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer CheckDownload;
        private System.ComponentModel.BackgroundWorker ExtractDownload;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ProgressBar DLPercentage;
        private System.Windows.Forms.Label DLLoaded;
        private System.Windows.Forms.Label DLSpeed;
    }
}