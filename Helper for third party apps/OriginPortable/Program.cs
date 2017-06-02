namespace OriginPortable
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
            var appDir = PathEx.Combine(@"%CurDir%\App\Origin");
            var appPath = Path.Combine(appDir, "Origin.exe");
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                if (!newInstance)
                    return;
#if x86
                if (Environment.Is64BitOperatingSystem)
                {
                    ProcessEx.Start($"%CurDir%\\{Path.GetFileNameWithoutExtension(PathEx.LocalPath)}64.exe", EnvironmentEx.CommandLine(false));
                    return;
                }
#endif
                if (IsRunning(appDir))
                    KillAll(appDir);
                var dataDir = PathEx.Combine(@"%CurDir%\Data");
                if (IsRunning(dataDir))
                    KillAll(dataDir);

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

                var defKeys = new[]
                {
                    @"HKCR\origin",
                    @"HKCR\origin2",
                    @"HKCU\Software\EA Games",
                    @"HKCU\Software\Electronic Arts",
                    @"HKLM\SOFTWARE\EA Games",
                    @"HKLM\SOFTWARE\Electronic Arts",
                    @"HKLM\SOFTWARE\Origin",
#if x86
                    @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin",
#else
                    @"HKLM\SOFTWARE\Wow6432Node\EA Games",
                    @"HKLM\SOFTWARE\Wow6432Node\Electronic Arts",
                    @"HKLM\SOFTWARE\Wow6432Node\Origin",
                    @"HKLM\SOFTWARE\Wow6432Node\Origin Games",
                    @"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin"
#endif
                };

                var bakKeys = new[]
                {
                    @"HKCR\SI13N7-BACKUP: origin",
                    @"HKCR\SI13N7-BACKUP: origin2",
                    @"HKCU\Software\SI13N7-BACKUP: EA Games",
                    @"HKCU\Software\SI13N7-BACKUP: Electronic Arts",
                    @"HKLM\SOFTWARE\SI13N7-BACKUP: EA Games",
                    @"HKLM\SOFTWARE\SI13N7-BACKUP: Electronic Arts",
                    @"HKLM\SOFTWARE\SI13N7-BACKUP: Origin",
#if x86
                    @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin",
#else
                    @"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: EA Games",
                    @"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Electronic Arts",
                    @"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin",
                    @"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Origin Games",
                    @"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\SI13N7-BACKUP: Origin"
#endif
                };

                for (var i = 0; i < defKeys.Length; i++)
                    if (!Reg.EntryExists(defKeys[i], "Portable App"))
                        Reg.MoveSubKey(defKeys[i], bakKeys[i]);

                var regPath = Path.Combine(dataDir, "settings.reg");
                if (!File.Exists(regPath))
                {
                    Reg.Write(@"HKCU\Software\EA Games", "Portable App", "True");
                    Reg.Write(@"HKCU\Software\Electronic Arts", "Portable App", "True");

                    Reg.Write(@"HKLM\SOFTWARE\EA Games", "Portable App", "True");
                    Reg.Write(@"HKLM\SOFTWARE\Electronic Arts", "Portable App", "True");
                    Reg.Write(@"HKLM\SOFTWARE\Origin", "Portable App", "True");
                    Reg.Write(@"HKLM\SOFTWARE\Origin Games", "Portable App", "True");
#if x86
                    Reg.Write(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App", "True");

                    Reg.Write(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "ClientVersion", "7.0.0.1");
                    Reg.Write(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "EADM6Version", "7.0.0.1");
                    Reg.Write(@"HKLM\SOFTWARE\Electronic Arts\EADM", "ClientVersion", "9.5.20.5318");
                    Reg.Write(@"HKLM\SOFTWARE\Origin", "AutopatchGlobal", "false");
                    Reg.Write(@"HKLM\SOFTWARE\Origin", "Autoupdate", "false");
                    Reg.Write(@"HKLM\SOFTWARE\Origin", "InstallSuccesfull", "true");
                    Reg.Write(@"HKLM\SOFTWARE\Origin", "IsBeta", "false");
                    Reg.Write(@"HKLM\SOFTWARE\Origin", "Launch", "21375453");
                    Reg.Write(@"HKLM\SOFTWARE\Origin", "ShowDirPage", "true");
                    Reg.Write(@"HKLM\SOFTWARE\Origin", "TelemOO", "false");

                    Reg.Write(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "Origin");
                    Reg.Write(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Publisher", "Electronic Arts, Inc.");
                    Reg.Write(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "9.5.20.5318");
                    Reg.Write(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "URLInfoAbout", "http://www.ea.com");
                    Reg.Write(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoModify", 1);
                    Reg.Write(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoRepair", 1);
#else
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Portable App", "True");

                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\EA Games", "Portable App", "True");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts", "Portable App", "True");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Portable App", "True");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Origin Games", "Portable App", "True");

                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "ClientVersion", "7.0.0.1");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "EADM6Version", "7.0.0.1");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EADM", "ClientVersion", "9.5.20.5318");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Origin", "AutopatchGlobal", "false");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Autoupdate", "false");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Origin", "InstallSuccesfull", "true");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Origin", "IsBeta", "false");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Origin", "Launch", "21375453");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Origin", "ShowDirPage", "true");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Origin", "TelemOO", "false");

                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "Origin");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "Publisher", "Electronic Arts, Inc.");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayName", "9.5.20.5318");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "URLInfoAbout", "http://www.ea.com");
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoModify", 1);
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "NoRepair", 1);
#endif
                    using (var p = ProcessEx.Start(StartInfoHelper(appPath, "/Register"), false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();

                    Reg.Write(@"HKCR\origin", "Portable App", "True");
                    Reg.Write(@"HKCR\origin2", "Portable App", "True");
                }
                else
                    Reg.ImportFile(regPath, true);

                Reg.Write(@"HKEY_CLASSES_ROOT\origin\shell\open\command", null, $"\"{appPath}\" \"%1\"");
                Reg.Write(@"HKEY_CLASSES_ROOT\origin2\shell\open\command", null, $"\"{appPath}\" \"%1\"");
#if !x86
                Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "ClientAccessDLLPath", Path.Combine(appDir, @"LegacyPM\CmdPortalClient.dll"));
                Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "ClientPath", Path.Combine(appDir, @"LegacyPM\OriginLegacyCLI.dll"));
                Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EA Core", "EADM6InstallDir", appDir);
                Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Electronic Arts\EADM", "ClientPath", Path.Combine(appDir, @"Origin.exe"));
                Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Origin", "ClientPath", Path.Combine(appDir, @"Origin.exe"));

                Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "UninstallString", Path.Combine(appDir, "OriginUninstall.exe"));
                Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "InstallLocation", appDir);
                Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayIcon", Path.Combine(appDir, "OriginUninstall.exe"));
#else
                Reg.Write(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "ClientAccessDLLPath", Path.Combine(appDir, @"LegacyPM\CmdPortalClient.dll"));
                Reg.Write(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "ClientPath", Path.Combine(appDir, @"LegacyPM\OriginLegacyCLI.dll"));
                Reg.Write(@"HKLM\SOFTWARE\Electronic Arts\EA Core", "EADM6InstallDir", appDir);
                Reg.Write(@"HKLM\SOFTWARE\Electronic Arts\EADM", "ClientPath", Path.Combine(appDir, @"Origin.exe"));
                Reg.Write(@"HKLM\SOFTWARE\Origin", "ClientPath", Path.Combine(appDir, @"Origin.exe"));

                Reg.Write(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "UninstallString", Path.Combine(appDir, "OriginUninstall.exe"));
                Reg.Write(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "InstallLocation", appDir);
                Reg.Write(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Origin", "DisplayIcon", Path.Combine(appDir, "OriginUninstall.exe"));
#endif
                ProcessEx.Start(StartInfoHelper(appPath, EnvironmentEx.CommandLine(false)));
                for (var i = 0; i < 10; i++)
                {
                    while (IsRunning(appDir) || IsRunning(dataDir))
                        Thread.Sleep(200);
                    Thread.Sleep(300);
                }
                CloseServices();

                Reg.ExportKeys(regPath, defKeys);
                foreach (var key in defKeys)
                    Reg.RemoveSubKey(key);
                for (var i = 0; i < bakKeys.Length; i++)
                    Reg.MoveSubKey(bakKeys[i], defKeys[i]);

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
            }
        }

        private static ProcessStartInfo StartInfoHelper(string appPath, string cmdLine)
        {
            var psi = new ProcessStartInfo();
            if (!string.IsNullOrWhiteSpace(cmdLine))
                psi.Arguments = cmdLine;
            psi.FileName = appPath;
            psi.WindowStyle = ProcessWindowStyle.Minimized;
            return psi;
        }

        private static bool IsRunning(string path)
        {
            try
            {
                var files = Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension).Where(n => n.ContainsEx("Origin") && !n.EqualsEx("OriginClientService", "OriginWebHelperService"));
                return Process.GetProcesses().Any(p => files.ContainsEx(p.ProcessName));
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
                    foreach (var p in Process.GetProcessesByName(name))
                    {
                        if (!p.StartInfo.WorkingDirectory.ContainsEx(path) && !p.StartInfo.FileName.ContainsEx(path))
                            continue;
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
            foreach (var srvName in new []
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
    }
}
