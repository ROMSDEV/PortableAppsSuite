using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace TeamViewerUpdater
{
    public partial class MainForm : Form
    {
        int count = 0;
        string ZipPath = string.Empty;
        string TeamViewer = Path.Combine(Application.StartupPath, "TeamViewer.exe");

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.TeamViewerUpdater;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            string UpdateURL = "http://download.teamviewer.com/download/TeamViewerPortable.zip";
            string FileName = "TeamViewerPortable.zip";
            DateTime onlineDate = SilDev.Network.GetOnlineFileDate(UpdateURL);
            DateTime localDate = File.GetLastWriteTime(TeamViewer);
            if (onlineDate.Date > localDate.Date)
            {
                if (ShowInfoBox("UpdateAvailable", MessageBoxButtons.YesNo) == DialogResult.Yes || Environment.CommandLine.Contains("/silent"))
                {
                    ZipPath = Path.Combine(Application.StartupPath, FileName);
                    if (!File.Exists(ZipPath))
                    {
                        SilDev.Network.DownloadFileAsync(UpdateURL, ZipPath);
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

        private void CheckClose(string _check, string _arg)
        {
            if (string.IsNullOrWhiteSpace(_check))
            {
                ShowInfoBox(_arg, MessageBoxButtons.OK);
                Application.Exit();
            }
        }

        private DialogResult ShowInfoBox(string _arg, MessageBoxButtons _btn)
        {
            if (Environment.CommandLine.Contains("/silent"))
                return DialogResult.No;
            string text = string.Empty;
            switch (_arg)
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
            return MessageBox.Show(text, Text, _btn, MessageBoxIcon.Information);
        }

        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            DLSpeed.Text = SilDev.Network.LatestAsyncDownloadInfo.TransferSpeed;
            DLPercentage.Value = SilDev.Network.LatestAsyncDownloadInfo.ProgressPercentage;
            DLLoaded.Text = SilDev.Network.LatestAsyncDownloadInfo.DataReceived;
            if (DLPercentage.Value > 0 && WindowState != FormWindowState.Normal)
                WindowState = FormWindowState.Normal;
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
                if (!ExtractDownload.IsBusy)
                    ExtractDownload.RunWorkerAsync();
            }
        }

        private void ExtractDownload_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                if (File.Exists(ZipPath))
                {
                    using (ZipArchive zip = ZipFile.OpenRead(ZipPath))
                    {
                        foreach (var ent in zip.Entries)
                        {
                            string EntPath = Path.Combine(Application.StartupPath, ent.FullName);
                            if (File.Exists(EntPath))
                                File.Delete(EntPath);
                            ent.ExtractToFile(EntPath, true);
                        }
                    }
                    File.SetLastWriteTime(TeamViewer, DateTime.Now);
                    e.Result = "Updated";
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (File.Exists(ZipPath))
                        File.Delete(ZipPath);
                }
                catch (Exception exx)
                {
                    SilDev.Log.Debug(exx);
                }
                e.Result = "UpdateFailed";
                SilDev.Log.Debug(ex);
            }
        }

        private void ExtractDownload_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Opacity = 0;
            ShowInTaskbar = false;
            ShowInfoBox(e.Result == null ? "UpdateFailed" : e.Result.ToString(), MessageBoxButtons.OK);
            Close();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (File.Exists(ZipPath))
                SilDev.Run.Cmd($"PING 127.0.0.1 -n 2 & DEL /F /Q \"{ZipPath}\"");
        }
    }
}
