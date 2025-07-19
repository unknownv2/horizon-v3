using System.Collections.Generic;
using NoDev.Common.IO;
using NoDev.Fatx.Device;

namespace NoDev.Fatx.Drives
{
    internal enum FatxDeviceType
    {
        USB,
        HDD,
        MU
    }

    internal enum MuPartitions : long
    {
        Cache = 0,
        Storage = 0x7FF000
    }

    internal enum UsbPartitions : long
    {
        SystemAuxPartition = 0x400,
        StorageSystem = 0x8000400,
        SystemExtPartition = 0x12000400,
        Storage = 0x20000000
    }

    internal enum HddPartitions : long
    {
        SecuritySector = 0x2000,
        SystemCache = 0x80000,
        TitleCache = 0x80080000,
        System1 = 0x10C080000,
        ExtendedSystem = 0x118EB0000,
        Compatibility = 0x120EB0000,
        Storage = 0x130EB0000
    }

    internal abstract class Drive
    {
        protected const uint Magic = 0x58544146;

        internal string Name;
        internal long Length;
        internal EndianIO IO;
        internal FatxDeviceType DeviceType;

        internal abstract bool IsValid { get; }
        internal abstract bool IsMounted { get; }
        internal abstract void Close();

        private List<FatxDevice> _volumes;

        internal void UnmountVolumes()
        {
            while (this._volumes.Count != 0)
            {
                this._volumes[0].Unmount();
                this._volumes.RemoveAt(0);
            }
        }

        internal void MountVolumes()
        {
            if (this._volumes != null)
                this.UnmountVolumes();

            this._volumes = new List<FatxDevice>();

            if (this.DeviceType == FatxDeviceType.HDD)
                this.MountHardDriveVolumes();
            else if (this.DeviceType == FatxDeviceType.MU)
                this.MountMemoryUnitVolumes();
            else
                this.MountUSBVolumes();
        }

        internal bool VolumesMounted
        {
            get { return this._volumes != null && this._volumes.Count != 0; }
        }

        private void MountMemoryUnitVolumes()
        {
            this.TryMountDevice(FatxPartitionType.NonGrowable, (long)MuPartitions.Storage, this.Length - (long)MuPartitions.Storage);
            // Mount other MU partitions.
        }

        private void MountHardDriveVolumes()
        {
            this.TryMountDevice(FatxPartitionType.NonGrowable, (long)HddPartitions.Storage, this.Length - (long)HddPartitions.Storage);
            // Mount other HDD partitions.
        }

        private void MountUSBVolumes()
        {
            this.TryMountDevice(FatxPartitionType.Growable, (long)UsbPartitions.Storage, this.Length - (long)UsbPartitions.Storage);
            // Mount other USB partitions.
        }

        private void TryMountDevice(FatxPartitionType partitionType, long deviceOffset, long deviceSize)
        {
            try
            {
                this._volumes.Add(new FatxDevice(null, this.IO, partitionType, deviceOffset, deviceSize));
            }
            catch
            {
                
            }
        }
    }
}
