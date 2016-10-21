namespace WinRARPortable
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
            var appPath = Environment.Is64BitProcess ? "%CurDir%\\winrar-x64\\WinRAR.exe" : "%CurDir%\\winrar\\WinRAR.exe";
            Ini.File(EnvironmentEx.GetVariableValue("CurDir"), Environment.Is64BitProcess ? "WinRAR64Portable.ini" : "WinRARPortable.ini");
            bool contextMenuEntriesAllowed;
            bool.TryParse(Ini.Read("ContextMenu", "EntriesAllowed"), out contextMenuEntriesAllowed);
            if (!contextMenuEntriesAllowed && Elevation.IsAdministrator && Environment.CommandLine.EndsWith(".exe\" /ClearRegistryAsAdmin", StringComparison.OrdinalIgnoreCase))
            {
                Reg.RemoveExistSubKey(Reg.RegKey.ClassesRoot, "WinRAR");
                Reg.RemoveExistSubKey(Reg.RegKey.ClassesRoot, "WinRAR.REV");
                Reg.RemoveExistSubKey(Reg.RegKey.ClassesRoot, "WinRAR.ZIP");
                Reg.RemoveExistSubKey(Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR");
                Reg.RemoveExistSubKey(Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR.REV");
                Reg.RemoveExistSubKey(Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR.ZIP");
                Reg.RemoveExistSubKey(Reg.RegKey.CurrentUser, "Software\\WinRAR");
                return;
            }
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                if (newInstance)
                {
                    var lastUpdateCheck = Ini.ReadDateTime("History", "LastUpdateCheck");
                    if ((DateTime.Now - lastUpdateCheck).TotalDays >= 1d || !File.Exists(PathEx.Combine(appPath)))
                    {
                        using (var p = ProcessEx.Start(Environment.Is64BitProcess ? "%CurDir%\\winrar-x64\\WinRARUpdater64.exe" : "%CurDir%\\winrar\\WinRARUpdater.exe", "/silent", false, false))
                            if (!p?.HasExited == true)
                                p?.WaitForExit();
                        Ini.Write("History", "LastUpdateCheck", DateTime.Now);
                    }
                    var ini = PathEx.Combine("%CurDir%", Environment.Is64BitProcess ? "winrar-x64\\WinRAR.ini" : "winrar\\WinRAR.ini");
                    if (!File.Exists(ini))
                        File.Create(ini).Close();
                    using (var p = ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false), false, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    var isRunning = true;
                    while (isRunning)
                    {
                        var runningApp = Process.GetProcessesByName("WinRAR");
                        isRunning = runningApp.Length > 0;
                        foreach (var app in runningApp)
                            app.WaitForExit();
                    }
                    var appDataPath = Path.Combine("%AppData%\\WinRAR");
                    if (Directory.Exists(appDataPath) && Directory.GetFiles(appDataPath, "*", SearchOption.AllDirectories).Length == 0)
                        Directory.Delete(appDataPath, true);
                    if (!contextMenuEntriesAllowed &&
                        (Reg.SubKeyExist(Reg.RegKey.ClassesRoot, "WinRAR") ||
                         Reg.SubKeyExist(Reg.RegKey.ClassesRoot, "WinRAR.REV") ||
                         Reg.SubKeyExist(Reg.RegKey.ClassesRoot, "WinRAR.ZIP") ||
                         Reg.SubKeyExist(Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR") ||
                         Reg.SubKeyExist(Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR.REV") ||
                         Reg.SubKeyExist(Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR.ZIP") ||
                         Reg.SubKeyExist(Reg.RegKey.CurrentUser, "Software\\WinRAR")))
                        Elevation.RestartAsAdministrator("/ClearRegistryAsAdmin");
                }
                else
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false));
        }
    }
}
