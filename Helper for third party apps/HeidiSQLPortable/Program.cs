namespace HeidiSQLPortable
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
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\HeidiSQL");
                if (!Directory.Exists(appDir))
                    return;

                var appPath = Path.Combine(appDir, "heidisql.exe");
                if (!newInstance)
                {
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false));
                    return;
                }

                var updaterPath = Path.Combine(appDir, "HeidiSQLUpdater.exe");
                if (ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)) || !File.Exists(updaterPath) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(updaterPath)))
                    return;

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                var dirMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine("%AppData%\\HeidiSQL"),
                        PathEx.Combine(dataDir, "HeidiSQL")
                    }
                };

                var fileMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "portable_settings.txt"),
                        PathEx.Combine(dataDir, "portable_settings.txt")
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

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);
                Helper.FileForwarding(Helper.Options.Start, fileMap, true);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);
                Helper.FileForwarding(Helper.Options.Exit, fileMap, true);
            }
        }
    }
}
