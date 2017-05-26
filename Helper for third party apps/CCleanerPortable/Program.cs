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
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                if (!newInstance)
                    return;

                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\CCleaner");
                if (!Directory.Exists(appDir))
                    return;

                var appPath = Path.Combine(appDir, "CCleaner.exe");
                var updaterPath = Path.Combine(appDir, "CCleanerUpdater.exe");
                if (!File.Exists(appPath) || ProcessEx.InstancesCount("CCleaner") > 0 || !File.Exists(updaterPath) || ProcessEx.InstancesCount("CCleanerUpdater") > 0)
                    return;

                CleanUpOld();
                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                var fileMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "ccleaner.ini"),
                        PathEx.Combine(dataDir, "ccleaner.ini")
                    }
                };
                Helper.FileForwarding(Helper.Options.Start, fileMap);

                var portableDat = Path.Combine(appDir, "portable.dat");
                if (!File.Exists(portableDat))
                    File.WriteAllText(portableDat, @"#PORTABLE#");

                Helper.ApplicationStart(updaterPath, "/silent", false);
                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.FileForwarding(Helper.Options.Exit, fileMap);
            }
        }

        private static void CleanUpOld()
        {
            var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
            var appDir = PathEx.Combine(PathEx.LocalDir, "CCleaner");
            if (!Directory.Exists(appDir))
                return;
            var oldCfgPath = Path.Combine(appDir, "ccleaner.ini");
            if (File.Exists(oldCfgPath))
            {
                if (!Directory.Exists(dataDir))
                    Directory.CreateDirectory(dataDir);
                File.Move(oldCfgPath, PathEx.Combine(dataDir, "ccleaner.ini"));
            }
            Directory.Delete(appDir, true);
        }
    }
}
