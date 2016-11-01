namespace SteamPortable
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
            var appDir = PathEx.Combine("%CurDir%\\App\\Steam");
            var locName = Path.GetFileNameWithoutExtension(PathEx.LocalPath);
            var cmdLineArgs = EnvironmentEx.CommandLine(false);
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                if (newInstance)
                {
#if x86
                    if (Environment.Is64BitOperatingSystem)
                    {
                        ProcessEx.Start($"%CurDir%\\{locName}64.exe", cmdLineArgs, true, false);
                        return;
                    }
#endif
                    if (IsRunning(appDir, "steam"))
                        KillAll(appDir, "steam");

                    var defServiceDir = PathEx.Combine("%CommonProgramFiles(x86)%\\Steam");
                    var serviceDir = PathEx.Combine("%CurDir%\\App\\Service");
                    Data.DirLink(defServiceDir, serviceDir, true);

                    var defCacheDir = PathEx.Combine("%LocalAppData%\\Steam");
                    var cacheDir = PathEx.Combine("%CurDir%\\Data\\Cache");
                    Data.DirLink(defCacheDir, cacheDir, true);

                    var iniPath = PathEx.Combine($"%CurDir%\\{locName?.Replace("64", string.Empty)}.ini");
                    if (File.Exists(iniPath))
                    {
                        bool improveSteamStart;
                        bool.TryParse(Ini.Read("Settings", "ImproveSteamStartTime", iniPath), out improveSteamStart);
                        if (improveSteamStart)
                            try
                            {
                                foreach (var f in Directory.GetFiles(appDir, "*.old"))
                                    File.Delete(f);
                                foreach (var f in Directory.GetFiles(appDir, "*.log"))
                                    File.Delete(f);
                                foreach (var f in Directory.GetFiles(appDir, "*.log.last"))
                                    File.Delete(f);
                                var file = Path.Combine(appDir, ".crash");
                                if (File.Exists(file))
                                    File.Delete(file);
                                file = Path.Combine(appDir, "ClientRegistry.blob");
                                if (File.Exists(file))
                                    File.Delete(file);
                                var dir = Path.Combine(appDir, "appcache");
                                if (Directory.Exists(dir))
                                    Directory.Delete(dir, true);
                                dir = Path.Combine(appDir, "appcache");
                                if (Directory.Exists(dir))
                                    Directory.Delete(dir, true);
                                dir = Path.Combine(appDir, "depotcache");
                                if (Directory.Exists(dir))
                                    Directory.Delete(dir, true);
                                dir = Path.Combine(appDir, "dumps");
                                if (Directory.Exists(dir))
                                    Directory.Delete(dir, true);
                                dir = Path.Combine(appDir, "logs");
                                if (Directory.Exists(dir))
                                    Directory.Delete(dir, true);
                                dir = Path.Combine(cacheDir, "htmlcache");
                                if (Directory.Exists(dir))
                                    Directory.Delete(dir, true);
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }
                        var steamAppsDir = Ini.Read("Settings", "SteamAppsPathOverride", iniPath);
                        if (!string.IsNullOrWhiteSpace(steamAppsDir))
                        {
                            var defaultSteamAppsPath = Path.Combine(appDir, "steamapps");
                            steamAppsDir = PathEx.Combine(steamAppsDir);
                            Data.DirLink(defaultSteamAppsPath, steamAppsDir, true);
                        }
                    }

                    if (!Reg.ValueExist("HKCU\\Software\\Valve", "Portable App"))
                        Reg.MoveSubKey("HKCU\\Software\\Valve", "Software\\SI13N7-BACKUP: Valve");
#if !x86
                    if (!Reg.ValueExist("HKLM\\SOFTWARE\\Wow6432Node\\Valve", "Portable App"))
                        Reg.MoveSubKey("HKLM\\SOFTWARE\\Wow6432Node\\Valve", "SOFTWARE\\Wow6432Node\\SI13N7-BACKUP: Valve");
#endif
                    if (!Reg.ValueExist("HKLM\\SOFTWARE\\Valve", "Portable App"))
                        Reg.MoveSubKey("HKLM\\SOFTWARE\\Valve", "SOFTWARE\\SI13N7-BACKUP: Valve");

                    var settingsKeyPath = PathEx.Combine("%CurDir%\\Data\\settings.reg");
                    Reg.ImportFile(settingsKeyPath);

                    Reg.WriteValue("HKCU\\Software\\Valve", "Portable App", "True");
                    Reg.WriteValue("HKCU\\Software\\Valve\\Steam", "SteamExe", Path.Combine(appDir, "Steam.exe").Replace("\\", "/").ToLower());
                    Reg.WriteValue("HKCU\\Software\\Valve\\Steam", "SteamPath", appDir.Replace("\\", "/").ToLower());
                    Reg.WriteValue("HKCU\\Software\\Valve\\Steam", "SourceModInstallPath", Path.Combine(appDir, "SteamApps\\sourcemods").Replace("\\", "/").ToLower());
                    Reg.WriteValue("HKCU\\Software\\Valve\\Steam\\ActiveProcess", "SteamClientDll", Path.Combine(appDir, "steamclient.dll").Replace("\\", "/").ToLower());
                    Reg.WriteValue("HKCU\\Software\\Valve\\Steam\\ActiveProcess", "SteamClientDll64", Path.Combine(appDir, "steamclient64.dll").Replace("\\", "/").ToLower());

                    Reg.WriteValue("HKLM\\SOFTWARE\\Valve", "Portable App", "True");
                    Reg.WriteValue("HKLM\\SOFTWARE\\Valve\\Steam", "InstallPath", appDir);
#if !x86
                    Reg.WriteValue("HKLM\\SOFTWARE\\Wow6432Node\\Valve", "Portable App", "True");
                    Reg.WriteValue("HKLM\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "InstallPath", appDir);
#endif
                    const string serviceName = "Steam Client Service";
                    if (Elevation.IsAdministrator && !Service.Exists(serviceName))
                    {
                        try
                        {
                            var binServicePath = Path.Combine(appDir, "bin\\steamservice.exe");
                            var servicePath = Path.Combine(serviceDir, "SteamService.exe");
                            File.Copy(binServicePath, servicePath, true);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                        Service.Install(serviceName, serviceName, Path.Combine(defServiceDir, "SteamService.exe"), "/RunAsService");
                        Service.Start(serviceName);
                    }

                    if (File.Exists(iniPath))
                    {
                        cmdLineArgs = $"{Ini.Read("Settings", "StartArguments", iniPath)} {cmdLineArgs}";
                        cmdLineArgs = cmdLineArgs.Trim();
                    }

                    using (var p = ProcessEx.Start(Path.Combine(appDir, "Steam.exe"), cmdLineArgs, true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();

                    for (var i = 0; i < 10; i++)
                    {
                        while (IsRunning(appDir, "Steam"))
                            Thread.Sleep(200);
                        Thread.Sleep(300);
                    }

                    if (Elevation.IsAdministrator && Service.Exists(serviceName) && Data.DirIsLink(defServiceDir))
                    {
                        Service.Stop(serviceName);
                        Service.Uninstall(serviceName);
                    }

                    Reg.ImportFile(new[] { "Windows Registry Editor Version 5.00", "[-HKEY_CLASSES_ROOT\\steam]", Environment.NewLine }, true);

                    var tempRegPath = PathEx.Combine("%CurDir%\\Data\\temp.reg");
                    var regFileContent = string.Empty;
                    try
                    {
                        if (File.Exists(tempRegPath))
                            File.Delete(tempRegPath);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                    Reg.ExportKeys(tempRegPath, "HKCU\\Software\\Valve");
                    if (File.Exists(tempRegPath))
                    {
                        regFileContent = File.ReadAllText(tempRegPath);
                        try
                        {
                            File.Delete(tempRegPath);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    }

                    Reg.ExportKeys(tempRegPath, "HKLM\\SOFTWARE\\Valve");
                    if (File.Exists(tempRegPath))
                    {
                        if (string.IsNullOrWhiteSpace(regFileContent))
                            regFileContent = File.ReadAllText(tempRegPath);
                        else
                        {
                            var lines = File.ReadAllLines(tempRegPath);
                            for (var i = 2; i < lines.Length; i++)
                            {
                                string line = $"{lines[i]}{Environment.NewLine}";
                                regFileContent += line;
                            }
                        }
                        try
                        {
                            File.Delete(tempRegPath);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    }
#if !x86
                    Reg.ExportKeys(tempRegPath, "HKLM\\SOFTWARE\\Wow6432Node\\Valve");
                    if (File.Exists(tempRegPath))
                    {
                        if (string.IsNullOrWhiteSpace(regFileContent))
                            regFileContent = File.ReadAllText(tempRegPath);
                        else
                        {
                            var lines = File.ReadAllLines(tempRegPath);
                            for (var i = 2; i < lines.Length; i++)
                            {
                                string line = $"{lines[i]}{Environment.NewLine}";
                                regFileContent += line;
                            }
                        }
                        try
                        {
                            File.Delete(tempRegPath);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    }
#endif
                    Reg.RemoveExistSubKey("HKCU\\Software\\Valve");
                    Reg.RemoveExistSubKey("HKLM\\SOFTWARE\\Valve");
#if !x86
                    Reg.RemoveExistSubKey("HKLM\\SOFTWARE\\Wow6432Node\\Valve");
#endif
                    Reg.RemoveValue("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", "Steam");

                    Reg.MoveSubKey("HKCU\\Software\\SI13N7-BACKUP: Valve", "Software\\Valve");
                    Reg.MoveSubKey("HKLM\\SOFTWARE\\SI13N7-BACKUP: Valve", "SOFTWARE\\Valve");
#if !x86
                    Reg.MoveSubKey("HKLM\\SOFTWARE\\Wow6432Node\\SI13N7-BACKUP: Valve", "SOFTWARE\\Wow6432Node\\Valve");
#endif
                    try
                    {
                        if (File.Exists(settingsKeyPath))
                        {
                            foreach (var p in Process.GetProcessesByName("reg"))
                                if (p.StartInfo.Arguments.ToLower().Contains(settingsKeyPath.ToLower()))
                                    p.Kill();
                            File.Delete(settingsKeyPath);
                        }
                        if (!File.Exists(settingsKeyPath))
                            File.WriteAllText(settingsKeyPath, regFileContent);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }

                    Data.DirUnLink(defServiceDir, true);
                    Data.DirUnLink(defCacheDir, true);

                    if (!File.Exists(iniPath))
                        return;
                    var steamAppsPath = Ini.Read("Settings", "SteamAppsPathOverride", iniPath);
                    if (string.IsNullOrWhiteSpace(steamAppsPath))
                        return;
                    var defSteamAppsPath = Path.Combine(appDir, "steamapps");
                    Data.DirUnLink(defSteamAppsPath, true);
                }
                else
                    ProcessEx.Start(Path.Combine(appDir, "Steam.exe"), cmdLineArgs, true);
        }

        private static bool IsRunning(string path, string match)
        {
            try
            {
                foreach (var f in Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories))
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    if (name.ContainsEx(match) && Process.GetProcessesByName(name).Length > 0)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        private static void KillAll(string path, string match)
        {
            try
            {
                foreach (var f in Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories))
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    if (!name.ContainsEx(match))
                        continue;
                    foreach (var p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(f)))
                    {
                        p.CloseMainWindow();
                        p.WaitForExit(100);
                        if (!p.HasExited)
                            p.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}
