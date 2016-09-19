using SilDev;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NTLitePortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            try
            {
                using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                {
                    if (newInstance)
                    {
                        LOG.AllowDebug();
#if x86
                        string ntlite = PATH.Combine("%CurDir%\\App\\NTLite");
#else
                        string ntlite = PATH.Combine("%CurDir%\\App\\NTLite64");
#endif
                        if (!Directory.Exists(ntlite) || Process.GetProcessesByName("NTLite").Length > 0)
                            return;
                        string temp = PATH.Combine("%CurDir%\\Data\\TEMP");
                        if (!Directory.Exists(temp))
                            Directory.CreateDirectory(temp);
                        string settings = Path.Combine(ntlite, "settings.xml");
                        try
                        {
                            if (File.Exists(settings))
                            {
                                string match = Regex.Match(File.ReadAllText(settings), "<TempFolder>(.+?)</TempFolder>", RegexOptions.IgnoreCase).Groups[1].Value;
                                if (!string.IsNullOrWhiteSpace(match))
                                {
                                    Encoding encoding;
                                    StringBuilder output = new StringBuilder();
                                    using (StreamReader sr = new StreamReader(settings))
                                    {
                                        string line;
                                        encoding = sr.CurrentEncoding;
                                        while ((line = sr.ReadLine()) != null)
                                        {
                                            string m = Regex.Match(line, "<TempFolder>(.+?)</TempFolder>", RegexOptions.IgnoreCase).Groups[1].Value;
                                            if (!string.IsNullOrWhiteSpace(m))
                                                line = line.Replace(m, line.ToLower().Contains(string.Format("{0}\\data\\temp", Path.GetFileName(PATH.GetEnvironmentVariableValue("CurDir")).ToLower())) ? temp : "%TEMP%");
                                            output.AppendLine(line);
                                        }
                                    }
                                    using (StreamWriter writer = new StreamWriter(settings, false, encoding))
                                        writer.Write(output.ToString());
                                }
                            }
                            else
                            {
                                string content = new CRYPT.Base64().DecodeString("PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxHZW5lcmFsT3B0aW9ucyB4bWxucz0idXJuOnNjaGVtYXMtbmxpdGVvcy1jb206cG4udjEiPg0KCTxMb2FkTGFzdFNlc3Npb24+dHJ1ZTwvTG9hZExhc3RTZXNzaW9uPg0KCTxMaWNlbnNlS2V5PjwvTGljZW5zZUtleT4NCgk8U2lsZW5jZVByb1VwZ3JhZGU+dHJ1ZTwvU2lsZW5jZVByb1VwZ3JhZGU+DQoJPFNpbGVuY2VDYW5jZWxlZFRhcmdldD5mYWxzZTwvU2lsZW5jZUNhbmNlbGVkVGFyZ2V0Pg0KCTxDaGVja1VwZGF0ZURhaWx5PmZhbHNlPC9DaGVja1VwZGF0ZURhaWx5Pg0KCTxMYXN0VXBkYXRlQ2hlY2s+MDwvTGFzdFVwZGF0ZUNoZWNrPg0KCTxTaG93SW5mb0Jhcj50cnVlPC9TaG93SW5mb0Jhcj4NCgk8U2tpcFJlbW92YWxXYXJuaW5nPmZhbHNlPC9Ta2lwUmVtb3ZhbFdhcm5pbmc+DQoJPFNraXBMaXZlV2FybmluZz5mYWxzZTwvU2tpcExpdmVXYXJuaW5nPg0KCTxTa2lwRXhwbG9yZVdhcm5pbmc+ZmFsc2U8L1NraXBFeHBsb3JlV2FybmluZz4NCgk8U2tpcEVzZFdhcm5pbmc+ZmFsc2U8L1NraXBFc2RXYXJuaW5nPg0KCTxWaXN1YWxTdHlsZT4yPC9WaXN1YWxTdHlsZT4NCgk8VmlzdWFsQWNjZW50PjA8L1Zpc3VhbEFjY2VudD4NCgk8VGVtcEZvbGRlcj4lVEVNUCU8L1RlbXBGb2xkZXI+DQoJPEltYWdlcz48L0ltYWdlcz4NCjwvR2VuZXJhbE9wdGlvbnM+DQo=");
                                content = content.Replace("%TEMP%", temp);
                                using (StreamWriter sw = File.CreateText(settings))
                                    sw.Write(content);
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                        RUN.App(new ProcessStartInfo() { FileName = Path.Combine(ntlite, "NTLite.exe") }, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug(ex);
            }
        }
    }
}
