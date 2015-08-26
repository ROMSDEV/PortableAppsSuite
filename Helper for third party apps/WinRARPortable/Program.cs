﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WinRARPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            if (SilDev.Elevation.IsAdministrator && Environment.CommandLine.EndsWith(".exe\" /ClearRegistryAsAdmin"))
            {
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.ClassesRoot, "WinRAR");
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.ClassesRoot, "WinRAR.REV");
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.ClassesRoot, "WinRAR.ZIP");
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR");
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR.REV");
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR.ZIP");
                SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, "Software\\WinRAR");
                return;
            }
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    string CurrentDate = DateTime.Now.ToString("M/d/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                    SilDev.Initialization.File(Application.StartupPath, Environment.Is64BitProcess ? "WinRAR64Portable.ini" : "WinRARPortable.ini");
                    string LastUpdateCheck = SilDev.Initialization.ReadValue("History", "LastUpdateCheck");
                    if (LastUpdateCheck != CurrentDate)
                    {
                        SilDev.Run.App(Application.StartupPath, Environment.Is64BitProcess ? "winrar-x64\\WinRARUpdater64.exe" : "winrar\\WinRARUpdater.exe", "/silent", SilDev.Run.WindowStyle.Normal, -1, 0);
                        SilDev.Initialization.WriteValue("History", "LastUpdateCheck", CurrentDate);
                    }
                    string ini = Path.Combine(Application.StartupPath, Environment.Is64BitProcess ? "winrar-x64\\WinRAR.ini" : "winrar\\WinRAR.ini");
                    if (!File.Exists(ini))
                        File.Create(ini).Close();
                    SilDev.Run.App(Application.StartupPath, Environment.Is64BitProcess ? "winrar-x64\\WinRAR.exe" : "winrar\\WinRAR.exe", Environment.CommandLine.Replace(string.Format("\"{0}\"", Application.ExecutablePath), string.Empty), 0);
                    bool isRunning = true;
                    while (isRunning)
                    {
                        Process[] runningApp = Process.GetProcessesByName("WinRAR");
                        isRunning = runningApp.Length > 0;
                        foreach (Process app in runningApp)
                            app.WaitForExit();
                    }
                    string appdataPath = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "WinRAR");
                    if (Directory.Exists(appdataPath))
                        Directory.Delete(appdataPath, true);
                    if (SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.ClassesRoot, "WinRAR") ||
                        SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.ClassesRoot, "WinRAR.REV") ||
                        SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.ClassesRoot, "WinRAR.ZIP") ||
                        SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR") ||
                        SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR.REV") ||
                        SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.CurrentUser, "Software\\Classes\\WinRAR.ZIP") ||
                        SilDev.Reg.SubKeyExist(SilDev.Reg.RegKey.CurrentUser, "Software\\WinRAR"))
                        SilDev.Elevation.RestartAsAdministrator("/ClearRegistryAsAdmin");
                }
                else
                    SilDev.Run.App(Application.StartupPath, Environment.Is64BitProcess ? "winrar-x64\\WinRAR.exe" : "winrar\\WinRAR.exe", Environment.CommandLine.Replace(string.Format("\"{0}\"", Application.ExecutablePath), string.Empty));
            }
        }
    }
}
