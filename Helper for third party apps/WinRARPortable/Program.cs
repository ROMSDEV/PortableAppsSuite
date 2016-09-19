using SilDev;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WinRARPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            LOG.AllowDebug();
            string appPath = Environment.Is64BitProcess ? "%CurDir%\\winrar-x64\\WinRAR.exe" : "%CurDir%\\winrar\\WinRAR.exe";
            INI.File(PATH.GetEnvironmentVariableValue("CurDir"), Environment.Is64BitProcess ? "WinRAR64Portable.ini" : "WinRARPortable.ini");
            bool ContextMenuEntriesAllowed = false;
            bool.TryParse(INI.Read("ContextMenu", "EntriesAllowed"), out ContextMenuEntriesAllowed);
            if (!ContextMenuEntriesAllowed && ELEVATION.IsAdministrator && Environment.CommandLine.EndsWith(".exe\" /ClearRegistryAsAdmin", StringComparison.OrdinalIgnoreCase))
            {
                REG.RemoveExistSubKey(REG.RegKey.ClassesRoot, "WinRAR");
                REG.RemoveExistSubKey(REG.RegKey.ClassesRoot, "WinRAR.REV");
                REG.RemoveExistSubKey(REG.RegKey.ClassesRoot, "WinRAR.ZIP");
                REG.RemoveExistSubKey(REG.RegKey.CurrentUser, "Software\\Classes\\WinRAR");
                REG.RemoveExistSubKey(REG.RegKey.CurrentUser, "Software\\Classes\\WinRAR.REV");
                REG.RemoveExistSubKey(REG.RegKey.CurrentUser, "Software\\Classes\\WinRAR.ZIP");
                REG.RemoveExistSubKey(REG.RegKey.CurrentUser, "Software\\WinRAR");
                return;
            }
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    DateTime LastUpdateCheck = INI.ReadDateTime("History", "LastUpdateCheck");
                    if ((DateTime.Now - LastUpdateCheck).TotalDays >= 1d || !File.Exists(PATH.Combine(appPath)))
                    {
                        RUN.App(new ProcessStartInfo()
                        {
                            Arguments = "/silent",
                            FileName = Environment.Is64BitProcess ? "%CurDir%\\winrar-x64\\WinRARUpdater64.exe" : "%CurDir%\\winrar\\WinRARUpdater.exe"
                        }, 0);
                        INI.Write("History", "LastUpdateCheck", DateTime.Now);
                    }
                    string ini = PATH.Combine("%CurDir%", Environment.Is64BitProcess ? "winrar-x64\\WinRAR.ini" : "winrar\\WinRAR.ini");
                    if (!File.Exists(ini))
                        File.Create(ini).Close();
                    RUN.App(new ProcessStartInfo()
                    {
                        Arguments = RUN.CommandLine(),
                        FileName = appPath
                    }, 0);
                    bool isRunning = true;
                    while (isRunning)
                    {
                        Process[] runningApp = Process.GetProcessesByName("WinRAR");
                        isRunning = runningApp.Length > 0;
                        foreach (Process app in runningApp)
                            app.WaitForExit();
                    }
                    string appDataPath = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "WinRAR");
                    if (Directory.Exists(appDataPath) && Directory.GetFiles(appDataPath, "*", SearchOption.AllDirectories).Length == 0)
                        Directory.Delete(appDataPath, true);
                    if (!ContextMenuEntriesAllowed && 
                        (REG.SubKeyExist(REG.RegKey.ClassesRoot, "WinRAR") ||
                         REG.SubKeyExist(REG.RegKey.ClassesRoot, "WinRAR.REV") ||
                         REG.SubKeyExist(REG.RegKey.ClassesRoot, "WinRAR.ZIP") ||
                         REG.SubKeyExist(REG.RegKey.CurrentUser, "Software\\Classes\\WinRAR") ||
                         REG.SubKeyExist(REG.RegKey.CurrentUser, "Software\\Classes\\WinRAR.REV") ||
                         REG.SubKeyExist(REG.RegKey.CurrentUser, "Software\\Classes\\WinRAR.ZIP") ||
                         REG.SubKeyExist(REG.RegKey.CurrentUser, "Software\\WinRAR")))
                        ELEVATION.RestartAsAdministrator("/ClearRegistryAsAdmin");
                }
                else
                    RUN.App(new ProcessStartInfo()
                    {
                        Arguments = RUN.CommandLine(),
                        FileName = appPath
                    });
            }
        }
    }
}
