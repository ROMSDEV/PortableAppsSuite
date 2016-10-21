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
            var appPath = PathEx.Combine("%CurDir%\\TS3Client\\ts3client_win32.exe");
            var configPath = PathEx.Combine("%CurDir%\\TS3Client\\config");
            const string iniName = "TS3ClientPortable.ini";
#else
            var appPath = PathEx.Combine("%CurDir%\\TS3Client64\\ts3client_win64.exe");
            var configPath = PathEx.Combine("%CurDir%\\TS3Client64\\config");
            const string iniName = "TS3Client64Portable.ini";
#endif
            var appDir = Path.GetDirectoryName(appPath);

            if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                Environment.Exit(1);

            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (!newInstance)
                    return;

                Ini.File(EnvironmentEx.GetVariableValue("CurDir"), iniName);
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

                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);
                using (var p = ProcessEx.Start(appPath, false, false))
                    if (!p?.HasExited == true)
                        p?.WaitForInputIdle();

                var winState = Ini.Read("Settings", "WinState");
                var windowState = winState.StartsWithEx("Min", "Max");
                var hideInTaskBar = Ini.ReadBoolean("Settings", "HideInTaskBar");
                var runningProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
                while (runningProcess.Length > 0)
                {
                    foreach (var p in runningProcess)
                        try
                        {
                            p.WaitForInputIdle();
                            if (p.HasExited)
                                continue;
                            if (windowState || hideInTaskBar)
                                p.WaitForExit(100);
                            else
                                p.WaitForExit();
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    if (windowState || hideInTaskBar)
                    {
                        var hWnd = WinApi.FindWindowByCaption("TeamSpeak 3");
                        if (hWnd != IntPtr.Zero)
                        {
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
                            if (hideInTaskBar)
                            {
                                hideInTaskBar = false;
                                TaskBar.DeleteTab(hWnd);
                            }
                        }
                    }
                    runningProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
                }

                Reg.RemoveExistSubKey(Reg.RegKey.CurrentUser, "Software\\TeamSpeak 3 Client");
                Reg.RemoveExistSubKey(Reg.RegKey.LocalMachine, "SOFTWARE\\TeamSpeak 3 Client");
                if (File.Exists(PathEx.Combine(appDir, "OverwolfTeamSpeakInstaller.exe")))
                    File.Delete(PathEx.Combine(appDir, "OverwolfTeamSpeakInstaller.exe"));
            }
        }
    }
}
