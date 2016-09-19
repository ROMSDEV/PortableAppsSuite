using SilDev;
using System;
using System.Diagnostics;
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
                    LOG.AllowDebug();

                    string iniPath = PATH.Combine("%CurDir%\\mpTrim\\mpTrim.ini");

                    int Left = 0;
                    if (!int.TryParse(INI.Read("Settings", "Left", iniPath), out Left))
                        Left = (int)Math.Round((Screen.PrimaryScreen.WorkingArea.Width / 2f) - (335f / 2f));
                    int Top = 0;
                    if (!int.TryParse(INI.Read("Settings", "Top", iniPath), out Top))
                        Top = (int)Math.Round((Screen.PrimaryScreen.WorkingArea.Height / 2f) - (410f / 2f));
                    REG.CreateNewSubKey("HKCU\\Software\\mpTrim");
                    REG.WriteValue("HKCU", "Software\\mpTrim", "MainFormLeft", Left, REG.RegValueKind.DWord);
                    REG.WriteValue("HKCU", "Software\\mpTrim", "MainFormTop", Top, REG.RegValueKind.DWord);

                    RUN.App(new ProcessStartInfo() { FileName = "%CurDir%\\mpTrim\\mpTrim.exe" }, 0);

                    string left = REG.ReadValue("HKCU\\Software\\mpTrim", "MainFormLeft");
                    string top = REG.ReadValue("HKCU\\Software\\mpTrim", "MainFormTop");
                    if (!string.IsNullOrWhiteSpace(left) && left != Left.ToString() &&
                        !string.IsNullOrWhiteSpace(top) && top != Top.ToString())
                    {
                        INI.File(iniPath);
                        INI.Write("Settings", "Left", left);
                        INI.Write("Settings", "Top", top);
                    }
                    REG.RemoveExistSubKey("HKCU\\Software\\mpTrim");
                }
                else
                    RUN.App(new ProcessStartInfo() { FileName = "%CurDir%\\mpTrim\\mpTrim.exe" });
            }
        }
    }
}
