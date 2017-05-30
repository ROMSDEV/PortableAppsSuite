namespace ResourceHackerPortable
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

                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\ResourceHacker");
                if (!Directory.Exists(appDir))
                    return;

                var appPath = Path.Combine(appDir, "ResourceHacker.exe");
                var updaterPath = Path.Combine(appDir, "ResourceHackerUpdater.exe");
                if (!File.Exists(appPath) || ProcessEx.InstancesCount("ResourceHacker") > 0 || !File.Exists(updaterPath) || ProcessEx.InstancesCount("ResourceHackerUpdater") > 0)
                    return;

                CleanUpOld();
                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                var fileMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "ResourceHacker.ini"),
                        PathEx.Combine(dataDir, "ResourceHacker.ini")
                    }
                };
                Helper.FileForwarding(Helper.Options.Start, fileMap);

                Helper.ApplicationStart(updaterPath, "/silent", false);
                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.FileForwarding(Helper.Options.Exit, fileMap);
            }
        }

        private static void CleanUpOld()
        {
            var dataMap = new[]
            {
                "Help",
                "changes.txt",
                "ReadMe.txt",
                "ResourceHacker.def",
                "ResourceHacker.exe",
                "ResourceHacker.ini",
                "ResourceHacker.log",
                "ResourceHackerPortable.ini"
            };
            foreach (var data in dataMap)
            {
                var path = PathEx.Combine(PathEx.LocalDir, data);
                if (!PathEx.DirOrFileExists(path))
                    continue;
                try
                {
                    if (!Path.HasExtension(path))
                        Directory.Delete(path, true);
                    else
                        File.Delete(path);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }
    }
}