using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RunPHP
{
    public partial class MainForm : Form
    {
        #region INTERN LIBRARY

        Form DLForm = new DownloadForm();

        string php = string.Empty;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int GetProcessId(IntPtr handle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowText(IntPtr hWnd, string text);

        private void CenterWindow(IntPtr hChildWnd, bool wnd)
        {
            Rectangle recChild = new Rectangle(0, 0, 0, 0);
            GetWindowRect(hChildWnd, ref recChild);

            int width = recChild.Width - recChild.X;
            int height = recChild.Height - recChild.Y;

            Point ptCenter = new Point(0, 0);
            if (wnd)
            {
                Rectangle recParent = new Rectangle(0, 0, 0, 0);
                GetWindowRect(this.Handle, ref recParent);
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

            MoveWindow(hChildWnd, ptStart.X, ptStart.Y, width, height, false);
        }

        private void CenterWindow(IntPtr hChildWnd)
        {
            CenterWindow(hChildWnd, true);
        }

        private void Run(string _file, string _path, string _args, bool _wnd)
        {
            if (File.Exists(_file) && Directory.Exists(_path))
            {
                if (this.Opacity != 0) 
                    this.Opacity = 0;
                try
                {
                    using (Process run = new Process())
                    {
                        run.StartInfo.Arguments = string.Format("-f \"{0}\"{1}", _file, _args);
                        run.StartInfo.FileName = php;
                        run.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        run.StartInfo.WorkingDirectory = _path;
                        run.Start();
                        Process RunningPHP = Process.GetProcessById(GetProcessId(run.Handle));
                        while (RunningPHP.MainWindowTitle != this.Text)
                        {
                            if (!run.HasExited)
                            {
                                CenterWindow(run.MainWindowHandle, _wnd);
                                SetWindowText(run.MainWindowHandle, this.Text);
                                RunningPHP = Process.GetProcessById(GetProcessId(run.Handle));
                                continue;
                            }
                            break;
                        }
                        if (!run.HasExited)
                            run.WaitForExit();
                    }
                }
                catch
                {
                    Process[] RunningPHP = Process.GetProcessesByName("php");
                    if (RunningPHP.Length > 0)
                        foreach (Process p in RunningPHP)
                            p.Kill();
                }
                if (this.Opacity != 1)
                    this.Opacity = 1;
            }
        }

        private void Run(string _file, string _path, string _args)
        {
            Run(_file, _path, _args, true);
        }

        private void Run(string _file, string _path)
        {
            Run(_file, _path, string.Empty, true);
        }

        private void Run(string _file)
        {
            Run(_file, Path.GetDirectoryName(_file), string.Empty, true);
        }

        #endregion

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Opacity = 0;
            php = Path.Combine(Application.StartupPath, "php\\php.exe");
            if (!File.Exists(php))
            {
                foreach (string dir in Directory.GetDirectories(Application.StartupPath))
                {
                    string _file = Path.Combine(dir, "php.exe");
                    if (File.Exists(_file))
                        php = _file;
                }
            }
            if (!File.Exists(php))
            {
                DialogResult dialog = SilDev.MsgBox.Show(this, "PHP not found. Do you want to download the newest verson now?", "PHP Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialog == DialogResult.Yes)
                {
                    if (!CheckDownload.Enabled)
                        CheckDownload.Enabled = true;
                    if (this.Opacity < 1)
                        this.Opacity = 1;
                    DLForm.Show(this);
                    CenterWindow(DLForm.Handle, true);
                }
                else
                    Environment.Exit(-1);
            }
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                string _file = string.Empty;
                string _path = string.Empty;
                string _args = string.Empty;
                for (int i = 1; i < Environment.GetCommandLineArgs().Length; i++)
                {
                    string arg = Environment.GetCommandLineArgs()[i];
                    switch (i)
                    {
                        case 1:
                            _file = arg;
                            break;
                        case 2:
                            _path = arg;
                            break;
                        case 3:
                            _args = arg;
                            break;
                    }
                }
                if (File.Exists(_file))
                {
                    FilePathTB.Text = _file;
                    DirTB.Text = Directory.Exists(_path) ? _path : Path.GetDirectoryName(_file);
                    ArgsTB.Text = _args;
                    //Run(FilePathTB.Text, DirTB.Text, !string.IsNullOrWhiteSpace(ArgsTB.Text) ? string.Format(" -- {0}", ArgsTB.Text) : string.Empty, false);
                    //this.Close();
                }
            }
            if (this.Opacity < 1)
                this.Opacity = 1;
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
                if (this.BackColor != SystemColors.Control)
                    this.BackColor = SystemColors.Control;
                if (!MainLayout.Visible)
                    MainLayout.Visible = true;
            }
            else
            {
                if (this.BackColor != SystemColors.ControlDarkDark)
                    this.BackColor = SystemColors.ControlDarkDark;
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

        private void RunBtn_Click(object sender, EventArgs e)
        {
            Run(FilePathTB.Text, DirTB.Text, !string.IsNullOrWhiteSpace(ArgsTB.Text) ? string.Format(" -- {0}", ArgsTB.Text) : string.Empty);
        }

        private void ScBtn_Click(object sender, EventArgs e)
        {
            string _file = FilePathTB.Text;
            string _path = DirTB.Text;
            if (File.Exists(_file) && Directory.Exists(_path))
            {
                string _batch = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), new FileInfo(_file).Name);
                string[] _content = new string[]
                {
                    "@ECHO OFF",
                    "TITLE Run PHP",
                    string.Format("CD /D \"{0}\"", Application.StartupPath),
                    string.Format("START {0} \"{1}\" \"{2}\"{3}", Path.GetFileName(Application.ExecutablePath), _file, _path, (!string.IsNullOrWhiteSpace(ArgsTB.Text) ? string.Format(" \"{0}\"", ArgsTB.Text) : string.Empty)),
                    "EXIT"
                };
                File.WriteAllText(string.Format("{0}.cmd", _batch), string.Join(Environment.NewLine, _content));
            }
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            DialogResult dialog = SilDev.MsgBox.Show(this, "Do you want to search updates for PHP?", "Check for Updates", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialog == DialogResult.Yes)
            {
                string phpVersion = FileVersionInfo.GetVersionInfo(php).ProductVersion;
                string source = SilDev.Network.DownloadString("http://windows.php.net/downloads/releases/sha1sum.txt");
                if (!source.Contains(string.Format("php-{0}-", phpVersion)))
                {
                    dialog = SilDev.MsgBox.Show(this, "A new version is available. Do you want to download it now?", "Updates Found", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dialog == DialogResult.Yes)
                    {
                        if (Directory.Exists(Path.GetDirectoryName(php)))
                            Directory.Delete(Path.GetDirectoryName(php), true);
                        if (!CheckDownload.Enabled)
                            CheckDownload.Enabled = true;
                        if (this.Opacity < 1)
                            this.Opacity = 1;
                        DLForm.Show(this);
                        CenterWindow(DLForm.Handle, true);
                    }
                }
                else
                    SilDev.MsgBox.Show(this, "You have already the newest version.", "No Updates Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
