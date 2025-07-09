using NoDev.XProfile.Records;
using NoDev.Xdbf;

namespace NoDev.XProfile.Trackers
{
    public abstract class DataFileTracker
    {
        public readonly TitleRecord TitleRecord;

        protected readonly ProfileFile Profile;
        protected readonly DataFile DataFile;

        public DataFileTracker(ProfileFile profileFile, uint titleId, DataFileOrigin origin)
        {
            if (!profileFile.TitleRecordExists(titleId))
                throw new XProfileException(string.Format("Title record not found in profile (0x{0:X8}).", titleId));

            if (!profileFile.DataFileExists(titleId, origin))
                throw new XProfileException(string.Format("Data file not found in profile (0x{0:X8}).", titleId));

            this.Profile = profileFile;

            this.TitleRecord = profileFile.GetTitleRecord(titleId);

            this.DataFile = profileFile.GetDataFile(titleId, origin);
        }

        public void Flush()
        {
            this.DataFile.Flush();
        }

        public void Close()
        {
            this.DataFile.Close();
        }
    }
}
