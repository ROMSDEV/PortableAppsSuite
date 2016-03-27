using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace WinSCP_PuTTY_Launcher
{
    public static class Main
    {
        public static string PuTTYPath { get; set; }

        public static string PuTTYExePath { get; set; }

        public static string WinSCPPath { get; set; }

        public static string WinSCPExePath { get; set; }

        public static Dictionary<int, IntPtr> PuTTYTabs = new Dictionary<int, IntPtr>();

        public static IntPtr PuTTYWnd = IntPtr.Zero;

        public static void SetPortableDirs()
        {
            try
            {
                string checkPath = Application.StartupPath;
                string portableAppsPath = Path.GetFullPath($@"{Application.StartupPath}\..\..");
                for (int i = 0; i < 2; i++)
                {
                    if (Directory.Exists(checkPath))
                    {
                        if (!Directory.Exists(PuTTYPath) && Directory.Exists(portableAppsPath) && checkPath == Application.StartupPath)
                        {
                            string file = Path.Combine(portableAppsPath, @"PuTTYPortable\PuTTYPortable.exe");
                            if (File.Exists(file))
                            {
                                PuTTYExePath = file;
                                PuTTYPath = Path.GetDirectoryName(PuTTYExePath);
                            }
                        }
                        if (!Directory.Exists(PuTTYPath))
                        {
                            foreach (string file in Directory.GetFiles(checkPath, "PuTTYPortable.exe", SearchOption.AllDirectories))
                            {
                                PuTTYExePath = file;
                                PuTTYPath = Path.GetDirectoryName(PuTTYExePath);
                                break;
                            }
                        }
                        if (!Directory.Exists(WinSCPPath) && Directory.Exists(portableAppsPath) && checkPath == Application.StartupPath)
                        {
                            string file = Path.Combine(portableAppsPath, @"WinSCPPortable\WinSCPPortable.exe");
                            if (File.Exists(file))
                            {
                                WinSCPExePath = file;
                                WinSCPPath = Path.GetDirectoryName(WinSCPExePath);
                            }
                        }
                        if (!Directory.Exists(WinSCPPath))
                        {
                            foreach (string file in Directory.GetFiles(checkPath, "WinSCPPortable.exe", SearchOption.AllDirectories))
                            {
                                WinSCPExePath = file;
                                WinSCPPath = Path.GetDirectoryName(WinSCPExePath);
                                break;
                            }
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(PuTTYPath) && !string.IsNullOrWhiteSpace(WinSCPPath))
                        break;
                    checkPath = Path.GetPathRoot(Application.StartupPath);
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            if (!Directory.Exists(WinSCPPath))
            {
                MessageBox.Show(string.Concat(new string[] 
                {
                    "The following files are missed:",
                    Environment.NewLine,
                    Environment.NewLine,
                    "- PuTTYPortable.exe",
                    Environment.NewLine,
                    "- WinSCPPortable.exe",
                    Environment.NewLine,
                    Environment.NewLine,
                    Environment.NewLine,
                    "Hint:",
                    Environment.NewLine,
                    Environment.NewLine,
                    "If you're using the Portable Apps Launcher you have to add 'WinSCP' and 'Putty' simply.",
                    Environment.NewLine,
                    Environment.NewLine,
                    "(Otherwise please check out 'http://PortableApps.com/Apps')"
                }), "WinSCP + PuTTY Launcher", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
    }
}