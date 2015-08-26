using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace WinFAQPortable
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
                    SilDev.Run.App(Application.StartupPath, "WinFAQ\\winfaq.chm", SilDev.Run.WindowStyle.Maximized);
                }
            }
        }
    }
}
