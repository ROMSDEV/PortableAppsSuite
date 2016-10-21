namespace DefragglerUpdater
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
            var defraggler = PathEx.Combine("%CurDir%\\Defraggler.exe");
            if (!File.Exists(defraggler))
            {
                MessageBox.Show(@"Defraggler not found.", new MainForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(-1);
            }
            if (Process.GetProcessesByName("Defraggler").Length > 0 || Process.GetProcessesByName("Defraggler64").Length > 0)
            {
                MessageBox.Show(@"Defraggler must be closed.", new MainForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(-1);
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
