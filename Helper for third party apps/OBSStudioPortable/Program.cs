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

#if x86
            var curPath64 = PathEx.Combine(PathEx.LocalDir, "OBSStudio64Portable.exe");
            if (Environment.Is64BitOperatingSystem && File.Exists(curPath64))
            {
                ProcessEx.Start(curPath64, EnvironmentEx.CommandLine());
                return;
            }
            var appPath = PathEx.Combine(PathEx.LocalDir, @"App\OBS32\bin\32bit\obs32.exe");
#else
            var appPath = PathEx.Combine(PathEx.LocalDir, @"App\OBS64\bin\64bit\obs64.exe");
#endif

            if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                return;

            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
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
