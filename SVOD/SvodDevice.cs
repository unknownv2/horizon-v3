using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NoDev.Common.IO;
using NoDev.Odfx;
using NoDev.Xbox360;

namespace NoDev.Svod
{
    public class SvodDevice : OdfxDevice
    {
        private readonly Stream[] _ioArr;
        private Stream _io;

        private readonly List<SvodCacheElement> _blockCache;
        private readonly byte[][] _cacheData, _cacheHashData;

        private readonly bool _hasEnhancedGdfLayout;
        private readonly long _dataStartOffset, _dataBlockLength;

        private byte _topLevelHashCount, _cacheHeadIndex;

        private readonly uint[] _svodDataBlocksPerHashTreeLevel = new uint[] { 0xCC, 0xA290, 0x818AC0 };

        public SvodDevice(string mountingPoint, string dataFileDirectory, SvodCreatePacket createPacket)
            : base(mountingPoint)
        {
            if (createPacket.Descriptor == null)
                throw new SvodException("Invalid volume descriptor.");

            if (createPacket.FsCache == null)
                throw new SvodException("Invalid file system cache.");

            var descriptor = createPacket.Descriptor;

            if (descriptor.DescriptorLength != 0x24)
                throw new SvodException(string.Format("Invalid volume descriptor length 0x{0:X8}.", createPacket.Descriptor.DescriptorLength));

            var cacheElementCount = descriptor.BlockCacheElementCount;

            if (cacheElementCount == 0x00)
                throw new SvodException(string.Format("Invalid cache element count."));

            if (descriptor.Features.MustBeZeroForFutureUsage != 0x00)
                throw new SvodException(string.Format("Unsupported device features 0x{0:X2} [0xC0000001].", descriptor.Features.Flags & 0x3f));

            uint startingDataBlock;

            if (((descriptor.Features.Flags >> 6) & 0x1) != 0x01)
                startingDataBlock = BitConverter.GetBytes(descriptor.StartingDataBlock)[0];
            else
            {
                startingDataBlock = descriptor.StartingDataBlock;

                if (startingDataBlock <= 0x10)
                    throw new SvodException(string.Format("Unexpected starting data block: 0x{0:X8} [0xC0000001].", startingDataBlock));
            }

            var numberOfDataBlocks = descriptor.NumberOfDataBlocks;

            if (((descriptor.Features.Flags >> 26) & 0x1) == 0x01)
                numberOfDataBlocks++;

            var fragmentCount = (ushort)((numberOfDataBlocks + 0xa1c3) / 0xa1c4);

            if (fragmentCount > 0x38)
                throw new SvodException(string.Format("Fragment count too high: 0x{0:X4}.", fragmentCount));

            if (cacheElementCount != 0x00)
            {
                this._blockCache = new List<SvodCacheElement>(cacheElementCount);
                this._cacheData = new byte[cacheElementCount][];

                for (var x = 0; x < cacheElementCount; x++)
                {
                    this._blockCache.Add(new SvodCacheElement {
                        BlockNumber = 0x00,
                        Type = 0xFF,
                        NextIndex = (byte)(x == cacheElementCount - 1 ? 0x00 : x + 1),
                        PreviousIndex = (byte)((x == 0 ? cacheElementCount : x) + 0xff)
                    });

                    this._cacheData[x] = new byte[0x1000];
                }

                this._cacheHashData = new byte[0x38][];

                for (var x = 0; x < 0x38; x++)
                    this._cacheHashData[x] = new byte[0x14];

                this._cacheHashData[0] = descriptor.FirstFragmentHashEntry.Hash;
            }

            _dataBlockLength = (long)numberOfDataBlocks << 0x0C;
            _dataStartOffset = ((long)startingDataBlock).RotateLeft(0x0C) & 0xFFFFFFFF000;

            _hasEnhancedGdfLayout = descriptor.Features.HasEnhancedGDFLayout;

            if (!Directory.Exists(dataFileDirectory))
                throw new DirectoryNotFoundException("SVOD: Data file folder not found.");

            var dataStreams = new Stream[fragmentCount];

            var dirFormat = dataFileDirectory + "\\Data";

            for (int x = 0; x < fragmentCount; x++)
            {
                var filePath = dirFormat + x.ToString("D4");

                if (!File.Exists(filePath))
                    throw new FileNotFoundException(string.Format("SVOD: Data file not found ({0}).", x));

                dataStreams[x] = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }

            _ioArr = dataStreams;

            FileSize = _dataBlockLength;

            MountDriver();
        }

        [Obfuscation]
        public override void Unmount()
        {
            base.Unmount();

            foreach (var s in this._ioArr)
                s.Close();
        }

        protected override void DeviceControl(ulong controlCode, ref byte[] outputBuffer)
        {
            if (controlCode != 0x2404c)
                return;

            outputBuffer.WriteInt32(0x04, 0x800);
            outputBuffer.WriteInt32(0x00, (int)((_dataStartOffset + _dataBlockLength) >> 0x0B));
        }

        protected override byte[] FscMapBuffer(long physicalOffset, long dataSize)
        {
            var io = new EndianIO(new MemoryStream(), EndianType.Big);

            var lowOffset = physicalOffset & 0xFFF;

            physicalOffset = (physicalOffset >> 0x0C & 0xFFFFFFFF).RotateLeft(0x0C) & 0xFFFFFFFF000;

            var length = dataSize + lowOffset;

            int err;

            long numberOfBytesToRead = length, len = length, fileOffset = 0;

            do
            {
                if (len == 0x00)
                    break;

                if (physicalOffset < this._dataStartOffset)
                {
                    if (!this._hasEnhancedGdfLayout || ((physicalOffset >> 0x0B >> 52) + physicalOffset >> 0x0C) != 0x10)
                        throw new SvodException("Invalid read request.");

                    var bytesToRead = 0x1000 - (physicalOffset & 0xfff);

                    numberOfBytesToRead = len > bytesToRead ? bytesToRead : numberOfBytesToRead;
                }
                else
                {
                    fileOffset = physicalOffset - _dataStartOffset;

                    if (this._hasEnhancedGdfLayout)
                        fileOffset += 0x1000;
                }

                if (numberOfBytesToRead < 0x1000 || ((ulong)((numberOfBytesToRead & 0xFFFFFFFF) + physicalOffset ^ physicalOffset + 0xFFF) & 0xFFFFFFFFFFFFF000) == 0x00)
                    err = this.SvodFullyCachedRead(io, fileOffset, numberOfBytesToRead);
                else
                    err = this.SvodPartiallyCachedRead(io, fileOffset, numberOfBytesToRead);

                len -= numberOfBytesToRead;
                fileOffset += numberOfBytesToRead;

            } while (err >= 0x00);


            if (lowOffset == 0x00)
                return io.ToArray();

            io.Position = lowOffset;

            return io.ReadByteArray(dataSize);
        }

        private int SvodPartiallyCachedRead(EndianIO io, long position, long length)
        {
            long pos = position & 0xfff, pos2 = position, len;

            if (pos == 0x00)
                len = length;
            else
            {
                len = 0x1000 - pos;

                if (len >= length)
                    throw new SvodException(string.Format("Invalid length supplied for partial cache reading: 0x{0:X8}.", length));

                this.SvodFullyCachedRead(io, pos2, len);

                pos2 += len & 0xffffffff;
                len = length - len;
            }

            while (len >= 0x1000)
            {
                uint
                    blockNum = (uint)((pos2 >> 0x0b >> 52) + pos2 >> 0x0c),
                    numberOfBytesToRead = (uint)(len & 0xfffff000),
                    nBlockNum = blockNum % _svodDataBlocksPerHashTreeLevel[0],
                    remainderBytes = 0xcc - nBlockNum << 0x0c;

                if (numberOfBytesToRead > remainderBytes)
                    numberOfBytesToRead = remainderBytes;

                byte[] hashBlock;

                if (this.SvodMapBlock(blockNum, 0x01, out hashBlock) < 0x00)
                    continue;

                uint fragmentIndex;

                var dataBlock = this.SvodReadBackingBlocks(blockNum, 0x00, numberOfBytesToRead >> 0x0c, out fragmentIndex);

                if (dataBlock == null)
                    continue;

                pos2 += numberOfBytesToRead & 0xffffffff;
                len -= numberOfBytesToRead;

                io.Write(dataBlock);

                int pHashIdx = (int)nBlockNum * 0x14, pDataBlockIdx = 0x00;

                do
                {
                    if (!hashBlock.Read(pHashIdx, 0x14).SequenceEqual(XeCrypt.XeCryptSha(dataBlock.Read(pDataBlockIdx, 0x1000))))
                        throw new SvodException(string.Format("Hash mismatch for block 0x{0:X8} [0xC0000032].", blockNum));

                    pHashIdx += 0x14;
                    pDataBlockIdx += 0x1000;

                } while ((numberOfBytesToRead -= 0x1000) > 0x00);
            }

            return len == 0x00 ? 0x00 : this.SvodFullyCachedRead(io, pos2, len);
        }

        private int SvodFullyCachedRead(EndianIO io, long position, long length)
        {
            var err = 0x00;

            if (length == 0)
                return err;

            long pos = position & 0xfff, len = 0x1000 - pos;

            if (len > length)
                len = length;

            while (length > 0x00)
            {
                byte[] dataBlock;

                err = this.SvodMapBlock((uint)((position >> 0x0b >> 52) + position >> 0x0c), 0x00, out dataBlock);

                if (err < 0)
                    continue;

                io.Write(dataBlock.Read(pos, len));

                length -= len;
                position += len & 0xffffffff;
            }

            return err;
        }

        private int SvodMapBlock(uint blockNumber, int elementType, out byte[] dataBlock)
        {
            var elementT = elementType;

            if (this.SvodMapExistingBlock(blockNumber, elementType, out dataBlock) != 0x00)
                return 0x00;

            int err;

            byte[] dbHashEntry;

            while (elementType != 0x02)
            {
                elementType++;

                err = this.SvodMapExistingBlock(blockNumber, elementType, out dataBlock);

                if (err != 0x00)
                    goto Map_New_Block;
            }

            var hashBlockNum = (int)(blockNumber / 0xa1c4);

            byte[] pbDataBlock;

            while (this._topLevelHashCount < hashBlockNum &&
                this.SvodMapNewBlock((uint)(this._topLevelHashCount * 0xa1c4), 0x02,
                    this._cacheHashData[this._topLevelHashCount], out pbDataBlock) >= 0x00)
            {
                // :o
            }

            dbHashEntry = this._cacheHashData[hashBlockNum];

            err = this.SvodMapNewBlock(blockNumber, elementType, dbHashEntry, out dataBlock);

            Map_New_Block:

            if (err < 0x00)
                return 0x00;

            do
            {
                if (elementType == elementT)
                    break;

                var hashIndex = (int)(elementType == 0x02 
                    ? (blockNumber / _svodDataBlocksPerHashTreeLevel[0]) % 0xcb 
                    : (blockNumber % _svodDataBlocksPerHashTreeLevel[0]));

                dbHashEntry = dataBlock.Read(hashIndex * 0x14, 0x14);

                elementType--;

            } while (this.SvodMapNewBlock(blockNumber, elementType, dbHashEntry, out dataBlock) >= 0x00);

            return 0x00;
        }

        private int SvodMapNewBlock(uint blockNumber, int elementType, IEnumerable<byte> blockHash, out byte[] dataBlock)
        {
            uint blockNum;

            switch (elementType)
            {
                case 0x01:
                    var sqr = _svodDataBlocksPerHashTreeLevel[0];
                    blockNum = blockNumber / sqr * sqr;
                    break;

                case 0x02:
                    blockNum = blockNumber / 0xa1c4 * 0xa1c4;
                    break;

                default:
                    blockNum = blockNumber;
                    break;
            }

            if (elementType > 0x00)
                blockNum = blockNumber - (blockNumber - blockNum);

            int cacheIndex = this._blockCache[this._cacheHeadIndex].PreviousIndex;

            var cacheEntry = this._blockCache[cacheIndex];

            uint fragmentIndex;

            dataBlock = this._cacheData[cacheEntry.NextIndex] = this.SvodReadBackingBlocks(blockNum, elementType, 0x01, out fragmentIndex);

            if (!blockHash.SequenceEqual(XeCrypt.XeCryptSha(dataBlock)))
                throw new SvodException(string.Format("Hash mismatch for block number 0x{0:X8} [0xC0000032].", blockNum));

            this._cacheHeadIndex = (byte)cacheIndex;

            cacheEntry.BlockNumber = blockNum;
            cacheEntry.Type = (byte)elementType;

            if (elementType == 0x02 && fragmentIndex == _topLevelHashCount && fragmentIndex < 0x38)
            {
                this._cacheHashData[fragmentIndex + 1] = dataBlock.Read(0xfdc, 0x14);

                _topLevelHashCount++;
            }

            return 0x00;
        }

        private int SvodMapExistingBlock(uint blockNumber, int elementType, out byte[] blockData)
        {
            uint blockNum;
            
            switch (elementType)
            {
                case 0x00:
                    var sqr = _svodDataBlocksPerHashTreeLevel[0];
                    blockNum = blockNumber / sqr * sqr;
                    break;

                case 0x01:
                case 0x02:
                    blockNum = (blockNumber / 0xA1C4) * 0xA1C4;
                    break;

                default:
                    blockNum = 0xffffffff;
                    break;
            }

            blockNum = blockNumber - (blockNumber - blockNum);

            int cacheIndex = this._cacheHeadIndex;

            do
            {
                var cacheEntry = this._blockCache[cacheIndex];

                if (cacheEntry.BlockNumber == blockNum && cacheEntry.Type == elementType)
                {
                    this._cacheHeadIndex = cacheEntry.NextIndex;

                    blockData = this._cacheData[cacheEntry.NextIndex];

                    return 0x01;
                }

                cacheIndex = cacheEntry.NextIndex;

            } while (cacheIndex != this._cacheHeadIndex);

            blockData = null;

            return 0x00;
        }

        private byte[] SvodReadBackingBlocks(uint blockNumber, int blockLevel, uint blockCount, out uint fragmentIndex)
        {
            if (blockLevel != 0x00 && blockCount != 0x01)
                throw new SvodException(string.Format("Invalid block count {0} detected while reading hash block 0x{1:X8}.", blockCount, blockNumber));

            const uint blocksPerLevel = 0xa290;

            uint blockNum;

            switch (blockLevel)
            {
                case 0x00:
                    blockNum = blockNumber / 0xcc + blockNumber / 0xa1c4 + blockNumber + 2;
                    break;
                case 0x01:
                    blockNum = blockNumber / 0xcc * 0xcd + blockNumber / 0xa1c4 + 1;
                    break;
                case 0x02:
                    blockNum = blockNumber / 0xa1c4 * blocksPerLevel;
                    break;
                default:
                    throw new SvodException(string.Format("Invalid block level {0} detected while reading block 0x{1:X8}.", blockLevel, blockNumber));
            }

            fragmentIndex = blockNum / blocksPerLevel;

            this._io = this._ioArr[fragmentIndex];

            this._io.Position = ((blockNum - (fragmentIndex * blocksPerLevel)) << 0x0C) & 0xFFFFFFFF;

            var blockLen = (int)(blockCount << 0x0c);

            var blockData = new byte[blockLen];

            if (this._io.Read(blockData, 0, blockLen) != blockLen)
                throw new SvodException(string.Format("Invalid block length read 0x{0:X8}:0x{1:X8} [0xC0000185].", fragmentIndex, blockLen));

            return blockData;
        }
    }

    public struct SvodCacheElement
    {
        public uint BlockNumber;
        public byte Type;
        public byte NextIndex;
        public byte PreviousIndex;
    }
}