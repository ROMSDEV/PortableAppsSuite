namespace GyazoPortable
{
    using System;
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
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                if (!newInstance)
                    return;
                Ini.SetFile(Path.ChangeExtension(PathEx.LocalPath, ".ini"));
                if (Ini.Read("Settings", "SecondRunMode", true))
                {
                    var defCacheDir = PathEx.Combine("%AppData%\\Gyazo");
                    var cacheDir = PathEx.Combine("%CurDir%\\Data\\cache");
                    Data.DirLink(defCacheDir, cacheDir, true);
                    var regPath = PathEx.Combine("%CurDir%\\Data\\settings.reg");
                    if (File.Exists(regPath))
                        Reg.ImportFile(regPath);
                    using (var p = ProcessEx.Start("%CurDir%\\App\\Gyazo\\GyStation.exe", true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                    Reg.ExportKeys(regPath, "HKCU\\Software\\Gyazo");
                    Data.DirUnLink(defCacheDir, true);
                }
                else
                {
                    using (var p = ProcessEx.Start("%CurDir%\\App\\Gyazo\\Gyazowin.exe", true, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();
                }
            }
        }
    }
}
