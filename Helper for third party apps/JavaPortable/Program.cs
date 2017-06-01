namespace JavaPortableLauncher
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;
    using SilDev;

    internal static class Program
    {
#if !x86
        private const string JavaVar = "Java64";
#else
        private const string JavaVar = "Java";
#endif
        private static readonly Dictionary<string, string> Java = new Dictionary<string, string>();

        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            Ini.SetFile("%CurDir%\\JavaPortable.ini");
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Ini.WriteAll();
            Java.Add("exe", "bin\\javaw.exe");
            if (!JavaExists())
                try
                {
                    var drive = new DriveInfo(EnvironmentEx.GetVariableValue("CurDir")).RootDirectory.Root.Name;
                    var javaDir = drive;
                    string javaPath = null;
                    var envValue = EnvironmentEx.GetVariableValue("CurDir");
                    foreach (var dirName in envValue.Split('\\'))
                        try
                        {
                            if (drive.Contains(dirName))
                                continue;
                            javaDir = Path.Combine(javaDir, dirName);
                            if (!File.Exists(Path.Combine(javaDir, $"CommonFiles\\{JavaVar}\\{Java["exe"]}")))
                                continue;
                            javaDir = Path.Combine(javaDir, $"CommonFiles\\{JavaVar}");
                            javaPath = Path.Combine(javaDir, Java["exe"]);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }

                    if (File.Exists(javaPath))
                    {
                        Ini.Write("Location", JavaVar, javaDir.Replace(envValue, "%CurDir%"));
                        throw new OperationCanceledException($"Java found in '{javaDir}'.");
                    }

                    MessageBox.Show($@"Please select the root folder of Java RE ({(Environment.Is64BitProcess ? "x64" : "x86")}) Portable.", @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    while (true)
                    {
                        using (var dialog = new FolderBrowserDialog())
                        {
                            dialog.Description = $@"Java Runtime Environments ({(Environment.Is64BitProcess ? "x64" : "x86")}):";
                            dialog.ShowDialog(new Form { ShowIcon = false, TopMost = true });
                            var tmp = Path.Combine(dialog.SelectedPath, Java["exe"]);
                            if (File.Exists(tmp) && PathEx.FileIs64Bit(tmp))
                                javaDir = dialog.SelectedPath.Replace(envValue, "%CurDir%");
                            var tmpPath = Path.Combine(javaDir, Java["exe"]);
                            if (string.IsNullOrWhiteSpace(javaDir) || !File.Exists(tmpPath))
                                if (MessageBox.Show(@"Java Portable not found!", $@"Java RE ({(Environment.Is64BitProcess ? "x64" : "x86")}) NOT Found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                                    continue;
                        }
                        break;
                    }
                    if (!string.IsNullOrWhiteSpace(javaDir))
                        Ini.Write("Location", JavaVar, javaDir);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            if (!JavaExists())
            {
                MessageBox.Show(@"Java Portable not found!", $@"Java Portable{(Environment.Is64BitProcess ? " (x64)" : string.Empty)}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(1);
            }
            if (!File.Exists(Ini.FilePath))
                return;
            string appPath = null;
            if (EnvironmentEx.CommandLineArgs(false).Count < 1)
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = @"Java Files (*.exe, *.jar)|*.exe;*.jar|All Files (*.*)|*.*";
                    dialog.Multiselect = false;
                    dialog.Title = @"Select a Java Application";
                    dialog.ShowDialog(new Form
                    {
                        ShowIcon = false,
                        TopMost = true
                    });
                    appPath = dialog.FileName;
                }
            }
            else
            {
                if (File.Exists(EnvironmentEx.CommandLineArgs(false)[0]))
                    appPath = EnvironmentEx.CommandLineArgs(false)[0];
            }
            try
            {
                if (string.IsNullOrWhiteSpace(appPath))
                    return;
                var appPathMd5 = appPath.EncryptToMd5();
                if (string.IsNullOrWhiteSpace(Ini.Read("Shortcuts", appPathMd5)))
                {
                    if (MessageBox.Show(@"Java Portable not found!", @"Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        try
                        {
                            var name = Path.GetFileNameWithoutExtension(appPath);
                            if (!Data.CreateShortcut(Application.ExecutablePath, $"%DesktopDir%\\{name}.lnk", EnvironmentEx.CommandLineArgs(false).Count > 0 ? EnvironmentEx.CommandLine(false) : $"\"{appPath}\""))
                                throw new Exception();
                            MessageBox.Show($@"Desktop shortcut for {name} created.", @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch
                        {
                            MessageBox.Show(@"Java Portable not found!", @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    if (MessageBox.Show(@"Java Portable not found!", @"Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        Ini.Write("Shortcuts", appPathMd5, "True");
                }
                var jreUsageDir = PathEx.Combine("%UserProfile%\\.oracle_jre_usage");
                if (!Directory.Exists(jreUsageDir))
                    Directory.CreateDirectory(jreUsageDir);
                Data.SetAttributes(jreUsageDir, FileAttributes.Hidden);
                var filePath = Path.Combine(PathEx.Combine(Ini.Read("Location", JavaVar)), Java["exe"]);
                var workDir = Path.Combine(PathEx.Combine(Ini.Read("Location", JavaVar)), Java["exe"]);
                var args = $"-jar {(EnvironmentEx.CommandLineArgs(false).Count > 0 ? EnvironmentEx.CommandLine(false).Trim() : $"\"{appPath}\"")}";
                using (var p = ProcessEx.Start(filePath, workDir, args, false, false))
                {
                    if (p.Id <= 0)
                        throw new NotSupportedException();
                    if (!p.HasExited)
                        p.WaitForExit();
                }
            }
            catch
            {
                MessageBox.Show(@"Java Portable not found!", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool JavaExists()
        {
            if (!File.Exists(Ini.FilePath))
                return false;
            try
            {
                var javaPath = PathEx.Combine(Ini.Read("Location", JavaVar));
                if (!Directory.Exists(javaPath))
                    throw new PathNotFoundException(javaPath);
                javaPath = Path.Combine(javaPath, Java["exe"]);
                return File.Exists(javaPath);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return false;
        }
    }
}
