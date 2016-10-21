namespace Cinebench64Portable // CINEBENCH_R15
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                if (newInstance)
                {
                    var appPath = PathEx.Combine("%CurDir%\\App\\cinebench64\\CINEBENCH Windows 64 Bit.exe");
                    using (var p = ProcessEx.Start(appPath, true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    while (Process.GetProcessesByName("CINEBENCH Windows 64 Bit").Length > 0)
                        foreach (var app in Process.GetProcessesByName("CINEBENCH Windows 64 Bit"))
                            app.WaitForExit();
                    try
                    {
                        var cachePath = PathEx.Combine("%AppData%\\MAXON");
                        foreach (var dir in Directory.GetDirectories(cachePath, "cinebench64_*", SearchOption.TopDirectoryOnly))
                            Directory.Delete(dir, true);
                        if (Directory.GetFiles(cachePath, "*", SearchOption.AllDirectories).Length == 0)
                            Directory.Delete(cachePath, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                }
        }
    }
}
