using SilDev;
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
                    string appPath = PATH.Combine("%CurDir%\\AIDA64\\aida64.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName("aida64").Length > 0)
                        return;
                    LOG.AllowDebug();
                    bool regBackup = false;
                    if (REG.SubKeyExist("HKEY_CURRENT_USER\\Software\\FinalWire"))
                    {
                        if (string.IsNullOrWhiteSpace(REG.ReadValue("HKEY_CURRENT_USER\\Software\\FinalWire", "Portable App")))
                        {
                            REG.RenameSubKey("HKEY_CURRENT_USER\\Software\\FinalWire", "Software\\SI13N7-BACKUP: FinalWire");
                            regBackup = true;
                        }
                    }
                    REG.ImportFile(PATH.Combine("%CurDir%\\settings.reg"));
                    REG.WriteValue("HKEY_CURRENT_USER\\Software\\FinalWire", "Portable App", "True");
                    RUN.App(new ProcessStartInfo() { FileName = appPath }, 0);
                    REG.ExportFile("HKEY_CURRENT_USER\\Software\\FinalWire", PATH.Combine("%CurDir%\\settings.reg"));
                    REG.RemoveExistSubKey(REG.RegKey.CurrentUser, "Software\\FinalWire");
                    if (regBackup)
                        REG.RenameSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: FinalWire", "Software\\FinalWire");
                }
            }
        }
    }
}
