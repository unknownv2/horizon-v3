using System;
using System.IO;
using NoDev.Common.IO;

namespace NoDev.XProfile.Records
{
    public enum DataType : byte
    {
        Context = 0x00,
        Int32 = 0x01,
        Int64 = 0x02,
        Double = 0x03,
        Unicode = 0x04,
        Float = 0x05,
        Binary = 0x06,
        DateTime = 0x07,
        Null = 0xff,
    }

    public class SettingRecord
    {
        public readonly uint SettingID;
        public readonly DataType SettingType;

        public readonly uint Unknown1;
        public readonly byte[] Unknown2;

        public dynamic Value1, Value2;

        public SettingRecord(uint settingId, DataType settingType, dynamic value1, dynamic value2 = null)
        {
            this.SettingID = settingId;
            this.SettingType = settingType;
            this.Value1 = value1;
            this.Value2 = value2;

            this.Unknown2 = new byte[7];
        }

        public SettingRecord(byte[] data)
        {
            var io = new EndianIO(data, EndianType.Big);

            this.SettingID = io.ReadUInt32();
            this.Unknown1 = io.ReadUInt32();
            this.SettingType = (DataType)io.ReadByte();
            this.Unknown2 = io.ReadByteArray(7);

            switch (this.SettingType)
            {
                case DataType.Context:
                case DataType.Int32:
                    this.Value1 = io.ReadInt32();
                    this.Value2 = io.ReadByteArray(4);
                    break;

                case DataType.Int64:
                    this.Value1 = io.ReadInt64();
                    break;

                case DataType.Double:
                    this.Value1 = io.ReadDouble();
                    break;

                case DataType.Unicode:
                case DataType.Binary:
                    int val = io.ReadInt32();
                    this.Value2 = io.ReadUInt32();
                    this.Value1 = io.ReadByteArray(val);
                    break;

                case DataType.Float:
                    this.Value1 = io.ReadSingle();
                    this.Value2 = io.ReadByteArray(4);
                    break;

                case DataType.DateTime:
                    this.Value1 = io.ReadInt64();
                    this.Value2 = new DateTime(this.Value1);
                    break;

                default:
                    this.Value1 = io.ReadByteArray(8);
                    break;
            }

            io.Close();
        }

        public byte[] ToArray()
        {
            var io = new EndianIO(new MemoryStream(20), EndianType.Big);

            io.Write(this.SettingID);
            io.Write(this.Unknown1);
            io.Write((byte)this.SettingType);
            io.Write(this.Unknown2);

            if (this.SettingType == DataType.Binary || this.SettingType == DataType.Unicode)
            {
                io.Write(this.Value1.Length);
                io.Write(this.Value2);
                io.Write(this.Value1);
            }
            else
            {
                io.Write(this.Value1);
                if (this.Value2 != null)
                    io.Write(this.Value2);
            }
                
            io.Close();

            return io.ToArray();
        }
    }
}
