using System;
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
                    SilDev.Log.AllowDebug();
                    string appPath = Path.Combine(Application.StartupPath, "App\\HLSW\\hlsw.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                        return;
                    string appBackupPath = string.Format("{0}.SI13N7-BACKUP", appPath);
                    string oldAppBackupPath = string.Format("{0}.BACKUP", appPath);
                    if (File.Exists(oldAppBackupPath) && !File.Exists(appBackupPath))
                        File.Move(oldAppBackupPath, appBackupPath);
                    if (!File.Exists(appBackupPath) && SilDev.Crypt.MD5.EncryptFile(appPath) == "1715a84deb7c6c3f35dc8acd96833ef9")
                    {
                        File.Copy(appPath, appBackupPath);
                        using (BinaryWriter bw = new BinaryWriter(File.Open(appPath, FileMode.Open)))
                        {
                            bw.BaseStream.Position = 0x439654;
                            bw.Write(Encoding.ASCII.GetBytes("..\\..\\Data\\Cfg"));
                        }
                    }
                    if (SilDev.Crypt.MD5.EncryptFile(appPath) != "8f4fcb7555b8a5cab46ca99750346321")
                        return;
                    string defaultSettings = SilDev.Crypt.Base64.Decrypt("W1Jvb3RdDQpTZWN0aW9ucz01MzEwYWY0ZjViZWIyMDlmNTFhZjIwNTkwY2UwMGY4OCxkNGMxZGNkNTZiOWMwZTdjNzFmYjI4MmJkN2M1YWVkMyxhYmQ0NzE3NzExZTZkMjJmZTNjMDhlYmYyMGM1M2I4YywNCls1MzEwYWY0ZjViZWIyMDlmNTFhZjIwNTkwY2UwMGY4OF0NCjUzMTBhZjRmNWJlYjIwOWY1MWFmMjA1OTBjZTAwZjg4X1Jvb3RLZXk9SEtFWV9DVVJSRU5UX1VTRVINCjUzMTBhZjRmNWJlYjIwOWY1MWFmMjA1OTBjZTAwZjg4X1N1YktleT1Tb2Z0d2FyZVxITFNXXGhsc3dcUHJvZlVJU1xQcm9maWxlc1xobHN3XFBhaW50TWFuYWdlcg0KNTMxMGFmNGY1YmViMjA5ZjUxYWYyMDU5MGNlMDBmODhfVmFsdWVzPWRhdGFfc2l6ZSxkYXRhX2ludGVncml0eSxkYXRhX2dlbmVyYXRvcixsaWJyYXJ5X3ZlcnNpb24sZGF0YV9saW5lX3NpemUsZGF0YV9ibG9ja19zaXplLGRhdGFfZm10X3JldiwNCmRhdGFfc2l6ZT1WYWx1ZUtpbmRfRFdvcmQ6OjYwDQpkYXRhX2ludGVncml0eT1WYWx1ZUtpbmRfU3RyaW5nOjowRiA2QiA5MCBBNSAwNSA1NyAzMiBFRCAwQSA2NSAzRCBENSAwMyBEQyBDMiA5Mw0KZGF0YV9nZW5lcmF0b3I9VmFsdWVLaW5kX1N0cmluZzo6UHJvZi1VSVMgcmVnaXN0cnkgYXJjaGl2ZXIgcmV2LiAwDQpsaWJyYXJ5X3ZlcnNpb249VmFsdWVLaW5kX1N0cmluZzo6UHJvZi1VSVMgdi4gMi44LjkuMA0KZGF0YV9saW5lX3NpemU9VmFsdWVLaW5kX0RXb3JkOjoxNg0KZGF0YV9ibG9ja19zaXplPVZhbHVlS2luZF9EV29yZDo6MTI4DQpkYXRhX2ZtdF9yZXY9VmFsdWVLaW5kX0RXb3JkOjowDQpbZDRjMWRjZDU2YjljMGU3YzcxZmIyODJiZDdjNWFlZDNdDQpkNGMxZGNkNTZiOWMwZTdjNzFmYjI4MmJkN2M1YWVkM19Sb290S2V5PUhLRVlfQ1VSUkVOVF9VU0VSDQpkNGMxZGNkNTZiOWMwZTdjNzFmYjI4MmJkN2M1YWVkM19TdWJLZXk9U29mdHdhcmVcSExTV1xNYW5hZ2VtZW50DQpkNGMxZGNkNTZiOWMwZTdjNzFmYjI4MmJkN2M1YWVkM19WYWx1ZXM9QXV0b0xvZ2luLE9mZmxpbmUyLExvZ2luT25TdGFydHVwLA0KQXV0b0xvZ2luPVZhbHVlS2luZF9EV29yZDo6MA0KT2ZmbGluZTI9VmFsdWVLaW5kX0RXb3JkOjoxDQpMb2dpbk9uU3RhcnR1cD1WYWx1ZUtpbmRfRFdvcmQ6OjANClthYmQ0NzE3NzExZTZkMjJmZTNjMDhlYmYyMGM1M2I4Y10NCmFiZDQ3MTc3MTFlNmQyMmZlM2MwOGViZjIwYzUzYjhjX1Jvb3RLZXk9SEtFWV9DVVJSRU5UX1VTRVINCmFiZDQ3MTc3MTFlNmQyMmZlM2MwOGViZjIwYzUzYjhjX1N1YktleT1Tb2Z0d2FyZVxITFNXXFNldHRpbmdzDQphYmQ0NzE3NzExZTZkMjJmZTNjMDhlYmYyMGM1M2I4Y19WYWx1ZXM9U2tpbiwNClNraW49VmFsdWVLaW5kX0RXb3JkOjo3DQo=");
                    string oldCfgPath = Path.Combine(Application.StartupPath, "Data\\Roaming");
                    string cfgPath = Path.Combine(Application.StartupPath, "Data\\Cfg");
                    if (Directory.Exists(oldCfgPath))
                    {
                        if (Directory.Exists(cfgPath))
                            Directory.Delete(cfgPath, true);
                        if (Directory.Exists(string.Format("{0}.BACKUP", oldCfgPath)))
                            Directory.Delete(string.Format("{0}.BACKUP", oldCfgPath), true);
                        Directory.Move(oldCfgPath, cfgPath);
                    }
                    if (!Directory.Exists(cfgPath))
                        Directory.CreateDirectory(cfgPath);
                    bool regBackup = false;
                    if (SilDev.Reg.SubKeyExist("HKEY_CURRENT_USER\\Software\\HLSW"))
                    {
                        if (string.IsNullOrWhiteSpace(SilDev.Reg.ReadValue("HKEY_CURRENT_USER\\Software\\HLSW", "Portable App")))
                        {
                            SilDev.Reg.RenameSubKey("HKEY_CURRENT_USER\\Software\\HLSW", "Software\\SI13N7-BACKUP: HLSW");
                            regBackup = true;
                        }
                    }
                    string settingsPath = Path.Combine(Application.StartupPath, "Data\\settings.reg");
                    string oldSettingsPath = Path.Combine(Application.StartupPath, "Data\\settings.ini");
                    if (File.Exists(oldSettingsPath) && !File.Exists(settingsPath))
                    {
                        SilDev.Reg.ImportFile(oldSettingsPath);
                        if (File.Exists(oldSettingsPath))
                            File.Delete(oldSettingsPath);
                    }
                    if (File.Exists(settingsPath))
                        SilDev.Reg.ImportFile(settingsPath);
                    SilDev.Reg.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW", "Portable App", "True");
                    SilDev.Reg.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW\\Management", "AutoLogin", 1);
                    SilDev.Reg.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW\\Management", "LoginOnStartup", 0);
                    SilDev.Reg.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW\\Management", "Offline", 1);
                    SilDev.Reg.WriteValue("HKEY_CURRENT_USER\\Software\\HLSW\\Management", "Offline2", 1);
                    SilDev.Run.App(appPath, 0);
                    if (File.Exists(oldSettingsPath))
                        File.Delete(oldSettingsPath);
                    SilDev.Reg.ExportFile("HKEY_CURRENT_USER\\Software\\HLSW", settingsPath);
                    SilDev.Reg.RemoveExistSubKey("HKEY_CURRENT_USER\\Software\\HLSW");
                    if (regBackup)
                        SilDev.Reg.RenameSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: HLSW", "Software\\HLSW");
                    if (SilDev.Elevation.IsAdministrator)
                    {
                        string[] regFileCon = new string[]
                        {
                            "Windows Registry Editor Version 5.00",
                            "",
                            "[-HKEY_CLASSES_ROOT\\.sl00]",
                            "[-HKEY_CLASSES_ROOT\\.sl01]",
                            "[-HKEY_CLASSES_ROOT\\.sl02]",
                            "[-HKEY_CLASSES_ROOT\\.sl03]",
                            "[-HKEY_CLASSES_ROOT\\.sl04]",
                            "[-HKEY_CLASSES_ROOT\\.sl05]",
                            "[-HKEY_CLASSES_ROOT\\.sl06]",
                            "[-HKEY_CLASSES_ROOT\\.sl07]",
                            "[-HKEY_CLASSES_ROOT\\.sl08]",
                            "[-HKEY_CLASSES_ROOT\\.sl09]",
                            "[-HKEY_CLASSES_ROOT\\.sl10]",
                            "[-HKEY_CLASSES_ROOT\\.sl11]",
                            "[-HKEY_CLASSES_ROOT\\.sl12]",
                            "[-HKEY_CLASSES_ROOT\\.sl13]",
                            "[-HKEY_CLASSES_ROOT\\.sl14]",
                            "[-HKEY_CLASSES_ROOT\\.sl15]",
                            "[-HKEY_CLASSES_ROOT\\.sl16]",
                            "[-HKEY_CLASSES_ROOT\\.sl17]",
                            "[-HKEY_CLASSES_ROOT\\.sl18]",
                            "[-HKEY_CLASSES_ROOT\\.sl19]",
                            "[-HKEY_CLASSES_ROOT\\.sl20]",
                            "[-HKEY_CLASSES_ROOT\\.sl21]",
                            "[-HKEY_CLASSES_ROOT\\.sl22]",
                            "[-HKEY_CLASSES_ROOT\\.sl23]",
                            "[-HKEY_CLASSES_ROOT\\.sl24]",
                            "[-HKEY_CLASSES_ROOT\\.sl25]",
                            "[-HKEY_CLASSES_ROOT\\.sl26]",
                            "[-HKEY_CLASSES_ROOT\\.sl27]",
                            "[-HKEY_CLASSES_ROOT\\.sl28]",
                            "[-HKEY_CLASSES_ROOT\\.sl29]",
                            "[-HKEY_CLASSES_ROOT\\.sl30]",
                            "[-HKEY_CLASSES_ROOT\\.sl31]",
                            "[-HKEY_CLASSES_ROOT\\.sl32]",
                            "[-HKEY_CLASSES_ROOT\\.sl33]",
                            "[-HKEY_CLASSES_ROOT\\.sslf]",
                            "[-HKEY_CLASSES_ROOT\\hlsw]",
                            "[-HKEY_CLASSES_ROOT\\HLSW Server List]",
                            "[-HKEY_CURRENT_USER\\Software\\HLSW]"
                        };
                        SilDev.Reg.ImportFile(regFileCon, true);
                    }
                }
            }
        }
    }
}
