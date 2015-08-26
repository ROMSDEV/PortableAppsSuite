using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
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
            string clientPath = Path.Combine(Application.StartupPath, "TS3Client\\ts3client_win32.exe");
            string configPath = Path.Combine(Application.StartupPath, "TS3Client\\config");
            string iniName = "TS3ClientPortable.ini";
#else
            string clientPath = Path.Combine(Application.StartupPath, "TS3Client64\\ts3client_win64.exe");
            string configPath = Path.Combine(Application.StartupPath, "TS3Client64\\config");
            string iniName = "TS3Client64Portable.ini";
#endif
            if (!File.Exists(clientPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(clientPath)).Length > 0)
                Environment.Exit(-1);
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    SilDev.Log.AllowDebug();
                    SilDev.Initialization.File(Application.StartupPath, iniName);
                    string waitForNetwork = SilDev.Initialization.ReadValue("Settings", "WaitForNetwork");
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
                                    if (NetworkInterface.GetIsNetworkAvailable())
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SilDev.Log.Debug(ex.Message, "TS3ClientPortable.Program - Connection check");
                        }
                    }
                    if (!Directory.Exists(configPath))
                        Directory.CreateDirectory(configPath);
                    SilDev.Run.WindowStyle winStyle = SilDev.Initialization.ReadValue("Settings", "WinState").ToLower().StartsWith("min") ? SilDev.Run.WindowStyle.Minimized : SilDev.Run.WindowStyle.Normal;
                    SilDev.Run.App(clientPath, false, winStyle, 0, -1);
                    bool hideInTaskBar = SilDev.Initialization.ReadValue("Settings", "HideInTaskBar").ToLower() == "true" ? false : true;
                    Process[] runningProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(clientPath));
                    while (runningProcess.Length > 0)
                    {
                        foreach (Process p in runningProcess)
                        {
                            if (!hideInTaskBar)
                                p.WaitForExit(200);
                            else
                                p.WaitForExit();
                        }
                        if (!hideInTaskBar)
                        {
                            IntPtr hWnd = SilDev.WinAPI.FindWindowByCaption(IntPtr.Zero, "TeamSpeak 3");
                            if (hWnd != IntPtr.Zero)
                            {
                                hideInTaskBar = true;
                                SilDev.WinAPI.HideWindow(hWnd);
                                SilDev.WinAPI.SetWindowLong(hWnd, -0x14, SilDev.WinAPI.GetWindowLong(hWnd, -0x14) | 0x0080);
                                SilDev.WinAPI.ShowWindow(hWnd);
                                if (winStyle == SilDev.Run.WindowStyle.Minimized)
                                    SilDev.WinAPI.ShowWindow(hWnd, SilDev.WinAPI.Win32HookAction.WS_MINIMIZE);
                            }
                        }
                        runningProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(clientPath));
                    }
                    SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\TeamSpeak 3 Client");
                    SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.LocalMachine, "SOFTWARE\\TeamSpeak 3 Client");
                    if (File.Exists(Path.Combine(Path.GetDirectoryName(clientPath), "OverwolfTeamSpeakInstaller.exe")))
                        File.Delete(Path.Combine(Path.GetDirectoryName(clientPath), "OverwolfTeamSpeakInstaller.exe"));
                }
            }
        }
    }
}
