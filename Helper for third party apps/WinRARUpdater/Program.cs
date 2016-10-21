namespace WinRARUpdater
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using Properties;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            if (Process.GetProcessesByName("WinRAR").Length > 0)
            {
                MessageBox.Show(@"WinRAR must be closed.", new MainForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (!newInstance)
                    return;

                if (!Elevation.WritableLocation())
                    Elevation.RestartAsAdministrator(Environment.CommandLine);

                var tmpDir = PathEx.Combine("%TEMP%", Process.GetCurrentProcess().ProcessName);
                var tmpZip = Path.Combine(tmpDir, "unrar.zip");
                ResourcesEx.Extract(Resources._UnRAR, tmpZip, true);
                Compaction.Unzip(tmpZip, tmpDir);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}
