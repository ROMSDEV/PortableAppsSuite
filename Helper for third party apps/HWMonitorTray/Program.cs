namespace HwMonTray
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows.Forms;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            string processName;
            using (var current = Process.GetCurrentProcess())
                processName = current.ProcessName;
            bool newInstance;
            using (new Mutex(true, processName, out newInstance))
            {
                if (!newInstance)
                    return;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FrmHwmTray());
            }
        }
    }
}
