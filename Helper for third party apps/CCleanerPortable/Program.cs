namespace CCleanerPortable
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
                if (!newInstance)
                    return;

                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\CCleaner");
                if (!Directory.Exists(appDir))
                    return;

                var appPath = Path.Combine(appDir, "CCleaner.exe");
                var updaterPath = Path.Combine(appDir, "CCleanerUpdater.exe");
                if (ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)) || !File.Exists(updaterPath) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(updaterPath)))
                    return;

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                var fileMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "ccleaner.ini"),
                        PathEx.Combine(dataDir, "ccleaner.ini")
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

                Helper.FileForwarding(Helper.Options.Start, fileMap);

                var portableDat = Path.Combine(appDir, "portable.dat");
                if (!File.Exists(portableDat))
                    File.WriteAllText(portableDat, @"#PORTABLE#");

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.FileForwarding(Helper.Options.Exit, fileMap);
            }
        }
    }
}
