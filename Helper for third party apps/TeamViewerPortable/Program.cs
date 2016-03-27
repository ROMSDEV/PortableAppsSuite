using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
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
                    SilDev.Ini.Write("Settings", "nosave", 1, iniPath);
                    SilDev.Ini.Write("Settings", "importsettings", 1, iniPath);

                    string tvIniPath = Path.Combine(rootDir, "tv.ini");
                    SilDev.Ini.File(Application.StartupPath, "TeamViewerPortable.ini");
                    Version CurVer = Assembly.GetExecutingAssembly().GetName().Version;
                    Version LastVer = Version.Parse("1.0.0.0");
                    if (!File.Exists(tvIniPath) || !Version.TryParse(SilDev.Ini.Read("History", "LastPortableVersion"), out LastVer) || CurVer != LastVer)
                    {
                        try
                        {
                            if (File.Exists(tvIniPath))
                                File.Delete(tvIniPath);
                            string archivePath = Path.Combine(Application.StartupPath, "temp.zip");
                            SilDev.Resource.ExtractConvert(Properties.Resources._default_settings, archivePath);
                            if (File.Exists(archivePath))
                            {
                                try
                                {
                                    using (ZipArchive zip = ZipFile.OpenRead(archivePath))
                                        zip.ExtractToDirectory(rootDir);
                                    File.Delete(archivePath);
                                    SilDev.Ini.Write("History", "LastVersion", CurVer);
                                }
                                catch (Exception ex)
                                {
                                    SilDev.Log.Debug(ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SilDev.Log.Debug(ex);
                        }
                    }

                    string CurrentDate = DateTime.Now.ToString("M/d/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                    string LastUpdateCheck = SilDev.Ini.Read("History", "LastUpdateCheck");
                    if (LastUpdateCheck != CurrentDate)
                    {
                        SilDev.Run.App(new ProcessStartInfo()
                        {
                            FileName = updaterPath,
                            Arguments = "/silent",
                            Verb = "runas"
                        }, 0);
                        SilDev.Ini.Write("History", "LastUpdateCheck", CurrentDate);
                    }

                    string defKey = $"SOFTWARE\\TeamViewer";
                    string bakKey = $"SOFTWARE\\SI13N7-BACKUP: TeamViewer";
                    if (!SilDev.Reg.ValueExist($"HKLM\\{defKey}", "Portable App"))
                        SilDev.Reg.RenameSubKey($"HKLM\\{defKey}", bakKey);
                    SilDev.Reg.WriteValue($"HKLM\\{defKey}", "Portable App", "True");

                    string defLogDir = SilDev.Run.EnvironmentVariableFilter("%AppData%\\TeamViewer");
                    SilDev.Data.DirLink(defLogDir, rootDir, true);

                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        FileName = appPath,
                        Verb = "runas"
                    }, 0);

                    bool isRunning = true;
                    while (isRunning)
                    {
                        isRunning = false;
                        foreach (string n in pList)
                        {
                            foreach (Process p in Process.GetProcessesByName(n))
                            {
                                try
                                {
                                    isRunning = !p.HasExited;
                                    if (isRunning)
                                    {
                                        p.WaitForExit();
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    SilDev.Log.Debug(ex);
                                }
                            }
                            if (isRunning)
                                break;
                        }
                    }

                    SilDev.Reg.RemoveExistSubKey($"HKLM\\{defKey}");
                    SilDev.Reg.RenameSubKey($"HKLM\\{bakKey}", defKey);

                    SilDev.Data.DirUnLink(defLogDir, true);
                }
            }
        }
    }
}
