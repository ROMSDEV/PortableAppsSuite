using SilDev;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace RunPHP
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            LOG.AllowDebug();
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    if (!ELEVATION.WritableLocation())
                        ELEVATION.RestartAsAdministrator();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    try
                    {
                        Application.Run(new MainForm());
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug(ex);
                    }
                }
            }
        }
    }
}
