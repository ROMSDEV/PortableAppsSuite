namespace RunPHP
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Windows.Forms;
    using SilDev;
    using SilDev.Forms;

    public partial class DownloadForm : Form
    {
        private readonly NetEx.AsyncTransfer _transfer = new NetEx.AsyncTransfer();
        private int _dlFinishCount;
        private string _phpPath = string.Empty;

        public DownloadForm()
        {
            InitializeComponent();
        }

        private void DownloadForm_Load(object sender, EventArgs e)
        {
            _phpPath = PathEx.Combine("%CurDir%\\php\\php.exe");
            Download();
            CheckDownload.Enabled = true;
        }

        private void DownloadForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_transfer.IsBusy)
                _transfer.CancelAsync();
            if (ExtractDownload.IsBusy)
                ExtractDownload.CancelAsync();
            if (!File.Exists(_phpPath))
                Application.Exit();
        }

        private void Download()
        {
            try
            {
                var source = NetEx.Transfer.DownloadString("http://windows.php.net/downloads/releases/sha1sum.txt");
                if (string.IsNullOrWhiteSpace(source))
                {
                    MessageBoxEx.Show("Sorry, no connection available.", "No Connection Available", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Application.Exit();
                    return;
                }
                var archive = string.Empty;
                foreach (var str in source.Split(' '))
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        var tmp = str.ToLower();
                        if (!tmp.Contains("test") && !tmp.Contains("debug") && !tmp.Contains("devel") && !tmp.Contains("nts") && tmp.Contains("-x86.zip"))
                            archive = str;
                    }
                if (!archive.EndsWith(".zip"))
                    archive = archive.Substring(0, archive.Length - 40).Trim();
                _transfer.DownloadFile($"http://windows.php.net/downloads/releases/{archive}", PathEx.Combine("%CurDir%", archive));
            }
            catch
            {
                Process.Start("http://windows.php.net/downloads/releases");
            }
        }

        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            DLSpeed.Text = $@"{(int)Math.Round(_transfer.TransferSpeed)} kb/s";
            DLPercentage.Value = _transfer.ProgressPercentage;
            DLLoaded.Text = _transfer.DataReceived;
            if (!_transfer.IsBusy)
                _dlFinishCount++;
            if (_dlFinishCount == 1)
                DLPercentage.JumpToEnd();
            if (_dlFinishCount < 10)
                return;
            CheckDownload.Enabled = false;
            ExtractDownload.RunWorkerAsync();
        }

        private void ExtractDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var path = PathEx.Combine("%CurDir%\\php");
                if (!File.Exists(_transfer.FilePath))
                    return;
                using (var zip = ZipFile.Open(_transfer.FilePath, ZipArchiveMode.Read))
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);
                    zip.ExtractToDirectory(path);
                }
                File.Delete(_transfer.FilePath);
            }
            catch
            {
                Application.Exit();
            }
        }

        private void ExtractDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;
            MessageBoxEx.Show(this, !_transfer.HasCanceled ? "Operation completed!" : "Operation failed!", "Info", MessageBoxButtons.OK, MessageBoxIcon.None);
            Close();
        }
    }
}
