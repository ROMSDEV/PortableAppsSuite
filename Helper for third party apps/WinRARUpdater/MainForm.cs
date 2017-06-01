namespace AppUpdater
{
    using System;
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
        private string _guid, _tmpDir;
        private int _countdown = 10;
        private bool _silent;

        public MainForm()
        {
            InitializeComponent();
            Icon = Resources.RAR;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _silent = Environment.CommandLine.ContainsEx("/silent");
            Text = Resources.WindowTitle;
            TaskBar.Progress.SetState(Handle, TaskBar.Progress.Flags.Indeterminate);

            var iniFile = Path.ChangeExtension(PathEx.LocalPath, ".ini");
            if (!File.Exists(iniFile))
            {
                Ini.SetFile(iniFile);
                Ini.Write("Settings", "Language", "English");
                Ini.Write("Settings", "Architecture", Environment.Is64BitProcess ? "64 bit" : "32 bit");
                Ini.Write("Settings", "DoNotAskAgain", false);
            }
            else
                Ini.SetFile(iniFile);

            if (!Ini.Read("Settings", "DoNotAskAgain", false))
            {
                Form langSelection = new LangSelectionForm();
                if (langSelection.ShowDialog() != DialogResult.OK)
                {
                    MessageBoxEx.Show(Resources.Msg_Hint_03, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                    return;
                }
            }

            var lang = $"{Ini.Read<string>("Settings", "Language", "English")} ({Ini.Read<string>("Settings", "Architecture", Environment.Is64BitProcess ? "64 bit" : "32 bit")})";
            var source = NetEx.Transfer.DownloadString(Resources.RegexUrl);
            string updUrl = null;
            try
            {
                foreach (Match match in Regex.Matches(source, Resources.RegexLinePattern, RegexOptions.Singleline))
                {
                    var mLine = match.Groups[1].ToString();
                    var mLang = Regex.Match(mLine, Resources.RegexLanguagePattern, RegexOptions.Singleline).Groups[1].ToString();
                    if (string.IsNullOrWhiteSpace(mLang) || !mLang.EqualsEx(lang))
                        continue;
                    var mVer = Regex.Match(mLine, Resources.RegexVersionPattern, RegexOptions.Singleline).Groups[1].ToString();
                    if (string.IsNullOrWhiteSpace(mVer) || mVer.ContainsEx("trial", "free", "beta"))
                        continue;
                    var mName = Regex.Match(mLine, Resources.RegexFileNamePattern, RegexOptions.Singleline).Groups[1].ToString();
                    if (string.IsNullOrWhiteSpace(mName))
                        continue;
                    updUrl = string.Format(Resources.UpdateUrl, mName);
                    if (!NetEx.FileIsAvailable(updUrl, 60000, Resources.UserAgent))
                    {
                        updUrl = null;
                        continue;
                    }
                    break;
                }
                if (string.IsNullOrEmpty(updUrl))
                    throw new ArgumentNullException(nameof(updUrl));
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
                    var archivePath = PathEx.Combine(PathEx.LocalDir, "update.zip");
                    if (!File.Exists(archivePath))
                    {
                        _guid = Guid.NewGuid().ToString();
                        if (_guid.Length >= 8)
                            _guid = _guid.Substring(0, 8);
                        _tmpDir = PathEx.Combine(string.Format(Resources.TmpDir, _guid));
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
            try
            {
                if (_transfer?.FilePath == null || !File.Exists(_transfer.FilePath))
                    return;

                Compaction.Zip7Helper.ExePath = PathEx.Combine(_tmpDir, "7z.exe");

                if (!File.Exists(Compaction.Zip7Helper.ExePath))
                    throw new FileNotFoundException();

                var updDir = PathEx.Combine(string.Format(Resources.UpdateDir, _guid));
                if (!Directory.Exists(updDir))
                    Directory.CreateDirectory(updDir);

                using (var p = Compaction.Zip7Helper.Unzip(_transfer.FilePath, updDir, ProcessWindowStyle.Minimized))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();

                var updFiles = Directory.GetFiles(updDir, "*", SearchOption.TopDirectoryOnly);
                if (updFiles.Length == 0)
                    throw new ArgumentNullException(nameof(updFiles));

                var curFiles = Directory.EnumerateFiles(PathEx.LocalDir, "*", SearchOption.TopDirectoryOnly).Where(s => !Resources.WhiteList.SplitNewLine().ContainsEx(Path.GetFileName(s)));
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

                foreach (var file in updFiles)
                    try
                    {
                        File.Move(file, file.Replace(updDir, PathEx.LocalDir));
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }

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
