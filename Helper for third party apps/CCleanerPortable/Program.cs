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
                    string rootDir = Path.Combine(Application.StartupPath, "CCleaner");
                    if (!Directory.Exists(rootDir))
                        return;
                    string appPath = Path.Combine(rootDir, "CCleaner.exe");
                    string updaterPath = Path.Combine(rootDir, "CCleanerUpdater.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0 ||
                        !File.Exists(updaterPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(updaterPath)).Length > 0)
                        return;
                    SilDev.Log.AllowDebug();
                    string portableDat = Path.Combine(rootDir, "portable.dat");
                    if (!File.Exists(portableDat))
                        using (StreamWriter sw = File.CreateText(portableDat))
                            sw.Write("#PORTABLE#");
                    string commandLine = Environment.CommandLine.Replace(string.Format("\"{0}\"", Application.ExecutablePath), string.Empty);
                    SilDev.Run.App(Path.GetDirectoryName(updaterPath), Path.GetFileName(updaterPath), "/silent", true, SilDev.Run.WindowStyle.Normal, -1, 0);
                    SilDev.Run.App(Path.GetDirectoryName(appPath), Path.GetFileName(appPath), commandLine, true, 0);
                }
            }
        }
    }
}
