using System;
using System.Windows.Forms;

namespace WinRARUpdater
{
    public partial class LangSelectionForm : Form
    {
        public LangSelectionForm()
        {
            InitializeComponent();
            this.Icon = (System.Drawing.Icon)Properties.Resources.RAR;
        }

        private void LangSelectionForm_Load(object sender, EventArgs e)
        {
            try
            {
                string Lang = SilDev.Initialization.ReadValue("Settings", "Language");
                LangSelectBox.SelectedItem = Lang;
                string Bit = SilDev.Initialization.ReadValue("Settings", "Architecture");
                BitSelectBox.SelectedItem = Bit;
            }
            catch
            {
                this.Close();
            }
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            try
            {
                SilDev.Initialization.WriteValue("Settings", "Language", LangSelectBox.SelectedItem.ToString());
                SilDev.Initialization.WriteValue("Settings", "Architecture", BitSelectBox.SelectedItem.ToString());
                SilDev.Initialization.WriteValue("Settings", "DoNotAskAgain", DoNotAskAgainCheck.Checked.ToString());
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
