namespace WinampPortable
{
    using System;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();

            Ini.File("%CurDir%\\winamp\\Winamp.ini");

            // Protable enforcement
            Ini.Write("Winamp", "no_registry", 0);
            Ini.Write("WinampReg", "NeedReg", 0);

            // Set default settings if not exists
            Ini.Write("Winamp", "skin", "Big Bento", false, true);
            Ini.Write("WinampReg", "skin", "Big Bento");
            Ini.Write("Winamp", "eq_data", "24,19,40,46,42,31,19,16,26,32", false, true);
            Ini.Write("gen_hotkeys", "nbkeys", 15, false, true);
            Ini.Write("gen_hotkeys", "version", 2, false, true);
            Ini.Write("gen_hotkeys", "enabled", 0, false, true);
            Ini.Write("gen_hotkeys", "appcommand", 0, false, true);
            Ini.Write("gen_hotkeys", "action0", "ghkdc play", false, true);
            Ini.Write("gen_hotkeys", "hotkey0", 3629, false, true);
            Ini.Write("gen_hotkeys", "action1", "ghkdc pause", false, true);
            Ini.Write("gen_hotkeys", "hotkey1", 3620, false, true);
            Ini.Write("gen_hotkeys", "action2", "ghkdc stop", false, true);
            Ini.Write("gen_hotkeys", "hotkey2", 3619, false, true);
            Ini.Write("gen_hotkeys", "action3", "ghkdc prev", false, true);
            Ini.Write("gen_hotkeys", "hotkey3", 3617, false, true);
            Ini.Write("gen_hotkeys", "action4", "ghkdc next", false, true);
            Ini.Write("gen_hotkeys", "hotkey4", 3618, false, true);
            Ini.Write("gen_hotkeys", "action5", "ghkdc vup", false, true);
            Ini.Write("gen_hotkeys", "hotkey5", 3622, false, true);
            Ini.Write("gen_hotkeys", "action6", "ghkdc vdown", false, true);
            Ini.Write("gen_hotkeys", "hotkey6", 3624, false, true);
            Ini.Write("gen_hotkeys", "action7", "ghkdc forward", false, true);
            Ini.Write("gen_hotkeys", "hotkey7", 3623, false, true);
            Ini.Write("gen_hotkeys", "action8", "ghkdc rewind", false, true);
            Ini.Write("gen_hotkeys", "hotkey8", 3621, false, true);
            Ini.Write("gen_hotkeys", "action9", "ghkdc jump", false, true);
            Ini.Write("gen_hotkeys", "hotkey9", 3658, false, true);
            Ini.Write("gen_hotkeys", "action10", "ghkdc file", false, true);
            Ini.Write("gen_hotkeys", "hotkey10", 3660, false, true);
            Ini.Write("gen_hotkeys", "action11", "ghkdc stop", false, true);
            Ini.Write("gen_hotkeys", "hotkey11", 2226, false, true);
            Ini.Write("gen_hotkeys", "action12", "ghkdc play/pause", false, true);
            Ini.Write("gen_hotkeys", "hotkey12", 2227, false, true);
            Ini.Write("gen_hotkeys", "action13", "ghkdc prev", false, true);
            Ini.Write("gen_hotkeys", "hotkey13", 2225, false, true);
            Ini.Write("gen_hotkeys", "action14", "ghkdc next", false, true);
            Ini.Write("gen_hotkeys", "hotkey14", 2224, false, true);
            Ini.Write("gen_hotkeys", "col1", 212, false, true);
            Ini.Write("gen_hotkeys", "col2", 177, false, true);

            ProcessEx.Start("%CurDir%\\winamp\\winamp.exe", EnvironmentEx.CommandLine());
        }
    }
}
