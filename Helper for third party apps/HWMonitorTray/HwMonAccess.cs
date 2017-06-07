namespace HwMonTray
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class HwMonAccess : IDisposable
    {
        private static HwMonTable tab = new HwMonTable();
        private readonly IntPtr wndTreeView;
        private readonly IntPtr hProcess = IntPtr.Zero;
        private IntPtr lpRemoteBuffer = IntPtr.Zero;
        private IntPtr lpLocalBuffer = IntPtr.Zero;
        private readonly bool Wow64 = true;
        private string Device;
        private string Type;
        private string ID_Values;

        public HwMonAccess()
        {
            var wndApp = Interop.FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "CPUID Hardware Monitor (GadgetHost)");
            uint dwProcessId;
            Interop.GetWindowThreadProcessId(wndApp, out dwProcessId);
            if (wndApp == IntPtr.Zero)
                return;
            var intPtr = Interop.FindWindowEx(wndApp, IntPtr.Zero, "AfxFrameOrView80s", null);
            if (intPtr == IntPtr.Zero)
            {
                intPtr = Interop.FindWindowEx(wndApp, IntPtr.Zero, "AfxFrameOrView80su", null);
                if (intPtr == IntPtr.Zero)
                {
                    intPtr = Interop.FindWindowEx(wndApp, IntPtr.Zero, "AfxFrameOrView90su", null);
                    if (intPtr == IntPtr.Zero)
                        throw new SystemException("no host");
                }
            }
            wndTreeView = Interop.FindWindowEx(intPtr, IntPtr.Zero, "SysTreeView32", null);
            if (wndTreeView == IntPtr.Zero)
                throw new SystemException("no treeview");
            lpLocalBuffer = Marshal.AllocHGlobal(1024);
            if (lpLocalBuffer == IntPtr.Zero)
                throw new SystemException("Failed to allocate memory in local process");
            hProcess = Interop.OpenProcess(2035711u, false, dwProcessId);
            if (hProcess == IntPtr.Zero)
                throw new ApplicationException("Failed to access process");
            lpRemoteBuffer = Interop.VirtualAllocEx(hProcess, IntPtr.Zero, 1024, 4096u, 4u);
            if (lpRemoteBuffer == IntPtr.Zero)
                throw new SystemException("Failed to allocate memory in remote process");
            if (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1 || Environment.OSVersion.Version.Major > 5)
                Interop.IsWow64Process(hProcess, out Wow64);
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void Dispose(bool disposing)
        {
            if (lpLocalBuffer != IntPtr.Zero)
                Marshal.FreeHGlobal(lpLocalBuffer);
            if (lpRemoteBuffer != IntPtr.Zero)
                Interop.VirtualFreeEx(hProcess, lpRemoteBuffer, 0, 32768u);
            if (hProcess != IntPtr.Zero)
                Interop.CloseHandle(hProcess);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        public static HwMonTable Read()
        {
            tab = new HwMonTable();
            using (var hwMonAccess = new HwMonAccess())
                hwMonAccess.Start();
            return tab;
        }

        public static bool Is64Bit()
        {
            bool result;
            using (var hwMonAccess = new HwMonAccess())
                result = !hwMonAccess.Wow64;
            return result;
        }

        private void Start()
        {
            var inItem = Interop.SendMessage(wndTreeView, Interop.TVM.GETNEXTITEM, Interop.TVGN.ROOT, IntPtr.Zero);
            ReadTVItems(inItem, 0);
        }

        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        private string GetTVItemTextEx(IntPtr wndTreeView, IntPtr item)
        {
            var tVITEM = default(Interop.TVITEM);
            tVITEM.mask = 1u;
            tVITEM.hItem = item;
            tVITEM.cchTextMax = 512;
            tVITEM.pszText = (IntPtr)(lpRemoteBuffer.ToInt32() + Marshal.SizeOf(typeof(Interop.TVITEM)));
            if (!Interop.WriteProcessMemory(hProcess, lpRemoteBuffer, ref tVITEM, Marshal.SizeOf(typeof(Interop.TVITEM)), IntPtr.Zero))
                throw new SystemException("Failed to write to process memory.");
            Interop.SendMessage(wndTreeView, Interop.TVM.GETITEMW, 0, lpRemoteBuffer);
            if (!Interop.ReadProcessMemory(hProcess, lpRemoteBuffer, lpLocalBuffer, 1024, IntPtr.Zero))
                throw new SystemException("Failed to read from process memory.");
            return Marshal.PtrToStringUni((IntPtr)(lpLocalBuffer.ToInt32() + Marshal.SizeOf(typeof(Interop.TVITEM))));
        }

        private void addToTable()
        {
            var array = ID_Values.Split('\t');
            var text = array[1];
            if (text.Contains("("))
                text = text.Substring(0, text.IndexOf("(", StringComparison.Ordinal));
            text = text.Replace("°C", "");
            text = text.Replace("RPM", "");
            text = text.Replace("V", "");
            text = text.Replace("W", "");
            text = text.Trim();
            var iD = array[0].ToUpper().Replace(" ", "");
            if (Device == "FB-DIMM")
                iD = "FBDIMM";
            tab.AddCount(new HwMonItem(Device, Type, iD, text));
        }

        private void ReadTVItems(IntPtr inItem, int level)
        {
            while (inItem != IntPtr.Zero)
            {
                var tVItemTextEx = GetTVItemTextEx(wndTreeView, inItem);
                switch (level)
                {
                    case 1:
                        Device = tVItemTextEx;
                        break;
                    case 2:
                        Type = tVItemTextEx;
                        break;
                    case 3:
                        ID_Values = tVItemTextEx;
                        addToTable();
                        break;
                }
                level++;
                ReadTVItems(Interop.SendMessage(wndTreeView, Interop.TVM.GETNEXTITEM, Interop.TVGN.CHILD, inItem), level);
                level--;
                inItem = Interop.SendMessage(wndTreeView, Interop.TVM.GETNEXTITEM, Interop.TVGN.NEXT, inItem);
            }
        }
    }
}
