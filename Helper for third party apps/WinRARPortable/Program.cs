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

#if x86
            var curIniPath = PathEx.Combine(PathEx.LocalDir, "WinRARPortable.ini");

            var appPath = PathEx.Combine(PathEx.LocalDir, "winrar\\WinRAR.exe");
            var appIniPath = PathEx.Combine(PathEx.LocalDir, "winrar\\WinRAR.ini");
            var appUpdPath = PathEx.Combine(PathEx.LocalDir, "winrar\\WinRARUpdater.exe");
#else
            var curIniPath = PathEx.Combine(PathEx.LocalDir, "WinRAR64Portable.ini");

            var appPath = PathEx.Combine(PathEx.LocalDir, "winrar-x64\\WinRAR.exe");
            var appIniPath = PathEx.Combine(PathEx.LocalDir, "winrar-x64\\WinRAR.ini");
            var appUpdPath = PathEx.Combine(PathEx.LocalDir, "winrar-x64\\WinRARUpdater64.exe");
#endif

            if (!File.Exists(curIniPath))
            {
                File.Create(curIniPath).Close();
                Ini.Write("ContextMenu", "EntriesAllowed", false);
                Ini.Write("Associations", "FileTypes", "001,7z,ace,arj,bz2,bzip2,cab,gz,gzip,iso,lha,lzh,lzma,rar,tar,taz,tbz,tbz2,tgz,tpz,txz,xy,z,zip");
            }
            Ini.File(curIniPath);

            var contextMenuEntriesAllowed = Ini.ReadBoolean("ContextMenu", "EntriesAllowed");
            if (!contextMenuEntriesAllowed && Elevation.IsAdministrator && Environment.CommandLine.EndsWithEx(".exe\" /ClearRegistryAsAdmin"))
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
                    if ((DateTime.Now - lastUpdateCheck).TotalDays >= 1d || !File.Exists(appPath))
                    {
                        using (var p = ProcessEx.Start(appUpdPath, "/silent", false, false))
                            if (!p?.HasExited == true)
                                p?.WaitForExit();
                        Ini.Write("History", "LastUpdateCheck", DateTime.Now);
                    }

                    if (!File.Exists(appIniPath))
                        File.Create(appIniPath).Close();

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
