namespace AppUpdater
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
            var lang = Ini.Read<string>("Settings", "Language", "English");
            var arch = Ini.Read<string>("Settings", "Architecture", Environment.Is64BitProcess ? "64 bit" : "32 bit");
            try
            {
                LangSelectBox.SelectedItem = lang;
                ArchSelectBox.SelectedItem = arch;
            }
            catch
            {
                DialogResult = DialogResult.Cancel;
            }
            if (Environment.CommandLine.ContainsEx("/archlock"))
                ArchSelectBox.Enabled = false;
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            Ini.Write("Settings", "Language", LangSelectBox.GetItemText(LangSelectBox.SelectedItem));
            Ini.Write("Settings", "Architecture", ArchSelectBox.GetItemText(ArchSelectBox.SelectedItem));
            Ini.Write("Settings", "DoNotAskAgain", DoNotAskAgainCheck.Checked);
            Ini.WriteAll();
            DialogResult = DialogResult.OK;
        }

        private void CancelBtn_Click(object sender, EventArgs e) =>
            DialogResult = DialogResult.Cancel;
    }
}
