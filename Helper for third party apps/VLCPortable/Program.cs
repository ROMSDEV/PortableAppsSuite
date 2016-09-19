using SilDev;
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
            string appPath = PATH.Combine("%CurDir%\\App\\vlc\\vlc.exe");
#else
            string appPath = PATH.Combine("%CurDir%\\App\\vlc64\\vlc.exe");
#endif
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                LOG.AllowDebug();
                if (newInstance)
                {
                    DATA.DirLink(PATH.Combine("%AppData%\\vlc"), PATH.Combine("%CurDir%\\Data"), true);
                    RUN.App(new ProcessStartInfo()
                    {
                        Arguments = $"{RUN.CommandLine()} --no-plugins-cache".Trim(),
                        FileName = appPath
                    }, 0);
                    while (Process.GetProcessesByName("vlc").Length > 0)
                    {
                        foreach (Process app in Process.GetProcessesByName("vlc"))
                            app.WaitForExit();
                    }
                    DATA.DirUnLink(PATH.Combine("%AppData%\\vlc"), true);
                }
                else
                    RUN.App(new ProcessStartInfo()
                    {
                        Arguments = RUN.CommandLine(),
                        FileName = appPath
                    });
            }
        }
    }
}
