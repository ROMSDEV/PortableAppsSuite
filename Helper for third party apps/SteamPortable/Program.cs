namespace SteamPortable
{
    using System;
    using System.Collections.Generic;
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
            using (new Mutex(true, ProcessEx.CurrentName, out bool newInstance))
            {
#if x86
                if (Environment.Is64BitOperatingSystem)
                {
                    var curName = Path.GetFileNameWithoutExtension(PathEx.LocalPath);
                    var curPath64 = PathEx.Combine(PathEx.LocalDir, $"{curName}64.exe");
                    ProcessEx.Start(curPath64, EnvironmentEx.CommandLine(false));
                    return;
                }
#endif

                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\Steam");
                var appPath = Path.Combine(appDir, "Steam.exe");
                var cmdLine = EnvironmentEx.CommandLine(false);

                if (!newInstance)
                {
                    ProcessEx.Start(appPath, cmdLine, true);
                    return;
                }

                var iniPath = Path.ChangeExtension(PathEx.LocalPath, ".ini");
                if (string.IsNullOrEmpty(iniPath))
                    return;

                const string steamAppsDefVar = "%CurDir%\\Data\\steamapps";
                if (!File.Exists(iniPath))
                {
                    var iniContent = new[]
                    {
                        "[Settings]",
                        "",
                        "; True to clear caches when Steam is closed; otherwise, False.",
                        "ImproveSteamStartTime=True",
                        "",
                        "; This option allows forwarding of the default steam game install directory.",
                        "; Steam handles this directory as if would be still on the default location.",
                        $"SteamAppsPathOverride={steamAppsDefVar}",
                        "",
                        "; Set start arguments that are used when Steam Portable creates a new instance.",
                        "; Options: https://developer.valvesoftware.com/wiki/Command_Line_Options",
                        ";StartArguments=-silent steam://friends/status/online"
                    };
                    File.WriteAllLines(iniPath, iniContent);
                }

                var steamAppsVar = Ini.Read("Settings", "SteamAppsPathOverride", steamAppsDefVar, iniPath);
                if (!steamAppsVar.EqualsEx(steamAppsDefVar))
                {
                    var steamAppsDir = PathEx.Combine(steamAppsVar);
                    if (!PathEx.IsValidPath(steamAppsDir))
                        steamAppsVar = steamAppsDefVar;
                }

                var defServiceDir = PathEx.Combine("%CommonProgramFiles(x86)%\\Steam");
                var serviceDir = PathEx.Combine(PathEx.LocalDir, "App\\Service");
                var dirMap = new Dictionary<string, string>
                {
                    {
                        defServiceDir,
                        serviceDir
                    },
                    {
                        "%LocalAppData%\\Steam",
                        "%CurDir%\\Data\\cache"
                    },
                    {
                        "%CurDir%\\App\\Steam\\steamapps",
                        steamAppsVar
                    }
                };

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                foreach (var d in dirMap.Keys.Skip(2))
                {
                    var dir = PathEx.Combine(d);
                    if (!Directory.Exists(dir) || Data.DirIsLink(dir))
                        continue;
                    using (var p = ProcessEx.Send(string.Format(Resources.ForceMove, dir, dataDir), true, false))
                        if (p?.HasExited == false)
                            p.WaitForExit();
                    if (!Directory.Exists(dir))
                        continue;
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        if (Data.ForceDelete(dir, true))
                            continue;
                        if (Ini.ReadDirect("Cache", "Hard", iniPath).EqualsEx("True"))
                        {
                            Ini.WriteDirect("Cache", null, null, iniPath);
                            continue;
                        }
                        Ini.WriteDirect("Cache", "Hard", true, iniPath);
                        ProcessEx.Start(PathEx.LocalPath, EnvironmentEx.CommandLine(false), true);
                    }
                }
                if (Ini.ReadDirect("Cache", "Hard", iniPath).EqualsEx("True"))
                    Ini.WriteDirect("Cache", null, null, iniPath);

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);

                var scMap = new Dictionary<string, string>
                {
                    {
                        "%CurDir%\\App\\Steam\\appcache",
                        "%CurDir%\\Data\\shortcuts\\appcache"
                    },
                    {
                        "%CurDir%\\App\\Steam\\config",
                        "%CurDir%\\Data\\shortcuts\\config"
                    },
                    {
                        "%CurDir%\\App\\Steam\\depotcache",
                        "%CurDir%\\Data\\shortcuts\\depotcache"
                    },
                    {
                        "%CurDir%\\App\\Steam\\dumps",
                        "%CurDir%\\Data\\shortcuts\\dumps"
                    },
                    {
                        "%CurDir%\\App\\Steam\\htmlcache",
                        "%CurDir%\\Data\\shortcuts\\htmlcache"
                    },
                    {
                        "%CurDir%\\App\\Steam\\logs",
                        "%CurDir%\\Data\\shortcuts\\logs"
                    },
                    {
                        "%CurDir%\\App\\Steam\\music",
                        "%CurDir%\\Data\\shortcuts\\music"
                    },
                    {
                        "%CurDir%\\App\\Steam\\remoteui",
                        "%CurDir%\\Data\\shortcuts\\remoteui"
                    },
                    {
                        "%CurDir%\\App\\Steam\\skins",
                        "%CurDir%\\Data\\shortcuts\\skins"
                    },
                    {
                        "%CurDir%\\App\\Steam\\userdata",
                        "%CurDir%\\Data\\shortcuts\\userdata"
                    },
                    {
                        "%CurDir%\\App\\Steam\\vrpanorama",
                        "%CurDir%\\Data\\shortcuts\\vrpanorama"
                    }
                };

                var scDir = Path.Combine(dataDir, "shortcuts");
                foreach (var data in scMap)
                {
                    var dir1 = PathEx.Combine(data.Key);
                    var dir2 = PathEx.Combine(data.Value);
                    try
                    {
                        if (Directory.Exists(dir2))
                        {
                            Data.DirCopy(dir2, dir1, true, true);
                            Directory.Delete(dir2, true);
                        }
                        if (!Directory.Exists(dir1))
                            Directory.CreateDirectory(dir1);
                        if (!Directory.Exists(scDir))
                            Directory.CreateDirectory(scDir);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                    Data.CreateShortcut(dir1, dir2);
                }

                var regKeys = new[]
                {
                    "HKCU\\Software\\Valve",
                    "HKLM\\SOFTWARE\\Valve"
#if !x86
                    ,
                    "HKLM\\SOFTWARE\\Wow6432Node\\Valve"
#endif
                };

                Helper.RegForwarding(Helper.Options.Start, regKeys);

                for (var i = 0; i < regKeys.Length; i++)
                {
                    var regKey = $"{regKeys[i]}\\Steam";
                    switch (i)
                    {
                        case 0:
                            Reg.Write(regKey, "SteamExe", PathEx.AltCombine(appPath).ToLower());
                            Reg.Write(regKey, "SteamPath", PathEx.AltCombine(appDir).ToLower());
                            Reg.Write(regKey, "SourceModInstallPath", PathEx.AltCombine(appDir, "steamapps\\sourcemods").ToLower());

                            regKey = $"{regKey}\\ActiveProcess";
                            Reg.Write(regKey, "SteamClientDll", PathEx.AltCombine(appDir, "steamclient.dll").ToLower());
                            Reg.Write(regKey, "SteamClientDll64", PathEx.AltCombine(appDir, "steamclient64.dll").ToLower());
                            break;
                        case 1:
#if !x86
                        case 2:
#endif
                            Reg.Write(regKey, "InstallPath", appDir);

#if !x86
                            if (i == 1)
                                continue;
#endif

                            regKey = $"{regKey}\\Apps\\CommonRedist";
                            Reg.Write($"{regKey}\\.NET\\3.5", "3.5 SP1", 1, RegistryValueKind.DWord);
                            Reg.Write($"{regKey}\\.NET\\3.5 Client Profile", "3.5 Client Profile SP1", 1, RegistryValueKind.DWord);
                            Reg.Write($"{regKey}\\.NET\\4.0", "4.0", 1, RegistryValueKind.DWord);
                            Reg.Write($"{regKey}\\.NET\\4.0 Client Profile", "4.0 Client Profile", 1, RegistryValueKind.DWord);
                            Reg.Write($"{regKey}\\.NET\\4.5", "4.5", 1, RegistryValueKind.DWord);
                            Reg.Write($"{regKey}\\.NET\\4.5.1", "4.5.1", 1, RegistryValueKind.DWord);
                            Reg.Write($"{regKey}\\.NET\\4.5.2", "4.5.2", 1, RegistryValueKind.DWord);
                            Reg.Write($"{regKey}\\.NET\\4.6", "4.6", 1, RegistryValueKind.DWord);
                            Reg.Write($"{regKey}\\.NET\\4.6.1", "4.6.1", 1, RegistryValueKind.DWord);

                            var value = File.Exists(PathEx.Combine("%system%\\d3d9.dll")) ? 1 : 0;
                            Reg.Write($"{regKey}\\DirectX", "June2010", value, RegistryValueKind.DWord);
                            Reg.Write($"{regKey}\\DirectX\\Jun2010", "dxsetup", value, RegistryValueKind.DWord);

                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2005X86) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2005", "x86 SP1", value, RegistryValueKind.DWord);

                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2005X64) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2005", "x64 SP1", value, RegistryValueKind.DWord);

                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2008X86) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2008", "x86 SP1", value, RegistryValueKind.DWord);
                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2008X64) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2008", "x64 SP1", value, RegistryValueKind.DWord);

                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2010X86) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2010", "x86", value, RegistryValueKind.DWord);
                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2010X64) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2010", "x64", value, RegistryValueKind.DWord);

                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2012X86) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2012", "x86 Update 2", value, RegistryValueKind.DWord);
                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2012X64) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2012", "x64 Update 2", value, RegistryValueKind.DWord);

                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2013X86) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2013", "x86 Update 1", value, RegistryValueKind.DWord);
                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2013X64) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2013", "x64 Update 1", value, RegistryValueKind.DWord);

                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2015X86) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2015", "x86", value, RegistryValueKind.DWord);
                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2015X64) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2015", "x64", value, RegistryValueKind.DWord);

                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2017X86) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2017", "x86", value, RegistryValueKind.DWord);
                            value = EnvironmentEx.Redist.IsInstalled(EnvironmentEx.Redist.Flags.VC2017X64) ? 1 : 0;
                            Reg.Write($"{regKey}\\vcredist\\2017", "x64", value, RegistryValueKind.DWord);
                            break;
                    }
                }

                const string serviceName = "Steam Client Service";
                if (!Service.Exists(serviceName))
                {
                    try
                    {
                        var binSrvPath = Path.Combine(appDir, "bin\\steamservice.exe");
                        var srvPath = Path.Combine(serviceDir, "SteamService.exe");
                        File.Copy(binSrvPath, srvPath, true);
                        binSrvPath = Path.ChangeExtension(binSrvPath, ".dll");
                        srvPath = Path.ChangeExtension(srvPath, ".dll");
                        File.Copy(binSrvPath, srvPath, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                    var defSrvPath = Path.Combine(defServiceDir, "SteamService.exe");
                    Service.Install(serviceName, serviceName, defSrvPath, "/RunAsService");
                    Service.Start(serviceName);
                }

                var startArgs = Ini.Read("Settings", "StartArguments", iniPath);
                if (!string.IsNullOrWhiteSpace(startArgs))
                    cmdLine = $"{startArgs} {cmdLine}".Trim();

                Helper.ApplicationStart(appPath, cmdLine);

                if (Service.Exists(serviceName))
                {
                    Service.Stop(serviceName);
                    Service.Uninstall(serviceName);
                }

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);

                Helper.RegForwarding(Helper.Options.Exit, regKeys);

                var regSecureMap = new Dictionary<string, Dictionary<string, string>>
                {
                    {
                        "-HKCR\\steam", null
                    }
                };
                Helper.RegSecureOverwrite(regSecureMap);

                if (!Ini.Read("Settings", "ImproveSteamStartTime", false, iniPath))
                    return;
                if (Directory.Exists(scDir))
                    try
                    {
                        Directory.Delete(scDir, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                var patternDict = new Dictionary<string, SearchOption>
                {
                    { ".crash", SearchOption.TopDirectoryOnly },
                    { "*.old", SearchOption.TopDirectoryOnly },
                    { "*.log", SearchOption.AllDirectories },
                    { "*.log.last", SearchOption.TopDirectoryOnly },
                    { "ClientRegistry.blob", SearchOption.TopDirectoryOnly }
                };
                foreach (var p in patternDict)
                    try
                    {
                        foreach (var f in Directory.EnumerateFiles(appDir, p.Key, p.Value))
                            try
                            {
                                File.Delete(f);
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                var dirs = new[]
                {
                    Path.Combine(appDir, "appcache"),
                    Path.Combine(appDir, "depotcache"),
                    Path.Combine(appDir, "dumps"),
                    Path.Combine(appDir, "htmlcache"),
                    Path.Combine(dataDir, "cache")
                };
                foreach (var d in dirs)
                    try
                    {
                        if (Directory.Exists(d))
                            Directory.Delete(d, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
            }
        }
    }
}
