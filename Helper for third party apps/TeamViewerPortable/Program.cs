using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows.Forms;

namespace TeamViewerPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    SilDev.Log.AllowDebug();
                    string[] pList = new string[]
                    {
                        "TeamViewer",
                        "TeamViewer_Desktop",
                        "tv_w32",
                        "tv_x64",
                        "TeamViewerUpdater"
                    };
                    foreach (string p in pList)
                        if (Process.GetProcessesByName(p).Length > 0)
                            return;
                    string rootDir = Path.Combine(Application.StartupPath, "TeamViewer");
                    string appPath = Path.Combine(rootDir, "TeamViewer.exe");
                    string updaterPath = Path.Combine(rootDir, "TeamViewerUpdater.exe");
                    if (!File.Exists(appPath) || !File.Exists(updaterPath))
                        return;
                    string iniPath = Path.Combine(rootDir, "TeamViewer.ini");
                    if (!File.Exists(iniPath))
                        File.Create(iniPath).Close();
                    SilDev.Initialization.WriteValue("Settings", "nosave", 1, iniPath);
                    SilDev.Initialization.WriteValue("Settings", "importsettings", 1, iniPath);
                    string tvIniPath = Path.Combine(rootDir, "tv.ini");
                    if (!File.Exists(tvIniPath))
                    {
                        string archivePath = Path.Combine(Application.StartupPath, "temp.zip");
                        SilDev.Resource.ExtractConvert(Properties.Resources._default_settings, archivePath);
                        if (File.Exists(archivePath))
                        {
                            try
                            {
                                using (ZipArchive zip = ZipFile.OpenRead(archivePath))
                                {
                                    zip.ExtractToDirectory(rootDir);
                                    zip.Dispose();
                                }
                                File.Delete(archivePath);
                            }
                            catch (Exception ex)
                            {
                                SilDev.Log.Debug(ex);
                            }
                        }
                    }
                    SilDev.Initialization.File(Application.StartupPath, "TeamViewerPortable.ini");
                    string CurrentDate = DateTime.Now.ToString("M/d/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                    string LastUpdateCheck = SilDev.Initialization.ReadValue("History", "LastUpdateCheck");
                    if (LastUpdateCheck != CurrentDate)
                    {
                        SilDev.Run.App(new ProcessStartInfo() { FileName = updaterPath, Arguments = "/silent", Verb = "runas" }, 0);
                        SilDev.Initialization.WriteValue("History", "LastUpdateCheck", CurrentDate);
                    }
                    SilDev.Run.App(new ProcessStartInfo() { FileName = appPath, Verb = "runas" }, 0);
                }
            }
        }
    }
}
