namespace OBSPortable
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
            var curPath64 = PathEx.Combine(PathEx.LocalDir, "OBSStudio64Portable.exe");
            if (Environment.Is64BitOperatingSystem && File.Exists(curPath64))
            {
                ProcessEx.Start(curPath64, EnvironmentEx.CommandLine(false));
                return;
            }
            var appPath = PathEx.Combine(PathEx.LocalDir, "App\\bin\\32bit\\obs32.exe");
            var redPath = PathEx.Combine(PathEx.LocalDir, "App\\bin\\64bit\\obs64.exe");
#else
            var appPath = PathEx.Combine(PathEx.LocalDir, "App\\bin\\64bit\\obs64.exe");
            var redPath = PathEx.Combine(PathEx.LocalDir, "App\\bin\\32bit\\obs32.exe");
#endif

            if (!File.Exists(appPath) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(redPath)))
                return;

            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                if (!newInstance)
                    return;

                var dirMap = new Dictionary<string, string>
                {
                    {
                        "%AppData%\\obs-studio",
                        "%CurDir%\\Data"
                    }
                };

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false));

                var names = new[]
                {
                    Path.GetFileNameWithoutExtension(appPath),
                    Path.GetFileNameWithoutExtension(redPath)
                };
                Recheck:
                foreach (var name in names)
                {
                    var wasRunning = false;
                    while (ProcessEx.IsRunning(name) || WinApi.FindWindowByCaption("OBS Studio Update") != IntPtr.Zero)
                    {
                        if (!wasRunning)
                            wasRunning = true;
                        Thread.Sleep(200);
                    }
                    if (!wasRunning)
                        continue;
                    Thread.Sleep(250);
                    goto Recheck;
                }

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);
            }
        }
    }
}
