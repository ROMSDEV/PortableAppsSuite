namespace DefragglerPortable
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

                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\Defraggler");
                if (!Directory.Exists(appDir))
                    return;

                var appPath = Path.Combine(appDir, "Defraggler.exe");
                var updaterPath = Path.Combine(appDir, "DefragglerUpdater.exe");
                if (!File.Exists(appPath) || ProcessEx.InstancesCount("Defraggler") > 0 || !File.Exists(updaterPath) || ProcessEx.InstancesCount("DefragglerUpdater") > 0)
                    return;

                CleanUpOld();
                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                var fileMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "defraggler.ini"),
                        PathEx.Combine(dataDir, "defraggler.ini")
                    },
                    {
                        PathEx.Combine(appDir, "statistics.bin"),
                        PathEx.Combine(dataDir, "statistics.bin")
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
            var appDir = PathEx.Combine(PathEx.LocalDir, "Defraggler");
            if (!Directory.Exists(appDir))
                return;
            var oldCfgPath = Path.Combine(appDir, "defraggler.ini");
            if (File.Exists(oldCfgPath))
            {
                if (!Directory.Exists(dataDir))
                    Directory.CreateDirectory(dataDir);
                File.Move(oldCfgPath, PathEx.Combine(dataDir, "defraggler.ini"));
            }
            var oldStsPath = Path.Combine(appDir, "statistics.bin");
            if (File.Exists(oldStsPath))
            {
                if (!Directory.Exists(dataDir))
                    Directory.CreateDirectory(dataDir);
                File.Move(oldStsPath, PathEx.Combine(dataDir, "statistics.bin"));
            }
            Directory.Delete(appDir, true);
        }
    }
}
