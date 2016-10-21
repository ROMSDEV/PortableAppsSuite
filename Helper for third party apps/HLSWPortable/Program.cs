namespace HLSWPortable
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (!newInstance)
                    return;
                var appPath = PathEx.Combine("%CurDir%\\App\\HLSW\\hlsw.exe");
                if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                    return;

                string appBackupPath = $"{appPath}.SI13N7-BACKUP";
                string oldAppBackupPath = $"{appPath}.BACKUP";
                if (File.Exists(oldAppBackupPath) && !File.Exists(appBackupPath))
                    File.Move(oldAppBackupPath, appBackupPath);

                try
                {
                    if (!File.Exists(appBackupPath) && Crypto.EncryptFileToMd5(appPath) == "1715a84deb7c6c3f35dc8acd96833ef9")
                    {
                        File.Copy(appPath, appBackupPath);
                        using (var bw = new BinaryWriter(File.Open(appPath, FileMode.Open)))
                        {
                            bw.BaseStream.Position = 0x439654;
                            bw.Write(Encoding.ASCII.GetBytes("..\\..\\Data\\Cfg"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                if (Crypto.EncryptFileToMd5(appPath) != "8f4fcb7555b8a5cab46ca99750346321")
                    return;

                var oldCfgPath = PathEx.Combine("%CurDir%\\Data\\Roaming");
                var cfgPath = PathEx.Combine("%CurDir%\\Data\\Cfg");
                if (Directory.Exists(oldCfgPath))
                {
                    if (Directory.Exists(cfgPath))
                        Directory.Delete(cfgPath, true);
                    if (Directory.Exists($"{oldCfgPath}.BACKUP"))
                        Directory.Delete($"{oldCfgPath}.BACKUP", true);
                    Directory.Move(oldCfgPath, cfgPath);
                }
                if (!Directory.Exists(cfgPath))
                    Directory.CreateDirectory(cfgPath);

                if (!Reg.ValueExist("HKEY_CURRENT_USER\\Software\\HLSW", "Portable App"))
                    Reg.MoveSubKey("HKEY_CURRENT_USER\\Software\\HLSW", "Software\\SI13N7-BACKUP: HLSW");

                var settingsPath = PathEx.Combine("%CurDir%\\Data\\settings.reg");
                var oldSettingsPath = PathEx.Combine("%CurDir%\\Data\\settings.ini");
                if (File.Exists(oldSettingsPath) && !File.Exists(settingsPath))
                {
                    Reg.ImportFile(oldSettingsPath);
                    if (File.Exists(oldSettingsPath))
                        File.Delete(oldSettingsPath);
                }

                if (File.Exists(settingsPath))
                    Reg.ImportFile(settingsPath);
                Reg.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW", "Portable App", "True");
                Reg.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW\\Management", "AutoLogin", 1);
                Reg.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW\\Management", "LoginOnStartup", 0);
                Reg.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW\\Management", "Offline", 1);
                Reg.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW\\Management", "Offline2", 1);

                using (var p = ProcessEx.Start(appPath, true, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();

                if (File.Exists(oldSettingsPath))
                    File.Delete(oldSettingsPath);

                Reg.ExportFile("HKEY_CURRENT_USER\\Software\\HLSW", settingsPath);
                Reg.RemoveExistSubKey("HKEY_CURRENT_USER\\Software\\HLSW");
                Reg.MoveSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: HLSW", "Software\\HLSW");

                if (!Elevation.IsAdministrator)
                    return;
                var regFileCon = new List<string> { "Windows Registry Editor Version 5.00", string.Empty };
                for (var i = 0; i < 34; i++)
                    regFileCon.Add($"[-HKEY_CLASSES_ROOT\\.sl{(i < 10 ? $"0{i}" : i.ToString())}]");
                regFileCon.Add("[-HKEY_CLASSES_ROOT\\.sslf]");
                regFileCon.Add("[-HKEY_CLASSES_ROOT\\hlsw]");
                regFileCon.Add("[-HKEY_CLASSES_ROOT\\HLSW Server List]");
                regFileCon.Add("[-HKEY_CURRENT_USER\\Software\\HLSW]");
                Reg.ImportFile(regFileCon.ToArray(), true);
            }
        }
    }
}
