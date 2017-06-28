namespace Cinebench64Portable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Portable;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            using (new Mutex(true, ProcessEx.CurrentName, out bool newInstance))
            {
                if (!newInstance)
                    return;

                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\cinebench64");
                var appPath = PathEx.Combine(appDir, "CINEBENCH Windows 64 Bit.exe");
                var updaterPath = Path.Combine(appDir, "CinebenchUpdater64.exe");
                if (ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)) || !File.Exists(updaterPath) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(updaterPath)))
                    return;

                var appDataDir = PathEx.Combine("%AppData%\\MAXON");
                var dataDir = PathEx.Combine("%CurDir%\\Data");

                RemoveLongPaths(appDir);
                RemoveLongPaths(appDataDir);
                RemoveLongPaths(dataDir);

                var dirMap = new Dictionary<string, string>
                {
                    {
                        appDataDir,
                        dataDir
                    },
                    {
                        PathEx.Combine(appDir, "cb_ranking"),
                        PathEx.Combine(dataDir, "cb_ranking")
                    }
                };

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);

                Helper.ApplicationStart(updaterPath, "/silent", null);
                if (!File.Exists(appPath))
                {
                    var updIniPath = Path.ChangeExtension(updaterPath, ".ini");
                    if (!string.IsNullOrEmpty(updIniPath) && File.Exists(updIniPath))
                        File.Delete(updIniPath);

                    RemoveLongPaths(appDir);
                    RemoveLongPaths(appDataDir);
                    RemoveLongPaths(dataDir);

                    Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);
                    return;
                }

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                RemoveLongPaths(appDir);
                RemoveLongPaths(appDataDir);
                RemoveLongPaths(dataDir);

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);
            }
        }

        /****************************************************************************************
         
            Cinebench creates ultra long paths in some cases... This causes a lot of critical
            issues in which the users are not able to access these files without any tricks.

        ****************************************************************************************/

        private static void RemoveLongPaths(string path)
        {
            if (!Directory.Exists(path))
                return;
            try
            {
                foreach (var dir in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
                    try
                    {
                        if (dir.Length >= 248)
                            throw new PathTooLongException();
                        if (Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly).Any(x => x.Length >= 260))
                            throw new PathTooLongException();
                    }
                    catch
                    {
                        Data.ForceDelete(dir);
                    }
            }
            catch
            {
                if (!path.EqualsEx(PathEx.LocalDir))
                    Data.ForceDelete(path);
            }
        }
    }
}
