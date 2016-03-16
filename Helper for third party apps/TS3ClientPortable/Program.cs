using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace TS3ClientPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
#if x86
            string appPath = Path.Combine(Application.StartupPath, "TS3Client\\ts3client_win32.exe");
            string configPath = Path.Combine(Application.StartupPath, "TS3Client\\config");
            string iniName = "TS3ClientPortable.ini";
#else
            string appPath = Path.Combine(Application.StartupPath, "TS3Client64\\ts3client_win64.exe");
            string configPath = Path.Combine(Application.StartupPath, "TS3Client64\\config");
            string iniName = "TS3Client64Portable.ini";
#endif
            if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                Environment.Exit(-1);
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    SilDev.Log.AllowDebug();
                    SilDev.Ini.File(Application.StartupPath, iniName);
                    string waitForNetwork = SilDev.Ini.Read("Settings", "WaitForNetwork");
                    int ms;
                    if (int.TryParse(waitForNetwork, out ms))
                    {
                        try
                        {
                            if (ms > 0)
                            {
                                for (int i = 0; i < ms; i++)
                                {
                                    Thread.Sleep(1);
                                    if (SilDev.Network.InternetIsAvailable())
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SilDev.Log.Debug(ex);
                        }
                    }
                    if (!Directory.Exists(configPath))
                        Directory.CreateDirectory(configPath);
                    SilDev.Run.App(new ProcessStartInfo() { FileName = appPath, WindowStyle = ProcessWindowStyle.Normal }, 0, -1);
                    bool hideInTaskBar = SilDev.Ini.Read("Settings", "HideInTaskBar").ToLower() == "true";
                    Process[] runningProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
                    while (runningProcess.Length > 0)
                    {
                        foreach (Process p in runningProcess)
                        {
                            if (hideInTaskBar)
                            {
                                p.WaitForInputIdle();
                                if (!p.HasExited)
                                    p.WaitForExit(100);
                            }
                            else
                                p.WaitForExit();
                        }
                        IntPtr hWnd = SilDev.WinAPI.SafeNativeMethods.FindWindowByCaption(IntPtr.Zero, "TeamSpeak 3");
                        if (hWnd != IntPtr.Zero && hideInTaskBar)
                        {
                            SilDev.WinAPI.HideWindow(hWnd);
                            SilDev.WinAPI.SafeNativeMethods.SetWindowLong(hWnd, -0x14, SilDev.WinAPI.SafeNativeMethods.GetWindowLong(hWnd, -0x14) | 0x0080);
                            SilDev.WinAPI.ShowWindow(hWnd);
                            hideInTaskBar = false;
                        }
                        runningProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
                    }
                    SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\TeamSpeak 3 Client");
                    SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.LocalMachine, "SOFTWARE\\TeamSpeak 3 Client");
                    if (File.Exists(Path.Combine(Path.GetDirectoryName(appPath), "OverwolfTeamSpeakInstaller.exe")))
                        File.Delete(Path.Combine(Path.GetDirectoryName(appPath), "OverwolfTeamSpeakInstaller.exe"));
                }
            }
        }
    }
}
