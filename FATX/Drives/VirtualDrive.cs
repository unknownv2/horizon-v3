using System.IO;
using System.Linq;
using NoDev.Common.IO;

namespace NoDev.Fatx.Drives
{
    internal sealed class VirtualDrive : Drive
    {
        internal enum InputTypes
        {
            DeviceFile,
            USBDirectory
        }

        internal override bool IsValid
        {
            get { return this.IsMounted; }
        }

        internal override bool IsMounted
        {
            get { return this.IO != null; }
        }

        internal override void Close()
        {
            this.IO.Close();
            this.IO = null;
        }

        internal VirtualDrive(string path, InputTypes inputType)
        {
            this.Name = path;

            if (inputType == InputTypes.USBDirectory)
            {
                this.IO = new MultiFileIO(Directory.GetFiles(path).ToList(), EndianType.Big);
                this.Length = this.IO.Length;
                this.DeviceType = FatxDeviceType.USB;
            }
            else
            {
                this.IO = new EndianIO(path, EndianType.Big);
                this.Length = this.IO.Length;
                if (this.Length >= 4 && this.IO.ReadUInt32() == Magic)
                    this.DeviceType = FatxDeviceType.MU;
                else if (this.Length >= (long)HddPartitions.Storage + 4)
                {
                    this.IO.Position = (long)HddPartitions.Storage;
                    if (this.IO.ReadUInt32() == Magic)
                        this.DeviceType = FatxDeviceType.HDD;
                }
                if (this.DeviceType == FatxDeviceType.USB)
                    this.Close();
            }
        }
    }
}
