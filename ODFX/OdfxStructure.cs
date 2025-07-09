using System;
using NoDev.Common;
using NoDev.Common.IO;

namespace NoDev.Odfx
{
    public abstract class GdfDevice
    {
        protected long FileSize;
        protected abstract void DeviceControl(ulong controlCode, ref byte[] outputBuffer);
    }

    public class GdfFcb : IoFcb
    {
        private readonly byte _flags;

        public long FileSize;
        public uint FirstBlockNumber;
        public DateTime TimeStamp;

        public bool IsDirectory
        {
            get
            {
                return (_flags & 0x1) != 0x00;
            }
        }

        public long BlockLength
        {
            get
            {
                return (FileSize & 0xFFFFFFFF);
            }
        }

        public GdfFcb(byte bFlags)
        {
            _flags = bFlags;
        }

        public GdfFcb(GdfDirectoryEntry directoryEntry, GdfFcb parentFcb)
        {
            this.FirstBlockNumber = directoryEntry.FirstSector;
            this.FileSize = directoryEntry.FileSize;
            this.TimeStamp = parentFcb.TimeStamp;

            int flags = parentFcb._flags & 0x04;

            if (directoryEntry.IsDirectory)
                flags |= 0x01;
            else if ((directoryEntry.Attributes & 0x40) != 0x00)
                flags |= 0x20;

            this._flags = (byte)(flags & 0xFF);
        }
    }

    public class GdfDirectoryEntry
    {
        public ushort LeftEntryIndex;
        public ushort RightEntryIndex;
        public uint FirstSector;
        public uint FileSize;
        public byte Attributes;
        public byte FileNameLength;
        public string FileName;

        public bool IsDirectory
        {
            get
            {
                return (Attributes & 0x10) != 0x00;
            }
        }

        public GdfDirectoryEntry(EndianIO io)
        {
            LeftEntryIndex = io.ReadUInt16();
            RightEntryIndex = io.ReadUInt16();
            FirstSector = io.ReadUInt32();
            FileSize = io.ReadUInt32();
            Attributes = io.ReadByte();
            FileName = io.ReadAsciiString(FileNameLength = io.ReadByte());
        }
    }

    internal class OdfxDirectoryEnumerationContext
    {
        private readonly string _searchPattern;
        private readonly IoFsdHandle _handle;
        private readonly OdfxDevice _device;

        internal OdfxDirectoryEnumerationContext(OdfxDevice device, string directoryInfo, string mask)
        {
            _device = device;
            _searchPattern = mask;

            _handle = new IoFsdHandle();

            _handle.Sp.CreateParametersParameters = new IrpCreateParameters {
                DesiredAccess = 0x100001,
                Options = 0x1004021,
                FileAttributes = 0x40,
                ShareAccess = 0x03,
                RemainingName = directoryInfo
            };

            _device.OdfxFsdCreate(_handle);
        }

        internal bool FindNextFile(out IoFsdDirectoryInformation findFileData)
        {
            return _device.OdfxFsdDirectoryControl(_handle, _searchPattern, out findFileData) == 0x00;
        }
    }
}