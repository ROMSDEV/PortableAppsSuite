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
            try
            {
                bool newInstance;
                using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                {
                    if (!newInstance)
                        return;
#if x86
                    var ntlite = PathEx.Combine("%CurDir%\\App\\NTLite");
#else
                    var ntlite = PathEx.Combine("%CurDir%\\App\\NTLite64");
#endif
                    if (!Directory.Exists(ntlite) || Process.GetProcessesByName("NTLite").Length > 0)
                        return;
                    var temp = PathEx.Combine("%CurDir%\\Data\\TEMP");
                    if (!Directory.Exists(temp))
                        Directory.CreateDirectory(temp);
                    var settings = Path.Combine(ntlite, "settings.xml");
                    try
                    {
                        if (File.Exists(settings))
                        {
                            var match = Regex.Match(File.ReadAllText(settings), "<TempFolder>(.+?)</TempFolder>", RegexOptions.IgnoreCase).Groups[1].Value;
                            if (!string.IsNullOrWhiteSpace(match))
                            {
                                Encoding encoding;
                                var output = new StringBuilder();
                                using (var sr = new StreamReader(settings))
                                {
                                    string line;
                                    encoding = sr.CurrentEncoding;
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        var m = Regex.Match(line, "<TempFolder>(.+?)</TempFolder>", RegexOptions.IgnoreCase).Groups[1].Value;
                                        if (!string.IsNullOrWhiteSpace(m))
                                        {
                                            var dirDirName = Path.GetFileName(EnvironmentEx.GetVariableValue("CurDir"));
                                            if (dirDirName != null)
                                                line = line.Replace(m, line.ToLower().Contains($"{dirDirName.ToLower()}\\data\\temp") ? temp : "%TEMP%");
                                        }
                                        output.AppendLine(line);
                                    }
                                }
                                using (var writer = new StreamWriter(settings, false, encoding))
                                    writer.Write(output.ToString());
                            }
                        }
                        else
                        {
                            var content = Resources.DefaultSetting;
                            content = content.Replace("%TEMP%", temp);
                            using (var sw = File.CreateText(settings))
                                sw.Write(content);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                    using (var p = ProcessEx.Start(Path.Combine(ntlite, "NTLite.exe"), true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}
