using SilDev;
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
            LOG.AllowDebug();
            string appDir = PATH.Combine(@"%CurDir%\App\Origin");
            string appPath = Path.Combine(appDir, "Origin.exe");
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
                    if (isRunning(appDir, "origin"))
                        killAll(appDir, "origin");

                    #region Link Directories

                    DATA.DirLink
                    (
                        PATH.Combine(@"%SystemDrive%\temp"),
                        PATH.Combine(@"%CurDir%\Data\Temp"), true
                    );

                    DATA.DirLink
                    (
                        PATH.Combine(@"%CommonProgramFiles(x86)%\EAInstaller"),
                        PATH.Combine(@"%CurDir%\App\Common\EAInstaller"), true
                    );

                    DATA.DirLink
                    (
                        PATH.Combine(@"%AppData%\EA Games"),
                        PATH.Combine(@"%CurDir%\Data\AppData\Roaming\EA Games"), true
                    );

                    DATA.DirLink
                    (
                        PATH.Combine(@"%AppData%\Electronic Arts"),
                        PATH.Combine(@"%CurDir%\Data\AppData\Roaming\Electronic Arts"), true
                    );

                    DATA.DirLink
                    (
                        PATH.Combine(@"%AppData%\Origin"),
                        PATH.Combine(@"%CurDir%\Data\AppData\Roaming\Origin"), true
                    );

                    DATA.DirLink
                    (
                        PATH.Combine(@"%LocalAppData%\EA Games"),
                        PATH.Combine(@"%CurDir%\Data\AppData\Local\EA Games"), true
                    );

                    DATA.DirLink
                    (
                        PATH.Combine(@"%LocalAppData%\Electronic Arts"),
                        PATH.Combine(@"%CurDir%\Data\AppData\Local\Electronic Arts"), true
                    );

                    DATA.DirLink
                    (
                        PATH.Combine(@"%LocalAppData%\Origin"),
                        PATH.Combine(@"%CurDir%\Data\AppData\Local\Origin"), true
                    );

                    DATA.DirLink
                    (
                        PATH.Combine(@"%ProgramData%\EA Games"),
                        PATH.Combine(@"%CurDir%\Data\ProgramData\EA Games"), true
                    );

                    DATA.DirLink
                    (
                        PATH.Combine(@"%ProgramData%\Electronic Arts"),
                        PATH.Combine(@"%CurDir%\Data\ProgramData\Electronic Arts"), true
                    );

                    DATA.DirLink
                    (
                        PATH.Combine(@"%ProgramData%\Origin"),
                        PATH.Combine(@"%CurDir%\Data\ProgramData\Origin"), true
                    );

                    #endregion

                    #region Registry Backup

                    if (!REG.ValueExist(@"HKCU\origin", "Portable App"))
                        REG.RenameSubKey(@"HKCU\origin", @"SOFTWARE\SI13N7-BACKUP: origin");

                    if (!REG.ValueExist(@"HKCU\origin2", "Portable App"))
                        REG.RenameSubKey(@"HKCU\origin2", @"SOFTWARE\SI13N7-BACKUP: origin2");

                    if (!REG.ValueExist(@"HKCU\Software\EA Games", "Portable App"))
                        REG.RenameSubKey(@"HKCU\Software\EA Games", @"SOFTWARE\SI13N7-BACKUP: EA Games");

                    if (!REG.ValueExist(@"HKCU\Software\Electronic Arts", "Portable App"))
                        REG.RenameSubKey(@"HKCU\Software\Electronic Arts", @"SOFTWARE\SI13N7-BACKUP: Electronic Arts");

                    if (!REG.ValueExist(@"HKLM\SOFTWARE\EA Games", "Portable App"))
                        REG.RenameSubKey(@"HKLM\SOFTWARE\EA Games", @"SOFTWARE\SI13N7-BACKUP: EA Games");

                    if (!REG.ValueExist(@"HKLM\SOFTWARE\Electronic Arts", "Portable App"))
                        REG.RenameSubKey(@"HKLM\SOFTWARE\Electronic Arts", @"SOFTWARE\SI13N7-BACKUP: Electronic Arts");

                    if (!REG.ValueExist(@"HKLM\SOFTWARE\Origin", "Portable App"))
                        REG.RenameSubKey(@"HKLM\SOFTWARE\Origin", @"SOFTWARE\SI13N7-BACKUP: Origin");

                    if (!REG.ValueExist(@"HKLM\SOFTWARE\Origin Games", "Portable App"))
                        REG.RenameSubKey(@"HKLM\SOFTWARE\Origin Games", @"SOFTWARE\SI13N7-BACKUP: Origin Games");
#if !x86
                    if (!REG.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\EA Games", "Portable App"))
                        REG.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\EA Games", @"SOFTWARE\Wow6432Node\SI13N7-BACKUP: EA Games");

                    if (!REG.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts", "Portable App"))
                        REG.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts", @"SOFTWARE\Wow6432Node\SI13N7-BACKUP: Electronic Arts");

                    if (!REG.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Portable App"))
                        REG.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\Origin", @"SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin");

                    if (!REG.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\Origin Games", "Portable App"))
                        REG.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\Origin Games", @"SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin Games");

                    if (!REG.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App"))
                        REG.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin");
#else
                    if (!REG.ValueExist(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App"))
                        REG.RenameSubKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin");
#endif
                    #endregion

                    #region Registry Import

                    string regPath = PATH.Combine(@"%CurDir%\Data\settings.reg");
                    if (!File.Exists(regPath))
                    {
                        REG.WriteValue(@"HKCU\Software\EA Games", "Portable App", "True");
                        REG.WriteValue(@"HKCU\Software\Electronic Arts", "Portable App", "True");

                        REG.WriteValue(@"HKLM\SOFTWARE\EA Games", "Portable App", "True");
                        REG.WriteValue(@"HKLM\SOFTWARE\Electronic Arts", "Portable App", "True");
                        REG.WriteValue(@"HKLM\SOFTWARE\Origin", "Portable App", "True");
                        REG.WriteValue(@"HKLM\SOFTWARE\Origin Games", "Portable App", "True");
#if x86
                        REG.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App", "True");

                        REG.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "ClientVersion", "7.0.0.1");
                        REG.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "EADM6Version", "7.0.0.1");
                        REG.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EADM", "ClientVersion", "9.5.20.5318");
                        REG.WriteValue(@"HKLM\SOFTWARE\Origin", "AutopatchGlobal", "false");
                        REG.WriteValue(@"HKLM\SOFTWARE\Origin", "Autoupdate", "false");
                        REG.WriteValue(@"HKLM\SOFTWARE\Origin", "InstallSuccesfull", "true");
                        REG.WriteValue(@"HKLM\SOFTWARE\Origin", "IsBeta", "false");
                        REG.WriteValue(@"HKLM\SOFTWARE\Origin", "Launch", "21375453");
                        REG.WriteValue(@"HKLM\SOFTWARE\Origin", "ShowDirPage", "true");
                        REG.WriteValue(@"HKLM\SOFTWARE\Origin", "TelemOO", "false");

                        REG.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "Origin");
                        REG.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Publisher", "Electronic Arts, Inc.");
                        REG.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "9.5.20.5318");
                        REG.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "URLInfoAbout", "http://www.ea.com");
                        REG.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoModify", 1);
                        REG.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoRepair", 1);
#else
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App", "True");

                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\EA Games", "Portable App", "True");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts", "Portable App", "True");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Portable App", "True");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin Games", "Portable App", "True");

                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "ClientVersion", "7.0.0.1");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "EADM6Version", "7.0.0.1");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EADM", "ClientVersion", "9.5.20.5318");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "AutopatchGlobal", "false");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Autoupdate", "false");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "InstallSuccesfull", "true");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "IsBeta", "false");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Launch", "21375453");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "ShowDirPage", "true");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "TelemOO", "false");

                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "Origin");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Publisher", "Electronic Arts, Inc.");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "9.5.20.5318");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "URLInfoAbout", "http://www.ea.com");
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoModify", 1);
                        REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoRepair", 1);
#endif
                        RUN.App(StartInfoHelper(appPath, "/Register"), 0);
                        REG.WriteValue(@"HKCR\origin", "Portable App", "True");
                        REG.WriteValue(@"HKCR\origin2", "Portable App", "True");
                    }
                    else
                        REG.ImportFile(regPath, true);

                    REG.WriteValue(@"HKEY_CLASSES_ROOT\origin\shell\open\command", null, string.Format("\"{0}\" \"%1\"", appPath));
                    REG.WriteValue(@"HKEY_CLASSES_ROOT\origin2\shell\open\command", null, string.Format("\"{0}\" \"%1\"", appPath));
#if !x86
                    REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "ClientAccessDLLPath", Path.Combine(appDir, @"LegacyPM\CmdPortalClient.dll"));
                    REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "ClientPath", Path.Combine(appDir, @"LegacyPM\OriginLegacyCLI.dll"));
                    REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "EADM6InstallDir", appDir);
                    REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EADM", "ClientPath", Path.Combine(appDir, @"Origin.exe"));
                    REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "ClientPath", Path.Combine(appDir, @"Origin.exe"));

                    REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "UninstallString", Path.Combine(appDir, "OriginUninstall.exe"));
                    REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "InstallLocation", appDir);
                    REG.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayIcon", Path.Combine(appDir, "OriginUninstall.exe"));
#else
                    REG.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "ClientAccessDLLPath", Path.Combine(appDir, @"LegacyPM\CmdPortalClient.dll"));
                    REG.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "ClientPath", Path.Combine(appDir, @"LegacyPM\OriginLegacyCLI.dll"));
                    REG.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "EADM6InstallDir", appDir);
                    REG.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EADM", "ClientPath", Path.Combine(appDir, @"Origin.exe"));
                    REG.WriteValue(@"HKLM\SOFTWARE\Origin", "ClientPath", Path.Combine(appDir, @"Origin.exe"));

                    REG.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "UninstallString", Path.Combine(appDir, "OriginUninstall.exe"));
                    REG.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "InstallLocation", appDir);
                    REG.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayIcon", Path.Combine(appDir, "OriginUninstall.exe"));
#endif
                    #endregion

                    string iniPath = PATH.Combine($@"%CurDir%\{Path.GetFileNameWithoutExtension(Application.ExecutablePath).Replace("64", string.Empty)}.ini");

                    bool StartMinimized = false;
                    bool.TryParse(INI.Read("Settings", "StartMinimized", iniPath), out StartMinimized);
                    if (StartMinimized)
                        MinimizedAtStart = 0;

                    bool RunPunkBusterOnlyWithOrigin = false;
                    bool.TryParse(INI.Read("Settings", "RunPunkBusterOnlyWithOrigin", iniPath), out RunPunkBusterOnlyWithOrigin);
                    if (RunPunkBusterOnlyWithOrigin)
                    {
                        RUN.Cmd("sc config \"PnkBstrA\" start= auto");
                        RUN.Cmd("sc start \"PnkBstrA\"");
                        RUN.Cmd("sc config \"PnkBstrB\" start= auto");
                    }

                    RUN.App(StartInfoHelper(appPath, cmdLineArgs));
                    for (int i = 0; i < 10; i++)
                    {
                        while (isRunning(appDir, "origin"))
                            Thread.Sleep(200);
                        Thread.Sleep(300);
                    }

                    string serviceName = "Origin Client Service";
                    if (ELEVATION.IsAdministrator && SERVICE.Exists(serviceName) && !REG.SubKeyExist(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin"))
                    {
                        SERVICE.Stop(serviceName);
                        SERVICE.Uninstall(serviceName);
                    }

                    # region Registry Export

                    string tempRegPath = PATH.Combine("%CurDir%\\Data\\temp.reg");
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
                                REG.ExportFile(@"HKCR\origin", tempRegPath);
                                break;
                            case 1:
                                REG.ExportFile(@"HKCR\origin2", tempRegPath);
                                break;
                            case 2: 
                                REG.ExportFile(@"HKCU\Software\EA Games", tempRegPath);
                                break;
                            case 3: 
                                REG.ExportFile(@"HKCU\Software\Electronic Arts", tempRegPath);
                                break;
                            case 4: 
                                REG.ExportFile(@"HKLM\SOFTWARE\EA Games", tempRegPath);
                                break;
                            case 5: 
                                REG.ExportFile(@"HKLM\SOFTWARE\Electronic Arts", tempRegPath);
                                break;
                            case 6: 
                                REG.ExportFile(@"HKLM\SOFTWARE\Origin", tempRegPath);
                                break;
#if x86
                            case 7:
                                REG.ExportFile(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", tempRegPath);
                                break;
#else
                            case 7:
                                REG.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\EA Games", tempRegPath);
                                break;
                            case 8:
                                REG.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts", tempRegPath);
                                break;
                            case 9:
                                REG.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\Origin", tempRegPath);
                                break;
                            case 10:
                                REG.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\Origin Games", tempRegPath);
                                break;
                            case 11:
                                REG.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", tempRegPath);
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

                    REG.RemoveExistSubKey(@"HKCR\origin");
                    REG.RemoveExistSubKey(@"HKCR\origin2");
                    REG.RemoveExistSubKey(@"HKCU\Software\EA Games");
                    REG.RemoveExistSubKey(@"HKCU\Software\Electronic Arts");
                    REG.RemoveExistSubKey(@"HKLM\SOFTWARE\EA Games");
                    REG.RemoveExistSubKey(@"HKLM\SOFTWARE\Electronic Arts");
                    REG.RemoveExistSubKey(@"HKLM\SOFTWARE\Origin");
                    REG.RemoveExistSubKey(@"HKLM\SOFTWARE\Origin Games");
                    REG.RemoveExistSubKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin");

                    REG.RenameSubKey(@"HKCR\SI13N7-BACKUP: origin", @"origin");
                    REG.RenameSubKey(@"HKCR\SI13N7-BACKUP: origin2", @"origin2");
                    REG.RenameSubKey(@"HKCU\Software\SI13N7-BACKUP: EA Games", @"Software\EA Games");
                    REG.RenameSubKey(@"HKCU\Software\SI13N7-BACKUP: Electronic Arts", @"Software\Electronic Arts");
                    REG.RenameSubKey(@"HKLM\SOFTWARE\SI13N7-BACKUP: EA Games", @"SOFTWARE\EA Games");
                    REG.RenameSubKey(@"HKLM\SOFTWARE\SI13N7-BACKUP: Electronic Arts", @"SOFTWARE\Electronic Arts");
                    REG.RenameSubKey(@"HKLM\SOFTWARE\SI13N7-BACKUP: Origin", @"SOFTWARE\Origin");
                    REG.RenameSubKey(@"HKLM\SOFTWARE\SI13N7-BACKUP: Origin Games", @"SOFTWARE\Wow6432Node\Origin Games");
#if x86
                    REG.RenameSubKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin");
#else
                    REG.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\EA Games");
                    REG.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts");
                    REG.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\Origin");
                    REG.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\Origin Games");
                    REG.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin");

                    REG.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: EA Games", @"SOFTWARE\Wow6432Node\EA Games");
                    REG.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Electronic Arts", @"SOFTWARE\Wow6432Node\Electronic Arts");
                    REG.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin", @"SOFTWARE\Wow6432Node\Origin");
                    REG.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin Games", @"SOFTWARE\Wow6432Node\Origin Games");
                    REG.RenameSubKey(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin", @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin");
#endif
                    #endregion

                    #region Unlink Directories

                    DATA.DirUnLink(PATH.Combine(@"%SystemDrive%\temp"), true);
                    DATA.DirUnLink(PATH.Combine(@"%CommonProgramFiles(x86)%\EAInstaller"), true);

                    DATA.DirUnLink(PATH.Combine(@"%AppData%\EA Games"), true);
                    DATA.DirUnLink(PATH.Combine(@"%AppData%\Electronic Arts"), true);
                    DATA.DirUnLink(PATH.Combine(@"%AppData%\Origin"), true);

                    DATA.DirUnLink(PATH.Combine(@"%LocalAppData%\EA Games"), true);
                    DATA.DirUnLink(PATH.Combine(@"%LocalAppData%\Electronic Arts"), true);
                    DATA.DirUnLink(PATH.Combine(@"%LocalAppData%\Origin"), true);

                    DATA.DirUnLink(PATH.Combine(@"%ProgramData%\EA Games"), true);
                    DATA.DirUnLink(PATH.Combine(@"%ProgramData%\Electronic Arts"), true);
                    DATA.DirUnLink(PATH.Combine(@"%ProgramData%\Origin"), true);

                    #endregion

                    if (RunPunkBusterOnlyWithOrigin)
                    {
                        RUN.Cmd("sc stop \"PnkBstrA\"");
                        RUN.Cmd("sc config \"PnkBstrA\" start= disabled");
                        RUN.Cmd("sc stop \"PnkBstrB\"");
                        RUN.Cmd("sc config \"PnkBstrB\" start= disabled");
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
            
            string appDir = PATH.Combine(@"%CurDir%\App");
            psi.EnvironmentVariables["ProgramFiles"] = appDir;
            psi.EnvironmentVariables["ProgramFiles(x86)"] = appDir;
            psi.EnvironmentVariables["ProgramW6432"] = appDir;

            string cAppDir = Path.Combine(appDir, "Common");
            if (!Directory.Exists(cAppDir))
                Directory.CreateDirectory(cAppDir);
            psi.EnvironmentVariables["CommonProgramFiles"] = cAppDir;
            psi.EnvironmentVariables["CommonProgramFiles(x86)"] = cAppDir;
            psi.EnvironmentVariables["CommonProgramW6432"] = psi.EnvironmentVariables["CommonProgramFiles"];

            string dataDir = PATH.Combine(@"%CurDir%\Data");
            if (!Directory.Exists(Path.Combine(dataDir, "Desktop")))
                Directory.CreateDirectory(Path.Combine(dataDir, "Desktop"));
            psi.EnvironmentVariables["ALLUSERSPROFILE"] = Path.Combine(dataDir, @"ProgramData");
            psi.EnvironmentVariables["USERPROFILE"] = dataDir;
            psi.EnvironmentVariables["APPDATA"] = Path.Combine(dataDir, @"AppData\Roaming");
            psi.EnvironmentVariables["LOCALAPPDATA"] = Path.Combine(dataDir, @"AppData\Local");
            psi.EnvironmentVariables["PUBLIC"] = psi.EnvironmentVariables["USERPROFILE"];
            psi.EnvironmentVariables["ProgramData"] = psi.EnvironmentVariables["ALLUSERSPROFILE"];
            psi.EnvironmentVariables["IGOLogPath"] = psi.EnvironmentVariables["ALLUSERSPROFILE"];

			string curDir = PATH.GetEnvironmentVariableValue("CurDir");
            psi.EnvironmentVariables["HOMEDRIVE"] = curDir.EndsWith(@"\") ? curDir.Substring(0, curDir.Length - 1) : curDir;
            psi.EnvironmentVariables["HOMEPATH"] = @"\Data";

            psi.EnvironmentVariables["Path"] = string.Format("{0}\\System32;{0};{0}\\System32\\Wbem;{1}", Environment.GetFolderPath(Environment.SpecialFolder.Windows), curDir);
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
                                    WINAPI.SafeNativeMethods.ShowWindow(p.MainWindowHandle, (int)WINAPI.ShowWindowFunc.SW_MINIMIZE);
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
