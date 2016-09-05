using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ProcessRun
{
    static class Program
    {
        static string file = Path.Combine(Application.StartupPath, "WinSize2.EXE");

        [STAThread]
        static void Main(string[] args)
        {
            if (File.Exists(file) && args.Length > 0)
            {
                try
                {
                    if (Process.GetProcessesByName("WinSize2").Length > 0)
                        foreach (Process p in Process.GetProcessesByName("WinSize2"))
                            p.Kill();

                    foreach (string arg in args)
                        foreach (Process p in Process.GetProcessesByName(arg))
                            p.WaitForExit();
                }
                catch
                {
                    // DO NOTHING
                }
                finally
                {
                    run();
                }
            }
        }

        static void run()
        {
            if (Process.GetProcessesByName("WinSize2").Length <= 0)
            {
                Process newProcess = new Process();
                newProcess.StartInfo.FileName = file;
                newProcess.StartInfo.WorkingDirectory = Application.StartupPath;
                newProcess.Start();
            }
        }
    }
}
