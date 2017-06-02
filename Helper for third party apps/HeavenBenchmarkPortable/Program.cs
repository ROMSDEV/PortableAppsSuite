namespace HeavenBenchmarkPortable // Heaven Benchmark 4.0
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
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
                if (newInstance)
                {
                    Data.DirLink("%UserProfile%\\Heaven", "%CurDir%\\Data", true);
                    using (var p = ProcessEx.Start("%CurDir%\\App\\heaven\\bin\\browser_x86.exe", $"-config \"{PathEx.Combine("%CurDir%\\App\\heaven\\data\\launcher\\launcher.xml")}\"", true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    while (Process.GetProcessesByName("browser_x86").Length > 0 || Process.GetProcessesByName("Heaven").Length > 0)
                    {
                        foreach (var app in Process.GetProcessesByName("browser_x86"))
                            app.WaitForExit();
                        foreach (var app in Process.GetProcessesByName("Heaven"))
                            app.WaitForExit();
                    }
                    Data.DirUnLink("%UserProfile%\\Heaven", true);
                }
        }
    }
}
