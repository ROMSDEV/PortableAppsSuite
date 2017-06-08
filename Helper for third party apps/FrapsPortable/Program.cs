namespace FrapsPortable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Portable;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
#if x86
            var appPath64 = PathEx.Combine(@"%CurDir%\Fraps64Portable.exe");
            if (Environment.Is64BitOperatingSystem && File.Exists(appPath64))
            {
                ProcessEx.Start(appPath64);
                Environment.Exit(Environment.ExitCode);
            }
#endif
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                if (!newInstance)
                    return;

                var appDir = PathEx.Combine(PathEx.LocalDir, "App\\Fraps");
                var appPath = PathEx.Combine(appDir, "fraps.exe");
                if (!File.Exists(appPath) || ProcessEx.IsRunning(Path.GetFileNameWithoutExtension(appPath)))
                    return;

                var regKeys = new[]
                {
                    "HKCU\\SOFTWARE\\Fraps3",
#if x86
                    "HKLM\\SOFTWARE\\Fraps"
#else
                    "HKLM\\SOFTWARE\\Wow6432Node\\Fraps"
#endif
                };

                var regSecureMap = new Dictionary<string, Dictionary<string, string>>
                {
                    {
                        "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Drivers32", new Dictionary<string, string>
                        {
                            {
                                "VIDC.FPS1",
                                "\"frapsv64.dll\""
                            }
                        }
                    },
                    {
                        "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\drivers.desc", new Dictionary<string, string>
                        {
                            {
                                "frapsv64.dll",
                                "\"Fraps Video Decompressor\""
                            }
                        }
                    },
                    {
                        "HKLM\\SYSTEM\\CurrentControlSet\\Control\\MediaResources\\icm\\VIDC.FPS1", new Dictionary<string, string>
                        {
                            {
                                "Description",
                                "\"Fraps Video Decompressor\""
                            },
                            {
                                "Driver",
                                "\"frapsvid.dll\""
                            }
                        }
                    },
                    {
#if x86
                        "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Fraps", new Dictionary<string, string>
#else
                        "HKLM\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Fraps", new Dictionary<string, string>
#endif
                            {
                                {
                                    "DisplayName",
                                    "\"Fraps (remove only)\""
                                },
                                {
                                    "NsisInst",
                                    "hex:d9,10,ca,32,3e,1b,08,44,ca,79,61,a3,f0,7a,39,18,ba,6f,27,e0"
                                },
                                {
                                    "NsisSM",
                                    "\"Fraps\""
                                },
                                {
                                    "UninstallString",
                                    "\"\\\"C:\\\\Fraps\\\\uninstall.exe\\\"\""
                                }
                            }
                    }
                };

                var fileSecureMap = new Dictionary<string, string>
                {
                    {
                        "%CurDir%\\App\\Driver\\SysWOW64\\frapsvid.dll",
#if x86
                        "%WinDir%\\System32\\frapsvid.dll"
#else
                        "%WinDir%\\SysWOW64\\frapsvid.dll"
#endif
                    }
#if !x86
                    ,
                    {
                        "%CurDir%\\App\\Driver\\System32\\frapsv64.dll",
                        "%WinDir%\\System32\\frapsv64.dll"
                    }
#endif
                };

                var dataDir = PathEx.Combine(PathEx.LocalDir, "Data");
                var outDir = PathEx.Combine(dataDir, "Output");
                if (!Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);

                Helper.RegForwarding(Helper.Options.Start, regKeys);

                var firstKey = regKeys.FirstOrDefault();
                if (!string.IsNullOrEmpty(firstKey))
                {
                    Reg.Write(firstKey, "Directory", appDir);
                    var entries = new[]
                    {
                        "Benchmark Directory",
                        "Movie Directory",
                        "Screenshot Directory"
                    };
                    foreach (var entry in entries)
                    {
                        var current = Reg.ReadString(firstKey, entry);
                        if (string.IsNullOrEmpty(current) || current.ContainsEx("FrapsPortable"))
                            Reg.Write(firstKey, entry, outDir);
                    }
                }

                var lastKey = regKeys.LastOrDefault();
                if (!string.IsNullOrEmpty(lastKey))
                    Reg.Write(lastKey, "Install_Dir", appDir);

                Helper.RegSecureOverwrite(regSecureMap);

                Helper.FileSecureForwarding(Helper.Options.Start, fileSecureMap);

                Helper.ApplicationStart(appPath, EnvironmentEx.CommandLine(false), false);

                Helper.RegForwarding(Helper.Options.Exit, regKeys);

                regSecureMap = new Dictionary<string, Dictionary<string, string>>
                {
                    {
                        "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Drivers32", new Dictionary<string, string>
                        {
                            {
                                "VIDC.FPS1",
                                null
                            }
                        }
                    },
                    {
                        "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\drivers.desc", new Dictionary<string, string>
                        {
                            {
                                "frapsv64.dll",
                                null
                            }
                        }
                    },
                    {
                        "-HKLM\\SYSTEM\\CurrentControlSet\\Control\\MediaResources\\icm\\VIDC.FPS1", null
                    },
                    {
#if x86
                        "-HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Fraps", null
#else
                        "-HKLM\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Fraps", null
#endif
                    }
                };

                Helper.RegSecureOverwrite(regSecureMap);

                Helper.FileSecureForwarding(Helper.Options.Exit, fileSecureMap);
            }
        }
    }
}
