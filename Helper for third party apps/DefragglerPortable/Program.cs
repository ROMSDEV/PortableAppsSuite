using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace DefragglerPortable
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
                    string commandLine = Environment.CommandLine.Replace(string.Format("\"{0}\"", Application.ExecutablePath), string.Empty);
                    SilDev.Run.App(Application.StartupPath, "Defraggler\\DefragglerUpdater.exe", "/silent", true, SilDev.Run.WindowStyle.Normal, -1, 0);
                    SilDev.Run.App(Application.StartupPath, "Defraggler\\Defraggler.exe", commandLine, true, 0);
                }
                else
                    Environment.Exit(2);
            }
        }
    }
}
