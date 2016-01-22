using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace RunPHP
{
    public partial class DownloadForm : Form
    {
        string php = string.Empty;
        int count = 0;

        public DownloadForm()
        {
            InitializeComponent();
        }

        private void DownloadForm_Load(object sender, EventArgs e)
        {
            php = Path.Combine(Application.StartupPath, "php\\php.exe");
            Download();
            CheckDownload.Enabled = true;
        }

        private void DownloadForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (SilDev.Network.AsyncDownloadIsBusy())
                SilDev.Network.CancelAsyncDownload();
            if (ExtractDownload.IsBusy)
                ExtractDownload.CancelAsync();
            if (!File.Exists(php))
                Environment.Exit(-1);
        }

        private void Download()
        {
            try
            {
                string source = SilDev.Network.DownloadString("http://windows.php.net/downloads/releases/sha1sum.txt");
                if (string.IsNullOrWhiteSpace(source))
                {
                    SilDev.MsgBox.Show("Sorry, no connection available.", "No Connection Available", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Environment.Exit(1);
                }
                string archive = string.Empty;
                foreach (string str in source.Split(' '))
                {
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        string tmp = str.ToLower();
                        if (!tmp.Contains("test") && !tmp.Contains("debug") && !tmp.Contains("devel") && !tmp.Contains("nts") && tmp.Contains("-x86.zip"))
                            archive = str;
                    }
                }
                if (!archive.EndsWith(".zip"))
                    archive = archive.Substring(0, archive.Length - 40).Trim();
                SilDev.Network.DownloadFileAsync(string.Format("http://windows.php.net/downloads/releases/{0}", archive), Path.Combine(Application.StartupPath, archive));
            }
            catch
            {
                Process.Start("http://windows.php.net/downloads/releases");
            }
        }

        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            DLSpeed.Text = SilDev.Network.LatestAsyncDownloadInfo.TransferSpeed;
            DLPercentage.Value = SilDev.Network.LatestAsyncDownloadInfo.ProgressPercentage;
            DLLoaded.Text = SilDev.Network.LatestAsyncDownloadInfo.DataReceived;
            if (!SilDev.Network.AsyncDownloadIsBusy())
                count++;
            if (count == 1)
            {
                DLPercentage.Maximum = 1000;
                DLPercentage.Value = 1000;
                DLPercentage.Value--;
                DLPercentage.Maximum = 100;
                DLPercentage.Value = 100;
            }
            if (count >= 10)
            {
                CheckDownload.Enabled = false;
                ExtractDownload.RunWorkerAsync();
            }
        }

        private void ExtractDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "php");
                if (File.Exists(SilDev.Network.LatestAsyncDownloadInfo.FilePath))
                {
                    using (ZipArchive zip = ZipFile.Open(SilDev.Network.LatestAsyncDownloadInfo.FilePath, ZipArchiveMode.Read))
                    {
                        if (Directory.Exists(path))
                            Directory.Delete(path, true);
                        zip.ExtractToDirectory(path);
                        zip.Dispose();
                    }
                    File.Delete(SilDev.Network.LatestAsyncDownloadInfo.FilePath);
                }
            }
            catch
            {
                Environment.Exit(-1);
            }
        }

        private void ExtractDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (!string.IsNullOrWhiteSpace(SilDev.Network.LatestAsyncDownloadInfo.StatusMessage))
                  SilDev.MsgBox.Show(this, SilDev.Network.LatestAsyncDownloadInfo.StatusMessage, "Info", MessageBoxButtons.OK, MessageBoxIcon.None);
                Close();
            }
        }
    }
}
