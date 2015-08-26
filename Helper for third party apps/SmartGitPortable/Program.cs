using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SmartGitPortable
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool newInstance = true;
            using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
            {
                if (newInstance)
                {
                    string appDir = Path.Combine(Application.StartupPath, "App\\SmartGit");
                    string dataPath = Path.Combine(Application.StartupPath, "Data");
                    string appDataPath = SilDev.Run.EnvironmentVariableFilter("%AppData%\\syntevo\\SmartGit");
                    DirLinkHelper(appDataPath, dataPath);
                    string dataCfgPath = Path.Combine(Application.StartupPath, "Data\\.gitconfig");
                    string cfgPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), ".gitconfig");
                    if (File.Exists(dataCfgPath))
                    {
                        if (File.Exists(cfgPath))
                        {
                            File.Move(cfgPath, string.Format("{0}.SI13N7-BACKUP", cfgPath));
                            File.Delete(cfgPath);
                        }
                        File.Move(dataCfgPath, cfgPath);
                        SilDev.Data.SetAttributes(cfgPath, SilDev.Data.Attrib.Hidden);
                    }
                    SilDev.Reg.ImportFile(string.Format(SilDev.Crypt.Base64.Decrypt("V2luZG93cyBSZWdpc3RyeSBFZGl0b3IgVmVyc2lvbiA1LjAwDQoNCltIS0VZX0NVUlJFTlRfVVNFUlxTb2Z0d2FyZVxlai10ZWNobm9sb2dpZXNcZXhlNGpcanZtc1x7MH0vanJlL2Jpbi9qYXZhLmV4ZV0NCiJMYXN0V3JpdGVUaW1lIj1oZXg6MDAsZjgsYmMsMjQsMzksYTgsY2YsMDENCiJWZXJzaW9uIj0iezF9Ig0KDQo="), appDir.ToLower().Replace("\\", "/"), "1.7.0_67").Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
                    SilDev.Run.App(appDir, "bin\\smartgit.exe", 0);
                    while (isRunning(appDir, "smartgit"))
                        Thread.Sleep(200);
                    SilDev.Reg.RemoveExistSubKey(SilDev.Reg.RegKey.CurrentUser, string.Format("Software\\ej-technologies\\exe4j\\jvms\\{0}", Path.Combine(appDir, "jre\\bin\\java.exe").ToLower().Replace("\\", "/")));
                    DirUnLinkHelper(appDataPath);
                    appDataPath = Path.GetFullPath(string.Format("{0}\\..", appDataPath));
                    if (Directory.GetFiles(appDataPath, "*", SearchOption.AllDirectories).Length == 0)
                        Directory.Delete(appDataPath, true);
                    if (File.Exists(cfgPath))
                    {
                        if (File.Exists(dataCfgPath))
                            File.Delete(dataCfgPath);
                        File.Move(cfgPath, dataCfgPath);
                        SilDev.Data.SetAttributes(dataCfgPath, SilDev.Data.Attrib.Normal);
                    }
                    if (File.Exists(string.Format("{0}.SI13N7-BACKUP", cfgPath)))
                    {
                        if (File.Exists(cfgPath))
                            File.Delete(cfgPath);
                        File.Move(string.Format("{0}.SI13N7-BACKUP", cfgPath), cfgPath);
                    }
                }
            }
        }

        static bool isRunning(string _path, string _match)
        {
            try
            {
                foreach (string f in Directory.GetFiles(_path, "*.exe", SearchOption.AllDirectories))
                {
                    string name = Path.GetFileNameWithoutExtension(f).ToLower();
                    if (name.Contains(_match.ToLower()) && Process.GetProcessesByName(name).Length > 0)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex.Message, "SteamPortable.Program.isRunning");
                return false;
            }
        }

        static void killAll(string _path, string _match)
        {
            try
            {
                foreach (string f in Directory.GetFiles(_path, "*.exe", SearchOption.AllDirectories))
                {
                    string name = Path.GetFileNameWithoutExtension(f).ToLower();
                    if (!name.Contains(_match.ToLower()))
                        continue;
                    foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(f)))
                    {
                        p.CloseMainWindow();
                        p.WaitForExit(100);
                        if (!p.HasExited)
                            p.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex.Message, "SteamPortable.Program.killAll");
            }
        }

        static void DirLinkHelper(string _src, string _dest)
        {
            if (!Directory.Exists(_dest))
                Directory.CreateDirectory(_dest);
            if (Directory.Exists(_dest))
            {
                if (Directory.Exists(_src))
                {
                    if (!SilDev.Data.DirIsLink(_src))
                    {
                        Directory.Move(_src, string.Format("{0}.SI13N7-BACKUP", _src));
                        if (Directory.Exists(_src))
                            Directory.Delete(_src, true);
                    }
                    else
                        SilDev.Data.DirUnLink(_src);
                }
                SilDev.Data.DirLink(_dest, _src);
            }
        }

        static void DirUnLinkHelper(string _path)
        {
            if (Directory.Exists(string.Format("{0}.SI13N7-BACKUP", _path)))
            {
                if (Directory.Exists(_path))
                    Directory.Delete(_path, true);
                Directory.Move(string.Format("{0}.SI13N7-BACKUP", _path), _path);
            }
            SilDev.Data.DirUnLink(_path);
        }
    }
}
