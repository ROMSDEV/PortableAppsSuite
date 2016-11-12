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
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                if (newInstance)
                {
                    var appPath = PathEx.Combine(PathEx.LocalDir,
#if x86
                     @"App\OBS32\bin\32bit\obs32.exe"
#else
                     @"App\OBS64\bin\64bit\obs64.exe"
#endif
                    );

                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                        return;
                    Data.DirLink(@"%AppData%\obs-studio", @"%CurDir%\Data", true);
                    using (var p = ProcessEx.Start(appPath, false, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    Data.DirUnLink(@"%AppData%\obs-studio", true);
                }
        }
    }
}
