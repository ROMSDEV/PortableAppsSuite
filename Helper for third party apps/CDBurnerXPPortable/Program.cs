namespace CDBurnerXPPortable
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
#if x86
                var curPath64 = PathEx.Combine(PathEx.LocalDir, "CDBurnerXP64Portable.exe");
                if (Environment.Is64BitOperatingSystem && File.Exists(curPath64))
                {
                    ProcessEx.Start(curPath64, EnvironmentEx.CommandLine(false));
                    return;
                }
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\CDBurnerXP");
#else
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\CDBurnerXP64");
#endif
                if (!Directory.Exists(appDir))
                    return;

                var appPath = Path.Combine(appDir, "cdbxpp.exe");
#if x86
                var updaterPath = PathEx.Combine(appDir, "CDBurnerXPUpdater.exe");
#else
                var updaterPath = PathEx.Combine(appDir, "CDBurnerXPUpdater64.exe");
#endif
                if (ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(appPath)) > 0 || !File.Exists(updaterPath) || ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(updaterPath)) > 0)
                    return;

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                CleanUpHelper(dataDir);
                var fileMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "UserSettings.ini"),
                        PathEx.Combine(dataDir, "UserSettings.ini")
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

                Helper.FileForwarding(Helper.Options.Start, fileMap, true);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.FileForwarding(Helper.Options.Exit, fileMap, true);
            }
        }

        private static void CleanUpHelper(string dataDir)
        {
#if x86
            var appDir = PathEx.Combine(PathEx.LocalDir, "CDBurnerXP");
#else
            var appDir = PathEx.Combine(PathEx.LocalDir, "CDBurnerXP64");
#endif
            if (!Directory.Exists(appDir))
                return;
            try
            {
                var oldCfgPath = Path.Combine(appDir, "UserSettings.ini");
                if (File.Exists(oldCfgPath))
                {
                    if (!Directory.Exists(dataDir))
                        Directory.CreateDirectory(dataDir);
                    File.Move(oldCfgPath, PathEx.Combine(dataDir, "UserSettings.ini"));
                }
                Directory.Delete(appDir, true);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}
