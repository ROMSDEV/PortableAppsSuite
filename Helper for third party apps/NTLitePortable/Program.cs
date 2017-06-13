namespace NTLitePortable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
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
            var curPath64 = PathEx.Combine(PathEx.LocalDir, "NTLite64Portable.exe");
            if (Environment.Is64BitOperatingSystem && File.Exists(curPath64))
            {
                ProcessEx.Start(curPath64, EnvironmentEx.CommandLine(false));
                return;
            }

            var appDir = PathEx.Combine(PathEx.LocalDir, "App\\NTLite");
            var updaterPath = PathEx.Combine(appDir, "NTLiteUpdater.exe");
#else
            var appDir = PathEx.Combine(PathEx.LocalDir, "App\\NTLite64");
            var updaterPath = PathEx.Combine(appDir, "NTLiteUpdater64.exe");
#endif
            var appPath = Path.Combine(appDir, "NTLite.exe");

            if (!File.Exists(updaterPath) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(updaterPath)))
                return;

            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                if (!newInstance)
                    return;

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");

                try
                {
                    var temp = Path.Combine(dataDir, "TEMP");
                    if (!Directory.Exists(temp))
                        Directory.CreateDirectory(temp);
                    var cfgPath = Path.Combine(dataDir, "settings.xml");
                    if (File.Exists(cfgPath))
                    {
                        var match = Regex.Match(File.ReadAllText(cfgPath), "<TempFolder>(.+?)</TempFolder>", RegexOptions.IgnoreCase).Groups[1].Value;
                        if (!string.IsNullOrWhiteSpace(match))
                        {
                            Encoding encoding;
                            var sb = new StringBuilder();
                            using (var sr = new StreamReader(cfgPath))
                            {
                                string line;
                                encoding = sr.CurrentEncoding;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    var m = Regex.Match(line, "<TempFolder>(.+?)</TempFolder>", RegexOptions.IgnoreCase).Groups[1].Value;
                                    if (!string.IsNullOrWhiteSpace(m))
                                    {
                                        var dirName = Path.GetFileName(EnvironmentEx.GetVariableValue("CurDir"));
                                        if (dirName != null)
                                            line = line.Replace(m, line.ToLower().Contains($"{dirName.ToLower()}\\data\\temp") ? temp : "%TEMP%");
                                    }
                                    sb.AppendLine(line);
                                }
                            }
                            using (var sw = new StreamWriter(cfgPath, false, encoding))
                                sw.Write(sb.ToString());
                        }
                    }
                    else
                    {
                        var content = Resources.DefaultSetting;
                        content = content.Replace("%TEMP%", temp);
                        using (var sw = File.CreateText(cfgPath))
                            sw.Write(content);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                try
                {
                    var srcDir = PathEx.Combine(appDir, "Presets");
                    if (Directory.Exists(srcDir) && !Data.DirIsLink(srcDir))
                    {
                        var destDir = PathEx.Combine(dataDir, "Presets");
                        Data.DirCopy(srcDir, destDir, true, true);
                        Directory.Delete(srcDir, true);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                var dirMap = new Dictionary<string, string>
                {
                    {
                        PathEx.Combine(appDir, "Presets"),
                        PathEx.Combine(dataDir, "Presets")
                    }
                };

                var fileMap = new Dictionary<string, string>
                {
                    {
                        Path.Combine(appDir, "settings.xml"),
                        Path.Combine(dataDir, "settings.xml")
                    },
                    {
                        Path.Combine(appDir, "settingsUI.xml"),
                        Path.Combine(dataDir, "settingsUI.xml")
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

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);
                Helper.FileForwarding(Helper.Options.Start, fileMap, true);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);
                Helper.FileForwarding(Helper.Options.Exit, fileMap, true);
            }
        }
    }
}
