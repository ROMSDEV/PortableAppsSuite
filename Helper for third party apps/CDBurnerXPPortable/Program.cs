using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CDBurnerXPPortable
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
                    SilDev.Log.AllowDebug();
#if x86
                    string appDir = SilDev.Run.EnvVarFilter("%CurrentDir%\\CDBurnerXP");
#else
                    string appDir = SilDev.Run.EnvVarFilter("%CurrentDir%\\CDBurnerXP64");
#endif
                    string appPath = Path.Combine(appDir, "cdbxpp.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                        return;
                    string iniPath = Path.Combine(appDir, "Config.ini");
                    if (!File.Exists(iniPath))
                        File.WriteAllLines(iniPath, new string[] { "[CDBurnerXP]", "Portable=1" });
                    SilDev.Run.App(new ProcessStartInfo() { FileName = appPath }, 0);
                    SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\Canneverbe Limited");
                }
            }
        }
    }
}
