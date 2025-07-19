using System;
using System.Collections.Generic;
using NoDev.Common;
using NoDev.Common.IO;

namespace NoDev.Fatx.Device
{    
    internal class FatxException : Exception
    {
        internal FatxException(string message)
            : base("FATX: " + message)
        {

        }
    }

    internal enum FatxPartitionType
    {
        NonGrowable = 0,
        Growable
    }

    internal class FatxDirectoryEntry
    {
        [Flags]
        internal enum Attribute : byte
        {
            Normal = 0x00,
            ReadOnly = 0x01,
            Hidden = 0x02,
            System = 0x04,
            Volume = 0x08,
            Directory = 0x10,
            Archive = 0x20,
            Device = 0x40,
        }

        internal byte FileNameLength;
        internal byte Characteristics;

        internal string Filename;

        internal uint FirstClusterNumber;
        internal readonly uint FileSize;

        internal int CreationTimeStamp;
        internal int LastAccessTimeStamp;

        internal bool IsDirectory
        {
            get { return ((this.Characteristics >> 4) & 0x01) == 1; }
        }

        internal readonly bool IsValid = true;

        internal FatxDirectoryEntry()
        {
            
        }

        internal FatxDirectoryEntry(EndianIO io)
        {
            this.FileNameLength = io.ReadByte();
            this.Characteristics = io.ReadByte();

            if (FileNameLength > 0x2a || FileNameLength == 0 || (this.Characteristics & 0xfffffff8 & 0xffffffcf) != 0x00)
            {
                this.IsValid = false;

                return;
            }

            var fileName = io.ReadAsciiString(this.FileNameLength);

            if (this.FileNameLength == 1 && fileName[0] == '.' || this.FileNameLength == 2 && fileName[1] == '.')
            {
                this.IsValid = false;

                return;
            }

            for (int x = 0; x < this.FileNameLength; x++)
            {
                var fchar = fileName[x];

                if (fchar >= 0x20 && fchar <= 0x7e && ((1 << (fchar & 0x1f)) & FatxDevice.FatxFatIllegalTable[((fchar >> 3) & 0x1FFFFFFC) / 4]) == 0)
                    continue;

                this.IsValid = false;

                return;
            }

            this.Filename = fileName;

            io.Position += 0x2a - this.FileNameLength;

            this.FirstClusterNumber = io.ReadUInt32();
            this.FileSize = io.ReadUInt32();
            this.CreationTimeStamp = io.ReadInt32();
            this.LastAccessTimeStamp = io.ReadInt32();
            //this.LastWriteTimeStamp = io.ReadInt32();
        }
    }

    internal class FatxCacheEntry
    {
        internal uint State, Flags, ContiguousClusters;

        internal uint StartingCluster
        {
            get { return (State & 0xfffffff); }
            set { State = (State & 0xf0000000) | (value & 0xfffffff); }
        }

        internal uint ClusterIndex
        {
            get { return Flags & 0xfffffff; }
            set { Flags = Flags & 0xf0000000 | value & 0xfffffff; }
        }

        internal int NextIndex
        {
            get { return (int)(State >> 28); }
            set { State = (uint)(((value << 28) & 0xf0000000)) | State & 0xfffffff; }
        }

        internal int PreviousIndex
        {
            get { return (int)(Flags >> 28); }
            set { Flags = (uint)((value << 28) & 0xf0000000) | Flags & 0xfffffff; }
        }
    }

    internal class FatxFcb : IoFcb
    {
        internal string FileName;

        internal byte FileAttributes, Flags, CacheHeadIndex, State;
        internal int CreationTimeStamp, LastAccessTimeStamp, ReferenceCount;
        internal uint FileSize, EndOfFile, DirectoryEntryByteOffset, ByteOffset, FirstCluster, LastCluster;

        internal DateTime LastWriteTimeStamp;
        internal ShareAccess ShareAccess;

        internal List<FatxCacheEntry> Cache;

        internal FatxFcb ParentFCB, CloneFCB;

        internal bool IsDirectory
        {
            get { return (State & 0x02) != 0; }
        }

        internal bool IsRootDir
        {
            get { return (State & 0x04) != 0; }
        }

        internal bool DeleteOnClose
        {
            get { return (State & 0x10) != 0; }
        }

        internal bool IsModified
        {
            get { return (State & 0x20) != 0; }
        }

        /* States
         * 0x01 - Is Title-Owned
         * 0x02 - Is Folder/Directory
         * 0x04 - Is Root Directory FCB
         * 0x10 - Marked For Deletion
         * 0x20 - Is Modified
         * 0x40 - Allocated FileName Replaced
        */

        internal FatxFcb()
        {
            
        }

        internal FatxFcb(FatxDirectoryEntry directoryLookup, FatxFcb parentFcb, uint directoryEntryByteOffset)
        {
            if (directoryLookup.IsDirectory)
                this.State |= 0x02;

            this.DirectoryEntryByteOffset = directoryEntryByteOffset;

            this.FileAttributes = directoryLookup.Characteristics;
            this.CreationTimeStamp = directoryLookup.CreationTimeStamp;
            this.LastAccessTimeStamp = directoryLookup.LastAccessTimeStamp;

            if (directoryLookup.FileNameLength > 0x2A || directoryLookup.FileNameLength == 0)
                throw new FatxException(string.Format("Detected an invalid filename length for directory entry {0}.", directoryLookup.Filename));

            this.FileSize = directoryLookup.FileSize;

            try
            {
                var t = this.LastAccessTimeStamp;

                this.LastWriteTimeStamp = new DateTime((t >> 25 & 0x7f) + 1980, t >> 21 & 0x0f, 
                    t >> 16 & 0x1f, t >> 11 & 0x1f, t >> 5 & 0x3f, (t & 0x1f) << 1).ToLocalTime();
            }
            catch
            {
                this.LastWriteTimeStamp = DateTime.Now;
            }

            this.FileName = directoryLookup.Filename;

            Flags = (byte) (directoryLookup.Characteristics + FileName.Length + 1);

            this.FirstCluster = directoryLookup.FirstClusterNumber;

            if (this.FirstCluster != 0)
                this.EndOfFile = 0xffffffff;
            else if (this.EndOfFile != 0x00)
                throw new FatxException("Attempted to create an FCB with an invalid directory entry.");

            this.ParentFCB = parentFcb;

            ReferenceCount = 0x01;
        }
    }

    internal class FatxAllocationState
    {
        internal class AllocationState
        {
            internal uint FirstAllocatedCluster;
            internal uint ContiguousClusters;
        }

        internal readonly List<AllocationState> AllocationStates;

        internal FatxAllocationState()
        {
            this.AllocationStates = new List<AllocationState>(0xA);
        }
    }

    internal enum FatxIO
    {
        Read = 2,
        Write
    }

    internal class FatxDirectoryEnumerationContext
    {
        private readonly string _searchPattern;
        private readonly IoFsdHandle _handle;
        private readonly FatxDevice _device;

        internal FatxDirectoryEnumerationContext(FatxDevice device, string directoryInfo, string mask)
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

            _device.FatxFsdCreate(_handle);
        }

        internal bool FindNextFile(out IoFsdDirectoryInformation findFileData)
        {
            return _device.FatxFsdDirectoryControl(_handle, _searchPattern, out findFileData) == 0x00;
        }

        internal void CloseEnumeration()
        {
            _device.FatxFsdClose(_handle);
        }
    }
}
