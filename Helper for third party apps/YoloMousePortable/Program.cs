namespace YoloMousePortable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Portable;
    using Properties;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();

#if x86
            var curPath64 = PathEx.Combine(PathEx.LocalDir, "YoloMouse64Portable.exe");
            if (Environment.Is64BitOperatingSystem && File.Exists(curPath64))
            {
                ProcessEx.Start(curPath64, EnvironmentEx.CommandLine(false));
                return;
            }
            var appDir = PathEx.Combine(PathEx.LocalDir, "App\\YoloMouse");
            var updaterPath = PathEx.Combine(appDir, "YoloMouseUpdater.exe");
#else
            var appDir = PathEx.Combine(PathEx.LocalDir, "App\\YoloMouse64");
            var updaterPath = PathEx.Combine(appDir, "YoloMouseUpdater64.exe");
#endif

            using (new Mutex(true, ProcessEx.CurrentName, out bool newInstance))
            {
                if (!newInstance)
                    return;

                var appPath = PathEx.Combine(appDir, "YoloMouse.exe");
                if (ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)) || !File.Exists(updaterPath) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(updaterPath)))
                    return;

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");

                var defCursorDir = PathEx.Combine(appDir, "Cursors");
                var cursorDir = PathEx.Combine(dataDir, "Cursors");
                var dirMap = new Dictionary<string, string>
                {
                    {
                        "%LocalAppData%\\YoloMouse",
                        dataDir
                    },
                    {
                        defCursorDir,
                        cursorDir
                    }
                };

                Helper.ApplicationStart(updaterPath, "/silent", null);
                if (!File.Exists(appPath))
                {
                    var updIniPath = Path.ChangeExtension(updaterPath, ".ini");
                    if (!string.IsNullOrEmpty(updIniPath) && File.Exists(updIniPath))
                        File.Delete(updIniPath);
                    return;
                }

                var cfgPath = Path.Combine(dataDir, "Settings.ini");
                if (!File.Exists(cfgPath))
                {
                    if (!Directory.Exists(dataDir))
                        Directory.CreateDirectory(dataDir);
                    File.WriteAllText(cfgPath, Resources.DefaultSettings);
                }

                if (Directory.Exists(defCursorDir))
                {
                    Data.DirCopy(defCursorDir, cursorDir, true, true);
                    try
                    {
                        Directory.Delete(defCursorDir);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        ProcessEx.SendHelper.WaitForExitThenDelete(defCursorDir, ProcessEx.CurrentName, Elevation.IsAdministrator);
                    }
                }

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);
            }
        }
    }
}
