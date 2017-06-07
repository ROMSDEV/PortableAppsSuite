namespace HwMonTray
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Interop
    {
        internal struct TVITEM
        {
            public uint mask;
            public IntPtr hItem;
            public uint state;
            public uint stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public uint iImage;
            public uint iSelectedImage;
            public uint cChildren;
            public IntPtr lParam;
        }

        internal enum SW
        {
            SW_HIDE,
            SW_SHOWNORMAL,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED,
            SW_SHOWMAXIMIZED,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE,
            SW_SHOW,
            SW_MINIMIZE,
            SW_SHOWMINNOACTIVE,
            SW_SHOWNA,
            SW_RESTORE,
            SW_SHOWDEFAULT,
            SW_FORCEMINIMIZE,
            SW_MAX = 11
        }

        internal enum TVM
        {
            FIRST = 4352,
            INSERTITEMA = 4352,
            DELETEITEM,
            EXPAND,
            GETITEMRECT = 4356,
            GETCOUNT,
            GETINDENT,
            SETINDENT,
            GETIMAGELIST,
            SETIMAGELIST,
            GETNEXTITEM,
            SELECTITEM,
            GETITEMA,
            SETITEMA,
            EDITLABELA,
            GETEDITCONTROL,
            GETVISIBLECOUNT,
            HITTEST,
            CREATEDRAGIMAGE,
            SORTCHILDREN,
            ENSUREVISIBLE,
            SORTCHILDRENCB,
            ENDEDITLABELNOW,
            GETISEARCHSTRINGA,
            SETTOOLTIPS,
            GETTOOLTIPS,
            SETINSERTMARK,
            SETITEMHEIGHT,
            GETITEMHEIGHT,
            SETBKCOLOR,
            SETTEXTCOLOR,
            GETBKCOLOR,
            GETTEXTCOLOR,
            SETSCROLLTIME,
            GETSCROLLTIME,
            SETINSERTMARKCOLOR = 4389,
            GETINSERTMARKCOLOR,
            GETITEMSTATE,
            SETLINECOLOR,
            GETLINECOLOR,
            MAPACCIDTOHTREEITEM,
            MAPHTREEITEMTOACCID,
            INSERTITEMW = 4402,
            GETITEMW = 4414,
            SETITEMW,
            GETISEARCHSTRINGW,
            EDITLABELW
        }

        internal enum TVE
        {
            COLLAPSE = 1,
            EXPAND,
            TOGGLE,
            EXPANDPARTIAL = 16384,
            COLLAPSERESET = 32768
        }

        internal enum TVGN
        {
            ROOT,
            NEXT,
            PREVIOUS,
            PARENT,
            CHILD,
            FIRSTVISIBLE,
            NEXTVISIBLE,
            PREVIOUSVISIBLE,
            DROPHILITE,
            CARET,
            LASTVISIBLE
        }

        internal const uint PROCESS_ALL_ACCESS = 2035711u;
        internal const uint MEM_COMMIT = 4096u;
        internal const uint MEM_RELEASE = 32768u;
        internal const uint PAGE_READWRITE = 4u;
        public const int WS_EX_TOOLWINDOW = 128;
        public const int WS_EX_APPWINDOW = 262144;
        public const int WS_MAXIMIZEBOX = 65536;
        public const int WS_MINIMIZEBOX = 131072;
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("kernel32")]
        internal static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        internal static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint dwFreeType);

        [DllImport("kernel32.dll")]
        internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref TVITEM buffer, int dwSize, IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr hProcess, out bool lpSystemInfo);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, TVM msg, TVGN wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern bool SendMessage(IntPtr hWnd, TVM msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int Index);

        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr hWnd, int Index, int Value);

        [DllImport("user32.dll")]
        internal static extern void SetWindowText(IntPtr hWnd, string Text);

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
