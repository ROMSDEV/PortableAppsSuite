namespace HexEditorMXPortable
{
    using System;
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
                var appPath = PathEx.Combine(PathEx.LocalDir, "App\\hexeditmx\\hexeditmx.exe");
                if (!File.Exists(appPath))
                    return;

                if (!newInstance)
                {
                    ProcessEx.Start(appPath, EnvironmentEx.CommandLine());
                    return;
                }

                if (ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)))
                    return;

                var regKeys = new[]
                {
                    "HKCU\\Software\\NEXT-Soft"
                };

                Helper.RegForwarding(Helper.Options.Start, regKeys);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(), false);

                Helper.RegForwarding(Helper.Options.Exit, regKeys);
            }
        }
    }
}
