namespace OriginPortable
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using SilDev;

    internal static class Program
    {
        private static int _minimizedAtStart = 10;

        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            var appDir = PathEx.Combine(@"%CurDir%\App\Origin");
            var appPath = Path.Combine(appDir, "Origin.exe");
            var locName = Path.GetFileNameWithoutExtension(PathEx.LocalPath);
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (!newInstance)
                    return;
#if x86
                if (Environment.Is64BitOperatingSystem)
                {
                    ProcessEx.Start($"%CurDir%\\{locName}64.exe", EnvironmentEx.CommandLine(false));
                    return;
                }
#endif
                if (IsRunning(appDir))
                    KillAll(appDir);

                Data.DirLink(@"%SystemDrive%\temp", @"%CurDir%\Data\Temp", true);
                Data.DirLink(@"%CommonProgramFiles(x86)%\EAInstaller", @"%CurDir%\App\Common\EAInstaller", true);
                Data.DirLink(@"%AppData%\EA Games", @"%CurDir%\Data\AppData\Roaming\EA Games", true);
                Data.DirLink(@"%AppData%\Electronic Arts", @"%CurDir%\Data\AppData\Roaming\Electronic Arts", true);
                Data.DirLink(@"%AppData%\Origin", @"%CurDir%\Data\AppData\Roaming\Origin", true);
                Data.DirLink(@"%LocalAppData%\EA Games", @"%CurDir%\Data\AppData\Local\EA Games", true);
                Data.DirLink(@"%LocalAppData%\Electronic Arts", @"%CurDir%\Data\AppData\Local\Electronic Arts", true);
                Data.DirLink(@"%LocalAppData%\Origin", @"%CurDir%\Data\AppData\Local\Origin", true);
                Data.DirLink(@"%ProgramData%\EA Games", @"%CurDir%\Data\ProgramData\EA Games", true);
                Data.DirLink(@"%ProgramData%\Electronic Arts", @"%CurDir%\Data\ProgramData\Electronic Arts", true);
                Data.DirLink(@"%ProgramData%\Origin", @"%CurDir%\Data\ProgramData\Origin", true);

                if (!Reg.ValueExist(@"HKCU\origin", "Portable App"))
                    Reg.MoveSubKey(@"HKCU\origin", @"SOFTWARE\SI13N7-BACKUP: origin");

                if (!Reg.ValueExist(@"HKCU\origin2", "Portable App"))
                    Reg.MoveSubKey(@"HKCU\origin2", @"SOFTWARE\SI13N7-BACKUP: origin2");

                if (!Reg.ValueExist(@"HKCU\Software\EA Games", "Portable App"))
                    Reg.MoveSubKey(@"HKCU\Software\EA Games", @"SOFTWARE\SI13N7-BACKUP: EA Games");

                if (!Reg.ValueExist(@"HKCU\Software\Electronic Arts", "Portable App"))
                    Reg.MoveSubKey(@"HKCU\Software\Electronic Arts", @"SOFTWARE\SI13N7-BACKUP: Electronic Arts");

                if (!Reg.ValueExist(@"HKLM\SOFTWARE\EA Games", "Portable App"))
                    Reg.MoveSubKey(@"HKLM\SOFTWARE\EA Games", @"SOFTWARE\SI13N7-BACKUP: EA Games");

                if (!Reg.ValueExist(@"HKLM\SOFTWARE\Electronic Arts", "Portable App"))
                    Reg.MoveSubKey(@"HKLM\SOFTWARE\Electronic Arts", @"SOFTWARE\SI13N7-BACKUP: Electronic Arts");

                if (!Reg.ValueExist(@"HKLM\SOFTWARE\Origin", "Portable App"))
                    Reg.MoveSubKey(@"HKLM\SOFTWARE\Origin", @"SOFTWARE\SI13N7-BACKUP: Origin");

                if (!Reg.ValueExist(@"HKLM\SOFTWARE\Origin Games", "Portable App"))
                    Reg.MoveSubKey(@"HKLM\SOFTWARE\Origin Games", @"SOFTWARE\SI13N7-BACKUP: Origin Games");
#if !x86
                if (!Reg.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\EA Games", "Portable App"))
                    Reg.MoveSubKey(@"HKLM\SOFTWARE\Wow6432Node\EA Games", @"SOFTWARE\Wow6432Node\SI13N7-BACKUP: EA Games");

                if (!Reg.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts", "Portable App"))
                    Reg.MoveSubKey(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts", @"SOFTWARE\Wow6432Node\SI13N7-BACKUP: Electronic Arts");

                if (!Reg.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Portable App"))
                    Reg.MoveSubKey(@"HKLM\SOFTWARE\Wow6432Node\Origin", @"SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin");

                if (!Reg.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\Origin Games", "Portable App"))
                    Reg.MoveSubKey(@"HKLM\SOFTWARE\Wow6432Node\Origin Games", @"SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin Games");

                if (!Reg.ValueExist(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App"))
                    Reg.MoveSubKey(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin");
#else
                if (!Reg.ValueExist(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App"))
                    Reg.MoveSubKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin");
#endif

                var regPath = PathEx.Combine(@"%CurDir%\Data\settings.reg");
                if (!File.Exists(regPath))
                {
                    Reg.WriteValue(@"HKCU\Software\EA Games", "Portable App", "True");
                    Reg.WriteValue(@"HKCU\Software\Electronic Arts", "Portable App", "True");

                    Reg.WriteValue(@"HKLM\SOFTWARE\EA Games", "Portable App", "True");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts", "Portable App", "True");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "Portable App", "True");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Origin Games", "Portable App", "True");
#if x86
                    Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App", "True");

                    Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "ClientVersion", "7.0.0.1");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "EADM6Version", "7.0.0.1");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EADM", "ClientVersion", "9.5.20.5318");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "AutopatchGlobal", "false");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "Autoupdate", "false");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "InstallSuccesfull", "true");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "IsBeta", "false");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "Launch", "21375453");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "ShowDirPage", "true");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "TelemOO", "false");

                    Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "Origin");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Publisher", "Electronic Arts, Inc.");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "9.5.20.5318");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "URLInfoAbout", "http://www.ea.com");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoModify", 1);
                    Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoRepair", 1);
#else
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App", "True");

                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\EA Games", "Portable App", "True");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts", "Portable App", "True");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Portable App", "True");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin Games", "Portable App", "True");

                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "ClientVersion", "7.0.0.1");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "EADM6Version", "7.0.0.1");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EADM", "ClientVersion", "9.5.20.5318");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "AutopatchGlobal", "false");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Autoupdate", "false");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "InstallSuccesfull", "true");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "IsBeta", "false");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Launch", "21375453");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "ShowDirPage", "true");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "TelemOO", "false");

                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "Origin");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Publisher", "Electronic Arts, Inc.");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "9.5.20.5318");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "URLInfoAbout", "http://www.ea.com");
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoModify", 1);
                    Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoRepair", 1);
#endif
                    using (var p = ProcessEx.Start(StartInfoHelper(appPath, "/Register"), false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();

                    Reg.WriteValue(@"HKCR\origin", "Portable App", "True");
                    Reg.WriteValue(@"HKCR\origin2", "Portable App", "True");
                }
                else
                    Reg.ImportFile(regPath, true);

                Reg.WriteValue(@"HKEY_CLASSES_ROOT\origin\shell\open\command", null, $"\"{appPath}\" \"%1\"");
                Reg.WriteValue(@"HKEY_CLASSES_ROOT\origin2\shell\open\command", null, $"\"{appPath}\" \"%1\"");
#if !x86
                Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "ClientAccessDLLPath", Path.Combine(appDir, @"LegacyPM\CmdPortalClient.dll"));
                Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "ClientPath", Path.Combine(appDir, @"LegacyPM\OriginLegacyCLI.dll"));
                Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "EADM6InstallDir", appDir);
                Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EADM", "ClientPath", Path.Combine(appDir, @"Origin.exe"));
                Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Origin", "ClientPath", Path.Combine(appDir, @"Origin.exe"));

                Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "UninstallString", Path.Combine(appDir, "OriginUninstall.exe"));
                Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "InstallLocation", appDir);
                Reg.WriteValue(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayIcon", Path.Combine(appDir, "OriginUninstall.exe"));
#else
                Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "ClientAccessDLLPath", Path.Combine(appDir, @"LegacyPM\CmdPortalClient.dll"));
                Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "ClientPath", Path.Combine(appDir, @"LegacyPM\OriginLegacyCLI.dll"));
                Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "EADM6InstallDir", appDir);
                Reg.WriteValue(@"HKLM\SOFTWARE\Electronic Arts\EADM", "ClientPath", Path.Combine(appDir, @"Origin.exe"));
                Reg.WriteValue(@"HKLM\SOFTWARE\Origin", "ClientPath", Path.Combine(appDir, @"Origin.exe"));

                Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "UninstallString", Path.Combine(appDir, "OriginUninstall.exe"));
                Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "InstallLocation", appDir);
                Reg.WriteValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayIcon", Path.Combine(appDir, "OriginUninstall.exe"));
#endif
                var iniPath = PathEx.Combine($@"%CurDir%\{locName?.Replace("64", string.Empty)}.ini");

                var startMinimized = Ini.ReadBoolean("Settings", "StartMinimized", iniPath);
                if (startMinimized)
                    _minimizedAtStart = 0;

                var runPunkBusterOnlyWithOrigin = Ini.ReadBoolean("Settings", "RunPunkBusterOnlyWithOrigin", iniPath);
                if (runPunkBusterOnlyWithOrigin)
                {
                    ProcessEx.Send("sc config \"PnkBstrA\" start= auto");
                    ProcessEx.Send("sc start \"PnkBstrA\"");
                    ProcessEx.Send("sc config \"PnkBstrB\" start= auto");
                }

                ProcessEx.Start(StartInfoHelper(appPath, EnvironmentEx.CommandLine(false)));
                for (var i = 0; i < 10; i++)
                {
                    while (IsRunning(appDir))
                        Thread.Sleep(200);
                    Thread.Sleep(300);
                }

#if !x86
                if (Elevation.IsAdministrator && !Reg.SubKeyExist(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin"))
#else
                if (Elevation.IsAdministrator && !Reg.SubKeyExist(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin"))
#endif
                    CloseServices();

                var tempRegPath = PathEx.Combine("%CurDir%\\Data\\temp.reg");
                var regFileContent = string.Empty;
                if (File.Exists(tempRegPath))
                    File.Delete(tempRegPath);
#if x86
                const int keysCount = 8;
#else
                const int keysCount = 12;
#endif
                for (var i = 0; i < keysCount; i++)
                {
                    switch (i)
                    {
                        case 0:
                            Reg.ExportFile(@"HKCR\origin", tempRegPath);
                            break;
                        case 1:
                            Reg.ExportFile(@"HKCR\origin2", tempRegPath);
                            break;
                        case 2:
                            Reg.ExportFile(@"HKCU\Software\EA Games", tempRegPath);
                            break;
                        case 3:
                            Reg.ExportFile(@"HKCU\Software\Electronic Arts", tempRegPath);
                            break;
                        case 4:
                            Reg.ExportFile(@"HKLM\SOFTWARE\EA Games", tempRegPath);
                            break;
                        case 5:
                            Reg.ExportFile(@"HKLM\SOFTWARE\Electronic Arts", tempRegPath);
                            break;
                        case 6:
                            Reg.ExportFile(@"HKLM\SOFTWARE\Origin", tempRegPath);
                            break;
#if x86
                        case 7:
                            Reg.ExportFile(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", tempRegPath);
                            break;
#else
                        case 7:
                            Reg.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\EA Games", tempRegPath);
                            break;
                        case 8:
                            Reg.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts", tempRegPath);
                            break;
                        case 9:
                            Reg.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\Origin", tempRegPath);
                            break;
                        case 10:
                            Reg.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\Origin Games", tempRegPath);
                            break;
                        case 11:
                            Reg.ExportFile(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", tempRegPath);
                            break;
#endif
                    }
                    if (!File.Exists(tempRegPath))
                        continue;
                    if (string.IsNullOrWhiteSpace(regFileContent))
                        regFileContent = File.ReadAllText(tempRegPath, Encoding.ASCII);
                    else
                    {
                        var lines = File.ReadAllLines(tempRegPath, Encoding.ASCII);
                        for (var j = 2; j < lines.Length; j++)
                        {
                            string line = $"{lines[j]}{Environment.NewLine}";
                            regFileContent += line;
                        }
                    }
                    File.Delete(tempRegPath);
                }

                if (File.Exists(regPath))
                    File.Delete(regPath);
                if (!File.Exists(regPath))
                    File.WriteAllText(regPath, regFileContent.Replace("™", string.Empty).Replace("â„¢", string.Empty), Encoding.ASCII);

                Reg.RemoveExistSubKey(@"HKCR\origin");
                Reg.RemoveExistSubKey(@"HKCR\origin2");
                Reg.RemoveExistSubKey(@"HKCU\Software\EA Games");
                Reg.RemoveExistSubKey(@"HKCU\Software\Electronic Arts");
                Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\EA Games");
                Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Electronic Arts");
                Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Origin");
                Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Origin Games");
                Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin");

                Reg.MoveSubKey(@"HKCR\SI13N7-BACKUP: origin", @"origin");
                Reg.MoveSubKey(@"HKCR\SI13N7-BACKUP: origin2", @"origin2");
                Reg.MoveSubKey(@"HKCU\Software\SI13N7-BACKUP: EA Games", @"Software\EA Games");
                Reg.MoveSubKey(@"HKCU\Software\SI13N7-BACKUP: Electronic Arts", @"Software\Electronic Arts");
                Reg.MoveSubKey(@"HKLM\SOFTWARE\SI13N7-BACKUP: EA Games", @"SOFTWARE\EA Games");
                Reg.MoveSubKey(@"HKLM\SOFTWARE\SI13N7-BACKUP: Electronic Arts", @"SOFTWARE\Electronic Arts");
                Reg.MoveSubKey(@"HKLM\SOFTWARE\SI13N7-BACKUP: Origin", @"SOFTWARE\Origin");
                Reg.MoveSubKey(@"HKLM\SOFTWARE\SI13N7-BACKUP: Origin Games", @"SOFTWARE\Wow6432Node\Origin Games");
#if x86
                Reg.MoveSubKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin");
#else
                Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\EA Games");
                Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts");
                Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\Origin");
                Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\Origin Games");
                Reg.RemoveExistSubKey(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin");

                Reg.MoveSubKey(@"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: EA Games", @"SOFTWARE\Wow6432Node\EA Games");
                Reg.MoveSubKey(@"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Electronic Arts", @"SOFTWARE\Wow6432Node\Electronic Arts");
                Reg.MoveSubKey(@"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin", @"SOFTWARE\Wow6432Node\Origin");
                Reg.MoveSubKey(@"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin Games", @"SOFTWARE\Wow6432Node\Origin Games");
                Reg.MoveSubKey(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin", @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin");
#endif

                Data.DirUnLink(@"%SystemDrive%\temp", true);
                Data.DirUnLink(@"%CommonProgramFiles(x86)%\EAInstaller", true);

                Data.DirUnLink(@"%AppData%\EA Games", true);
                Data.DirUnLink(@"%AppData%\Electronic Arts", true);
                Data.DirUnLink(@"%AppData%\Origin", true);

                Data.DirUnLink(@"%LocalAppData%\EA Games", true);
                Data.DirUnLink(@"%LocalAppData%\Electronic Arts", true);
                Data.DirUnLink(@"%LocalAppData%\Origin", true);

                Data.DirUnLink(@"%ProgramData%\EA Games", true);
                Data.DirUnLink(@"%ProgramData%\Electronic Arts", true);
                Data.DirUnLink(@"%ProgramData%\Origin", true);

                if (!runPunkBusterOnlyWithOrigin)
                    return;
                ProcessEx.Send("sc stop \"PnkBstrA\"");
                ProcessEx.Send("sc config \"PnkBstrA\" start= disabled");
                ProcessEx.Send("sc stop \"PnkBstrB\"");
                ProcessEx.Send("sc config \"PnkBstrB\" start= disabled");
            }
        }

        private static ProcessStartInfo StartInfoHelper(string appPath, string cmdLine)
        {
            var psi = new ProcessStartInfo();

            if (!string.IsNullOrWhiteSpace(cmdLine))
                psi.Arguments = cmdLine;
            psi.FileName = appPath;
            psi.WindowStyle = ProcessWindowStyle.Minimized;

            /*
            psi.UseShellExecute = false;

            var appDir = PathEx.Combine(@"%CurDir%\App");
            psi.EnvironmentVariables["ProgramFiles"] = appDir;
            psi.EnvironmentVariables["ProgramFiles(x86)"] = appDir;
            psi.EnvironmentVariables["ProgramW6432"] = appDir;

            var cAppDir = Path.Combine(appDir, "Common");
            if (!Directory.Exists(cAppDir))
                Directory.CreateDirectory(cAppDir);
            psi.EnvironmentVariables["CommonProgramFiles"] = cAppDir;
            psi.EnvironmentVariables["CommonProgramFiles(x86)"] = cAppDir;
            psi.EnvironmentVariables["CommonProgramW6432"] = psi.EnvironmentVariables["CommonProgramFiles"];

            var dataDir = PathEx.Combine(@"%CurDir%\Data");
            if (!Directory.Exists(Path.Combine(dataDir, "Desktop")))
                Directory.CreateDirectory(Path.Combine(dataDir, "Desktop"));
            psi.EnvironmentVariables["ALLUSERSPROFILE"] = Path.Combine(dataDir, @"ProgramData");
            psi.EnvironmentVariables["USERPROFILE"] = dataDir;
            psi.EnvironmentVariables["APPDATA"] = Path.Combine(dataDir, @"AppData\Roaming");
            psi.EnvironmentVariables["LOCALAPPDATA"] = Path.Combine(dataDir, @"AppData\Local");
            psi.EnvironmentVariables["PUBLIC"] = psi.EnvironmentVariables["USERPROFILE"];
            psi.EnvironmentVariables["ProgramData"] = psi.EnvironmentVariables["ALLUSERSPROFILE"];
            psi.EnvironmentVariables["IGOLogPath"] = psi.EnvironmentVariables["ALLUSERSPROFILE"];

            var curDir = EnvironmentEx.GetVariableValue("CurDir");
            psi.EnvironmentVariables["HOMEDRIVE"] = curDir.EndsWith(@"\") ? curDir.Substring(0, curDir.Length - 1) : curDir;
            psi.EnvironmentVariables["HOMEPATH"] = @"\Data";

            psi.EnvironmentVariables["Path"] = string.Format("{0}\\System32;{0};{0}\\System32\\Wbem;{1}", Environment.GetFolderPath(Environment.SpecialFolder.Windows), curDir);
            */

            return psi;
        }

        private static bool IsRunning(string path)
        {
            try
            {
                foreach (var f in Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories))
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    if (!name.ContainsEx("Origin") || name.EqualsEx("OriginClientService", "OriginWebHelperService") || Process.GetProcessesByName(name).Length == 0)
                        continue;
                    if (_minimizedAtStart > 10 || name.EqualsEx("Origin"))
                        return true;
                    foreach (var p in Process.GetProcessesByName(name))
                    {
                        if (!p.MainWindowTitle.EqualsEx(name))
                            continue;
                        var hWnd = WinApi.FindWindowByCaption(p.MainWindowTitle);
                        if (hWnd != IntPtr.Zero)
                            WinApi.UnsafeNativeMethods.ShowWindowAsync(hWnd, WinApi.ShowWindowFunc.SW_SHOWMINIMIZED);
                        _minimizedAtStart++;
                    }
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

        private static void KillAll(string path)
        {
            try
            {
                CloseServices();
                foreach (var f in Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories))
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    if (!name.ContainsEx("Origin"))
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

        private static void CloseServices()
        {
            var srvName = "Origin Client Service";
            if (Service.Exists(srvName))
            {
                Service.Stop(srvName);
                Service.Uninstall(srvName);
            }
            srvName = "Origin Web Helper Service";
            if (!Service.Exists(srvName))
                return;
            Service.Stop(srvName);
            Service.Uninstall(srvName);
        }
    }
}
