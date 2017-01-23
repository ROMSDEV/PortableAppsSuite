namespace VLCPortable
{
    using System;
    using System.Diagnostics;
    using System.IO;
#if x86
    using System.IO;
#endif
    using System.Threading;
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
                if (newInstance)
                {
                    const string linkPath = "%AppData%\\vlc";
                    const string targetDir = "%CurDir%\\Data";
                    if (!Process.GetCurrentProcess().IsSandboxed())
                        Data.DirLink(linkPath, targetDir, true);
                    else
                        Data.DirCopy(targetDir, linkPath);

                    using (var p = ProcessEx.Start(appPath, EnvironmentEx.CommandLine() + " --no-plugins-cache", false, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    while (Process.GetProcessesByName("vlc").Length > 0)
                        foreach (var app in Process.GetProcessesByName("vlc"))
                            app.WaitForExit();

                    if (!Process.GetCurrentProcess().IsSandboxed())
                        Data.DirUnLink(linkPath, true);
                }
                else
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine());
        }
    }
}
