using SilDev;
using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace TeamViewerUpdater
{
    public partial class MainForm : Form
    {
        NET.AsyncTransfer Transfer = new NET.AsyncTransfer();
        int DownloadFinishedCount = 0;
        string ZipPath = string.Empty;
        string TeamViewer = PATH.Combine("%CurDir%\\TeamViewer.exe");

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.TeamViewerUpdater;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            string UpdateURL = "http://download.teamviewer.com/download/TeamViewerPortable.zip";
            string FileName = "TeamViewerPortable.zip";
            DateTime onlineDate = NET.GetFileDate(UpdateURL);
            DateTime localDate = File.GetLastWriteTime(TeamViewer);
            if (onlineDate.Date > localDate.Date)
            {
                if (ShowInfoBox("UpdateAvailable", MessageBoxButtons.YesNo) == DialogResult.Yes || Environment.CommandLine.Contains("/silent"))
                {
                    ZipPath = PATH.Combine("%CurDir%", FileName);
                    if (!File.Exists(ZipPath))
                    {
                        Transfer.DownloadFile(UpdateURL, ZipPath);
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
            DLSpeed.Text = $"{(int)Math.Round(Transfer.TransferSpeed)} kb/s";
            DLPercentage.Value = Transfer.ProgressPercentage;
            DLLoaded.Text = Transfer.DataReceived;
            if (DLPercentage.Value > 0 && WindowState != FormWindowState.Normal)
                WindowState = FormWindowState.Normal;
            if (!Transfer.IsBusy)
                DownloadFinishedCount++;
            if (DownloadFinishedCount == 1)
            {
                DLPercentage.Maximum = 1000;
                DLPercentage.Value = 1000;
                DLPercentage.Value--;
                DLPercentage.Maximum = 100;
                DLPercentage.Value = 100;
            }
            if (DownloadFinishedCount >= 10)
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
                            string EntPath = PATH.Combine("%CurDir%", ent.FullName);
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
                    LOG.Debug(exx);
                }
                e.Result = "UpdateFailed";
                LOG.Debug(ex);
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
                RUN.Cmd($"PING 127.0.0.1 -n 2 & DEL /F /Q \"{ZipPath}\"");
        }
    }
}
