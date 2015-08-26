using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ZAPPortable
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
                    SilDev.Log.AllowDebug();
                    string appPath = Path.Combine(Application.StartupPath, "App\\ZedAttackProxy\\zap.jar");
                    if (!File.Exists(appPath) || Process.GetProcessesByName("hlsw").Length > 0)
                        return;
                    string Java = null;
                    foreach (var dir in Application.StartupPath.Split('\\'))
                    {
                        Java = Path.Combine(string.Format("{0}\\", Java), dir);
                        if (Environment.Is64BitOperatingSystem)
                        {
                            if (File.Exists(Path.Combine(Java, "Java64Portable\\CommonFiles\\Java64\\bin\\javaw.exe")))
                            {
                                Java = Path.Combine(Java, "Java64Portable\\CommonFiles\\Java64\\bin\\javaw.exe");
                                break;
                            }
                            if (File.Exists(Path.Combine(Java, "CommonFiles\\Java64\\bin\\javaw.exe")))
                            {
                                Java = Path.Combine(Java, "CommonFiles\\Java64\\bin\\javaw.exe");
                                break;
                            }
                        }
                        if (File.Exists(Path.Combine(Java, "Java64Portable\\CommonFiles\\Java\\bin\\javaw.exe")))
                        {
                            Java = Path.Combine(Java, "Java64Portable\\CommonFiles\\Java\\bin\\javaw.exe");
                            break;
                        }
                        if (File.Exists(Path.Combine(Java, "JavaPortable\\CommonFiles\\Java\\bin\\javaw.exe")))
                        {
                            Java = Path.Combine(Java, "JavaPortable\\CommonFiles\\Java\\bin\\javaw.exe");
                            break;
                        }
                        if (File.Exists(Path.Combine(Java, "CommonFiles\\Java\\bin\\javaw.exe")))
                        {
                            Java = Path.Combine(Java, "CommonFiles\\Java\\bin\\javaw.exe");
                            break;
                        }
                    }
                    if (!File.Exists(Java))
                    {
                        MessageBox.Show("Java Portable not found!", "Zed Attack Proxy Portable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    string PortableProfile = Path.Combine(Application.StartupPath, "Data\\Cfg");
                    if (!Directory.Exists(PortableProfile))
                        Directory.CreateDirectory(PortableProfile);
                    if (Directory.Exists(PortableProfile) && !File.Exists(Path.Combine(PortableProfile, ".PortableApp")))
                        File.Create(Path.Combine(PortableProfile, ".PortableApp")).Close();
                    string UserProfile = SilDev.Run.EnvironmentVariableFilter("%UserProfile%\\OWASP ZAP");
                    if (Directory.Exists(UserProfile) && !File.Exists(Path.Combine(UserProfile, ".PortableApp")))
                        Directory.Move(UserProfile, string.Format("{0}.SI13N7-BACKUP", UserProfile));
                    if (!Directory.Exists(UserProfile))
                        DirectoryCopy(PortableProfile, UserProfile, true);
                    using (Process app = new Process())
                    {
                        string commandLine = Environment.CommandLine.Replace(string.Format("\"{0}\"", Application.ExecutablePath), string.Empty);
                        app.StartInfo.Arguments = string.Format("-Xmx512m -XX:PermSize=256M -jar \"{0}\" {1}", appPath, commandLine);
                        app.StartInfo.FileName = Java;
                        app.StartInfo.WorkingDirectory = Path.GetDirectoryName(appPath);
                        app.Start();
                        app.WaitForExit();
                    }
                    if (Directory.Exists(UserProfile))
                    {
                        if (Directory.Exists(PortableProfile))
                            Directory.Delete(PortableProfile, true);
                        DirectoryCopy(UserProfile, PortableProfile, true);
                        Directory.Delete(UserProfile, true);
                    }
                    if (!Directory.Exists(UserProfile) && Directory.Exists(string.Format("{0}.SI13N7-BACKUP", UserProfile)))
                        Directory.Move(string.Format("{0}.SI13N7-BACKUP", UserProfile), UserProfile);
                }
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
                SilDev.Log.Debug(ex.Message, "ZAPPortable.Program.DirectoryCopy");
            }
        }
    }
}
