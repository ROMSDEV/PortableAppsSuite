namespace ValleyBenchmarkPortable // Valley Benchmark 1.0
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
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (!newInstance)
                    return;
                Data.DirLink(PathEx.Combine("%UserProfile%\\Valley"), PathEx.Combine("%CurDir%\\Data"), true);
                using (var p = ProcessEx.Start("%CurDir%\\App\\valley\\bin\\browser_x86.exe", $"-config \"{PathEx.Combine("%CurDir%\\App\\Valley\\data\\launcher\\launcher.xml")}\"", false, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
                while (Process.GetProcessesByName("browser_x86").Length > 0 || Process.GetProcessesByName("Valley").Length > 0)
                {
                    foreach (var app in Process.GetProcessesByName("browser_x86"))
                        app.WaitForExit();
                    foreach (var app in Process.GetProcessesByName("Valley"))
                        app.WaitForExit();
                }
                Data.DirUnLink(PathEx.Combine("%UserProfile%\\Valley"), true);
            }
        }
    }
}
