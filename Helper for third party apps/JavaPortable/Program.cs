using SilDev;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace JavaPortableLauncher
{
    static class Program
    {

#if !x86
        private static string JavaVar = "Java64";
#else
        private static string JavaVar = "Java";
#endif
        private static Dictionary<string, string> Java = new Dictionary<string, string>();

        [STAThread]
        static void Main()
        {
            LOG.AllowDebug();
            INI.File(PATH.GetEnvironmentVariableValue("CurDir"), "JavaPortable.ini");
            Java.Add("exe", "bin\\javaw.exe");
            if (!JavaExists())
            {
                try
                {
                    string drive = new DriveInfo(PATH.GetEnvironmentVariableValue("CurDir")).RootDirectory.Root.Name;
                    string JavaDir = drive;
                    string JavaPath = null;
                    foreach (string dirName in PATH.GetEnvironmentVariableValue("CurDir").Split('\\'))
                    {
                        try
                        {
                            if (drive.Contains(dirName))
                                continue;
                            JavaDir = Path.Combine(JavaDir, dirName);
                            if (File.Exists(Path.Combine(JavaDir, $"CommonFiles\\{JavaVar}\\{Java["exe"]}")))
                            {
                                JavaDir = Path.Combine(JavaDir, $"CommonFiles\\{JavaVar}");
                                JavaPath = Path.Combine(JavaDir, Java["exe"]);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                    }

                    if (File.Exists(JavaPath))
                    {
                        INI.Write("Location", JavaVar, JavaDir.Replace(PATH.GetEnvironmentVariableValue("CurDir"), "%CurDir%"));
                        throw new OperationCanceledException($"Java found in '{JavaDir}'.");
                    }

                    MessageBox.Show($"Please select the root folder of Java RE ({(Environment.Is64BitProcess ? "x64" : "x86")}) Portable.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    while (true)
                    {
                        using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                        {
                            dialog.Description = $"Java Runtime Environments ({(Environment.Is64BitProcess ? "x64" : "x86")}):";
                            dialog.ShowDialog(new Form() { ShowIcon = false, TopMost = true });
                            string tmp = Path.Combine(dialog.SelectedPath, Java["exe"]);
                            if (File.Exists(tmp) && RUN.Is64Bit(tmp))
                                JavaDir = dialog.SelectedPath.Replace(PATH.GetEnvironmentVariableValue("CurDir"), "%CurDir%");
                            string tmpPath = Path.Combine(JavaDir, Java["exe"]);
                            if (string.IsNullOrWhiteSpace(JavaDir) || !File.Exists(tmpPath))
                            {
                                if (MessageBox.Show("You wanna try another directory?", $"Java RE ({(Environment.Is64BitProcess ? "x64" : "x86")}) NOT Found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                                    continue;
                            }
                        }
                        break;
                    }
                    if (!string.IsNullOrWhiteSpace(JavaDir))
                        INI.Write("Location", JavaVar, JavaDir);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }
            if (!JavaExists())
            {
                MessageBox.Show("Sorry, Java not found.", $"Java Portable{(Environment.Is64BitProcess ? " (x64)" : string.Empty)}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(-1);
            }
            if (File.Exists(INI.File()))
            {
                string AppPath = null;
                if (RUN.CommandLineArgs(false).Count < 1)
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
                    if (File.Exists(RUN.CommandLineArgs(false)[0]))
                        AppPath = RUN.CommandLineArgs(false)[0];
                }
                try
                {
                    if (!string.IsNullOrWhiteSpace(AppPath))
                    {
                        string AppPathMD5 = CRYPT.MD5.EncryptString(AppPath);
                        if (string.IsNullOrWhiteSpace(INI.Read("Shortcuts", AppPathMD5)))
                        {
                            if (MessageBox.Show("You wanna create a desktop shortcut?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                try
                                {
                                    string name = Path.GetFileNameWithoutExtension(AppPath);
                                    if (!DATA.CreateShortcut(Application.ExecutablePath, $"%DesktopDir%\\{name}.lnk", RUN.CommandLineArgs(false).Count > 0 ? RUN.CommandLine(false) : $"\"{AppPath}\""))
                                        throw new Exception();
                                    MessageBox.Show(string.Format("Desktop shortcut for {0} created.", name), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                catch
                                {
                                    MessageBox.Show("Sorry, something went wrong. No shortcut created.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                            if (MessageBox.Show("Ask again for this file?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                                INI.Write("Shortcuts", AppPathMD5, "True");
                        }
                        string jreUsageDir = PATH.Combine("%UserProfile%\\.oracle_jre_usage");
                        if (!Directory.Exists(jreUsageDir))
                            Directory.CreateDirectory(jreUsageDir);
                        DATA.SetAttributes(jreUsageDir, FileAttributes.Hidden);
                        int pid = RUN.App(new ProcessStartInfo()
                        {
                            Arguments = $"-jar {(RUN.CommandLineArgs(false).Count > 0 ? RUN.CommandLine(false) : $"\"{AppPath}\"")}",
                            FileName = Path.Combine(PATH.Combine(INI.Read("Location", JavaVar)), Java["exe"]),
                            WorkingDirectory = Path.GetDirectoryName(AppPath)
                        });
                        if (pid < 0)
                            throw new Exception();
                    }
                }
                catch
                {
                    MessageBox.Show("Sorry, something went wrong with the choosen Java app...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static bool JavaExists()
        {
            if (File.Exists(INI.File()))
            {
                try
                {
                    string JavaPath = PATH.Combine(INI.Read("Location", JavaVar));
                    if (!Directory.Exists(JavaPath))
                        throw new OperationCanceledException("Path not found.");
                    JavaPath = Path.Combine(JavaPath, Java["exe"]);
                    return File.Exists(JavaPath);
                }
                catch (Exception ex)
                {
                    LOG.Debug(ex);
                }
            }
            return false;
        }
    }
}
