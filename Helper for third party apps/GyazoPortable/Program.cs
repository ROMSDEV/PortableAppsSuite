using SilDev;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GyazoPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    bool secondRunMode = false;
                    INI.File($"%CurDir%\\{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}.ini");
                    if (File.Exists(INI.File()))
                    {
                        if (INI.Read("Settings", "SecondRunMode").ToLower() == "true")
                        {
                            secondRunMode = true;
                            RUN.App(new ProcessStartInfo() { FileName = "%CurDir%\\Gyazo\\Gyazowin.exe" }, 0);
                        }
                    }
                    if (!secondRunMode)
                    {
                        string iniSettings = PATH.Combine("%CurDir%\\Gyazo\\settings.ini");
                        if (File.Exists(iniSettings))
                        {
                            REG.CreateNewSubKey(REG.RegKey.CurrentUser, "Software\\Gyazo");
                            REG.CreateNewSubKey(REG.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo");
                            REG.CreateNewSubKey(REG.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings");
                            string entries = INI.Read("Registry", "Entries", iniSettings);
                            if (entries.Contains(","))
                            {
                                foreach (var ent in entries.Split(','))
                                {
                                    string val = INI.Read("Settings", ent, iniSettings);
                                    if (!string.IsNullOrWhiteSpace(val))
                                        REG.WriteValue(REG.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", ent, val, REG.RegValueKind.DWord);
                                }
                            }
                        }
                        RUN.App(new ProcessStartInfo() { FileName = "%CurDir%\\Gyazo\\GyStation.exe" }, 0);
                        if (!File.Exists(iniSettings))
                            File.Create(iniSettings).Close();
                        Dictionary<string, string> regSettings = new Dictionary<string, string>();
                        regSettings.Add("CaptureGifHotKey", REG.ReadValue(REG.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "CaptureGifHotKey", REG.RegValueKind.DWord));
                        regSettings.Add("CaptureGifHotKeyState", REG.ReadValue(REG.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "CaptureGifHotKeyState", REG.RegValueKind.DWord));
                        regSettings.Add("CaptureHotKey", REG.ReadValue(REG.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "CaptureHotKey", REG.RegValueKind.DWord));
                        regSettings.Add("CaptureHotKeyState", REG.ReadValue(REG.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "CaptureHotKeyState", REG.RegValueKind.DWord));
                        regSettings.Add("PlaySound", REG.ReadValue(REG.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "PlaySound", REG.RegValueKind.DWord));
                        regSettings.Add("PrintScreen", REG.ReadValue(REG.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "PrintScreen", REG.RegValueKind.DWord));
                        regSettings.Add("ResizeFileMaxSize", REG.ReadValue(REG.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "ResizeFileMaxSize", REG.RegValueKind.DWord));
                        regSettings.Add("StayTray", REG.ReadValue(REG.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "StayTray", REG.RegValueKind.DWord));
                        foreach (var ent in regSettings)
                        {
                            if (!string.IsNullOrWhiteSpace(ent.Value) && ent.Value.ToLower() != "dword")
                            {
                                string entries = INI.Read("Registry", "Entries", iniSettings);
                                INI.Write("Registry", "Entries", !string.IsNullOrWhiteSpace(entries) ? string.Format("{0},{1}", entries, ent.Key) : ent.Key, iniSettings);
                                INI.Write("Settings", ent.Key, ent.Value, iniSettings);
                            }
                        }
                        REG.RemoveExistSubKey(REG.RegKey.CurrentUser, "Software\\Gyazo");
                    }
                    string tempDir = Path.Combine(Environment.GetEnvironmentVariable("AppData"), "Gyazo");
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
