namespace SunVoxPortable
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            bool newInstance;
            using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                var appPath = PathEx.Combine("%CurDir%\\App\\sunvox_win32\\sunvox.exe");
                if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                    return;
                if (!newInstance)
                    return;
                try
                {
                    foreach (var f in Directory.GetFiles(PathEx.Combine("%AppData%"), "*", SearchOption.TopDirectoryOnly))
                    {
                        if (!f.ToLower().Contains("sunvox") || f.ToUpper().EndsWith("SI13N7-BACKUP"))
                            continue;
                        string b = $"{f}.SI13N7-BACKUP";
                        if (File.Exists(b))
                            continue;
                        File.Move(f, b);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                var dataPath = PathEx.Combine("%CurDir%\\Data");
                try
                {
                    if (Directory.Exists(dataPath))
                        foreach (var f in Directory.GetFiles(dataPath, "*", SearchOption.TopDirectoryOnly))
                            if (f.ToLower().Contains("sunvox"))
                                File.Move(f, f.Replace(dataPath, PathEx.Combine("%AppData%")));
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                using (var p = ProcessEx.Start(appPath, false, ProcessWindowStyle.Maximized, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();
                try
                {
                    foreach (var f in Directory.GetFiles(PathEx.Combine("%AppData%"), "*", SearchOption.TopDirectoryOnly))
                    {
                        if (!f.ToLower().Contains("sunvox") || f.ToUpper().EndsWith("SI13N7-BACKUP"))
                            continue;
                        if (!Directory.Exists(dataPath))
                            Directory.CreateDirectory(dataPath);
                        File.Move(f, f.Replace(PathEx.Combine("%AppData%"), dataPath));
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                try
                {
                    foreach (var b in Directory.GetFiles(PathEx.Combine("%AppData%"), "*", SearchOption.TopDirectoryOnly))
                    {
                        if (!b.ToLower().Contains("sunvox") || !b.ToUpper().EndsWith("SI13N7-BACKUP"))
                            continue;
                        var f = b.Replace(".SI13N7-BACKUP", string.Empty);
                        File.Move(b, f);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }
    }
}
