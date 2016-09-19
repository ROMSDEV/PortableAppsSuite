using SilDev;
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
            string appPath = PATH.Combine("%CurDir%\\App\\YoloMouse\\YoloMouse.exe");
            if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                Environment.Exit(-1);
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    LOG.AllowDebug();
                    string dataDir = PATH.Combine("%CurDir%\\Data");
                    string defDataDir = PATH.Combine("%LocalAppData%\\YoloMouse");
                    DATA.DirLink(defDataDir, dataDir, true);
                    RUN.App(new ProcessStartInfo() { FileName = appPath }, 0);
                    DATA.DirUnLink(defDataDir, true);
                }
            }
        }
    }
}
