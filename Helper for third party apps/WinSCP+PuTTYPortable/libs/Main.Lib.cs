using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WinSCP_PuTTY_Launcher
{
    class Main
    {
        private static string _puttyPath;

        public static string PuTTYPath
        {
            get { return _puttyPath; }
            set { _puttyPath = value; }
        }

        private static string _puttyExePath;

        public static string PuTTYExePath
        {
            get { return _puttyExePath; }
            set { _puttyExePath = value; }
        }

        private static string _winscpPath;

        public static string WinSCPPath
        {
            get { return _winscpPath; }
            set { _winscpPath = value; }
        }

        private static string _winscpExePath;

        public static string WinSCPExePath
        {
            get { return _winscpExePath; }
            set { _winscpExePath = value; }
        }

        private static Dictionary<int, IntPtr> _puttyTabs = new Dictionary<int, IntPtr>();

        public static Dictionary<int, IntPtr> PuTTYTabs
        {
            get { return _puttyTabs; }
            set { _puttyTabs = value; }
        }

        private static IntPtr _puttyWnd = IntPtr.Zero;

        public static IntPtr PuTTYWnd
        {
            get { return _puttyWnd; }
            set { _puttyWnd = value; }
        }

        public static void SetPortableDirs()
        {
            try
            {
                string checkPath = Application.StartupPath;
                string portableAppsPath = Path.GetFullPath(string.Format(@"{0}\..\..", Application.StartupPath));
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
                SilDev.Log.Debug(ex.Message, "WinSCP_PuTTY_Launcher.Main.SetPortableDirs");
            }
            if (!Directory.Exists(WinSCPPath))
            {
                MessageBox.Show(string.Concat(new string[] 
                {
                    "The following files are missed:\n\n",
                    "- PuTTYPortable.exe\n",
                    "- WinSCPPortable.exe\n\n\n",
                    "Hint:\n\n",
                    "If you're using the Portable Apps Launcher you have to add 'WinSCP' and 'Putty' simply.\n\n",
                    "(Otherwise please check out 'http://PortableApps.com/Apps')"
                }), "WinSCP + PuTTY Launcher", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
    }
}