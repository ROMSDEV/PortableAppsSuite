using SilDev;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace HLSWPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    LOG.AllowDebug();

                    string appPath = PATH.Combine("%CurDir%\\App\\HLSW\\hlsw.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                        return;

                    string appBackupPath = string.Format("{0}.SI13N7-BACKUP", appPath);
                    string oldAppBackupPath = string.Format("{0}.BACKUP", appPath);
                    if (File.Exists(oldAppBackupPath) && !File.Exists(appBackupPath))
                        File.Move(oldAppBackupPath, appBackupPath);

                    try
                    {
                        if (!File.Exists(appBackupPath) && CRYPT.MD5.EncryptFile(appPath) == "1715a84deb7c6c3f35dc8acd96833ef9")
                        {
                            File.Copy(appPath, appBackupPath);
                            using (BinaryWriter bw = new BinaryWriter(File.Open(appPath, FileMode.Open)))
                            {
                                bw.BaseStream.Position = 0x439654;
                                bw.Write(Encoding.ASCII.GetBytes("..\\..\\Data\\Cfg"));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }

                    if (CRYPT.MD5.EncryptFile(appPath) != "8f4fcb7555b8a5cab46ca99750346321")
                        return;

                    string defaultSettings = new CRYPT.Base64().DecodeString("W1Jvb3RdDQpTZWN0aW9ucz01MzEwYWY0ZjViZWIyMDlmNTFhZjIwNTkwY2UwMGY4OCxkNGMxZGNkNTZiOWMwZTdjNzFmYjI4MmJkN2M1YWVkMyxhYmQ0NzE3NzExZTZkMjJmZTNjMDhlYmYyMGM1M2I4YywNCls1MzEwYWY0ZjViZWIyMDlmNTFhZjIwNTkwY2UwMGY4OF0NCjUzMTBhZjRmNWJlYjIwOWY1MWFmMjA1OTBjZTAwZjg4X1Jvb3RLZXk9SEtFWV9DVVJSRU5UX1VTRVINCjUzMTBhZjRmNWJlYjIwOWY1MWFmMjA1OTBjZTAwZjg4X1N1YktleT1Tb2Z0d2FyZVxITFNXXGhsc3dcUHJvZlVJU1xQcm9maWxlc1xobHN3XFBhaW50TWFuYWdlcg0KNTMxMGFmNGY1YmViMjA5ZjUxYWYyMDU5MGNlMDBmODhfVmFsdWVzPWRhdGFfc2l6ZSxkYXRhX2ludGVncml0eSxkYXRhX2dlbmVyYXRvcixsaWJyYXJ5X3ZlcnNpb24sZGF0YV9saW5lX3NpemUsZGF0YV9ibG9ja19zaXplLGRhdGFfZm10X3JldiwNCmRhdGFfc2l6ZT1WYWx1ZUtpbmRfRFdvcmQ6OjYwDQpkYXRhX2ludGVncml0eT1WYWx1ZUtpbmRfU3RyaW5nOjowRiA2QiA5MCBBNSAwNSA1NyAzMiBFRCAwQSA2NSAzRCBENSAwMyBEQyBDMiA5Mw0KZGF0YV9nZW5lcmF0b3I9VmFsdWVLaW5kX1N0cmluZzo6UHJvZi1VSVMgcmVnaXN0cnkgYXJjaGl2ZXIgcmV2LiAwDQpsaWJyYXJ5X3ZlcnNpb249VmFsdWVLaW5kX1N0cmluZzo6UHJvZi1VSVMgdi4gMi44LjkuMA0KZGF0YV9saW5lX3NpemU9VmFsdWVLaW5kX0RXb3JkOjoxNg0KZGF0YV9ibG9ja19zaXplPVZhbHVlS2luZF9EV29yZDo6MTI4DQpkYXRhX2ZtdF9yZXY9VmFsdWVLaW5kX0RXb3JkOjowDQpbZDRjMWRjZDU2YjljMGU3YzcxZmIyODJiZDdjNWFlZDNdDQpkNGMxZGNkNTZiOWMwZTdjNzFmYjI4MmJkN2M1YWVkM19Sb290S2V5PUhLRVlfQ1VSUkVOVF9VU0VSDQpkNGMxZGNkNTZiOWMwZTdjNzFmYjI4MmJkN2M1YWVkM19TdWJLZXk9U29mdHdhcmVcSExTV1xNYW5hZ2VtZW50DQpkNGMxZGNkNTZiOWMwZTdjNzFmYjI4MmJkN2M1YWVkM19WYWx1ZXM9QXV0b0xvZ2luLE9mZmxpbmUyLExvZ2luT25TdGFydHVwLA0KQXV0b0xvZ2luPVZhbHVlS2luZF9EV29yZDo6MA0KT2ZmbGluZTI9VmFsdWVLaW5kX0RXb3JkOjoxDQpMb2dpbk9uU3RhcnR1cD1WYWx1ZUtpbmRfRFdvcmQ6OjANClthYmQ0NzE3NzExZTZkMjJmZTNjMDhlYmYyMGM1M2I4Y10NCmFiZDQ3MTc3MTFlNmQyMmZlM2MwOGViZjIwYzUzYjhjX1Jvb3RLZXk9SEtFWV9DVVJSRU5UX1VTRVINCmFiZDQ3MTc3MTFlNmQyMmZlM2MwOGViZjIwYzUzYjhjX1N1YktleT1Tb2Z0d2FyZVxITFNXXFNldHRpbmdzDQphYmQ0NzE3NzExZTZkMjJmZTNjMDhlYmYyMGM1M2I4Y19WYWx1ZXM9U2tpbiwNClNraW49VmFsdWVLaW5kX0RXb3JkOjo3DQo=");
                    string oldCfgPath = PATH.Combine("%CurDir%\\Data\\Roaming");
                    string cfgPath = PATH.Combine("%CurDir%\\Data\\Cfg");
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

                    if (!REG.ValueExist("HKEY_CURRENT_USER\\Software\\HLSW", "Portable App"))
                        REG.RenameSubKey("HKEY_CURRENT_USER\\Software\\HLSW", "Software\\SI13N7-BACKUP: HLSW");

                    string settingsPath = PATH.Combine("%CurDir%\\Data\\settings.reg");
                    string oldSettingsPath = PATH.Combine("%CurDir%\\Data\\settings.ini");
                    if (File.Exists(oldSettingsPath) && !File.Exists(settingsPath))
                    {
                        REG.ImportFile(oldSettingsPath);
                        if (File.Exists(oldSettingsPath))
                            File.Delete(oldSettingsPath);
                    }

                    if (File.Exists(settingsPath))
                        REG.ImportFile(settingsPath);
                    REG.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW", "Portable App", "True");
                    REG.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW\\Management", "AutoLogin", 1);
                    REG.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW\\Management", "LoginOnStartup", 0);
                    REG.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW\\Management", "Offline", 1);
                    REG.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW\\Management", "Offline2", 1);

                    RUN.App(new ProcessStartInfo() { FileName = appPath }, 0);

                    if (File.Exists(oldSettingsPath))
                        File.Delete(oldSettingsPath);

                    REG.ExportFile("HKEY_CURRENT_USER\\Software\\HLSW", settingsPath);
                    REG.RemoveExistSubKey("HKEY_CURRENT_USER\\Software\\HLSW");
                    REG.RenameSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: HLSW", "Software\\HLSW");

                    if (ELEVATION.IsAdministrator)
                    {
                        List<string> regFileCon = new List<string>();
                        regFileCon.Add("Windows Registry Editor Version 5.00");
                        regFileCon.Add("");
                        for (int i = 0; i < 34; i++)
                            regFileCon.Add($"[-HKEY_CLASSES_ROOT\\.sl{(i < 10 ? $"0{i}" : i.ToString())}]");
                        regFileCon.Add("[-HKEY_CLASSES_ROOT\\.sslf]");
                        regFileCon.Add("[-HKEY_CLASSES_ROOT\\hlsw]");
                        regFileCon.Add("[-HKEY_CLASSES_ROOT\\HLSW Server List]");
                        regFileCon.Add("[-HKEY_CURRENT_USER\\Software\\HLSW]");
                        REG.ImportFile(regFileCon.ToArray(), true);
                    }
                }
            }
        }
    }
}
