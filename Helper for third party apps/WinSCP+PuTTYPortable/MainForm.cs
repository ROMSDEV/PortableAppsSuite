using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WinSCP_PuTTY_Launcher
{
    public partial class MainForm : Form
    {
        static bool CancelExit = true;
        static bool IsResizing = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SilDev.Initialization.File(Application.StartupPath, "WinSCP+PuTTYPortable.ini");
            if (File.Exists(SilDev.Initialization.File()))
            {
                int left = Int16.MinValue, top = Int16.MinValue, width = Int16.MinValue, height = Int16.MinValue;
                string value = SilDev.Initialization.ReadValue("Settings", "Left");
                if (!string.IsNullOrWhiteSpace(value))
                    int.TryParse(value, out left);
                value = SilDev.Initialization.ReadValue("Settings", "Top");
                if (!string.IsNullOrWhiteSpace(value))
                    int.TryParse(value, out top);
                value = SilDev.Initialization.ReadValue("Settings", "Width");
                if (!string.IsNullOrWhiteSpace(value))
                    int.TryParse(value, out width);
                value = SilDev.Initialization.ReadValue("Settings", "Height");
                if (!string.IsNullOrWhiteSpace(value)) 
                    int.TryParse(value, out height);
                bool maximized = false;
                value = SilDev.Initialization.ReadValue("Settings", "Maximized");
                if (!string.IsNullOrWhiteSpace(value))
                    bool.TryParse(value, out maximized);
                if (width >= 640 && height >= 480)
                    this.Size = new Size(width, height);
                if (left > Int16.MinValue && top > Int16.MinValue)
                {
                    Point location = new Point(left, top);
                    Rectangle workingArea = new Rectangle(Int16.MaxValue, Int16.MaxValue, Int16.MinValue, Int16.MinValue);
                    foreach (Screen screen in Screen.AllScreens)
                        workingArea = Rectangle.Union(workingArea, screen.Bounds);
                    if (location.X < (workingArea.Width - this.Size.Width) && location.X > 0 && location.Y < (workingArea.Height - this.Height) && location.Y > 0)
                    {
                        this.StartPosition = FormStartPosition.Manual;
                        this.Location = location;
                    }
                }
                if (maximized)
                    this.WindowState = FormWindowState.Maximized;
            }
            bwWorker.RunWorkerAsync();
            PuttyTabber.Enabled = true;
        }

        private void MainForm_ResizeBegin(object sender, EventArgs e)
        {
            IsResizing = true;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            foreach (IntPtr hwnd in Main.PuTTYTabs.Values)
                SilDev.WinAPI.MoveWindow(hwnd, 0, 0, puttyTabCtrl.SelectedTab.Width, puttyTabCtrl.SelectedTab.Height, true);
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            IsResizing = false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                foreach (var hwnd in Main.PuTTYTabs)
                {
                    Process p = Process.GetProcessById(hwnd.Key);
                    p.CloseMainWindow();
                    p.WaitForExit(10);
                    if (!p.HasExited)
                        p.Kill();
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex.Message, "WinSCP_PuTTY_Launcher.MainForm.MainForm_FormClosing");
            }
            Main.PuTTYTabs.Clear();
            puttyTabCtrl.TabPages.Clear();
            if (CancelExit)
            {
                if (this.ShowInTaskbar)
                    this.ShowInTaskbar = false;
                if (this.Opacity == 1f)
                    this.Opacity = 0;
                e.Cancel = CancelExit;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (this.WindowState != FormWindowState.Maximized)
                {
                    SilDev.Initialization.WriteValue("Settings", "Left", this.Left);
                    SilDev.Initialization.WriteValue("Settings", "Top", this.Top);
                    SilDev.Initialization.WriteValue("Settings", "Width", this.Width);
                    SilDev.Initialization.WriteValue("Settings", "Height", this.Height);
                }
                SilDev.Initialization.WriteValue("Settings", "Maximized", this.WindowState == FormWindowState.Maximized);
                foreach (Process p in Process.GetProcessesByName("PUTTY"))
                {
                    p.CloseMainWindow();
                    p.WaitForExit(10);
                    if (!p.HasExited)
                        p.Kill();
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex.Message, "WinSCP_PuTTY_Launcher.MainForm.MainForm_FormClosed");
            }
            SilDev.Reg.RemoveExistSubKey(@"HKEY_CURRENT_USER\Software\SimonTatham");
        }

        private void bwWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string WinSCPPortable_ini = Path.Combine(Main.WinSCPPath, "WinSCPPortable.ini");
            if (!File.Exists(WinSCPPortable_ini))
                File.Create(WinSCPPortable_ini).Close();
            if (File.Exists(WinSCPPortable_ini))
                SilDev.Initialization.WriteValue("AppInfo", "Name", "Portable", WinSCPPortable_ini);
            string WinSCPPortableSettings_ini = Path.Combine(Main.WinSCPPath, @"Data\settings\WinSCPPortableSettings.ini");
            if (File.Exists(WinSCPPortableSettings_ini))
            {
                string oldPath = SilDev.Initialization.ReadValue("WinSCPPortableSettings", "LastDrive", WinSCPPortableSettings_ini);
                if (string.IsNullOrWhiteSpace(oldPath))
                {
                    oldPath = Main.WinSCPPath.Replace(" ", "%20").Replace(@"\", "%5C");
                    SilDev.Initialization.WriteValue("WinSCPPortableSettings", "LastDrive", oldPath, WinSCPPortableSettings_ini);
                }
                string newPath = Main.WinSCPPath.Replace(" ", "%20").Replace(@"\", "%5C");
                SilDev.Initialization.WriteValue("WinSCPPortableSettings", "LastDrive", newPath, WinSCPPortableSettings_ini);
                if (oldPath != newPath)
                {
                    string settingsPath = Path.Combine(Main.WinSCPPath, @"Data\settings\winscp.ini");
                    if (File.Exists(settingsPath))
                    {
                        bool overwriteSettings = false;
                        string settingsContent = string.Empty;
                        using (StreamReader sr = new StreamReader(settingsPath))
                        {
                            settingsContent = sr.ReadToEnd();
                            sr.Dispose();
                        }
                        if (settingsContent.Contains(oldPath))
                        {
                            overwriteSettings = true;
                            settingsContent = settingsContent.Replace(oldPath, newPath);
                        }
                        if (overwriteSettings)
                        {
                            File.Delete(settingsPath);
                            File.WriteAllText(settingsPath, settingsContent);
                        }
                    }
                }
            }
            if (File.Exists(Main.PuTTYExePath))
            {
                SilDev.Run.App(WinSCP_PuTTY_Launcher.Main.PuTTYExePath);
                for (int i = 0; i < 1496; i++)
                {
                    Thread.Sleep(10);
                    if (Process.GetProcessesByName("PUTTY").Length > 0)
                    {
                        foreach (Process p in Process.GetProcessesByName("PUTTY"))
                        {
                            if (p.MainWindowTitle.ToLower().Contains("putty configuration"))
                            {
                                Main.PuTTYWnd = p.MainWindowHandle;
                                SilDev.WinAPI.HideWindow(Main.PuTTYWnd);
                                break;
                            }
                        }
                        if (Main.PuTTYWnd != IntPtr.Zero)
                            break;
                    }
                }
            }
            SilDev.Run.App(WinSCP_PuTTY_Launcher.Main.WinSCPExePath);
            for (int i = 0; i < 224; i++)
            {
                Thread.Sleep(10);
                bool winscpIsIdle = false;
                foreach (Process p in Process.GetProcessesByName("WinSCP"))
                {
                    if (p.MainWindowTitle.ToLower().Contains("winscp "))
                    {
                        winscpIsIdle = true;
                        break;
                    }
                }
                if (winscpIsIdle)
                    break;
            }
            bool isRunning = true;
            if (Main.PuTTYWnd != IntPtr.Zero)
            {
                while (isRunning)
                {
                    isRunning = Process.GetProcessesByName("WinSCP").Length > 0;
                    foreach (Process p in Process.GetProcessesByName("WinSCP"))
                        p.WaitForExit();
                }
                foreach (KeyValuePair<int, IntPtr> p in Main.PuTTYTabs)
                    Process.GetProcessById(p.Key).WaitForExit();
                SilDev.WinAPI.ShowWindow(Main.PuTTYWnd);
                foreach (Process p in Process.GetProcessesByName("PUTTY"))
                {
                    if (p.MainWindowTitle.ToLower().Contains("putty configuration"))
                    {
                        p.CloseMainWindow();
                        break;
                    }
                }
            }
        }

        private void bwWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CancelExit = false;
            this.Close();
        }

        private void PuttyTabber_Tick(object sender, EventArgs e)
        {
            foreach (Process p in Process.GetProcessesByName("PUTTY"))
            {
                if (p.MainWindowHandle == IntPtr.Zero || Main.PuTTYTabs.Keys.Contains(p.Id) || Main.PuTTYTabs.Values.Contains(p.MainWindowHandle) || p.MainWindowHandle == Main.PuTTYWnd || p.MainWindowTitle.ToLower().Contains("putty configuration"))
                    continue;
                if (!this.ShowInTaskbar)
                    this.ShowInTaskbar = true;
                if (this.Opacity != 1f)
                    this.Opacity = 1f;
                puttyTabCtrl.TabPages.Add(p.MainWindowHandle.ToString());
                puttyTabCtrl.SelectedIndex = puttyTabCtrl.TabPages.Count - 1;
                if (!Main.PuTTYTabs.ContainsKey(p.Id))
                    Main.PuTTYTabs.Add(p.Id, p.MainWindowHandle);
                SilDev.WinAPI.SetWindowLong(p.MainWindowHandle, -16, (int)(SilDev.WinAPI.GetWindowLong(p.MainWindowHandle, -16) & ~0x00020000L & ~0x00010000L));
                SilDev.WinAPI.MoveWindow(p.MainWindowHandle, 0, 0, puttyTabCtrl.SelectedTab.Width, puttyTabCtrl.SelectedTab.Height, true);
                SilDev.WinAPI.SetParent(p.MainWindowHandle, puttyTabCtrl.SelectedTab.Handle);
                if (!this.TopMost)
                    this.TopMost = true;
                Thread.Sleep(5);
                if (this.TopMost) 
                    this.TopMost = false;
            }
            int ToRemove = -1;
            foreach (var hwnd in Main.PuTTYTabs)
            {
                try
                {
                    Process.GetProcessById(hwnd.Key);
                }
                catch
                {
                    foreach (TabPage tab in puttyTabCtrl.TabPages)
                    {
                        if (tab.Text == hwnd.Value.ToString())
                        {
                            puttyTabCtrl.TabPages.Remove(tab);
                            break;
                        }
                    }
                    puttyTabCtrl.SelectedIndex = puttyTabCtrl.TabPages.Count - 1;
                    ToRemove = hwnd.Key;
                }
            }
            if (Main.PuTTYTabs.Keys.Contains(ToRemove) && ToRemove != -1)
                Main.PuTTYTabs.Remove(ToRemove);
            if (!IsResizing && puttyTabCtrl.TabCount > 0)
            {
                if (SilDev.WinAPI.GetForegroundWindow() == this.Handle)
                {
                    int tabtext = 0;
                    int.TryParse(puttyTabCtrl.SelectedTab.Text, out tabtext);
                    IntPtr hwnd = (IntPtr)tabtext;
                    if (hwnd != IntPtr.Zero)
                        SilDev.WinAPI.SetForegroundWindow(hwnd);
                }
            }
            if (puttyTabCtrl.TabCount <= 0)
                this.Close();
        }
    }
}

