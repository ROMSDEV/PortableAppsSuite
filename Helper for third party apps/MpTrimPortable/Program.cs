using System;
using System.Diagnostics;
using System.Drawing;
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
                    SilDev.Log.AllowDebug();

                    string iniPath = SilDev.Run.EnvVarFilter("%CurrentDir%\\mpTrim\\mpTrim.ini");

                    int Left = 0;
                    if (!int.TryParse(SilDev.Ini.Read("Settings", "Left", iniPath), out Left))
                        Left = (int)Math.Round((Screen.PrimaryScreen.WorkingArea.Width / 2f) - (335f / 2f));
                    int Top = 0;
                    if (!int.TryParse(SilDev.Ini.Read("Settings", "Top", iniPath), out Top))
                        Top = (int)Math.Round((Screen.PrimaryScreen.WorkingArea.Height / 2f) - (410f / 2f));
                    SilDev.Reg.CreateNewSubKey("HKCU\\Software\\mpTrim");
                    SilDev.Reg.WriteValue("HKCU", "Software\\mpTrim", "MainFormLeft", Left, SilDev.Reg.RegValueKind.DWord);
                    SilDev.Reg.WriteValue("HKCU", "Software\\mpTrim", "MainFormTop", Top, SilDev.Reg.RegValueKind.DWord);

                    SilDev.Run.App(new ProcessStartInfo() { FileName = "%CurrentDir%\\mpTrim\\mpTrim.exe" }, 0);

                    string left = SilDev.Reg.ReadValue("HKCU\\Software\\mpTrim", "MainFormLeft");
                    string top = SilDev.Reg.ReadValue("HKCU\\Software\\mpTrim", "MainFormTop");
                    if (!string.IsNullOrWhiteSpace(left) && left != Left.ToString() &&
                        !string.IsNullOrWhiteSpace(top) && top != Top.ToString())
                    {
                        SilDev.Ini.File(iniPath);
                        SilDev.Ini.Write("Settings", "Left", left);
                        SilDev.Ini.Write("Settings", "Top", top);
                    }
                    SilDev.Reg.RemoveExistSubKey("HKCU\\Software\\mpTrim");
                }
                else
                    SilDev.Run.App(new ProcessStartInfo() { FileName = "%CurrentDir%\\mpTrim\\mpTrim.exe" });
            }
        }
    }
}
