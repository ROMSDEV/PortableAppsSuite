namespace OriginPortable
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using Microsoft.Win32;
    using Portable;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\Origin");
                var appPath = Path.Combine(appDir, "Origin.exe");

                if (!newInstance)
                {
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false));
                    return;
                }

#if x86
                if (Environment.Is64BitOperatingSystem)
                {
                    var curPath64 = PathEx.Combine(PathEx.LocalDir, $"{Path.GetFileNameWithoutExtension(PathEx.LocalPath)}64.exe");
                    ProcessEx.Start(curPath64, EnvironmentEx.CommandLine(false));
                    return;
                }
#endif

                if (IsRunning(appDir))
                    CloseAll(appDir);

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                if (IsRunning(dataDir))
                    CloseAll(dataDir);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Helper.RedistHandling(Helper.Options.Start, EnvironmentEx.RedistPack.VC2010_x86, EnvironmentEx.RedistPack.VC2013_x86
#if !x86
                    , EnvironmentEx.RedistPack.VC2010_x64, EnvironmentEx.RedistPack.VC2013_x64
#endif
                );

                var iniPath = Path.ChangeExtension(PathEx.LocalPath, ".ini");
                if (!File.Exists(iniPath))
                    Ini.WriteDirect("Settings", "Tray", false, iniPath);

                var dirMap = new Dictionary<string, string>
                {
                    {
                        "%SystemDrive%\\temp",
                        "%CurDir%\\Data\\Temp"
                    },
                    {
                        "%CommonProgramFiles(x86)%\\EAInstaller",
                        "%CurDir%\\App\\Common\\EAInstaller"
                    },
                    {
                        "%AppData%\\EA Games",
                        "%CurDir%\\Data\\AppData\\Roaming\\EA Games"
                    },
                    {
                        "%AppData%\\Electronic Arts",
                        "%CurDir%\\Data\\AppData\\Roaming\\Electronic Arts"
                    },
                    {
                        "%AppData%\\Origin",
                        "%CurDir%\\Data\\AppData\\Roaming\\Origin"
                    },
                    {
                        "%LocalAppData%\\EA Games",
                        "%CurDir%\\Data\\AppData\\Local\\EA Games"
                    },
                    {
                        "%LocalAppData%\\Electronic Arts",
                        "%CurDir%\\Data\\AppData\\Local\\Electronic Arts"
                    },
                    {
                        "%LocalAppData%\\Origin",
                        "%CurDir%\\Data\\AppData\\Local\\Origin"
                    },
                    {
                        "%ProgramData%\\EA Games",
                        "%CurDir%\\Data\\ProgramData\\EA Games"
                    },
                    {
                        "%ProgramData%\\Electronic Arts",
                        "%CurDir%\\Data\\ProgramData\\Electronic Arts"
                    },
                    {
                        "%ProgramData%\\Origin",
                        "%CurDir%\\Data\\ProgramData\\Origin"
                    }
                };

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);

                var regKeys = new[]
                {
                    "HKCR\\origin",
                    "HKCR\\origin2",
                    "HKCU\\Software\\EA Games",
                    "HKCU\\Software\\Electronic Arts",
                    "HKLM\\SOFTWARE\\EA Games",
                    "HKLM\\SOFTWARE\\Electronic Arts",
                    "HKLM\\SOFTWARE\\Origin",
#if x86
                    "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Origin",
#else
                    "HKLM\\SOFTWARE\\Wow6432Node\\EA Games",
                    "HKLM\\SOFTWARE\\Wow6432Node\\Electronic Arts",
                    "HKLM\\SOFTWARE\\Wow6432Node\\Origin",
                    "HKLM\\SOFTWARE\\Wow6432Node\\Origin Games",
                    "HKLM\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Origin"
#endif
                };

                Helper.RegForwarding(Helper.Options.Start, regKeys);

                var key = "HKCR\\origin";
                Reg.Write(key, null, $"\"{appPath}\" \"%1\"");

                key = "HKCR\\origin2";
                Reg.Write(key, null, $"\"{appPath}\" \"%1\"");

#if x86
                key = "HKLM\\SOFTWARE\\Electronic Arts\\EA Core";
#else
                key = "HKLM\\SOFTWARE\\Wow6432Node\\Electronic Arts\\EA Core";
#endif
                Reg.Write(key, "ClientAccessDLLPath", Path.Combine(appDir, "LegacyPM\\CmdPortalClient.dll"));
                Reg.Write(key, "ClientPath", Path.Combine(appDir, "LegacyPM\\OriginLegacyCLI.dll"));
                Reg.Write(key, "EADM6InstallDir", appDir);

#if x86
                key = "HKLM\\SOFTWARE\\Electronic Arts\\EADM";
#else
                key = "HKLM\\SOFTWARE\\Wow6432Node\\Electronic Arts\\EADM";
#endif
                Reg.Write(key, "ClientPath", appPath);

#if x86
                key = "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Origin";
#else
                key = "HKLM\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Origin";
#endif
                Reg.Write(key, "DisplayIcon", PathEx.Combine(appDir, "OriginUninstall.exe"));
                Reg.Write(key, "InstallLocation", appDir);
                Reg.Write(key, "UninstallString", PathEx.Combine(appDir, "OriginUninstall.exe"));

                var regPath = Path.Combine(dataDir, "settings.reg");
                if (!File.Exists(regPath))
                {
                    string version;
                    try
                    {
                        version = FileVersionInfo.GetVersionInfo(appPath).FileVersion;
                        if (version.Contains(','))
                            version = version.Split(',').Select(c => c.Trim()).Join('.');
                    }
                    catch
                    {
                        version = "0.0.0.0";
                    }

#if x86
                    key = "HKLM\\SOFTWARE\\Electronic Arts\\EA Core";
#else
                    key = "HKLM\\SOFTWARE\\Wow6432Node\\Electronic Arts\\EA Core";
#endif
                    Reg.Write(key, "ClientVersion", "7.0.0.1");
                    Reg.Write(key, "EADM6Version", "7.0.0.1");

#if x86
                    key = "HKLM\\SOFTWARE\\Electronic Arts\\EADM";
#else
                    key = "HKLM\\SOFTWARE\\Wow6432Node\\Electronic Arts\\EADM";
#endif
                    Reg.Write(key, "ClientVersion", version);

#if x86
                    key = "HKLM\\SOFTWARE\\Origin";
#else
                    key = "HKLM\\SOFTWARE\\Wow6432Node\\Origin";
#endif

                    Reg.Write(key, "AutopatchGlobal", false, RegistryValueKind.String);
                    Reg.Write(key, "Autoupdate", false, RegistryValueKind.String);
                    Reg.Write(key, "ClientVersion", version);
                    Reg.Write(key, "InstallSuccesfull", true, RegistryValueKind.String);
                    Reg.Write(key, "IsBeta", false, RegistryValueKind.String);
                    Reg.Write(key, "Launch", 0x014629dd, RegistryValueKind.String);
                    Reg.Write(key, "ShowDirPage", true, RegistryValueKind.String);
                    Reg.Write(key, "TelemOO", false, RegistryValueKind.String);

#if x86
                    key = "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Origin";
#else
                    key = "HKLM\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Origin";
#endif
                    Reg.Write(key, "DisplayName", "Origin");
                    Reg.Write(key, "DisplayVersion", version);
                    Reg.Write(key, "EstimatedSize", 0x0004d338, RegistryValueKind.DWord);
                    Reg.Write(key, "NoModify", 0x00000001, RegistryValueKind.DWord);
                    Reg.Write(key, "NoRepair", 0x00000001, RegistryValueKind.DWord);
                    Reg.Write(key, "OriginUninstall.exe", true, RegistryValueKind.String);
                    Reg.Write(key, "Publisher", "Electronic Arts, Inc.");
                    Reg.Write(key, "sEstimatedSize2", 0x00000000, RegistryValueKind.DWord);
                    Reg.Write(key, "URLInfoAbout", "http://www.ea.com");
                }

                using (var p = ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false), Elevation.IsAdministrator, Ini.ReadDirect("Settings", "Tray").EqualsEx("True") ? ProcessWindowStyle.Minimized : ProcessWindowStyle.Normal, false))
                    if (p?.HasExited == false)
                        p.WaitForExit();

                var dirs = new[] { appDir, dataDir };
                Recheck:
                foreach (var dir in dirs)
                {
                    var wasRunning = false;
                    while (IsRunning(dir))
                    {
                        if (!wasRunning)
                            wasRunning = true;
                        Thread.Sleep(200);
                    }
                    if (!wasRunning)
                        continue;
                    Thread.Sleep(250);
                    goto Recheck;
                }

                CloseServices();

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);

                Helper.RegForwarding(Helper.Options.Exit, regKeys);

                Helper.RedistHandling(Helper.Options.Exit, EnvironmentEx.RedistPack.VC2010_x86, EnvironmentEx.RedistPack.VC2013_x86
#if !x86
                    , EnvironmentEx.RedistPack.VC2010_x64, EnvironmentEx.RedistPack.VC2013_x64
#endif
                );
            }
        }

        private static bool IsNoService(string path) =>
            !Path.GetFileNameWithoutExtension(path).ContainsEx("service");

        private static bool IsRunning(string path)
        {
            try
            {
                var names = Directory.EnumerateFiles(path, "*.exe", SearchOption.AllDirectories)
                                     .Select(Path.GetFileNameWithoutExtension)
                                     .Where(n => n.ContainsEx("Origin"))
                                     .Where(IsNoService);
                return names.Any(ProcessEx.IsRunning);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        private static void CloseServices()
        {
            foreach (var srvName in new[]
            {
                "Origin Client Service",
                "Origin Web Helper Service"
            })
            {
                if (!Service.Exists(srvName))
                    continue;
                Service.Stop(srvName);
                Service.Uninstall(srvName);
            }
        }

        private static void CloseAll(string path)
        {
            try
            {
                CloseServices();
                foreach (var f in Directory.EnumerateFiles(path, "*.exe", SearchOption.AllDirectories))
                    foreach (var p in ProcessEx.GetInstances(f))
                    {
                        p?.CloseMainWindow();
                        p?.WaitForExit(100);
                        if (p?.HasExited == false)
                            p.Kill();
                    }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}
