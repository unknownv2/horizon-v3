using System;
using System.Runtime.InteropServices;

namespace NoDev.Horizon
{
    internal static class NativeMethods
    {
        internal const int HWND_BROADCAST = 0xffff;

        internal static readonly int WM_HZN_SHOW = Common.NativeMethods.RegisterWindowMessage("WM_HZN_SHOW");

        [DllImport("User32.dll")]
        internal extern static bool PrintWindow(IntPtr hWnd, IntPtr dc, uint reservedFlag);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern Int32 SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SHFILEOPSTRUCT
        {
            internal IntPtr hwnd;
            internal uint wFunc;
            internal IntPtr pFrom;
            internal IntPtr pTo;
            internal ushort fFlags;
            internal int fAnyOperationsAborted;
            internal IntPtr hNameMappings;

            [MarshalAs(UnmanagedType.LPWStr)]
            internal string lpszProgressTitle;
        }

        internal const uint
            FO_MOVE = 0x0001,
            FO_COPY = 0x0002,
            FO_DELETE = 0x0003,
            FO_RENAME = 0x0004;

        internal const ushort
            FOF_MULTIDESTFILES = 0x0001,
            FOF_CONFIRMMOUSE = 0x0002,
            FOF_SILENT = 0x0004,
            FOF_RENAMEONCOLLISION = 0x0008,
            FOF_NOCONFIRMATION = 0x0010,
            FOF_WANTMAPPINGHANDLE = 0x0020,
            FOF_ALLOWUNDO = 0x0040,
            FOF_FILESONLY = 0x0080,
            FOF_SIMPLEPROGRESS = 0x0100,
            FOF_NOCONFIRMMKDIR = 0x0200,
            FOF_NOERRORUI = 0x0400,
            FOF_NOCOPYSECURITYATTRIBS = 0x0800,
            FOF_NORECURSION = 0x1000,
            FOF_NO_CONNECTED_ELEMENTS = 0x2000,
            FOF_WANTNUKEWARNING = 0x4000,
            FOF_NORECURSEREPARSE = 0x8000;
    }
}
