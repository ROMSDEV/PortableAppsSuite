using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

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
                    SilDev.Initialization.File(Application.StartupPath, "GyazoPortable.ini");
                    if (File.Exists(SilDev.Initialization.File()))
                    {
                        if (SilDev.Initialization.ReadValue("Settings", "SecondRunMode").ToLower() == "true")
                        {
                            secondRunMode = true;
                            SilDev.Run.App(new ProcessStartInfo() { FileName = "%CurrentDir%\\Gyazo\\Gyazowin.exe" }, 0);
                        }
                    }
                    if (!secondRunMode)
                    {
                        string iniSettings = Path.Combine(Application.StartupPath, "Gyazo\\settings.ini");
                        if (File.Exists(iniSettings))
                        {
                            SilDev.Reg.CreateNewSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\Gyazo");
                            SilDev.Reg.CreateNewSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo");
                            SilDev.Reg.CreateNewSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings");
                            string entries = SilDev.Initialization.ReadValue("Registry", "Entries", iniSettings);
                            if (entries.Contains(","))
                            {
                                foreach (var ent in entries.Split(','))
                                {
                                    string val = SilDev.Initialization.ReadValue("Settings", ent, iniSettings);
                                    if (!string.IsNullOrWhiteSpace(val))
                                        SilDev.Reg.WriteValue(SilDev.Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", ent, val, SilDev.Reg.RegValueKind.DWord);
                                }
                            }
                        }
                        SilDev.Run.App(new ProcessStartInfo() { FileName = "%CurrentDir%\\Gyazo\\GyStation.exe" }, 0);
                        if (!File.Exists(iniSettings))
                            File.Create(iniSettings).Close();
                        Dictionary<string, string> regSettings = new Dictionary<string, string>();
                        regSettings.Add("CaptureGifHotKey", SilDev.Reg.ReadValue(SilDev.Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "CaptureGifHotKey", SilDev.Reg.RegValueKind.DWord));
                        regSettings.Add("CaptureGifHotKeyState", SilDev.Reg.ReadValue(SilDev.Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "CaptureGifHotKeyState", SilDev.Reg.RegValueKind.DWord));
                        regSettings.Add("CaptureHotKey", SilDev.Reg.ReadValue(SilDev.Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "CaptureHotKey", SilDev.Reg.RegValueKind.DWord));
                        regSettings.Add("CaptureHotKeyState", SilDev.Reg.ReadValue(SilDev.Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "CaptureHotKeyState", SilDev.Reg.RegValueKind.DWord));
                        regSettings.Add("PlaySound", SilDev.Reg.ReadValue(SilDev.Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "PlaySound", SilDev.Reg.RegValueKind.DWord));
                        regSettings.Add("PrintScreen", SilDev.Reg.ReadValue(SilDev.Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "PrintScreen", SilDev.Reg.RegValueKind.DWord));
                        regSettings.Add("ResizeFileMaxSize", SilDev.Reg.ReadValue(SilDev.Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "ResizeFileMaxSize", SilDev.Reg.RegValueKind.DWord));
                        regSettings.Add("StayTray", SilDev.Reg.ReadValue(SilDev.Reg.RegKey.CurrentUser, "Software\\Gyazo\\Gyazo\\Settings", "StayTray", SilDev.Reg.RegValueKind.DWord));
                        foreach (var ent in regSettings)
                        {
                            if (!string.IsNullOrWhiteSpace(ent.Value) && ent.Value.ToLower() != "dword")
                            {
                                string entries = SilDev.Initialization.ReadValue("Registry", "Entries", iniSettings);
                                SilDev.Initialization.WriteValue("Registry", "Entries", !string.IsNullOrWhiteSpace(entries) ? string.Format("{0},{1}", entries, ent.Key) : ent.Key, iniSettings);
                                SilDev.Initialization.WriteValue("Settings", ent.Key, ent.Value, iniSettings);
                            }
                        }
                        SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\Gyazo");
                    }
                    string tempDir = Path.Combine(Environment.GetEnvironmentVariable("AppData"), "Gyazo");
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
