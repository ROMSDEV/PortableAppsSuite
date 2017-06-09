namespace WinampPortable
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Forms;
    using Properties;
    using SilDev;

    public partial class LangSelectionForm : Form
    {
        private readonly object[] _languages;
        private readonly string _configPath;

        [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
        public LangSelectionForm(string[] languages, string configPath)
        {
            InitializeComponent();
            Icon = Resources.Winamp;
            _languages = languages;
            _configPath = configPath;
        }

        private void LangSelectionForm_Load(object sender, EventArgs e)
        {
            try
            {
                LangSelectBox.Items.Add("English (US)");
                LangSelectBox.Items.AddRange(_languages);
                LangSelectBox.SelectedIndex = 0;
            }
            catch
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            var lang = LangSelectBox.GetItemText(LangSelectBox.SelectedItem);
            if (!lang.EqualsEx("English (US)"))
                Ini.WriteDirect("Winamp", "langpack", $"{lang}.wlz", _configPath);
            DialogResult = DialogResult.OK;
        }

        private void CancelBtn_Click(object sender, EventArgs e) =>
            DialogResult = DialogResult.Cancel;
    }
}
