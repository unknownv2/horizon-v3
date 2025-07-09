using System;
using System.Collections.Generic;
using System.Linq;
using NoDev.XProfile.Records;
using NoDev.Xdbf;

namespace NoDev.XProfile.Trackers
{
    public class AchievementTracker : DataFileTracker
    {
        public readonly IReadOnlyCollection<AchievementRecord> Achievements;
        private readonly List<AchievementRecord> _achievements; 

        public AchievementTracker(ProfileFile profileFile, uint titleId)
            : base(profileFile, titleId, DataFileOrigin.Profile)
        {
            this._achievements = new List<AchievementRecord>(
                this.DataFile.GetRecords(Namespace.Achievements).Select(
                    r => new AchievementRecord(this.DataFile.GetData(r))));

            if (this._achievements.Count != this.TitleRecord.AchievementsPossible)
                throw new XProfileException(string.Format("Achievement count does not match total possible in title record (0x{0:X8}).", titleId));

            this.Achievements = this._achievements.AsReadOnly();
        }

        public void SortAchievements(Comparison<AchievementRecord> comp)
        {
            this._achievements.Sort(comp);
        }

        public void AddAchievement(AchievementRecord ach)
        {
            if (this._achievements.Any(a => a.ID == ach.ID))
                throw new XProfileException(string.Format("Attempted to create a duplicate achievement record (0x{0:X8}).", ach.ID));

            this.DataFile.UpdateOrInsertRecord(Namespace.Achievements, ach.ID, ach.ToArray());

            this._achievements.Add(ach);
        }

        public byte[] GetAchievementTile(AchievementRecord ach)
        {
            if (!ach.Achieved && !ach.AchievedOnline)
                throw new XProfileException("Attempted to retrieve a tile for a locked achievement.");

            var imgRecord = this.DataFile.GetRecord(Namespace.Images, ach.ImageID);

            if (imgRecord == null)
                return null;

            return this.DataFile.GetData(imgRecord);
        }

        public void UnlockAchievementOnline(AchievementRecord ach, DateTime timeAchieved, byte[] tile = null)
        {
            this.UnlockAchievement(ach, tile);

            ach.DateTimeAchieved = timeAchieved;
            ach.AchievedOnline = true;
        }

        public void LockAchievement(AchievementRecord ach)
        {
            if (!ach.Achieved && !ach.AchievedOnline)
                throw new Exception("Attempted to lock and already locked achievement.");

            if (this.TitleRecord.CreditEarned - ach.Credit < 0)
                throw new XProfileException("Attempted to set credit earned below zero for title.");

            if (this.TitleRecord.AchievementsEarned - 1 < 0)
                throw new XProfileException("Attempted to set achievements earned below zero for title.");

            ach.Achieved = false;
            ach.AchievedOnline = false;
            ach.DateTimeAchieved = DateTime.FromFileTimeUtc(0);

            this.DataFile.UpdateData(Namespace.Achievements, ach.ID, ach.ToArray());

            this.DataFile.RemoveSyncRecord(Namespace.Achievements, ach.ID);

            if (this.DataFile.RecordExists(Namespace.Images, ach.ImageID))
                this.DataFile.RemoveRecord(Namespace.Images, ach.ImageID);

            this.TitleRecord.AchievementsEarned--;
            this.TitleRecord.CreditEarned -= ach.Credit;
            this.Profile.UpdateTitleRecord(this.TitleRecord);

            this.Profile.Settings.IncrementOrCreateInt32Setting(XProfileID.GamercardAchievementsEarned, -1);
            this.Profile.Settings.IncrementOrCreateInt32Setting(XProfileID.GamercardCreditEarned, -ach.Credit);
        }

        public bool IsPendingSync(AchievementRecord ach)
        {
            return this.DataFile.IsPendingSync(Namespace.Achievements, ach.ID);
        }

        public void UnlockAchievement(AchievementRecord ach, byte[] tile = null)
        {
            if (ach.Achieved || ach.AchievedOnline)
                throw new XProfileException("Attempted to unlock an already unlocked achievement.");

            if (this.TitleRecord.CreditEarned + ach.Credit > this.TitleRecord.CreditPossible)
                throw new XProfileException("Attempted to add credit passed the title total.");

            if (this.TitleRecord.AchievementsEarned + 1 > this.TitleRecord.AchievementsPossible)
                throw new XProfileException("Attempted to add achievement passed the title total.");

            ach.Achieved = true;

            this.DataFile.UpdateData(Namespace.Achievements, ach.ID, ach.ToArray());

            if (tile != null)
                this.DataFile.UpdateOrInsertRecord(Namespace.Images, ach.ImageID, tile);

            this.TitleRecord.AchievementsEarned++;
            this.TitleRecord.CreditEarned += ach.Credit;
            this.Profile.UpdateTitleRecord(this.TitleRecord);

            this.Profile.Settings.IncrementOrCreateInt32Setting(XProfileID.GamercardAchievementsEarned, 1);
            this.Profile.Settings.IncrementOrCreateInt32Setting(XProfileID.GamercardCreditEarned, ach.Credit);
        }
    }
}