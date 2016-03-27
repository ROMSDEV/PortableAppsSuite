namespace WinRARUpdater
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.DLPercentage = new System.Windows.Forms.ProgressBar();
            this.DLLoaded = new System.Windows.Forms.Label();
            this.DLSpeed = new System.Windows.Forms.Label();
            this.CheckDownload = new System.Windows.Forms.Timer(this.components);
            this.ExtractDownload = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // DLPercentage
            // 
            this.DLPercentage.Location = new System.Drawing.Point(11, 39);
            this.DLPercentage.Name = "DLPercentage";
            this.DLPercentage.Size = new System.Drawing.Size(242, 23);
            this.DLPercentage.TabIndex = 10;
            // 
            // DLLoaded
            // 
            this.DLLoaded.BackColor = System.Drawing.Color.Transparent;
            this.DLLoaded.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DLLoaded.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.DLLoaded.Location = new System.Drawing.Point(11, 65);
            this.DLLoaded.Name = "DLLoaded";
            this.DLLoaded.Size = new System.Drawing.Size(242, 23);
            this.DLLoaded.TabIndex = 9;
            this.DLLoaded.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DLSpeed
            // 
            this.DLSpeed.BackColor = System.Drawing.Color.Transparent;
            this.DLSpeed.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DLSpeed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.DLSpeed.Location = new System.Drawing.Point(11, 13);
            this.DLSpeed.Name = "DLSpeed";
            this.DLSpeed.Size = new System.Drawing.Size(242, 23);
            this.DLSpeed.TabIndex = 8;
            this.DLSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CheckDownload
            // 
            this.CheckDownload.Interval = 10;
            this.CheckDownload.Tick += new System.EventHandler(this.CheckDownload_Tick);
            // 
            // ExtractDownload
            // 
            this.ExtractDownload.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ExtractDownload_DoWork);
            this.ExtractDownload.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.ExtractDownload_RunWorkerCompleted);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = global::WinRARUpdater.Properties.Resources.diagonal_pattern;
            this.ClientSize = new System.Drawing.Size(264, 100);
            this.Controls.Add(this.DLPercentage);
            this.Controls.Add(this.DLLoaded);
            this.Controls.Add(this.DLSpeed);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Opacity = 0D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WinRAR Updater (64-bit)";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar DLPercentage;
        private System.Windows.Forms.Label DLLoaded;
        private System.Windows.Forms.Label DLSpeed;
        private System.Windows.Forms.Timer CheckDownload;
        private System.ComponentModel.BackgroundWorker ExtractDownload;
    }
}

