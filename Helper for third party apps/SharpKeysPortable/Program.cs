namespace SharpKeysPortable
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.Win32;
    using Portable;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            using (new Mutex(true, ProcessEx.CurrentName, out bool newInstance))
            {
                if (!newInstance)
                    return;

                var appPath = PathEx.Combine(PathEx.LocalDir, "App\\SharpKeys\\SharpKeys.exe");
                if (!File.Exists(appPath))
                    return;

                const string firstKey = "HKCU\\Software\\RandyRants";
                const string lastKey = "HKCU\\Software\\RandyRants\\SharpKeys";

                Helper.RegForwarding(Helper.Options.Start, lastKey);
                Reg.Write(lastKey, "ShowWarning", 1, RegistryValueKind.DWord);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.RegForwarding(Helper.Options.Exit, firstKey);
                if (!Reg.GetSubKeys(firstKey).Any())
                    Reg.RemoveSubKey(firstKey);
            }
        }
    }
}
