namespace AppUpdater
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
        private readonly NetEx.AsyncTransfer _transfer = new NetEx.AsyncTransfer();
        private readonly bool _silent = Environment.CommandLine.ContainsEx("/silent");
        private readonly string _appPath = PathEx.Combine(Resources.AppPath);
        private int _countdown = 10;
        private string _tmpDir;
        private Version _ver;

        public MainForm()
        {
            InitializeComponent();
            Icon = Resources.VLCUpdater;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Text = Resources.WindowTitle;
            TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);
            if (!NetEx.InternetIsAvailable())
            {
                if (!_silent || !File.Exists(_appPath))
                    MessageBoxEx.Show(Resources.Msg_Err_00, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            string updUrl;
            try
            {
                var mirrors = Resources.UpdateMirrors.SplitNewLine();
                var mirrorMap = new Dictionary<long, string>();
                foreach (var url in mirrors)
                {
                    var ping = NetEx.Ping(url);
                    while (mirrorMap.ContainsKey(ping))
                        ping++;
                    mirrorMap.Add(ping, url);
                }
                var sortedPings = mirrorMap.Keys.ToList();
                sortedPings.Sort();
                var bestPing = sortedPings.Min();
                var mirror = mirrorMap[bestPing];
                var source = NetEx.Transfer.DownloadString(string.Format(Resources.RegexUrl, mirror));
                if (string.IsNullOrWhiteSpace(source))
                    foreach (var ping in sortedPings)
                    {
                        source = NetEx.Transfer.DownloadString(string.Format(Resources.RegexUrl, ping));
                        break;
                    }
                if (string.IsNullOrWhiteSpace(source))
                    throw new ArgumentNullException(nameof(source));
                var vers = new List<Version>();
                foreach (Match match in Regex.Matches(source, Resources.RegexVersionPattern, RegexOptions.Singleline))
                {
                    var mVer = match.Groups[1].ToString();
                    if (string.IsNullOrWhiteSpace(mVer))
                        continue;
                    var cVer = mVer.ToCharArray();
                    if (!cVer.Count(char.IsDigit).IsBetween(3, 16) || !cVer.Count(c => c == '.').IsBetween(2, 3))
                        continue;
                    mVer = new string(mVer.Where(c => char.IsDigit(c) || c == '.').ToArray());
                    if (!Version.TryParse(mVer, out Version ver))
                        continue;
                    vers.Add(ver);
                }
                _ver = vers.Max();
                updUrl = string.Format(Resources.UpdateUrl, mirror, _ver,
#if x86
                    "32"
#else
                    "64"
#endif
                );
                var exists = NetEx.FileIsAvailable(updUrl, 60000, Resources.UserAgent);
                if (!exists)
                    foreach (var ping in sortedPings)
                    {
                        updUrl = string.Format(Resources.UpdateUrl, mirrorMap[ping], _ver,
#if x86
                            "32"
#else
                            "64"
#endif
                        );
                        exists = NetEx.FileIsAvailable(updUrl, 60000, Resources.UserAgent);
                        if (exists)
                            break;
                    }
                if (!exists)
                    throw new PathNotFoundException(updUrl);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                if (!_silent || !File.Exists(_appPath))
                    MessageBoxEx.Show(Resources.Msg_Warn_01, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }
            var localDate = File.GetLastWriteTime(_appPath);
            var onlineDate = NetEx.GetFileDate(updUrl, Resources.UserAgent);
            if ((onlineDate - localDate).Days > 0)
            {
                if (_silent || MessageBoxEx.Show(Resources.Msg_Hint_00, Resources.WindowTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    var archivePath = PathEx.Combine(PathEx.LocalDir, $"..\\{PathEx.GetTempFileName()}");
                    if (!File.Exists(archivePath))
                    {
                        _tmpDir = PathEx.Combine(Path.GetTempPath(), PathEx.GetTempDirName(Resources.AppName));
                        var hlpPath = Path.Combine(_tmpDir, "7z.zip");
                        ResourcesEx.Extract(Resources._7z, hlpPath, true);
                        Compaction.Unzip(hlpPath, _tmpDir);
                        _transfer.DownloadFile(updUrl, archivePath);
                        Opacity = 1f;
                        CheckDownload.Enabled = true;
                        return;
                    }
                    ExtractDownload.RunWorkerAsync();
                }
                Application.Exit();
                return;
            }
            if (!_silent)
                MessageBoxEx.Show(Resources.Msg_Hint_01, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
        }

        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            DLSpeed.Text = _transfer.TransferSpeedAd;
            DLPercentage.Value = _transfer.ProgressPercentage;
            DLLoaded.Text = _transfer.DataReceived;
            if (!_transfer.IsBusy)
                _countdown--;
            if (_countdown == 9)
                DLPercentage.JumpToEnd();
            TaskBar.Progress.SetValue(Handle, DLPercentage.Value, DLPercentage.Maximum);
            if (_countdown > 0)
                return;
            CheckDownload.Enabled = false;
            ExtractDownload.RunWorkerAsync();
        }

        private void ExtractDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            var updDir = PathEx.Combine(PathEx.LocalDir, $"..\\{PathEx.GetTempDirName()}");
            try
            {
                if (_transfer?.FilePath == null || !File.Exists(_transfer.FilePath))
                    return;
                Compaction.Zip7Helper.ExePath = PathEx.Combine(_tmpDir, "7z.exe");
                if (!File.Exists(Compaction.Zip7Helper.ExePath))
                    throw new FileNotFoundException();
                if (!Directory.Exists(updDir))
                    Directory.CreateDirectory(updDir);
                using (var p = Compaction.Zip7Helper.Unzip(_transfer.FilePath, updDir, ProcessWindowStyle.Minimized))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
                var innerDir = Path.Combine(updDir, string.Concat("vlc-", _ver));
                var updFiles = Directory.GetFiles(innerDir, "*", SearchOption.TopDirectoryOnly);
                if (updFiles.Length == 0)
                    throw new ArgumentNullException(nameof(updFiles));
                var appPath = Directory.EnumerateFiles(updDir, $"*{Resources.AppName}.exe", SearchOption.AllDirectories).FirstOrDefault();
                if (string.IsNullOrEmpty(appPath))
                    throw new ArgumentNullException(nameof(appPath));
                var curFiles = Directory.EnumerateFiles(PathEx.LocalDir, "*", SearchOption.AllDirectories).Where(x => !Resources.WhiteList.ContainsEx(Path.GetFileName(x)));
                foreach (var file in curFiles)
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        try
                        {
                            File.Move(file, $"{file}.{{{Path.GetDirectoryName(updDir)}}}");
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    }
                Data.DirCopy(innerDir, PathEx.LocalDir, true, true);
                if (Directory.Exists(updDir))
                    Directory.Delete(updDir, true);
                if (File.Exists(_appPath))
                    File.SetLastWriteTime(_appPath, DateTime.Now);
                e.Result = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                e.Result = false;
                Data.ForceDelete(updDir);
            }
        }

        private void ExtractDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);
            switch (e.Result as bool?)
            {
                case true:
                    if (!_silent)
                        MessageBoxEx.Show(Resources.Msg_Hint_02, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                default:
                    if (!_silent || !File.Exists(_appPath))
                        MessageBoxEx.Show(Resources.Msg_Warn_01, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
            Application.Exit();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Ini.WriteDirect("History", "LastCheck", File.Exists(_appPath) ? DateTime.Now : DateTime.MinValue);
            ProcessEx.SendHelper.WaitThenDelete(_tmpDir, 5, Elevation.IsAdministrator);
            ProcessEx.SendHelper.WaitThenDelete(_transfer.FilePath, 5, Elevation.IsAdministrator);
        }
    }
}
