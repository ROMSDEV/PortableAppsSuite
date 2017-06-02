namespace Cinebench64Portable // CINEBENCH_R15
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
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

                var appDir = PathEx.Combine("%CurDir%\\App\\cinebench64");
                var appPath = PathEx.Combine(appDir, "CINEBENCH Windows 64 Bit.exe");

                if (!File.Exists(appPath) || Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)).Length > 0)
                    return;

                var dataDirMap = new[,]
                {
                    {
                        PathEx.Combine("%AppData%\\MAXON"),
                        PathEx.Combine("%CurDir%\\Data")
                    }
                };
                for (var i = 0; i < dataDirMap.GetLength(0); i++)
                {
                    if (!Directory.Exists(dataDirMap[i, 1]))
                        Directory.CreateDirectory(dataDirMap[i, 1]);
                    Data.DirLink(dataDirMap[i, 0], dataDirMap[i, 1], true);
                }

                using (var p = ProcessEx.Start(appPath, true, false))
                    if (!p?.HasExited == true)
                        p?.WaitForExit();

                bool isRunning;
                do
                {
                    isRunning = ProcessEx.IsRunning(appPath);
                    Thread.Sleep(200);
                }
                while (isRunning);

                for (var i = 0; i < dataDirMap.GetLength(0); i++)
                    Data.DirUnLink(dataDirMap[i, 0], true);
            }
        }
    }
}
