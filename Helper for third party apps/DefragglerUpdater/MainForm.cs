using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace DefragglerUpdater
{
    public partial class MainForm : Form
    {
        SilDev.Network.AsyncTransfer Transfer = new SilDev.Network.AsyncTransfer();
        int DownloadFinishedCount = 0;
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

                string OnlineVersion = string.Concat(FileName.Where(c => char.IsDigit(c)).ToArray());
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
                            Transfer.DownloadFile(UpdateURL, ZipPath);
                            CheckDownload.Enabled = true;
                            if (Environment.CommandLine.Contains("/silent"))
                            {
                                Opacity = 0;
                                ShowInTaskbar = false;
                            }
                        }
                        else
                        {
                            ExtractDownload.RunWorkerAsync();
                            Opacity = 0;
                            ShowInTaskbar = false;
                        }
                    }
                    else
                        Close();
                }
                else
                    throw new Exception();
            }
            catch
            {
                ShowInfoBox("NoUpdates", MessageBoxButtons.OK);
                Close();
            }
        }

        private void CheckClose(string _check, string _arg)
        {
            if (string.IsNullOrWhiteSpace(_check))
            {
                ShowInfoBox(_arg, MessageBoxButtons.OK);
                Close();
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
                            string EntPath = Path.Combine(Application.StartupPath, ent.FullName);
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
                SilDev.Log.Debug(ex);
            }
        }

        private void ExtractDownload_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Opacity = 0;
            ShowInTaskbar = false;
            ShowInfoBox(e.Result.ToString(), MessageBoxButtons.OK);
            Close();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (File.Exists(ZipPath))
                SilDev.Run.Cmd($"PING 127.0.0.1 -n 2 & DEL /F /Q \"{ZipPath}\"");
        }
    }
}
