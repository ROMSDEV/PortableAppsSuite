using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace OBSPortable
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
                    string appPath = Path.Combine(Application.StartupPath, "OBS\\OBS.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                        return;
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = "-portable",
                        FileName = appPath
                    }, 0);
                }
            }
        }
    }
}
