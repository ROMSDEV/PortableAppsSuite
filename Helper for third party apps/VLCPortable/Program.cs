using System;
using System.Diagnostics;
using System.Threading;

namespace VLCPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
#if x86
            string appPath = SilDev.Run.EnvVarFilter("%CurrentDir%\\App\\vlc\\vlc.exe");
#else
            string appPath = SilDev.Run.EnvVarFilter("%CurrentDir%\\App\\vlc64\\vlc.exe");
#endif
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                SilDev.Log.AllowDebug();
                if (newInstance)
                {
                    SilDev.Data.DirLink(SilDev.Run.EnvVarFilter("%AppData%\\vlc"), SilDev.Run.EnvVarFilter("%CurrentDir%\\Data"), true);
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = $"{SilDev.Run.CommandLine()} --no-plugins-cache".Trim(),
                        FileName = appPath
                    }, 0);
                    while (Process.GetProcessesByName("vlc").Length > 0)
                    {
                        foreach (Process app in Process.GetProcessesByName("vlc"))
                            app.WaitForExit();
                    }
                    SilDev.Data.DirUnLink(SilDev.Run.EnvVarFilter("%AppData%\\vlc"), true);
                }
                else
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = SilDev.Run.CommandLine(),
                        FileName = appPath
                    });
            }
        }
    }
}
