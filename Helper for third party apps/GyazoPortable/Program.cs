namespace GyazoPortable
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
            using (new Mutex(true, ProcessEx.CurrentName, out bool newInstance))
            {
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\Gyazo");
                if (!Directory.Exists(appDir))
                    return;

                var appPath = Path.Combine(appDir, "Gyazowin.exe");
                var trayMode = Environment.CommandLine.ContainsEx("/Tray") || Ini.Read("Settings", "Tray", false, Path.ChangeExtension(PathEx.LocalPath, ".ini"));

                if (!newInstance)
                {
                    if (trayMode)
                        ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false));
                    return;
                }

                if (trayMode)
                    appPath = Path.Combine(appDir, "GyStation.exe");
                var updaterPath = Path.Combine(appDir, "GyazoUpdater.exe");
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

                var dirMap = new Dictionary<string, string>
                {
                    {
                        "%AppData%\\Gyazo",
                        "%CurDir%\\Data"
                    }
                };

                var regKeys = new[]
                {
                    "HKCU\\Software\\Gyazo"
                };

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);
                Helper.RegForwarding(Helper.Options.Start, regKeys);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);
                Helper.RegForwarding(Helper.Options.Exit, regKeys);
            }
        }
    }
}
