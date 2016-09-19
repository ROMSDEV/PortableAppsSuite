using SilDev;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace FFactoryPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                string appPath = PATH.Combine("%CurDir%\\App\\FormatFactory\\FormatFactory.exe");
                if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                    return;
                if (newInstance)
                {
                    LOG.AllowDebug();

                    if (!REG.ValueExist("HKEY_CURRENT_USER\\Software\\FreeTime", "Portable App"))
                        REG.RenameSubKey("HKEY_CURRENT_USER\\Software\\FreeTime", "Software\\SI13N7-BACKUP: FreeTime");

                    string settingsPath = PATH.Combine("%CurDir%\\Data\\settings.reg");
                    string oldSettingsPath = PATH.Combine("%CurDir%\\Data\\settings.ini");
                    if (File.Exists(oldSettingsPath) && !File.Exists(settingsPath))
                        REG.ImportFile(oldSettingsPath);
                    if (File.Exists(settingsPath))
                        REG.ImportFile(settingsPath);

                    REG.RegKey HKCU = REG.RegKey.CurrentUser;
                    string subKey = "Software\\FreeTime";
                    string appSubKey = "Software\\FreeTime\\FormatFactory";
                    REG.WriteValue(HKCU, subKey, "Portable App", true, REG.RegValueKind.String);
                    REG.WriteValue(HKCU, subKey, "FormatFactory", Path.GetDirectoryName(appPath), REG.RegValueKind.String);
                    REG.WriteValue(HKCU, appSubKey, "CheckNewVersion", 0, REG.RegValueKind.DWord);
                    REG.WriteValue(HKCU, appSubKey, "CodecInstalled", 0, REG.RegValueKind.DWord);
                    REG.WriteValue(HKCU, appSubKey, "OptionActivePage", 0, REG.RegValueKind.DWord);
                    REG.WriteValue(HKCU, appSubKey, "OutputDir", PATH.Combine("%CurDir%\\Data\\Output"), REG.RegValueKind.String);
                    REG.WriteValue(HKCU, appSubKey, "UseCount", 1, REG.RegValueKind.DWord);
#if Legacy
                    REG.WriteValue(HKCU, appSubKey, "Skin", 2, REG.RegValueKind.DWord);
                    REG.WriteValue(HKCU, appSubKey, "Version", "3.3.5", REG.RegValueKind.String);
#else
                    REG.WriteValue(HKCU, appSubKey, "Skin", 6, REG.RegValueKind.DWord);
                    REG.WriteValue(HKCU, appSubKey, "StartMethodTab", 0, REG.RegValueKind.DWord);

                    try
                    {
                        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(PATH.Combine("%CurDir%\\App\\FormatFactory\\FormatFactory.exe"));
                        REG.WriteValue(HKCU, appSubKey, "Version", $"{string.Join(".", new List<string>(fvi.ProductVersion.Split('.')).GetRange(0, 3))}", REG.RegValueKind.String);
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }
#endif
                    RUN.App(new ProcessStartInfo()
                    {
                        Arguments = RUN.CommandLine(),
                        FileName = appPath
                    }, 0);
                    if (File.Exists(oldSettingsPath))
                        File.Delete(oldSettingsPath);
                    REG.ExportFile("HKEY_CURRENT_USER\\Software\\FreeTime", settingsPath);
                    REG.RemoveExistSubKey(HKCU, subKey);
                    REG.RenameSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: FreeTime", "Software\\FreeTime");
                }
            }
        }
    }
}
