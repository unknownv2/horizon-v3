using System;
using System.Collections.Generic;
using System.Linq;
using NoDev.XProfile.Records;
using NoDev.Xdbf;

namespace NoDev.XProfile.Trackers
{
    public class AvatarAwardTracker : DataFileTracker
    {
        public readonly IReadOnlyCollection<AvatarAwardRecord> Awards;
        private readonly List<AvatarAwardRecord> _awards; 

        public AvatarAwardTracker(ProfileFile profileFile, uint titleId)
            : base(profileFile, titleId, DataFileOrigin.PEC)
        {
            this._awards = new List<AvatarAwardRecord>(
                this.DataFile.GetRecords(Namespace.Avatar).Select(
                    r => new AvatarAwardRecord(this.DataFile.GetData(r))));

            if (this._awards.Count == 0)
                return;

            if (this._awards.Count != this.TitleRecord.AllAvatarAwards.Possible)
                throw new XProfileException(string.Format("Award count does not match total possible in title record (0x{0:X8}).", titleId));

            byte maleCount = 0, femaleCount = 0;
            this._awards.ForEach(aw => {
                switch (aw.BodyType)
                {
                    case AssetBodyType.Male:
                        maleCount++;
                        break;
                    case AssetBodyType.Female:
                        femaleCount++;
                        break;
                    case AssetBodyType.Both:
                        maleCount++;
                        femaleCount++;
                        break;
                }
            });

            if (maleCount != this.TitleRecord.MaleAvatarAwards.Possible)
                throw new XProfileException(string.Format("Male award count does not match total possible in title record (0x{0:X8}).", titleId));

            if (femaleCount != this.TitleRecord.FemaleAvatarAwards.Possible)
                throw new XProfileException(string.Format("Female award count does not match total possible in title record (0x{0:X8}).", titleId));

            this.Awards = this._awards.AsReadOnly();
        }

        public void AddAward(AvatarAwardRecord aw)
        {
            if (this._awards.Any(a => a.ID == aw.ID))
                throw new XProfileException(string.Format("Attempted to create a duplicate avatar award record (0x{0:X16}).", aw.ID));

            this.DataFile.UpdateOrInsertRecord(Namespace.Avatar, aw.ID, aw.ToArray());

            this._awards.Add(aw);
        }

        public byte[] GetAwardTile(AvatarAwardRecord aw)
        {
            if (!aw.Awarded)
                throw new XProfileException("Attempted to retrieve a tile for an unawarded avatar asset.");

            var imgRecord = this.DataFile.GetRecord(Namespace.Images, aw.ImageID);

            if (imgRecord == null)
                throw new XProfileException("Attempted to retrieve a non-existant avatar award tile.");

            return this.DataFile.GetData(imgRecord);
        }

        public void AwardAssetOnline(AvatarAwardRecord aw, DateTime timeAchieved)
        {
            this.AwardAsset(aw);

            aw.DateTimeAwarded = timeAchieved;
            aw.AwardedOnline = true;
        }

        private void AwardAsset(AvatarAwardRecord aw)
        {
            if (aw.Awarded)
                throw new XProfileException("Attempted to award an already awarded avatar asset.");

            if (this.TitleRecord.AllAvatarAwards.Earned + 1 > this.TitleRecord.AllAvatarAwards.Possible)
                throw new XProfileException("Attempted to award an avatar asset passed the title total.");

            var bodyType = aw.BodyType;

            if (bodyType == AssetBodyType.Male || bodyType == AssetBodyType.Both)
                if (this.TitleRecord.MaleAvatarAwards.Earned + 1 > this.TitleRecord.MaleAvatarAwards.Possible)
                    throw new XProfileException("Attempted to award a male avatar asset passed the title total.");
            
            if (bodyType == AssetBodyType.Female || bodyType == AssetBodyType.Both)
                if (this.TitleRecord.FemaleAvatarAwards.Earned + 1 > this.TitleRecord.FemaleAvatarAwards.Possible)
                    throw new XProfileException("Attempted to award a female avatar asset passed the title total.");

            aw.Awarded = true;

            this.DataFile.UpdateData(Namespace.Avatar, aw.ID, aw.ToArray());

            this.TitleRecord.AllAvatarAwards.Earned++;

            if (bodyType == AssetBodyType.Male || bodyType == AssetBodyType.Both)
                this.TitleRecord.MaleAvatarAwards.Earned++;

            if (bodyType == AssetBodyType.Female || bodyType == AssetBodyType.Both)
                this.TitleRecord.FemaleAvatarAwards.Earned++;

            this.TitleRecord.SetTitleListRecordInfo(0x10);

            this.Profile.UpdateTitleRecord(this.TitleRecord);
        }
    }
}