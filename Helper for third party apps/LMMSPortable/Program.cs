namespace LMMSPortable
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
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
#if x86
                var curPath64 = PathEx.Combine(PathEx.LocalDir, "LMMS64Portable.exe");
                if (Environment.Is64BitOperatingSystem && File.Exists(curPath64))
                {
                    ProcessEx.Start(curPath64, EnvironmentEx.CommandLine(false));
                    return;
                }
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\lmms");
                var updPath = PathEx.Combine(appDir, "LMMSUpdater.exe");
#else
                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\lmms64");
                var updPath = PathEx.Combine(appDir, "LMMSUpdater64.exe");
#endif
                var altAppDir = PathEx.AltCombine(appDir);
                var appPath = Path.Combine(appDir, "lmms.exe");
                if (ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)) || !File.Exists(updPath) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(updPath)))
                    return;

                if (!newInstance)
                {
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine(false));
                    return;
                }

                Helper.ApplicationStart(updPath, "/silent", null);
                if (!File.Exists(appPath))
                    return;

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                var altDataDir = PathEx.AltCombine(dataDir);
                var dataDirs = new[]
                {
                    Path.Combine(dataDir, "lmms\\presets"),
                    Path.Combine(dataDir, "lmms\\projects"),
                    Path.Combine(dataDir, "lmms\\samples\\gig"),
                    Path.Combine(dataDir, "lmms\\samples\\sf2")
                };
                foreach (var dir in dataDirs)
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                var configPath = PathEx.Combine(dataDir, ".lmmsrc.xml");
                var fileMap = new Dictionary<string, string>
                {
                    {
                        "%UserProfile%\\.lmmsrc.xml",
                        configPath
                    }
                };
                var config = string.Empty;
                if (File.Exists(configPath))
                    config = File.ReadAllText(configPath);
                if (!config.ContainsEx("{0}", "{1}", "{2}", "{3}"))
                    config = Resources.DefaultConfig;
                config = string.Format(config, appDir, dataDir, altDataDir, altAppDir);
                File.WriteAllText(configPath, config);
                Helper.FileForwarding(Helper.Options.Start, fileMap);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.FileForwarding(Helper.Options.Exit, fileMap);
                if (!File.Exists(configPath))
                    return;
                config = File.ReadAllText(configPath);
                config = config.Replace(appDir, "{0}")
                               .Replace(dataDir, "{1}")
                               .Replace(altDataDir, "{2}")
                               .Replace(altAppDir, "{3}");
                File.WriteAllText(configPath, config);
            }
        }
    }
}
