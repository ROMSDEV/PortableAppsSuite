using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

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
                    string appPath = Path.Combine(Application.StartupPath, "CDBurnerXP\\cdbxpp.exe");
                    if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                        return;
                    string iniPath = Path.Combine(Application.StartupPath, "CDBurnerXP\\Config.ini");
                    if (!File.Exists(iniPath))
                        File.WriteAllLines(iniPath, new string[] { "[CDBurnerXP]", "Portable=1" });
                    SilDev.Run.App(appPath, 0);
                    SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\Canneverbe Limited");
                }
            }
        }
    }
}
