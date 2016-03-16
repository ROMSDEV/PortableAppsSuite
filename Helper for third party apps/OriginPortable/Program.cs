using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace OriginPortable
{
    static class Program
    {
        static string cmdLineArgs = Environment.CommandLine.Replace(Application.ExecutablePath, string.Empty).Replace("\"\"", string.Empty).Trim();
        static int MinimizedAtStart = 10;

        [STAThread]
        static void Main()
        {
            SilDev.Log.AllowDebug();
            string appDir = Path.Combine(Application.StartupPath, @"App\Origin");
            string appPath = Path.Combine(appDir, "Origin.exe");
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
#if x86
                    if (Environment.Is64BitOperatingSystem)
                    {
                        SilDev.Run.App(new ProcessStartInfo()
                        {
                            Arguments = cmdLineArgs,
                            FileName = $"%CurrentDir%\\{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}64.exe"
                        });
                        return;
                    }
#endif
                    if (isRunning(appDir, "origin"))
                        killAll(appDir, "origin");

                    #region Link Directories

                    SilDev.Data.DirLink
                    (
                        SilDev.Run.EnvironmentVariableFilter(@"%SystemDrive%\temp"),
                        SilDev.Run.EnvironmentVariableFilter(@"%CurrentDir%\Data\Temp"), true
                    );

                    SilDev.Data.DirLink
                    (
                        SilDev.Run.EnvironmentVariableFilter(@"%CommonProgramFiles(x86)%\EAInstaller"),
                        SilDev.Run.EnvironmentVariableFilter(@"%CurrentDir%\App\Common\EAInstaller"), true
                    );

                    SilDev.Data.DirLink
                    (
                        SilDev.Run.EnvironmentVariableFilter(@"%AppData%\EA Games"),
                        SilDev.Run.EnvironmentVariableFilter(@"%CurrentDir%\Data\AppData\Roaming\EA Games"), true
                    );

                    SilDev.Data.DirLink
                    (
                        SilDev.Run.EnvironmentVariableFilter(@"%AppData%\Electronic Arts"),
                        SilDev.Run.EnvironmentVariableFilter(@"%CurrentDir%\Data\AppData\Roaming\Electronic Arts"), true
                    );

                    SilDev.Data.DirLink
                    (
                        SilDev.Run.EnvironmentVariableFilter(@"%AppData%\Origin"),
                        SilDev.Run.EnvironmentVariableFilter(@"%CurrentDir%\Data\AppData\Roaming\Origin"), true
                    );

                    SilDev.Data.DirLink
                    (
                        SilDev.Run.EnvironmentVariableFilter(@"%LocalAppData%\EA Games"),
                        SilDev.Run.EnvironmentVariableFilter(@"%CurrentDir%\Data\AppData\Local\EA Games"), true
                    );

                    SilDev.Data.DirLink
                    (
                        SilDev.Run.EnvironmentVariableFilter(@"%LocalAppData%\Electronic Arts"),
                        SilDev.Run.EnvironmentVariableFilter(@"%CurrentDir%\Data\AppData\Local\Electronic Arts"), true
                    );

                    SilDev.Data.DirLink
                    (
                        SilDev.Run.EnvironmentVariableFilter(@"%LocalAppData%\Origin"),
                        SilDev.Run.EnvironmentVariableFilter(@"%CurrentDir%\Data\AppData\Local\Origin"), true
                    );

                    SilDev.Data.DirLink
                    (
                        SilDev.Run.EnvironmentVariableFilter(@"%ProgramData%\EA Games"),
                        SilDev.Run.EnvironmentVariableFilter(@"%CurrentDir%\Data\ProgramData\EA Games"), true
                    );

                    SilDev.Data.DirLink
                    (
                        SilDev.Run.EnvironmentVariableFilter(@"%ProgramData%\Electronic Arts"),
                        SilDev.Run.EnvironmentVariableFilter(@"%CurrentDir%\Data\ProgramData\Electronic Arts"), true
                    );

                    SilDev.Data.DirLink
                    (
                        SilDev.Run.EnvironmentVariableFilter(@"%ProgramData%\Origin"),
                        SilDev.Run.EnvironmentVariableFilter(@"%CurrentDir%\Data\ProgramData\Origin"), true
                    );

                    #endregion

                    #region Registry Backup

                    if (!SilDev.Reg.ValueExist(@"HKCU\origin", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKCU\origin", @"SOFTWARE\SI13N7-BACKUP: origin");

                    if (!SilDev.Reg.ValueExist(@"HKCU\origin2", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKCU\origin2", @"SOFTWARE\SI13N7-BACKUP: origin2");

                    if (!SilDev.Reg.ValueExist(@"HKCU\Software\EA Games", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKCU\Software\EA Games", @"SOFTWARE\SI13N7-BACKUP: EA Games");

                    if (!SilDev.Reg.ValueExist(@"HKCU\Software\Electronic Arts", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKCU\Software\Electronic Arts", @"SOFTWARE\SI13N7-BACKUP: Electronic Arts");

                    if (!SilDev.Reg.ValueExist(@"HKLM\SOFTWARE\EA Games", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\EA Games", @"SOFTWARE\SI13N7-BACKUP: EA Games");

                    if (!SilDev.Reg.ValueExist(@"HKLM\SOFTWARE\Electronic Arts", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Electronic Arts", @"SOFTWARE\SI13N7-BACKUP: Electronic Arts");

                    if (!SilDev.Reg.ValueExist(@"HKLM\SOFTWARE\Origin", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Origin", @"SOFTWARE\SI13N7-BACKUP: Origin");

                    if (!SilDev.Reg.ValueExist(@"HKLM\SOFTWARE\Origin Games", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Origin Games", @"SOFTWARE\SI13N7-BACKUP: Origin Games");
#if !x86
                    if (!SilDev.Reg.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\EA Games", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\EA Games", @"SOFTWARE\Wow6432Node\SI13N7-BACKUP: EA Games");

                    if (!SilDev.Reg.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts", @"SOFTWARE\Wow6432Node\SI13N7-BACKUP: Electronic Arts");

                    if (!SilDev.Reg.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\Origin", @"SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin");

                    if (!SilDev.Reg.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\Origin Games", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\Origin Games", @"SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin Games");

                    if (!SilDev.Reg.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin");
#else
                    if (!SilDev.Reg.ValueExist(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App"))
                        SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin");
#endif
                    #endregion

                    #region Registry Import

                    string regPath = Path.Combine(Application.StartupPath, @"Data\settings.reg");
                    if (!File.Exists(regPath))
                    {
                        SilDev.Reg.WriteValue(@"HKCU\Software\EA Games", "Portable App", "True");
                        SilDev.Reg.WriteValue(@"HKCU\Software\Electronic Arts", "Portable App", "True");

                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\EA Games", "Portable App", "True");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts", "Portable App", "True");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "Portable App", "True");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Origin Games", "Portable App", "True");
#if x86
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App", "True");

                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "ClientVersion", "7.0.0.1");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "EADM6Version", "7.0.0.1");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EADM", "ClientVersion", "9.5.20.5318");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "AutopatchGlobal", "false");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "Autoupdate", "false");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "InstallSuccesfull", "true");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "IsBeta", "false");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "Launch", "21375453");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "ShowDirPage", "true");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "TelemOO", "false");

                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "Origin");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Publisher", "Electronic Arts, Inc.");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "9.5.20.5318");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "URLInfoAbout", "http://www.ea.com");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoModify", 1);
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoRepair", 1);
#else
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App", "True");

                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\EA Games", "Portable App", "True");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts", "Portable App", "True");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Portable App", "True");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin Games", "Portable App", "True");

                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "ClientVersion", "7.0.0.1");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "EADM6Version", "7.0.0.1");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EADM", "ClientVersion", "9.5.20.5318");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "AutopatchGlobal", "false");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Autoupdate", "false");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "InstallSuccesfull", "true");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "IsBeta", "false");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Launch", "21375453");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "ShowDirPage", "true");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "TelemOO", "false");

                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "Origin");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Publisher", "Electronic Arts, Inc.");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "9.5.20.5318");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "URLInfoAbout", "http://www.ea.com");
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoModify", 1);
                        SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoRepair", 1);
#endif
                        SilDev.Run.App(StartInfoHelper(appPath, "/Register"), 0);
                        SilDev.Reg.WriteValue(@"HKCR\origin", "Portable App", "True");
                        SilDev.Reg.WriteValue(@"HKCR\origin2", "Portable App", "True");
                    }
                    else
                        SilDev.Reg.ImportFile(regPath, true);

                    SilDev.Reg.WriteValue(@"HKEY_CLASSES_ROOT\origin\shell\open\command", null, string.Format("\"{0}\" \"%1\"", appPath));
                    SilDev.Reg.WriteValue(@"HKEY_CLASSES_ROOT\origin2\shell\open\command", null, string.Format("\"{0}\" \"%1\"", appPath));
#if !x86
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "ClientAccessDLLPath", Path.Combine(appDir, @"LegacyPM\CmdPortalClient.dll"));
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "ClientPath", Path.Combine(appDir, @"LegacyPM\OriginLegacyCLI.dll"));
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "EADM6InstallDir", appDir);
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EADM", "ClientPath", Path.Combine(appDir, @"Origin.exe"));
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "ClientPath", Path.Combine(appDir, @"Origin.exe"));

                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "UninstallString", Path.Combine(appDir, "OriginUninstall.exe"));
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "InstallLocation", appDir);
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayIcon", Path.Combine(appDir, "OriginUninstall.exe"));
#else
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "ClientAccessDLLPath", Path.Combine(appDir, @"LegacyPM\CmdPortalClient.dll"));
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "ClientPath", Path.Combine(appDir, @"LegacyPM\OriginLegacyCLI.dll"));
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "EADM6InstallDir", appDir);
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EADM", "ClientPath", Path.Combine(appDir, @"Origin.exe"));
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "ClientPath", Path.Combine(appDir, @"Origin.exe"));

                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "UninstallString", Path.Combine(appDir, "OriginUninstall.exe"));
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "InstallLocation", appDir);
                    SilDev.Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayIcon", Path.Combine(appDir, "OriginUninstall.exe"));
#endif
                    #endregion

                    string iniPath = Path.Combine(Application.StartupPath, string.Format("{0}.ini", Path.GetFileNameWithoutExtension(Application.ExecutablePath).Replace("64", string.Empty)));

                    bool StartMinimized = false;
                    bool.TryParse(SilDev.Ini.Read("Settings", "StartMinimized", iniPath), out StartMinimized);
                    if (StartMinimized)
                        MinimizedAtStart = 0;

                    bool RunPunkBusterOnlyWithOrigin = false;
                    bool.TryParse(SilDev.Ini.Read("Settings", "RunPunkBusterOnlyWithOrigin", iniPath), out RunPunkBusterOnlyWithOrigin);
                    if (RunPunkBusterOnlyWithOrigin)
                    {
                        SilDev.Run.Cmd("sc config \"PnkBstrA\" start= auto");
                        SilDev.Run.Cmd("sc start \"PnkBstrA\"");
                        SilDev.Run.Cmd("sc config \"PnkBstrB\" start= auto");
                    }

                    SilDev.Run.App(StartInfoHelper(appPath, cmdLineArgs));
                    for (int i = 0; i < 10; i++)
                    {
                        while (isRunning(appDir, "origin"))
                            Thread.Sleep(200);
                        Thread.Sleep(300);
                    }

                    string serviceName = "Origin Client Service";
                    if (SilDev.Elevation.IsAdministrator && SilDev.WinAPI.ServiceTools.ServiceExists(serviceName) && !SilDev.Reg.SubKeyExist(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin"))
                    {
                        SilDev.WinAPI.ServiceTools.StopService(serviceName);
                        SilDev.WinAPI.ServiceTools.UninstallService(serviceName);
                    }

                    # region Registry Export

                    string tempRegPath = SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\Data\\temp.reg");
                    string regFileContent = string.Empty;
                    if (File.Exists(tempRegPath))
                        File.Delete(tempRegPath);
#if x86
                    int keysCount = 8;
#else
                    int keysCount = 12;
#endif
                    for (int i = 0; i < keysCount; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                SilDev.Reg.ExportFile(@"HKCR\origin", tempRegPath);
                                break;
                            case 1:
                                SilDev.Reg.ExportFile(@"HKCR\origin2", tempRegPath);
                                break;
                            case 2: 
                                SilDev.Reg.ExportFile(@"HKCU\Software\EA Games", tempRegPath);
                                break;
                            case 3: 
                                SilDev.Reg.ExportFile(@"HKCU\Software\Electronic Arts", tempRegPath);
                                break;
                            case 4: 
                                SilDev.Reg.ExportFile(@"HKLM\SOFTWARE\EA Games", tempRegPath);
                                break;
                            case 5: 
                                SilDev.Reg.ExportFile(@"HKLM\SOFTWARE\Electronic Arts", tempRegPath);
                                break;
                            case 6: 
                                SilDev.Reg.ExportFile(@"HKLM\SOFTWARE\Origin", tempRegPath);
                                break;
#if x86
                            case 7:
                                SilDev.Reg.ExportFile(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", tempRegPath);
                                break;
#else
                            case 7:
                                SilDev.Reg.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\EA Games", tempRegPath);
                                break;
                            case 8:
                                SilDev.Reg.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts", tempRegPath);
                                break;
                            case 9:
                                SilDev.Reg.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\Origin", tempRegPath);
                                break;
                            case 10:
                                SilDev.Reg.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\Origin Games", tempRegPath);
                                break;
                            case 11:
                                SilDev.Reg.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", tempRegPath);
                                break;
#endif
                        }
                        if (File.Exists(tempRegPath))
                        {
                            if (string.IsNullOrWhiteSpace(regFileContent))
                                regFileContent = File.ReadAllText(tempRegPath, System.Text.Encoding.ASCII);
                            else
                            {
                                string[] lines = File.ReadAllLines(tempRegPath, System.Text.Encoding.ASCII);
                                for (int j = 2; j < lines.Length; j++)
                                {
                                    string line = string.Format("{0}{1}", lines[j], Environment.NewLine);
                                    regFileContent += line;
                                }
                            }
                            File.Delete(tempRegPath);
                        }
                    }

                    if (File.Exists(regPath))
                        File.Delete(regPath);
                    if (!File.Exists(regPath))
                        File.WriteAllText(regPath, regFileContent.Replace("™", string.Empty).Replace("â„¢", string.Empty), System.Text.Encoding.ASCII);

                    #endregion

                    #region Registry Cleanup

                    SilDev.Reg.RemoveExistSubKey(@"HKCR\origin");
                    SilDev.Reg.RemoveExistSubKey(@"HKCR\origin2");
                    SilDev.Reg.RemoveExistSubKey(@"HKCU\Software\EA Games");
                    SilDev.Reg.RemoveExistSubKey(@"HKCU\Software\Electronic Arts");
                    SilDev.Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\EA Games");
                    SilDev.Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Electronic Arts");
                    SilDev.Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Origin");
                    SilDev.Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Origin Games");
                    SilDev.Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin");

                    SilDev.Reg.RenameSubKey(@"HKCR\SI13N7-BACKUP: origin", @"origin");
                    SilDev.Reg.RenameSubKey(@"HKCR\SI13N7-BACKUP: origin2", @"origin2");
                    SilDev.Reg.RenameSubKey(@"HKCU\Software\SI13N7-BACKUP: EA Games", @"Software\EA Games");
                    SilDev.Reg.RenameSubKey(@"HKCU\Software\SI13N7-BACKUP: Electronic Arts", @"Software\Electronic Arts");
                    SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\SI13N7-BACKUP: EA Games", @"SOFTWARE\EA Games");
                    SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\SI13N7-BACKUP: Electronic Arts", @"SOFTWARE\Electronic Arts");
                    SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\SI13N7-BACKUP: Origin", @"SOFTWARE\Origin");
                    SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\SI13N7-BACKUP: Origin Games", @"SOFTWARE\Wow6432Node\Origin Games");
#if x86
                    SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin");
#else
                    SilDev.Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\EA Games");
                    SilDev.Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts");
                    SilDev.Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\Origin");
                    SilDev.Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\Origin Games");
                    SilDev.Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin");

                    SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: EA Games", @"SOFTWARE\Wow6432Node\EA Games");
                    SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Electronic Arts", @"SOFTWARE\Wow6432Node\Electronic Arts");
                    SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin", @"SOFTWARE\Wow6432Node\Origin");
                    SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin Games", @"SOFTWARE\Wow6432Node\Origin Games");
                    SilDev.Reg.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin", @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin");
#endif
                    #endregion

                    #region Unlink Directories

                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter(@"%SystemDrive%\temp"), true);
                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter(@"%CommonProgramFiles(x86)%\EAInstaller"), true);

                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter(@"%AppData%\EA Games"), true);
                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter(@"%AppData%\Electronic Arts"), true);
                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter(@"%AppData%\Origin"), true);

                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter(@"%LocalAppData%\EA Games"), true);
                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter(@"%LocalAppData%\Electronic Arts"), true);
                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter(@"%LocalAppData%\Origin"), true);

                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter(@"%ProgramData%\EA Games"), true);
                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter(@"%ProgramData%\Electronic Arts"), true);
                    SilDev.Data.DirUnLink(SilDev.Run.EnvironmentVariableFilter(@"%ProgramData%\Origin"), true);

                    #endregion

                    if (RunPunkBusterOnlyWithOrigin)
                    {
                        SilDev.Run.Cmd("sc stop \"PnkBstrA\"");
                        SilDev.Run.Cmd("sc config \"PnkBstrA\" start= disabled");
                        SilDev.Run.Cmd("sc stop \"PnkBstrB\"");
                        SilDev.Run.Cmd("sc config \"PnkBstrB\" start= disabled");
                    }
                }
            }
        }

        static ProcessStartInfo StartInfoHelper(string _appPath, string _cmdLine)
        {
            ProcessStartInfo psi = new ProcessStartInfo();

            if (!string.IsNullOrWhiteSpace(_cmdLine))
                psi.Arguments = _cmdLine;
            psi.FileName = _appPath;
            psi.WindowStyle = ProcessWindowStyle.Minimized;

            #region DISABLED
            /*
            psi.UseShellExecute = false;
            
            string appDir = Path.Combine(Application.StartupPath, "App");
            psi.EnvironmentVariables["ProgramFiles"] = appDir;
            psi.EnvironmentVariables["ProgramFiles(x86)"] = appDir;
            psi.EnvironmentVariables["ProgramW6432"] = appDir;

            string cAppDir = Path.Combine(appDir, "Common");
            if (!Directory.Exists(cAppDir))
                Directory.CreateDirectory(cAppDir);
            psi.EnvironmentVariables["CommonProgramFiles"] = cAppDir;
            psi.EnvironmentVariables["CommonProgramFiles(x86)"] = cAppDir;
            psi.EnvironmentVariables["CommonProgramW6432"] = psi.EnvironmentVariables["CommonProgramFiles"];

            string dataDir = Path.Combine(Application.StartupPath, "Data");
            if (!Directory.Exists(Path.Combine(dataDir, "Desktop")))
                Directory.CreateDirectory(Path.Combine(dataDir, "Desktop"));
            psi.EnvironmentVariables["ALLUSERSPROFILE"] = Path.Combine(dataDir, @"ProgramData");
            psi.EnvironmentVariables["USERPROFILE"] = dataDir;
            psi.EnvironmentVariables["APPDATA"] = Path.Combine(dataDir, @"AppData\Roaming");
            psi.EnvironmentVariables["LOCALAPPDATA"] = Path.Combine(dataDir, @"AppData\Local");
            psi.EnvironmentVariables["PUBLIC"] = psi.EnvironmentVariables["USERPROFILE"];
            psi.EnvironmentVariables["ProgramData"] = psi.EnvironmentVariables["ALLUSERSPROFILE"];
            psi.EnvironmentVariables["IGOLogPath"] = psi.EnvironmentVariables["ALLUSERSPROFILE"];

            psi.EnvironmentVariables["HOMEDRIVE"] = Application.StartupPath.EndsWith(@"\") ? Application.StartupPath.Substring(0, Application.StartupPath.Length - 1) : Application.StartupPath;
            psi.EnvironmentVariables["HOMEPATH"] = @"\Data";

            psi.EnvironmentVariables["Path"] = string.Format("{0}\\System32;{0};{0}\\System32\\Wbem;{1}", Environment.GetFolderPath(Environment.SpecialFolder.Windows), appDir);
            */
            #endregion

            return psi;
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
                    {
                        if (MinimizedAtStart <= 10 && name == "origin")
                        {
                            foreach (Process p in Process.GetProcessesByName(name))
                            {
                                if (p.MainWindowTitle.ToLower() == name)
                                {
                                    MinimizedAtStart++;
                                    SilDev.WinAPI.SafeNativeMethods.ShowWindow(p.MainWindowHandle, SilDev.WinAPI.Win32HookAction.SW_MINIMIZE);
                                }
                            }
                        }
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
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
                SilDev.Log.Debug(ex);
            }
        }

        #endregion
    }
}
