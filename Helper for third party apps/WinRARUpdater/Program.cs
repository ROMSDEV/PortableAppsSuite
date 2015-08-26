﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WinRARUpdater
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            if (Process.GetProcessesByName("WinRAR").Length > 0)
            {
                MessageBox.Show("WinRAR must be closed.", new MainForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    SilDev.Log.AllowDebug();
                    if (!SilDev.Elevation.WritableLocation())
                        SilDev.Elevation.RestartAsAdministrator(Environment.CommandLine);
                    SilDev.Source.AddFile("aceed86b06a889a33d71e8f0e65735bf", "UnRAR.exe");
                    SilDev.Source.LoadAssemblies(Properties.Resources._UnRAR);
                    Thread.Sleep(200);
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
            }
        }
    }
}
