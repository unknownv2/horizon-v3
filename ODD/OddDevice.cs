using System;
using System.IO;
using System.Text;
using NoDev.Odfx;

namespace NoDev.Odd
{
    public class OddDevice : OdfxDevice
    {
        private readonly FileStream _fileStream;
        private readonly long _gamePartitionPosition;

        private static readonly long[] GamePartitionPositions = { 0xFDA0000, 0x02090000, 0x00010000, 0x18310000 };

        public OddDevice(string mountingPoint, string isoFileName)
            : base(mountingPoint)
        {
            _fileStream = new FileStream(isoFileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            foreach (var pos in GamePartitionPositions)
            {
                _fileStream.Position = pos;

                if (Encoding.ASCII.GetString(_fileStream.Read(0x14)) != OdfxGdfVolumeDescriptorSignature)
                    continue;

                _gamePartitionPosition = pos - 0x10000;

                break;
            }

            if (_gamePartitionPosition == 0)
                throw new Exception("Invalid Xbox 360 GDF image detected.");

            FileSize = _fileStream.Length - _gamePartitionPosition;

            MountDriver();
        }

        public override void Unmount()
        {
            base.Unmount();

            this._fileStream.Close();
        }

        protected override void DeviceControl(ulong controlCode, ref byte[] outputBuffer)
        {
            if (controlCode != 0x2404c)
                return;

            outputBuffer.WriteInt32(0x04, 0x800);
            outputBuffer.WriteInt32(0x00, (int)((_fileStream.Length - _gamePartitionPosition - 0xFD80000) >> 0x0B));
        }

        protected override byte[] FscMapBuffer(long physicalOffset, long dataSize)
        {
            _fileStream.Position = _gamePartitionPosition + physicalOffset;

            return _fileStream.Read((int)dataSize);
        }
    }
}