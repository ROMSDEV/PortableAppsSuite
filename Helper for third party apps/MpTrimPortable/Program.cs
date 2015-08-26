using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace mpTrimPortable
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
                    SilDev.Initialization.File(Application.StartupPath, "mpTrim\\mpTrim.ini");
                    string left = SilDev.Initialization.ReadValue("Settings", "Left");
                    if (string.IsNullOrWhiteSpace(left))
                        left = "50";
                    string top = SilDev.Initialization.ReadValue("Settings", "Top");
                    if (string.IsNullOrWhiteSpace(left))
                        top = "50";
                    SilDev.Reg.CreateNewSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\mpTrim");
                    SilDev.Reg.WriteValue(SilDev.Reg.RegKey.CurrentUser, "Software\\mpTrim", "MainFormLeft", left, SilDev.Reg.RegValueKind.DWord);
                    SilDev.Reg.WriteValue(SilDev.Reg.RegKey.CurrentUser, "Software\\mpTrim", "MainFormTop", top, SilDev.Reg.RegValueKind.DWord);
                    SilDev.Run.App(Application.StartupPath, "mpTrim\\mpTrim.exe", 0);
                    left = SilDev.Reg.ReadValue(SilDev.Reg.RegKey.CurrentUser, "Software\\mpTrim", "MainFormLeft").ToString();
                    top = SilDev.Reg.ReadValue(SilDev.Reg.RegKey.CurrentUser, "Software\\mpTrim", "MainFormTop").ToString();
                    if (!string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(top))
                    {
                        SilDev.Initialization.WriteValue("Settings", "Left", left);
                        SilDev.Initialization.WriteValue("Settings", "Top", top);
                    }
                    SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\mpTrim");
                }
                else
                    SilDev.Run.App(Application.StartupPath, "mpTrim\\mpTrim.exe");
            }
        }
    }
}
