namespace VLCPortable
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
#if x86
            var curPath64 = PathEx.Combine(PathEx.LocalDir, "VLC64Portable.exe");
            if (Environment.Is64BitOperatingSystem && File.Exists(curPath64))
            {
                ProcessEx.Start(curPath64, EnvironmentEx.CommandLine());
                return;
            }
            var appDir = PathEx.Combine(PathEx.LocalDir, "App\\vlc");
            var updaterPath = PathEx.Combine(appDir, "VLCUpdater.exe");
#else
            var appDir = PathEx.Combine(PathEx.LocalDir, "App\\vlc64");
            var updaterPath = PathEx.Combine(appDir, "VLCUpdater64.exe");
#endif

            if (!File.Exists(updaterPath) || ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(updaterPath)) > 0)
                return;

            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                var appPath = PathEx.Combine(appDir, "vlc.exe");

                if (!newInstance)
                {
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine());
                    return;
                }

                if (ProcessEx.InstancesCount(Path.GetFileNameWithoutExtension(appPath)) > 0)
                    return;

                var dirMap = new Dictionary<string, string>
                {
                    {
                        "%AppData%\\vlc",
                        "%CurDir%\\Data"
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

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);

                Helper.ApplicationStart(appPath, $"{EnvironmentEx.CommandLine()} --no-plugins-cache");

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);
            }
        }
    }
}
