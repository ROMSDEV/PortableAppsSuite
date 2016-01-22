using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DefragglerUpdater
{
    public partial class MainForm : Form
    {
        int count = 0;
        string ZipPath = string.Empty;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                string Defraggler = Path.Combine(Application.StartupPath, "Defraggler.exe");
                string UpdateURL = "https://www.piriform.com/defraggler/download/portable/downloadfile";

                string LocalVersion = string.Empty;
                string[] VerFilter = FileVersionInfo.GetVersionInfo(Defraggler).FileVersion.Replace(" ", string.Empty).Split(',');
                if (VerFilter.Length > 0)
                    LocalVersion = VerFilter[0];
                string[] split = LocalVersion.Split('.');
                LocalVersion = string.Empty;
                for (int i = 0; i < 2; i++)
                    LocalVersion += split[i];
                CheckClose(LocalVersion, "LocalVersion");

                string FileName = SilDev.Network.GetOnlineFileName(UpdateURL);
                CheckClose(FileName, "FileName");

                string OnlineVersion = Regex.Match(FileName, "dfsetup(.+?).zip").Groups[1].Value;
                CheckClose(OnlineVersion, "OnlineVersion");

                if (Convert.ToInt32(LocalVersion) < Convert.ToInt32(OnlineVersion))
                {
                    if (ShowInfoBox("UpdateAvailable", MessageBoxButtons.YesNo) == DialogResult.Yes || Environment.CommandLine.Contains("/silent"))
                    {
                        foreach (string d in Directory.GetDirectories(Application.StartupPath, "*", SearchOption.TopDirectoryOnly))
                            Directory.Delete(d, true);
                        foreach (string f in Directory.GetFiles(Application.StartupPath, "*", SearchOption.TopDirectoryOnly))
                        {
                            if (Path.GetFileName(f).ToLower() != Path.GetFileName(Application.ExecutablePath).ToLower() &&
                                Path.GetFileName(f).ToLower() != Path.GetFileName(ZipPath).ToLower() &&
                                Path.GetFileName(f).ToLower() != "defraggler.ini" &&
                                Path.GetFileName(f).ToLower() != "portable.dat")
                                File.Delete(f);
                        }
                        ZipPath = Path.Combine(Application.StartupPath, FileName);
                        if (!File.Exists(ZipPath))
                        {
                            SilDev.Network.DownloadFileAsync(UpdateURL, ZipPath);
                            CheckDownload.Enabled = true;
                            if (Environment.CommandLine.Contains("/silent"))
                            {
                                this.Opacity = 0;
                                this.ShowInTaskbar = false;
                            }
                        }
                        else
                        {
                            ExtractDownload.RunWorkerAsync();
                            this.Opacity = 0;
                            this.ShowInTaskbar = false;
                        }
                    }
                    else
                        this.Close();
                }
                else
                    throw new Exception();
            }
            catch
            {
                ShowInfoBox("NoUpdates", MessageBoxButtons.OK);
                this.Close();
            }
        }

        private void CheckClose(string _check, string _arg)
        {
            if (string.IsNullOrWhiteSpace(_check))
            {
                ShowInfoBox(_arg, MessageBoxButtons.OK);
                this.Close();
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
                    text = "Defraggler successfully updated.";
                    break;
                case "UpdateFailed":
                    text = "Defraggler update failed.";
                    break;
                default:
                    text = "No newer version available.";
                    break;
            }
            return MessageBox.Show(text, this.Text, _btn, MessageBoxIcon.Information);
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
                    e.Result = "Updated";
                }
            }
            catch (Exception ex)
            {
                e.Result = "UpdateFailed";
                SilDev.Log.Debug(ex.Message, "MainForm_Load - ZipArchive");
            }
        }

        private void ExtractDownload_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            ShowInfoBox(e.Result.ToString(), MessageBoxButtons.OK);
            this.Close();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (File.Exists(ZipPath))
                SilDev.Run.App(@"%WinDir%\System32", "cmd.exe", string.Format("/C PING 127.0.0.1 -n 2 & DEL /F /Q \"{0}\"", ZipPath), SilDev.Run.WindowStyle.Hidden);
        }
    }
}
