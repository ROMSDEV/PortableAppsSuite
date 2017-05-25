namespace VLCPortable
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
#if x86
    using System.IO;
#endif
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
            var appPath = PathEx.Combine("%CurDir%\\App\\vlc\\vlc.exe");
#else
            var appPath = PathEx.Combine("%CurDir%\\App\\vlc64\\vlc.exe");
#endif
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (!newInstance)
                {
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine());
                    return;
                }
                var dirMap = new Dictionary<string, string>
                {
                    {
                        "%AppData%\\vlc",
                        "%CurDir%\\Data"
                    }
                };
                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);
                Helper.ApplicationStart(appPath, $"{EnvironmentEx.CommandLine()} --no-plugins-cache");
                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);
            }
        }
    }
}
