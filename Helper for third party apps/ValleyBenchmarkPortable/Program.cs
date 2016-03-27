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
                    SilDev.Data.DirLink(SilDev.Run.EnvironmentVariableFilter("%UserProfile%\\Valley"), SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\Data"), true);
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = $"-config \"{SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\App\\Valley\\data\\launcher\\launcher.xml")}\"",
                        FileName = "%CurrentDir%\\App\\valley\\bin\\browser_x86.exe"
                    }, 0);
                    while (Process.GetProcessesByName("browser_x86").Length > 0 || Process.GetProcessesByName("Valley").Length > 0)
                    {
                        foreach (Process app in Process.GetProcessesByName("browser_x86"))
                            app.WaitForExit();
                        foreach (Process app in Process.GetProcessesByName("Valley"))
                            app.WaitForExit();
                    }
                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter("%UserProfile%\\Valley"), true);
                }
            }
        }
    }
}
