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
                    var archivePath = PathEx.Combine(PathEx.LocalDir, "update.zip");
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
            try
            {
                if (_transfer?.FilePath == null || !File.Exists(_transfer.FilePath))
                    return;
                using (var archive = ZipFile.OpenRead(_transfer.FilePath))
                    foreach (var ent in archive.Entries)
                    {
                        var entPath = PathEx.Combine(PathEx.LocalDir, ent.FullName);
                        if (!Path.HasExtension(entPath))
                            continue;
                        if (File.Exists(entPath))
                            File.Delete(entPath);
                        var entDir = Path.GetDirectoryName(entPath);
                        if (entDir != null && !Directory.Exists(entDir))
                            Directory.CreateDirectory(entDir);
                        ent.ExtractToFile(entPath, true);
                        if (ent.Name.EqualsEx($"{Resources.AppName}.exe"))
                            File.SetLastWriteTime(entPath, DateTime.Now);
                    }
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
                        MessageBoxEx.Show(Resources.Msg_Warn_01, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                }
            Application.Exit();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Ini.WriteDirect("History", "LastCheck", DateTime.Now);
            ProcessEx.SendHelper.WaitThenDelete(_transfer.FilePath);
        }
    }
}
