namespace AppUpdater
{
    using System;
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
        private readonly bool _silent = Environment.CommandLine.ContainsEx("/silent");
        private readonly string _appPath = PathEx.Combine(Resources.AppPath);
        private int _countdown = 10;
        private string _tmpDir;

        public MainForm()
        {
            InitializeComponent();
            Icon = Resources.FurMarkUpdater;
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
            string updUrl = null;
            try
            {
                var source = NetEx.Transfer.DownloadString(Resources.RegexFirstUrl);
                if (string.IsNullOrWhiteSpace(source))
                    throw new ArgumentNullException(nameof(source));
                source = TextEx.FormatNewLine(source).SplitNewLine().SkipWhile(x => !x.ContainsEx(Resources.RegexSecBtnMatch)).Take(1).Join();
                foreach (Match match in Regex.Matches(source, Resources.RegexSecUrlPattern, RegexOptions.Singleline))
                {
                    var mUrl = match.Groups[1].ToString();
                    if (string.IsNullOrWhiteSpace(mUrl))
                        continue;
                    source = NetEx.Transfer.DownloadString(mUrl);
                    if (string.IsNullOrWhiteSpace(source))
                        throw new ArgumentNullException(nameof(source));
                    source = TextEx.FormatNewLine(source).SplitNewLine().SkipWhile(x => !x.ContainsEx(Resources.RegexThirdBtnMatch) || !Resources.RegexThirdExtMatch.SplitNewLine().Any(y => x.ContainsEx(y))).Take(1).Join();
                    foreach (Match match2 in Regex.Matches(source, Resources.RegexThirdUrlPattern, RegexOptions.Singleline))
                    {
                        mUrl = match2.Groups[1].ToString();
                        if (string.IsNullOrWhiteSpace(mUrl))
                            continue;
                        if (mUrl.ContainsEx("/show/"))
                            mUrl = mUrl.Replace("/show/", "/get/");
                        updUrl = mUrl;
                        break;
                    }
                    break;
                }
                if (!NetEx.FileIsAvailable(updUrl, 60000, Resources.UserAgent))
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
            var updDir = PathEx.Combine(PathEx.LocalDir, $"..\\{PathEx.GetTempDirName()}");
            try
            {
                if (_transfer?.FilePath == null || !File.Exists(_transfer.FilePath))
                    return;
                var helperPath = PathEx.Combine(_tmpDir, "iu.exe");
                if (!File.Exists(helperPath))
                    throw new FileNotFoundException();
                if (!Directory.Exists(updDir))
                    Directory.CreateDirectory(updDir);
                using (var p = ProcessEx.Start(helperPath, string.Format(Resources.Extract, _transfer.FilePath, updDir), Elevation.IsAdministrator, ProcessWindowStyle.Minimized, false))
                    if (p?.HasExited == false)
                        p.WaitForExit();
                Thread.Sleep(1500);
                var innerDir = Path.Combine(updDir, "{app}");
                var updFiles = Directory.GetFiles(innerDir, "*", SearchOption.AllDirectories);
                if (updFiles.Length == 0)
                    throw new ArgumentNullException(nameof(updFiles));
                var appPath = Directory.EnumerateFiles(updDir, $"*{Resources.AppName}.exe", SearchOption.AllDirectories).FirstOrDefault();
                if (string.IsNullOrEmpty(appPath))
                    throw new ArgumentNullException(nameof(appPath));
                var curFiles = Directory.EnumerateFiles(PathEx.LocalDir, "*", SearchOption.AllDirectories).Where(s => !s.ContainsEx(updDir) && !Resources.WhiteList.ContainsEx(Path.GetFileName(s)));
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
