using SilDev;
using System;
using System.Diagnostics;
using System.IO;
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
                    LOG.AllowDebug();

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

                    string rootDir = PATH.Combine("%CurDir%\\TeamViewer");
                    string appPath = Path.Combine(rootDir, "TeamViewer.exe");
                    string updaterPath = Path.Combine(rootDir, "TeamViewerUpdater.exe");
                    if (!File.Exists(appPath) || !File.Exists(updaterPath))
                        return;

                    string CurrentDate = DateTime.Now.ToString("M/d/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                    string LastUpdateCheck = INI.Read("History", "LastUpdateCheck");
                    if (LastUpdateCheck != CurrentDate)
                    {
                        RUN.App(new ProcessStartInfo()
                        {
                            FileName = updaterPath,
                            Arguments = "/silent",
                            Verb = "runas"
                        }, 0);
                        INI.Write("History", "LastUpdateCheck", CurrentDate);
                    }

                    string iniPath = Path.Combine(rootDir, "TeamViewer.ini");
                    if (!File.Exists(iniPath))
                        File.Create(iniPath).Close();
                    INI.Write("Settings", "nosave", 1, iniPath);
                    INI.Write("Settings", "importsettings", 1, iniPath);

                    string tvIniPath = Path.Combine(rootDir, "tv.ini");
                    INI.File($"%CurDir%\\{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}.ini");
                    Version CurVer = Assembly.GetExecutingAssembly().GetName().Version;
                    Version LastVer = INI.ReadVersion("History", "LastVersion");
                    if (!File.Exists(tvIniPath) || CurVer != LastVer)
                    {
                        try
                        {
                            if (File.Exists(tvIniPath))
                                File.Delete(tvIniPath);
                            File.WriteAllText(tvIniPath, Properties.Resources._default_settings);
                            INI.Write("History", "LastVersion", CurVer);
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                    }

                    string defKey = $"SOFTWARE\\TeamViewer";
                    string bakKey = $"SOFTWARE\\SI13N7-BACKUP: TeamViewer";
                    if (!REG.ValueExist($"HKLM\\{defKey}", "Portable App"))
                        REG.RenameSubKey($"HKLM\\{defKey}", bakKey);
                    REG.WriteValue($"HKLM\\{defKey}", "Portable App", "True");

                    string defLogDir = PATH.Combine("%AppData%\\TeamViewer");
                    DATA.DirLink(defLogDir, rootDir, true);

                    RUN.App(new ProcessStartInfo()
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
                                    LOG.Debug(ex);
                                }
                            }
                            if (isRunning)
                                break;
                        }
                    }

                    REG.RemoveExistSubKey($"HKLM\\{defKey}");
                    REG.RenameSubKey($"HKLM\\{bakKey}", defKey);

                    DATA.DirUnLink(defLogDir, true);
                }
            }
        }
    }
}
