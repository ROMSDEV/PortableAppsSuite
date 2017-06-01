namespace RecuvaPortable
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

                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\Recuva");
                if (!Directory.Exists(appDir))
                    return;

                var appPath = Path.Combine(appDir, "Recuva.exe");
                var updaterPath = Path.Combine(appDir, "RecuvaUpdater.exe");
                if (ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(appPath)) > 0 || !File.Exists(updaterPath) || ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(updaterPath)) > 0)
                    return;

                CleanUpOld();
                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                var fileMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "recuva.ini"),
                        PathEx.Combine(dataDir, "recuva.ini")
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

        private static void CleanUpOld()
        {
            var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
            var appDir = PathEx.Combine(PathEx.LocalDir, "Recuva");
            if (!Directory.Exists(appDir))
                return;
            var oldCfgPath = Path.Combine(appDir, "recuva.ini");
            if (File.Exists(oldCfgPath))
            {
                if (!Directory.Exists(dataDir))
                    Directory.CreateDirectory(dataDir);
                File.Move(oldCfgPath, PathEx.Combine(dataDir, "recuva.ini"));
            }
            Directory.Delete(appDir, true);
        }
    }
}
