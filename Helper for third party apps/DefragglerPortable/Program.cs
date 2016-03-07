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
                    string rootDir = Path.Combine(Application.StartupPath, "Defraggler");
                    if (!Directory.Exists(rootDir))
                        return;
                    string appPath = Path.Combine(rootDir, "Defraggler.exe");
                    string updaterPath = Path.Combine(rootDir, "DefragglerUpdater.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0 ||
                        !File.Exists(updaterPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(updaterPath)).Length > 0)
                        return;
                    SilDev.Log.AllowDebug();
                    string cmdLine = Environment.CommandLine.Replace($"\"{Application.ExecutablePath}\"", string.Empty).TrimStart();
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = "/silent",
                        FileName = updaterPath,
                        Verb = "runas"
                    }, -1, 0);
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = cmdLine,
                        FileName = appPath,
                        Verb = "runas"
                    }, 0);
                }
            }
        }
    }
}
