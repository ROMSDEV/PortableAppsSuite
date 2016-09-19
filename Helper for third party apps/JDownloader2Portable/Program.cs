using SilDev;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace JDownloader2Portable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            LOG.AllowDebug();
            try
            {
                string JavaPath = string.Empty;
                string ExePath = PATH.Combine("%CurDir%\\JDownloader_v2.0\\JDownloader2.exe");
                if (!File.Exists(ExePath))
                {
                    string drive = new DriveInfo(PATH.GetEnvironmentVariableValue("CurDir")).RootDirectory.Root.Name;
                    string JavaDir = drive;
                    foreach (string dirName in PATH.GetEnvironmentVariableValue("CurDir").Split('\\'))
                    {
                        try
                        {
                            if (drive.Contains(dirName))
                                continue;
                            JavaDir = Path.Combine(JavaDir, dirName);
                            string tmpDir = string.Empty;
                            if (Environment.Is64BitOperatingSystem)
                            {
                                tmpDir = Path.Combine(JavaDir, "CommonFiles\\Java64");
                                if (Directory.Exists(tmpDir))
                                {
                                    foreach (string file in Directory.GetFiles(tmpDir, "javaw.exe", SearchOption.AllDirectories))
                                    {
                                        JavaDir = tmpDir;
                                        JavaPath = file;
                                        break;
                                    }
                                }
                            }
                            tmpDir = Path.Combine(JavaDir, "CommonFiles\\Java64");
                            if (File.Exists(tmpDir))
                            {
                                if (Directory.Exists(tmpDir))
                                {
                                    foreach (string file in Directory.GetFiles(tmpDir, "javaw.exe", SearchOption.AllDirectories))
                                    {
                                        JavaDir = tmpDir;
                                        JavaPath = file;
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG.Debug(ex);
                        }
                    }
                    if (!File.Exists(JavaPath))
                    {
                        if (File.Exists($"{ExePath}.disabled"))
                            File.Move($"{ExePath}.disabled", ExePath);
                        string UpdExePath = PATH.Combine("%CurDir%\\JDownloader_v2.0\\JDownloader2Update.exe");
                        if (File.Exists($"{UpdExePath}.disabled"))
                            File.Move($"{UpdExePath}.disabled", UpdExePath);
                    }
                }
                if (!File.Exists(JavaPath) && !File.Exists(ExePath))
                {
                    MessageBox.Show("Java Portable not found!", "JDownloader 2 Portable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string JDownloader = PATH.Combine("%CurDir%\\JDownloader_v2.0\\JDownloader.jar");
                if (!File.Exists(JDownloader))
                {
                    MessageBox.Show("JDownloader 2 not found!", "JDownloader 2 Portable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                bool newInstance = true;
                using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                {
                    if (newInstance)
                    {
                        string jreUsageDir = PATH.Combine("%UserProfile%\\.oracle_jre_usage");
                        if (!Directory.Exists(jreUsageDir))
                            Directory.CreateDirectory(jreUsageDir);
                        DATA.SetAttributes(jreUsageDir, FileAttributes.Hidden);
                        if (File.Exists(ExePath))
                            RUN.App(new ProcessStartInfo()
                            {
                                Arguments = RUN.CommandLine(false),
                                FileName = ExePath,
                            }, 0);
                        else
                            RUN.App(new ProcessStartInfo()
                            {
                                Arguments = $"-jar \"{JDownloader}\" {RUN.CommandLine(false)}".Trim(),
                                FileName = JavaPath,
                                WorkingDirectory = Path.GetDirectoryName(JDownloader)
                            }, 0);
                        if (Directory.Exists(jreUsageDir))
                            Directory.Delete(jreUsageDir, true);
                    }
                    else
                    {
                        if (File.Exists(ExePath))
                            RUN.App(new ProcessStartInfo()
                            {
                                Arguments = RUN.CommandLine(false),
                                FileName = ExePath,
                            }, 0);
                        else
                            RUN.App(new ProcessStartInfo()
                            {
                                Arguments = $"-jar \"{JDownloader}\" {RUN.CommandLine(false)}".Trim(),
                                FileName = JavaPath,
                                WorkingDirectory = Path.GetDirectoryName(JDownloader)
                            }, 0);
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
