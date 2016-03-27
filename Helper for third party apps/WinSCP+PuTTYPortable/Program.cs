using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WinSCP_PuTTY_Launcher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SilDev.Log.AllowDebug();
            WinSCP_PuTTY_Launcher.Main.SetPortableDirs();
            if (!File.Exists(WinSCP_PuTTY_Launcher.Main.WinSCPExePath))
                Environment.Exit(0);
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                else
                    SilDev.Run.App(new ProcessStartInfo() { FileName = WinSCP_PuTTY_Launcher.Main.WinSCPExePath });
            }
        }
    }
}
