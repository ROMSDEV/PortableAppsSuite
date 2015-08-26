using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AIDA64Portable
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
                    string appPath = Path.Combine(Application.StartupPath, "AIDA64\\aida64.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName("aida64").Length > 0)
                        return;
                    SilDev.Log.AllowDebug();
                    bool regBackup = false;
                    if (SilDev.Reg.SubKeyExist("HKEY_CURRENT_USER\\Software\\FinalWire"))
                    {
                        if (string.IsNullOrWhiteSpace(SilDev.Reg.ReadValue("HKEY_CURRENT_USER\\Software\\FinalWire", "Portable App")))
                        {
                            SilDev.Reg.RenameSubKey("HKEY_CURRENT_USER\\Software\\FinalWire", "Software\\SI13N7-BACKUP: FinalWire");
                            regBackup = true;
                        }
                    }
                    SilDev.Reg.ImportFile(Path.Combine(Application.StartupPath, "settings.reg"));
                    SilDev.Reg.WriteValue("HKEY_CURRENT_USER\\Software\\FinalWire", "Portable App", "True");
                    SilDev.Run.App(appPath, 0);
                    SilDev.Reg.ExportFile("HKEY_CURRENT_USER\\Software\\FinalWire", Path.Combine(Application.StartupPath, "settings.reg"));
                    SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\FinalWire");
                    if (regBackup)
                        SilDev.Reg.RenameSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: FinalWire", "Software\\FinalWire");
                }
            }
        }
    }
}
