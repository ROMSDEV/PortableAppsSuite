namespace TS3ClientPortable
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
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
            {
                if (!newInstance)
                    return;

#if x86
                var curPath64 = PathEx.Combine(PathEx.LocalDir, "TS3Client64Portable.exe");
                if (Environment.Is64BitOperatingSystem && File.Exists(curPath64))
                {
                    ProcessEx.Start(curPath64, EnvironmentEx.CommandLine());
                    return;
                }

                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\ts3_x86");
                var appPath = PathEx.Combine(appDir, "ts3client_win32.exe");
#else
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\ts3_x64");
                var appPath = PathEx.Combine(appDir, "ts3client_win64.exe");
#endif

                if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                    return;

                var iniPath = Path.ChangeExtension(PathEx.LocalPath, ".ini");

                var waitForNetwork = Ini.Read("Settings", "WaitForNetwork", iniPath);
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

                var dataDirMap = new[,]
                {
                    {
                        PathEx.Combine(appDir, "config"),
                        PathEx.Combine("%CurDir%\\Data")
                    },
                    {
                        PathEx.Combine("%UserProfile%\\.QtWebEngineProcess"),
                        PathEx.Combine("%CurDir%\\Data\\.QtWebEngineProcess")
                    },
                    {
                        PathEx.Combine("%UserProfile%\\.TeamSpeak 3"),
                        PathEx.Combine("%CurDir%\\Data\\.TeamSpeak 3")
                    }
                };
                for (var i = 0; i < dataDirMap.GetLength(0); i++)
                {
                    if (!Directory.Exists(dataDirMap[i, 1]))
                        Directory.CreateDirectory(dataDirMap[i, 1]);
                    Data.DirLink(dataDirMap[i, 0], dataDirMap[i, 1], true);
                }

                var regKeyMap = new [,]
                {
                    {
                        "HKCU\\Software\\TeamSpeak 3 Client",
                        "HKCU\\Software\\SI13N7-BACKUP: TeamSpeak 3 Client"
                    },
                    {
                        "HKLM\\SOFTWARE\\TeamSpeak 3 Client",
                        "HKLM\\SOFTWARE\\SI13N7-BACKUP: TeamSpeak 3 Client"
                    }
                };
                for (var i = 0; i < regKeyMap.GetLength(0); i++)
                {
                    if (Reg.EntryExists(regKeyMap[i, 0], "Portable App"))
                        continue;
                    if (Reg.SubKeyExists(regKeyMap[i, 0]))
                        Reg.MoveSubKey(regKeyMap[i, 0], regKeyMap[i, 1]);
                    Reg.Write(regKeyMap[i, 0], "Portable App", "True");
                }

                var runningProcess = ProcessEx.Start(appPath, false, false);

                var winState = Ini.Read("Settings", "WinState", iniPath);
                var winStateEnabled = winState.EqualsEx("Minimized", "Min", "Maximized", "Max", iniPath);
                var hideInTaskBar = Ini.Read("Settings", "HideInTaskBar", false, iniPath);

                Restart:
                for (var i = 0; i < 10; i++)
                {
                    while (!runningProcess?.HasExited == true)
                    {
                        runningProcess?.WaitForExit(1000);
                        if (!winStateEnabled && !hideInTaskBar)
                            continue;
                        var hWnd = WinApi.FindWindowByCaption("TeamSpeak 3");
                        if (hWnd == IntPtr.Zero)
                            continue;
                        if (winState.EqualsEx("Minimized", "Min"))
                        {
                            winStateEnabled = false;
                            WinApi.UnsafeNativeMethods.ShowWindowAsync(hWnd, WinApi.ShowWindowFunc.SW_SHOWMINIMIZED);
                        }
                        else if (winState.EqualsEx("Maximized", "Max"))
                        {
                            winStateEnabled = false;
                            WinApi.UnsafeNativeMethods.ShowWindowAsync(hWnd, WinApi.ShowWindowFunc.SW_SHOWMAXIMIZED);
                        }
                        if (!hideInTaskBar)
                            continue;
                        hideInTaskBar = false;
                        TaskBar.DeleteTab(hWnd);
                    }

                    bool isRunning;
                    do
                    {
                        isRunning = ProcessEx.IsRunning(appPath);
                        Thread.Sleep(200);
                    }
                    while (isRunning);

                    IntPtr updWindPtr;
                    do
                    {
                        updWindPtr = WinApi.FindWindowByCaption("TeamSpeak 3 Client Update");
                        Thread.Sleep(200);
                    }
                    while (updWindPtr != IntPtr.Zero);

                    if (ProcessEx.IsRunning(appPath))
                    {
                        var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
                        if (processes.Length == 0)
                            break;

                        runningProcess = processes.First();

                        winStateEnabled = winState.StartsWithEx("Min", "Max");
                        hideInTaskBar = Ini.Read("Settings", "HideInTaskBar", false, iniPath);

                        goto Restart;
                    }

                    Thread.Sleep(250);
                }

                for (var i = 0; i < dataDirMap.GetLength(0); i++)
                    Data.DirUnLink(dataDirMap[i, 0], true);

                for (var i = 0; i < regKeyMap.GetLength(0); i++)
                {
                    Reg.RemoveSubKey(regKeyMap[i, 0]);
                    Reg.MoveSubKey(regKeyMap[i, 1], regKeyMap[i, 0]);
                }

                var owInstallerPath = PathEx.Combine(appDir, "OverwolfTeamSpeakInstaller.exe");
                if (File.Exists(owInstallerPath))
                    File.Delete(owInstallerPath);
            }
        }
    }
}
