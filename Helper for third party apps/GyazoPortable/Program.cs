namespace GyazoPortable
{
    using System;
    using System.Collections.Generic;
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
                if (newInstance)
                {
                    var secondRunMode = false;
                    Ini.File($"%CurDir%\\{Path.GetFileNameWithoutExtension(PathEx.LocalPath)}.ini");
                    if (File.Exists(Ini.File()))
                        if (Ini.Read("Settings", "SecondRunMode").EqualsEx("true"))
                        {
                            secondRunMode = true;
                            using (var p = ProcessEx.Start("%CurDir%\\Gyazo\\Gyazowin.exe", true, false))
                                if (!p?.HasExited == true)
                                    p?.WaitForExit();
                        }
                    if (!secondRunMode)
                    {
                        var iniSettings = PathEx.Combine("%CurDir%\\Gyazo\\settings.ini");
                        if (File.Exists(iniSettings))
                        {
                            Reg.CreateNewSubKey(Reg.RegKey.CurrentUser, "Software\\Gyazo");
                            Reg.CreateNewSubKey(Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo");
                            Reg.CreateNewSubKey(Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings");
                            var entries = Ini.Read("Registry", "Entries", iniSettings);
                            if (entries.Contains(","))
                                foreach (var ent in entries.Split(','))
                                {
                                    var val = Ini.Read("Settings", ent, iniSettings);
                                    if (!string.IsNullOrWhiteSpace(val))
                                        Reg.WriteValue(Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", ent, val, Reg.RegValueKind.DWord);
                                }
                        }
                        using (var p = ProcessEx.Start("%CurDir%\\Gyazo\\GyStation.exe", true, false))
                            if (!p?.HasExited == true)
                                p?.WaitForExit();
                        if (!File.Exists(iniSettings))
                            File.Create(iniSettings).Close();
                        var regSettings = new Dictionary<string, string>
                        {
                            { "CaptureGifHotKey", Reg.ReadStringValue(Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "CaptureGifHotKey", Reg.RegValueKind.DWord) },
                            { "CaptureGifHotKeyState", Reg.ReadStringValue(Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "CaptureGifHotKeyState", Reg.RegValueKind.DWord) },
                            { "CaptureHotKey", Reg.ReadStringValue(Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "CaptureHotKey", Reg.RegValueKind.DWord) },
                            { "CaptureHotKeyState", Reg.ReadStringValue(Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "CaptureHotKeyState", Reg.RegValueKind.DWord) },
                            { "PlaySound", Reg.ReadStringValue(Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "PlaySound", Reg.RegValueKind.DWord) },
                            { "PrintScreen", Reg.ReadStringValue(Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "PrintScreen", Reg.RegValueKind.DWord) },
                            { "ResizeFileMaxSize", Reg.ReadStringValue(Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "ResizeFileMaxSize", Reg.RegValueKind.DWord) },
                            { "StayTray", Reg.ReadStringValue(Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "StayTray", Reg.RegValueKind.DWord) }
                        };
                        foreach (var ent in regSettings)
                        {
                            if (string.IsNullOrWhiteSpace(ent.Value) || ent.Value.ToLower() == "dword")
                                continue;
                            var entries = Ini.Read("Registry", "Entries", iniSettings);
                            Ini.Write("Registry", "Entries", !string.IsNullOrWhiteSpace(entries) ? $"{entries},{ent.Key}" : ent.Key, iniSettings);
                            Ini.Write("Settings", ent.Key, ent.Value, iniSettings);
                        }
                        Reg.RemoveExistSubKey(Reg.RegKey.CurrentUser, "Software\\Gyazo");
                    }
                    var tempDir = PathEx.Combine("%AppData%\\Gyazo");
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                }
        }
    }
}
