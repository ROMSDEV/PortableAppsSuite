using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace YoloMousePortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            string appPath = Path.Combine(Application.StartupPath, "App\\YoloMouse\\YoloMouse.exe");
            if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                Environment.Exit(-1);
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    SilDev.Log.AllowDebug();
                    string dataDir = SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\Data");
                    string defDataDir = SilDev.Run.EnvironmentVariableFilter("%LocalAppData%\\YoloMouse");
                    SilDev.Data.DirLink(defDataDir, dataDir, true);
                    SilDev.Run.App(new ProcessStartInfo() { FileName = appPath }, 0);
                    SilDev.Data.DirUnLink(defDataDir, true);
                }
            }
        }
    }
}
