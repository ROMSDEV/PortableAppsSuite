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
            const byte sArch = 64;
            var sPath = PathEx.Combine(PathEx.LocalDir, "OBSStudio64Portable.exe");
            if (Environment.Is64BitOperatingSystem && File.Exists(sPath))
            {
                ProcessEx.Start(sPath, EnvironmentEx.CommandLine());
                return;
            }
            var sAppPath = PathEx.Combine(PathEx.LocalDir, @"App\OBS32\bin\64bit\obs64.exe");

            const byte cArch = 32;
            var cAppPath = PathEx.Combine(PathEx.LocalDir, @"App\OBS32\bin\32bit\obs32.exe");
#else
            const byte sArch = 32;
            var sAppPath = PathEx.Combine(PathEx.LocalDir, @"App\OBS64\bin\32bit\obs32.exe");

            const byte cArch = 64;
            var cAppPath = PathEx.Combine(PathEx.LocalDir, @"App\OBS64\bin\64bit\obs64.exe");
#endif

            if (!File.Exists(cAppPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(cAppPath)).Length > 0 || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(sAppPath)).Length > 0)
                return;

            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                if (!newInstance)
                    return;

                var bDir = PathEx.Combine(PathEx.LocalDir, $@"App\OBS{cArch}\bin\{sArch}bit");
                try
                {
                    if (Directory.Exists(bDir))
                        Directory.Delete(bDir, true);
                }
                catch
                {
                    AppDomain.CurrentDomain.ProcessExit += (s, e) => ProcessEx.Send($"RD /S /Q \"{bDir}\"");
                }
                var pDir = PathEx.Combine(PathEx.LocalDir, $@"App\OBS{cArch}\obs-plugins\{sArch}bit");
                try
                {
                    if (Directory.Exists(pDir))
                        Directory.Delete(pDir, true);
                }
                catch
                {
                    AppDomain.CurrentDomain.ProcessExit += (s, e) => ProcessEx.Send($"RD /S /Q \"{pDir}\"");
                }

                var cfgDir = PathEx.Combine("%AppData%\\obs-studio");
                var datDir = PathEx.Combine("%CurDir%\\Data");
                Data.DirLink(cfgDir, datDir, true);

                using (var p = ProcessEx.Start(cAppPath, false, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
                for (var i = 0; i < 10; i++)
                {
                    var isRunning = true;
                    while (isRunning)
                    {
                        isRunning = ProcessEx.IsRunning(cAppPath) || ProcessEx.IsRunning(sAppPath) || WinApi.FindWindowByCaption("OBS Studio Update") != IntPtr.Zero;
                        Thread.Sleep(200);
                    }
                    Thread.Sleep(500);
                }

                Data.DirUnLink(@"%AppData%\obs-studio", true);
            }
        }
    }
}
