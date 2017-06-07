namespace AppUpdater
{
    using System;
    using System.ComponentModel;
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
            Icon = Resources.Prime95Updater;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _silent = Environment.CommandLine.ContainsEx("/silent");
            Text = Resources.WindowTitle;
            TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);
            var source = NetEx.Transfer.DownloadString(Resources.RegexUrl);
            string updUrl = null;
            try
            {
                var pattern = string.Format(Resources.RegexUrlPattern,
#if x86
                    "32"
#else
                    "64"
#endif
                );
                source = TextEx.FormatNewLine(source).SplitNewLine().SkipWhile(x => !x.ContainsEx("p95") && !x.ContainsEx(".zip")).Take(1).Join();
                foreach (Match match in Regex.Matches(source, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase))
                {
                    var mUrl = match.Groups[1].ToString();
                    if (string.IsNullOrWhiteSpace(mUrl))
                        continue;
                    updUrl = string.Format(Resources.UpdateUrl, mUrl,
#if x86
                        "32"
#else
                        "64"
#endif
                    );
                    break;
                }
                if (!NetEx.FileIsAvailable(updUrl, 60000, Resources.UserAgent))
                    throw new PathNotFoundException(updUrl);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                if (!_silent)
                    MessageBoxEx.Show(Resources.Msg_Warn_01, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }
            var appPath = PathEx.Combine(Resources.AppPath);
            var localDate = File.GetLastWriteTime(appPath);
            var onlineDate = NetEx.GetFileDate(updUrl, Resources.UserAgent);
            if ((onlineDate - localDate).Days > 0)
            {
                if (_silent || MessageBoxEx.Show(Resources.Msg_Hint_00, Resources.WindowTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    var archivePath = PathEx.Combine(PathEx.LocalDir, $"..\\{PathEx.GetTempFileName()}");
                    if (!File.Exists(archivePath))
                    {
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
            Ini.WriteDirect("History", "LastCheck", File.Exists(appPath) ? DateTime.Now : DateTime.MinValue);
            ProcessEx.SendHelper.WaitThenDelete(_transfer.FilePath);
        }
    }
}
