using SilDev;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace CCleanerUpdater
{
    public partial class MainForm : Form
    {
        NET.AsyncTransfer Transfer = new NET.AsyncTransfer();
        int DownloadFinishedCount = 0;
        string ZipPath = string.Empty;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            TASKBAR.Progress.SetState(Handle, TASKBAR.Progress.States.Indeterminate);

            string CCleaner = PATH.Combine("%CurDir%\\CCleaner.exe");
            string UpdateURL = "https://www.piriform.com/ccleaner/download/portable/downloadfile";

            string LocalVersion = string.Empty;
            try
            {
                string[] VerFilter = FileVersionInfo.GetVersionInfo(CCleaner).FileVersion.Replace(" ", string.Empty).Split(',');
                if (VerFilter.Length >= 2)
                    for (int i = 0; i < 2; i++)
                        LocalVersion += VerFilter[i];
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
            CheckClose(LocalVersion, "LocalVersion");

            string FileName = NET.GetFileName(UpdateURL);
            CheckClose(FileName, "FileName");

            string OnlineVersion = string.Concat(FileName.Where(c => char.IsDigit(c)).ToArray());
            CheckClose(OnlineVersion, "OnlineVersion");

            if (Convert.ToInt32(LocalVersion) < Convert.ToInt32(OnlineVersion))
            {
                if (ShowInfoBox("UpdateAvailable", MessageBoxButtons.YesNo) == DialogResult.Yes || Environment.CommandLine.Contains("/silent"))
                {
                    ZipPath = PATH.Combine("%CurDir%", FileName);
                    if (!File.Exists(ZipPath))
                    {
                        Opacity = 1f;
                        Transfer.DownloadFile(UpdateURL, ZipPath);
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
                    text = "CCleaner successfully updated.";
                    break;
                case "UpdateFailed":
                    text = "CCleaner update failed.";
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
            TASKBAR.Progress.SetValue(Handle, DLPercentage.Value, DLPercentage.Maximum);
            if (DownloadFinishedCount >= 10)
            {
                CheckDownload.Enabled = false;
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
                            string EntDir = Path.GetDirectoryName(EntPath);
                            if (!Directory.Exists(EntDir))
                                Directory.CreateDirectory(EntDir);
                            ent.ExtractToFile(EntPath, true);
                        }
                    }
                    e.Result = "Updated";
                }
            }
            catch (Exception ex)
            {
                e.Result = "UpdateFailed";
                LOG.Debug(ex);
            }
        }

        private void ExtractDownload_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            TASKBAR.Progress.SetState(Handle, TASKBAR.Progress.States.Indeterminate);
            ShowInfoBox(e.Result.ToString(), MessageBoxButtons.OK);
            Close();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (File.Exists(ZipPath))
                RUN.Cmd($"/C PING 127.0.0.1 -n 2 & DEL /F /Q \"{ZipPath}\"");
        }
    }
}
