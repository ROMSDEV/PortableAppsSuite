namespace TeamViewerUpdater
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            var teamViewer = PathEx.Combine("%CurDir%\\TeamViewer.exe");
            if (!File.Exists(teamViewer))
            {
                MessageBox.Show(@"TeamViewer not found.", @"TeamViewer Updater", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string[] pList =
            {
                "TeamViewer",
                "TeamViewer_Desktop",
                "tv_w32",
                "tv_x64"
            };
            if (pList.Any(p => Process.GetProcessesByName(p).Length > 0))
            {
                MessageBox.Show(@"TeamViewer must be closed.", @"TeamViewer Updater", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
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
                try
                {
                    Application.Run(new MainForm());
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }
    }
}
