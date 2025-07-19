using System;
using System.Runtime.InteropServices;

namespace NoDev.Fatx
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer,
            uint nInBufferSize, ref long lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);
    }
}
