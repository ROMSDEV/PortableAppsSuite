namespace CCleanerUpdater
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Windows.Forms;
    using SilDev;
    using SilDev.Forms;

    public partial class MainForm : Form
    {
        private readonly NetEx.AsyncTransfer _transfer = new NetEx.AsyncTransfer();
        private int _ldFinishCount;
        private string _zipPath = string.Empty;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);

            var cCleaner = PathEx.Combine("%CurDir%\\CCleaner.exe");
            const string updateUrl = "https://www.piriform.com/ccleaner/download/portable/downloadfile";

            var localVersion = string.Empty;
            try
            {
                var verFilter = FileVersionInfo.GetVersionInfo(cCleaner).FileVersion.Replace(" ", string.Empty).Split(',');
                if (verFilter.Length >= 2)
                    for (var i = 0; i < 2; i++)
                        localVersion += verFilter[i];
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            CheckClose(localVersion, "LocalVersion");

            var fileName = NetEx.GetFileName(updateUrl);
            CheckClose(fileName, "FileName");

            var onlineVersion = string.Concat(fileName.Where(char.IsDigit).ToArray());
            CheckClose(onlineVersion, "OnlineVersion");

            if (Convert.ToInt32(localVersion) < Convert.ToInt32(onlineVersion))
            {
                if (ShowInfoBox("UpdateAvailable", MessageBoxButtons.YesNo) == DialogResult.Yes || Environment.CommandLine.Contains("/silent"))
                {
                    _zipPath = PathEx.Combine("%CurDir%", fileName);
                    if (!File.Exists(_zipPath))
                    {
                        Opacity = 1f;
                        _transfer.DownloadFile(updateUrl, _zipPath);
                        CheckDownload.Enabled = true;
                    }
                    else
                        ExtractDownload.RunWorkerAsync();
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

        private void CheckClose(string check, string arg)
        {
            if (!string.IsNullOrWhiteSpace(check))
                return;
            ShowInfoBox(arg, MessageBoxButtons.OK);
            Application.Exit();
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
                    text = "CCleaner successfully updated.";
                    break;
                case "UpdateFailed":
                    text = "CCleaner update failed.";
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
            if (!_transfer.IsBusy)
                _ldFinishCount++;
            if (_ldFinishCount == 1)
                DLPercentage.JumpToEnd();
            TaskBar.Progress.SetValue(Handle, DLPercentage.Value, DLPercentage.Maximum);
            if (_ldFinishCount < 10)
                return;
            CheckDownload.Enabled = false;
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
                        var entDir = Path.GetDirectoryName(entPath);
                        if (entDir != null && !Directory.Exists(entDir))
                            Directory.CreateDirectory(entDir);
                        ent.ExtractToFile(entPath, true);
                    }
                e.Result = "Updated";
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                e.Result = "UpdateFailed";
            }
        }

        private void ExtractDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);
            ShowInfoBox(e.Result.ToString(), MessageBoxButtons.OK);
            Close();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (File.Exists(_zipPath))
                ProcessEx.Send($"PING 127.0.0.1 -n 2 & DEL /F /Q \"{_zipPath}\"");
        }
    }
}
