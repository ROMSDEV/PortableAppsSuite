namespace VLCPortable
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
#if x86
            var appPath = PathEx.Combine("%CurDir%\\App\\vlc\\vlc.exe");
#else
            var appPath = PathEx.Combine("%CurDir%\\App\\vlc64\\vlc.exe");
#endif
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                if (newInstance)
                {
                    Data.DirLink("%AppData%\\vlc", "%CurDir%\\Data", true);
                    using (var p = ProcessEx.Start(appPath, EnvironmentEx.CommandLine() + " --no-plugins-cache", false, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    while (Process.GetProcessesByName("vlc").Length > 0)
                        foreach (var app in Process.GetProcessesByName("vlc"))
                            app.WaitForExit();
                    Data.DirUnLink("%AppData%\\vlc", true);
                }
                else
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine());
        }
    }
}
