namespace UniversalLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
            Log.AllowLogging();

            var ixiPath = PathEx.Combine(PathEx.LocalDir, $"App\\{ProcessEx.CurrentName}.ixi");
            var iniPath = PathEx.Combine(PathEx.LocalDir, $"App\\{ProcessEx.CurrentName}.ini");
            if (!File.Exists(iniPath))
                return;
            Ini.SetFile(iniPath);
            if (File.Exists(ixiPath))
                Ini.FilePath = iniPath;
            else
            {
                Ini.ReadAll();
                Ini.SaveCache(ixiPath, iniPath);
            }

            var appPath = PathEx.Combine(Ini.Read("Root", "AppPath"));
            if (string.IsNullOrEmpty(appPath) || !File.Exists(appPath))
                return;

            var instanceKey = Path.GetFileName(appPath).ToLower().GetHashCode().ToString();
            bool newInstance;
            using (new Mutex(true, instanceKey, out newInstance))
            {
#if x86
                var architecture = Ini.Read("Root", "Architecture").EqualsEx("x64");
                if (!Environment.Is64BitOperatingSystem && architecture)
                    return;
                architecture = Ini.Read("Root", "Architecture").EqualsEx("Current", "Now", "x64");
                if (Environment.Is64BitOperatingSystem && architecture)
                {
                    var name = string.Concat(Path.GetFileNameWithoutExtension(PathEx.LocalPath), "64.exe");
                    var path = PathEx.Combine(PathEx.LocalDir, name);
                    ProcessEx.Start(path);
                    Environment.ExitCode = 0;
                    Environment.Exit(Environment.ExitCode);
                }
#endif

                if (!Elevation.IsAdministrator && Ini.Read("Root", "Elevated", false))
                    Elevation.RestartAsAdministrator();

                var cmdLine = EnvironmentEx.CommandLine(Ini.Read("Root", "SortCmdLine", false)).Trim();
                cmdLine = string.Format(Ini.Read<string>("Root", "CmdLine", "{0}"), cmdLine).Trim();

                if (!newInstance)
                {
                    var pName = Process.GetCurrentProcess().ProcessName;
                    var pHandle = Process.GetCurrentProcess().Handle;
                    var pCount = Process.GetProcessesByName(pName).Where(p => p.Handle != pHandle).ToArray().Length;
                    if (pCount > 0)
                    {
                        if (Ini.Read("Root", "MultiInstances", false))
                            ProcessEx.Start(appPath, cmdLine, Elevation.IsAdministrator);
                        return;
                    }
                }
                var iniOwCount = Ini.Read("IniOverwrite", "Count", 0);
                if (iniOwCount > 0)
                {
                    var iniMap = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                    for (var i = 0; i < iniOwCount; i++)
                    {
                        var file = PathEx.Combine(Ini.Read("IniOverwrite", $"File_{i}"));
                        if (string.IsNullOrWhiteSpace(file))
                            continue;
                        var count = Ini.Read("IniOverwrite", $"Count_{i}", 0);
                        if (count <= 0)
                            continue;
                        for (var j = 0; j < count; j++)
                        {
                            var section = Ini.Read("IniOverwrite", $"Section_{i}_{j}");
                            if (string.IsNullOrWhiteSpace(section))
                                continue;
                            var key = Ini.Read("IniOverwrite", $"Key_{i}_{j}");
                            if (string.IsNullOrWhiteSpace(key))
                                continue;
                            var value = Ini.Read("IniOverwrite", $"Value_{i}_{j}");
                            if (string.IsNullOrWhiteSpace(value))
                                continue;
                            var isPath = Ini.Read("IniOverwrite", $"IsPath_{i}_{j}", false);
                            if (isPath)
                                value = PathEx.Combine(value);
                            if (!iniMap.ContainsKey(file))
                                iniMap[file] = new Dictionary<string, Dictionary<string, string>>();
                            if (!iniMap.ContainsKey(section))
                                iniMap[file][section] = new Dictionary<string, string>();
                            iniMap[file][section][key] = value;
                        }
                    }
                    foreach (var file in iniMap.Keys)
                        Helper.ConfigOverwrite(iniMap[file], file);
                }

                var dirMap = new Dictionary<string, string>();
                var dirCount = Ini.Read("DirRouting", "Count", 0);
                if (dirCount > 0)
                {
                    for (var i = 0; i < dirCount; i++)
                    {
                        var srcPath = PathEx.Combine(Ini.Read("DirRouting", $"Source_{i}"));
                        if (string.IsNullOrWhiteSpace(srcPath))
                            continue;
                        var destPath = PathEx.Combine(Ini.Read("DirRouting", $"Destination_{i}"));
                        if (string.IsNullOrWhiteSpace(destPath))
                            continue;
                        dirMap.Add(srcPath, destPath);
                    }
                    Helper.DirectoryForwarding(Helper.Options.Start, dirMap);
                }

                var fileMap = new Dictionary<string, string>();
                var fileCount = Ini.Read("FileRouting", "Count", 0);
                if (fileCount > 0)
                {
                    for (var i = 0; i < fileCount; i++)
                    {
                        var srcPath = PathEx.Combine(Ini.Read("FileRouting", $"Source_{i}"));
                        if (string.IsNullOrWhiteSpace(srcPath))
                            continue;
                        var destPath = PathEx.Combine(Ini.Read("FileRouting", $"Destination_{i}"));
                        if (string.IsNullOrWhiteSpace(destPath))
                            continue;
                        fileMap.Add(srcPath, destPath);
                    }
                    Helper.FileForwarding(Helper.Options.Start, fileMap);
                }

                var regKeys = new List<string>();
                var regCount = Ini.Read("RegRouting", "Count", 0);
                if (regCount > 0)
                {
                    for (var i = 0; i < regCount; i++)
                        regKeys.Add(Ini.Read("RegRouting", $"Key_{i}"));
                    regKeys = regKeys.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                }
                Helper.RegForwarding(Helper.Options.Start, regKeys.ToArray());

                var regOwCount = Ini.Read("RegOverwrite", "Count", 0);
                if (regKeys.Count > 0 && regOwCount > 0)
                    for (var i = 0; i < regOwCount; i++)
                    {
                        var key = Ini.Read("RegOverwrite", $"Key_{i}");
                        if (string.IsNullOrWhiteSpace(key))
                            continue;
                        var count = Ini.Read("RegOverwrite", $"Count_{i}", 0);
                        if (count <= 0)
                            continue;
                        for (var j = 0; j < count; j++)
                        {
                            var entry = Ini.Read("RegOverwrite", $"Entry_{i}_{j}");
                            if (string.IsNullOrWhiteSpace(entry))
                                continue;
                            var value = Ini.Read("RegOverwrite", $"Value_{i}_{j}");
                            if (string.IsNullOrWhiteSpace(value))
                                continue;
                            RegistryValueKind kind;
                            switch (Ini.Read("RegOverwrite", $"Kind_{i}_{j}")?.ToUpper())
                            {
                                case "BINARY":
                                    kind = RegistryValueKind.Binary;
                                    break;
                                case "DWORD":
                                    kind = RegistryValueKind.DWord;
                                    break;
                                case "EXPANDSTRING":
                                    kind = RegistryValueKind.ExpandString;
                                    break;
                                case "MULTISTRING":
                                    kind = RegistryValueKind.MultiString;
                                    break;
                                case "QWORD":
                                    kind = RegistryValueKind.QWord;
                                    break;
                                case "STRING":
                                    kind = RegistryValueKind.String;
                                    break;
                                case "UNKNOWN":
                                    kind = RegistryValueKind.Unknown;
                                    break;
                                default:
                                    kind = RegistryValueKind.None;
                                    break;
                            }
                            if (kind == RegistryValueKind.String)
                            {
                                var path = PathEx.Combine(value);
                                if (PathEx.DirOrFileExists(path))
                                    value = path;
                            }
                            Reg.Write(key, entry, value, kind);
                        }
                    }

                Helper.ApplicationStart(appPath, cmdLine);

                var clearOnlyDirs = Ini.Read("ClearOnly", "Dirs");
                if (!string.IsNullOrWhiteSpace(clearOnlyDirs))
                {
                    if (!clearOnlyDirs.EndsWith("|"))
                        clearOnlyDirs += "|";
                    var dirs = clearOnlyDirs.Split('|').Select(s => PathEx.Combine(s));
                    foreach (var dir in dirs)
                        try
                        {
                            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                                continue;
                            Directory.Delete(dir, true);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                }

                var clearOnlyFiles = Ini.Read("ClearOnly", "Files");
                if (!string.IsNullOrWhiteSpace(clearOnlyFiles))
                {
                    if (!clearOnlyFiles.EndsWith("|"))
                        clearOnlyFiles += "|";
                    var files = clearOnlyFiles.Split('|').Select(s => PathEx.Combine(s));
                    foreach (var file in files)
                        try
                        {
                            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                                continue;
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                }

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);
                Helper.FileForwarding(Helper.Options.Exit, fileMap);
                Helper.RegForwarding(Helper.Options.Exit, regKeys.ToArray());
            }
        }
    }
}
