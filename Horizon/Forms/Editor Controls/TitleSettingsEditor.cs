using System;
using System.IO;
using NoDev.Common.IO;
using NoDev.XProfile;
using NoDev.XProfile.Trackers;
using NoDev.Xdbf;

namespace NoDev.Horizon
{
    internal class TitleSettingsEditor : ProfileEditor
    {
        private SettingsTracker _settings;

        protected override void BeforeOpen()
        {
            this._settings = new SettingsTracker(this.Profile.GetDataFile(this.Info.TitleID, DataFileOrigin.Profile));
        }

        protected override void CloseStreams()
        {
            if (this._settings != null)
                this._settings.Close();

            base.CloseStreams();
        }

        protected override void AfterSave()
        {
            if (this._settings != null)
                this._settings.Flush();
            base.AfterSave();
        }

        private int _titleSettingSize;
        protected int TitleSettingSize
        {
            get
            {
                return this._titleSettingSize;
            }
            set
            {
                if (value > 1000)
                    throw new Exception("TitleSettingsSize must not exceed 1000 bytes!");

                this._titleSettingSize = value;
            }
        }

        protected void LoadFullTitleSetting(EndianType endianType)
        {
            var io = new EndianIO(new MemoryStream(this._titleSettingSize * 3), endianType);

            io.Write(this.ReadTitleSetting(XProfileID.TitleSpecific1));

            this.IO = io;

            if (!_settings.Exists(XProfileID.TitleSpecific2))
                return;

            io.Write(_settings.GetBinaryValue(XProfileID.TitleSpecific2));

            if (_settings.Exists(XProfileID.TitleSpecific3))
                io.Write(_settings.GetBinaryValue(XProfileID.TitleSpecific3));
        }

        protected void LoadTitleSetting(ulong id, EndianType endianType)
        {
            byte[] titleSetting = this.ReadTitleSetting(id);
            var io = new EndianIO(new MemoryStream(titleSetting.Length), endianType);
            io.Write(titleSetting);
            this.IO = io;
        }

        private byte[] ReadTitleSetting(ulong id)
        {
            if (!this._settings.Exists(id))
                throw new Exception("Title setting not found in data file.");

            return this._settings.GetBinaryValue(id);
        }

        protected void SaveFullTitleSetting()
        {
            this.SaveFullTitleSetting(this.IO.ToArray());
        }

        protected void SaveFullTitleSetting(EndianIO io)
        {
            this.SaveFullTitleSetting(io.ToArray());
        }

        protected void SaveFullTitleSetting(byte[] data)
        {
            if (data == null)
                throw new Exception("Attempted to write null title record data.");

            if (data.Length > this.TitleSettingSize * 3)
                throw new Exception(String.Format("Title settings data exceeds the maximum defined length of {0} bytes.", this._titleSettingSize * 3));

            var dataSizes = new int[3];
            int lastSetting = (int)Math.Ceiling((double)data.Length / this._titleSettingSize) - 1;
            for (int x = 0; x < lastSetting; x++)
                dataSizes[x] = this._titleSettingSize;
            dataSizes[lastSetting] = this._titleSettingSize - (data.Length % this._titleSettingSize);
            if (dataSizes[lastSetting] == 0)
                dataSizes[lastSetting] = this._titleSettingSize;

            var titleSetting = new byte[dataSizes[0]];
            Array.Copy(data, titleSetting, titleSetting.Length);
            this.WriteTitleSetting(XProfileID.TitleSpecific1, titleSetting);

            titleSetting = new byte[dataSizes[1]];
            Array.Copy(data, dataSizes[0], titleSetting, 0, dataSizes[1]);
            this.WriteTitleSetting(XProfileID.TitleSpecific2, titleSetting);

            titleSetting = new byte[dataSizes[2]];
            Array.Copy(data, dataSizes[0] + dataSizes[1], titleSetting, 0, dataSizes[2]);
            this.WriteTitleSetting(XProfileID.TitleSpecific3, titleSetting);
        }

        protected void SaveTitleSetting(ulong id, EndianIO io)
        {
            this.WriteTitleSetting(id, io.ToArray());
        }

        protected void SaveTitleSetting(ulong id)
        {
            this.WriteTitleSetting(id, this.IO.ToArray());
        }

        protected void SaveTitleSetting(ulong id, byte[] data)
        {
            this.WriteTitleSetting(id, data);
        }

        private void WriteTitleSetting(ulong id, byte[] data)
        {
            if (data.Length > this.TitleSettingSize)
                throw new Exception(String.Format("Title setting data exceeds maximum defined length of {0} bytes.", this._titleSettingSize));

            this._settings.UpdateOrCreateBinaryValue(id, data);
        }
    }
}
