using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace JDownloader2Portable
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
                    string JavaDir = string.Empty;
                    string JavaPath = string.Empty;
                    string ExePath = Path.Combine(Application.StartupPath, "JDownloader_v2.0\\JDownloader2.exe");
                    if (!File.Exists(ExePath))
                    {
                        string dir = Application.StartupPath;
                        for (int i = 0; i < Application.StartupPath.Count(x => x == '\\'); i++)
                        {
                            dir = Path.GetFullPath(Path.Combine(dir, "../"));
                            for (int j = 0; j < 7; j++)
                            {
                                if (!Environment.Is64BitOperatingSystem)
                                {
                                    j = 2;
                                    continue;
                                }
                                switch (j)
                                {
                                    case 0:
                                        JavaDir = Path.Combine(dir, "Java64Portable\\CommonFiles\\Java64");
                                        break;
                                    case 1:
                                        JavaDir = Path.Combine(dir, "JavaPortable\\CommonFiles\\Java64");
                                        break;
                                    case 2:
                                        JavaDir = Path.Combine(dir, "CommonFiles\\Java64");
                                        break;
                                    case 3:
                                        JavaDir = Path.Combine(dir, "Java64Portable\\CommonFiles\\Java");
                                        break;
                                    case 4:
                                        JavaDir = Path.Combine(dir, "JavaPortable\\CommonFiles\\Java");
                                        break;
                                    case 5:
                                        JavaDir = Path.Combine(dir, "CommonFiles\\Java");
                                        break;
                                    case 6:
                                        JavaDir = Environment.GetEnvironmentVariable("JAVA_HOME");
                                        break;
                                }
                                if (Directory.Exists(JavaDir))
                                {
                                    foreach (string file in Directory.GetFiles(JavaDir, "javaw.exe", SearchOption.AllDirectories))
                                    {
                                        JavaPath = file;
                                        break;
                                    }
                                    if (File.Exists(JavaPath))
                                        break;
                                }
                            }
                        }
                        if (!File.Exists(JavaPath))
                        {
                            if (File.Exists(string.Format("{0}.disabled", ExePath)))
                                File.Move(string.Format("{0}.disabled", ExePath), ExePath);
                            string UpdExePath = Path.Combine(Application.StartupPath, "JDownloader_v2.0\\JDownloader2Update.exe");
                            if (File.Exists(string.Format("{0}.disabled", UpdExePath)))
                                File.Move(string.Format("{0}.disabled", UpdExePath), UpdExePath);
                        }
                    }
                    string commandLine = Environment.CommandLine.Replace(Application.ExecutablePath, string.Empty).Replace("\"\"", string.Empty).TrimStart().TrimEnd();
                    if (File.Exists(ExePath))
                    {
                        using (Process app = new Process())
                        {
                            app.StartInfo.Arguments = commandLine;
                            app.StartInfo.FileName = ExePath;
                            app.StartInfo.WorkingDirectory = Path.GetDirectoryName(app.StartInfo.FileName);
                            app.Start();
                        }
                        return;
                    }
                    if (!File.Exists(JavaPath))
                    {
                        MessageBox.Show("Java Portable not found!", "JDownloader 2 Portable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    string JDownloader = Path.Combine(Application.StartupPath, "JDownloader_v2.0\\JDownloader.jar");
                    if (!File.Exists(JDownloader))
                    {
                        MessageBox.Show("JDownloader 2 not found!", "JDownloader 2 Portable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    using (Process app = new Process())
                    {
                        app.StartInfo.Arguments = string.Format("-jar \"{0}\" {1}", JDownloader, commandLine);
                        app.StartInfo.FileName = JavaPath;
                        app.StartInfo.WorkingDirectory = Path.GetDirectoryName(JDownloader);
                        app.Start();
                    }
                }
            }
        }
    }
}
