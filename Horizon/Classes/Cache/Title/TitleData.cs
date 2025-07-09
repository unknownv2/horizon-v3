using System.Collections.Generic;
using NoDev.Common.IO;

namespace NoDev.Horizon
{
    internal static class TitleCollection
    {
        internal static readonly Dictionary<uint, Title> Titles = new Dictionary<uint, Title>();

        internal static void ReadTitles(EndianIO io)
        {
            while (io.Position != io.Length)
            {
                var title = new Title(io);
                Titles.Add(title.ID, title);
            }
        }
    }

    internal class Title
    {
        internal readonly uint ID;
        internal readonly string Name;
        internal readonly int AchievementCount, Credit;
        internal readonly byte AwardCount, MaleAwardCount, FemaleAwardCount;

        internal readonly Achievement[] Achievements;
        internal readonly Award[] Awards;

        internal Title(EndianIO io)
        {
            this.ID = io.ReadUInt32();
            this.Name = io.ReadUnicodeString(io.ReadInt32());
            this.AchievementCount = io.ReadInt32();
            this.Credit = io.ReadInt32();
            this.AwardCount = io.ReadByte();
            this.MaleAwardCount = io.ReadByte();
            this.FemaleAwardCount = io.ReadByte();

            this.Achievements = new Achievement[this.AchievementCount];
            for (int x = 0; x < this.AchievementCount; x++)
                this.Achievements[x] = new Achievement(io);

            this.Awards = new Award[this.AwardCount];
            for (int x = 0; x < this.AwardCount; x++)
                this.Awards[x] = new Award(io);
        }
    }

    internal struct Achievement
    {
        internal readonly uint ID, ImageID, Flags;
        internal readonly int Credit;
        internal readonly string Label, AchievedDescription, UnachievedDescription;

        internal Achievement(EndianIO io)
        {
            this.ID = io.ReadUInt32();
            this.ImageID = io.ReadUInt32();
            this.Credit = io.ReadInt32();
            this.Flags = io.ReadUInt32();
            this.Label = io.ReadUnicodeString(io.ReadInt32());
            this.AchievedDescription = io.ReadUnicodeString(io.ReadInt32());
            this.UnachievedDescription = io.ReadUnicodeString(io.ReadInt32());
        }
    }

    internal struct Award
    {
        public readonly uint ImageID;
        public readonly ulong Reserved, ID;
        public readonly string Name, Description, UnawardedDescription;

        internal Award(EndianIO io)
        {
            this.ID = io.ReadUInt64();
            this.Reserved = io.ReadUInt64();
            this.ImageID = io.ReadUInt32();
            this.Name = io.ReadUnicodeString(io.ReadInt32());
            this.Description = io.ReadUnicodeString(io.ReadInt32());
            this.UnawardedDescription = io.ReadUnicodeString(io.ReadInt32());
        }
    }
}
