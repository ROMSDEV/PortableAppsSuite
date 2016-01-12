using System;
using System.Collections.Generic;
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
            SilDev.Log.AllowDebug();
            string appDir = SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\App\\Steam");
            string cmdLineArgs = Environment.CommandLine.Replace(Application.ExecutablePath, string.Empty).Replace("\"\"", string.Empty).TrimStart().TrimEnd();
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
#if x86
                    if (Environment.Is64BitOperatingSystem)
                    {
                        SilDev.Run.App(Application.StartupPath, string.Format("{0}64.exe", Path.GetFileNameWithoutExtension(Application.ExecutablePath)), cmdLineArgs);
                        return;
                    }
#endif
                    if (isRunning(appDir, "steam"))
                        killAll(appDir, "steam");

                    string defServiceDir = SilDev.Run.EnvironmentVariableFilter("%CommonProgramFiles(x86)%\\Steam");
                    string serviceDir = SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\App\\Service");
                    SilDev.Data.DirLink(defServiceDir, serviceDir, true);

                    string defCacheDir = SilDev.Run.EnvironmentVariableFilter("%LocalAppData%\\Steam");
                    string cacheDir = SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\Data\\Cache");
                    SilDev.Data.DirLink(defCacheDir, cacheDir, true);

                    string iniPath = Path.Combine(Application.StartupPath, string.Format("{0}.ini", Path.GetFileNameWithoutExtension(Application.ExecutablePath).Replace("64", string.Empty)));
                    if (File.Exists(iniPath))
                    {
                        bool improveSteamStart = false;
                        bool.TryParse(SilDev.Initialization.ReadValue("Settings", "ImproveSteamStartTime", iniPath), out improveSteamStart);
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
                                SilDev.Log.Debug(ex);
                            }
                        }
                        string steamAppsDir = SilDev.Initialization.ReadValue("Settings", "SteamAppsPathOverride", iniPath);
                        if (!string.IsNullOrWhiteSpace(steamAppsDir))
                        {
                            string defaultSteamAppsPath = Path.Combine(appDir, "steamapps");
                            steamAppsDir = SilDev.Run.EnvironmentVariableFilter(steamAppsDir);
                            SilDev.Data.DirLink(defaultSteamAppsPath, steamAppsDir, true);
                        }
                    }

                    if (SilDev.Reg.SubKeyExist("HKCU\\Software\\Valve") && string.IsNullOrWhiteSpace(SilDev.Reg.ReadValue("HKCU\\Software\\Valve", "Portable App")))
                        SilDev.Reg.RenameSubKey("HKCU\\Software\\Valve", "Software\\SI13N7-BACKUP: Valve");

                    if (SilDev.Reg.SubKeyExist("HKLM\\SOFTWARE\\Wow6432Node\\Valve") && string.IsNullOrWhiteSpace(SilDev.Reg.ReadValue("HKLM\\SOFTWARE\\Wow6432Node\\Valve", "Portable App")))
                        SilDev.Reg.RenameSubKey("HKLM\\SOFTWARE\\Wow6432Node\\Valve", "SOFTWARE\\Wow6432Node\\SI13N7-BACKUP: Valve");

                    if (SilDev.Reg.SubKeyExist("HKLM\\SOFTWARE\\Valve") && string.IsNullOrWhiteSpace(SilDev.Reg.ReadValue("HKLM\\SOFTWARE\\Valve", "Portable App")))
                        SilDev.Reg.RenameSubKey("HKLM\\SOFTWARE\\Valve", "SOFTWARE\\SI13N7-BACKUP: Valve");

                    string settingsKeyPath = SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\Data\\settings.reg");
                    SilDev.Reg.ImportFile(settingsKeyPath);

                    SilDev.Reg.WriteValue("HKCU\\Software\\Valve", "Portable App", "True");
                    SilDev.Reg.WriteValue("HKCU\\Software\\Valve\\Steam", "SteamExe", Path.Combine(appDir, "Steam.exe").Replace("\\", "/").ToLower());
                    SilDev.Reg.WriteValue("HKCU\\Software\\Valve\\Steam", "SteamPath", appDir.Replace("\\", "/").ToLower());
                    SilDev.Reg.WriteValue("HKCU\\Software\\Valve\\Steam", "SourceModInstallPath", Path.Combine(appDir, "SteamApps\\sourcemods").Replace("\\", "/").ToLower());
                    SilDev.Reg.WriteValue("HKCU\\Software\\Valve\\Steam\\ActiveProcess", "SteamClientDll", Path.Combine(appDir, "steamclient.dll").Replace("\\", "/").ToLower());
                    SilDev.Reg.WriteValue("HKCU\\Software\\Valve\\Steam\\ActiveProcess", "SteamClientDll64", Path.Combine(appDir, "steamclient64.dll").Replace("\\", "/").ToLower());

                    SilDev.Reg.WriteValue("HKLM\\SOFTWARE\\Valve", "Portable App", "True");
                    SilDev.Reg.WriteValue("HKLM\\SOFTWARE\\Valve\\Steam", "InstallPath", appDir);
#if !x86
                    SilDev.Reg.WriteValue("HKLM\\SOFTWARE\\Wow6432Node\\Valve", "Portable App", "True");
                    SilDev.Reg.WriteValue("HKLM\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "InstallPath", appDir);
#endif
                    string serviceName = "Steam Client Service";
                    if (SilDev.Elevation.IsAdministrator && !SilDev.ServiceTools.ServiceExists(serviceName))
                    {
                        try
                        {
                            string binServicePath = Path.Combine(appDir, "bin\\steamservice.exe");
                            string servicePath = Path.Combine(serviceDir, "SteamService.exe");
                            File.Copy(binServicePath, servicePath, true);
                        }
                        catch (Exception ex)
                        {
                            SilDev.Log.Debug(ex);
                        }
                        SilDev.ServiceTools.InstallService(serviceName, serviceName, Path.Combine(defServiceDir, "SteamService.exe"), "/RunAsService");
                        SilDev.ServiceTools.StartService(serviceName);
                    }

                    if (File.Exists(iniPath))
                    {
                        cmdLineArgs = string.Format("{0} {1}", SilDev.Initialization.ReadValue("Settings", "StartArguments", iniPath), cmdLineArgs);
                        cmdLineArgs = cmdLineArgs.TrimStart().TrimEnd();
                    }

                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = cmdLineArgs,
                        FileName = Path.Combine(appDir, "Steam.exe")
                    }, 0);
                    while (isRunning(appDir, "Steam"))
                        Thread.Sleep(200);

                    if (SilDev.Elevation.IsAdministrator && SilDev.ServiceTools.ServiceExists(serviceName) && SilDev.Data.DirIsLink(defServiceDir))
                    {
                        SilDev.ServiceTools.StopService(serviceName);
                        SilDev.ServiceTools.UninstallService(serviceName);
                    }

                    SilDev.Reg.ImportFile(new string[] { "Windows Registry Editor Version 5.00", "[-HKEY_CLASSES_ROOT\\steam]", Environment.NewLine }, true);

                    string tempRegPath = SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\Data\\temp.reg");
                    string regFileContent = string.Empty;
                    try
                    {
                        if (File.Exists(tempRegPath))
                            File.Delete(tempRegPath);
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                    SilDev.Reg.ExportFile("HKCU\\Software\\Valve", tempRegPath);
                    if (File.Exists(tempRegPath))
                    {
                        regFileContent = File.ReadAllText(tempRegPath);
                        try
                        {
                            File.Delete(tempRegPath);
                        }
                        catch (Exception ex)
                        {
                            SilDev.Log.Debug(ex);
                        }
                    }

                    SilDev.Reg.ExportFile("HKLM\\SOFTWARE\\Valve", tempRegPath);
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
                            SilDev.Log.Debug(ex);
                        }
                    }
#if !x86
                    SilDev.Reg.ExportFile("HKLM\\SOFTWARE\\Wow6432Node\\Valve", tempRegPath);
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
                            SilDev.Log.Debug(ex);
                        }
                    }
#endif
                    SilDev.Reg.RemoveExistSubKey("HKCU\\Software\\Valve");
                    SilDev.Reg.RemoveExistSubKey("HKLM\\SOFTWARE\\Valve");
#if !x86
                    SilDev.Reg.RemoveExistSubKey("HKLM\\SOFTWARE\\Wow6432Node\\Valve");
#endif
                    SilDev.Reg.RemoveValue("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", "Steam");

                    if (SilDev.Reg.SubKeyExist("HKCU\\Software\\SI13N7-BACKUP: Valve"))
                        SilDev.Reg.RenameSubKey("HKCU\\Software\\SI13N7-BACKUP: Valve", "Software\\Valve");

                    if (SilDev.Reg.SubKeyExist("HKLM\\SOFTWARE\\SI13N7-BACKUP: Valve"))
                        SilDev.Reg.RenameSubKey("HKLM\\SOFTWARE\\SI13N7-BACKUP: Valve", "SOFTWARE\\Valve");
#if !x86
                    if (SilDev.Reg.SubKeyExist("HKLM\\SOFTWARE\\Wow6432Node\\SI13N7-BACKUP: Valve"))
                        SilDev.Reg.RenameSubKey("HKLM\\SOFTWARE\\Wow6432Node\\SI13N7-BACKUP: Valve", "SOFTWARE\\Wow6432Node\\Valve");
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
                        SilDev.Log.Debug(ex);
                    }

                    SilDev.Data.DirUnLink(defServiceDir, true);
                    SilDev.Data.DirUnLink(defCacheDir, true);

                    if (File.Exists(iniPath))
                    {
                        string steamAppsPath = SilDev.Initialization.ReadValue("Settings", "SteamAppsPathOverride", iniPath);
                        if (!string.IsNullOrWhiteSpace(steamAppsPath))
                        {
                            string defaultSteamAppsPath = Path.Combine(appDir, "steamapps");
                            SilDev.Data.DirUnLink(defaultSteamAppsPath, true);
                        }
                    }
                }
                else
                    SilDev.Run.App(new ProcessStartInfo()
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
                SilDev.Log.Debug(ex.Message, "SteamPortable.Program.isRunning");
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
                SilDev.Log.Debug(ex.Message, "SteamPortable.Program.killAll");
            }
        }

        #endregion
    }
}
