using SilDev;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SteamPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            LOG.AllowDebug();
            string appDir = PATH.Combine("%CurDir%\\App\\Steam");
            string cmdLineArgs = Environment.CommandLine.Replace(Application.ExecutablePath, string.Empty).Replace("\"\"", string.Empty).Trim();
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
#if x86
                    if (Environment.Is64BitOperatingSystem)
                    {
                        RUN.App(new ProcessStartInfo()
                        {
                            Arguments = cmdLineArgs,
                            FileName = $"%CurDir%\\{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}64.exe"
                        });
                        return;
                    }
#endif
                    if (isRunning(appDir, "steam"))
                        killAll(appDir, "steam");

                    string defServiceDir = PATH.Combine("%CommonProgramFiles(x86)%\\Steam");
                    string serviceDir = PATH.Combine("%CurDir%\\App\\Service");
                    DATA.DirLink(defServiceDir, serviceDir, true);

                    string defCacheDir = PATH.Combine("%LocalAppData%\\Steam");
                    string cacheDir = PATH.Combine("%CurDir%\\Data\\Cache");
                    DATA.DirLink(defCacheDir, cacheDir, true);

                    string iniPath = PATH.Combine($"%CurDir%\\{Path.GetFileNameWithoutExtension(Application.ExecutablePath).Replace("64", string.Empty)}.ini");
                    if (File.Exists(iniPath))
                    {
                        bool improveSteamStart = false;
                        bool.TryParse(INI.Read("Settings", "ImproveSteamStartTime", iniPath), out improveSteamStart);
                        if (improveSteamStart)
                        {
                            try
                            {
                                foreach (string f in Directory.GetFiles(appDir, "*.old"))
                                    File.Delete(f);
                                foreach (string f in Directory.GetFiles(appDir, "*.log"))
                                    File.Delete(f);
                                foreach (string f in Directory.GetFiles(appDir, "*.log.last"))
                                    File.Delete(f);
                                string file = Path.Combine(appDir, ".crash");
                                if (File.Exists(file))
                                    File.Delete(file);
                                file = Path.Combine(appDir, "ClientRegistry.blob");
                                if (File.Exists(file))
                                    File.Delete(file);
                                string dir = Path.Combine(appDir, "appcache");
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
                                LOG.Debug(ex);
                            }
                        }
                        string steamAppsDir = INI.Read("Settings", "SteamAppsPathOverride", iniPath);
                        if (!string.IsNullOrWhiteSpace(steamAppsDir))
                        {
                            string defaultSteamAppsPath = Path.Combine(appDir, "steamapps");
                            steamAppsDir = PATH.Combine(steamAppsDir);
                            DATA.DirLink(defaultSteamAppsPath, steamAppsDir, true);
                        }
                    }

                    if (!REG.ValueExist("HKCU\\Software\\Valve", "Portable App"))
                        REG.RenameSubKey("HKCU\\Software\\Valve", "Software\\SI13N7-BACKUP: Valve");
#if !x86
                    if (!REG.ValueExist("HKLM\\SOFTWARE\\Wow6432Node\\Valve", "Portable App"))
                        REG.RenameSubKey("HKLM\\SOFTWARE\\Wow6432Node\\Valve", "SOFTWARE\\Wow6432Node\\SI13N7-BACKUP: Valve");
#endif
                    if (!REG.ValueExist("HKLM\\SOFTWARE\\Valve", "Portable App"))
                        REG.RenameSubKey("HKLM\\SOFTWARE\\Valve", "SOFTWARE\\SI13N7-BACKUP: Valve");

                    string settingsKeyPath = PATH.Combine("%CurDir%\\Data\\settings.reg");
                    REG.ImportFile(settingsKeyPath);

                    REG.WriteValue("HKCU\\Software\\Valve", "Portable App", "True");
                    REG.WriteValue("HKCU\\Software\\Valve\\Steam", "SteamExe", Path.Combine(appDir, "Steam.exe").Replace("\\", "/").ToLower());
                    REG.WriteValue("HKCU\\Software\\Valve\\Steam", "SteamPath", appDir.Replace("\\", "/").ToLower());
                    REG.WriteValue("HKCU\\Software\\Valve\\Steam", "SourceModInstallPath", Path.Combine(appDir, "SteamApps\\sourcemods").Replace("\\", "/").ToLower());
                    REG.WriteValue("HKCU\\Software\\Valve\\Steam\\ActiveProcess", "SteamClientDll", Path.Combine(appDir, "steamclient.dll").Replace("\\", "/").ToLower());
                    REG.WriteValue("HKCU\\Software\\Valve\\Steam\\ActiveProcess", "SteamClientDll64", Path.Combine(appDir, "steamclient64.dll").Replace("\\", "/").ToLower());

                    REG.WriteValue("HKLM\\SOFTWARE\\Valve", "Portable App", "True");
                    REG.WriteValue("HKLM\\SOFTWARE\\Valve\\Steam", "InstallPath", appDir);
#if !x86
                    REG.WriteValue("HKLM\\SOFTWARE\\Wow6432Node\\Valve", "Portable App", "True");
                    REG.WriteValue("HKLM\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "InstallPath", appDir);
#endif
                    string serviceName = "Steam Client Service";
                    if (ELEVATION.IsAdministrator && !SERVICE.Exists(serviceName))
                    {
                        try
                        {
                            string binServicePath = Path.Combine(appDir, "bin\\steamservice.exe");
                            string servicePath = Path.Combine(serviceDir, "SteamService.exe");
                            File.Copy(binServicePath, servicePath, true);
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                        SERVICE.Install(serviceName, serviceName, Path.Combine(defServiceDir, "SteamService.exe"), "/RunAsService");
                        SERVICE.Start(serviceName);
                    }

                    if (File.Exists(iniPath))
                    {
                        cmdLineArgs = $"{INI.Read("Settings", "StartArguments", iniPath)} {cmdLineArgs}";
                        cmdLineArgs = cmdLineArgs.Trim();
                    }

                    RUN.App(new ProcessStartInfo()
                    {
                        Arguments = cmdLineArgs,
                        FileName = Path.Combine(appDir, "Steam.exe")
                    }, 0);
                    for (int i = 0; i < 10; i++)
                    {
                        while (isRunning(appDir, "Steam"))
                            Thread.Sleep(200);
                        Thread.Sleep(300);
                    }

                    if (ELEVATION.IsAdministrator && SERVICE.Exists(serviceName) && DATA.DirIsLink(defServiceDir))
                    {
                        SERVICE.Stop(serviceName);
                        SERVICE.Uninstall(serviceName);
                    }

                    REG.ImportFile(new string[] { "Windows Registry Editor Version 5.00", "[-HKEY_CLASSES_ROOT\\steam]", Environment.NewLine }, true);

                    string tempRegPath = PATH.Combine("%CurDir%\\Data\\temp.reg");
                    string regFileContent = string.Empty;
                    try
                    {
                        if (File.Exists(tempRegPath))
                            File.Delete(tempRegPath);
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }
                    REG.ExportFile("HKCU\\Software\\Valve", tempRegPath);
                    if (File.Exists(tempRegPath))
                    {
                        regFileContent = File.ReadAllText(tempRegPath);
                        try
                        {
                            File.Delete(tempRegPath);
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                    }

                    REG.ExportFile("HKLM\\SOFTWARE\\Valve", tempRegPath);
                    if (File.Exists(tempRegPath))
                    {
                        if (string.IsNullOrWhiteSpace(regFileContent))
                            regFileContent = File.ReadAllText(tempRegPath);
                        else
                        {
                            string[] lines = File.ReadAllLines(tempRegPath);
                            for (int i = 2; i < lines.Length; i++)
                            {
                                string line = string.Format("{0}{1}", lines[i], Environment.NewLine);
                                regFileContent += line;
                            }
                        }
                        try
                        {
                            File.Delete(tempRegPath);
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                    }
#if !x86
                    REG.ExportFile("HKLM\\SOFTWARE\\Wow6432Node\\Valve", tempRegPath);
                    if (File.Exists(tempRegPath))
                    {
                        if (string.IsNullOrWhiteSpace(regFileContent))
                            regFileContent = File.ReadAllText(tempRegPath);
                        else
                        {
                            string[] lines = File.ReadAllLines(tempRegPath);
                            for (int i = 2; i < lines.Length; i++)
                            {
                                string line = string.Format("{0}{1}", lines[i], Environment.NewLine);
                                regFileContent += line;
                            }
                        }
                        try
                        {
                            File.Delete(tempRegPath);
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                    }
#endif
                    REG.RemoveExistSubKey("HKCU\\Software\\Valve");
                    REG.RemoveExistSubKey("HKLM\\SOFTWARE\\Valve");
#if !x86
                    REG.RemoveExistSubKey("HKLM\\SOFTWARE\\Wow6432Node\\Valve");
#endif
                    REG.RemoveValue("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", "Steam");

                    REG.RenameSubKey("HKCU\\Software\\SI13N7-BACKUP: Valve", "Software\\Valve");
                    REG.RenameSubKey("HKLM\\SOFTWARE\\SI13N7-BACKUP: Valve", "SOFTWARE\\Valve");
#if !x86
                    REG.RenameSubKey("HKLM\\SOFTWARE\\Wow6432Node\\SI13N7-BACKUP: Valve", "SOFTWARE\\Wow6432Node\\Valve");
#endif
                    try
                    {
                        if (File.Exists(settingsKeyPath))
                        {
                            foreach (Process p in Process.GetProcessesByName("reg"))
                                if (p.StartInfo.Arguments.ToLower().Contains(settingsKeyPath.ToLower()))
                                    p.Kill();
                            File.Delete(settingsKeyPath);
                        }
                        if (!File.Exists(settingsKeyPath))
                            File.WriteAllText(settingsKeyPath, regFileContent);
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }

                    DATA.DirUnLink(defServiceDir, true);
                    DATA.DirUnLink(defCacheDir, true);

                    if (File.Exists(iniPath))
                    {
                        string steamAppsPath = INI.Read("Settings", "SteamAppsPathOverride", iniPath);
                        if (!string.IsNullOrWhiteSpace(steamAppsPath))
                        {
                            string defaultSteamAppsPath = Path.Combine(appDir, "steamapps");
                            DATA.DirUnLink(defaultSteamAppsPath, true);
                        }
                    }
                }
                else
                    RUN.App(new ProcessStartInfo()
                    {
                        Arguments = cmdLineArgs,
                        FileName = Path.Combine(appDir, "Steam.exe")
                    }, 0);
            }
        }

        #region HELPER

        static bool isRunning(string _path, string _match)
        {
            try
            {
                foreach (string f in Directory.GetFiles(_path, "*.exe", SearchOption.AllDirectories))
                {
                    string name = Path.GetFileNameWithoutExtension(f).ToLower();
                    if (name.Contains(_match.ToLower()) && Process.GetProcessesByName(name).Length > 0)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
                return false;
            }
        }

        static void killAll(string _path, string _match)
        {
            try
            {
                foreach (string f in Directory.GetFiles(_path, "*.exe", SearchOption.AllDirectories))
                {
                    string name = Path.GetFileNameWithoutExtension(f).ToLower();
                    if (!name.Contains(_match.ToLower()))
                        continue;
                    foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(f)))
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
                LOG.Debug(ex);
            }
        }

        #endregion
    }
}
