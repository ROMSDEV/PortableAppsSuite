using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WinRARUpdater
{
    public partial class MainForm : Form
    {
        SilDev.Network.AsyncTransfer Transfer = new SilDev.Network.AsyncTransfer();
        int DownloadFinishedCount = 0;
        string SetupPath = string.Empty;

        List<string> WhiteList = new List<string>()
        {
            "archivecomment.txt",
            "rarreg.key",
            "winrar.ini",
            "winrarupdater64.exe",
            "winrarupdater64.ini",
            "winrarupdater.exe",
            "winrarupdater.ini"
        };

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.RAR;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string IniFile = Path.Combine(Application.StartupPath, $"{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}.ini");
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

            try
            {
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
                                SilDev.Log.Debug($"Key: {item.Key} - Value[0]: {item.Value[0]} - Value[1]: {item.Value[1]}");
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
                    if (!File.Exists(WinRAR) || Environment.CommandLine.ToLower().Contains("/silent") || ShowInfoBox("UpdateAvailable", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        SetupPath = Path.Combine(Application.StartupPath, FileName);
                        if (File.Exists(SetupPath))
                        {
                            try
                            {
                                File.Delete(SetupPath);
                            }
                            catch
                            {
                                SilDev.Run.Cmd($"DEL /F /Q \"{SetupPath}\"", true);
                            }
                        }
                        if (!File.Exists(SetupPath))
                        {
                            Opacity = 1d;
                            ShowInTaskbar = true;
                            WindowState = FormWindowState.Normal;
                            Transfer.DownloadFile(string.Format("http://www.rarsoft.com/rar/{0}", FileName), SetupPath);
                            CheckDownload.Enabled = true;
                            return;
                        }
                    }
                }
                else
                    ShowInfoBox("NoUpdates", MessageBoxButtons.OK);
            }
            catch
            {
                ShowInfoBox("UpdateFailed", MessageBoxButtons.OK);
            }
            Application.Exit();
        }

        private void CheckClose(string item, string arg)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                ShowInfoBox(arg, MessageBoxButtons.OK);
                Application.Exit();
            }
        }

        private DialogResult ShowInfoBox(string arg, MessageBoxButtons btn)
        {
            if (Environment.CommandLine.Contains("/silent"))
                return DialogResult.OK;
            string text = string.Empty;
            switch (arg)
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
            return MessageBox.Show(text, Text, btn, MessageBoxIcon.Information);
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
                if (File.Exists(SetupPath))
                {
                    string UnRAR = SilDev.Source.TempAssembliesFilePath("UnRAR.exe");
                    if (!File.Exists(UnRAR))
                        throw new Exception("UnRAR.exe not exists");
                    string TempDir = Path.Combine(Application.StartupPath, "Update");
                    if (!Directory.Exists(TempDir))
                        Directory.CreateDirectory(TempDir);
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = $"x -u \"{SetupPath}\" \"{TempDir}\"",
                        FileName = UnRAR,
                        WindowStyle = ProcessWindowStyle.Minimized
                    }, 0);
                    string[] updFiles = Directory.GetFiles(TempDir, "*", SearchOption.TopDirectoryOnly);
                    if (updFiles.Length <= 0)
                        throw new Exception("No files found to update");
                    List<string> curFiles = new List<string>();
                    curFiles.AddRange(Directory.GetFiles(Application.StartupPath, "*", SearchOption.TopDirectoryOnly).Where(s => !WhiteList.Contains(Path.GetFileName(s).ToLower())));
                    foreach (string file in curFiles)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            try
                            {
                                File.Move(file, $"{file}.{SilDev.Crypt.MD5.EncryptString(Path.GetRandomFileName())}");
                            }
                            catch (Exception ex)
                            {
                                SilDev.Log.Debug(ex);
                            }
                        }
                    }
                    foreach (string file in updFiles)
                    {
                        try
                        {
                            File.Move(file, file.Replace(TempDir, Application.StartupPath));
                        }
                        catch (Exception ex)
                        {
                            SilDev.Log.Debug(ex);
                        }
                    }
                    if (Directory.Exists(TempDir))
                        Directory.Delete(TempDir, true);
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
                SilDev.Run.Cmd($"PING 127.0.0.1 -n 2 && DEL /F /Q \"{SetupPath}\"");
        }
    }
}
