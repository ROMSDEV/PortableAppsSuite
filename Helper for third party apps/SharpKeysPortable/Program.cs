using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SharpKeysPortable
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
                    SilDev.Reg.CreateNewSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\RandyRants\\SharpKeys");
                    SilDev.Reg.WriteValue(SilDev.Reg.RegKey.CurrentUser, "Software\\RandyRants\\SharpKeys", "ShowWarning", "1", SilDev.Reg.RegValueKind.DWord);
                    SilDev.Run.App(Application.StartupPath, "SharpKeys\\SharpKeys.exe", 0);
                    SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\RandyRants\\SharpKeys");
                    if (SilDev.Reg.GetSubKeys(SilDev.Reg.RegKey.CurrentUser, "Software\\RandyRants").Count <= 0)
                        SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\RandyRants");
                }
                else
                    Environment.Exit(2);
            }
        }
    }
}
