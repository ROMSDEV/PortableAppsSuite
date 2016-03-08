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
            string appPath = SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\App\\vlc\\vlc.exe");
#else
            string appPath = SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\App\\vlc64\\vlc.exe");
#endif
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                SilDev.Log.AllowDebug();
                if (newInstance)
                {
                    SilDev.Data.DirLink(SilDev.Run.EnvironmentVariableFilter("%AppData%\\vlc"), SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\Data"), true);
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
                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter("%AppData%\\vlc"), true);
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
