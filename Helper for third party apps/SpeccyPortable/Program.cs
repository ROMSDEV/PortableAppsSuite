namespace SpeccyPortable
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

                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\Speccy");
                if (!Directory.Exists(appDir))
                    return;

                var appPath = Path.Combine(appDir, "Speccy.exe");
                var updaterPath = Path.Combine(appDir, "SpeccyUpdater.exe");
                if (ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(appPath)) > 0 || !File.Exists(updaterPath) || ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(updaterPath)) > 0)
                    return;

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                var fileMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "speccy.ini"),
                        PathEx.Combine(dataDir, "speccy.ini")
                    }
                };

                Helper.ApplicationStart(updaterPath, "/silent", null);

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
