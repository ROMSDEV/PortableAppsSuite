namespace DiscordPortable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
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
                if (!newInstance)
                    return;

                var defaultKeys = new[]
                {
                    "HKCR\\Discord"
                };

                var backupKeys = new[]
                {
                    "HKCR\\SI13N7-BACKUP: Discord"
                };

                if (Elevation.IsAdministrator && Environment.CommandLine.ContainsEx("{E429830E-A5A1-4FA0-9D5F-B21964F4698E}"))
                    goto ClearRegistry;

                for (var i = 0; i < defaultKeys.Length; i++)
                    if (!Reg.EntryExists(defaultKeys[i], "Portable App"))
                        Reg.MoveSubKey(defaultKeys[i], backupKeys[i]);

                if (!Elevation.IsAdministrator && defaultKeys.Any(Reg.SubKeyExists))
                {
                    Elevation.RestartAsAdministrator();
                    return;
                }

                var defAppDir = PathEx.Combine("%LocalAppData%\\Discord");
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\Discord");
                Data.DirLink(defAppDir, appDir, true);

                var defTmpDir = PathEx.Combine("%LocalAppData%\\SquirrelTemp");
                var tmpDir = Path.Combine(appDir, "packages\\SquirrelTemp");
                Data.DirLink(defTmpDir, tmpDir, true);

                var defDataDir = PathEx.Combine("%AppData%\\discord");
                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data\\AppData");
                Data.DirLink(defDataDir, dataDir, true);

                var regPath = PathEx.Combine(PathEx.LocalDir, "Data\\settings.reg");
                Reg.ImportFile(regPath);

                var appPath = Path.Combine(appDir, "Update.exe");
                using (var p = ProcessEx.Start(appPath, "--processStart Discord.exe", false, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();

                var fList = new List<string>();
                foreach (var d in Directory.GetDirectories(appDir, "app-*", SearchOption.TopDirectoryOnly))
                    fList.AddRange(Directory.GetFiles(d, "*.exe", SearchOption.TopDirectoryOnly));
                for (var i = 0; i < 10; i++)
                {
                    var isRunning = true;
                    while (isRunning)
                    {
                        foreach (var f in fList)
                        {
                            isRunning = ProcessEx.IsRunning(f);
                            if (!isRunning)
                                continue;
                            i = 0;
                            break;
                        }
                        Thread.Sleep(200);
                    }
                    Thread.Sleep(250);
                }

                Data.DirUnLink(defAppDir, true);
                Data.DirUnLink(defTmpDir, true);
                Data.DirUnLink(defDataDir, true);

                if (defaultKeys.Any(Reg.SubKeyExists))
                    Reg.ExportKeys(regPath, defaultKeys);

                ClearRegistry:

                foreach (var key in defaultKeys)
                    Reg.RemoveSubKey(key);

                for (var i = 0; i < backupKeys.Length; i++)
                    Reg.MoveSubKey(backupKeys[i], defaultKeys[i]);

                if (!Elevation.IsAdministrator && defaultKeys.Any(Reg.SubKeyExists))
                    Elevation.RestartAsAdministrator("{E429830E-A5A1-4FA0-9D5F-B21964F4698E}");
            }
        }
    }
}
