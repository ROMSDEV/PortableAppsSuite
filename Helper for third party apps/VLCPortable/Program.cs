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
                string cmdLine = null;
                int length = Environment.GetCommandLineArgs().Length - 1;
                if (length > 0)
                {
                    string[] cmdLineArgs = new string[length];
                    Array.ConstrainedCopy(Environment.GetCommandLineArgs(), 1, cmdLineArgs, 0, length);
                    cmdLine = string.Format("\"{0}\" ", string.Join("\" \"", cmdLineArgs));
                }
                if (newInstance)
                {
                    SilDev.Data.DirLink(SilDev.Run.EnvironmentVariableFilter("%AppData%\\vlc"), SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\Data"), true);
                    SilDev.Run.App(new ProcessStartInfo() { FileName = appPath, Arguments = string.Format("{0}--no-plugins-cache", cmdLine) }, 0);
                    while (Process.GetProcessesByName("vlc").Length > 0)
                    {
                        foreach (Process app in Process.GetProcessesByName("vlc"))
                            app.WaitForExit();
                    }
                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter("%AppData%\\vlc"), true);
                }
                else
                    SilDev.Run.App(new ProcessStartInfo() { FileName = appPath, Arguments = cmdLine }, 0);
            }
        }
    }
}
