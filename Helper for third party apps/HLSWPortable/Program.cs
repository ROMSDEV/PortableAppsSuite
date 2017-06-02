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
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
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
                    if (!File.Exists(appBackupPath) && Crypto.EncryptFileToMd5(appPath).EqualsEx("1715a84deb7c6c3f35dc8acd96833ef9"))
                    {
                        File.Copy(appPath, appBackupPath);
                        using (var bw = new BinaryWriter(File.Open(appPath, FileMode.Open)))
                        {
                            bw.BaseStream.Position = 0x439654L;
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

                var cfgPath = PathEx.Combine("%CurDir%\\Data\\Cfg");
                if (!Directory.Exists(cfgPath))
                    Directory.CreateDirectory(cfgPath);

                if (!Reg.EntryExists("HKCU\\Software\\HLSW", "Portable App"))
                    Reg.MoveSubKey("HKCU\\Software\\HLSW", "HKCU\\Software\\SI13N7-BACKUP: HLSW");

                var settingsPath = PathEx.Combine("%CurDir%\\Data\\settings.reg");
                if (File.Exists(settingsPath))
                    Reg.ImportFile(settingsPath);
                Reg.Write("HKCU\\Software\\HLSW", "Portable App", "True");
                Reg.Write("HKCU\\Software\\HLSW\\Management", "AutoLogin", 1);
                Reg.Write("HKCU\\Software\\HLSW\\Management", "LoginOnStartup", 0);
                Reg.Write("HKCU\\Software\\HLSW\\Management", "Offline", 1);
                Reg.Write("HKCU\\Software\\HLSW\\Management", "Offline2", 1);

                using (var p = ProcessEx.Start(appPath, true, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();

                Reg.ExportKeys(settingsPath, "HKCU\\Software\\HLSW");
                Reg.RemoveSubKey("HKCU\\Software\\HLSW");
                Reg.MoveSubKey("HKCU\\Software\\SI13N7-BACKUP: HLSW", "HKCU\\Software\\HLSW");

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
