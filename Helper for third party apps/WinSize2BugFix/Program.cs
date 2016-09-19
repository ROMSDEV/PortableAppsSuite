using SilDev;
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
            string file = PATH.Combine("%CurDir%\\WinSize2.EXE");
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
                            RUN.App(new ProcessStartInfo() { FileName = PATH.Combine("%CurDir%", file) }, 100);
                        }
                        Thread.Sleep(100);
                        foreach (Process p in Process.GetProcessesByName("WinSize2"))
                        {
                            if (p.MainWindowHandle == IntPtr.Zero)
                                continue;
                            StringBuilder title = new StringBuilder(32);
                            IntPtr result = IntPtr.Zero;
                            if ((int)WINAPI.SafeNativeMethods.SendMessageTimeoutText(p.MainWindowHandle, 0xd, (UIntPtr)32, title, 0x2, 200, out result) > 0)
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
                    LOG.Debug(ex);
                }
            }
        }
    }
}
