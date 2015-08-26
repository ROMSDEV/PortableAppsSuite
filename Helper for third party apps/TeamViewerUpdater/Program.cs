using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace TeamViewerUpdater
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SilDev.Log.AllowDebug();
            string TeamViewer = Path.Combine(Application.StartupPath, "TeamViewer.exe");
            if (!File.Exists(TeamViewer))
            {
                MessageBox.Show("TeamViewer not found.", new MainForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string[] pList = new string[]
            {
                "TeamViewer",
                "TeamViewer_Desktop",
                "tv_w32",
                "tv_x64",
            };
            foreach (string p in pList)
            {
                if (Process.GetProcessesByName(p).Length > 0)
                {
                    MessageBox.Show("TeamViewer must be closed.", new MainForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
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
                    try
                    {
                        Application.Run(new MainForm());
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                }
            }
        }
    }
}
