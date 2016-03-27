using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace CheatEnginePortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                string appPath = Path.Combine(Application.StartupPath, "App\\CheatEngine");
                if (!File.Exists(Path.Combine(appPath, "Cheat Engine.exe")))
                    return;
                if (newInstance)
                {
                    string dataTables = Path.Combine(Application.StartupPath, "Data\\My Cheat Tables");
                    if (!Directory.Exists(dataTables))
                        Directory.CreateDirectory(dataTables);
                    string dataSettings = Path.Combine(Application.StartupPath, "Data\\settings.reg");
                    if (!File.Exists(dataSettings))
                    {
                        string dataDir = Path.Combine(Application.StartupPath, "Data");
                        if (!Directory.Exists(dataDir))
                            Directory.CreateDirectory(dataDir);
                        string[] defaultSettings = new string[]
                        {
                            "Windows Registry Editor Version 5.00",
                            "",
                            "[HKEY_CURRENT_USER\\Software\\Cheat Engine]",
                            "\"Portable App\"=\"True\"",
                            "\"Initial tables dir\"=\"\""
                        };
                        File.WriteAllLines(dataSettings, defaultSettings);
                    }
                    if (File.Exists(dataSettings))
                        SilDev.Ini.Write("HKEY_CURRENT_USER\\Software\\Cheat Engine", "\"Initial tables dir\"", $"\"{dataTables.Replace("\\", "\\\\")}\"", dataSettings);
                    if (!SilDev.Reg.ValueExist("HKEY_CURRENT_USER\\Software\\Cheat Engine", "Portable App"))
                        SilDev.Reg.RenameSubKey("HKEY_CURRENT_USER\\Software\\Cheat Engine", "Software\\SI13N7-BACKUP: Cheat Engine");
                    SilDev.Reg.ImportFile(dataSettings);
                    SilDev.Run.App(new ProcessStartInfo() { FileName = "%CurrentDir%\\App\\CheatEngine\\Cheat Engine.exe" }, 0);
                    bool isRunning = true;
                    while (isRunning)
                    {
                        Process[] runningApp = Process.GetProcessesByName($"cheatengine-{(Environment.Is64BitOperatingSystem ? "x86_64" : "i386")}");
                        isRunning = runningApp.Length > 0;
                        foreach (Process app in runningApp)
                            app.WaitForExit();
                    }
                    string userTables = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Cheat Tables");
                    if (Directory.Exists(userTables))
                        Directory.Delete(userTables, true);
                    SilDev.Reg.ExportFile("HKEY_CURRENT_USER\\Software\\Cheat Engine", dataSettings);
                    SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\Cheat Engine");
                    SilDev.Reg.RenameSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: Cheat Engine", "Software\\Cheat Engine");
                }
                else
                    SilDev.Run.App(new ProcessStartInfo() { FileName = "%CurrentDir%\\App\\CheatEngine\\Cheat Engine.exe" });
            }
        }
    }
}
