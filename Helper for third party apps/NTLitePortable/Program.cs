namespace NTLitePortable
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
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
                ProcessEx.Start(curPath64, EnvironmentEx.CommandLine());
                return;
            }

            var appDir = PathEx.Combine("%CurDir%\\App\\NTLite");
#else
            var appDir = PathEx.Combine("%CurDir%\\App\\NTLite64");
#endif
            var appPath = Path.Combine(appDir, "NTLite.exe");

            if (!Directory.Exists(appDir) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                return;

            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (!newInstance)
                    return;

                var temp = PathEx.Combine("%CurDir%\\Data\\TEMP");
                if (!Directory.Exists(temp))
                    Directory.CreateDirectory(temp);
                var cfgPath = Path.Combine(appDir, "settings.xml");

                try
                {
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

                using (var p = ProcessEx.Start(appPath, true, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
            }
        }
    }
}
