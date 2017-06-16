namespace RunPHP
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using SilDev.Forms;

    public partial class MainForm : Form
    {
        private readonly Form _downloadForm = new DownloadForm();
        private readonly string _appPath = PathEx.Combine(Resources.AppPath);

        public MainForm()
        {
            InitializeComponent();
            Icon = Resources.appicon16;
        }

        private void CenterWindow(IntPtr hWnd, bool isChild)
        {
            var rect = new Rectangle(0, 0, 0, 0);
            WinApi.UnsafeNativeMethods.GetWindowRect(hWnd, ref rect);
            var width = rect.Width - rect.X;
            var height = rect.Height - rect.Y;
            var point = new Point(0, 0);
            if (isChild)
            {
                rect = new Rectangle(0, 0, 0, 0);
                WinApi.UnsafeNativeMethods.GetWindowRect(Handle, ref rect);
                point.X = rect.X + (rect.Width - rect.X) / 2;
                point.Y = rect.Y + (rect.Height - rect.Y) / 2;
            }
            else
            {
                point.X = Screen.PrimaryScreen.Bounds.Width / 2;
                point.Y = Screen.PrimaryScreen.Bounds.Height / 2;
            }
            point = new Point
            {
                X = point.X - width / 2,
                Y = point.Y - height / 2
            };
            var min = new Point(0, 0);
            var max = new Point(Screen.FromHandle(hWnd).WorkingArea.Width - width, Screen.FromHandle(hWnd).WorkingArea.Height - height);
            point.X = point.X < min.X ? min.X : point.X > max.X ? max.X : point.X;
            point.Y = point.Y < min.Y ? min.Y : point.Y > max.Y ? max.Y : point.Y;
            WinApi.UnsafeNativeMethods.MoveWindow(hWnd, point.X, point.Y, width, height, false);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!File.Exists(_appPath))
            {
                var dialog = MessageBoxEx.Show(this, Resources.Msg_Ask_00, Resources.WindowTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialog == DialogResult.Yes)
                {
                    if (!CheckDownload.Enabled)
                        CheckDownload.Enabled = true;
                    _downloadForm.Show(this);
                    CenterWindow(_downloadForm.Handle, true);
                }
                else
                {
                    Application.Exit();
                    return;
                }
            }
            if (Environment.GetCommandLineArgs().Length <= 1)
                return;
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
            if (!File.Exists(path))
                return;
            FilePathTB.Text = path;
            DirTB.Text = Directory.Exists(dir) ? dir : Path.GetDirectoryName(path);
            ArgsTB.Text = args;
        }

        private void MainForm_Move(object sender, EventArgs e)
        {
            if (Application.OpenForms.Count > 1)
                CenterWindow(_downloadForm.Handle, true);
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
                dialog.Filter = Resources.FileFilter;
                dialog.Multiselect = false;
                dialog.ShowDialog(new Form
                {
                    ShowIcon = false,
                    TopMost = true
                });
                if (string.IsNullOrEmpty(dialog.FileName) || !File.Exists(dialog.FileName))
                    return;
                FilePathTB.Text = dialog.FileName;
                FilePathTB.Select(0, FilePathTB.Text.Length - 1);
                FilePathTB.ScrollToCaret();
                DirTB.Text = Path.GetDirectoryName(FilePathTB.Text);
                if (DirTB.Text?.Length > 1)
                    DirTB.Select(0, DirTB.Text.Length - 1);
                DirTB.ScrollToCaret();
            }
        }

        private void DirBtn_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.ShowDialog(new Form
                {
                    ShowIcon = false,
                    TopMost = true
                });
                if (string.IsNullOrEmpty(dialog.SelectedPath) || !Directory.Exists(dialog.SelectedPath))
                    return;
                DirTB.Text = Path.GetDirectoryName(dialog.SelectedPath);
                if (DirTB.Text?.Length > 1)
                    DirTB.Select(0, DirTB.Text.Length - 1);
                DirTB.ScrollToCaret();
            }
        }

        private void RunBtn_Click(object sender, EventArgs e)
        {
            var dir = PathEx.Combine(DirTB.Text);
            if (!Directory.Exists(dir))
            {
                MessageBoxEx.Show(this, Resources.Msg_Hint_02, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            var path = PathEx.Combine(FilePathTB.Text);
            if (!File.Exists(path))
            {
                MessageBoxEx.Show(this, Resources.Msg_Hint_02, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            WindowState = FormWindowState.Minimized;
            var arguments = ArgsTB.Text;
            if (!string.IsNullOrWhiteSpace(arguments))
                arguments = $" -- {arguments}";
            arguments = string.Format(Resources.ComArgs, _appPath, path, arguments).TrimEnd();
            using (var p = ProcessEx.Start(Resources.ComSpec, dir, arguments, Elevation.IsAdministrator, false))
                if (p?.HasExited == false)
                    p.WaitForExit();
            WindowState = FormWindowState.Normal;
        }

        private void ScBtn_Click(object sender, EventArgs e)
        {
            var dir = PathEx.Combine(DirTB.Text);
            if (!Directory.Exists(dir))
                return;
            var path = PathEx.Combine(FilePathTB.Text);
            if (!File.Exists(path))
                return;
            var arguments = ArgsTB.Text;
            if (!string.IsNullOrWhiteSpace(arguments))
                arguments = $" -- {arguments}";
            arguments = string.Format(Resources.Args, EnvironmentEx.GetVariablePathFull(path, false), arguments).TrimEnd();
            var script = PathEx.Combine(string.Format(Resources.ScriptPath, Path.GetFileName(path), new Crypto.Md5().EncryptFile(path)));
            var content = string.Format(Resources.ScriptDummy, EnvironmentEx.GetVariablePathFull(_appPath, false), EnvironmentEx.GetVariablePathFull(dir, false), arguments);
            File.WriteAllText(script, content);
            MessageBoxEx.Show(this, Resources.Msg_Hint_01, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            ProcessEx.Start(Resources.ExplorerPath, Path.GetDirectoryName(script));
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            var dialog = MessageBoxEx.Show(this, Resources.Msg_Ask_01, Resources.WindowTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialog != DialogResult.Yes)
            {
                MessageBoxEx.Show(this, Resources.Msg_Hint_02, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (!NetEx.InternetIsAvailable())
            {
                MessageBoxEx.Show(this, Resources.Msg_Err_00, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var source = NetEx.Transfer.DownloadString($"{Resources.UpdateUrl}/{Resources.HashFile}");
                var version = FileVersionInfo.GetVersionInfo(_appPath).ProductVersion;
                if (source.Contains($"{Resources.AppName}-{version}-"))
                    throw new ArgumentException();
                dialog = MessageBoxEx.Show(this, Resources.Msg_Ask_02, Resources.WindowTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                if (dialog != DialogResult.Yes)
                    throw new WarningException();
                var dir = Path.GetDirectoryName(_appPath);
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
                if (!CheckDownload.Enabled)
                    CheckDownload.Enabled = true;
                _downloadForm.Show(this);
                CenterWindow(_downloadForm.Handle, true);
            }
            catch (WarningException)
            {
                MessageBoxEx.Show(this, Resources.Msg_Hint_02, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception)
            {
                MessageBoxEx.Show(this, Resources.Msg_Hint_00, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
    }
}
