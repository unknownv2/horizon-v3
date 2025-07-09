using System;
using System.IO;
using NoDev.Common.IO;

namespace NoDev.XProfile.Records
{
    public class AvatarAwardRecord
    {
        private const int KnownDataSize = 0x2c;

        private const uint
            PlatformMask = 0x00700000,
            AwardedOnlineMask = 0x00010000,
            AwardedMask = 0x00020000,
            ShowUnawardedMask = 0x00000008;

        public readonly Guid AssetID;
        public uint ImageID;
        public ulong Reserved, ID;

        public DateTime DateTimeAwarded;
        public string Name, Description, UnawardedDescription;

        private readonly byte[] _assetIdBe;
        private uint _flags;

        public AssetGuidType GuidType
        {
            get
            {
                if ((_assetIdBe[9] & 240) != 0xc0)
                {
                    switch ((_assetIdBe[8] & 15))
                    {
                        case 0:
                            return AssetGuidType.TOC;
                        case 1:
                            return AssetGuidType.Awardable;
                        case 2:
                            return AssetGuidType.MarketPlace;
                        case 15:
                            return AssetGuidType.Custom;
                    }
                }
                return AssetGuidType.Custom;
            }
        }

        public AssetBodyType BodyType
        {
            get { return (AssetBodyType)(this._assetIdBe[7] & 15); }
        }

        public AssetCategory Category
        {
            get { return (AssetCategory)this._assetIdBe.ReadInt32(); }
        }

        public Platform Platform
        {
            get
            {
                return (Platform)(_flags & PlatformMask);
            }
            set
            {
                _flags = (_flags & ~PlatformMask) | (uint)value & PlatformMask;
            }
        }

        public bool Awarded
        {
            get
            {
                return (_flags & AwardedMask) != 0;
            }
            set
            {
                if (value)
                {
                    _flags |= AwardedMask;
                    Platform = Platform.Xbox360;
                    AwardedOnline = false;
                }
                else
                {
                    _flags &= ~AwardedMask;
                }
            }
        }

        public bool AwardedOnline
        {
            get
            {
                return (_flags & AwardedOnlineMask) != 0;
            }
            set
            {
                if (value)
                {
                    _flags |= AwardedOnlineMask;
                    Platform = Platform.Xbox360;
                    Awarded = false;
                }
                else
                {
                    _flags &= ~AwardedOnlineMask;
                }
            }
        }

        public bool ShowUnawarded
        {
            get
            {
                return (_flags & ShowUnawardedMask) != 0;
            }
            set
            {
                if (value)
                {
                    _flags |= ShowUnawardedMask;
                }
                else
                {
                    _flags &= ~ShowUnawardedMask;
                }
            }
        }

        public AvatarAwardRecord()
        {

        }

        public AvatarAwardRecord(byte[] data)
        {
            var io = new EndianIO(data, EndianType.Big);

            int dataSize = io.ReadInt32();

            if (dataSize < KnownDataSize)
                throw new XProfileException(string.Format("Invalid data size in Achievement Record (0x{0:X8}).", dataSize));

            byte[] guidBe = io.ReadByteArray(0x10);
            short data2 = guidBe.ReadInt16(0x04);
            this.AssetID = new Guid(guidBe.ReadInt32(), data2, guidBe.ReadInt16(0x06), guidBe.Read(0x08));
            this._assetIdBe = guidBe;

            this.ID = (ulong)this._assetIdBe.ReadInt32(0x0c) << 32 | (uint)data2 << 16 | (ushort)((this._assetIdBe[7] & 15) << 8);

            this.ImageID = io.ReadUInt32();
            this._flags = io.ReadUInt32();
            this.DateTimeAwarded = DateTime.FromFileTimeUtc(io.ReadInt64());
            this.Reserved = io.ReadUInt64();

            io.Position += dataSize - KnownDataSize;

            this.Name = io.ReadNullTerminatedUnicodeString();
            this.Description = io.ReadNullTerminatedUnicodeString();
            this.UnawardedDescription = io.ReadNullTerminatedUnicodeString();

            this.AssetID = new Guid(this._assetIdBe);
        }

        public byte[] ToArray()
        {
            var io = new EndianIO(new MemoryStream(KnownDataSize), EndianType.Big);
            io.Write(KnownDataSize);
            io.Write(_assetIdBe);
            io.Write(ImageID);
            io.Write(_flags);
            io.Write(DateTimeAwarded.ToFileTimeUtc());
            io.Write(Reserved);
            io.WriteNullTerminatedUnicodeString(Name);
            io.WriteNullTerminatedUnicodeString(Description);
            io.WriteNullTerminatedUnicodeString(UnawardedDescription);
            io.Close();

            return io.ToArray();
        }
    }
}
