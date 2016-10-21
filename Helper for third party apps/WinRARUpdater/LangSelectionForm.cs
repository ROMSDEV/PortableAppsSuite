namespace WinRARUpdater
{
    using System;
    using System.Windows.Forms;
    using Properties;
    using SilDev;

    public partial class LangSelectionForm : Form
    {
        public LangSelectionForm()
        {
            InitializeComponent();
            Icon = Resources.RAR;
        }

        private void LangSelectionForm_Load(object sender, EventArgs e)
        {
            try
            {
                var lang = Ini.Read("Settings", "Language");
                LangSelectBox.SelectedItem = lang;
                var bit = Ini.Read("Settings", "Architecture");
                BitSelectBox.SelectedItem = bit;
            }
            catch
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            Ini.Write("Settings", "Language", LangSelectBox.GetItemText(LangSelectBox.SelectedItem));
            Ini.Write("Settings", "Architecture", BitSelectBox.GetItemText(BitSelectBox.SelectedItem));
            Ini.Write("Settings", "DoNotAskAgain", DoNotAskAgainCheck.Checked);
            DialogResult = DialogResult.OK;
        }

        private void CancelBtn_Click(object sender, EventArgs e) =>
            DialogResult = DialogResult.Cancel;
    }
}
