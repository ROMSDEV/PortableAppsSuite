using SilDev;
using System;
using System.Diagnostics;
using System.Threading;

namespace ValleyBenchmarkPortable // Valley Benchmark 1.0
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
                    DATA.DirLink(PATH.Combine("%UserProfile%\\Valley"), PATH.Combine("%CurDir%\\Data"), true);
                    RUN.App(new ProcessStartInfo()
                    {
                        Arguments = $"-config \"{PATH.Combine("%CurDir%\\App\\Valley\\data\\launcher\\launcher.xml")}\"",
                        FileName = "%CurDir%\\App\\valley\\bin\\browser_x86.exe"
                    }, 0);
                    while (Process.GetProcessesByName("browser_x86").Length > 0 || Process.GetProcessesByName("Valley").Length > 0)
                    {
                        foreach (Process app in Process.GetProcessesByName("browser_x86"))
                            app.WaitForExit();
                        foreach (Process app in Process.GetProcessesByName("Valley"))
                            app.WaitForExit();
                    }
                    DATA.DirUnLink(PATH.Combine("%UserProfile%\\Valley"), true);
                }
            }
        }
    }
}
