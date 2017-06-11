namespace TS3ClientPortable
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.Win32;
    using Portable;
    using Properties;
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
                const string appName = "ts3client_win32";
#else
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\ts3_x64");
                var appPath = PathEx.Combine(appDir, "ts3client_win64.exe");
                const string appName = "ts3client_win64";
#endif

                var updPath = PathEx.Combine(appDir, "update.exe");
                if (!File.Exists(updPath) || ProcessEx.IsRunning(appName))
                    return;

                var iniPath = Path.ChangeExtension(PathEx.LocalPath, ".ini");
                if (string.IsNullOrEmpty(iniPath))
                    return;
                if (!File.Exists(iniPath))
                {
                    var iniContent = new[]
                    {
                        "[Settings]",
                        "",
                        "; True to hide TeamSpeak on the Windows taskbar; otherwise, False.",
                        "HideInTaskBar=False",
                        "",
                        "; Time (in milliseconds) to wait for a network connection before TeamSpeak is started.",
                        "; This option can be useful for the Windows autorun feature. If TeamSpeak is started",
                        "; before the network connection is available, the connection is failed and no replay",
                        "; occurs.",
                        "WaitForNetwork=0",
                        "",
                        "; Sets the window state of TeamSpeak. Options: Maximized, Minimized or Normal.",
                        "WinState=Normal"
                    };
                    File.WriteAllLines(iniPath, iniContent);
                }

                var time = Ini.Read("Settings", "WaitForNetwork", 0, iniPath);
                if (time > 300000)
                    time = 300000;
                if (time.IsBetween(1, 300000))
                    for (var i = 0; i < time; i++)
                    {
                        if (NetEx.InternetIsAvailable())
                            break;
                        Thread.Sleep(1);
                    }

                var dirMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "config"),
                        "%CurDir%\\Data"
                    },
                    {
                        "%LocalAppData%\\TeamSpeak 3",
                        "%CurDir%\\Data\\Temp"
                    },
                    {
                        "%UserProfile%\\.QtWebEngineProcess",
                        "%CurDir%\\Data\\Temp\\.QtWebEngineProcess"
                    },
                    {
                        "%UserProfile%\\.TeamSpeak 3",
                        "%CurDir%\\Data\\Temp\\.TeamSpeak 3"
                    }
                };
                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);

                var regKeys = new[]
                {
                    "HKCU\\Software\\TeamSpeak 3 Client",
                    "HKCR\\.ts3_addon",
                    "HKCR\\.ts3_iconpack",
                    "HKCR\\.ts3_plugin",
                    "HKCR\\.ts3_soundpack",
                    "HKCR\\.ts3_style",
                    "HKCR\\.ts3_translation",
                    "HKCR\\ts3addon"
                };
                var firstKey = regKeys.LastOrDefault();
                Helper.RegForwarding(Helper.Options.Start, firstKey);
                Reg.Write(firstKey, null, appDir);
                Reg.Write(firstKey, "ConfigLocation", 1, RegistryValueKind.String);

                Process appProcess = null;
                if (File.Exists(appPath))
                    appProcess = ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false), Elevation.IsAdministrator, false);
                else
                    ProcessEx.Start(updPath, Elevation.IsAdministrator, false)?.WaitForExit();

                var wrapper = PathEx.Combine(appDir, "createfileassoc.exe");
                var hide = Ini.Read("Settings", "HideInTaskBar", false, iniPath);
                var state = Ini.Read("Settings", "WinState", iniPath);
                var enable = state.EqualsEx("Maximized", "Minimized", iniPath);

                if (appProcess == null)
                    goto Second;

                First:
                while (!appProcess.HasExited)
                {
                    appProcess.WaitForExit(1000);

                    if (File.Exists(wrapper) && !Crypto.EncryptFileToMd5(wrapper).EqualsEx("cdbe3628ca898a852502c8ae897e5a54"))
                        try
                        {
                            File.WriteAllBytes(wrapper, Resources.EmptyWrapper);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }

                    if (!hide && !enable)
                        continue;

                    var hWnd = WinApi.FindWindowByCaption("TeamSpeak 3");
                    if (hWnd == IntPtr.Zero)
                        continue;

                    if (hide)
                    {
                        hide = false;
                        TaskBar.DeleteTab(hWnd);
                    }

                    if (!enable)
                        continue;
                    enable = false;
                    WinApi.UnsafeNativeMethods.ShowWindowAsync(hWnd, state.EqualsEx("Minimized") ? WinApi.ShowWindowFunc.SW_SHOWMINIMIZED : WinApi.ShowWindowFunc.SW_SHOWMAXIMIZED);
                }
                while (ProcessEx.IsRunning(appName))
                    Thread.Sleep(200);

                Second:
                for (var i = 0; i < 10; i++)
                {
                    while (ProcessEx.IsRunning(updPath) || WinApi.FindWindowByCaption("TeamSpeak 3 Client Update") != IntPtr.Zero)
                        Thread.Sleep(200);
                    if (ProcessEx.IsRunning(appName))
                    {
                        appProcess = Process.GetProcessesByName(appName).FirstOrDefault();
                        if (appProcess == null)
                            break;
                        hide = Ini.Read("Settings", "HideInTaskBar", false, iniPath);
                        enable = state.EqualsEx("Maximized", "Minimized", iniPath);
                        goto First;
                    }
                    Thread.Sleep(250);
                }

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);

                Helper.RegForwarding(Helper.Options.Exit, firstKey);

                var rootKeys = regKeys.Skip(1).ToArray();
                if (rootKeys.Any(Reg.SubKeyExists))
                {
                    var regSecureMap = rootKeys.ToDictionary<string, string, Dictionary<string, string>>(x => $"-{x}", x => null);
                    Helper.RegSecureOverwrite(regSecureMap, true);
                }

                try
                {
                    var owInstallerPath = Path.Combine(appDir, "OverwolfTeamSpeakInstaller.exe");
                    if (File.Exists(owInstallerPath))
                        File.Delete(owInstallerPath);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }
    }
}
