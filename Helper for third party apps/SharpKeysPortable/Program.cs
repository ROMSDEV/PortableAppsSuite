using SilDev;
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
                    INI.File($"%CurDir%\\{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}.ini");
                    if (INI.Read("Settings", "ShowWarning").ToLower() == "true")
                    {
                        REG.CreateNewSubKey(REG.RegKey.CurrentUser, "Software\\RandyRants\\SharpKeys");
                        REG.WriteValue(REG.RegKey.CurrentUser, "Software\\RandyRants\\SharpKeys", "ShowWarning", "1", REG.RegValueKind.DWord);
                    }
                    RUN.App(new ProcessStartInfo() { FileName = "%CurDir%\\SharpKeys\\SharpKeys.exe" }, 0);
                    INI.Write("Settings", "ShowWarning", true);
                    REG.RemoveExistSubKey(REG.RegKey.CurrentUser, "Software\\RandyRants\\SharpKeys");
                    if (REG.GetSubKeys(REG.RegKey.CurrentUser, "Software\\RandyRants").Count <= 0)
                        REG.RemoveExistSubKey(REG.RegKey.CurrentUser, "Software\\RandyRants");
                }
            }
        }
    }
}
