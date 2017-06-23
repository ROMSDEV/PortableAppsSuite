namespace TeamViewerPortable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                if (!newInstance)
                    return;

                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\TeamViewer");
                var appPath = Path.Combine(appDir, "TeamViewer.exe");
                var updaterPath = Path.Combine(appDir, "TeamViewerUpdater.exe");
                var processes = new[]
                {
                    "TeamViewer",
                    "TeamViewer_Desktop",
                    "tv_w32",
                    "tv_x64",
                    "TeamViewerUpdater"
                };
                if (!File.Exists(updaterPath) || processes.Any(name => ProcessEx.IsRunning(name)))
                    return;

                Helper.ApplicationStart(updaterPath, "/silent", null);
                if (!File.Exists(appPath))
                {
                    var updIniPath = Path.ChangeExtension(updaterPath, ".ini");
                    if (!string.IsNullOrEmpty(updIniPath) && File.Exists(updIniPath))
                        File.Delete(updIniPath);
                    return;
                }

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                if (!Directory.Exists(dataDir))
                    Directory.CreateDirectory(dataDir);

                var fileMap = new Dictionary<string, string>
                {
                    {
                        "%CurDir%\\App\\TeamViewer\\TeamViewer.ini",
                        "%CurDir%\\Data\\config.ini"
                    },
                    {
                        "%CurDir%\\App\\TeamViewer\\tv.ini",
                        "%CurDir%\\Data\\settings.ini"
                    }
                };

                var configPath = PathEx.Combine(fileMap.First().Key);
                if (File.Exists(configPath))
                    File.Delete(configPath);
                configPath = PathEx.Combine(fileMap.First().Value);
                if (!File.Exists(configPath))
                    File.WriteAllText(configPath, Resources.config);
                Ini.WriteDirect("Settings", "nosave", 1, configPath);
                Ini.WriteDirect("Settings", "importsettings", 1, configPath);

                var settingsPath = PathEx.Combine(fileMap.Last().Key);
                if (File.Exists(settingsPath))
                    File.Delete(settingsPath);
                settingsPath = PathEx.Combine(fileMap.Last().Value);
                if (!File.Exists(settingsPath))
                    File.WriteAllText(settingsPath, Resources.settings);

                Helper.FileForwarding(Helper.Options.Start, fileMap);

                var dirMap = new Dictionary<string, string>
                {
                    {
                        "%AppData%\\TeamViewer",
                        "%CurDir%\\Data\\logs"
                    }
                };

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);

                const string regKey = "HKLM\\SOFTWARE\\TeamViewer";

                Helper.RegForwarding(Helper.Options.Start, regKey);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false));

                Helper.FileForwarding(Helper.Options.Exit, fileMap);

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);

                Helper.RegForwarding(Helper.Options.Exit, regKey);
            }
        }
    }
}
