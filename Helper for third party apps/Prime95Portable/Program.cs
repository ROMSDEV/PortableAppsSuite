namespace Prime95Portable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Portable;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                var appDir = PathEx.Combine(PathEx.LocalDir, "App", Environment.Is64BitOperatingSystem ? "p95win64" : "p95win32");
                if (!Directory.Exists(appDir))
                    return;
                var appPath = Path.Combine(appDir, "prime95.exe");

                if (!newInstance)
                {
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false));
                    return;
                }

                var updaterPath = Path.Combine(appDir, Environment.Is64BitOperatingSystem ? "Prime95Updater64.exe" : "Prime95Updater.exe");
                if (ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(appPath)) > 0 || !File.Exists(updaterPath) || ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(updaterPath)) > 0)
                    return;

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                var cfgPath = PathEx.Combine(dataDir, "prime.txt");
                var fileMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "prime.txt"),
                        cfgPath
                    }
                };

                Helper.ApplicationStart(updaterPath, "/silent", null);
                if (!File.Exists(appPath))
                {
                    var updIniPath = Path.ChangeExtension(updaterPath, ".ini");
                    if (!string.IsNullOrEmpty(updIniPath) && File.Exists(updIniPath))
                        File.Delete(updIniPath);
                    return;
                }

                if (!File.Exists(cfgPath))
                {
                    if (!Directory.Exists(dataDir))
                        Directory.CreateDirectory(dataDir);
                    File.WriteAllText(cfgPath, "TrayIcon=0");
                }

                Helper.FileForwarding(Helper.Options.Start, fileMap);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.FileForwarding(Helper.Options.Exit, fileMap);
            }
        }
    }
}
