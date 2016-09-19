using SilDev;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RunPHP
{
    public partial class MainForm : Form
    {
        Form DLForm = new DownloadForm();
        string php = string.Empty;

        private void CenterWindow(IntPtr hChildWnd, bool wnd)
        {
            Rectangle recChild = new Rectangle(0, 0, 0, 0);
            WINAPI.SafeNativeMethods.GetWindowRect(hChildWnd, ref recChild);

            int width = recChild.Width - recChild.X;
            int height = recChild.Height - recChild.Y;

            Point ptCenter = new Point(0, 0);
            if (wnd)
            {
                Rectangle recParent = new Rectangle(0, 0, 0, 0);
                WINAPI.SafeNativeMethods.GetWindowRect(Handle, ref recParent);
                ptCenter.X = recParent.X + ((recParent.Width - recParent.X) / 2);
                ptCenter.Y = recParent.Y + ((recParent.Height - recParent.Y) / 2);
            }
            else
            {
                ptCenter.X = (Screen.PrimaryScreen.Bounds.Width / 2);
                ptCenter.Y = (Screen.PrimaryScreen.Bounds.Height / 2);
            }

            Point ptStart = new Point(0, 0);
            ptStart.X = (ptCenter.X - (width / 2));
            ptStart.Y = (ptCenter.Y - (height / 2));

            ptStart.X = (ptStart.X < 0) ? 0 : ptStart.X;
            ptStart.Y = (ptStart.Y < 0) ? 0 : ptStart.Y;

            WINAPI.SafeNativeMethods.MoveWindow(hChildWnd, ptStart.X, ptStart.Y, width, height, false);
        }

        private void CenterWindow(IntPtr hChildWnd) =>
            CenterWindow(hChildWnd, true);

        private void Run(string fileName, string workingDir, string args, bool centerWindow = true)
        {
            if (File.Exists(fileName) && Directory.Exists(workingDir))
            {
                if (Opacity != 0)
                    Opacity = 0;
                try
                {
                    using (Process p = new Process())
                    {
                        p.StartInfo.Arguments = string.Format("-f \"{0}\"{1}", fileName, args);
                        p.StartInfo.FileName = php;
                        p.StartInfo.WorkingDirectory = workingDir;
                        p.Start();
                        Process RunningPHP = Process.GetProcessById((int)WINAPI.SafeNativeMethods.GetProcessId(p.Handle));
                        while (RunningPHP.MainWindowTitle != Text)
                        {
                            if (!p.HasExited)
                            {
                                CenterWindow(p.MainWindowHandle, centerWindow);
                                WINAPI.SafeNativeMethods.SetWindowText(p.MainWindowHandle, Text);
                                RunningPHP = Process.GetProcessById((int)WINAPI.SafeNativeMethods.GetProcessId(p.Handle));
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
                        Process[] RunningPHP = Process.GetProcessesByName("php");
                        if (RunningPHP.Length > 0)
                            foreach (Process p in RunningPHP)
                                p.Kill();
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }
                }
                if (Opacity != 1)
                    Opacity = 1;
            }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            php = PATH.Combine("%CurDir%\\php\\php.exe");
            if (!File.Exists(php))
            {
                foreach (string dir in Directory.GetDirectories(PATH.GetEnvironmentVariableValue("CurDir")))
                {
                    string _file = Path.Combine(dir, "php.exe");
                    if (File.Exists(_file))
                        php = _file;
                }
            }
            if (!File.Exists(php))
            {
                DialogResult dialog = MSGBOX.Show(this, "PHP not found. Do you want to download the newest verson now?", "PHP Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialog == DialogResult.Yes)
                {
                    if (!CheckDownload.Enabled)
                        CheckDownload.Enabled = true;
                    if (Opacity < 1)
                        Opacity = 1;
                    DLForm.Show(this);
                    CenterWindow(DLForm.Handle, true);
                }
                else
                    Environment.Exit(-1);
            }
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                string path = string.Empty;
                string dir = string.Empty;
                string args = string.Empty;
                for (int i = 1; i < Environment.GetCommandLineArgs().Length; i++)
                {
                    string arg = Environment.GetCommandLineArgs()[i];
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
                CenterWindow(DLForm.Handle, true);
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
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "PHP Script (*.php)|*.php|All Files (*.*)|*.*";
                dialog.Multiselect = false;
                dialog.ShowDialog(new Form() { ShowIcon = false, TopMost = true });
                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    if (File.Exists(dialog.FileName))
                    {
                        FilePathTB.Text = dialog.FileName;
                        FilePathTB.Select(0, FilePathTB.Text.Length - 1);
                        FilePathTB.ScrollToCaret();
                        DirTB.Text = Path.GetDirectoryName(php);
                        DirTB.Select(0, DirTB.Text.Length - 1);
                        DirTB.ScrollToCaret();
                    }
                }
            }
        }

        private void DirBtn_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.ShowDialog(new Form() { ShowIcon = false, TopMost = true });
                if (!string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    if (Directory.Exists(dialog.SelectedPath))
                    {
                        DirTB.Text = Path.GetDirectoryName(dialog.SelectedPath);
                        DirTB.Select(0, DirTB.Text.Length - 1);
                        DirTB.ScrollToCaret();
                    }
                }
            }
        }

        private void RunBtn_Click(object sender, EventArgs e) =>
            Run(FilePathTB.Text, DirTB.Text, !string.IsNullOrWhiteSpace(ArgsTB.Text) ? $" -- {ArgsTB.Text}" : string.Empty);

        private void ScBtn_Click(object sender, EventArgs e)
        {
            string path = FilePathTB.Text;
            string dir = DirTB.Text;
            if (File.Exists(path) && Directory.Exists(dir))
            {
                string _batch = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), new FileInfo(path).Name);
                string[] _content = new string[]
                {
                    "@ECHO OFF",
                    "TITLE Run PHP",
                    $"CD /D \"{PATH.GetEnvironmentVariableValue("CurDir")}\"",
                    $"START {Path.GetFileName(Application.ExecutablePath)} \"{path}\" \"{dir}\"{(!string.IsNullOrWhiteSpace(ArgsTB.Text) ? string.Format(" \"{0}\"", ArgsTB.Text) : string.Empty)}",
                    "EXIT"
                };
                File.WriteAllText(string.Format("{0}.cmd", _batch), string.Join(Environment.NewLine, _content));
            }
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MSGBOX.Show(this, "Do you want to search updates for PHP?", "Check for Updates", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialog == DialogResult.Yes)
            {
                string phpVersion = FileVersionInfo.GetVersionInfo(php).ProductVersion;
                string source = NET.DownloadString("http://windows.php.net/downloads/releases/sha1sum.txt");
                if (!source.Contains(string.Format("php-{0}-", phpVersion)))
                {
                    dialog = MSGBOX.Show(this, "A new version is available. Do you want to download it now?", "Updates Found", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dialog == DialogResult.Yes)
                    {
                        if (Directory.Exists(Path.GetDirectoryName(php)))
                            Directory.Delete(Path.GetDirectoryName(php), true);
                        if (!CheckDownload.Enabled)
                            CheckDownload.Enabled = true;
                        if (Opacity < 1)
                            Opacity = 1;
                        DLForm.Show(this);
                        CenterWindow(DLForm.Handle, true);
                    }
                }
                else
                    MSGBOX.Show(this, "You have already the newest version.", "No Updates Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
