namespace AppUpdater
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
            this.BackgroundImage = global::AppUpdater.Properties.Resources.diagonal_pattern;
            this.ClientSize = new System.Drawing.Size(264, 100);
            this.Controls.Add(this.DLPercentage);
            this.Controls.Add(this.DLLoaded);
            this.Controls.Add(this.DLSpeed);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Opacity = 0D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
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

