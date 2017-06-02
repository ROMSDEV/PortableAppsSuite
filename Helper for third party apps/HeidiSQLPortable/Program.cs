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
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
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
                if (ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(appPath)) > 0 || !File.Exists(updaterPath) || ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(updaterPath)) > 0)
                    return;

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                CleanUpHelper(dataDir);

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

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);
                Helper.FileForwarding(Helper.Options.Start, fileMap, true);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);
                Helper.FileForwarding(Helper.Options.Exit, fileMap, true);
            }
        }

        private static void CleanUpHelper(string dataDir)
        {
            var appDir = PathEx.Combine(PathEx.LocalDir, "HeidiSQL");
            if (!Directory.Exists(appDir))
                return;
            var oldCfgPath = Path.Combine(appDir, "portable_settings.txt");
            if (File.Exists(oldCfgPath))
            {
                if (!Directory.Exists(dataDir))
                    Directory.CreateDirectory(dataDir);
                File.Move(oldCfgPath, PathEx.Combine(dataDir, "portable_settings.txt"));
            }
            Directory.Delete(appDir, true);
        }
    }
}
