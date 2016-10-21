namespace YoloMousePortable
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
            var appPath = PathEx.Combine("%CurDir%\\App\\YoloMouse\\YoloMouse.exe");
            if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                Environment.Exit(1);
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                if (newInstance)
                {
                    var dataDir = PathEx.Combine("%CurDir%\\Data");
                    var defDataDir = PathEx.Combine("%LocalAppData%\\YoloMouse");
                    Data.DirLink(defDataDir, dataDir, true);
                    using (var p = ProcessEx.Start(appPath, false, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    Data.DirUnLink(defDataDir, true);
                }
        }
    }
}
