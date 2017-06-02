namespace CheatEnginePortable
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
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                var appPath = PathEx.Combine("%CurDir%\\App\\CheatEngine");
                if (!File.Exists(Path.Combine(appPath, "Cheat Engine.exe")))
                    return;
                if (newInstance)
                {
                    var dataTables = PathEx.Combine("%CurDir%\\Data\\My Cheat Tables");
                    if (!Directory.Exists(dataTables))
                        Directory.CreateDirectory(dataTables);
                    var dataSettings = PathEx.Combine("%CurDir%\\Data\\settings.reg");
                    if (!File.Exists(dataSettings))
                    {
                        var dataDir = PathEx.Combine("%CurDir%\\Data");
                        if (!Directory.Exists(dataDir))
                            Directory.CreateDirectory(dataDir);
                        string[] defaultSettings =
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
                        Ini.Write("HKEY_CURRENT_USER\\Software\\Cheat Engine", "\"Initial tables dir\"", $"\"{dataTables.Replace("\\", "\\\\")}\"", dataSettings);
                    if (!Reg.EntryExists("HKEY_CURRENT_USER\\Software\\Cheat Engine", "Portable App"))
                        Reg.MoveSubKey("HKEY_CURRENT_USER\\Software\\Cheat Engine", "HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: Cheat Engine");
                    Reg.ImportFile(dataSettings);
                    using (var p = ProcessEx.Start(appPath, true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    var isRunning = true;
                    while (isRunning)
                    {
                        var runningApp = Process.GetProcessesByName($"cheatengine-{(Environment.Is64BitOperatingSystem ? "x86_64" : "i386")}");
                        isRunning = runningApp.Length > 0;
                        foreach (var app in runningApp)
                            app.WaitForExit();
                    }
                    var userTables = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Cheat Tables");
                    if (Directory.Exists(userTables))
                        Directory.Delete(userTables, true);
                    Reg.ExportKeys(dataSettings, "HKEY_CURRENT_USER\\Software\\Cheat Engine");
                    Reg.RemoveSubKey("HKEY_CURRENT_USER\\Software\\Cheat Engine");
                    Reg.MoveSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: Cheat Engine", "HKEY_CURRENT_USER\\Software\\Cheat Engine");
                }
                else
                    ProcessEx.Start(appPath, true);
            }
        }
    }
}
