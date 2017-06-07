namespace AppUpdater
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using SilDev.Forms;

    public partial class MainForm : Form
    {
        private readonly NetEx.AsyncTransfer _transfer = new NetEx.AsyncTransfer();
        private int _countdown = 10;
        private bool _silent;

        public MainForm()
        {
            InitializeComponent();
            Icon = Resources.CCUpdater;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _silent = Environment.CommandLine.ContainsEx("/silent");
            Text = Resources.WindowTitle;
            TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);
            var appPath = PathEx.Combine(Resources.AppPath);
            Version localVersion;
            try
            {
                var version = FileVersionInfo.GetVersionInfo(appPath).FileVersion;
                if (version.Contains(','))
                    version = version.Split(',').Select(c => c.Trim()).Join('.');
                localVersion = Version.Parse(version);
            }
            catch
            {
                localVersion = new Version("0.0.0.0");
            }
            Version onlineVersion;
            try
            {
                var source = NetEx.Transfer.DownloadString(Resources.VersionUrl);
                if (string.IsNullOrEmpty(source))
                    throw new ArgumentNullException(nameof(source));
                source = TextEx.FormatNewLine(source).SplitNewLine().SkipWhile(x => !x.ContainsEx(Resources.VersionHeader)).Take(1).Join();
                var inner = Regex.Match(source, Resources.VersionRegex).Groups[1].ToString();
                if (string.IsNullOrEmpty(inner))
                    throw new ArgumentNullException(nameof(inner));
                var index = inner.IndexOf('(');
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                var ver = inner.Substring(0, index).Trim(' ', 'v').Split('.');
                if (!Version.TryParse($"{ver.Take(2).Join('.')}.0.{ver.Last()}", out onlineVersion))
                    throw new ArgumentNullException(nameof(onlineVersion));
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                if (!_silent)
                    MessageBoxEx.Show(Resources.Msg_Warn_01, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }
            if (localVersion < onlineVersion)
            {
                if (_silent || MessageBoxEx.Show(Resources.Msg_Hint_00, Resources.WindowTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    var archivePath = PathEx.Combine(PathEx.LocalDir, $"..\\{PathEx.GetTempFileName()}");
                    if (!File.Exists(archivePath))
                    {
                        _transfer.DownloadFile(Resources.UpdateUrl, archivePath);
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
            var entry = string.Empty;
            try
            {
                if (_transfer?.FilePath == null || !File.Exists(_transfer.FilePath))
                    return;
                if (!Directory.Exists(updDir))
                    Directory.CreateDirectory(updDir);
                using (var archive = ZipFile.OpenRead(_transfer.FilePath))
                    foreach (var ent in archive.Entries)
                        try
                        {
                            if (!Path.HasExtension(ent.FullName))
                                continue;
                            var entPath = ent.FullName;
                            entPath = PathEx.Combine(updDir, entPath);
                            if (File.Exists(entPath))
                                File.Delete(entPath);
                            var entDir = Path.GetDirectoryName(entPath);
                            if (string.IsNullOrEmpty(entDir))
                                continue;
                            if (!Directory.Exists(entDir))
                            {
                                if (File.Exists(entDir))
                                    File.Delete(entDir);
                                Directory.CreateDirectory(entDir);
                            }
                            ent.ExtractToFile(entPath, true);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                            entry = ent.FullName.Split('/').Join('\\');
                            throw;
                        }
                var appPath = Directory.EnumerateFiles(updDir, $"*{Resources.AppName}.exe", SearchOption.AllDirectories).FirstOrDefault();
                if (string.IsNullOrEmpty(appPath))
                    throw new ArgumentNullException(nameof(appPath));
                appPath = Directory.EnumerateFiles(updDir, $"*{Resources.AppName64}.exe", SearchOption.AllDirectories).FirstOrDefault();
                if (string.IsNullOrEmpty(appPath))
                    throw new ArgumentNullException(nameof(appPath));
                var appDir = Path.GetDirectoryName(appPath);
                if (string.IsNullOrEmpty(appDir))
                    throw new ArgumentNullException(nameof(appDir));
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
                            File.Move(file, $"{file}.{{{Path.GetFileName(updDir)}}}");
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                            Data.ForceDelete(file, true);
                        }
                    }
                Data.DirCopy(appDir, PathEx.LocalDir, true, true);
                if (Directory.Exists(updDir))
                    Directory.Delete(updDir, true);
                appPath = PathEx.Combine(Resources.AppPath);
                if (File.Exists(appPath))
                    File.SetLastWriteTime(appPath, DateTime.Now);
                appPath = PathEx.Combine(Resources.AppPath64);
                if (File.Exists(appPath))
                    File.SetLastWriteTime(appPath, DateTime.Now);
                e.Result = true;
            }
            catch (PathTooLongException ex)
            {
                Log.Write(ex);
                e.Result = false;
                Data.ForceDelete(updDir);
                var appPath = PathEx.Combine(Resources.AppPath);
                if (_silent)
                    _silent = File.Exists(appPath);
                if (!_silent)
                    MessageBoxEx.Show(string.Format("{0}{3}{3}('{1}\\{2}')", ex.Message, PathEx.LocalDir, entry, Environment.NewLine), Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (!_silent)
                switch (e.Result as bool?)
                {
                    case true:
                        MessageBoxEx.Show(Resources.Msg_Hint_02, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    default:
                        MessageBoxEx.Show(Resources.Msg_Warn_01, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                }
            Application.Exit();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            var appPath = PathEx.Combine(Resources.AppPath);
            var appPath64 = PathEx.Combine(Resources.AppPath64);
            Ini.WriteDirect("History", "LastCheck", File.Exists(appPath) && File.Exists(appPath64) ? DateTime.Now : DateTime.MinValue);
            ProcessEx.SendHelper.WaitThenDelete(_transfer.FilePath);
        }
    }
}
