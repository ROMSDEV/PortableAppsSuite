namespace FrapsPortable
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using Properties;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            var appPath = PathEx.Combine(@"%CurDir%\App\Fraps\fraps.exe");
            var appProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
            if (!File.Exists(appPath) || appProcess.Length > 0 && appProcess[0].StartInfo.FileName == appPath)
                Environment.Exit(-1);
#if x86
            var appPath64 = PathEx.Combine(@"%CurDir%\Fraps64Portable.exe");
            if (Environment.Is64BitOperatingSystem && File.Exists(appPath64))
            {
                ProcessEx.Start(appPath64);
                Environment.Exit(Environment.ExitCode);
            }
#endif
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                if (!newInstance)
                    return;
#if x86
                if (!Reg.EntryExists(@"HKLM\SOFTWARE\Fraps", "Portable App"))
                    Reg.MoveSubKey(@"HKLM\SOFTWARE\Fraps", @"HKLM\SOFTWARE\SI13N7-BACKUP: Fraps");
                Reg.Write(@"HKLM\SOFTWARE\Fraps", "Install_Dir", Path.GetDirectoryName(appPath));
#else
                if (!Reg.EntryExists(@"HKLM\SOFTWARE\Wow6432Node\Fraps", "Portable App"))
                    Reg.MoveSubKey(@"HKLM\SOFTWARE\Wow6432Node\Fraps", @"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Fraps");
                Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Fraps", "Install_Dir", Path.GetDirectoryName(appPath));
#endif
                var regHelperPath = PathEx.Combine($@"%TEMP%\{appPath.EncryptToMd5()}.reg");
                if (File.Exists(regHelperPath))
                    File.Delete(regHelperPath);
                File.WriteAllText(regHelperPath, Resources.RegHelper0);
                if (File.Exists(regHelperPath))
                {
                    Reg.ImportFile(regHelperPath);
                    File.Delete(regHelperPath);
                }

                var dataPath = PathEx.Combine(@"%CurDir%\Data");
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);

                if (!Reg.EntryExists(@"HKCU\SOFTWARE\Fraps3", "Portable App"))
                    Reg.MoveSubKey(@"HKCU\SOFTWARE\Fraps3", @"HKCU\SOFTWARE\SI13N7-BACKUP: Fraps");

                var settingsPath = Path.Combine(dataPath, "settings.reg");
                if (File.Exists(settingsPath))
                    Reg.ImportFile(settingsPath);

                Reg.Write(@"HKCU\SOFTWARE\Fraps3", "Portable App", true);

                Reg.Write(@"HKCU\SOFTWARE\Fraps3", "Directory", Path.GetDirectoryName(appPath));

                var tmp = Reg.ReadString(@"HKCU\SOFTWARE\Fraps3", "Benchmark Directory");
                var outputPath = Path.Combine(dataPath, "Output");
                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);
                if (string.IsNullOrWhiteSpace(tmp) || tmp.Contains("portable"))
                    Reg.Write(@"HKCU\SOFTWARE\Fraps3", "Benchmark Directory", outputPath);
                tmp = Reg.ReadString(@"HKCU\SOFTWARE\Fraps3", "Movie Directory");
                if (string.IsNullOrWhiteSpace(tmp) || tmp.Contains("portable"))
                    Reg.Write(@"HKCU\SOFTWARE\Fraps3", "Movie Directory", outputPath);
                tmp = Reg.ReadString(@"HKCU\SOFTWARE\Fraps3", "Screenshot Directory");
                if (string.IsNullOrWhiteSpace(tmp) || tmp.Contains("portable"))
                    Reg.Write(@"HKCU\SOFTWARE\Fraps3", "Screenshot Directory", outputPath);

                var driverPath = PathEx.Combine(@"%CurDir%\App\Driver");
#if x86
                using (var p = ProcessEx.Send(string.Format("COPY /Y \"{0}\\SysWOW64\\{1}\" \"%WinDir%\\System32\\{1}\"", driverPath, "frapsvid.dll"), true, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
#else
                using (var p = ProcessEx.Send(string.Format("COPY /Y \"{0}\\{1}\" \"%WinDir%\\{1}\" && COPY /Y \"{0}\\{2}\" \"%WinDir%\\{2}\"", driverPath, @"System32\frapsv64.dll", @"SysWOW64\frapsvid.dll"), true, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
#endif
#if x86
                if (!Reg.SubKeyExists(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Fraps"))
#else
                if (!Reg.SubKeyExists(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Fraps"))
#endif
                {
                    if (File.Exists(regHelperPath))
                        File.Delete(regHelperPath);
                    File.WriteAllText(regHelperPath, Resources.RegHelper1);
                    if (File.Exists(regHelperPath))
                    {
                        Reg.ImportFile(regHelperPath);
                        File.Delete(regHelperPath);
                    }
#if x86
                    Reg.Write(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Fraps", "UninstallString", PathEx.Combine(Path.GetDirectoryName(appPath), "uninstall.exe"));
#else
                    Reg.Write(@"HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Fraps", "UninstallString", PathEx.Combine(Path.GetDirectoryName(appPath), "uninstall.exe"));
#endif
                }

                using (var p = ProcessEx.Start(appPath, true, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();

                appProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
                while (appProcess.Length > 0)
                {
                    foreach (var p in appProcess)
                        p.WaitForExit();
                    appProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
                }

                Reg.ExportKeys(settingsPath, @"HKCU\SOFTWARE\Fraps3");
                Reg.RemoveSubKey(@"HKCU\SOFTWARE\Fraps3");
#if x86
                Reg.RemoveSubKey(@"HKLM\SOFTWARE\Fraps");
#else
                Reg.RemoveSubKey(@"HKLM\SOFTWARE\Wow6432Node\Fraps");
#endif

                var backupMode = Reg.MoveSubKey(@"HKCU\SOFTWARE\SI13N7-BACKUP: Fraps3", @"HKCU\SOFTWARE\Fraps3");
#if x86
                if (!backupMode && Reg.MoveSubKey(@"HKLM\SOFTWARE\SI13N7-BACKUP: Fraps", @"HKLM\SOFTWARE\Fraps"))
                    backupMode = true;
#else
                if (!backupMode && Reg.MoveSubKey(@"HKLM\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Fraps", @"HKLM\SOFTWARE\Fraps"))
                    backupMode = true;
#endif
                if (backupMode)
                    return;
                if (File.Exists(regHelperPath))
                    File.Delete(regHelperPath);
                File.WriteAllText(regHelperPath, Resources.RegHelper2);
                if (File.Exists(regHelperPath))
                {
                    Reg.ImportFile(regHelperPath);
                    File.Delete(regHelperPath);
                }
#if x86
                File.WriteAllText(regHelperPath, Resources.RegHelper3);
                if (File.Exists(regHelperPath))
                {
                    Reg.ImportFile(regHelperPath);
                    File.Delete(regHelperPath);
                }
                using (var p = ProcessEx.Send("DEL /F /Q \"%WinDir%\\System32\\frapsvid.dll\"", true, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
#else
                File.WriteAllText(regHelperPath, Resources.RegHelper4);
                if (File.Exists(regHelperPath))
                {
                    Reg.ImportFile(regHelperPath);
                    File.Delete(regHelperPath);
                }

                using (var p = ProcessEx.Send("/C DEL /F /Q \"%WinDir%\\System32\\frapsv64.dll\" && DEL /F /Q \"%WinDir%\\SysWOW64\\frapsvid.dll\"", true, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
#endif
            }
        }
    }
}
