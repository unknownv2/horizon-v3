using NoDev.XProfile.Records;
using NoDev.Xdbf;

namespace NoDev.XProfile.Trackers
{
    public class SettingsTracker
    {
        private readonly DataFile _dataFile;

        public SettingsTracker(DataFile dataFile)
        {
            this._dataFile = dataFile;
        }

        public void Close()
        {
            this.Flush();
            this._dataFile.Close();
        }

        public void Flush()
        {
            this._dataFile.Flush();
        }

        public SettingRecord GetSetting(ulong id)
        {
            return new SettingRecord(this._dataFile.GetData(Namespace.Settings, id));
        }

        public bool Exists(ulong id)
        {
            return this._dataFile.RecordExists(Namespace.Settings, id);
        }

        public void UpdateOrCreateSetting(ulong id, SettingRecord rec)
        {
            this.WriteSetting(id, rec.ToArray());
        }

        private void WriteSetting(ulong id, byte[] setting)
        {
            this._dataFile.UpdateOrInsertRecord(Namespace.Settings, id, setting);
        }

        public void UpdateOrCreateInt32Setting(ulong id, int value)
        {
            SettingRecord rec;

            if (!this.Exists(id))
                rec = new SettingRecord((uint) id, DataType.Int32, value, 0);
            else
            {
                rec = this.GetSetting(id);
                if (rec.SettingType != DataType.Int32)
                    throw new XProfileException("Cannot update a non-Int32 setting record.");
                rec.Value1 = value;
            }

            this.WriteSetting(id, rec.ToArray());
        }

        public void IncrementOrCreateInt32Setting(ulong id, int inc)
        {
            SettingRecord rec;

            if (!this.Exists(id))
                rec = new SettingRecord((uint)id, DataType.Int32, inc, 0);
            else
            {
                rec = this.GetSetting(id);
                if (rec.SettingType != DataType.Int32)
                    throw new XProfileException("Cannot increment a non-Int32 setting record.");
                rec.Value1 += inc;
            }

            this.WriteSetting(id, rec.ToArray());
        }

        public byte[] GetBinaryValue(ulong id)
        {
            var rec = this.GetSetting(id);

            if (rec.SettingType != DataType.Binary)
                throw new XProfileException("Attempted to read binary from a non-binary setting record.");

            return rec.Value1;
        }

        public void UpdateOrCreateBinaryValue(ulong id, byte[] data)
        {
            SettingRecord rec;

            if (!this.Exists(id))
                rec = new SettingRecord((uint)id, DataType.Binary, data, 0);
            else
            {
                rec = this.GetSetting(id);
                if (rec.SettingType != DataType.Binary)
                    throw new XProfileException("Attempted to write binary to a non-binary setting record.");
                rec.Value1 = data;
            }

            this.WriteSetting(id, rec.ToArray());
        }
    }
}