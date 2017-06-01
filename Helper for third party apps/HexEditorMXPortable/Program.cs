namespace HexEditorMXPortable
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
            {
                var appPath = PathEx.Combine("%CurDir%\\App\\hexeditmx\\hexeditmx.exe");
                if (newInstance)
                {
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                        return;

                    if (!Reg.EntryExists("HKEY_CURRENT_USER\\Software\\NEXT-Soft", "Portable App"))
                        Reg.MoveSubKey("HKEY_CURRENT_USER\\Software\\NEXT-Soft", "HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: NEXT-Soft");

                    var settingsPath = PathEx.Combine("%CurDir%\\Data\\settings.reg");
                    var settingsDir = Path.GetDirectoryName(settingsPath);
                    if (string.IsNullOrEmpty(settingsDir))
                        return;
                    if (!Directory.Exists(settingsDir))
                        Directory.CreateDirectory(settingsDir);
                    var oldSettingsPath = PathEx.Combine("%CurDir%\\Data\\settings.ini");
                    if (File.Exists(oldSettingsPath) && !File.Exists(settingsPath))
                        Reg.ImportFile(oldSettingsPath);
                    if (File.Exists(settingsPath))
                        Reg.ImportFile(settingsPath);

                    Reg.Write("HKEY_CURRENT_USER\\Software\\NEXT-Soft", "Portable App", "True");

                    using (var p = ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false), false, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();

                    var isRunning = true;
                    while (isRunning)
                    {
                        var runningApp = Process.GetProcessesByName("hexeditmx");
                        isRunning = runningApp.Length > 0;
                        foreach (var app in runningApp)
                            app.WaitForExit();
                    }

                    if (File.Exists(oldSettingsPath))
                        File.Delete(oldSettingsPath);

                    Reg.ExportKeys(settingsPath, "HKEY_CURRENT_USER\\Software\\NEXT-Soft");
                    Reg.RemoveSubKey("HKEY_CURRENT_USER\\Software\\NEXT-Soft");
                    Reg.MoveSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: NEXT-Soft", "HKEY_CURRENT_USER\\Software\\NEXT-Soft");
                }
                else
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false));
            }
        }
    }
}
