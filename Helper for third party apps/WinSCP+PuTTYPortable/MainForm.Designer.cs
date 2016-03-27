namespace WinSCP_PuTTY_Launcher
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
            this.bwWorker = new System.ComponentModel.BackgroundWorker();
            this.PuttyTabber = new System.Windows.Forms.Timer(this.components);
            this.puttyTabCtrl = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // bwWorker
            // 
            this.bwWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwWorker_DoWork);
            this.bwWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwWorker_RunWorkerCompleted);
            // 
            // PuttyTabber
            // 
            this.PuttyTabber.Interval = 10;
            this.PuttyTabber.Tick += new System.EventHandler(this.PuttyTabber_Tick);
            // 
            // puttyTabCtrl
            // 
            this.puttyTabCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.puttyTabCtrl.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.puttyTabCtrl.Location = new System.Drawing.Point(0, 0);
            this.puttyTabCtrl.Name = "puttyTabCtrl";
            this.puttyTabCtrl.SelectedIndex = 0;
            this.puttyTabCtrl.Size = new System.Drawing.Size(792, 569);
            this.puttyTabCtrl.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(792, 569);
            this.Controls.Add(this.puttyTabCtrl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Opacity = 0D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PuTTY Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResizeBegin += new System.EventHandler(this.MainForm_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker bwWorker;
        private System.Windows.Forms.Timer PuttyTabber;
        private System.Windows.Forms.TabControl puttyTabCtrl;

    }
}