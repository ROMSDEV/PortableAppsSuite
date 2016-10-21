namespace LMMSPortable
{
    using System;
    using System.Diagnostics;
    using System.IO;
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
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
#if x86
                var appDir = PathEx.Combine("%CurDir%\\App\\lmms");
#else
                var appDir = PathEx.Combine("%CurDir%\\App\\lmms64");
#endif
                var appPath = Path.Combine(appDir, "lmms.exe");
                if (!File.Exists(appPath))
                    return;
                if (newInstance)
                {
                    var dataDir = PathEx.Combine("%CurDir%\\Data\\lmms");
                    if (!Directory.Exists(dataDir))
                    {
                        Directory.CreateDirectory(Path.Combine(dataDir, "presets"));
                        Directory.CreateDirectory(Path.Combine(dataDir, "projects"));
                        Directory.CreateDirectory(Path.Combine(dataDir, "samples"));
                    }
                    var defCfgPath = PathEx.Combine("%UserProfile%\\.lmmsrc.xml");
                    string bakCfgPath = $"{defCfgPath}.SI13N7-BACKUP";
                    var cfgPath = PathEx.Combine("%CurDir%\\Data\\.lmmsrc.xml");
                    if (File.Exists(defCfgPath))
                    {
                        File.Move(defCfgPath, bakCfgPath);
                        Data.SetAttributes(bakCfgPath, FileAttributes.Hidden);
                    }
                    if (!File.Exists(defCfgPath))
                    {
                        var cfgCon = File.Exists(cfgPath) ? File.ReadAllText(cfgPath) : Resources.DefaultConfig;
                        cfgCon = string.Format(cfgCon, appDir, Path.GetFullPath($"{dataDir}\\.."));
                        File.WriteAllText(defCfgPath, cfgCon);
                    }
                    if (File.Exists(defCfgPath))
                        Data.SetAttributes(defCfgPath, FileAttributes.Hidden);
                    using (var p = ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false), false, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    var isRunning = true;
                    while (isRunning)
                    {
                        var runningApp = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
                        isRunning = runningApp.Length > 0;
                        foreach (var app in runningApp)
                            app.WaitForExit();
                    }
                    if (File.Exists(defCfgPath))
                    {
                        if (File.Exists(cfgPath))
                            File.Delete(cfgPath);
                        var cfgCon = File.ReadAllText(defCfgPath);
                        cfgCon = cfgCon.Replace(appDir, "{0}").Replace(Path.GetFullPath($"{dataDir}\\.."), "{1}");
                        File.WriteAllText(cfgPath, cfgCon);
                    }
                    if (File.Exists(defCfgPath))
                        File.Delete(defCfgPath);
                    if (File.Exists(bakCfgPath))
                        File.Move(bakCfgPath, defCfgPath);
                    if (File.Exists(defCfgPath))
                        Data.SetAttributes(defCfgPath, FileAttributes.Normal);
                }
                else
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false));
            }
        }
    }
}
