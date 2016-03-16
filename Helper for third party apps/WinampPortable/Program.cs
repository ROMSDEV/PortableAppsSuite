using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WinampPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SilDev.Log.AllowDebug();
            SilDev.Ini.File(Application.StartupPath, "winamp\\Winamp.ini");

            // Protable enforcement
            SilDev.Ini.Write("Winamp", "no_registry", 0);
            SilDev.Ini.Write("WinampReg", "NeedReg", 0);

            // Set default settings if not exists
            SilDev.Ini.Write("Winamp", "skin", "Big Bento", false, true);
            SilDev.Ini.Write("WinampReg", "skin", "Big Bento");
            SilDev.Ini.Write("Winamp", "eq_data", "24,19,40,46,42,31,19,16,26,32", false, true);
            SilDev.Ini.Write("gen_hotkeys", "nbkeys", 15, false, true);
            SilDev.Ini.Write("gen_hotkeys", "version", 2, false, true);
            SilDev.Ini.Write("gen_hotkeys", "enabled", 0, false, true);
            SilDev.Ini.Write("gen_hotkeys", "appcommand", 0, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action0", "ghkdc play", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey0", 3629, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action1", "ghkdc pause", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey1", 3620, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action2", "ghkdc stop", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey2", 3619, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action3", "ghkdc prev", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey3", 3617, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action4", "ghkdc next", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey4", 3618, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action5", "ghkdc vup", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey5", 3622, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action6", "ghkdc vdown", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey6", 3624, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action7", "ghkdc forward", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey7", 3623, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action8", "ghkdc rewind", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey8", 3621, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action9", "ghkdc jump", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey9", 3658, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action10", "ghkdc file", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey10", 3660, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action11", "ghkdc stop", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey11", 2226, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action12", "ghkdc play/pause", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey12", 2227, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action13", "ghkdc prev", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey13", 2225, false, true);
            SilDev.Ini.Write("gen_hotkeys", "action14", "ghkdc next", false, true);
            SilDev.Ini.Write("gen_hotkeys", "hotkey14", 2224, false, true);
            SilDev.Ini.Write("gen_hotkeys", "col1", 212, false, true);
            SilDev.Ini.Write("gen_hotkeys", "col2", 177, false, true);

            SilDev.Run.App(new ProcessStartInfo()
            {
                Arguments = SilDev.Run.CommandLine(),
                FileName = "%CurrentDir%\\winamp\\winamp.exe"
            });
        }
    }
}
