namespace RunPHP
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using SilDev;
    using SilDev.Forms;

    public partial class MainForm : Form
    {
        private readonly Form _dlForm = new DownloadForm();
        private string _php = string.Empty;

        public MainForm()
        {
            InitializeComponent();
        }

        private void CenterWindow(IntPtr hChildWnd, bool wnd)
        {
            var recChild = new Rectangle(0, 0, 0, 0);
            WinApi.UnsafeNativeMethods.GetWindowRect(hChildWnd, ref recChild);

            var width = recChild.Width - recChild.X;
            var height = recChild.Height - recChild.Y;

            var ptCenter = new Point(0, 0);
            if (wnd)
            {
                var recParent = new Rectangle(0, 0, 0, 0);
                WinApi.UnsafeNativeMethods.GetWindowRect(Handle, ref recParent);
                ptCenter.X = recParent.X + (recParent.Width - recParent.X) / 2;
                ptCenter.Y = recParent.Y + (recParent.Height - recParent.Y) / 2;
            }
            else
            {
                ptCenter.X = Screen.PrimaryScreen.Bounds.Width / 2;
                ptCenter.Y = Screen.PrimaryScreen.Bounds.Height / 2;
            }

            var ptStart = new Point
            {
                X = ptCenter.X - width / 2,
                Y = ptCenter.Y - height / 2
            };

            ptStart.X = ptStart.X < 0 ? 0 : ptStart.X;
            ptStart.Y = ptStart.Y < 0 ? 0 : ptStart.Y;

            WinApi.UnsafeNativeMethods.MoveWindow(hChildWnd, ptStart.X, ptStart.Y, width, height, false);
        }

        private void Run(string fileName, string workingDir, string args, bool centerWindow = true)
        {
            if (!File.Exists(fileName) || !Directory.Exists(workingDir))
                return;
            if (Opacity > 0)
                Opacity = 0;
            try
            {
                using (var p = new Process())
                {
                    p.StartInfo.Arguments = $"-f \"{fileName}\"{args}";
                    p.StartInfo.FileName = _php;
                    p.StartInfo.WorkingDirectory = workingDir;
                    p.Start();
                    var runningPhp = Process.GetProcessById((int)WinApi.UnsafeNativeMethods.GetProcessId(p.Handle));
                    while (runningPhp.MainWindowTitle != Text)
                    {
                        if (!p.HasExited)
                        {
                            CenterWindow(p.MainWindowHandle, centerWindow);
                            WinApi.UnsafeNativeMethods.SetWindowText(p.MainWindowHandle, Text);
                            runningPhp = Process.GetProcessById((int)WinApi.UnsafeNativeMethods.GetProcessId(p.Handle));
                            continue;
                        }
                        break;
                    }
                    if (!p.HasExited)
                        p.WaitForExit();
                }
            }
            catch
            {
                try
                {
                    var runningPhp = Process.GetProcessesByName("php");
                    if (runningPhp.Length > 0)
                        foreach (var p in runningPhp)
                            p.Kill();
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
            if (Opacity < 1)
                Opacity = 1;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            _php = PathEx.Combine("%CurDir%\\php\\php.exe");
            if (!File.Exists(_php))
                foreach (var dir in Directory.GetDirectories(EnvironmentEx.GetVariableValue("CurDir")))
                {
                    var file = Path.Combine(dir, "php.exe");
                    if (File.Exists(file))
                        _php = file;
                }
            if (!File.Exists(_php))
            {
                var dialog = MessageBoxEx.Show(this, "PHP not found. Do you want to download the newest verson now?", "PHP Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialog == DialogResult.Yes)
                {
                    if (!CheckDownload.Enabled)
                        CheckDownload.Enabled = true;
                    if (Opacity < 1)
                        Opacity = 1;
                    _dlForm.Show(this);
                    CenterWindow(_dlForm.Handle, true);
                }
                else
                    Environment.Exit(-1);
            }
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                var path = string.Empty;
                var dir = string.Empty;
                var args = string.Empty;
                for (var i = 1; i < Environment.GetCommandLineArgs().Length; i++)
                {
                    var arg = Environment.GetCommandLineArgs()[i];
                    switch (i)
                    {
                        case 1:
                            path = arg;
                            break;
                        case 2:
                            dir = arg;
                            break;
                        case 3:
                            args = arg;
                            break;
                    }
                }
                if (File.Exists(path))
                {
                    FilePathTB.Text = path;
                    DirTB.Text = Directory.Exists(dir) ? dir : Path.GetDirectoryName(path);
                    ArgsTB.Text = args;
                }
            }
            if (Opacity < 1)
                Opacity = 1;
        }

        private void MainForm_Move(object sender, EventArgs e)
        {
            if (Application.OpenForms.Count > 1)
                CenterWindow(_dlForm.Handle, true);
        }

        private void CheckDownload_Tick(object sender, EventArgs e)
        {
            if (Application.OpenForms.Count == 1)
            {
                if (CheckDownload.Enabled)
                    CheckDownload.Enabled = false;
                if (BackColor != SystemColors.Control)
                    BackColor = SystemColors.Control;
                if (!MainLayout.Visible)
                    MainLayout.Visible = true;
            }
            else
            {
                if (BackColor != SystemColors.ControlDarkDark)
                    BackColor = SystemColors.ControlDarkDark;
                if (MainLayout.Visible)
                    MainLayout.Visible = false;
            }
        }

        private void FilePathBtn_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = @"PHP Script (*.php)|*.php|All Files (*.*)|*.*";
                dialog.Multiselect = false;
                dialog.ShowDialog(new Form { ShowIcon = false, TopMost = true });
                if (string.IsNullOrEmpty(dialog.FileName))
                    return;
                if (!File.Exists(dialog.FileName))
                    return;
                FilePathTB.Text = dialog.FileName;
                FilePathTB.Select(0, FilePathTB.Text.Length - 1);
                FilePathTB.ScrollToCaret();
                DirTB.Text = Path.GetDirectoryName(_php);
                if (DirTB.Text?.Length > 1)
                    DirTB.Select(0, DirTB.Text.Length - 1);
                DirTB.ScrollToCaret();
            }
        }

        private void DirBtn_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.ShowDialog(new Form { ShowIcon = false, TopMost = true });
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                    return;
                if (!Directory.Exists(dialog.SelectedPath))
                    return;
                DirTB.Text = Path.GetDirectoryName(dialog.SelectedPath);
                if (DirTB.Text?.Length > 1)
                    DirTB.Select(0, DirTB.Text.Length - 1);
                DirTB.ScrollToCaret();
            }
        }

        private void RunBtn_Click(object sender, EventArgs e) =>
            Run(FilePathTB.Text, DirTB.Text, !string.IsNullOrWhiteSpace(ArgsTB.Text) ? $" -- {ArgsTB.Text}" : string.Empty);

        private void ScBtn_Click(object sender, EventArgs e)
        {
            var path = FilePathTB.Text;
            var dir = DirTB.Text;
            if (!File.Exists(path) || !Directory.Exists(dir))
                return;
            var batch = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), new FileInfo(path).Name);
            string[] content =
            {
                "@ECHO OFF",
                "TITLE Run PHP",
                $"CD /D \"{EnvironmentEx.GetVariableValue("CurDir")}\"",
                $"START {Path.GetFileName(Application.ExecutablePath)} \"{path}\" \"{dir}\"{(!string.IsNullOrWhiteSpace(ArgsTB.Text) ? $" \"{ArgsTB.Text}\"" : string.Empty)}",
                "EXIT"
            };
            File.WriteAllText($"{batch}.cmd", string.Join(Environment.NewLine, content));
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            var dialog = MessageBoxEx.Show(this, "Do you want to search updates for PHP?", "Check for Updates", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialog != DialogResult.Yes)
                return;
            var phpVersion = FileVersionInfo.GetVersionInfo(_php).ProductVersion;
            var source = NetEx.Transfer.DownloadString("http://windows.php.net/downloads/releases/sha1sum.txt");
            if (!source.Contains($"php-{phpVersion}-"))
            {
                dialog = MessageBoxEx.Show(this, "A new version is available. Do you want to download it now?", "Updates Found", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dialog != DialogResult.Yes)
                    return;
                var phpDir = Path.GetDirectoryName(_php);
                if (Directory.Exists(phpDir))
                    Directory.Delete(phpDir, true);
                if (!CheckDownload.Enabled)
                    CheckDownload.Enabled = true;
                if (Opacity < 1)
                    Opacity = 1;
                _dlForm.Show(this);
                CenterWindow(_dlForm.Handle, true);
            }
            else
                MessageBoxEx.Show(this, "You have already the newest version.", "No Updates Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
