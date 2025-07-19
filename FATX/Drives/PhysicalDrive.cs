using System;
using System.IO;
using System.Management;
using NoDev.Common.IO;
using Microsoft.Win32.SafeHandles;

namespace NoDev.Fatx.Drives
{
    internal sealed class PhysicalDrive : Drive
    {
        private SafeFileHandle _handle;

        private ulong DiskSize { get; set; }

        internal override void Close()
        {
            this._handle.Close();
            this._handle = null;
        }

        internal override bool IsValid
        {
            get { return this._handle != null && !this._handle.IsInvalid && !this._handle.IsClosed; }
        }

        internal static ManagementObjectCollection GetPhysicalDisks()
        {
            return new ManagementObjectSearcher(new WqlObjectQuery("SELECT Name, ConfigManagerErrorCode, Size FROM Win32_DiskDrive WHERE ConfigManagerErrorCode = 0 AND Size is NOT NULL")).Get();
        }

        internal override bool IsMounted
        {
            get
            {
                var physicalDisks = GetPhysicalDisks();
                foreach (ManagementObject physDisk in physicalDisks)
                    if ((string)physDisk["Name"] == this.Name && physDisk["Size"] != null && (ulong)physDisk["Size"] == this.DiskSize)
                        return true;
                return false;
            }
        }

        internal PhysicalDrive(string volumeName, ulong diskSize)
        {
            this.Name = volumeName;
            this.DiskSize = diskSize;
            this._handle = Common.NativeMethods.CreateFile(this.Name, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0xA0000040, IntPtr.Zero);
            if (this.IsValid)
            {
                uint returnedBytes;
                if (NativeMethods.DeviceIoControl(this._handle.DangerousGetHandle(), 0x0007405C, IntPtr.Zero, 0, ref Length, 8, out returnedBytes, IntPtr.Zero))
                {
                    this.IO = new EndianIO(new FileStream(this._handle, FileAccess.ReadWrite), EndianType.Big);
                    if (this.Length >= 4 && this.IO.ReadUInt32() == Magic)
                        this.DeviceType = FatxDeviceType.MU;
                    else if (this.Length >= (long)HddPartitions.Storage + 4)
                    {
                        this.IO.Position = (long)HddPartitions.Storage;
                        if (this.IO.ReadUInt32() == Magic)
                            this.DeviceType = FatxDeviceType.HDD;
                    }
                }
            }
            if (this.DeviceType == FatxDeviceType.USB)
                this.Close();
        }
    }
}
