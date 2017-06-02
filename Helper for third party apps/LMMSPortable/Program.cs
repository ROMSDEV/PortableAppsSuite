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
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
#if x86
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\lmms");
#else
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\lmms64");
#endif
                var appPath = Path.Combine(appDir, "lmms.exe");
                if (!File.Exists(appPath))
                    return;
                if (newInstance)
                {
                    var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                    var profDir = PathEx.Combine(dataDir, "lmms");
                    if (!Directory.Exists(profDir))
                    {
                        Directory.CreateDirectory(Path.Combine(profDir, "presets"));
                        Directory.CreateDirectory(Path.Combine(profDir, "projects"));
                        Directory.CreateDirectory(Path.Combine(profDir, "samples\\gig"));
                        Directory.CreateDirectory(Path.Combine(profDir, "samples\\sf2"));
                    }
                    var defCfgPath = PathEx.Combine("%UserProfile%\\.lmmsrc.xml");
                    string bakCfgPath = $"{defCfgPath}.SI13N7-BACKUP";
                    var cfgPath = PathEx.Combine(dataDir, ".lmmsrc.xml");
                    if (File.Exists(defCfgPath))
                    {
                        File.Move(defCfgPath, bakCfgPath);
                        Data.SetAttributes(bakCfgPath, FileAttributes.Hidden);
                    }
                    if (!File.Exists(defCfgPath))
                    {
                        var cfgCon = File.Exists(cfgPath) ? File.ReadAllText(cfgPath) : Resources.DefaultConfig;
                        cfgCon = string.Format(cfgCon, appDir, dataDir, dataDir.Replace("\\", "/"));
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
                        try
                        {
                            var cfgCon = File.ReadAllText(defCfgPath);
                            cfgCon = cfgCon.Replace(appDir, "{0}")
                                           .Replace(dataDir, "{1}")
                                           .Replace(dataDir.Replace("\\", "/"), "{2}");
                            File.WriteAllText(cfgPath, cfgCon);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    }
                    try
                    {
                        if (File.Exists(defCfgPath))
                            File.Delete(defCfgPath);
                        if (File.Exists(bakCfgPath))
                            File.Move(bakCfgPath, defCfgPath);
                        if (File.Exists(defCfgPath))
                            Data.SetAttributes(defCfgPath, FileAttributes.Normal);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        ProcessEx.Send(string.Format(Resources.DelayedRestoration, defCfgPath, bakCfgPath));
                    }
                }
                else
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false));
            }
        }
    }
}
