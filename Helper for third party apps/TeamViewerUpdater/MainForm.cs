namespace TeamViewerUpdater
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.IO.Compression;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using SilDev.Forms;

    public partial class MainForm : Form
    {
        private readonly string _teamViewer = PathEx.Combine("%CurDir%\\TeamViewer.exe");
        private readonly NetEx.AsyncTransfer _transfer = new NetEx.AsyncTransfer();
        private int _dlFinishCount;
        private string _zipPath = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            Icon = Resources.TeamViewerUpdater;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            const string updateUrl = "http://download.teamviewer.com/download/TeamViewerPortable.zip";
            const string fileName = "TeamViewerPortable.zip";
            var onlineDate = NetEx.GetFileDate(updateUrl);
            var localDate = File.GetLastWriteTime(_teamViewer);
            if (onlineDate.Date > localDate.Date)
            {
                if (ShowInfoBox("UpdateAvailable", MessageBoxButtons.YesNo) == DialogResult.Yes || Environment.CommandLine.Contains("/silent"))
                {
                    _zipPath = PathEx.Combine("%CurDir%", fileName);
                    if (!File.Exists(_zipPath))
                    {
                        _transfer.DownloadFile(updateUrl, _zipPath);
                        CheckDownload.Enabled = true;
                        Opacity = 1f;
                    }
                    else
                    {
                        ExtractDownload.RunWorkerAsync();
                        Opacity = 0;
                    }
                }
                else
                    Close();
            }
            else
            {
                ShowInfoBox("NoUpdates", MessageBoxButtons.OK);
                Application.Exit();
            }
        }

        private DialogResult ShowInfoBox(string arg, MessageBoxButtons btn)
        {
            if (Environment.CommandLine.Contains("/silent"))
                return DialogResult.No;
            string text;
            switch (arg)
            {
                case "UpdateAvailable":
                    text = "A newer version is available. Would you like to update now?";
                    break;
                case "Updated":
                    text = "TeamViewer successfully updated.";
                    break;
                case "UpdateFailed":
                    text = "TeamViewer update failed.";
                    break;
                default:
                    text = "No newer version available.";
                    break;
            }
            return MessageBox.Show(text, Text, btn, MessageBoxIcon.Information);
        }

        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            DLSpeed.Text = $@"{(int)Math.Round(_transfer.TransferSpeed)} kb/s";
            DLPercentage.Value = _transfer.ProgressPercentage;
            DLLoaded.Text = _transfer.DataReceived;
            if (DLPercentage.Value > 0 && WindowState != FormWindowState.Normal)
                WindowState = FormWindowState.Normal;
            if (!_transfer.IsBusy)
                _dlFinishCount++;
            if (_dlFinishCount == 1)
                DLPercentage.JumpToEnd();
            if (_dlFinishCount < 10)
                return;
            CheckDownload.Enabled = false;
            if (!ExtractDownload.IsBusy)
                ExtractDownload.RunWorkerAsync();
        }

        private void ExtractDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (!File.Exists(_zipPath))
                    return;
                using (var zip = ZipFile.OpenRead(_zipPath))
                    foreach (var ent in zip.Entries)
                    {
                        var entPath = PathEx.Combine("%CurDir%", ent.FullName);
                        if (File.Exists(entPath))
                            File.Delete(entPath);
                        ent.ExtractToFile(entPath, true);
                    }
                File.SetLastWriteTime(_teamViewer, DateTime.Now);
                e.Result = "Updated";
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                try
                {
                    if (File.Exists(_zipPath))
                        File.Delete(_zipPath);
                }
                catch (Exception exx)
                {
                    Log.Write(exx);
                }
                e.Result = "UpdateFailed";
            }
        }

        private void ExtractDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Opacity = 0;
            ShowInTaskbar = false;
            ShowInfoBox(e.Result?.ToString() ?? "UpdateFailed", MessageBoxButtons.OK);
            Close();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (File.Exists(_zipPath))
                ProcessEx.Send($"PING 127.0.0.1 -n 2 & DEL /F /Q \"{_zipPath}\"");
        }
    }
}
