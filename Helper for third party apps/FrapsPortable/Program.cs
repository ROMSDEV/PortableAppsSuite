using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace FrapsPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            string appPath = Path.Combine(Application.StartupPath, @"App\Fraps\fraps.exe");
            Process[] appProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
            if (!File.Exists(appPath) || appProcess.Length > 0 && appProcess[0].StartInfo.FileName == appPath)
                Environment.Exit(-1);
#if x86
            string appPath64 = Path.Combine(Application.StartupPath, "Fraps64Portable.exe");
            if (Environment.Is64BitOperatingSystem && File.Exists(appPath64))
            {
                SilDev.Run.App(new ProcessStartInfo() { FileName = appPath64 });
                Environment.Exit(Environment.ExitCode);
            }
#endif
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    SilDev.Log.AllowDebug();
                    bool backupMode = false;
#if x86
                    if (SilDev.Reg.SubKeyExist(@"HKEY_LOCAL_MACHINE\SOFTWARE\Fraps"))
                    {
                        if (string.IsNullOrWhiteSpace(SilDev.Reg.ReadValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Fraps", "Portable App")))
                        {
                            backupMode = true;
                            SilDev.Reg.RenameSubKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Fraps", @"SOFTWARE\SI13N7-BACKUP: Fraps");
                        }
                    }
                    SilDev.Reg.WriteValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Fraps", "Install_Dir", Path.GetDirectoryName(appPath));
#else
                    if (SilDev.Reg.SubKeyExist(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Fraps"))
                    {
                        if (string.IsNullOrWhiteSpace(SilDev.Reg.ReadValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Fraps", "Portable App")))
                        {
                            backupMode = true;
                            SilDev.Reg.RenameSubKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Fraps", @"SOFTWARE\Wow6432Node\SI13N7-BACKUP: Fraps");
                        }
                    }
                    SilDev.Reg.WriteValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Fraps", "Install_Dir", Path.GetDirectoryName(appPath));
#endif
                    string regHelperPath = SilDev.Run.EnvironmentVariableFilter(string.Format(@"%TEMP%\{0}.reg", SilDev.Crypt.MD5.Encrypt(appPath)));
                    if (File.Exists(regHelperPath))
                        File.Delete(regHelperPath);
                    File.WriteAllText(regHelperPath, SilDev.Crypt.Base64.Decrypt("V2luZG93cyBSZWdpc3RyeSBFZGl0b3IgVmVyc2lvbiA1LjAwDQoNCltIS0VZX0xPQ0FMX01BQ0hJTkVcU09GVFdBUkVcTWljcm9zb2Z0XFdpbmRvd3MgTlRcQ3VycmVudFZlcnNpb25cRHJpdmVyczMyXQ0KIlZJREMuRlBTMSI9ImZyYXBzdjY0LmRsbCINCg0KW0hLRVlfTE9DQUxfTUFDSElORVxTT0ZUV0FSRVxNaWNyb3NvZnRcV2luZG93cyBOVFxDdXJyZW50VmVyc2lvblxkcml2ZXJzLmRlc2NdDQoiZnJhcHN2NjQuZGxsIj0iRnJhcHMgVmlkZW8gRGVjb21wcmVzc29yIg0KDQpbSEtFWV9MT0NBTF9NQUNISU5FXFNZU1RFTVxDdXJyZW50Q29udHJvbFNldFxDb250cm9sXE1lZGlhUmVzb3VyY2VzXGljbVxWSURDLkZQUzFdDQoiRGVzY3JpcHRpb24iPSJGcmFwcyBWaWRlbyBEZWNvbXByZXNzb3IiDQoiRHJpdmVyIj0iZnJhcHN2aWQuZGxsIg0K"));
                    if (File.Exists(regHelperPath))
                    {
                        SilDev.Reg.ImportFile(regHelperPath);
                        File.Delete(regHelperPath);
                    }
                    string dataPath = Path.Combine(Application.StartupPath, "Data");
                    if (!Directory.Exists(dataPath))
                        Directory.CreateDirectory(dataPath);
                    if (SilDev.Reg.SubKeyExist(@"HKEY_CURRENT_USER\SOFTWARE\Fraps3"))
                    {
                        if (string.IsNullOrWhiteSpace(SilDev.Reg.ReadValue(@"HKEY_CURRENT_USER\SOFTWARE\Fraps3", "Portable App")))
                        {
                            backupMode = true;
                            SilDev.Reg.RenameSubKey(@"HKEY_CURRENT_USER\SOFTWARE\Fraps3", @"SOFTWARE\SI13N7-BACKUP: Fraps");
                        }
                    }
                    string settingsPath = Path.Combine(dataPath, "settings.reg");
                    if (File.Exists(settingsPath))
                        SilDev.Reg.ImportFile(settingsPath);
                    SilDev.Reg.WriteValue(@"HKEY_CURRENT_USER\SOFTWARE\Fraps3", "Portable App", true);
                    string outputPath = Path.Combine(dataPath, "Output");
                    if (!Directory.Exists(outputPath))
                        Directory.CreateDirectory(outputPath);
                    SilDev.Reg.WriteValue(@"HKEY_CURRENT_USER\SOFTWARE\Fraps3", "Directory", Path.GetDirectoryName(appPath));
                    string tmp = SilDev.Reg.ReadValue(@"HKEY_CURRENT_USER\SOFTWARE\Fraps3", "Benchmark Directory");
                    if (string.IsNullOrWhiteSpace(tmp) || tmp.Contains("portable"))
                        SilDev.Reg.WriteValue(@"HKEY_CURRENT_USER\SOFTWARE\Fraps3", "Benchmark Directory", outputPath);
                    tmp = SilDev.Reg.ReadValue(@"HKEY_CURRENT_USER\SOFTWARE\Fraps3", "Movie Directory");
                    if (string.IsNullOrWhiteSpace(tmp) || tmp.Contains("portable"))
                        SilDev.Reg.WriteValue(@"HKEY_CURRENT_USER\SOFTWARE\Fraps3", "Movie Directory", outputPath);
                    tmp = SilDev.Reg.ReadValue(@"HKEY_CURRENT_USER\SOFTWARE\Fraps3", "Screenshot Directory");
                    if (string.IsNullOrWhiteSpace(tmp) || tmp.Contains("portable"))
                        SilDev.Reg.WriteValue(@"HKEY_CURRENT_USER\SOFTWARE\Fraps3", "Screenshot Directory", outputPath);

                    string driverPath = Path.Combine(Application.StartupPath, @"App\Driver");
#if x86
                    SilDev.Run.Cmd(string.Format("COPY /Y \"{0}\\SysWOW64\\{1}\" \"%WinDir%\\System32\\{1}\"", driverPath, "frapsvid.dll"), true);
#else
                    SilDev.Run.Cmd(string.Format("COPY /Y \"{0}\\{1}\" \"%WinDir%\\{1}\" && COPY /Y \"{0}\\{2}\" \"%WinDir%\\{2}\"", driverPath, @"System32\frapsv64.dll", @"SysWOW64\frapsvid.dll"), true);
#endif
#if x86
                    if (!SilDev.Reg.SubKeyExist(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Fraps"))
#else
                    if (!SilDev.Reg.SubKeyExist(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Fraps"))
#endif
                    {
                        if (File.Exists(regHelperPath))
                            File.Delete(regHelperPath);
                        File.WriteAllText(regHelperPath, SilDev.Crypt.Base64.Decrypt("V2luZG93cyBSZWdpc3RyeSBFZGl0b3IgVmVyc2lvbiA1LjAwDQoNCltIS0VZX0xPQ0FMX01BQ0hJTkVcU09GVFdBUkVcV293NjQzMk5vZGVcTWljcm9zb2Z0XFdpbmRvd3NcQ3VycmVudFZlcnNpb25cVW5pbnN0YWxsXEZyYXBzXQ0KIkRpc3BsYXlOYW1lIj0iRnJhcHMgKHJlbW92ZSBvbmx5KSINCiJOc2lzSW5zdCI9aGV4OmQ5LDEwLGNhLDMyLDNlLDFiLDA4LDQ0LGNhLDc5LDYxLGEzLGYwLDdhLDM5LDE4LGJhLDZmLDI3LGUwDQoiTnNpc1NNIj0iRnJhcHMiDQoiVW5pbnN0YWxsU3RyaW5nIj0iXCJDOlxcRnJhcHNcXHVuaW5zdGFsbC5leGVcIiINCg0K"));
                        if (File.Exists(regHelperPath))
                        {
                            SilDev.Reg.ImportFile(regHelperPath);
                            File.Delete(regHelperPath);
                        }
#if x86
                        SilDev.Reg.WriteValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Fraps", "UninstallString", Path.Combine(Path.GetDirectoryName(appPath), "uninstall.exe"));
#else
                        SilDev.Reg.WriteValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Fraps", "UninstallString", Path.Combine(Path.GetDirectoryName(appPath), "uninstall.exe"));
#endif
                    }
                    SilDev.Run.App(new ProcessStartInfo() { FileName = appPath }, 0);
                    appProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
                    while (appProcess.Length > 0)
                    {
                        foreach (Process p in appProcess)
                            p.WaitForExit();
                        appProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
                    }
                    SilDev.Reg.ExportFile(@"HKEY_CURRENT_USER\SOFTWARE\Fraps3", settingsPath);
                    SilDev.Reg.RemoveExistSubKey(@"HKEY_CURRENT_USER\SOFTWARE\Fraps3");
#if x86
                    SilDev.Reg.RemoveExistSubKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Fraps");
#else
                    SilDev.Reg.RemoveExistSubKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Fraps");
#endif
                    if (backupMode)
                    {
                        SilDev.Reg.RenameSubKey(@"HKEY_CURRENT_USER\SOFTWARE\SI13N7-BACKUP: Fraps3", @"SOFTWARE\Fraps3");
#if x86
                        SilDev.Reg.RenameSubKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\SI13N7-BACKUP: Fraps", @"SOFTWARE\Fraps");
#else
                        SilDev.Reg.RenameSubKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\SI13N7-BACKUP: Fraps", @"SOFTWARE\Fraps");
#endif
                    }
                    else
                    {
                        if (File.Exists(regHelperPath))
                            File.Delete(regHelperPath);
                        File.WriteAllText(regHelperPath, SilDev.Crypt.Base64.Decrypt("V2luZG93cyBSZWdpc3RyeSBFZGl0b3IgVmVyc2lvbiA1LjAwDQoNCltIS0VZX0xPQ0FMX01BQ0hJTkVcU09GVFdBUkVcTWljcm9zb2Z0XFdpbmRvd3MgTlRcQ3VycmVudFZlcnNpb25cRHJpdmVyczMyXQ0KIlZJREMuRlBTMSI9LQ0KDQpbSEtFWV9MT0NBTF9NQUNISU5FXFNPRlRXQVJFXE1pY3Jvc29mdFxXaW5kb3dzIE5UXEN1cnJlbnRWZXJzaW9uXGRyaXZlcnMuZGVzY10NCiJmcmFwc3Y2NC5kbGwiPS0NCg0KWy1IS0VZX0xPQ0FMX01BQ0hJTkVcU1lTVEVNXEN1cnJlbnRDb250cm9sU2V0XENvbnRyb2xcTWVkaWFSZXNvdXJjZXNcaWNtXFZJREMuRlBTMV0NCg0K"));
                        if (File.Exists(regHelperPath))
                        {
                            SilDev.Reg.ImportFile(regHelperPath);
                            File.Delete(regHelperPath);
                        }
#if x86
                        File.WriteAllText(regHelperPath, SilDev.Crypt.Base64.Decrypt("V2luZG93cyBSZWdpc3RyeSBFZGl0b3IgVmVyc2lvbiA1LjAwDQoNClstSEtFWV9MT0NBTF9NQUNISU5FXFNPRlRXQVJFXE1pY3Jvc29mdFxXaW5kb3dzXEN1cnJlbnRWZXJzaW9uXFVuaW5zdGFsbFxGcmFwc10NCg=="));
                        if (File.Exists(regHelperPath))
                        {
                            SilDev.Reg.ImportFile(regHelperPath);
                            File.Delete(regHelperPath);
                        }
                        SilDev.Run.App(new ProcessStartInfo()
                        {
                            FileName = @"%WinDir%\System32\cmd.exe",
                            Arguments = string.Format("/C DEL /F /Q \"%WinDir%\\System32\\{1}\"", driverPath, "frapsvid.dll"),
                            WindowStyle = ProcessWindowStyle.Hidden
                        }, 0);
#else
                        File.WriteAllText(regHelperPath, SilDev.Crypt.Base64.Decrypt("V2luZG93cyBSZWdpc3RyeSBFZGl0b3IgVmVyc2lvbiA1LjAwDQoNClstSEtFWV9MT0NBTF9NQUNISU5FXFNPRlRXQVJFXFdvdzY0MzJOb2RlXE1pY3Jvc29mdFxXaW5kb3dzXEN1cnJlbnRWZXJzaW9uXFVuaW5zdGFsbFxGcmFwc10NCg=="));
                        if (File.Exists(regHelperPath))
                        {
                            SilDev.Reg.ImportFile(regHelperPath);
                            File.Delete(regHelperPath);
                        }
                        SilDev.Run.App(new ProcessStartInfo()
                        {
                            FileName = @"%WinDir%\System32\cmd.exe",
                            Arguments = string.Format("/C DEL /F /Q \"%WinDir%\\{1}\" && DEL /F /Q \"%WinDir%\\{2}\"", driverPath, @"System32\frapsv64.dll", @"SysWOW64\frapsvid.dll"),
                            WindowStyle = ProcessWindowStyle.Hidden
                        }, 0);
#endif
                    }
                }
            }
        }
    }
}
