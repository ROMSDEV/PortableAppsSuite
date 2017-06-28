namespace JDownloader2Portable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using Portable;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();

            var appDir = PathEx.Combine(PathEx.LocalDir, "App\\jd2");
            var appPath = Path.Combine(appDir, "JDownloader.jar");
            if (!File.Exists(appPath))
            {
                MessageBox.Show(@"JDownloader 2 not found!", @"JDownloader 2 Portable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var iniPath = Path.ChangeExtension(PathEx.LocalPath, ".ini");
            if (!File.Exists(iniPath))
                Ini.WriteDirect("Associations", "FileTypes", "ccf,dlc,jdc,rsdf,sft", iniPath);

            Helper.FindJava(out string javaPath, iniPath);
            using (new Mutex(true, ProcessEx.CurrentName, out bool newInstance))
            {
                var cmdLine = $"-jar \"{appPath}\" {EnvironmentEx.CommandLine(false)}".Trim();
                if (!newInstance)
                {
                    ProcessEx.Start(javaPath, appDir, cmdLine);
                    return;
                }
                try
                {
                    DataHelper();
                    using (var p = ProcessEx.Start(javaPath, appDir, cmdLine, false, false))
                        if (p?.HasExited == false)
                            p.WaitForExit();
                    Recheck:
                    var appName = Path.GetFileNameWithoutExtension(javaPath);
                    foreach (var p in ProcessEx.GetInstances(appName))
                    {
                        var wasRunning = false;
                        bool isRunning;
                        do
                        {
                            isRunning = p?.GetCommandLine()?.ContainsEx(appDir) == true;
                            if (p?.HasExited == false)
                                p.WaitForExit();
                            if (!wasRunning && isRunning)
                                wasRunning = true;
                            Thread.Sleep(200);
                        }
                        while (isRunning);
                        if (!wasRunning)
                            continue;
                        Thread.Sleep(250);
                        goto Recheck;
                    }

                    var usageDir = PathEx.Combine("%UserProfile%\\.oracle_jre_usage");
                    if (Directory.Exists(usageDir))
                        Directory.Delete(usageDir);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }

        private static void DataHelper()
        {
            var dirMap = new Dictionary<string, string>
            {
                {
                    "%CurDir%\\JDownloader_v2.0\\.install4j",
                    "%CurDir%\\App\\jd2\\.install4j"
                },
                {
                    "%CurDir%\\JDownloader_v2.0\\captchas",
                    "%CurDir%\\App\\jd2\\captchas"
                },
                {
                    "%CurDir%\\JDownloader_v2.0\\cfg",
                    "%CurDir%\\App\\jd2\\cfg"
                },
                {
                    "%CurDir%\\JDownloader_v2.0\\logs",
                    "%CurDir%\\App\\jd2\\logs"
                },
                {
                    "%CurDir%\\JDownloader_v2.0\\tmp",
                    "%CurDir%\\App\\jd2\\tmp"
                }
            };
            foreach (var pair in dirMap)
            {
                var srcDir = PathEx.Combine(pair.Key);
                if (!Directory.Exists(srcDir))
                    continue;
                var destDir = PathEx.Combine(pair.Value);
                Data.DirCopy(srcDir, destDir, true, true);
            }
            try
            {
                var dir = PathEx.Combine("%CurDir%\\JDownloader_v2.0");
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            var dirList = new List<string>
            {
                "%CurDir%\\App\\jd2\\.install4j",
                "%CurDir%\\App\\jd2\\captchas",
                "%CurDir%\\App\\jd2\\cfg",
                "%CurDir%\\App\\jd2\\logs",
                "%CurDir%\\App\\jd2\\tmp"
            };
            try
            {
                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                if (!Directory.Exists(dataDir))
                    Directory.CreateDirectory(dataDir);
                foreach (var d in dirList)
                {
                    var dir = PathEx.Combine(d);
                    if (!Directory.Exists(dir))
                        continue;
                    var name = Path.GetFileName(dir);
                    if (string.IsNullOrEmpty(name))
                        continue;
                    var file = Path.Combine(dataDir, $"{name}.lnk");
                    Data.CreateShortcut(dir, file);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}
