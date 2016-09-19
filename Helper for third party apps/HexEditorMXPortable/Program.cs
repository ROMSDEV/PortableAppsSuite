using SilDev;
using System;
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
                string appPath = PATH.Combine("%CurDir%\\App\\hexeditmx\\hexeditmx.exe");
                if (newInstance)
                {
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                        return;

                    LOG.AllowDebug();

                    if (!REG.ValueExist("HKEY_CURRENT_USER\\Software\\NEXT-Soft", "Portable App"))
                        REG.RenameSubKey("HKEY_CURRENT_USER\\Software\\NEXT-Soft", "Software\\SI13N7-BACKUP: NEXT-Soft");

                    string settingsPath = PATH.Combine("%CurDir%\\Data\\settings.reg");
                    if (!Directory.Exists(Path.GetDirectoryName(settingsPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
                    string oldSettingsPath = PATH.Combine("%CurDir%\\Data\\settings.ini");
                    if (File.Exists(oldSettingsPath) && !File.Exists(settingsPath))
                        REG.ImportFile(oldSettingsPath);
                    if (File.Exists(settingsPath))
                        REG.ImportFile(settingsPath);

                    REG.WriteValue("HKEY_CURRENT_USER\\Software\\NEXT-Soft", "Portable App", "True");

                    RUN.App(new ProcessStartInfo()
                    {
                        Arguments = RUN.CommandLine(),
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

                    REG.ExportFile("HKEY_CURRENT_USER\\Software\\NEXT-Soft", settingsPath);
                    REG.RemoveExistSubKey("HKEY_CURRENT_USER\\Software\\NEXT-Soft");
                    REG.RenameSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: NEXT-Soft", "Software\\NEXT-Soft");
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
