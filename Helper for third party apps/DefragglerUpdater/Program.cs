using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace DefragglerUpdater
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            string Defraggler = Path.Combine(Application.StartupPath, "Defraggler.exe");
            if (!File.Exists(Defraggler))
            {
                MessageBox.Show("Defraggler not found.", new MainForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(-1);
            }
            if (Process.GetProcessesByName("Defraggler").Length > 0 || Process.GetProcessesByName("Defraggler64").Length > 0)
            {
                MessageBox.Show("Defraggler must be closed.", new MainForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(-1);
            }
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
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
