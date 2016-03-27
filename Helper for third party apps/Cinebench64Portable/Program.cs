using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Cinebench64Portable // CINEBENCH_R15
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    SilDev.Log.AllowDebug();
                    SilDev.Run.App(new ProcessStartInfo() { FileName = "%CurrentDir%\\App\\cinebench64\\CINEBENCH Windows 64 Bit.exe" }, 0);
                    while (Process.GetProcessesByName("CINEBENCH Windows 64 Bit").Length > 0)
                        foreach (Process app in Process.GetProcessesByName("CINEBENCH Windows 64 Bit"))
                            app.WaitForExit();
                    try
                    {
                        string cachePath = SilDev.Run.EnvironmentVariableFilter("%AppData%\\MAXON");
                        foreach (var dir in Directory.GetDirectories(cachePath, "cinebench64_*", SearchOption.TopDirectoryOnly))
                            Directory.Delete(dir, true);
                        if (Directory.GetFiles(cachePath, "*", SearchOption.AllDirectories).Length == 0)
                            Directory.Delete(cachePath, true);
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                }
            }
        }
    }
}
