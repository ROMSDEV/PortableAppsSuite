namespace CDBurnerXPPortable
{
    using System;
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
#if x86
                    var appDir = PathEx.Combine("%CurDir%\\CDBurnerXP");
#else
                    var appDir = PathEx.Combine("%CurDir%\\CDBurnerXP64");
#endif
                    var appPath = Path.Combine(appDir, "cdbxpp.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                        return;
                    var iniPath = Path.Combine(appDir, "Config.ini");
                    if (!File.Exists(iniPath))
                        File.WriteAllLines(iniPath, new[] { @"[CDBurnerXP]", @"Portable=1" });
                    using (var p = ProcessEx.Start(appPath, true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    Reg.RemoveExistSubKey(Reg.RegKey.CurrentUser, "Software\\Canneverbe Limited");
                }
        }
    }
}
