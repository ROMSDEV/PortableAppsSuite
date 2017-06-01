namespace mpTrimPortable
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows.Forms;
    using Microsoft.Win32;
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
                    var left = (int)Math.Round(Screen.PrimaryScreen.WorkingArea.Width / 2f - 335f / 2f);
                    Reg.Write("HKCU\\Software\\mpTrim", "MainFormLeft", left, RegistryValueKind.DWord);
                    var top = (int)Math.Round(Screen.PrimaryScreen.WorkingArea.Height / 2f - 410f / 2f);
                    Reg.Write("HKCU\\Software\\mpTrim", "MainFormTop", top, RegistryValueKind.DWord);
                    using (var p = ProcessEx.Start("%CurDir%\\mpTrim\\mpTrim.exe", false, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    Reg.RemoveSubKey("HKCU\\Software\\mpTrim");
                }
                else
                    ProcessEx.Start("%CurDir%\\mpTrim\\mpTrim.exe");
        }
    }
}
