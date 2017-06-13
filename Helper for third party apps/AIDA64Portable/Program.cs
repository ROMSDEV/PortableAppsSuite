namespace AIDA64Portable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Portable;
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

                var appPath = PathEx.Combine(PathEx.LocalDir, "App\\AIDA64\\aida64.exe");
                if (!File.Exists(appPath) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)))
                    return;

                const string iniPath = "%CurDir%\\Data\\aida64.ini";
                var iniMap = new Dictionary<string, Dictionary<string, string>>
                {
                    {
                        "Generic", new Dictionary<string, string>
                        {
                            {
                                "NoRegistry",
                                "1"
                            },
                            {
                                "NetUpdateFreq",
                                "1"
                            },
                            {
                                "ReportFileFolder",
                                PathEx.Combine(PathEx.LocalDir, "Data\\Reports")
                            }
                        }
                    }
                };

                var fileMap = new Dictionary<string, string>
                {
                    {
                        "%CurDir%\\App\\AIDA64\\aida64.ini",
                        "%CurDir%\\Data\\aida64.ini"
                    },
                    {
                        "%CurDir%\\App\\AIDA64\\aida64.reg.ini",
                        "%CurDir%\\Data\\aida64.reg.ini"
                    }
                };

                Helper.ConfigOverwrite(iniMap, iniPath);

                Helper.FileForwarding(Helper.Options.Start, fileMap, true);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.FileForwarding(Helper.Options.Exit, fileMap, true);
            }
        }
    }
}
