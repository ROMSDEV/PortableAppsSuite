using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;

namespace JavaPortableLauncher
{
    static class Program
    {
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink { }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        private static Dictionary<string, string> Java = new Dictionary<string, string>();

        [STAThread]
        static void Main()
        {
#if !x86
            SilDev.Log.ActivateDebug();
            SilDev.Initialization.File(Application.StartupPath, "JavaPortable.ini");
            Java.Add("exe", @"bin\javaw.exe");
            if (!JavaExists())
            {
                try
                {
                    string JavaPath = Application.StartupPath;
                    foreach (var dir in (!Application.StartupPath.Contains(@"\") ? string.Format(@"{0}\", Application.StartupPath) : Application.StartupPath).Split('\\'))
                    {
                        if (string.IsNullOrWhiteSpace(dir))
                            continue;
                        JavaPath = Path.Combine(string.Format(@"{0}\", JavaPath), dir);
                        string tmp = Path.Combine(string.Format(@"{0}\", JavaPath), @"CommonFiles\Java64");
                        if (File.Exists(Path.Combine(tmp, Java["exe"])) && SilDev.Run.Is64Bit(Path.Combine(tmp, Java["exe"])))
                        {
                            JavaPath = tmp.Replace(Application.StartupPath, "%CurrentDir%");
                            SilDev.Initialization.WriteValue("Directories", "Java64", JavaPath);
                            throw new OperationCanceledException(string.Format("Java found in {0}.", JavaPath));
                        }
                    }
                    JavaPath = null;
                    MessageBox.Show("Please select the root folder of Java RE (x64) Portable.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    while (true)
                    {
                        using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                        {
                            dialog.Description = "Java Runtime Environments (x64):";
                            dialog.ShowDialog(new Form() { Icon = (System.Drawing.Icon)Properties.Resources.Java, TopMost = true });
                            string tmp = Path.Combine(dialog.SelectedPath, Java["exe"]);
                            if (File.Exists(tmp) && SilDev.Run.Is64Bit(tmp))
                                JavaPath = dialog.SelectedPath.Replace(Application.StartupPath, "%CurrentDir%");
                            if (string.IsNullOrWhiteSpace(JavaPath))
                            {
                                if (MessageBox.Show("You wanna try another directory?", "Java RE (x64) NOT Found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                                    continue;
                            }
                        }
                        break;
                    }
                    if (!string.IsNullOrWhiteSpace(JavaPath))
                        SilDev.Initialization.WriteValue("Directories", "Java64", JavaPath);
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }
            if (!JavaExists())
            {
                MessageBox.Show("Sorry, Java not found.", "Java App Launcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(-1);
            }
            if (File.Exists(SilDev.Initialization.File()))
            {
                string AppPath = null;
                if (Environment.GetCommandLineArgs().Length <= 1)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Filter = "Java Files (*.exe, *.jar)|*.exe;*.jar|All Files (*.*)|*.*";
                        dialog.Multiselect = false;
                        dialog.Title = "Select a Java Application";
                        dialog.ShowDialog(new Form() { ShowIcon = false, TopMost = true });
                        AppPath = dialog.FileName;
                    }
                }
                else
                {
                    if (File.Exists(Environment.GetCommandLineArgs()[1]))
                        AppPath = Environment.GetCommandLineArgs()[1];
                }
                try
                {
                    if (!string.IsNullOrWhiteSpace(AppPath))
                    {
                        string AppPathMD5 = SilDev.Crypt.MD5.Encrypt(AppPath);
                        if (string.IsNullOrWhiteSpace(SilDev.Initialization.ReadValue("Shortcuts", AppPathMD5)))
                        {
                            if (MessageBox.Show("You wanna create a desktop shortcut?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                try
                                {
                                    string name = Path.GetFileNameWithoutExtension(AppPath);
                                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), string.Format("{0}.lnk", name));
                                    IShellLink shell = (IShellLink)new ShellLink();
                                    shell.SetArguments(string.Format("\"{0}\"", AppPath));
                                    shell.SetDescription(string.Empty);
                                    shell.SetPath(Assembly.GetExecutingAssembly().Location);
                                    shell.SetWorkingDirectory(Application.StartupPath);
                                    IPersistFile file = (IPersistFile)shell;
                                    file.Save(path, false);
                                    MessageBox.Show(string.Format("Desktop shortcut for {0} created.", name), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                catch
                                {
                                    MessageBox.Show("Sorry, something went wrong. No shortcut created.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                            if (MessageBox.Show("Ask again for this file?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                                SilDev.Initialization.WriteValue("Shortcuts", AppPathMD5, "True");
                        }
                        using (Process app = new Process())
                        {
                            string JavaDir = SilDev.Initialization.ReadValue("Directories", "Java64");
                            app.StartInfo.Arguments = string.Format("-jar \"{0}\"", AppPath);
                            app.StartInfo.FileName = Path.Combine(SilDev.Run.EnvironmentVariableFilter(JavaDir), Java["exe"]);
                            app.StartInfo.WorkingDirectory = Path.GetDirectoryName(AppPath);
                            app.Start();
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Sorry, something went wrong with the choosen Java app...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
#else
            SilDev.Log.ActivateDebug();
            SilDev.Initialization.File(Application.StartupPath, "JavaPortable.ini");
            Java.Add("exe", @"bin\javaw.exe");
            if (!JavaExists())
            {
                try
                {
                    string JavaPath = Application.StartupPath;
                    foreach (var dir in (!Application.StartupPath.Contains(@"\") ? string.Format(@"{0}\", Application.StartupPath) : Application.StartupPath).Split('\\'))
                    {
                        if (string.IsNullOrWhiteSpace(dir))
                            continue;
                        JavaPath = Path.Combine(string.Format(@"{0}\", JavaPath), dir);
                        string tmp = Path.Combine(string.Format(@"{0}\", JavaPath), @"CommonFiles\Java");
                        if (File.Exists(Path.Combine(tmp, Java["exe"])) && !SilDev.Run.Is64Bit(Path.Combine(tmp, Java["exe"])))
                        {
                            JavaPath = tmp.Replace(Application.StartupPath, "%CurrentDir%");
                            SilDev.Initialization.WriteValue("Directories", "Java", JavaPath);
                            throw new OperationCanceledException(string.Format("Java found in {0}.", JavaPath));
                        }
                    }
                    JavaPath = null;
                    MessageBox.Show("Please select the root folder of Java RE (x86) Portable.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    while (true)
                    {
                        using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                        {
                            dialog.Description = "Java Runtime Environments (x86):";
                            dialog.ShowDialog(new Form() { Icon = (System.Drawing.Icon)Properties.Resources.Java, TopMost = true });
                            string tmp = Path.Combine(dialog.SelectedPath, Java["exe"]);
                            if (File.Exists(tmp) && !SilDev.Run.Is64Bit(tmp))
                                JavaPath = dialog.SelectedPath.Replace(Application.StartupPath, "%CurrentDir%");
                            if (string.IsNullOrWhiteSpace(JavaPath))
                            {
                                if (MessageBox.Show("You wanna try another directory?", "Java RE (x86) NOT Found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                                    continue;
                            }
                        }
                        break;
                    }
                    if (!string.IsNullOrWhiteSpace(JavaPath))
                        SilDev.Initialization.WriteValue("Directories", "Java", JavaPath);
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }
            if (!JavaExists())
            {
                MessageBox.Show("Sorry, Java not found.", "Java App Launcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(-1);
            }
            if (File.Exists(SilDev.Initialization.File()))
            {
                string AppPath = null;
                if (Environment.GetCommandLineArgs().Length <= 1)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Filter = "Java Files (*.exe, *.jar)|*.exe;*.jar|All Files (*.*)|*.*";
                        dialog.Multiselect = false;
                        dialog.Title = "Select a Java Application";
                        dialog.ShowDialog(new Form() { ShowIcon = false, TopMost = true });
                        AppPath = dialog.FileName;
                    }
                }
                else
                {
                    if (File.Exists(Environment.GetCommandLineArgs()[1]))
                        AppPath = Environment.GetCommandLineArgs()[1];
                }
                try
                {
                    if (!string.IsNullOrWhiteSpace(AppPath))
                    {
                        string AppPathMD5 = SilDev.Crypt.MD5.Encrypt(AppPath);
                        if (string.IsNullOrWhiteSpace(SilDev.Initialization.ReadValue("Shortcuts", AppPathMD5)))
                        {
                            if (MessageBox.Show("You wanna create a desktop shortcut?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                try
                                {
                                    string name = Path.GetFileNameWithoutExtension(AppPath);
                                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), string.Format("{0}.lnk", name));
                                    IShellLink shell = (IShellLink)new ShellLink();
                                    shell.SetArguments(string.Format("\"{0}\"", AppPath));
                                    shell.SetDescription(string.Empty);
                                    shell.SetPath(Assembly.GetExecutingAssembly().Location);
                                    shell.SetWorkingDirectory(Application.StartupPath);
                                    IPersistFile file = (IPersistFile)shell;
                                    file.Save(path, false);
                                    MessageBox.Show(string.Format("Desktop shortcut for {0} created.", name), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                catch
                                {
                                    MessageBox.Show("Sorry, something went wrong. No shortcut created.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                            if (MessageBox.Show("Ask again for this file?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                                SilDev.Initialization.WriteValue("Shortcuts", AppPathMD5, "True");
                        }
                        using (Process app = new Process())
                        {
                            string JavaDir = SilDev.Initialization.ReadValue("Directories", "Java");
                            app.StartInfo.Arguments = string.Format("-jar \"{0}\"", AppPath);
                            app.StartInfo.FileName = Path.Combine(SilDev.Run.EnvironmentVariableFilter(JavaDir), Java["exe"]);
                            app.StartInfo.WorkingDirectory = Path.GetDirectoryName(AppPath);
                            app.Start();
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Sorry, something went wrong with the choosen Java app...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
#endif
        }

        private static bool JavaExists()
        {
#if !x86
            if (File.Exists(SilDev.Initialization.File()))
            {
                try
                {
                    string JavaPath = SilDev.Run.EnvironmentVariableFilter(SilDev.Initialization.ReadValue("Directories", "Java64"));
                    if (!Directory.Exists(JavaPath))
                        throw new OperationCanceledException("Directory not found.");
                    JavaPath = Path.Combine(JavaPath, Java["exe"]);
                    if (File.Exists(JavaPath))
                        if (SilDev.Run.Is64Bit(JavaPath))
                            return true;
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }
            return false;
#else
            if (File.Exists(SilDev.Initialization.File()))
            {
                try
                {
                    string JavaPath = SilDev.Run.EnvironmentVariableFilter(SilDev.Initialization.ReadValue("Directories", "Java"));
                    if (!Directory.Exists(JavaPath))
                        throw new OperationCanceledException("Directory not found.");
                    JavaPath = Path.Combine(JavaPath, Java["exe"]);
                    if (File.Exists(JavaPath))
                        if (!SilDev.Run.Is64Bit(JavaPath))
                            return true;
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }
            return false;
#endif
        }
    }
}
