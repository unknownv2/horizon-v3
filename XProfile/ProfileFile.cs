using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NoDev.Common.IO;
using NoDev.XProfile.Records;
using NoDev.XProfile.Trackers;
using NoDev.Xdbf;

namespace NoDev.XProfile
{
    public class ProfileFile
    {
        public const uint TitleID = 0xfffe07d1;

        public readonly XProfileAccount Account;
        public readonly SettingsTracker Settings;

        private readonly DriveInfo _drive;
        private readonly DataFile _profileData;
        private readonly string _accountPath;
        private readonly ulong _profileId;

        private ProfileEmbeddedContent _pec;
        private ProfileEmbeddedContent PEC
        {
            get
            {
                if (this._pec == null)
                {
                    bool e = File.Exists(this._pecFilePath);
                    var io = new EndianIO(this._pecFilePath, EndianType.Big, FileMode.OpenOrCreate);
                    this._pec = e ? new ProfileEmbeddedContent(io) : new ProfileEmbeddedContent(io, this._profileId);
                }

                return this._pec;
            }
        }

        private readonly string _pecFilePath;

        public ProfileFile(DriveInfo drive, ulong profileId)
        {
            this._drive = drive;

            this._accountPath = drive.Name + "Account";
            if (!File.Exists(this._accountPath))
                throw new XProfileException("Account file not found.");

            this.Account = new XProfileAccount(File.ReadAllBytes(this._accountPath));

            string dataFilePath = this.TitleIDToFilePath(TitleID, DataFileOrigin.Profile);
            if (!File.Exists(dataFilePath))
                throw new XProfileException("Dashboard data not found in profile.");

            this._profileData = new DataFile(dataFilePath, DataFileOrigin.Profile);

            this.Settings = new SettingsTracker(this._profileData);

            this._pecFilePath = drive.Name + "PEC";

            this._profileId = profileId;
        }

        public void Close()
        {
            if (this._pec != null)
                this._pec.Close();

            this.Flush();

            this._profileData.Close();
        }

        public void Flush()
        {
            this._profileData.Flush();
        }

        public void SaveAccount()
        {
            File.WriteAllBytes(this._accountPath, this.Account.ToArray());
        }

        public bool TitleRecordExists(uint titleId)
        {
            return this._profileData.RecordExists(Namespace.Titles, titleId);
        }

        public IEnumerable<TitleRecord> GetAllTitleRecords()
        {
            return this._profileData.GetRecords(Namespace.Titles).Select(r => new TitleRecord(this._profileData.GetData(r)));
        }

        public TitleRecord GetTitleRecord(uint titleId)
        {
            return new TitleRecord(this._profileData.GetData(Namespace.Titles, titleId));
        }

        public void UpdateTitleRecord(TitleRecord titleRecord)
        {
            this._profileData.UpdateData(Namespace.Titles, titleRecord.TitleID, titleRecord.ToArray());
        }

        public void AddTitleRecord(TitleRecord titleRecord, byte[] tile = null)
        {
            if (this.TitleRecordExists(titleRecord.TitleID))
                throw new XProfileException(string.Format("Attempted to add an already existing title record (0x{0:X8}).", titleRecord.TitleID));

            this._profileData.UpdateOrInsertRecord(Namespace.Titles, titleRecord.TitleID, titleRecord.ToArray());

            this.Settings.IncrementOrCreateInt32Setting(XProfileID.GamercardTitlesPlayed, 1);

            var dataFile = new DataFile(this.TitleIDToFilePath(titleRecord.TitleID, DataFileOrigin.Profile), 0x200, DataFileOrigin.Profile);

            if (tile != null)
                dataFile.UpdateOrInsertRecord(Namespace.Images, 0x8000, tile);

            dataFile.UpdateOrInsertRecord(Namespace.Strings, 0x8000, titleRecord.TitleName.GetNullTerminatedBigEndianUnicodeArray());

            dataFile.Close();

            if (titleRecord.AllAvatarAwards.Possible == 0)
                return;

            dataFile = new DataFile(this.TitleIDToFilePath(titleRecord.TitleID, DataFileOrigin.PEC), DataFileOrigin.PEC);

            dataFile.Close();
        }

        public DataFile CreateDataFile(uint titleId, string titleName, byte[] tile, DataFileOrigin origin)
        {
            var newTitle = new DataFile(this.TitleIDToFilePath(titleId, origin), 0x200, origin);

            if (tile != null)
                newTitle.UpdateOrInsertRecord(Namespace.Images, 0x8000, tile);

            newTitle.UpdateOrInsertRecord(Namespace.Strings, 0x8000, titleName.GetNullTerminatedBigEndianUnicodeArray());

            return newTitle;
        }

        public bool DataFileExists(uint titleId, DataFileOrigin origin)
        {
            return File.Exists(this.TitleIDToFilePath(titleId, origin));
        }

        public DataFile GetDataFile(uint titleId, DataFileOrigin origin)
        {
            if (origin == DataFileOrigin.Profile && titleId == TitleID)
                throw new XProfileException("Attempted to retrieve the profile data file.");

            return new DataFile(this.TitleIDToFilePath(titleId, origin), origin);
        }

        public string TitleIDToFilePath(uint titleId, DataFileOrigin origin)
        {
            return (origin == DataFileOrigin.Profile ? this._drive.Name : this.PEC.Drive.Name) + string.Format("{0:X8}.gpd", titleId);
        }
    }
}