namespace Prime95Portable
{
    using System;
    using System.Diagnostics;
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
            using (new Mutex(true, ProcessEx.CurrentName, out bool newInstance))
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

#if LEGACY
                if (!File.Exists(appPath) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)))
                    return;
#else
                var updaterPath = Path.Combine(appDir, Environment.Is64BitOperatingSystem ? "Prime95Updater64.exe" : "Prime95Updater.exe");
                if (ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)) || !File.Exists(updaterPath) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(updaterPath)))
                    return;

                Helper.ApplicationStart(updaterPath, "/silent", null);
                if (!File.Exists(appPath))
                {
                    var updIniPath = Path.ChangeExtension(updaterPath, ".ini");
                    if (!string.IsNullOrEmpty(updIniPath) && File.Exists(updIniPath))
                        File.Delete(updIniPath);
                    return;
                }
#endif

                try
                {
                    var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                    var cfgPath = PathEx.Combine(appDir, "prime.txt");
                    if (!File.Exists(cfgPath))
                        File.Create(cfgPath).Close();
                    if (!File.ReadAllText(cfgPath).EqualsEx(dataDir))
                        File.WriteAllText(cfgPath, $"WorkingDir={dataDir}");
                    cfgPath = PathEx.Combine(dataDir, "prime.txt");
                    if (!File.Exists(cfgPath))
                    {
                        if (!Directory.Exists(dataDir))
                            Directory.CreateDirectory(dataDir);
                        File.WriteAllText(cfgPath, "TrayIcon=0");
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), ProcessWindowStyle.Maximized, false);
            }
        }
    }
}
