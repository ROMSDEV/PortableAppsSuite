namespace AppUpdater
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using SilDev.Forms;

    public partial class MainForm : Form
    {
        private readonly NetEx.AsyncTransfer _transfer = new NetEx.AsyncTransfer();
        private string _guid, _tmpDir;
        private int _countdown = 10;
        private bool _silent;

        public MainForm()
        {
            InitializeComponent();
            Icon = Resources.CheatEngineUpdater;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _silent = Environment.CommandLine.ContainsEx("/silent");
            Text = Resources.WindowTitle;
            TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);

            string updUrl = null;
            try
            {
                var source = NetEx.Transfer.DownloadString(Resources.RegexUrl);
                if (string.IsNullOrWhiteSpace(source))
                    throw new ArgumentNullException(nameof(source));

                var paths = new Dictionary<Version, string>();
                foreach (Match match in Regex.Matches(source, Resources.RegexPattern, RegexOptions.Singleline))
                {
                    var mPath = match.Groups[1].ToString();
                    if (string.IsNullOrWhiteSpace(mPath) || mPath.Count(c => c == '/') != 1)
                        continue;

                    var mVer = mPath.Split('/').FirstOrDefault();
                    if (string.IsNullOrEmpty(mVer) || !mVer.All(c => char.IsDigit(c) || c == '.'))
                        continue;

                    Version ver;
                    if (!Version.TryParse(mVer, out ver))
                        continue;

                    if (paths.ContainsKey(ver))
                        continue;
                    paths.Add(ver, mPath);
                }

                updUrl = string.Format(Resources.UpdateUrl, paths[paths.Keys.Max()]);
                if (!NetEx.FileIsAvailable(updUrl, 60000, Resources.UserAgent))
                    throw new PathNotFoundException(updUrl);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                if (!_silent)
                    MessageBoxEx.Show(Resources.Msg_Warn_02, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }

            var appPath = PathEx.Combine(Resources.AppPath);
            var localDate = File.GetLastWriteTime(appPath);
            var onlineDate = NetEx.GetFileDate(updUrl, Resources.UserAgent);
            if ((onlineDate - localDate).Days > 0)
            {
                if (_silent || MessageBoxEx.Show(Resources.Msg_Hint_00, Resources.WindowTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    var archivePath = PathEx.Combine(PathEx.LocalDir, "setup.exe");
                    if (!File.Exists(archivePath))
                    {
                        _guid = Guid.NewGuid().ToString();
                        if (_guid.Length >= 8)
                            _guid = _guid.Substring(0, 8);
                        _tmpDir = PathEx.Combine(string.Format(Resources.TmpDir, _guid));
                        var hlpPath = Path.Combine(_tmpDir, "iu.zip");
                        ResourcesEx.Extract(Resources.iu, hlpPath, true);
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
            try
            {
                if (_transfer?.FilePath == null || !File.Exists(_transfer.FilePath))
                    return;

                var helperPath = PathEx.Combine(_tmpDir, "iu.exe");
                if (!File.Exists(helperPath))
                    throw new FileNotFoundException();

                var updDir = PathEx.Combine(string.Format(Resources.UpdateDir, _guid));
                if (!Directory.Exists(updDir))
                    Directory.CreateDirectory(updDir);

                using (var p = ProcessEx.Start(helperPath, string.Format(Resources.Extract, _transfer.FilePath, updDir), Elevation.IsAdministrator, ProcessWindowStyle.Minimized, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();

                Thread.Sleep(1500);
                var innerDir = Path.Combine(updDir, "{app}");
                var updFiles = Directory.GetFiles(innerDir, "*", SearchOption.TopDirectoryOnly);
                if (updFiles.Length == 0)
                    throw new ArgumentNullException(nameof(updFiles));

                var curFiles = Directory.EnumerateFiles(PathEx.LocalDir, "*", SearchOption.AllDirectories)
                                        .Where(s => !s.ContainsEx(updDir) && !Resources.WhiteList.ContainsEx(Path.GetFileName(s)));
                foreach (var file in curFiles)
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        try
                        {
                            File.Move(file, $"{file}.{{{_guid}}}");
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    }

                Data.DirCopy(innerDir, PathEx.LocalDir, true, true);
                if (Directory.Exists(updDir))
                    Directory.Delete(updDir, true);

                var exePath = PathEx.Combine(Resources.AppPath);
                if (File.Exists(exePath))
                    File.SetLastWriteTime(exePath, DateTime.Now);

                e.Result = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                e.Result = false;
            }
        }

        private void ExtractDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);
            if (!_silent)
                switch (e.Result as bool?)
                {
                    case true:
                        MessageBoxEx.Show(Resources.Msg_Hint_02, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    default:
                        MessageBoxEx.Show(Resources.Msg_Warn_02, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                }
            Application.Exit();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Ini.Write("History", "LastCheck", DateTime.Now);
            Ini.WriteAll();
            ProcessEx.SendHelper.WaitThenDelete(_tmpDir, 5, Elevation.IsAdministrator);
            ProcessEx.SendHelper.WaitThenDelete(_transfer.FilePath, 5, Elevation.IsAdministrator);
        }
    }
}
