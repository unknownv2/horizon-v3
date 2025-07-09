using System;
using System.IO;
using NoDev.Common.IO;

namespace NoDev.XProfile.Records
{
    public class AchievementRecord
    {
        private const int KnownDataSize = 0x1c;

        private const uint
            TypeMask = 0x00000007,
            PlatformMask = 0x00700000,
            ShowUnachievedMask = 0x00000008,
            AchievedOnlineMask = 0x00010000,
            AchievedMask = 0x00020000;

        public readonly uint ID;
        public DateTime DateTimeAchieved;
        public int Credit;
        public uint ImageID, Flags;
        public string Label, Description, UnachievedDescription;

        public AchievementType Type
        {
            get
            {
                return (AchievementType)(Flags & TypeMask);
            }
            set
            {
                Flags = (Flags & ~TypeMask) | (uint)value & TypeMask;
            }
        }

        public Platform Platform
        {
            get
            {
                return (Platform)(Flags & PlatformMask);
            }
            set
            {
                Flags = (Flags & ~PlatformMask) | (uint)value & PlatformMask;
            }
        }

        public bool Achieved
        {
            get
            {
                return (Flags & AchievedMask) != 0;
            }
            set
            {
                if (value)
                {
                    Flags |= AchievedMask;
                    Platform = Platform.Xbox360;
                    AchievedOnline = false;
                }
                else
                {
                    Flags &= ~AchievedMask;
                }
            }
        }

        public bool AchievedOnline
        {
            get
            {
                return (Flags & AchievedOnlineMask) != 0;
            }
            set
            {
                if (value)
                {
                    Flags |= AchievedOnlineMask;
                    Platform = Platform.Xbox360;
                    Achieved = false;
                }
                else
                {
                    Flags &= ~AchievedOnlineMask;
                }
            }
        }

        public bool ShowUnachieved
        {
            get
            {
                return (Flags & ShowUnachievedMask) != 0;
            }
            set
            {
                if (value)
                {
                    Flags |= ShowUnachievedMask;
                }
                else
                {
                    Flags &= ~ShowUnachievedMask;
                }
            }
        }

        public AchievementRecord(uint id)
        {
            this.ID = id;
        }

        public AchievementRecord(byte[] data)
        {
            var io = new EndianIO(data, EndianType.Big);

            int dataSize = io.ReadInt32();

            if (dataSize < KnownDataSize)
                throw new XProfileException(string.Format("Invalid data size in Achievement Record (0x{0:X8}).", dataSize));

            this.ID = io.ReadUInt32();
            this.ImageID = io.ReadUInt32();
            this.Credit = io.ReadInt32();
            this.Flags = io.ReadUInt32();

            this.DateTimeAchieved = DateTime.FromFileTimeUtc(io.ReadInt64());

            io.Position += dataSize - KnownDataSize;

            this.Label = io.ReadNullTerminatedUnicodeString();
            this.Description = io.ReadNullTerminatedUnicodeString();
            this.UnachievedDescription = io.ReadNullTerminatedUnicodeString();

            io.Close();
        }

        public byte[] ToArray()
        {
            var ew = new EndianIO(new MemoryStream(KnownDataSize), EndianType.Big);

            ew.Write(KnownDataSize);
            ew.Write(this.ID);
            ew.Write(this.ImageID);
            ew.Write(this.Credit);
            ew.Write(this.Flags);
            ew.Write(this.DateTimeAchieved.ToFileTimeUtc());
            ew.WriteNullTerminatedUnicodeString(this.Label);
            ew.WriteNullTerminatedUnicodeString(this.Description);
            ew.WriteNullTerminatedUnicodeString(this.UnachievedDescription);

            ew.Close();

            return ew.ToArray();
        }
    }
}
