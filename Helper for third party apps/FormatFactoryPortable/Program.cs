namespace FFactoryPortable
{
    using System;
#if !Legacy
    using System.Collections.Generic;
#endif
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
                var appPath = PathEx.Combine("%CurDir%\\App\\FormatFactory\\FormatFactory.exe");
                if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                    return;
                if (!newInstance)
                    return;
                if (!Reg.ValueExist("HKEY_CURRENT_USER\\Software\\FreeTime", "Portable App"))
                    Reg.MoveSubKey("HKEY_CURRENT_USER\\Software\\FreeTime", "Software\\SI13N7-BACKUP: FreeTime");

                var settingsPath = PathEx.Combine("%CurDir%\\Data\\settings.reg");
                var oldSettingsPath = PathEx.Combine("%CurDir%\\Data\\settings.ini");
                if (File.Exists(oldSettingsPath) && !File.Exists(settingsPath))
                    Reg.ImportFile(oldSettingsPath);
                if (File.Exists(settingsPath))
                    Reg.ImportFile(settingsPath);

                const string appSubKey = "Software\\FreeTime\\FormatFactory";
                const Reg.RegKey hkcu = Reg.RegKey.CurrentUser;
                const string subKey = "Software\\FreeTime";
                Reg.WriteValue(hkcu, subKey, "Portable App", true, Reg.RegValueKind.String);
                Reg.WriteValue(hkcu, subKey, "FormatFactory", Path.GetDirectoryName(appPath), Reg.RegValueKind.String);
                Reg.WriteValue(hkcu, appSubKey, "CheckNewVersion", 0, Reg.RegValueKind.DWord);
                Reg.WriteValue(hkcu, appSubKey, "CodecInstalled", 0, Reg.RegValueKind.DWord);
                Reg.WriteValue(hkcu, appSubKey, "OptionActivePage", 0, Reg.RegValueKind.DWord);
                Reg.WriteValue(hkcu, appSubKey, "OutputDir", PathEx.Combine("%CurDir%\\Data\\Output"), Reg.RegValueKind.String);
                Reg.WriteValue(hkcu, appSubKey, "UseCount", 1, Reg.RegValueKind.DWord);
#if Legacy
                Reg.WriteValue(hkcu, appSubKey, "Skin", 2, Reg.RegValueKind.DWord);
                Reg.WriteValue(hkcu, appSubKey, "Version", "3.3.5", Reg.RegValueKind.String);
#else
                Reg.WriteValue(hkcu, appSubKey, "Skin", 6, Reg.RegValueKind.DWord);
                Reg.WriteValue(hkcu, appSubKey, "StartMethodTab", 0, Reg.RegValueKind.DWord);

                try
                {
                    var fvi = FileVersionInfo.GetVersionInfo(PathEx.Combine("%CurDir%\\App\\FormatFactory\\FormatFactory.exe"));
                    Reg.WriteValue(hkcu, appSubKey, "Version", $"{string.Join(".", new List<string>(fvi.ProductVersion.Split('.')).GetRange(0, 3))}", Reg.RegValueKind.String);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
#endif
                using (var p = ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false), false, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
                if (File.Exists(oldSettingsPath))
                    File.Delete(oldSettingsPath);
                Reg.ExportKeys(settingsPath, "HKEY_CURRENT_USER\\Software\\FreeTime");
                Reg.RemoveExistSubKey(hkcu, subKey);
                Reg.MoveSubKey("HKEY_CURRENT_USER\\Software\\SI13N7-BACKUP: FreeTime", "Software\\FreeTime");
            }
        }
    }
}
