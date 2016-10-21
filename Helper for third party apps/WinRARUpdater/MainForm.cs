namespace WinRARUpdater
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using SilDev.Forms;

    public partial class MainForm : Form
    {
        private static readonly string UnRarExe = PathEx.Combine("%TEMP%", Process.GetCurrentProcess().ProcessName, "UnRAR.exe");
        private readonly NetEx.AsyncTransfer _transfer = new NetEx.AsyncTransfer();

        private readonly List<string> _whiteList = new List<string>
        {
            "archivecomment.txt",
            "rarreg.key",
            "winrar.ini",
            "winrarupdater64.exe",
            "winrarupdater64.ini",
            "winrarupdater.exe",
            "winrarupdater.ini"
        };

        private int _dlFinishCount;
        private string _setupPath = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            Icon = Resources.RAR;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var iniFile = PathEx.Combine($"%CurDir%\\{Path.GetFileNameWithoutExtension(PathEx.LocalPath)}.ini");
            if (!File.Exists(iniFile))
            {
                Ini.File(iniFile);
                Ini.Write("Settings", "Language", "English");
                Ini.Write("Settings", "Architecture", Environment.Is64BitProcess ? "64 bit" : "32 bit");
                Ini.Write("Settings", "DoNotAskAgain", false);
            }
            else
                Ini.File(iniFile);

            bool doNotAskAgain;
            if (!bool.TryParse(Ini.Read("Settings", "DoNotAskAgain"), out doNotAskAgain) || !doNotAskAgain)
            {
                Form langSelection = new LangSelectionForm();
                if (langSelection.ShowDialog() != DialogResult.OK)
                {
                    ShowInfoBox("Canceled", MessageBoxButtons.OK);
                    Application.Exit();
                    return;
                }
            }

            try
            {
                var winRar = PathEx.Combine("%CurDir%\\WinRAR.exe");
                const string updateUrl = "http://www.rarsoft.com/download.htm";

                var localVersion = "0";
                if (File.Exists(winRar))
                {
                    var verFilter = FileVersionInfo.GetVersionInfo(winRar).FileVersion.Replace(" ", string.Empty).Split(',');
                    if (verFilter.Length >= 1)
                        localVersion = verFilter.Aggregate(localVersion, (c, t) => c + t);
                }
                CheckClose(localVersion, "LocalVersion");

                var htmContent = NetEx.Transfer.DownloadString(updateUrl);
                CheckClose(htmContent, "OnlineVersion");

                var onlineInfo = new Dictionary<string, string[]>();
                foreach (Match match in Regex.Matches(htmContent, "<tr>(.+?)</tr>", RegexOptions.Singleline))
                {
                    var version = Regex.Match(match.Groups[1].ToString(), "<td align=\"center\">(.+?)</td>", RegexOptions.Singleline).Groups[1].ToString();
                    var language = Regex.Match(match.Groups[1].ToString(), "<b>(.+?)</b></a></td>", RegexOptions.Singleline).Groups[1].ToString();
                    var location = Regex.Match(match.Groups[1].ToString(), "<td><a href=\"/rar/(.+?)\">", RegexOptions.Singleline).Groups[1].ToString();
                    if (string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(language) || string.IsNullOrWhiteSpace(location) || !string.IsNullOrWhiteSpace(version) && (version.ToLower().Contains("trial") || version.ToLower().Contains("free") || version.ToLower().Contains("beta")))
                        continue;
                    if (onlineInfo.ContainsKey(language))
                    {
                        foreach (var item in onlineInfo)
                        {
                            if (item.Key != language)
                                continue;
                            Log.Write($"Key: {item.Key} - Value[0]: {item.Value[0]} - Value[1]: {item.Value[1]}");
                            break;
                        }
                        continue;
                    }
                    onlineInfo.Add(language, new[] { version, location });
                }
                if (onlineInfo.Count <= 0)
                    CheckClose(string.Empty, "OnlineVersion");

                var onlineVersion = string.Empty;
                var fileName = string.Empty;
                foreach (var item in onlineInfo)
                {
                    var lang = Ini.Read("Settings", "Language");
                    if (string.IsNullOrWhiteSpace(lang))
                        lang = "English";
                    var bits = Ini.Read("Settings", "Architecture");
                    if (string.IsNullOrWhiteSpace(lang))
                        bits = Environment.Is64BitProcess ? "64 bit" : "32 bit";
                    if (item.Key.ToLower() != $"{lang.ToLower()} ({bits.ToLower()})")
                        continue;
                    onlineVersion = $"{item.Value[0]}.0";
                    fileName = item.Value[1];
                    break;
                }
                CheckClose(fileName, "FileName");

                int localVer;
                int.TryParse(localVersion.Replace(".", string.Empty), out localVer);

                int onlineVer;
                int.TryParse(onlineVersion.Replace(".", string.Empty), out onlineVer);

                if (localVer < onlineVer)
                {
                    if (!File.Exists(winRar) || Environment.CommandLine.ContainsEx("/silent") || ShowInfoBox("UpdateAvailable", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        _setupPath = PathEx.Combine("%CurDir%", fileName);
                        if (File.Exists(_setupPath))
                            try
                            {
                                File.Delete(_setupPath);
                            }
                            catch
                            {
                                ProcessEx.Send($"DEL /F /Q \"{_setupPath}\"", true);
                            }
                        if (!File.Exists(_setupPath))
                        {
                            Opacity = 1d;
                            ShowInTaskbar = true;
                            WindowState = FormWindowState.Normal;
                            _transfer.DownloadFile($"http://www.rarsoft.com/rar/{fileName}", _setupPath);
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
            if (!string.IsNullOrWhiteSpace(item))
                return;
            ShowInfoBox(arg, MessageBoxButtons.OK);
            Application.Exit();
        }

        private DialogResult ShowInfoBox(string arg, MessageBoxButtons btn)
        {
            if (Environment.CommandLine.Contains("/silent"))
                return DialogResult.OK;
            string text;
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
            DLSpeed.Text = $@"{(int)Math.Round(_transfer.TransferSpeed)} kb/s";
            DLPercentage.Value = _transfer.ProgressPercentage;
            DLLoaded.Text = _transfer.DataReceived;
            if (!_transfer.IsBusy)
                _dlFinishCount++;
            if (_dlFinishCount == 1)
                DLPercentage.JumpToEnd();
            if (_dlFinishCount < 10)
                return;
            CheckDownload.Enabled = false;
            ExtractDownload.RunWorkerAsync();
        }

        private void ExtractDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (!File.Exists(_setupPath))
                    return;
                if (!File.Exists(UnRarExe))
                    throw new Exception("UnRAR.exe not exists");
                var updDir = PathEx.Combine("%CurDir%\\Update");
                if (!Directory.Exists(updDir))
                    Directory.CreateDirectory(updDir);
                using (var p = ProcessEx.Start(UnRarExe, $"x -u \"{_setupPath}\" \"{updDir}\"", false, ProcessWindowStyle.Minimized, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
                var updFiles = Directory.GetFiles(updDir, "*", SearchOption.TopDirectoryOnly);
                if (updFiles.Length <= 0)
                    throw new Exception("No files found to update");
                var curFiles = new List<string>();
                curFiles.AddRange(Directory.GetFiles(EnvironmentEx.GetVariableValue("CurDir"), "*", SearchOption.TopDirectoryOnly)
                                           .Where(s => !_whiteList.ContainsEx(Path.GetFileName(s))));
                foreach (var file in curFiles)
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        try
                        {
                            File.Move(file, $"{file}.{PathEx.GetTempDirName()}");
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    }
                foreach (var file in updFiles)
                    try
                    {
                        File.Move(file, file.Replace(updDir, EnvironmentEx.GetVariableValue("CurDir")));
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                if (Directory.Exists(updDir))
                    Directory.Delete(updDir, true);
                e.Result = "Updated";
            }
            catch (Exception ex)
            {
                e.Result = "UpdateFailed";
                Log.Write(ex);
            }
        }

        private void ExtractDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Opacity = 0;
            ShowInTaskbar = false;
            ShowInfoBox(e.Result.ToString(), MessageBoxButtons.OK);
            Application.Exit();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (File.Exists(_setupPath))
                ProcessEx.Send($"PING 127.0.0.1 -n 5 && DEL /F /Q \"{_setupPath}\" &&  && RD /S /Q \"{Path.GetDirectoryName(UnRarExe)}\"");
        }
    }
}
