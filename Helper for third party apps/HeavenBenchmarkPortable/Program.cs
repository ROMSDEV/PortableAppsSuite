using System;
using System.Diagnostics;
using System.Threading;

namespace HeavenBenchmarkPortable // Heaven Benchmark 4.0
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
                    SilDev.Data.DirLink(SilDev.Run.EnvironmentVariableFilter("%UserProfile%\\Heaven"), SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\Data"), true);
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = $"-config \"{SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\App\\heaven\\data\\launcher\\launcher.xml")}\"",
                        FileName = "%CurrentDir%\\App\\heaven\\bin\\browser_x86.exe"
                    }, 0);
                    while (Process.GetProcessesByName("browser_x86").Length > 0 || Process.GetProcessesByName("Heaven").Length > 0)
                    {
                        foreach (Process app in Process.GetProcessesByName("browser_x86"))
                            app.WaitForExit();
                        foreach (Process app in Process.GetProcessesByName("Heaven"))
                            app.WaitForExit();
                    }
                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter("%UserProfile%\\Heaven"), true);
                }
            }
        }
    }
}
