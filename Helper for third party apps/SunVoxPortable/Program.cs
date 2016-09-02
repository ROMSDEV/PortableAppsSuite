using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SunVoxPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                string appPath = Path.Combine(Application.StartupPath, "App\\sunvox_win32\\sunvox.exe");
                if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                    return;
                if (newInstance)
                {
                    SilDev.Log.AllowDebug();
                    try
                    {
                        foreach (string f in Directory.GetFiles(SilDev.Run.EnvVarFilter("%AppData%"), "*", SearchOption.TopDirectoryOnly))
                        {
                            if (!f.ToLower().Contains("sunvox") || f.ToUpper().EndsWith("SI13N7-BACKUP"))
                                continue;
                            string b = string.Format("{0}.SI13N7-BACKUP", f);
                            if (File.Exists(b))
                                continue;
                            File.Move(f, b);
                        }
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                    string dataPath = Path.Combine(Application.StartupPath, "Data");
                    try
                    {
                        if (Directory.Exists(dataPath))
                        {
                            foreach (string f in Directory.GetFiles(dataPath, "*", SearchOption.TopDirectoryOnly))
                                if (f.ToLower().Contains("sunvox"))
                                    File.Move(f, f.Replace(dataPath, SilDev.Run.EnvVarFilter("%AppData%")));
                        }
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                    SilDev.Run.App(new ProcessStartInfo()
                    {
                        FileName = appPath,
                        WindowStyle = ProcessWindowStyle.Maximized
                    }, 0);
                    try
                    {
                        foreach (string f in Directory.GetFiles(SilDev.Run.EnvVarFilter("%AppData%"), "*", SearchOption.TopDirectoryOnly))
                        {
                            if (!f.ToLower().Contains("sunvox") || f.ToUpper().EndsWith("SI13N7-BACKUP"))
                                continue;
                            if (!Directory.Exists(dataPath))
                                Directory.CreateDirectory(dataPath);
                            File.Move(f, f.Replace(SilDev.Run.EnvVarFilter("%AppData%"), dataPath));
                        }
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                    try
                    {
                        foreach (string b in Directory.GetFiles(SilDev.Run.EnvVarFilter("%AppData%"), "*", SearchOption.TopDirectoryOnly))
                        {
                            if (!b.ToLower().Contains("sunvox") || !b.ToUpper().EndsWith("SI13N7-BACKUP"))
                                continue;
                            string f = b.Replace(".SI13N7-BACKUP", string.Empty);
                            File.Move(b, f);
                        }
                    }
                    catch (Exception ex)
                    {
                        SilDev.Log.Debug(ex);
                    }
                }
            }
        }
    }
}
