namespace OBSPortable
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
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
                if (newInstance)
                {
                    var appPath = PathEx.Combine("%CurDir%\\OBS\\OBS.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                        return;
                    using (var p = ProcessEx.Start(appPath, "-portable", false, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                }
        }
    }
}
