namespace CheatEnginePortable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.Win32;
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
#if x86
                var curPath64 = PathEx.Combine(PathEx.LocalDir, "CheatEngine64Portable.exe");
                if (Environment.Is64BitOperatingSystem)
                {
                    if (!File.Exists(curPath64))
                        return;
                    ProcessEx.Start(curPath64, EnvironmentEx.CommandLine(false));
                    return;
                }
#endif

                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\CheatEngine");
                var appPath = PathEx.Combine(appDir, "Cheat Engine.exe");

                if (!newInstance)
                {
                    if (!File.Exists(appPath))
                        return;
                    ProcessEx.Start(appDir, true);
                    return;
                }

                var updaterPath = PathEx.Combine(appDir, "CheatEngineUpdater.exe");
                if (!File.Exists(updaterPath))
                    return;

                Helper.ApplicationStart(updaterPath, "/silent", null);
                if (!File.Exists(appPath))
                {
                    var updIniPath = Path.ChangeExtension(updaterPath, ".ini");
                    if (!string.IsNullOrEmpty(updIniPath) && File.Exists(updIniPath))
                        File.Delete(updIniPath);
                    return;
                }

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                var tablesDir = PathEx.Combine(dataDir, "My Cheat Tables");
                var dirMap = new Dictionary<string, string>
                {
                    {
                        "%MyDocuments%\\My Cheat Tables",
                        tablesDir
                    }
                };

                var regKeys = new[]
                {
                    "HKCU\\Software\\Cheat Engine"
                };

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);

                Helper.RegForwarding(Helper.Options.Start, regKeys);
                Reg.Write(regKeys.First(), "Initial tables dir", tablesDir, RegistryValueKind.String);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), Elevation.IsAdministrator);

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);

                Helper.RegForwarding(Helper.Options.Exit, regKeys);
            }
        }
    }
}
