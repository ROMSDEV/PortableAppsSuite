using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace HexEditorMXPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                string appPath = Path.Combine(Application.StartupPath, "App\\hexeditmx\\hexeditmx.exe");
                if (newInstance)
                {
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                        return;

                    SilDev.Log.AllowDebug();

                    if (!SilDev.Reg.ValueExist("HKEY_CURRENT_USER\\Software\\NEXT-Soft", "Portable App"))
                        SilDev.Reg.RenameSubKey("HKEY_CURRENT_USER\\Software\\NEXT-Soft", "Software\\SI13N7-BACKUP: NEXT-Soft");

                    string settingsPath = Path.Combine(Application.StartupPath, "Data\\settings.reg");
                    if (!Directory.Exists(Path.GetDirectoryName(settingsPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
                    string oldSettingsPath = Path.Combine(Application.StartupPath, "Data\\settings.ini");
                    if (File.Exists(oldSettingsPath) && !File.Exists(settingsPath))
                        SilDev.Reg.ImportFile(oldSettingsPath);
                    if (File.Exists(settingsPath))
                        SilDev.Reg.ImportFile(settingsPath);

                    SilDev.Reg.WriteValue("HKEY_CURRENT_USER\\Software\\NEXT-Soft", "Portable App", "True");

                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = SilDev.Run.CommandLine(),
                        FileName = appPath
                    }, 0);

                    bool isRunning = true;
                    while (isRunning)
                    {
                        Process[] runningApp = Process.GetProcessesByName("hexeditmx");
                        isRunning = runningApp.Length > 0;
                        foreach (Process app in runningApp)
                            app.WaitForExit();
                    }

                    if (File.Exists(oldSettingsPath))
                        File.Delete(oldSettingsPath);

                    SilDev.Reg.ExportFile("HKEY_CURRENT_USER\\Software\\NEXT-Soft", settingsPath);
                    SilDev.Reg.RemoveExistSubKey("HKEY_CURRENT_USER\\Software\\NEXT-Soft");
                    SilDev.Reg.RenameSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: NEXT-Soft", "Software\\NEXT-Soft");
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
