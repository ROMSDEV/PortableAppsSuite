namespace RunPHP
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows.Forms;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
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
