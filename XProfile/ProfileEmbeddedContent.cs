using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NoDev.Common;
using NoDev.Common.IO;
using NoDev.Stfs;
using NoDev.Xbox360;

namespace NoDev.XProfile
{
    public sealed class ProfileEmbeddedContent
    {
        private readonly EndianIO _io;
        private readonly List<byte[]> _consoleList;
        private readonly StfsVolumeDescriptor _volumeDescriptor;
        private StfsDevice _stfsPec;
        
        public DriveInfo Drive { get; private set; }

        private const long BackingFileOffset = 0x1000;

        private byte[] _creator;
        private void SetCreator(ulong creator)
        {
            byte[] c = BitConverter.GetBytes(creator);
            Array.Reverse(c);
            this._creator = c;
        }

        private static StfsCreatePacket CreateStfsCreatePacket(StfsVolumeDescriptor descriptor)
        {
            return new StfsCreatePacket
            {
                BlockCacheElementCount = 0x05,
                BackingFileOffset = BackingFileOffset,
                BackingMaximumVolumeSize = 0x00000000fffff000,
                DeviceCharacteristics = 0x01,
                VolumeDescriptor = descriptor
            };
        }

        public ProfileEmbeddedContent(EndianIO inIo)
        {
            if (inIo.Length < BackingFileOffset)
                throw new XProfileException("Invalid PEC file length.");

            this._io = inIo;

            inIo.Position = 0x244;

            this._volumeDescriptor = new StfsVolumeDescriptor(inIo);

            inIo.Position += 0x04;

            ulong creator = inIo.ReadUInt64();
            this.SetCreator(creator);

            int entryCount = this._io.ReadByte();

            if (entryCount > 0x64)
                throw new XProfileException("Invalid number of consoles found in PEC.");

            this._consoleList = new List<byte[]>(entryCount);

            for (int x = 0; x < entryCount; x++)
                this._consoleList.Add(this._io.ReadByteArray(5));

            this.Mount(creator);
        }

        public ProfileEmbeddedContent(EndianIO outIo, ulong creator)
        {
            this.SetCreator(creator);
            this._consoleList = new List<byte[]>(1);
            this._volumeDescriptor = new StfsVolumeDescriptor();

            this._io = outIo;

            outIo.SetLength(BackingFileOffset);

            this.Mount(creator);

            this.Save();
        }

        private void Mount(ulong creator)
        {
            char driveLetter = Win32.GetFreeDriveLetter();
            this._stfsPec = new StfsDevice(driveLetter + ":", this._io.Stream, CreateStfsCreatePacket(_volumeDescriptor), "PEC " + creator.ToString("X16"));
            this.Drive = new DriveInfo(driveLetter.ToString(CultureInfo.InvariantCulture));
        }

        public void Close()
        {
            this.Save();
            this._stfsPec.Unmount();
            this._io.Close();
        }

        public void Save()
        {
            var cert = new XeConsoleCertificate(KeyStorage.PublicKey);

            int consoleCount = this._consoleList.Count;

            this._consoleList[consoleCount - 1] = cert.ConsoleID;

            var io = this._io;

            io.Position = 0x274;
            io.Write((byte)consoleCount);

            foreach (var console in this._consoleList)
                io.Write(console);

            io.Position = 0x244;
            io.Write(this._volumeDescriptor.ToArray());

            io.Position += 0x04;
            io.Write(this._creator);

            io.Position = 0x23c;

            byte[] hash = XeCrypt.XeCryptSha(io.ReadByteArray(0xDC4));

            io.Position = 0x228;
            io.Write(hash);

            io.Position = 0x00;
            io.Write(KeyStorage.PublicKey);

            io.Position = 0x23c;

            byte[] sig = XeCrypt.FormatSignature(KeyStorage.PrivateKeys, io.ReadByteArray(0xdc4));

            io.Position = 0x1a8;
            io.Write(sig);
        }
    }
}