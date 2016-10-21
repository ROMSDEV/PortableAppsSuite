namespace WinSize2Portable
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (!newInstance)
                    return;
                try
                {
                    var dataPath = PathEx.Combine("%CurDir%\\Data");
                    if (!Directory.Exists(dataPath))
                    {
                        Directory.CreateDirectory(dataPath);
                        File.Create(Path.Combine(dataPath, "WinSize2.INI")).Close();
                        File.Create(Path.Combine(dataPath, "WinSize2_FullScreen.INI")).Close();
                    }
                    var appdataPath = Path.Combine("%AppData%\\MagraSoft");
                    Ini.File(appdataPath, "WinSize2_Root.INI");
                    Ini.Write("Settings", "INIWithScreenSize", 0);
                    Ini.Write("Settings", "INIFile_Path", dataPath);
                    using (var p = ProcessEx.Start("%CurDir%\\App\\WinSize2\\_WinSize2_Launcher.exe", false, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    var isRunning = true;
                    while (isRunning)
                    {
                        isRunning = Process.GetProcessesByName("WinSize2").Length > 0;
                        foreach (var p in Process.GetProcessesByName("WinSize2"))
                            p.WaitForExit();
                    }
                    Directory.Delete(appdataPath, true);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }
    }
}
