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
            SilDev.Log.AllowDebug();
            string appPath = Environment.Is64BitProcess ? "%CurrentDir%\\winrar-x64\\WinRAR.exe" : "%CurrentDir%\\winrar\\WinRAR.exe";
            SilDev.Ini.File(Application.StartupPath, Environment.Is64BitProcess ? "WinRAR64Portable.ini" : "WinRARPortable.ini");
            bool ContextMenuEntriesAllowed = false;
            bool.TryParse(SilDev.Ini.Read("ContextMenu", "EntriesAllowed"), out ContextMenuEntriesAllowed);
            if (!ContextMenuEntriesAllowed && SilDev.Elevation.IsAdministrator && Environment.CommandLine.EndsWith(".exe\" /ClearRegistryAsAdmin", StringComparison.OrdinalIgnoreCase))
            {
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.ClassesRoot, "WinRAR");
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.ClassesRoot, "WinRAR.REV");
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.ClassesRoot, "WinRAR.ZIP");
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR");
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR.REV");
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR.ZIP");
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\WinRAR");
                return;
            }
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    string CurrentDate = DateTime.Now.ToString("M/d/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                    string LastUpdateCheck = SilDev.Ini.Read("History", "LastUpdateCheck");
                    if (LastUpdateCheck != CurrentDate || !File.Exists(appPath))
                    {
                        SilDev.Run.App(new ProcessStartInfo()
                        {
                            Arguments = "/silent",
                            FileName = Environment.Is64BitProcess ? "%CurrentDir%\\winrar-x64\\WinRARUpdater64.exe" : "%CurrentDir%\\winrar\\WinRARUpdater.exe"
                        }, 0);
                        SilDev.Ini.Write("History", "LastUpdateCheck", CurrentDate);
                    }
                    string ini = Path.Combine(Application.StartupPath, Environment.Is64BitProcess ? "winrar-x64\\WinRAR.ini" : "winrar\\WinRAR.ini");
                    if (!File.Exists(ini))
                        File.Create(ini).Close();
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = SilDev.Run.CommandLine(),
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
                        (SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.ClassesRoot, "WinRAR") ||
                         SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.ClassesRoot, "WinRAR.REV") ||
                         SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.ClassesRoot, "WinRAR.ZIP") ||
                         SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR") ||
                         SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR.REV") ||
                         SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR.ZIP") ||
                         SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.CurrentUser, "Software\\WinRAR")))
                        SilDev.Elevation.RestartAsAdministrator("/ClearRegistryAsAdmin");
                }
                else
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = SilDev.Run.CommandLine(),
                        FileName = appPath
                    });
            }
        }
    }
}
