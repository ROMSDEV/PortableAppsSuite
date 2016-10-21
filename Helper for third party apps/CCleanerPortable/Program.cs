namespace CCleanerPortable
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                if (newInstance)
                {
                    var rootDir = PathEx.Combine("%CurDir%\\CCleaner");
                    if (!Directory.Exists(rootDir))
                        return;
                    var appPath = Path.Combine(rootDir, "CCleaner.exe");
                    var updaterPath = Path.Combine(rootDir, "CCleanerUpdater.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0 ||
                        !File.Exists(updaterPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(updaterPath)).Length > 0)
                        return;
                    var portableDat = Path.Combine(rootDir, "portable.dat");
                    if (!File.Exists(portableDat))
                        File.WriteAllText(portableDat, @"#PORTABLE#");
                    using (var p = ProcessEx.Start(updaterPath, "/silent", true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    using (var p = ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false), true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                }
        }
    }
}
