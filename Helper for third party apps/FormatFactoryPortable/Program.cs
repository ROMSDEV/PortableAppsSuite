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
                string appPath = Path.Combine(Application.StartupPath, "App\\FormatFactory\\FormatFactory.exe");
                if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                    return;
                if (newInstance)
                {
                    SilDev.Log.AllowDebug();

                    if (!SilDev.Reg.ValueExist("HKEY_CURRENT_USER\\Software\\FreeTime", "Portable App"))
                        SilDev.Reg.RenameSubKey("HKEY_CURRENT_USER\\Software\\FreeTime", "Software\\SI13N7-BACKUP: FreeTime");

                    string settingsPath = Path.Combine(Application.StartupPath, "Data\\settings.reg");
                    string oldSettingsPath = Path.Combine(Application.StartupPath, "Data\\settings.ini");
                    if (File.Exists(oldSettingsPath) && !File.Exists(settingsPath))
                        SilDev.Reg.ImportFile(oldSettingsPath);
                    if (File.Exists(settingsPath))
                        SilDev.Reg.ImportFile(settingsPath);

                    SilDev.Reg.RegKey HKCU = SilDev.Reg.RegKey.CurrentUser;
                    string subKey = "Software\\FreeTime";
                    string appSubKey = "Software\\FreeTime\\FormatFactory";
                    SilDev.Reg.WriteValue(HKCU, subKey, "Portable App", true, SilDev.Reg.RegValueKind.String);
                    SilDev.Reg.WriteValue(HKCU, subKey, "FormatFactory", Path.GetDirectoryName(appPath), SilDev.Reg.RegValueKind.String);
                    SilDev.Reg.WriteValue(HKCU, appSubKey, "CheckNewVersion", 0, SilDev.Reg.RegValueKind.DWord);
                    SilDev.Reg.WriteValue(HKCU, appSubKey, "CodecInstalled", 0, SilDev.Reg.RegValueKind.DWord);
                    SilDev.Reg.WriteValue(HKCU, appSubKey, "OptionActivePage", 0, SilDev.Reg.RegValueKind.DWord);
                    SilDev.Reg.WriteValue(HKCU, appSubKey, "OutputDir", Path.Combine(Application.StartupPath, "Data\\Output"), SilDev.Reg.RegValueKind.String);
                    SilDev.Reg.WriteValue(HKCU, appSubKey, "UseCount", 1, SilDev.Reg.RegValueKind.DWord);
#if Legacy
                    SilDev.Reg.WriteValue(HKCU, appSubKey, "Skin", 2, SilDev.Reg.RegValueKind.DWord);
                    SilDev.Reg.WriteValue(HKCU, appSubKey, "Version", "3.3.5", SilDev.Reg.RegValueKind.String);
#else
                    SilDev.Reg.WriteValue(HKCU, appSubKey, "Skin", 6, SilDev.Reg.RegValueKind.DWord);
                    SilDev.Reg.WriteValue(HKCU, appSubKey, "StartMethodTab", 0, SilDev.Reg.RegValueKind.DWord);

                    try
                    {
                        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Path.Combine(Application.StartupPath, "App\\FormatFactory\\FormatFactory.exe"));
                        SilDev.Reg.WriteValue(HKCU, appSubKey, "Version", $"{string.Join(".", new List<string>(fvi.ProductVersion.Split('.')).GetRange(0, 3))}", SilDev.Reg.RegValueKind.String);
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
#endif
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        Arguments = SilDev.Run.CommandLine(),
                        FileName = appPath
                    }, 0);
                    if (File.Exists(oldSettingsPath))
                        File.Delete(oldSettingsPath);
                    SilDev.Reg.ExportFile("HKEY_CURRENT_USER\\Software\\FreeTime", settingsPath);
                    SilDev.Reg.RemoveExistSubKey(HKCU, subKey);
                    SilDev.Reg.RenameSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: FreeTime", "Software\\FreeTime");
                }
            }
        }
    }
}
