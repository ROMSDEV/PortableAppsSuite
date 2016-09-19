using SilDev;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WinSize2Portable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            LOG.AllowDebug();
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    try
                    {
                        string dataPath = PATH.Combine("%CurDir%\\Data");
                        if (!Directory.Exists(dataPath))
                        {
                            Directory.CreateDirectory(dataPath);
                            File.Create(Path.Combine(dataPath, "WinSize2.INI")).Close();
                            File.Create(Path.Combine(dataPath, "WinSize2_FullScreen.INI")).Close();
                        }
                        string appdataPath = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "MagraSoft");
                        INI.File(appdataPath, "WinSize2_Root.INI");
                        INI.Write("Settings", "INIWithScreenSize", 0);
                        INI.Write("Settings", "INIFile_Path", dataPath);
                        RUN.App(new ProcessStartInfo() { FileName = "%CurDir%\\App\\WinSize2\\_WinSize2_Launcher.exe" }, 0);
                        bool isRunning = true;
                        while (isRunning)
                        {
                            isRunning = Process.GetProcessesByName("WinSize2").Length > 0;
                            foreach (Process p in Process.GetProcessesByName("WinSize2"))
                                p.WaitForExit();
                        }
                        Directory.Delete(appdataPath, true);
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }
                }
            }
        }
    }
}
