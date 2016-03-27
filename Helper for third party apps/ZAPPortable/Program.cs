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
            try
            {
                using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                {
                    if (newInstance)
                    {
                        SilDev.Log.AllowDebug();

                        string appPath = Path.Combine(Application.StartupPath, "App\\ZedAttackProxy\\zap-2.4.3.jar");
                        if (!File.Exists(appPath) || Process.GetProcessesByName("zap").Length > 0)
                            return;

                        string appDrive = new DriveInfo(Application.StartupPath).RootDirectory.Root.Name;
                        string JavaPath = appDrive;
                        foreach (string dirName in Application.StartupPath.Split('\\'))
                        {
                            if (appDrive.Contains(dirName))
                                continue;
                            JavaPath = Path.Combine(JavaPath, dirName);
                            if (Environment.Is64BitOperatingSystem)
                            {
                                if (File.Exists(Path.Combine(JavaPath, "Java64Portable\\CommonFiles\\Java64\\bin\\javaw.exe")))
                                {
                                    JavaPath = Path.Combine(JavaPath, "Java64Portable\\CommonFiles\\Java64\\bin\\javaw.exe");
                                    break;
                                }
                                if (File.Exists(Path.Combine(JavaPath, "CommonFiles\\Java64\\bin\\javaw.exe")))
                                {
                                    JavaPath = Path.Combine(JavaPath, "CommonFiles\\Java64\\bin\\javaw.exe");
                                    break;
                                }
                            }
                            if (File.Exists(Path.Combine(JavaPath, "Java64Portable\\CommonFiles\\Java\\bin\\javaw.exe")))
                            {
                                JavaPath = Path.Combine(JavaPath, "Java64Portable\\CommonFiles\\Java\\bin\\javaw.exe");
                                break;
                            }
                            if (File.Exists(Path.Combine(JavaPath, "JavaPortable\\CommonFiles\\Java\\bin\\javaw.exe")))
                            {
                                JavaPath = Path.Combine(JavaPath, "JavaPortable\\CommonFiles\\Java\\bin\\javaw.exe");
                                break;
                            }
                            if (File.Exists(Path.Combine(JavaPath, "CommonFiles\\Java\\bin\\javaw.exe")))
                            {
                                JavaPath = Path.Combine(JavaPath, "CommonFiles\\Java\\bin\\javaw.exe");
                                break;
                            }
                        }
                        if (!File.Exists(JavaPath))
                        {
                            MessageBox.Show("Java Portable not found!", "Zed Attack Proxy Portable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        string PortableProfile = SilDev.Run.EnvironmentVariableFilter("%CurrentDir%\\Data");
                        if (!Directory.Exists(PortableProfile))
                            Directory.CreateDirectory(PortableProfile);
                        string UserProfile = SilDev.Run.EnvironmentVariableFilter("%UserProfile%\\OWASP ZAP");
                        SilDev.Data.DirLink(UserProfile, PortableProfile, true);

                        string cmdLine = Environment.CommandLine.Replace($"\"{Application.ExecutablePath}\"", string.Empty).TrimStart();
                        SilDev.Run.App(new ProcessStartInfo()
                        {
                            Arguments = $"-Xmx512m -XX:PermSize=256M -jar \"{appPath}\" {cmdLine}",
                            FileName = JavaPath,
                            WorkingDirectory = Path.GetDirectoryName(appPath)
                        }, 0);

                        SilDev.Data.DirUnLink(UserProfile, true);
                    }
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }
    }
}
