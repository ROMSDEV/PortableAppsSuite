namespace RunPHP
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using SilDev.Forms;

    public partial class DownloadForm : Form
    {
        private readonly NetEx.AsyncTransfer _transfer = new NetEx.AsyncTransfer();
        private readonly string _appPath = PathEx.Combine(Resources.AppPath);
        private int _countdown = 10;

        public DownloadForm() => 
            InitializeComponent();

        private void DownloadForm_Load(object sender, EventArgs e)
        {
            try
            {
                string source;
                if (!NetEx.InternetIsAvailable() || string.IsNullOrWhiteSpace(source = NetEx.Transfer.DownloadString($"{Resources.UpdateUrl}/{Resources.HashFile}")))
                {
                    MessageBoxEx.Show(this, Resources.Msg_Err_00, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Application.Exit();
                    return;
                }
                var file = string.Empty;
                foreach (var str in source.Split(' '))
                {
                    if (string.IsNullOrWhiteSpace(str) || str.ContainsEx(Resources.SearchBlacklist.SplitNewLine()) || !str.ContainsEx(Resources.SearchMatch))
                        continue;
                    file = str;
                }
                if (!file.EndsWithEx(".zip"))
                    file = file.Substring(0, file.Length - new Crypto.Sha1().HashLength).Trim();
                _transfer.DownloadFile($"{Resources.UpdateUrl}/{file}", PathEx.Combine(PathEx.LocalDir, file));
                CheckDownload.Enabled = true;
            }
            catch
            {
                Process.Start(Resources.UpdateUrl);
            }
        }

        private void DownloadForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_transfer.IsBusy)
                _transfer.CancelAsync();
            if (ExtractDownload.IsBusy)
                ExtractDownload.CancelAsync();
            if (!File.Exists(_appPath))
                Application.Exit();
        }

        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            DLSpeed.Text = _transfer.TransferSpeedAd;
            DLPercentage.Value = _transfer.ProgressPercentage;
            DLLoaded.Text = _transfer.DataReceived;
            if (!_transfer.IsBusy)
                _countdown--;
            if (_countdown == 9)
                DLPercentage.JumpToEnd();
            if (_countdown > 0)
                return;
            CheckDownload.Enabled = false;
            ExtractDownload.RunWorkerAsync();
        }

        private void ExtractDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (!File.Exists(_transfer.FilePath))
                    return;
                using (var file = ZipFile.Open(_transfer.FilePath, ZipArchiveMode.Read))
                {
                    var dir = Path.GetDirectoryName(_appPath);
                    if (Directory.Exists(dir))
                        Directory.Delete(dir, true);
                    file.ExtractToDirectory(dir);
                }
                File.Delete(_transfer.FilePath);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                Application.Exit();
            }
        }

        private void ExtractDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;
            MessageBoxEx.Show(this, !_transfer.HasCanceled ? Resources.Msg_Hint_01 : Resources.Msg_Warn_00, Resources.WindowTitle, MessageBoxButtons.OK, !_transfer.HasCanceled ? MessageBoxIcon.Asterisk : MessageBoxIcon.Warning);
            Close();
        }
    }
}
