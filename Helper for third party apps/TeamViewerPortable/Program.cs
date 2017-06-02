namespace TeamViewerPortable
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Properties;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
                if (newInstance)
                {
                    string[] pList =
                    {
                        "TeamViewer",
                        "TeamViewer_Desktop",
                        "tv_w32",
                        "tv_x64",
                        "TeamViewerUpdater"
                    };
                    if (pList.Any(p => Process.GetProcessesByName(p).Length > 0))
                        return;

                    var rootDir = PathEx.Combine("%CurDir%\\TeamViewer");
                    var appPath = Path.Combine(rootDir, "TeamViewer.exe");
                    var updaterPath = Path.Combine(rootDir, "TeamViewerUpdater.exe");
                    if (!File.Exists(appPath) || !File.Exists(updaterPath))
                        return;

                    var currentDate = DateTime.Now.ToString("M/d/yyyy", CultureInfo.CreateSpecificCulture("en-US"));
                    var lastUpdateCheck = Ini.Read("History", "LastUpdateCheck");
                    if (lastUpdateCheck != currentDate)
                    {
                        using (var p = ProcessEx.Start(updaterPath, "/silent", true, false))
                            if (!p?.HasExited == true)
                                p?.WaitForExit();
                        Ini.Write("History", "LastUpdateCheck", currentDate);
                    }

                    var iniPath = Path.Combine(rootDir, "TeamViewer.ini");
                    if (!File.Exists(iniPath))
                        File.Create(iniPath).Close();
                    Ini.Write("Settings", "nosave", 1, iniPath);
                    Ini.Write("Settings", "importsettings", 1, iniPath);

                    var tvIniPath = Path.Combine(rootDir, "tv.ini");
                    Ini.SetFile(Path.ChangeExtension(PathEx.LocalPath, ".ini"));
                    var curVer = Assembly.GetExecutingAssembly().GetName().Version;
                    var lastVer = Ini.Read("History", "LastVersion", Version.Parse("0.0.0.0"));
                    if (!File.Exists(tvIniPath) || curVer != lastVer)
                        try
                        {
                            if (File.Exists(tvIniPath))
                                File.Delete(tvIniPath);
                            File.WriteAllText(tvIniPath, Resources.DefaultSettings);
                            Ini.Write("History", "LastVersion", curVer);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }

                    const string defKey = "SOFTWARE\\TeamViewer";
                    const string bakKey = "SOFTWARE\\SI13N7-BACKUP: TeamViewer";
                    if (!Reg.EntryExists($"HKLM\\{defKey}", "Portable App"))
                        Reg.MoveSubKey($"HKLM\\{defKey}", bakKey);
                    Reg.EntryExists($"HKLM\\{defKey}", "Portable App", "True");

                    var defLogDir = PathEx.Combine("%AppData%\\TeamViewer");
                    Data.DirLink(defLogDir, rootDir, true);

                    using (var p = ProcessEx.Start(appPath, true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();

                    var isRunning = true;
                    while (isRunning)
                    {
                        isRunning = false;
                        foreach (var n in pList)
                        {
                            foreach (var p in Process.GetProcessesByName(n))
                                try
                                {
                                    isRunning = !p.HasExited;
                                    if (!isRunning)
                                        continue;
                                    p.WaitForExit();
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    Log.Write(ex);
                                }
                            if (isRunning)
                                break;
                        }
                    }

                    Reg.RemoveSubKey($"HKLM\\{defKey}");
                    Reg.MoveSubKey($"HKLM\\{bakKey}", defKey);

                    Data.DirUnLink(defLogDir, true);
                }
        }
    }
}
