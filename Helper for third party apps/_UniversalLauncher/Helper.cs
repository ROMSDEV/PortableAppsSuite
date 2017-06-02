namespace Portable
{
#if ApplicationStart || ConfigOverwrite || DirectoryForwarding || FileForwarding || RegistryForwarding
    using System;
#if ConfigOverwrite || DirectoryForwarding || FileForwarding
    using System.Collections.Generic;
#endif
    using System.IO;
    using System.Linq;
#if ApplicationStart
    using System.Threading;
#endif
    using SilDev;

#endif

    public static class Helper
    {
#if DirectoryForwarding || FileForwarding || RegistryForwarding
        public enum Options
        {
            Start,
            Exit
        }
#endif

#if ApplicationStart
        public static void ApplicationStart(string filePath, string commandLine = null, bool? full = true)
        {
            var path = PathEx.Combine(filePath);
            if (!File.Exists(path))
                return;
            using (var p = ProcessEx.Start(path, commandLine, Elevation.IsAdministrator, false))
                if (p?.HasExited == false)
                    p.WaitForExit();
            if (full == null)
                return;
            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                return;
            Recheck:
            foreach (var file in Directory.EnumerateFiles(dir, "*.exe", SearchOption.AllDirectories))
            {
                var wasRunning = false;
                bool isRunning;
                do
                {
                    isRunning = ProcessEx.IsRunning(full == true ? file : Path.GetFileName(file));
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
        }
#endif

#if ConfigOverwrite
        public static void ConfigOverwrite(Dictionary<string, Dictionary<string, string>> iniMap, string path)
        {
            if (iniMap?.Any() != true)
                return;
            var file = PathEx.Combine(path);
            try
            {
                var dir = Path.GetDirectoryName(file);
                if (string.IsNullOrEmpty(dir))
                    throw new ArgumentNullException(dir);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (!File.Exists(file))
                    File.Create(file).Close();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return;
            }
            foreach (var data in iniMap)
            {
                var section = data.Key;
                foreach (var pair in iniMap[section])
                {
                    var key = pair.Key;
                    var value = pair.Value;
                    Ini.WriteDirect(section, key, value, path);
                }
            }
        }
#endif

#if DirectoryForwarding
        public static void DirectoryForwarding(Options option, Dictionary<string, string> dirMap)
        {
            if (dirMap?.Any() != true)
                return;
            foreach (var data in dirMap)
            {
                if (string.IsNullOrWhiteSpace(data.Key) || string.IsNullOrWhiteSpace(data.Value))
                    continue;
                var srcPath = PathEx.Combine(data.Key);
                var destPath = PathEx.Combine(data.Value);
                Data.SetAttributes(srcPath, FileAttributes.Normal);
                var backupPath = srcPath + ".SI13N7-BACKUP";
                switch (option)
                {
                    case Options.Exit:
                        Data.SetAttributes(backupPath, FileAttributes.Normal);
                        if (Data.DirUnLink(srcPath, true))
                            continue;
                        try
                        {
                            if (Directory.Exists(srcPath))
                            {
                                Data.SetAttributes(srcPath, FileAttributes.Normal);
                                Data.DirCopy(srcPath, destPath, true, true);
                                Directory.Delete(srcPath, true);
                            }
                            if (Directory.Exists(backupPath))
                            {
                                Data.DirCopy(backupPath, srcPath, true, true);
                                Directory.Delete(backupPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                        break;
                    default:
                        if (Data.DirLink(srcPath, destPath, true))
                        {
                            Data.SetAttributes(backupPath, FileAttributes.Normal);
                            continue;
                        }
                        try
                        {
                            if (Directory.Exists(srcPath))
                            {
                                if (!Directory.Exists(backupPath))
                                {
                                    Data.DirSafeMove(srcPath, backupPath);
                                    Data.SetAttributes(backupPath, FileAttributes.Hidden);
                                }
                                if (Directory.Exists(srcPath))
                                    Directory.Delete(srcPath, true);
                            }
                            if (Directory.Exists(destPath))
                            {
                                Data.SetAttributes(destPath, FileAttributes.Normal);
                                Data.DirCopy(destPath, srcPath);
                            }
                            if (!Directory.Exists(srcPath))
                                Directory.CreateDirectory(srcPath);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                        break;
                }
            }
        }
#endif

#if FileForwarding
        public static void FileForwarding(Options option, Dictionary<string, string> fileMap, bool simple = false)
        {
            if (fileMap?.Any() != true)
                return;
            foreach (var data in fileMap)
            {
                if (string.IsNullOrWhiteSpace(data.Key) || string.IsNullOrWhiteSpace(data.Value))
                    continue;
                var srcPath = PathEx.Combine(data.Key);
                var destPath = PathEx.Combine(data.Value);
                try
                {
                    if (!File.Exists(destPath))
                    {
                        var destDir = Path.GetDirectoryName(destPath);
                        if (string.IsNullOrEmpty(destDir))
                            throw new ArgumentNullException(nameof(destDir));
                        if (!Directory.Exists(destDir))
                            Directory.CreateDirectory(destDir);
                        File.Create(destPath).Close();
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    continue;
                }
                var backupPath = srcPath + ".SI13N7-BACKUP";
                switch (option)
                {
                    case Options.Exit:
                        if (simple)
                        {
                            try
                            {
                                if (File.Exists(srcPath))
                                    File.Copy(srcPath, destPath, true);
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }
                            continue;
                        }
                        Data.SetAttributes(backupPath, FileAttributes.Normal);
                        if (Elevation.IsAdministrator && Data.FileUnLink(srcPath, true))
                            continue;
                        try
                        {
                            if (File.Exists(srcPath))
                            {
                                Data.SetAttributes(srcPath, FileAttributes.Normal);
                                File.Copy(srcPath, destPath, true);
                                File.Delete(srcPath);
                            }
                            if (File.Exists(backupPath))
                            {
                                File.Copy(backupPath, srcPath, true);
                                File.Delete(backupPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                        break;
                    default:
                        if (simple)
                        {
                            try
                            {
                                if (File.Exists(destPath) && (!File.Exists(srcPath) || File.GetLastWriteTime(destPath) > File.GetLastWriteTime(srcPath)))
                                    File.Copy(destPath, srcPath, true);
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }
                            continue;
                        }
                        if (Elevation.IsAdministrator && Data.FileLink(srcPath, destPath, true))
                        {
                            Data.SetAttributes(backupPath, FileAttributes.Hidden);
                            continue;
                        }
                        try
                        {
                            if (File.Exists(srcPath))
                            {
                                if (!File.Exists(backupPath))
                                {
                                    File.Copy(srcPath, backupPath);
                                    Data.SetAttributes(backupPath, FileAttributes.Hidden);
                                }
                                File.Delete(srcPath);
                            }
                            if (!File.Exists(destPath))
                            {
                                var dir = Path.GetDirectoryName(destPath);
                                if (string.IsNullOrEmpty(dir))
                                    continue;
                                if (!Directory.Exists(dir))
                                    Directory.CreateDirectory(dir);
                                File.Create(destPath).Close();
                            }
                            Data.SetAttributes(backupPath, FileAttributes.Normal);
                            File.Copy(destPath, srcPath, true);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                        break;
                }
            }
        }
#endif

#if RegistryForwarding
        public static void RegForwarding(Options option, params string[] keys)
        {
            if (keys == null || keys.Length == 0)
                return;
            var backup = PathEx.Combine($"%TEMP%\\backup-{{{Math.Abs(PathEx.LocalPath.GetHashCode())}}}.reg");
            var file = PathEx.Combine(PathEx.LocalDir, "Data\\settings.reg");
            switch (option)
            {
                case Options.Exit:
                    if (keys.Length > 0)
                    {
                        Reg.ExportKeys(file, keys);
                        foreach (var key in keys)
                            Reg.RemoveSubKey(key);
                    }
                    if (!File.Exists(backup))
                        return;
                    Reg.ImportFile(backup);
                    File.Delete(backup);
                    break;
                default:
                    if (!keys.Any(x => Reg.EntryExists(x, "Portable App")))
                    {
                        if (!File.Exists(backup))
                            Reg.ExportKeys(backup, keys);
                        foreach (var key in keys)
                            Reg.RemoveSubKey(key);
                    }
                    foreach (var key in keys)
                        Reg.Write(key, "Portable App", "True");
                    if (File.Exists(file))
                        Reg.ImportFile(file);
                    break;
            }
        }
#endif
    }
}
