namespace HWMonitorPortable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Portable;
    using Properties;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            using (new Mutex(true, ProcessEx.CurrentName, out bool newInstance))
            {
                if (!newInstance)
                    return;

                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\hwmon");
                if (!Directory.Exists(appDir))
                    return;

                var appPath = Path.Combine(appDir, Environment.CommandLine.ContainsEx("/Tray") ? "HwMonTray.exe" : (Environment.Is64BitOperatingSystem ? "HWMonitor_x64.exe" : "HWMonitor_x32.exe"));
                var updaterPath = Path.Combine(appDir, "HWMonitorUpdater.exe");
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

                const string iniPath = "%CurDir%\\Data\\HWMonitorW.ini";
                var iniMap = new Dictionary<string, Dictionary<string, string>>
                {
                    {
                        "HWMonitor", new Dictionary<string, string>
                        {
                            {
                                "UPDATES",
                                "0"
                            }
                        }
                    }
                };

                var fileMap = new Dictionary<string, string>
                {
                    {
                        "%CurDir%\\App\\hwmon\\HWMonitorW.ini",
                        "%CurDir%\\Data\\HWMonitorW.ini"
                    }
                };

                Helper.ConfigOverwrite(iniMap, iniPath);

                Helper.FileForwarding(Helper.Options.Start, fileMap, true);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.FileForwarding(Helper.Options.Exit, fileMap, true);
            }
        }
    }
}
