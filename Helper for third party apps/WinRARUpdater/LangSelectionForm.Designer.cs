namespace WinRARUpdater
{
    partial class LangSelectionForm
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
            this.LangSelectBox = new System.Windows.Forms.ComboBox();
            this.BitSelectBox = new System.Windows.Forms.ComboBox();
            this.DoNotAskAgainCheck = new System.Windows.Forms.CheckBox();
            this.OkBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LangSelectBox
            // 
            this.LangSelectBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LangSelectBox.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.LangSelectBox.FormattingEnabled = true;
            this.LangSelectBox.Items.AddRange(new object[] {
            "Albanian",
            "Arabic",
            "Armenian",
            "Belarusian",
            "Bulgarian",
            "Catalan",
            "Chinese Simplified",
            "Chinese Traditional",
            "Croatian",
            "Czech",
            "Danish",
            "Dutch",
            "English",
            "Estonian",
            "Finnish",
            "French",
            "Galician",
            "Georgian",
            "German",
            "Greek",
            "Hebrew",
            "Hungarian",
            "Indonesian",
            "Italian",
            "Japanese",
            "Korean",
            "Lithuanian",
            "Norwegian",
            "Persian",
            "Polish",
            "Portuguese",
            "Portuguese Brazilian",
            "Romanian",
            "Russian",
            "Serbian Cyrillic",
            "Sinhala",
            "Slovak",
            "Slovenian",
            "Spanish",
            "Swedish",
            "Thai",
            "Turkish",
            "Turkmen",
            "Ukrainian",
            "Vietnamese"});
            this.LangSelectBox.Location = new System.Drawing.Point(13, 16);
            this.LangSelectBox.Name = "LangSelectBox";
            this.LangSelectBox.Size = new System.Drawing.Size(197, 21);
            this.LangSelectBox.TabIndex = 0;
            // 
            // BitSelectBox
            // 
            this.BitSelectBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BitSelectBox.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.BitSelectBox.FormattingEnabled = true;
            this.BitSelectBox.Items.AddRange(new object[] {
            "32 bit",
            "64 bit"});
            this.BitSelectBox.Location = new System.Drawing.Point(216, 16);
            this.BitSelectBox.Name = "BitSelectBox";
            this.BitSelectBox.Size = new System.Drawing.Size(77, 21);
            this.BitSelectBox.TabIndex = 1;
            // 
            // DoNotAskAgainCheck
            // 
            this.DoNotAskAgainCheck.AutoSize = true;
            this.DoNotAskAgainCheck.BackColor = System.Drawing.Color.Transparent;
            this.DoNotAskAgainCheck.Checked = true;
            this.DoNotAskAgainCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DoNotAskAgainCheck.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.DoNotAskAgainCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.DoNotAskAgainCheck.Location = new System.Drawing.Point(20, 50);
            this.DoNotAskAgainCheck.Name = "DoNotAskAgainCheck";
            this.DoNotAskAgainCheck.Size = new System.Drawing.Size(110, 17);
            this.DoNotAskAgainCheck.TabIndex = 2;
            this.DoNotAskAgainCheck.Text = "Do not ask again!";
            this.DoNotAskAgainCheck.UseVisualStyleBackColor = false;
            // 
            // OkBtn
            // 
            this.OkBtn.Location = new System.Drawing.Point(132, 46);
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.Size = new System.Drawing.Size(75, 23);
            this.OkBtn.TabIndex = 3;
            this.OkBtn.Text = "OK";
            this.OkBtn.UseVisualStyleBackColor = true;
            this.OkBtn.Click += new System.EventHandler(this.OkBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(211, 46);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 4;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // LangSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::WinRARUpdater.Properties.Resources.diagonal_pattern;
            this.ClientSize = new System.Drawing.Size(305, 84);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OkBtn);
            this.Controls.Add(this.DoNotAskAgainCheck);
            this.Controls.Add(this.BitSelectBox);
            this.Controls.Add(this.LangSelectBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LangSelectionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WinRAR Language Selection";
            this.Load += new System.EventHandler(this.LangSelectionForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox LangSelectBox;
        private System.Windows.Forms.ComboBox BitSelectBox;
        private System.Windows.Forms.CheckBox DoNotAskAgainCheck;
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.Button CancelBtn;
    }
}