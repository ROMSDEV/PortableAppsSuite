using SilDev;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace LMMSPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
#if x86
                string appDir = PATH.Combine("%CurDir%\\App\\lmms");
#else
                string appDir = PATH.Combine("%CurDir%\\App\\lmms64");
#endif
                string appPath = Path.Combine(appDir, "lmms.exe");
                if (!File.Exists(appPath))
                    return;
                if (newInstance)
                {
                    string dataDir = PATH.Combine("%CurDir%\\Data\\lmms");
                    if (!Directory.Exists(dataDir))
                    {
                        Directory.CreateDirectory(Path.Combine(dataDir, "presets"));
                        Directory.CreateDirectory(Path.Combine(dataDir, "projects"));
                        Directory.CreateDirectory(Path.Combine(dataDir, "samples"));
                    }
                    string defCfgPath = PATH.Combine("%UserProfile%\\.lmmsrc.xml");
                    string bakCfgPath = $"{defCfgPath}.SI13N7-BACKUP";
                    string cfgPath = PATH.Combine("%CurDir%\\Data\\.lmmsrc.xml");
                    if (File.Exists(defCfgPath))
                    {
                        File.Move(defCfgPath, bakCfgPath);
                        DATA.SetAttributes(bakCfgPath, FileAttributes.Hidden);
                    }
                    if (!File.Exists(defCfgPath))
                    {
                        string cfgCon = File.Exists(cfgPath) ? File.ReadAllText(cfgPath) : Properties.Resources.lmms_cfg_dummy;
                        cfgCon = string.Format(cfgCon, appDir, Path.GetFullPath($"{dataDir}\\.."));
                        File.WriteAllText(defCfgPath, cfgCon);
                    }
                    if (File.Exists(defCfgPath))
                        DATA.SetAttributes(defCfgPath, FileAttributes.Hidden);
                    RUN.App(new ProcessStartInfo()
                    {
                        Arguments = RUN.CommandLine(false),
                        FileName = appPath
                    }, 0);
                    bool isRunning = true;
                    while (isRunning)
                    {
                        Process[] runningApp = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
                        isRunning = runningApp.Length > 0;
                        foreach (Process app in runningApp)
                            app.WaitForExit();
                    }
                    if (File.Exists(defCfgPath))
                    {
                        if (File.Exists(cfgPath))
                            File.Delete(cfgPath);
                        string cfgCon = File.ReadAllText(defCfgPath);
                        cfgCon = cfgCon.Replace(appDir, "{0}").Replace(Path.GetFullPath($"{dataDir}\\.."), "{1}");
                        File.WriteAllText(cfgPath, cfgCon);
                    }
                    if (File.Exists(defCfgPath))
                        File.Delete(defCfgPath);
                    if (File.Exists(bakCfgPath))
                        File.Move(bakCfgPath, defCfgPath);
                    if (File.Exists(defCfgPath))
                        DATA.SetAttributes(defCfgPath, FileAttributes.Normal);
                }
                else
                    RUN.App(new ProcessStartInfo()
                    {
                        Arguments = RUN.CommandLine(false),
                        FileName = appPath
                    });
            }
        }
    }
}
