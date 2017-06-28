namespace FFactoryPortable
{
    using System;
#if !Legacy
    using System.Collections.Generic;
    using System.Diagnostics;
#endif
    using System.IO;
    using System.Threading;
    using Microsoft.Win32;
    using Portable;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            using (new Mutex(true, ProcessEx.CurrentName, out bool newInstance))
            {
                if (!newInstance)
                    return;

                var appPath = PathEx.Combine("%CurDir%\\App\\FormatFactory\\FormatFactory.exe");
                if (!File.Exists(appPath) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)))
                    return;

                var regKeys = new[]
                {
                    "HKCU\\Software\\FreeTime",
                    "HKCU\\Software\\FreeTime\\FormatFactory"
                };

                Helper.RegForwarding(Helper.Options.Start, regKeys[0]);

                Reg.Write(regKeys[0], "FormatFactory", Path.GetDirectoryName(appPath), RegistryValueKind.String);
                Reg.Write(regKeys[1], "CheckNewVersion", 0, RegistryValueKind.DWord);
                Reg.Write(regKeys[1], "CodecInstalled", 0, RegistryValueKind.DWord);
                Reg.Write(regKeys[1], "OptionActivePage", 0, RegistryValueKind.DWord);
                Reg.Write(regKeys[1], "OutputDir", PathEx.Combine("%CurDir%\\Data\\Output"), RegistryValueKind.String);
                Reg.Write(regKeys[1], "UseCount", 1, RegistryValueKind.DWord);
#if Legacy
                Reg.Write(regKeys[1], "Skin", 2, RegistryValueKind.DWord);
                Reg.Write(regKeys[1], "Version", "3.3.5", RegistryValueKind.String);
#else
                Reg.Write(regKeys[1], "Skin", 6, RegistryValueKind.DWord);
                Reg.Write(regKeys[1], "StartMethodTab", 0, RegistryValueKind.DWord);
                try
                {
                    var fvi = FileVersionInfo.GetVersionInfo(appPath);
                    var str = new List<string>(fvi.ProductVersion.Split('.')).GetRange(0, 3).Join('.');
                    Reg.Write(regKeys[1], "Version", str, RegistryValueKind.String);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
#endif

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.RegForwarding(Helper.Options.Exit, regKeys[0]);
            }
        }
    }
}
