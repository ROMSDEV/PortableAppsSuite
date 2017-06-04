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
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\ResourceHacker");
                if (!Directory.Exists(appDir))
                    return;
                var appPath = Path.Combine(appDir, "ResourceHacker.exe");

                if (!newInstance)
                {
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false));
                    return;
                }

                var updaterPath = Path.Combine(appDir, "ResourceHackerUpdater.exe");
                if (ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(appPath)) > 0 || !File.Exists(updaterPath) || ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(updaterPath)) > 0)
                    return;

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                var fileMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "ResourceHacker.ini"),
                        PathEx.Combine(dataDir, "ResourceHacker.ini")
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

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.FileForwarding(Helper.Options.Exit, fileMap);
            }
        }
    }
}
