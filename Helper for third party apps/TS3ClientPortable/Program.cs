namespace TS3ClientPortable
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
            var curPath64 = PathEx.Combine(PathEx.LocalDir, "TS3Client64Portable.exe");
            if (Environment.Is64BitOperatingSystem && File.Exists(curPath64))
            {
                ProcessEx.Start(curPath64, EnvironmentEx.CommandLine());
                return;
            }

            var oldAppDir = PathEx.Combine(PathEx.LocalDir, "TS3Client");
            var appDir = PathEx.Combine(PathEx.LocalDir, "App\\ts3_x86");
            var appPath = Path.Combine(appDir, "ts3client_win32.exe");
#else
            var oldAppDir = PathEx.Combine(PathEx.LocalDir, "TS3Client64");
            var appDir = PathEx.Combine(PathEx.LocalDir, "App\\ts3_x64");
            var appPath = Path.Combine(appDir, "ts3client_win64.exe");
#endif

            var oldCfgDir = Path.Combine(oldAppDir, "config");
            var cfgDir = Path.Combine(appDir, "config");
            var datDir = PathEx.Combine(PathEx.LocalDir, "Data");

            if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                Environment.Exit(1);

            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (!newInstance)
                    return;

                Ini.File(PathEx.LocalDir, $"{Process.GetCurrentProcess().ProcessName}.ini");
                var waitForNetwork = Ini.Read("Settings", "WaitForNetwork");
                int ms;
                if (int.TryParse(waitForNetwork, out ms))
                    try
                    {
                        if (ms > 0)
                            for (var i = 0; i < ms; i++)
                            {
                                Thread.Sleep(1);
                                if (NetEx.InternetIsAvailable())
                                    break;
                            }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }

                if (Directory.Exists(oldCfgDir) && !Directory.Exists(datDir))
                    Directory.Move(oldCfgDir, datDir);
                if (Directory.Exists(oldAppDir))
                    Directory.Delete(oldAppDir, true);
                if (Directory.Exists(cfgDir))
                {
                    if (Directory.Exists(datDir))
                        Directory.Delete(cfgDir, true);
                    else
                        Directory.Move(cfgDir, datDir);
                }
                if (!Directory.Exists(datDir))
                    Directory.CreateDirectory(datDir);
                Data.DirLink(cfgDir, datDir, true);

                var runningProcess = ProcessEx.Start(appPath, false, false);

                var winState = Ini.Read("Settings", "WinState");
                var windowState = winState.StartsWithEx("Min", "Max");
                var hideInTaskBar = Ini.ReadBoolean("Settings", "HideInTaskBar");
                while (!runningProcess?.HasExited == true)
                {
                    runningProcess?.WaitForExit(1000);
                    if (!windowState && !hideInTaskBar)
                        continue;
                    var hWnd = WinApi.FindWindowByCaption("TeamSpeak 3");
                    if (hWnd == IntPtr.Zero)
                        continue;
                    if (winState.StartsWithEx("Min"))
                    {
                        windowState = false;
                        WinApi.UnsafeNativeMethods.ShowWindowAsync(hWnd, WinApi.ShowWindowFunc.SW_SHOWMINIMIZED);
                    }
                    else if (winState.StartsWithEx("Max"))
                    {
                        windowState = false;
                        WinApi.UnsafeNativeMethods.ShowWindowAsync(hWnd, WinApi.ShowWindowFunc.SW_SHOWMAXIMIZED);
                    }
                    if (!hideInTaskBar)
                        continue;
                    hideInTaskBar = false;
                    TaskBar.DeleteTab(hWnd);
                }

                var usrTmpDir = PathEx.Combine("%UserProfile%\\.TeamSpeak 3");
                if (Directory.Exists(usrTmpDir))
                    Directory.Delete(usrTmpDir, true);
                Data.DirUnLink(cfgDir, true);
                Reg.RemoveExistSubKey(Reg.RegKey.CurrentUser, "Software\\TeamSpeak 3 Client");
                Reg.RemoveExistSubKey(Reg.RegKey.LocalMachine, "SOFTWARE\\TeamSpeak 3 Client");
                if (File.Exists(PathEx.Combine(appDir, "OverwolfTeamSpeakInstaller.exe")))
                    File.Delete(PathEx.Combine(appDir, "OverwolfTeamSpeakInstaller.exe"));
            }
        }
    }
}
