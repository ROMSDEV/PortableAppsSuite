namespace FurMarkPortable
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
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\FurMark");
                if (!Directory.Exists(appDir))
                    return;

                var appPath = Path.Combine(appDir, "FurMark.exe");

                if (!newInstance)
                {
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false), Elevation.IsAdministrator);
                    return;
                }

                CleanUpHelper();

                var updaterPath = PathEx.Combine(appDir, "FurMarkUpdater.exe");
                if (ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(appPath)) > 0 || !File.Exists(updaterPath) || ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(updaterPath)) > 0)
                    return;

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");

                var dirMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "screenshots"),
                        PathEx.Combine(dataDir, "screenshots")
                    }
                };

                var fileMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "furmark-gpu-monitoring.csv"),
                        PathEx.Combine(dataDir, "furmark-gpu-monitoring.csv")
                    },
                    {
                        PathEx.Combine(appDir, "furmark-gpu-monitoring.xml"),
                        PathEx.Combine(dataDir, "furmark-gpu-monitoring.xml")
                    },
                    {
                        PathEx.Combine(appDir, "startup_options.xml"),
                        PathEx.Combine(dataDir, "startup_options.xml")
                    }
                };
                try
                {
                    foreach (var file in Directory.EnumerateFiles(appDir, "*.log", SearchOption.TopDirectoryOnly))
                    {
                        var name = Path.GetFileName(file);
                        fileMap.Add(file, PathEx.Combine(dataDir, name));
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);

                Helper.FileForwarding(Helper.Options.Start, fileMap, true);

                Helper.ApplicationStart(updaterPath, "/silent", null);
                if (!File.Exists(appPath))
                {
                    var updIniPath = Path.ChangeExtension(updaterPath, ".ini");
                    if (!string.IsNullOrEmpty(updIniPath) && File.Exists(updIniPath))
                        File.Delete(updIniPath);
                    return;
                }

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);

                Helper.FileForwarding(Helper.Options.Exit, fileMap, true);
            }
        }

        private static void CleanUpHelper()
        {
            var appDir = PathEx.Combine(PathEx.LocalDir, "FurMark");
            if (!Directory.Exists(appDir))
                return;
            try
            {
                Directory.Delete(appDir, true);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}
