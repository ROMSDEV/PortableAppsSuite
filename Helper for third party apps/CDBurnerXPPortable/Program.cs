using SilDev;
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
                    LOG.AllowDebug();
#if x86
                    string appDir = PATH.Combine("%CurDir%\\CDBurnerXP");
#else
                    string appDir = PATH.Combine("%CurDir%\\CDBurnerXP64");
#endif
                    string appPath = Path.Combine(appDir, "cdbxpp.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                        return;
                    string iniPath = Path.Combine(appDir, "Config.ini");
                    if (!File.Exists(iniPath))
                        File.WriteAllLines(iniPath, new string[] { "[CDBurnerXP]", "Portable=1" });
                    RUN.App(new ProcessStartInfo() { FileName = appPath }, 0);
                    REG.RemoveExistSubKey(REG.RegKey.CurrentUser, "Software\\Canneverbe Limited");
                }
            }
        }
    }
}
