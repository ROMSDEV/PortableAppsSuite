namespace JDownloader2Portable
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
                var javaPath = string.Empty;
                var exePath = PathEx.Combine("%CurDir%\\JDownloader_v2.0\\JDownloader2.exe");
                var envVar = EnvironmentEx.GetVariableValue("CurDir");
                if (!File.Exists(exePath))
                {
                    var drive = new DriveInfo(envVar).RootDirectory.Root.Name;
                    var JavaDir = drive;
                    foreach (var dirName in envVar.Split('\\'))
                        try
                        {
                            if (drive.Contains(dirName))
                                continue;
                            JavaDir = Path.Combine(JavaDir, dirName);
                            string tmpDir;
                            if (Environment.Is64BitOperatingSystem)
                            {
                                tmpDir = Path.Combine(JavaDir, "CommonFiles\\Java64");
                                if (Directory.Exists(tmpDir))
                                    foreach (var file in Directory.GetFiles(tmpDir, "javaw.exe", SearchOption.AllDirectories))
                                    {
                                        JavaDir = tmpDir;
                                        javaPath = file;
                                        break;
                                    }
                            }
                            tmpDir = Path.Combine(JavaDir, "CommonFiles\\Java64");
                            if (!File.Exists(tmpDir))
                                continue;
                            if (!Directory.Exists(tmpDir))
                                continue;
                            foreach (var file in Directory.GetFiles(tmpDir, "javaw.exe", SearchOption.AllDirectories))
                            {
                                JavaDir = tmpDir;
                                javaPath = file;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    if (!File.Exists(javaPath))
                    {
                        if (File.Exists($"{exePath}.disabled"))
                            File.Move($"{exePath}.disabled", exePath);
                        var updExePath = PathEx.Combine("%CurDir%\\JDownloader_v2.0\\JDownloader2Update.exe");
                        if (File.Exists($"{updExePath}.disabled"))
                            File.Move($"{updExePath}.disabled", updExePath);
                    }
                }
                if (!File.Exists(javaPath) && !File.Exists(exePath))
                {
                    MessageBox.Show(@"Java Portable not found!", @"JDownloader 2 Portable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var jDownloader = Path.Combine("%CurDir%\\JDownloader_v2.0\\JDownloader.jar");
                if (!File.Exists(jDownloader))
                {
                    MessageBox.Show(@"JDownloader 2 not found!", @"JDownloader 2 Portable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                bool newInstance;
                using (new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                {
                    var jdDir = Path.GetDirectoryName(jDownloader);
                    if (newInstance)
                    {
                        var jreUsageDir = PathEx.Combine("%UserProfile%\\.oracle_jre_usage");
                        if (!Directory.Exists(jreUsageDir))
                            Directory.CreateDirectory(jreUsageDir);
                        Data.SetAttributes(jreUsageDir, FileAttributes.Hidden);
                        if (File.Exists(exePath))
                        {
                            using (var p = ProcessEx.Start(exePath, EnvironmentEx.CommandLine(false), false, false))
                                if (!p?.HasExited == true)
                                    p?.WaitForExit();
                        }
                        else
                        {
                            using (var p = ProcessEx.Start(javaPath, jdDir, $"-jar \"{jDownloader}\" {EnvironmentEx.CommandLine(false)}".Trim(), false, false))
                                if (!p?.HasExited == true)
                                    p?.WaitForExit();
                        }
                        if (Directory.Exists(jreUsageDir))
                            Directory.Delete(jreUsageDir, true);
                    }
                    else
                    {
                        if (File.Exists(exePath))
                            ProcessEx.Start(exePath, EnvironmentEx.CommandLine(false));
                        else
                            ProcessEx.Start(javaPath, jdDir, $"-jar \"{jDownloader}\" {EnvironmentEx.CommandLine(false)}".Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}
