namespace AppUpdater
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using SilDev.Forms;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            MessageBoxEx.TopMost = true;
            if (Process.GetProcessesByName(Resources.AppName).Length > 0)
            {
                MessageBoxEx.Show(Resources.Msg_Warn_00, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(1);
            }
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                if (!newInstance)
                    return;
                if (!Elevation.WritableLocation())
                    Elevation.RestartAsAdministrator();
                Ini.SetFile(Path.ChangeExtension(PathEx.LocalPath, ".ini"));
                if ((DateTime.Now - Ini.Read("History", "LastCheck", DateTime.MinValue)).Days < 1)
                    return;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}
