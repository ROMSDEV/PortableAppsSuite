namespace ZAPPortable
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
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
                    var appPath = PathEx.Combine("%CurDir%\\App\\ZedAttackProxy\\zap-2.5.0.jar");
                    if (!File.Exists(appPath) || Process.GetProcessesByName("zap").Length > 0)
                        return;

                    var appDrive = new DriveInfo(EnvironmentEx.GetVariableValue("CurDir")).RootDirectory.Root.Name;
                    var javaPath = appDrive;
                    foreach (var dirName in EnvironmentEx.GetVariableValue("CurDir").Split('\\'))
                    {
                        if (appDrive.Contains(dirName))
                            continue;
                        javaPath = Path.Combine(javaPath, dirName);
                        if (Environment.Is64BitOperatingSystem)
                        {
                            if (File.Exists(Path.Combine(javaPath, "Java64Portable\\CommonFiles\\Java64\\bin\\javaw.exe")))
                            {
                                javaPath = Path.Combine(javaPath, "Java64Portable\\CommonFiles\\Java64\\bin\\javaw.exe");
                                break;
                            }
                            if (File.Exists(Path.Combine(javaPath, "CommonFiles\\Java64\\bin\\javaw.exe")))
                            {
                                javaPath = Path.Combine(javaPath, "CommonFiles\\Java64\\bin\\javaw.exe");
                                break;
                            }
                        }
                        if (File.Exists(Path.Combine(javaPath, "Java64Portable\\CommonFiles\\Java\\bin\\javaw.exe")))
                        {
                            javaPath = Path.Combine(javaPath, "Java64Portable\\CommonFiles\\Java\\bin\\javaw.exe");
                            break;
                        }
                        if (File.Exists(Path.Combine(javaPath, "JavaPortable\\CommonFiles\\Java\\bin\\javaw.exe")))
                        {
                            javaPath = Path.Combine(javaPath, "JavaPortable\\CommonFiles\\Java\\bin\\javaw.exe");
                            break;
                        }
                        if (File.Exists(Path.Combine(javaPath, "CommonFiles\\Java\\bin\\javaw.exe")))
                        {
                            javaPath = Path.Combine(javaPath, "CommonFiles\\Java\\bin\\javaw.exe");
                            break;
                        }
                    }
                    if (!File.Exists(javaPath))
                    {
                        MessageBox.Show(@"Java Portable not found!", @"Zed Attack Proxy Portable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var portableProfile = PathEx.Combine("%CurDir%\\Data");
                    if (!Directory.Exists(portableProfile))
                        Directory.CreateDirectory(portableProfile);
                    var userProfile = PathEx.Combine("%UserProfile%\\OWASP ZAP");
                    Data.DirLink(userProfile, portableProfile, true);

                    using (var p = ProcessEx.Start(javaPath, Path.GetDirectoryName(appPath), $"-Xmx512m -XX:PermSize=256M -jar \"{appPath}\" {EnvironmentEx.CommandLine(false)}", false, false))
                        if (!p?.HasExited == true)
                            p?.WaitForExit();

                    Data.DirUnLink(userProfile, true);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}
