using System;
using System.Windows.Forms;

namespace WinRARUpdater
{
    public partial class LangSelectionForm : Form
    {
        public LangSelectionForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.RAR;
        }

        private void LangSelectionForm_Load(object sender, EventArgs e)
        {
            try
            {
                string Lang = SilDev.Ini.Read("Settings", "Language");
                LangSelectBox.SelectedItem = Lang;
                string Bit = SilDev.Ini.Read("Settings", "Architecture");
                BitSelectBox.SelectedItem = Bit;
            }
            catch
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            SilDev.Ini.Write("Settings", "Language", LangSelectBox.GetItemText(LangSelectBox.SelectedItem));
            SilDev.Ini.Write("Settings", "Architecture", BitSelectBox.GetItemText(BitSelectBox.SelectedItem));
            SilDev.Ini.Write("Settings", "DoNotAskAgain", DoNotAskAgainCheck.Checked);
            DialogResult = DialogResult.OK;
        }

        private void CancelBtn_Click(object sender, EventArgs e) =>
            DialogResult = DialogResult.Cancel;
    }
}
