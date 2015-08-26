using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ProcessRun
{
    static class Program
    {
        [DllImport("user32.dll", EntryPoint = "SendMessageTimeout", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint SendMessageTimeoutText(IntPtr hWnd, int Msg, int countOfChars, StringBuilder wndTitle, uint flags, uint uTImeoutj, uint result);

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
                        if (Process.GetProcessesByName("WinSize2").Length <= 0)
                        {
                            if (i > 0)
                                i = 0;

                            Process newProcess = new Process();
                            newProcess.StartInfo.FileName = file;
                            newProcess.StartInfo.WorkingDirectory = Application.StartupPath;
                            newProcess.Start();
                            Thread.Sleep(100);
                        }
                        else
                            Thread.Sleep(100);

                        foreach (Process p in Process.GetProcessesByName("WinSize2"))
                        {
                            var title = new StringBuilder(32);
                            if (p.MainWindowTitle.Length > 0 || SendMessageTimeoutText(p.MainWindowHandle, 0xd, 32, title, 0x2, 200, 0) > 0)
                            {
                                if (p.MainWindowTitle.Contains("WinSize2") || title.ToString().Contains("WinSize2"))
                                {
                                    p.CloseMainWindow();
                                    p.Close();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // DO NOTHING
                }
            }
        }
    }
}
