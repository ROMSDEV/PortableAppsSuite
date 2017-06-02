namespace ZAPPortable
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            try
            {
                bool newInstance;
                using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
                {
                    if (!newInstance)
                        return;

                    var appDir = PathEx.Combine("%CurDir%\\App\\ZedAttackProxy");
                    var appPaths = Directory.GetFiles(appDir, "zap-*.jar", SearchOption.TopDirectoryOnly).ToList();
                    appPaths.Sort();
                    var appPath = appPaths.Last();

                    if (!File.Exists(appPath) || IsRunning())
                        return;

                    var javaPath = GetJavaPath();
                    if (string.IsNullOrWhiteSpace(javaPath))
                        return;

                    var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                    if (!Directory.Exists(dataDir))
                        Directory.CreateDirectory(dataDir);
                    var userProfile = PathEx.Combine("%UserProfile%", "OWASP ZAP");
                    Data.DirLink(userProfile, dataDir, true);

                    using (var p = ProcessEx.Start(javaPath, Path.GetDirectoryName(appPath), $"-Xmx512m -XX:PermSize=256M -jar \"{appPath}\" {EnvironmentEx.CommandLine(false)}", false, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    var isRunning = true;
                    while (isRunning)
                    {
                        isRunning = IsRunning();
                        Thread.Sleep(100);
                    }

                    Data.DirUnLink(userProfile, true);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private static bool IsRunning()
        {
            try
            {
                var isRunning = Process.GetProcesses().Any(p => p.MainWindowHandle != IntPtr.Zero && p.MainWindowTitle.ContainsEx("OWASP ZAP"));
                if (isRunning)
                    return true;
                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data", "plugin");
                foreach (var file in Directory.GetFiles(dataDir, "ZAP*windows.exe", SearchOption.AllDirectories))
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    isRunning = Process.GetProcessesByName(name).Length > 0;
                    if (isRunning)
                        break;
                }
                return isRunning;
            }
            catch
            {
                return false;
            }
        }

        private static string GetJavaPath()
        {
            try
            {
                string path;
                var appDrive = new DriveInfo(PathEx.LocalDir).RootDirectory.Root.Name;
                var javaPath = appDrive;
                foreach (var dirName in PathEx.LocalDir.Split('\\').Skip(1))
                {
                    if (appDrive.Contains(dirName))
                        continue;
                    const string template = @"CommonFiles\\Java{0}\\bin\\javaw.exe";
                    javaPath = Path.Combine(javaPath, dirName);
                    if (Environment.Is64BitOperatingSystem)
                    {
                        path = Path.Combine(javaPath, "Java64Portable", string.Format(template, 64));
                        if (File.Exists(path))
                        {
                            javaPath = path;
                            break;
                        }
                        path = Path.Combine(javaPath, string.Format(template, 64));
                        if (File.Exists(path))
                        {
                            javaPath = path;
                            break;
                        }
                    }
                    path = Path.Combine(javaPath, "Java64Portable", string.Format(template, string.Empty));
                    if (File.Exists(path))
                    {
                        javaPath = path;
                        break;
                    }
                    path = Path.Combine(javaPath, "JavaPortable", string.Format(template, string.Empty));
                    if (File.Exists(path))
                    {
                        javaPath = path;
                        break;
                    }
                    path = Path.Combine(javaPath, string.Format(template, string.Empty));
                    if (!File.Exists(path))
                        continue;
                    javaPath = path;
                    break;
                }
                if (File.Exists(javaPath))
                    return javaPath;
                path = PathEx.Combine("%ProgramFiles%", "Java");
                path = Directory.GetFiles(path, "javaw.exe", SearchOption.AllDirectories).FirstOrDefault();
                if (File.Exists(path))
                {
                    javaPath = path;
                    return javaPath;
                }
                if (!Environment.Is64BitOperatingSystem)
                    return javaPath;
                path = PathEx.Combine("%ProgramFilesX86%", "Java");
                path = Directory.GetFiles(path, "javaw.exe", SearchOption.AllDirectories).FirstOrDefault();
                javaPath = path;
                return javaPath;
            }
            catch
            {
                MessageBox.Show(@"Java Portable not found!", @"Zed Attack Proxy Portable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
