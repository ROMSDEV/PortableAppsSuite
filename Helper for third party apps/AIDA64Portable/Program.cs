namespace AIDA64Portable
{
    using System;
    using System.Diagnostics;
    using System.IO;
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
                if (newInstance)
                {
                    var appPath = PathEx.Combine("%CurDir%\\AIDA64\\aida64.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName("aida64").Length > 0)
                        return;
                    var regBackup = false;
                    const string regKey = "HKEY_CURRENT_USER\\Software\\FinalWire";
                    if (Reg.SubKeyExists(regKey))
                        if (string.IsNullOrWhiteSpace(Reg.ReadString(regKey, "Portable App")))
                        {
                            Reg.MoveSubKey(regKey, "HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: FinalWire");
                            regBackup = true;
                        }
                    Reg.ImportFile(PathEx.Combine("%CurDir%\\settings.reg"));
                    Reg.Write(regKey, "Portable App", "True");
                    using (var p = ProcessEx.Start(appPath, true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    Reg.ExportKeys(PathEx.Combine("%CurDir%\\settings.reg"), regKey);
                    Reg.RemoveSubKey("HKEY_CURRENT_USER\\Software\\FinalWire");
                    if (regBackup)
                        Reg.MoveSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: FinalWire", "HKEY_CURRENT_USER\\Software\\FinalWire");
                }
        }
    }
}
