using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace CCleanerUpdater
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            string CCleaner = Path.Combine(Application.StartupPath, "CCleaner.exe");
            if (!File.Exists(CCleaner))
            {
                MessageBox.Show("CCleaner not found.", new MainForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(-1);
            }
            if (Process.GetProcessesByName("CCleaner").Length > 0 || Process.GetProcessesByName("CCleaner64").Length > 0)
            {
                MessageBox.Show("CCleaner must be closed.", new MainForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(-1);
            }
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    SilDev.Log.AllowDebug();
                    if (!SilDev.Elevation.WritableLocation())
                        SilDev.Elevation.RestartAsAdministrator();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
            }
        }
    }
}
