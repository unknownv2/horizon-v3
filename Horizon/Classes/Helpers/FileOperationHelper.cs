using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NoDev.Horizon
{
    internal class FileOperationHelper
    {
        private readonly IntPtr _hwnd;
        private readonly ushort _fFlags;
        private readonly string _lpszProgressTitle;

        internal FileOperationHelper(IntPtr hwnd, ushort fFlags, string lpszProgressTitle = null)
        {
            if (lpszProgressTitle == null && (fFlags & NativeMethods.FOF_SIMPLEPROGRESS) != 0)
                throw new Exception("Progress title cannot be set without the simple progress flag.");

            this._hwnd = hwnd;
            this._fFlags = fFlags;
            this._lpszProgressTitle = lpszProgressTitle;
        }

        internal void Delete(string[] files)
        {
            var lpFileOp = new NativeMethods.SHFILEOPSTRUCT();
            lpFileOp.hwnd = this._hwnd;
            lpFileOp.wFunc = NativeMethods.FO_DELETE;
            lpFileOp.pFrom = ToPCZZTSTR(files);
            lpFileOp.pTo = IntPtr.Zero;
            lpFileOp.fFlags = this._fFlags;
            lpFileOp.lpszProgressTitle = this._lpszProgressTitle;

            NativeMethods.SHFileOperation(ref lpFileOp);
        }

        internal void Copy(string[] from, string[] to)
        {
            this.CopyMove(NativeMethods.FO_COPY, from, to);
        }

        internal void Move(string[] from, string[] to)
        {
            this.CopyMove(NativeMethods.FO_MOVE, from, to);
        }

        private void CopyMove(uint method, string[] from, string[] to)
        {
            if (from.Length != to.Length)
                throw new Exception("Source file count does not match destination file count.");

            var lpFileOp = new NativeMethods.SHFILEOPSTRUCT();
            lpFileOp.hwnd = this._hwnd;
            lpFileOp.wFunc = method;
            lpFileOp.pFrom = ToPCZZTSTR(from);
            lpFileOp.pTo = ToPCZZTSTR(to);
            lpFileOp.fFlags = this._fFlags;
            lpFileOp.lpszProgressTitle = this._lpszProgressTitle;

            NativeMethods.SHFileOperation(ref lpFileOp);
        }

        private static IntPtr ToPCZZTSTR(string[] stringList)
        {
            if (stringList.Length == 0)
                return Marshal.StringToHGlobalUni("\0\0");

            var sb = new StringBuilder(stringList[0]);

            for (int x = 1; x < stringList.Length; x++)
                sb.Append('\0' + stringList[x]);

            sb.Append("\0\0");

            return Marshal.StringToHGlobalUni(sb.ToString());
        }
    }
}
