namespace FFactoryPortable
{
    using System;
#if !Legacy
    using System.Collections.Generic;
#endif
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using Microsoft.Win32;
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
                if (!newInstance)
                    return;

                var appPath = PathEx.Combine("%CurDir%\\App\\FormatFactory\\FormatFactory.exe");

                if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                    return;

                if (!Reg.EntryExists("HKCU\\Software\\FreeTime", "Portable App"))
                    Reg.MoveSubKey("HKCU\\Software\\FreeTime", "HKCU\\Software\\SI13N7-BACKUP: FreeTime");

                var settingsPath = PathEx.Combine("%CurDir%\\Data\\settings.reg");
                if (File.Exists(settingsPath))
                    Reg.ImportFile(settingsPath);

                var hkcu = Registry.CurrentUser;
                const string appSubKey = "Software\\FreeTime\\FormatFactory";
                const string subKey = "Software\\FreeTime";
                Reg.Write(hkcu, subKey, "Portable App", true, RegistryValueKind.String);
                Reg.Write(hkcu, subKey, "FormatFactory", Path.GetDirectoryName(appPath), RegistryValueKind.String);
                Reg.Write(hkcu, appSubKey, "CheckNewVersion", 0, RegistryValueKind.DWord);
                Reg.Write(hkcu, appSubKey, "CodecInstalled", 0, RegistryValueKind.DWord);
                Reg.Write(hkcu, appSubKey, "OptionActivePage", 0, RegistryValueKind.DWord);
                Reg.Write(hkcu, appSubKey, "OutputDir", PathEx.Combine("%CurDir%\\Data\\Output"), RegistryValueKind.String);
                Reg.Write(hkcu, appSubKey, "UseCount", 1, RegistryValueKind.DWord);
#if Legacy
                Reg.Write(hkcu, appSubKey, "Skin", 2, RegistryValueKind.DWord);
                Reg.Write(hkcu, appSubKey, "Version", "3.3.5", RegistryValueKind.String);
#else
                Reg.Write(hkcu, appSubKey, "Skin", 6, RegistryValueKind.DWord);
                Reg.Write(hkcu, appSubKey, "StartMethodTab", 0, RegistryValueKind.DWord);

                try
                {
                    var fvi = FileVersionInfo.GetVersionInfo(PathEx.Combine("%CurDir%\\App\\FormatFactory\\FormatFactory.exe"));
                    Reg.Write(hkcu, appSubKey, "Version", $"{string.Join(".", new List<string>(fvi.ProductVersion.Split('.')).GetRange(0, 3))}", RegistryValueKind.String);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
#endif
                using (var p = ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false), false, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();

                bool isRunning;
                do
                {
                    isRunning = ProcessEx.IsRunning(appPath);
                    Thread.Sleep(200);
                }
                while (isRunning);

                Reg.ExportKeys(settingsPath, "HKCU\\Software\\FreeTime");
                Reg.RemoveSubKey(hkcu, subKey);
                Reg.MoveSubKey("HKCU\\Software\\SI13N7-BACKUP: FreeTime", "HKCU\\Software\\FreeTime");
            }
        }
    }
}
