namespace CCleanerUpdater
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            var cCleaner = PathEx.Combine("%CurDir%\\CCleaner.exe");
            if (!File.Exists(cCleaner))
            {
                MessageBox.Show(@"CCleaner not found.", new MainForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(1);
            }
            if (Process.GetProcessesByName("CCleaner").Length > 0 || Process.GetProcessesByName("CCleaner64").Length > 0)
            {
                MessageBox.Show(@"CCleaner must be closed.", new MainForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(1);
            }
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (!newInstance)
                    return;
                if (!Elevation.WritableLocation())
                    Elevation.RestartAsAdministrator();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}
