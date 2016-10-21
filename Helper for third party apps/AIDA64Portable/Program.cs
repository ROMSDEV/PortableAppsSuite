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
                    if (Reg.SubKeyExist(regKey))
                        if (string.IsNullOrWhiteSpace(Reg.ReadStringValue(regKey, "Portable App")))
                        {
                            Reg.MoveSubKey(regKey, "Software\\SI13N7-BACKUP: FinalWire");
                            regBackup = true;
                        }
                    Reg.ImportFile(PathEx.Combine("%CurDir%\\settings.reg"));
                    Reg.WriteValue(regKey, "Portable App", "True");
                    using (var p = ProcessEx.Start(appPath, true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    Reg.ExportFile(regKey, PathEx.Combine("%CurDir%\\settings.reg"));
                    Reg.RemoveExistSubKey(Reg.RegKey.CurrentUser, "Software\\FinalWire");
                    if (regBackup)
                        Reg.MoveSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: FinalWire", "Software\\FinalWire");
                }
        }
    }
}
