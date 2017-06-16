namespace ZAPPortable
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Portable;
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

                    var appDir = PathEx.Combine(PathEx.LocalDir, "App\\ZedAttackProxy");
                    var appPaths = Directory.GetFiles(appDir, "zap-*.jar", SearchOption.TopDirectoryOnly).ToList();
                    appPaths.Sort();
                    var appPath = appPaths.Last();

                    if (!File.Exists(appPath) || IsRunning())
                        return;

                    var iniPath = Path.ChangeExtension(PathEx.LocalPath, ".ini");
                    string javaPath;
                    Helper.FindJava(out javaPath, iniPath);

                    var dirMap = new Dictionary<string, string>
                    {
                        {
                            "%UserProfile%\\OWASP ZAP",
                            "%CurDir%\\Data"
                        }
                    };
                    Helper.DirectoryForwarding(Helper.Options.Start, dirMap);

                    var args = $"-Xmx512m -XX:PermSize=256M -jar \"{appPath}\" {EnvironmentEx.CommandLine(false)}".Trim();
                    using (var p = ProcessEx.Start(javaPath, Path.GetDirectoryName(appPath), args, false, false))
                        if (p?.HasExited == false)
                            p.WaitForExit();
                    var isRunning = true;
                    while (isRunning)
                    {
                        isRunning = IsRunning();
                        Thread.Sleep(100);
                    }

                    Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);
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
                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data\\plugin");
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
    }
}
