namespace HLSWPortable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
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

                var appPath = PathEx.Combine(PathEx.LocalDir, "App\\HLSW\\hlsw.exe");
                if (!File.Exists(appPath) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)))
                    return;

                try
                {
                    string unpachedAppPath = $"{appPath}.SI13N7-BACKUP";
                    if (!File.Exists(unpachedAppPath) && Crypto.EncryptFileToMd5(appPath).EqualsEx("1715a84deb7c6c3f35dc8acd96833ef9"))
                    {
                        File.Copy(appPath, unpachedAppPath);
                        using (var bw = new BinaryWriter(File.Open(appPath, FileMode.Open)))
                        {
                            bw.BaseStream.Position = 0x439654L;
                            bw.Write(Encoding.ASCII.GetBytes("..\\..\\Data\\Cfg"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                if (!Crypto.EncryptFileToMd5(appPath).EqualsEx("8f4fcb7555b8a5cab46ca99750346321"))
                    return;

                Helper.RedistHandling(Helper.Options.Start, EnvironmentEx.Redist.Flags.VC2008X86);

                var regKeys = new[]
                {
                    "HKCU\\Software\\HLSW",
                    "HKCU\\Software\\HLSW\\Management"
                };

                var dataPath = PathEx.Combine(PathEx.LocalDir, "Data\\Cfg");
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);

                var firstKey = regKeys.FirstOrDefault();
                if (!string.IsNullOrEmpty(firstKey))
                    Helper.RegForwarding(Helper.Options.Start, firstKey);

                var lastKey = regKeys.LastOrDefault();
                if (!string.IsNullOrEmpty(lastKey))
                {
                    Reg.Write(lastKey, "AutoLogin", 1, RegistryValueKind.DWord);
                    Reg.Write(lastKey, "LoginOnStartup", 0, RegistryValueKind.DWord);
                    Reg.Write(lastKey, "Offline", 1, RegistryValueKind.DWord);
                    Reg.Write(lastKey, "Offline2", 1, RegistryValueKind.DWord);
                }

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                if (!string.IsNullOrEmpty(firstKey))
                    Helper.RegForwarding(Helper.Options.Exit, firstKey);

                var regSecureMap = new Dictionary<string, Dictionary<string, string>>
                {
                    { "[-HKCR\\.sslf]", null },
                    { "[-HKCR\\hlsw]", null },
                    { "[-HKCR\\HLSW Server List]", null }
                };
                for (var i = 0; i < 34; i++)
                    regSecureMap.Add($"[-HKCR\\.sl{(i < 10 ? $"0{i}" : i.ToString())}]", null);

                if (regSecureMap.Keys.Any(Reg.SubKeyExists))
                    Helper.RegSecureOverwrite(regSecureMap, true);

                Helper.RedistHandling(Helper.Options.Exit, EnvironmentEx.Redist.Flags.VC2008X86);
            }
        }
    }
}
