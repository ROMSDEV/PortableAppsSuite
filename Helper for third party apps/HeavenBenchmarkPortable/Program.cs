using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace HeavenBenchmarkPortable // Heaven Benchmark 4.0
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    string dataPath = Path.Combine(Application.StartupPath, "Data\\UserProfile");
                    string appdataPath = Path.Combine(Environment.GetEnvironmentVariable("UserProfile"), "Heaven");
                    if (!Directory.Exists(dataPath) && Directory.Exists(string.Format("{0}.BACKUP", dataPath)))
                        DirectoryCopy(string.Format("{0}.BACKUP", dataPath), dataPath, true);
                    if (Directory.Exists(dataPath))
                    {
                        if (Directory.Exists(appdataPath))
                            Directory.Delete(appdataPath, true);
                        DirectoryCopy(dataPath, appdataPath, true);
                    }
                    SilDev.Run.App(Path.Combine(Application.StartupPath, "App\\heaven\\bin"), "browser_x86.exe", string.Format("-config \"{0}\"", Path.Combine(Application.StartupPath, "App\\heaven\\data\\launcher\\launcher.xml")), 0);
                    foreach (Process app in Process.GetProcessesByName("Heaven"))
                        app.WaitForExit();
                    if (Directory.Exists(appdataPath))
                    {
                        if (Directory.Exists(dataPath))
                        {
                            if (Directory.Exists(string.Format("{0}.BACKUP", dataPath)))
                                Directory.Delete(string.Format("{0}.BACKUP", dataPath), true);
                            Directory.Move(dataPath, string.Format("{0}.BACKUP", dataPath));
                        }
                        DirectoryCopy(appdataPath, dataPath, true);
                        Directory.Delete(appdataPath, true);
                    }
                }
                else
                    Environment.Exit(2);
            }
        }

        static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);
                DirectoryInfo[] dirs = dir.GetDirectories();
                if (!dir.Exists)
                    throw new DirectoryNotFoundException(string.Format("Source directory does not exist or could not be found: {0}", sourceDirName));
                if (!Directory.Exists(destDirName))
                    Directory.CreateDirectory(destDirName);
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, false);
                }
                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                    }
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex.Message, "DirectoryCopy");
            }
        }
    }
}
