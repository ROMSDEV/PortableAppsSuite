using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WinRARUpdater
{
    public partial class MainForm : Form
    {
        int count = 0;
        string SetupPath = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.RAR;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string IniFile = Path.Combine(Application.StartupPath, string.Format("{0}.ini", Path.GetFileNameWithoutExtension(Application.ExecutablePath)));
            if (!File.Exists(IniFile))
            {
                SilDev.Ini.File(IniFile);
                SilDev.Ini.Write("Settings", "Language", "English");
                SilDev.Ini.Write("Settings", "Architecture", Environment.Is64BitProcess ? "64 bit" : "32 bit");
                SilDev.Ini.Write("Settings", "DoNotAskAgain", false);
            }
            else
                SilDev.Ini.File(IniFile);

            bool DoNotAskAgain = false;
            if (!bool.TryParse(SilDev.Ini.Read("Settings", "DoNotAskAgain"), out DoNotAskAgain) || !DoNotAskAgain)
            {
                Form LangSelection = new LangSelectionForm();
                if (LangSelection.ShowDialog() != DialogResult.OK)
                {
                    ShowInfoBox("Canceled", MessageBoxButtons.OK);
                    Application.Exit();
                    return;
                }
            }

            string WinRAR = Path.Combine(Application.StartupPath, "WinRAR.exe");
            string UpdateURL = "http://www.rarsoft.com/download.htm";

            string LocalVersion = "0";
            if (File.Exists(WinRAR))
            {
                string[] VerFilter = FileVersionInfo.GetVersionInfo(WinRAR).FileVersion.Replace(" ", string.Empty).Split(',');
                if (VerFilter.Length >= 1)
                    for (int i = 0; i < VerFilter.Length; i++)
                        LocalVersion += VerFilter[i];
            }
            CheckClose(LocalVersion, "LocalVersion");

            string HtmContent = SilDev.Network.DownloadString(UpdateURL);
            CheckClose(HtmContent, "OnlineVersion");
            
            Dictionary<string, string[]> OnlineInfo = new Dictionary<string, string[]>();
            foreach (Match match in Regex.Matches(HtmContent, "<tr>(.+?)</tr>", RegexOptions.Singleline))
            {
                string Version = Regex.Match(match.Groups[1].ToString(), "<td align=\"center\">(.+?)</td>", RegexOptions.Singleline).Groups[1].ToString();
                string Language = Regex.Match(match.Groups[1].ToString(), "<b>(.+?)</b></a></td>", RegexOptions.Singleline).Groups[1].ToString();
                string Location = Regex.Match(match.Groups[1].ToString(), "<td><a href=\"/rar/(.+?)\">", RegexOptions.Singleline).Groups[1].ToString();
                if (string.IsNullOrWhiteSpace(Version) || string.IsNullOrWhiteSpace(Language) || string.IsNullOrWhiteSpace(Location) || !string.IsNullOrWhiteSpace(Version) && (Version.ToLower().Contains("trial") || Version.ToLower().Contains("free") || Version.ToLower().Contains("beta")))
                    continue;
                if (OnlineInfo.ContainsKey(Language))
                {
                    foreach (var item in OnlineInfo)
                    {
                        if (item.Key == Language)
                        {
                            SilDev.Log.Debug(string.Format("Key: {0} - Value[0]: {1} - Value[1]: {2}", item.Key, item.Value[0], item.Value[1]), "OnlineInfo");
                            break;
                        }
                    }
                    continue;
                }
                OnlineInfo.Add(Language, new string[] { Version, Location });
            }
            if (OnlineInfo.Count <= 0)
                CheckClose(string.Empty, "OnlineVersion");

            string OnlineVersion = string.Empty;
            string FileName = string.Empty;
            foreach (var item in OnlineInfo)
            {
                string Lang = SilDev.Ini.Read("Settings", "Language");
                if (string.IsNullOrWhiteSpace(Lang))
                    Lang = "English";
                string Bits = SilDev.Ini.Read("Settings", "Architecture");
                if (string.IsNullOrWhiteSpace(Lang))
                    Bits = Environment.Is64BitProcess ? "64 bit" : "32 bit";
                if (item.Key.ToLower() == $"{Lang.ToLower()} ({Bits.ToLower()})")
                {
                    OnlineVersion = $"{item.Value[0]}.0";
                    FileName = item.Value[1];
                    break;
                }
            }
            CheckClose(FileName, "FileName");

            int LocalVer = 0;
            int.TryParse(LocalVersion.Replace(".", string.Empty), out LocalVer);

            int OnlineVer = 0;
            int.TryParse(OnlineVersion.Replace(".", string.Empty), out OnlineVer);

            if (LocalVer < OnlineVer)
            {
                if (!File.Exists(WinRAR) || Environment.CommandLine.Contains("/silent") || ShowInfoBox("UpdateAvailable", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SetupPath = Path.Combine(Application.StartupPath, FileName);
                    if (!File.Exists(SetupPath))
                    {
                        Opacity = 1d;
                        ShowInTaskbar = true;
                        WindowState = FormWindowState.Normal;
                        SilDev.Network.DownloadFileAsync(string.Format("http://www.rarsoft.com/rar/{0}", FileName), SetupPath);
                        CheckDownload.Enabled = true;
                    }
                }
                else
                    Application.Exit();
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
                    text = "WinRAR successfully updated.";
                    break;
                case "UpdateFailed":
                    text = "WinRAR update failed.";
                    break;
                case "Canceled":
                    text = "Canceled.";
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
                if (File.Exists(SetupPath))
                {
                    string UnRAR = SilDev.Source.GetFilePath("UnRAR.exe");
                    if (!File.Exists(UnRAR))
                        throw new Exception("UnRAR.exe not exists");
                    string ContentPath = Path.Combine(Application.StartupPath, "Update");
                    if (!Directory.Exists(ContentPath))
                        Directory.CreateDirectory(ContentPath);
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = $"x -u \"{SetupPath}\" \"{ContentPath}\"",
                        FileName = UnRAR,
                        WindowStyle = ProcessWindowStyle.Minimized
                    }, 0);
                    string[] FileList = Directory.GetFiles(ContentPath, "*", SearchOption.TopDirectoryOnly);
                    if (FileList.Length <= 0)
                        throw new Exception("No files found to update");
                    foreach (string file in Directory.GetFiles(Application.StartupPath, "*.lng", SearchOption.TopDirectoryOnly))
                        File.Delete(file);
                    foreach (string file in FileList)
                    {
                        string dest = file.Replace(ContentPath, Application.StartupPath);
                        if (File.Exists(dest))
                            File.Delete(dest);
                        File.Move(file, file.Replace(ContentPath, Application.StartupPath));
                    }
                    if (Directory.Exists(ContentPath))
                        Directory.Delete(ContentPath, true);
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
            Application.Exit();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (File.Exists(SetupPath))
                SilDev.Run.Cmd($"PING 127.0.0.1 -n 2 & DEL /F /Q \"{SetupPath}\"");
        }
    }
}
