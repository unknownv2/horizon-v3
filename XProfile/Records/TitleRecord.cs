using System;
using System.IO;
using NoDev.Common.IO;

namespace NoDev.XProfile.Records
{
    public struct XAvatarAwardsCounter
    {
        public byte Earned;
        public byte Possible;
    }

    public class TitleRecord
    {
        public readonly uint TitleID;
        public readonly string TitleName;
        public readonly int AchievementsPossible, CreditPossible;
        public readonly ushort ReservedAchievementCount;

        public int AchievementsEarned;
        public int CreditEarned;
        
        public XAvatarAwardsCounter AllAvatarAwards;
        public XAvatarAwardsCounter MaleAvatarAwards;
        public XAvatarAwardsCounter FemaleAvatarAwards;

        public uint ReservedFlags;
        public DateTime LastLoaded;

        // Not sure what this is yet.
        public void SetTitleListRecordInfo(uint flag)
        {
            this.ReservedFlags |= flag;
        }

        public TitleRecord(uint titleId, string titleName, int totalAchievements, int totalCredit, byte totalAwards, byte totalMaleAwards, byte totalFemaleAwards)
        {
            this.TitleID = titleId;
            this.TitleName = titleName;
            this.AchievementsPossible = totalAchievements;
            this.CreditPossible = totalCredit;
            this.AllAvatarAwards.Possible = totalAwards;
            this.MaleAvatarAwards.Possible = totalMaleAwards;
            this.FemaleAvatarAwards.Possible = totalFemaleAwards;
        }

        public TitleRecord(byte[] data)
        {
            var io = new EndianIO(data, EndianType.Big);
            this.TitleID = io.ReadUInt32();
            this.AchievementsPossible = io.ReadInt32();
            this.AchievementsEarned = io.ReadInt32();
            this.CreditPossible = io.ReadInt32();
            this.CreditEarned = io.ReadInt32();
            this.ReservedAchievementCount = io.ReadUInt16();
            this.AllAvatarAwards.Earned = io.ReadByte();
            this.AllAvatarAwards.Possible = io.ReadByte();
            this.MaleAvatarAwards.Earned = io.ReadByte();
            this.MaleAvatarAwards.Possible = io.ReadByte();
            this.FemaleAvatarAwards.Earned = io.ReadByte();
            this.FemaleAvatarAwards.Possible = io.ReadByte();
            this.ReservedFlags = io.ReadUInt32();
            this.LastLoaded = DateTime.FromFileTimeUtc(io.ReadInt64());
            this.TitleName = io.ReadNullTerminatedUnicodeString();
            io.Close();
        }

        public byte[] ToArray()
        {
            var io = new EndianIO(new MemoryStream(41), EndianType.Big);
            io.Write(this.TitleID);
            io.Write(this.AchievementsPossible);
            io.Write(this.AchievementsEarned);
            io.Write(this.CreditPossible);
            io.Write(this.CreditEarned);
            io.Write(this.ReservedAchievementCount);
            io.Write(this.AllAvatarAwards.Earned);
            io.Write(this.AllAvatarAwards.Possible);
            io.Write(this.MaleAvatarAwards.Earned);
            io.Write(this.MaleAvatarAwards.Possible);
            io.Write(this.FemaleAvatarAwards.Earned);
            io.Write(this.FemaleAvatarAwards.Possible);
            io.Write(this.ReservedFlags);

            if (this.LastLoaded == default(DateTime))
                io.Write(new byte[8]);
            else
                io.Write(this.LastLoaded.ToFileTimeUtc());

            io.WriteNullTerminatedUnicodeString(TitleName);
            io.Close();

            return io.ToArray();
        }
    }
}
