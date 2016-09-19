using SilDev;
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
            string appPath = PATH.Combine("%CurDir%\\TS3Client\\ts3client_win32.exe");
            string configPath = PATH.Combine("%CurDir%\\TS3Client\\config");
            string iniName = "TS3ClientPortable.ini";
#else
            string appPath = PATH.Combine("%CurDir%\\TS3Client64\\ts3client_win64.exe");
            string configPath = PATH.Combine("%CurDir%\\TS3Client64\\config");
            string iniName = "TS3Client64Portable.ini";
#endif
            if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                Environment.Exit(-1);
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    LOG.AllowDebug();
                    INI.File(PATH.GetEnvironmentVariableValue("CurDir"), iniName);
                    string waitForNetwork = INI.Read("Settings", "WaitForNetwork");
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
                                    if (NET.InternetIsAvailable())
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                    }
                    if (!Directory.Exists(configPath))
                        Directory.CreateDirectory(configPath);
                    RUN.App(new ProcessStartInfo() { FileName = appPath, WindowStyle = ProcessWindowStyle.Normal }, 0, null);
                    bool hideInTaskBar = INI.Read("Settings", "HideInTaskBar").ToLower() == "true";
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
                        IntPtr hWnd = WINAPI.SafeNativeMethods.FindWindowByCaption(IntPtr.Zero, "TeamSpeak 3");
                        if (hWnd != IntPtr.Zero && hideInTaskBar)
                        {
                            WINAPI.HideWindow(hWnd);
                            WINAPI.SafeNativeMethods.SetWindowLong(hWnd, -0x14, WINAPI.SafeNativeMethods.GetWindowLong(hWnd, -0x14) | 0x0080);
                            WINAPI.ShowWindow(hWnd);
                            hideInTaskBar = false;
                        }
                        runningProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
                    }
                    REG.RemoveExistSubKey(REG.RegKey.CurrentUser, "Software\\TeamSpeak 3 Client");
                    REG.RemoveExistSubKey(REG.RegKey.LocalMachine, "SOFTWARE\\TeamSpeak 3 Client");
                    if (File.Exists(Path.Combine(Path.GetDirectoryName(appPath), "OverwolfTeamSpeakInstaller.exe")))
                        File.Delete(Path.Combine(Path.GetDirectoryName(appPath), "OverwolfTeamSpeakInstaller.exe"));
                }
            }
        }
    }
}
