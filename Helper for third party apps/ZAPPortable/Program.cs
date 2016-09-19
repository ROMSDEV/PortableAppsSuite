using SilDev;
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
                        LOG.AllowDebug();

                        string appPath = PATH.Combine("%CurDir%\\App\\ZedAttackProxy\\zap-2.5.0.jar");
                        if (!File.Exists(appPath) || Process.GetProcessesByName("zap").Length > 0)
                            return;

                        string appDrive = new DriveInfo(PATH.GetEnvironmentVariableValue("CurDir")).RootDirectory.Root.Name;
                        string JavaPath = appDrive;
                        foreach (string dirName in PATH.GetEnvironmentVariableValue("CurDir").Split('\\'))
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

                        string PortableProfile = PATH.Combine("%CurDir%\\Data");
                        if (!Directory.Exists(PortableProfile))
                            Directory.CreateDirectory(PortableProfile);
                        string UserProfile = PATH.Combine("%UserProfile%\\OWASP ZAP");
                        DATA.DirLink(UserProfile, PortableProfile, true);

                        string cmdLine = Environment.CommandLine.Replace($"\"{Application.ExecutablePath}\"", string.Empty).TrimStart();
                        RUN.App(new ProcessStartInfo()
                        {
                            Arguments = $"-Xmx512m -XX:PermSize=256M -jar \"{appPath}\" {cmdLine}",
                            FileName = JavaPath,
                            WorkingDirectory = Path.GetDirectoryName(appPath)
                        }, 0);

                        DATA.DirUnLink(UserProfile, true);
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
