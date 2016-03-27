using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ProcessRun
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            string file = Path.Combine(Application.StartupPath, "WinSize2.EXE");
            if (File.Exists(file))
            {
                try
                {
                    for (var i = 0; i < 100; i++)
                    {
                        if (Process.GetProcessesByName("WinSize2").Length == 0)
                        {
                            if (i > 0)
                                i = 0;
                            SilDev.Run.App(new ProcessStartInfo() { FileName = Path.Combine(SilDev.Run.EnvironmentVariableFilter("%CurrentDir%"), file) }, 100);
                        }
                        Thread.Sleep(100);
                        foreach (Process p in Process.GetProcessesByName("WinSize2"))
                        {
                            if (p.MainWindowHandle == IntPtr.Zero)
                                continue;
                            StringBuilder title = new StringBuilder(32);
                            if (SilDev.WinAPI.SafeNativeMethods.SendMessageTimeoutText(p.MainWindowHandle, 0xd, 32, title, 0x2, 200, 0) > 0)
                            {
                                if (title.ToString().ToLower().Contains("winsize2"))
                                {
                                    p.CloseMainWindow();
                                    p.Close();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SilDev.Log.Debug(ex);
                }
            }
        }
    }
}
