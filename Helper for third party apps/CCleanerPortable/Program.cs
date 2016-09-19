using SilDev;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace CCleanerPortable
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
                    string rootDir = PATH.Combine("%CurDir%\\CCleaner");
                    if (!Directory.Exists(rootDir))
                        return;
                    string appPath = Path.Combine(rootDir, "CCleaner.exe");
                    string updaterPath = Path.Combine(rootDir, "CCleanerUpdater.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0 ||
                        !File.Exists(updaterPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(updaterPath)).Length > 0)
                        return;
                    LOG.AllowDebug();
                    string portableDat = Path.Combine(rootDir, "portable.dat");
                    if (!File.Exists(portableDat))
                        File.WriteAllText(portableDat, "#PORTABLE#");
                    string cmdLine = Environment.CommandLine.Replace($"\"{Application.ExecutablePath}\"", string.Empty).TrimStart();
                    RUN.App(new ProcessStartInfo()
                    {
                        Arguments = "/silent",
                        FileName = updaterPath,
                        Verb = "runas"
                    }, 0);
                    RUN.App(new ProcessStartInfo()
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
