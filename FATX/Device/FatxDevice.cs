using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CallbackFS;
using NoDev.Common;
using NoDev.Common.IO;

namespace NoDev.Fatx.Device
{
    using ACCESS_MASK = UInt32;
    using NTSTATUS = UInt32;

    internal class FatxDevice
    {
        private readonly object _threadLocker = new object();

        private readonly CallbackFileSystem _drive;
        private readonly EndianIO _io;

        private FatxFcb _rootDcb;

        private readonly FatxPartitionType _partitionType;

        private readonly long _deviceSize, _deviceOffset;

        private uint _bootSectorSize, _clusterSize, _clusterSizeM1, _clusterSizeM1N;
        private uint _clusterCount, _freedClusterCount, _lastFreeClusterIndex, _sectorsPerClusterM1;
        private long _backingClusterOffset;
        private byte _chainShift, _clusterShift, _allocationShift;

        internal static readonly uint[] FatxFatIllegalTable = new uint[] { 0xFFFFFFFF, 0xFC009C04, 0x10000000, 0x10000000 };

        internal FatxDevice(string mountingPoint, EndianIO io, FatxPartitionType partitionType, long deviceOffset, long deviceSize)
        {
            this._io = io;

            _partitionType = partitionType;
            _deviceOffset = deviceOffset;
            _deviceSize = deviceSize;

            CallbackFileSystem.Initialize("44D14AF2-249C-408D-ACD3-A0BA7C8E1E11");

            var drive = new CallbackFileSystem();

            drive.ParallelProcessingAllowed = true;
            drive.SerializeCallbacks = false;
            drive.FileCacheEnabled = true;

            this._drive = drive;

            this.RegisterCBFSEventHandlers();
            
            drive.CreateStorage();
            drive.MountMedia(00);
            drive.SetFileSystemName("FATX");

            drive.AddMountingPoint(mountingPoint ?? Win32.GetFreeDriveLetter() + ":");
        }

        internal void Unmount()
        {
            this._drive.DeleteMountingPoint(0);
            this._drive.UnmountMedia(true);
            this._drive.DeleteStorage(true);
        }

        private void CbFsMountEvent(CallbackFileSystem sender)
        {
            _rootDcb = new FatxFcb
            {
                State = 0x06, 
                FileAttributes = 0x03, 
                EndOfFile = 0xffffffff, 
                ReferenceCount = 0x01
            };

            FatxResetClusterCache(this._rootDcb);

            FatxProcessBootSector(_partitionType);

            FatxInitializeAllocationSupport();
        }

        private static void CbFsUnmountEvent(CallbackFileSystem sender) { }
        private static void CbFsStorageEjectedEvent(CallbackFileSystem sender) { }
        private static void CbFsGetFileNameByFileIdEvent(CallbackFileSystem sender, long fileId, ref string filePath, ref ushort filePathLength) { }
        private static void CbFsFlushFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo) { }

        private void CbFsSetFileAttributesEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo,
            DateTime creationTime, DateTime lastAccessTime, DateTime lastWriteTime, uint attributes)
        {
            if (attributes == 0x00 || fileInfo.UserContext.Equals(IntPtr.Zero))
                return;

            lock (_threadLocker)
            {
                var handle = (IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target;

                var infoWriter = new EndianIO(new MemoryStream(), EndianType.Big);

                infoWriter.Write(creationTime.ToBinary());
                infoWriter.Write(lastAccessTime.ToBinary());
                infoWriter.Write(lastWriteTime.ToBinary());
                infoWriter.Write(lastWriteTime.ToBinary());
                infoWriter.Write(attributes);
                infoWriter.Close();

                handle.Sp.SetFileParameters = new IrpParametersSetFile {
                    FileInformationClass = FileInformationClass.FileBasicInformation,
                    FileObject = null,
                    Length = 36
                };

                handle.Irp.UserBuffer = infoWriter.ToArray();

                FatxFsdSetInformation(handle);
            }
        }

        private void CbFsSetEndOfFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, long endOfFile)
        {
            if (fileInfo.UserContext.Equals(IntPtr.Zero))
                return;

            lock (_threadLocker)
            {
                var handle = (IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target;

                handle.Irp.UserBuffer = new byte[0x08];
                handle.Irp.UserBuffer.WriteInt64(0x00, endOfFile);

                handle.Sp.SetFileParameters = new IrpParametersSetFile
                {
                    FileInformationClass = FileInformationClass.FileEndOfFileInformation,
                    Length = 0x08
                };

                this.FatxFsdSetInformation(handle);
            }
        }

        private void CbFsSetAllocationSizeEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, long allocationSize)
        {
            if (fileInfo.UserContext.Equals(IntPtr.Zero))
                return;

            lock (_threadLocker)
            {
                var handle = (IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target;

                handle.Irp.UserBuffer = new byte[0x08];
                handle.Irp.UserBuffer.WriteInt64(0x00, allocationSize);

                handle.Sp.SetFileParameters = new IrpParametersSetFile
                {
                    FileInformationClass = FileInformationClass.FileAllocationInformation,
                    Length = 0x08
                };

                FatxFsdSetInformation(handle);
            }
        }

        private void CbFsRenameOrMoveEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, string newFileName)
        {
            lock (_threadLocker)
            {
                var handle = new IoFsdHandle();
                var directoryHandle = new IoFsdHandle();

                handle.Sp.CreateParametersParameters = new IrpCreateParameters
                {
                    DesiredAccess = 0x110000,
                    Options = 0x1004020,
                    FileAttributes = 0x00,
                    ShareAccess = 0x07,
                    RemainingName = fileInfo.FileName
                };

                var fsdStatus = FatxFsdCreate(handle);

                if (fsdStatus != NT.STATUS_SUCCESS)
                    return;

                directoryHandle.Sp.CreateParametersParameters = new IrpCreateParameters
                {
                    DesiredAccess = 0x100002,
                    Options = 0x1004000,
                    FileAttributes = 0x00,
                    ShareAccess = 0x03,
                    RemainingName = newFileName
                };

                directoryHandle.Sp.Flags = 0x04;

                FatxFsdCreate(directoryHandle);

                var infoWriter = new EndianIO(new MemoryStream(), EndianType.Big);

                infoWriter.Write(0x01);                             // Set ReplaceIfExists to true
                infoWriter.Write(-3);                               // Set RootDirectory to null
                infoWriter.Write((ushort)newFileName.Length);
                infoWriter.Write((ushort)(newFileName.Length + 1)); // Filename max length
                infoWriter.WriteAsciiString(newFileName);

                infoWriter.Close();

                handle.Irp.UserBuffer = infoWriter.ToArray();

                handle.Sp.SetFileParameters = new IrpParametersSetFile
                {
                    FileInformationClass = FileInformationClass.FileRenameInformation,
                    FileObject = directoryHandle.Sp.FileObject,
                    Length = (uint)handle.Irp.UserBuffer.Length
                };

                this.FatxFsdSetInformation(handle);
            }
        }

        private void CbFsReadFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, long position, byte[] buffer, int bytesToRead, ref int bytesRead)
        {
            if (fileInfo.UserContext.Equals(IntPtr.Zero))
                return;

            lock (_threadLocker)
            {
                var ioHandle = (IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target;

                var fcb = (FatxFcb)ioHandle.Sp.FileObject.FsContext;

                bytesRead = 0x00;

                if (bytesToRead <= 0x00 || fcb.IsDirectory)
                    return;

                var io = new EndianIO(buffer, EndianType.Big);

                if (FscTestForFullyCachedIo((uint)position, (uint)bytesToRead))
                    FatxFullyCachedSynchronousIo(fcb, FatxIO.Read, (uint)position, io, (uint)bytesToRead, false);
                else
                    FatxPartiallyCachedSynchronousIo(fcb, FatxIO.Read, (uint)position, io, (uint)bytesToRead);

                io.Close();

                bytesRead = bytesToRead;
            }
        }

        private void CbFsWriteFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, long position, byte[] buffer, int bytesToWrite, ref int bytesWritten)
        {
            if (fileInfo.UserContext.Equals(IntPtr.Zero) || bytesToWrite <= 0x00)
                return;

            lock (_threadLocker)
            {
                var extendedLength = (uint)(_clusterSize + position + bytesToWrite - 1 & _clusterSizeM1N);

                if (position > extendedLength)
                    throw new FatxException("Invalid stream position. [0xC000007F]");

                var fcb = (FatxFcb)((IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target).Sp.FileObject.FsContext;

                if (fcb.EndOfFile == uint.MaxValue)
                {
                    uint returnedCluster, continuousClusterCount;

                    var dwReturn = FatxFileByteOffsetToCluster(fcb, uint.MaxValue, out returnedCluster, out continuousClusterCount);

                    if (dwReturn != NT.STATUS_SUCCESS && dwReturn != NT.STATUS_END_OF_FILE)
                        throw new FatxException(string.Format("FatxFileByteOffsetToPhysicalByteOffset failed with [0x{0:X8}].", dwReturn));
                }

                if (fcb.FileSize > fcb.EndOfFile)
                    throw new FatxException(string.Format("The file size for file {0} was greater than the allocated file size [0xC0000102]", fcb.FileName));

                if (fcb.FileSize <= fcb.EndOfFile && position + bytesToWrite > fcb.EndOfFile)
                    FatxExtendFileAllocation(fcb, extendedLength);

                var writePosition = (uint)(position & uint.MaxValue);

                var io = new EndianIO(buffer, EndianType.Big);

                if (FscTestForFullyCachedIo(writePosition, (uint)bytesToWrite))
                    FatxFullyCachedSynchronousIo(fcb, FatxIO.Write, writePosition, io, (uint)bytesToWrite, false);
                else
                    FatxPartiallyCachedSynchronousIo(fcb, FatxIO.Write, writePosition, io, (uint)bytesToWrite);

                io.Close();

                bytesWritten = bytesToWrite;
            }
        }

        private void CbFsOpenFileEvent(CallbackFileSystem sender, string fileName, ACCESS_MASK desiredAccess, uint fileAttributes, uint shareMode, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileInfo.UserContext != IntPtr.Zero)
                return;

            lock (_threadLocker)
            {
                var handle = new IoFsdHandle {
                    Sp = {
                        CreateParametersParameters = new IrpCreateParameters {
                            DesiredAccess = desiredAccess,
                            Options = 0x5000060,
                            FileAttributes = 0x80,
                            ShareAccess = 0x01,
                            RemainingName = fileName
                        }
                    }
                };

                if (FatxFsdCreate(handle) != NT.STATUS_SUCCESS)
                    return;

                fileInfo.UserContext = GCHandle.ToIntPtr(GCHandle.Alloc(handle));
            }
        }

        private void CbFsGetVolumeSizeEvent(CallbackFileSystem sender, ref long totalAllocationUnits, ref long availableAllocationUnits)
        {
            totalAllocationUnits = ((long)_clusterCount * _clusterSize) / 0x200;
            availableAllocationUnits = ((long)_freedClusterCount * _clusterSize) / 0x200;
        }

        private void CbFsGetVolumeLabelEvent(CallbackFileSystem sender, ref string volumeLabel)
        {
            var handle = new IoFsdHandle {
                Sp = {
                    CreateParametersParameters = new IrpCreateParameters {
                        DesiredAccess = 0x00,
                        Options = 0x1000060,
                        FileAttributes = 0x80,
                        ShareAccess = 0x01,
                        RemainingName = "\\name.txt"
                    }
                }
            };

            if (FatxFsdCreate(handle) != NT.STATUS_SUCCESS)
                return;

            var nameIo = new EndianIO(new MemoryStream(), EndianType.Big);

            var nameFcb = (FatxFcb)handle.Sp.FileObject.FsContext;

            if (FscTestForFullyCachedIo(0x00, nameFcb.FileSize))
                FatxFullyCachedSynchronousIo(nameFcb, FatxIO.Read, 0x00, nameIo, nameFcb.FileSize, false);
            else
                FatxPartiallyCachedSynchronousIo(nameFcb, FatxIO.Read, 0x00, nameIo, nameFcb.FileSize);

            this.FatxFsdClose(handle);

            nameIo.Close();

            var nameArr = nameIo.ToArray();

            if (nameArr[0] == 0xfe && nameArr[1] == 0xff)
                volumeLabel = Encoding.BigEndianUnicode.GetString(nameArr);
        }

        private void CbFsSetVolumeLabelEvent(CallbackFileSystem sender, string volumeLabel)
        {
            if (string.IsNullOrWhiteSpace(volumeLabel))
                return;

            var tempBuffer = Encoding.BigEndianUnicode.GetBytes(volumeLabel);

            var buffer = new byte[tempBuffer.Length + 2];
            buffer.Write(Encoding.BigEndianUnicode.GetPreamble());
            buffer.Write(tempBuffer, 2);

            var handle = new IoFsdHandle {
                Sp = {
                    CreateParametersParameters = new IrpCreateParameters {
                        DesiredAccess = 0x00,
                        Options = 0x5000060,
                        FileAttributes = 0x80,
                        ShareAccess = 0x01,
                        RemainingName = "\\name.txt"
                    }
                }
            };

            if (FatxFsdCreate(handle) != NT.STATUS_SUCCESS)
                return;

            var fcb = (FatxFcb)handle.Sp.FileObject.FsContext;

            if (fcb.EndOfFile == uint.MaxValue)
            {
                uint returnedCluster, continuousClusterCount;

                var dwReturn = FatxFileByteOffsetToCluster(fcb, uint.MaxValue, out returnedCluster, out continuousClusterCount);

                if (dwReturn != NT.STATUS_SUCCESS && dwReturn != NT.STATUS_END_OF_FILE)
                    throw new FatxException(string.Format("FatxFileByteOffsetToPhysicalByteOffset failed with [0x{0:X8}].", dwReturn));
            }

            if (fcb.FileSize > fcb.EndOfFile)
                throw new FatxException(string.Format("The file size for file {0} is greater than the allocated file size [0xC0000102]", fcb.FileName));

            var bytesToWrite = (uint)buffer.Length;

            if (fcb.FileSize <= fcb.EndOfFile && bytesToWrite > fcb.EndOfFile)
                FatxExtendFileAllocation(fcb, _clusterSize + bytesToWrite - 1 & _clusterSizeM1N);

            var io = new EndianIO(buffer, EndianType.Big);

            if (FscTestForFullyCachedIo(0, bytesToWrite))
                FatxFullyCachedSynchronousIo(fcb, FatxIO.Write, 0, io, bytesToWrite, false);
            else
                FatxPartiallyCachedSynchronousIo(fcb, FatxIO.Write, 0, io, bytesToWrite);

            FatxFsdCleanup(handle);
            FatxFsdClose(handle);
        }

        private uint _volumeId;

        private void CbFsGetVolumeIDEvent(CallbackFileSystem sender, ref uint volumeId)
        {
            if (_volumeId == 0)
                _volumeId = (uint)new Random().Next(1, int.MaxValue);

            volumeId = _volumeId;
        }

        private void CbFsIsDirectoryEmptyEvent(CallbackFileSystem sender, CbFsFileInfo directoryInfo, string fileName, ref bool isEmpty)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            lock (_threadLocker)
            {
                var handle = new IoFsdHandle {
                    Sp = {
                        CreateParametersParameters = new IrpCreateParameters {
                            DesiredAccess = 0x100001,
                            Options = 0x4021,
                            FileAttributes = 0x40,
                            ShareAccess = 0x03,
                            RemainingName = fileName
                        }
                    }
                };

                FatxFsdCreate(handle);

                isEmpty = FatxIsDirectoryEmpty((FatxFcb)handle.Sp.FileObject.FsContext) == NT.STATUS_SUCCESS;

                FatxFsdClose(handle);
            }
        }

        private void CbFsGetFileInfoEvent(CallbackFileSystem sender, string fileName, ref bool fileExists,
            ref DateTime creationTime, ref DateTime lastAccessTime, ref DateTime lastWriteTime,
            ref long endOfFile, ref long allocationSize, ref CBFS_LARGE_INTEGER fileId,
            ref uint fileAttributes, ref string shortFileName, ref string realFileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            lock (_threadLocker)
            {
                var handle = new IoFsdHandle();

                var searchSeperator = fileName.LastIndexOf('\\') + 1;

                var name = fileName.Substring(0x00, searchSeperator);

                handle.Sp.CreateParametersParameters = new IrpCreateParameters
                {
                    DesiredAccess = 0x100001,
                    Options = 0x1004021,
                    FileAttributes = 0x40,
                    ShareAccess = 0x03,
                    RemainingName = name
                };

                if (FatxFsdCreate(handle) != NT.STATUS_SUCCESS)
                    return;

                IoFsdDirectoryInformation directoryInformation;

                var search = fileName.Substring(searchSeperator);

                fileExists = FatxFsdDirectoryControl(handle, search, out directoryInformation) == NT.STATUS_SUCCESS;

                if (!fileExists)
                    return;

                creationTime = directoryInformation.CreationTime;
                lastAccessTime = directoryInformation.LastAccessTime;
                lastWriteTime = directoryInformation.LastWriteTime;
                endOfFile = directoryInformation.EndOfFile;
                allocationSize = directoryInformation.AllocationSize;
                fileAttributes = directoryInformation.FileAttributes;
                fileId.QuadPart = 0x00;
            }
        }

        private void CbFsEnumerateDirectoryEvent(CallbackFileSystem sender, CbFsFileInfo directoryInfo,
            CbFsHandleInfo handleInfo, CbFsDirectoryEnumerationInfo enumerationInfo, string mask, int index, bool restart, ref bool fileFound,
            ref string fileName, ref string shortFileName,
            ref DateTime creationTime, ref DateTime lastAccessTime,
            ref DateTime lastWriteTime, ref long endOfFile, ref long allocationSize,
            ref CBFS_LARGE_INTEGER fileId, ref uint fileAttributes)
        {
            lock (_threadLocker)
            {
                FatxDirectoryEnumerationContext context;

                if (restart && enumerationInfo.UserContext != IntPtr.Zero)
                {
                    var userContext = GCHandle.FromIntPtr(enumerationInfo.UserContext);

                    if (userContext.IsAllocated)
                        userContext.Free();

                    enumerationInfo.UserContext = IntPtr.Zero;
                }

                if (enumerationInfo.UserContext != IntPtr.Zero)
                    context = (FatxDirectoryEnumerationContext)GCHandle.FromIntPtr(enumerationInfo.UserContext).Target;
                else
                {
                    context = new FatxDirectoryEnumerationContext(this, directoryInfo.FileName, mask);

                    enumerationInfo.UserContext = GCHandle.ToIntPtr(GCHandle.Alloc(context));
                }

                IoFsdDirectoryInformation fileData;

                fileFound = context.FindNextFile(out fileData);

                if (!fileFound)
                {
                    var userContext = GCHandle.FromIntPtr(enumerationInfo.UserContext);

                    if (userContext.IsAllocated)
                        userContext.Free();

                    enumerationInfo.UserContext = IntPtr.Zero;
                }

                fileName = fileData.FileName;
                creationTime = fileData.CreationTime;
                lastAccessTime = fileData.LastAccessTime;
                lastWriteTime = fileData.LastWriteTime;
                fileAttributes = fileData.FileAttributes;
                endOfFile = fileData.EndOfFile;
                allocationSize = fileData.AllocationSize;
                fileId.QuadPart = 0x00;
            }
        }

        private void CbFsDeleteFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo)
        {
            lock (_threadLocker)
            {
                var handle = new IoFsdHandle();

                handle.Sp.CreateParametersParameters = new IrpCreateParameters
                {
                    DesiredAccess = 0x10000,
                    Options = 0x1004040,
                    FileAttributes = 0x00,
                    ShareAccess = 0x07,
                    RemainingName = fileInfo.FileName
                };

                var status = FatxFsdCreate(handle);

                if (status == 0x00)
                {
                    handle.Irp.UserBuffer = new byte[] { 0x01 };

                    handle.Sp.SetFileParameters = new IrpParametersSetFile
                    {
                        FileInformationClass = FileInformationClass.FileDispositionInformation,
                        FileObject = handle.Sp.FileObject,
                        Length = 1
                    };

                    FatxFsdSetInformation(handle);

                    FatxFsdCleanup(handle);
                    FatxFsdClose(handle);
                }

                handle.Sp.FileObject = null;
                handle.Sp = null;
            }
        }

        private void CbFsCreateFileEvent(CallbackFileSystem sender, string fileName, ACCESS_MASK desiredAccess, uint fileAttributes, uint shareMode, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo)
        {
            lock (_threadLocker)
            {
                var handle = new IoFsdHandle {
                    Sp = {
                        CreateParametersParameters = (fileAttributes & (uint)CbFsFileAttributes.CBFS_FILE_ATTRIBUTE_DIRECTORY) == 0x00
                            ? new IrpCreateParameters {
                                DesiredAccess = desiredAccess,
                                Options = 0x5000060,
                                FileAttributes = 0x80,
                                ShareAccess = 0x00,
                                RemainingName = fileName
                            }
                            : new IrpCreateParameters {
                                DesiredAccess = 0x100001,
                                Options = 0x2004021,
                                FileAttributes = 0x10,
                                ShareAccess = 0x03,
                                RemainingName = fileName
                            }
                    }
                };

                if (FatxFsdCreate(handle) == 0x00)
                    fileInfo.UserContext = GCHandle.ToIntPtr(GCHandle.Alloc(handle));
                else
                    handle.Sp = null;
            }
        }

        private void CbFsCloseFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo)
        {
            if (handleInfo.UserContext == IntPtr.Zero)
                return;

            lock (_threadLocker)
            {
                var ioHandle = (IoFsdHandle)GCHandle.FromIntPtr(handleInfo.UserContext).Target;

                FatxFsdCleanup(ioHandle);
                FatxFsdClose(ioHandle);

                GCHandle.FromIntPtr(handleInfo.UserContext).Free();

                fileInfo.UserContext = IntPtr.Zero;
            }
        }

        private static void CbFsCanFileBeDeletedEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo, ref bool canBeDeleted)
        {
            canBeDeleted = true;
        }

        private void OnCloseDirectoryEnumeration(CallbackFileSystem sender, CbFsFileInfo directoryInfo, CbFsDirectoryEnumerationInfo enumerationInfo)
        {
            if (enumerationInfo.UserContext.Equals(IntPtr.Zero))
                return;

            lock (_threadLocker)
            {
                if (!GCHandle.FromIntPtr(enumerationInfo.UserContext).IsAllocated)
                    return;

                ((FatxDirectoryEnumerationContext)GCHandle.FromIntPtr(enumerationInfo.UserContext).Target).CloseEnumeration();

                GCHandle.FromIntPtr(enumerationInfo.UserContext).Free();
            }
        }

        private void RegisterCBFSEventHandlers()
        {
            this._drive.OnCanFileBeDeleted += CbFsCanFileBeDeletedEvent;
            this._drive.OnCloseDirectoryEnumeration += OnCloseDirectoryEnumeration;
            this._drive.OnCloseFile += CbFsCloseFileEvent;
            this._drive.OnCreateFile += CbFsCreateFileEvent;
            this._drive.OnDeleteFile += CbFsDeleteFileEvent;
            this._drive.OnEnumerateDirectory += CbFsEnumerateDirectoryEvent;
            this._drive.OnFlushFile += CbFsFlushFileEvent;
            this._drive.OnGetFileInfo += CbFsGetFileInfoEvent;
            this._drive.OnGetFileNameByFileId += CbFsGetFileNameByFileIdEvent;
            this._drive.OnGetVolumeId += CbFsGetVolumeIDEvent;
            this._drive.OnGetVolumeLabel += CbFsGetVolumeLabelEvent;
            this._drive.OnGetVolumeSize += CbFsGetVolumeSizeEvent;
            this._drive.OnIsDirectoryEmpty += CbFsIsDirectoryEmptyEvent;
            this._drive.OnMount += CbFsMountEvent;
            this._drive.OnOpenFile += CbFsOpenFileEvent;
            this._drive.OnReadFile += CbFsReadFileEvent;
            this._drive.OnRenameOrMoveFile += CbFsRenameOrMoveEvent;
            this._drive.OnSetAllocationSize += CbFsSetAllocationSizeEvent;
            this._drive.OnSetEndOfFile += CbFsSetEndOfFileEvent;
            this._drive.OnSetFileAttributes += CbFsSetFileAttributesEvent;
            this._drive.OnSetVolumeLabel += CbFsSetVolumeLabelEvent;
            this._drive.OnStorageEjected += CbFsStorageEjectedEvent;
            this._drive.OnUnmount += CbFsUnmountEvent;
            this._drive.OnWriteFile += CbFsWriteFileEvent;
        }

        static FatxDevice()
        {
            CallbackFileSystem.SetRegistrationKey("633C971A14057D2F0419662BA81DCAEF2C37AD58F663ECC6A36015022381FC384367A4FDCFDA1AD37FACECA4D70095CB2688D3304D1909BBC85CDF4592016B862F63521E212EB3F039864BC809F61F7CE996FF5C8D9A1B184D5ADBD8C9B6DF3C9D6AAB288D9A1B180D3ADF3C290B");
        }

        internal NTSTATUS FatxFsdCreate(IoFsdHandle handle)
        {
            NTSTATUS status = 0x00;

            var irpSp = handle.Sp;

            var relatedFile = irpSp.FileObject.RelatedFileObject;

            FatxFcb fatxDcb = _rootDcb, dcb;

            var fileName = irpSp.CreateParametersParameters.RemainingName;

            if (relatedFile == null)
            {
                if (fileName[0x00] != '\\')
                    goto InvalidObjectName;

                fatxDcb.ReferenceCount++;

                if (fileName.Length == 0x01)
                    goto SetShareAccess;

                if (fileName.Length > 0xfa)
                    goto InvalidObjectName;
            }
            else
            {
                if ((relatedFile.Flags & 0x80) != 0x00)
                    goto VolumeIsDismounted;

                dcb = relatedFile.FsContext as FatxFcb;

                if (dcb == null || !dcb.IsDirectory)
                    goto InvalidParameter;

                if (dcb.DeleteOnClose)
                    goto DeletePending;

                dcb.ReferenceCount++;

                fatxDcb = dcb;

                if (fileName[0x00] == '\\' || fatxDcb.Flags + fileName.Length + 1 > 0xfa)
                    goto InvalidObjectName;
            }

            var trailingBackslash = fileName[fileName.Length - 1] == '\\';

            if (trailingBackslash)
                fileName = fileName.Remove(fileName.Length - 1);

            var directoryFile = Bitwise.IsFlagOn(irpSp.CreateParametersParameters.Options, IRP.FILE_DIRECTORY_FILE);

            uint fileExists;

            do
            {
                string firstName;

                Win32.ObDissectName(fileName, out firstName, out fileName);

                if (fileName.Length != 0x00 && fileName[0x00] == '\\')
                    goto InvalidObjectName;

                do
                {
                    if (!FatxIsValidFatFileName(firstName))
                        goto InvalidObjectName;

                    FatxDirectoryEntry directoryEntry;

                    uint directoryByteOffset, emptyDirectoryByteOffset;

                    status = FatxLookupElementNameInDirectory(fatxDcb, firstName, 
                        out directoryEntry, out directoryByteOffset, out emptyDirectoryByteOffset);

                    if (status == NT.STATUS_SUCCESS)
                    {
                        if (fileName.Length != 0x00)
                        {
                            if (!fatxDcb.IsDirectory)
                                goto ObjectPathNotFound;
                        }
                        else if ((irpSp.Flags & IoStackLocation.SL_OPEN_TARGET_DIRECTORY) != 0x00)
                        {
                            fileExists = 0x04;
                            goto OpenTargetDirectory;
                        }

                        dcb = FatxCreateFcb(fatxDcb, directoryEntry, directoryByteOffset);

                        FatxDereferenceFcb(fatxDcb);

                        fatxDcb = dcb;

                        if (fileName.Length == 0x00)
                            goto SetShareAccess;

                        Win32.ObDissectName(fileName, out firstName, out fileName);

                        continue;
                    }

                    if (status != NT.STATUS_OBJECT_NAME_NOT_FOUND)
                        goto Exit;

                    if (fileName.Length != 0x00)
                        goto ObjectPathNotFound;

                    if (fatxDcb.DeleteOnClose)
                        goto DeletePending;

                    if ((irpSp.Flags & IoStackLocation.SL_OPEN_TARGET_DIRECTORY) != 0x00)
                    {
                        fileExists = 0x05;
                        goto OpenTargetDirectory;
                    }

                    uint options = irpSp.CreateParametersParameters.Options, createDisposition = options >> 24;

                    if (createDisposition == IRP.FILE_OPEN || createDisposition == IRP.FILE_OVERWRITE)
                        goto Exit;

                    if (trailingBackslash && Bitwise.IsFlagOn(options, IRP.FILE_NON_DIRECTORY_FILE))
                        goto InvalidObjectName;

                    status = FatxCreateNewFile(fatxDcb, firstName, emptyDirectoryByteOffset, directoryFile 
                        ? FatxDirectoryEntry.Attribute.Directory : FatxDirectoryEntry.Attribute.Normal, 
                        0x00, out directoryEntry, out dcb);

                    if (status != NT.STATUS_SUCCESS)
                        goto Exit;

                    FatxDereferenceFcb(fatxDcb);

                    irpSp.FileObject.FsContext = dcb;

                    if (Bitwise.IsFlagOn(options, 0x1000) && !directoryFile)
                        dcb.State |= 0x10;

                    return NT.STATUS_SUCCESS;

                } while (fileName.Length == 0x00 || fileName[0x00] != '\\');

            } while (fatxDcb.IsDirectory);

        SetShareAccess:
            irpSp.FileObject.FsContext = fatxDcb;
            goto Exit;

        OpenTargetDirectory:
            FatxOpenTargetDirectory(handle.Irp, fatxDcb, fileExists);
            goto Exit;

        ObjectPathNotFound:
            status = NT.STATUS_OBJECT_PATH_NOT_FOUND;
            goto Exit;

        VolumeIsDismounted:
            status = NT.STATUS_VOLUME_DISMOUNTED;
            goto Exit;

        InvalidParameter:
            status = NT.STATUS_INVALID_PARAMETER;
            goto Exit;

        InvalidObjectName:
            status = NT.STATUS_OBJECT_NAME_INVALID;
            goto Exit;

        DeletePending:
            status = NT.STATUS_DELETE_PENDING;

        Exit:

            if (status != NT.STATUS_SUCCESS && fatxDcb != null)
                FatxDereferenceFcb(fatxDcb);

            return status;
        }

        internal NTSTATUS FatxFsdDirectoryControl(IoFsdHandle handle, string searchPattern, out IoFsdDirectoryInformation fsdDirectoryInformation)
        {
            fsdDirectoryInformation = new IoFsdDirectoryInformation();

            var fileObject = handle.Sp.FileObject;

            var fcb = fileObject.FsContext as FatxFcb;

            if (fcb == null || !fcb.IsDirectory)
                return NT.STATUS_INVALID_PARAMETER;

            if ((fileObject.Flags & 0x80) != 0x00)
                return NT.STATUS_VOLUME_DISMOUNTED;

            if ((fileObject.Flags & 0x10) != 0x00)
                return NT.STATUS_FILE_CLOSED;

            if (fcb.DeleteOnClose)
                return NT.STATUS_DELETE_PENDING;

            var enumContext = fileObject.FsContext2;

            if (enumContext == null)
            {
                IoFsd.IoCreateDirectoryEnumContext(searchPattern, false, out enumContext);

                fileObject.FsContext2 = enumContext;
            }

            if (Bitwise.IsFlagOn(handle.Sp.Flags, IoStackLocation.SL_RESTART_SCAN))
                fsdDirectoryInformation.NextEntryOffset = 0x00;

            uint directoryEntryOffset;
            FatxDirectoryEntry directoryEntry;

            var status = FatxFindNextDirectoryEntry(fcb, enumContext.CurrentDirectoryOffset, enumContext.SearchPattern, out directoryEntry, out directoryEntryOffset);

            if (status != 0x00)
                return status;

            if (directoryEntry.IsDirectory)
            {
                fsdDirectoryInformation.AllocationSize = 0x00;
                fsdDirectoryInformation.EndOfFile = 0x00;
            }
            else
            {
                fsdDirectoryInformation.AllocationSize = directoryEntry.FileSize;
                fsdDirectoryInformation.EndOfFile = directoryEntry.FileSize;
            }

            fsdDirectoryInformation.CreationTime = FatxFatTimestampToTime(directoryEntry.CreationTimeStamp);

            var lastAccessTime = FatxFatTimestampToTime(directoryEntry.LastAccessTimeStamp);

            fsdDirectoryInformation.LastAccessTime = lastAccessTime;
            fsdDirectoryInformation.LastWriteTime = lastAccessTime;
            fsdDirectoryInformation.ChangeTime = lastAccessTime;

            fsdDirectoryInformation.FileAttributes = directoryEntry.Characteristics != 0x00 ? directoryEntry.Characteristics : 0x80u;

            fsdDirectoryInformation.FileName = directoryEntry.Filename;

            enumContext.CurrentDirectoryOffset = directoryEntryOffset + 0x40;

            fcb.ByteOffset = directoryEntryOffset;

            return 0x00;
        }

        private void FatxFsdSetInformation(IoFsdHandle handle)
        {
            var irp = handle.Irp;

            var fcb = handle.Sp.FileObject.FsContext as FatxFcb;

            if (fcb == null)
            {
                irp.UserIosb.Status = NT.STATUS_INVALID_PARAMETER;
                return;
            }

            if (fcb.CloneFCB != null)
            {
                irp.UserIosb.Status = NT.STATUS_ACCESS_DENIED;
                return;
            }

            var infoClass = handle.Sp.SetFileParameters.FileInformationClass;

            if (infoClass == FileInformationClass.FileEndOfFileInformation)
            {
                if (fcb.IsDirectory)
                    irp.UserIosb.Status = NT.STATUS_INVALID_PARAMETER;
                else
                {
                    irp.UserIosb.Status = 0x00;
                    FatxSetEndOfFileInformation(fcb, new FileEndOfFileInfo(irp.UserBuffer));
                }

                return;
            }

            if (fcb == _rootDcb)
            {
                irp.UserIosb.Status = NT.STATUS_INVALID_PARAMETER;
                return;
            }

            switch (infoClass)
            {
                case FileInformationClass.FileBasicInformation:
                    FatxSetBasicInformation(fcb, new FileBasicInformation(irp.UserBuffer));
                    break;
                case FileInformationClass.FileRenameInformation:
                    FatxSetRenameInformation(handle, new FileRenameInformation(irp.UserBuffer));
                    break;
                case FileInformationClass.FileDispositionInformation:
                    FatxSetDispositionInformation(fcb, new FileDispositionInformation(irp.UserBuffer));
                    break;
                case FileInformationClass.FileAllocationInformation:
                    var allocationInformation = new FileAllocationInformation(irp.UserBuffer);

                    if (allocationInformation.AllocationSize.HighPart != 0x00)
                    {
                        irp.UserIosb.Status = NT.STATUS_DISK_FULL;
                        return;
                    }

                    FatxSetAllocationSize(fcb, allocationInformation.AllocationSize.LowPart, false, false);
                    break;
            }

            irp.UserIosb.Status = 0x00;
        }

        private void FatxFsdCleanup(IoFsdHandle handle)
        {
            var fcb = (FatxFcb)handle.Sp.FileObject.FsContext;

            if (fcb.ReferenceCount != 0x01 || (handle.Sp.FileObject.Flags & 0x80) != 0x00)
                return;

            if (fcb.DeleteOnClose)
            {
                FatxMarkDirectoryEntryDeleted(fcb);
                FatxFreeClusters(fcb.FirstCluster, false);
                FatxDereferenceFcb(fcb.ParentFCB);
                fcb.ParentFCB = null;
            }
            else if (fcb.IsModified)
                FatxUpdateDirectoryEntry(fcb);

            handle.Sp.FileObject.Flags |= 0x10;
        }

        internal void FatxFsdClose(IoFsdHandle handle)
        {
            var fcb = (FatxFcb)handle.Sp.FileObject.FsContext;

            if (fcb != null)
                FatxDereferenceFcb(fcb);
        }

        private void FatxProcessBootSector(FatxPartitionType partitionType)
        {
            _io.Position = this._deviceOffset + 0x08; // Magic, ID

            var sectorsPerCluster = _io.ReadUInt32();

            _sectorsPerClusterM1 = sectorsPerCluster - 1;

            _bootSectorSize = sectorsPerCluster + 0xfff & 0xfffff000;

            if (_bootSectorSize <= 0x1000)
                _bootSectorSize = 0x1000;

            if ((_bootSectorSize & 0xfff) != 0)
                throw new FatxException(string.Format("Invalid boot sector length detected 0x{0:X8}.", _bootSectorSize));

            if (_bootSectorSize > _deviceSize)
                throw new FatxException("Boot sector size was greater than device size [0xC000014F].");

            _clusterSize = sectorsPerCluster << 0x09;

            _clusterSizeM1 = _clusterSize - 1;

            _clusterSizeM1N = ~_clusterSizeM1;

            if (_clusterSize != 0)
                _clusterShift = (byte)(0x1f - _clusterSize.CountLeadingZerosWord());

            if (partitionType == FatxPartitionType.Growable && _clusterSize != 0x4000)
                throw new FatxException("Invalid bytes per cluster for growable file partition.");

            if (_clusterSize < _bootSectorSize)
                throw new FatxException("Cluster size is smaller than the boot sector.");

            var clusterCount = (_deviceSize >> _clusterShift) + 1;

            if (clusterCount >= 0xfff0 || partitionType == FatxPartitionType.Growable)
            {
                _chainShift = 0x02;
                clusterCount <<= 0x02;
            }
            else
            {
                _chainShift = 0x01;
                clusterCount <<= 0x01;
            }

            clusterCount = clusterCount + _bootSectorSize - 1 & ~(_bootSectorSize - 1) & uint.MaxValue;

            _clusterCount = (uint)(_deviceSize - _bootSectorSize - clusterCount >> _clusterShift);

            _backingClusterOffset = clusterCount + (_bootSectorSize & uint.MaxValue);

            if ((_backingClusterOffset & 0xfff) != 0)
                throw new FatxException(string.Format("Invalid backing cluster offset length detected 0x{0:X16}.", _backingClusterOffset));

            if ((_clusterSize & 0xfff) != 0)
                throw new FatxException(string.Format("Invalid cluster size detected 0x{0:X8}.", _clusterSize));

            this._rootDcb.FirstCluster = _io.ReadUInt32();
        }

        private EndianIO _allocSupport;

        private void FatxInitializeAllocationSupport()
        {
            var realClusterCount = _clusterCount + 1;

            if (realClusterCount > 0x40000)
            {
                var allocationShift = (uint)(1 << 0x1f - ((realClusterCount << 1) - 1).CountLeadingZerosWord() >> 11);

                if (allocationShift < 0x400)
                    allocationShift = 0x400;

                _allocationShift = (byte)(0x1f - allocationShift.CountLeadingZerosWord());

                this._allocSupport = new EndianIO(new byte[0x100], EndianType.Little);
            }

            uint
                chainMapOffset = 0,
                lastFreeClusterIndex = uint.MaxValue,
                freedClusterCount = 0x00,
                remainderOfChainMap = realClusterCount << _chainShift,
                mappedBufferSize = 0x1000;

            do
            {
                if ((chainMapOffset & 0xfff) != 0x00)
                    throw new FatxException("Detected an invalid cluster offset.");

                if (mappedBufferSize > remainderOfChainMap)
                    mappedBufferSize = remainderOfChainMap;

                uint
                    chainMapBlockSize = mappedBufferSize,
                    staticRemainderChainMapBlockSize = 0x1000,
                    currentChainMapLocation = 0x00;

                var reader = new EndianIO(this.FscMapBuffer(_bootSectorSize + chainMapOffset, mappedBufferSize), EndianType.Big);

                do
                {
                    if (chainMapBlockSize < 0x1000)
                        staticRemainderChainMapBlockSize = chainMapBlockSize;

                    uint
                        realRemainderCmbs = staticRemainderChainMapBlockSize,
                        readClusterCount = freedClusterCount;

                    if (_chainShift == 1)
                    {
                        if (chainMapOffset == 0)
                            currentChainMapLocation = 2;

                        if (currentChainMapLocation < realRemainderCmbs)
                        {
                            reader.Position = currentChainMapLocation;

                            do
                            {
                                uint clusterNum = reader.ReadUInt16();

                                if (clusterNum != 0)
                                    continue;

                                if (lastFreeClusterIndex == uint.MaxValue)
                                    lastFreeClusterIndex = (chainMapOffset >> 1) + (currentChainMapLocation >> 1);

                                freedClusterCount++;

                            } while ((currentChainMapLocation += 2) < realRemainderCmbs);
                        }
                    }
                    else
                    {
                        currentChainMapLocation = chainMapOffset == 0 ? 4u : 0;

                        if (currentChainMapLocation < realRemainderCmbs)
                        {
                            reader.Position = currentChainMapLocation;

                            do
                            {
                                var clusterNum = reader.ReadUInt32();

                                if (clusterNum != 0)
                                    continue;

                                if (lastFreeClusterIndex == uint.MaxValue)
                                    lastFreeClusterIndex = (chainMapOffset >> 2) + (currentChainMapLocation >> 2);

                                freedClusterCount++;

                            } while ((currentChainMapLocation += 4) < realRemainderCmbs);
                        }
                    }

                    if (readClusterCount != freedClusterCount)
                        FatxFreeClusterRange(chainMapOffset >> _chainShift >> _allocationShift);

                    chainMapOffset += staticRemainderChainMapBlockSize;

                } while ((chainMapBlockSize -= staticRemainderChainMapBlockSize) != 0);

            } while ((remainderOfChainMap -= mappedBufferSize) != 0);

            _freedClusterCount = freedClusterCount;
            _lastFreeClusterIndex = lastFreeClusterIndex;
        }

        private static FatxFcb FatxCreateFcb(FatxFcb parentFcb, FatxDirectoryEntry directoryEntry, uint directoryByteOffset)
        {
            parentFcb.ReferenceCount++;

            var fcb = new FatxFcb(directoryEntry, parentFcb, directoryByteOffset);

            FatxResetClusterCache(fcb);

            return fcb;
        }

        private static void FatxDereferenceFcb(FatxFcb fcb)
        {
            if (fcb.ReferenceCount <= 0)
                throw new FatxException("Attempted to de-reference an FCB with zero references.");

            do
            {
                fcb.ReferenceCount--;

                if (fcb.ReferenceCount != 0)
                    break;

                fcb.CloneFCB = null;

                fcb = fcb.ParentFCB;

            } while (fcb != null);
        }

        private static NTSTATUS FatxOpenTargetDirectory(IRP irp, FatxFcb directoryFcb, uint fileExists)
        {
            if (directoryFcb == null)
                return NT.STATUS_FILE_CORRUPT_ERROR;

            var irpSp = irp.Sp;

            var fileObject = irpSp.FileObject;

            if (!directoryFcb.IsDirectory)
                throw new FatxException("Attempted to open a file as a target directory.");

            if (directoryFcb.ShareAccess.OpenCount == 0x00)
                IoFsd.IoSetShareAccess(irpSp.CreateParametersParameters.DesiredAccess, irpSp.CreateParametersParameters.ShareAccess, fileObject, ref directoryFcb.ShareAccess);
            else
                IoFsd.IoCheckShareAccess(irpSp.CreateParametersParameters.DesiredAccess, irpSp.CreateParametersParameters.ShareAccess, fileObject, ref directoryFcb.ShareAccess, true);

            directoryFcb.ReferenceCount++;

            fileObject.FsContext = directoryFcb;
            fileObject.FsContext2 = null;

            irp.UserIosb.Information = fileExists;

            return NT.STATUS_SUCCESS;
        }

        private NTSTATUS FatxSetRenameInformation(IoFsdHandle handle, FileRenameInformation renameInformation)
        {
            if (renameInformation.FileName.Length == 0x00)
                return NT.STATUS_INVALID_PARAMETER;

            var fcb = handle.Sp.FileObject.FsContext as FatxFcb;

            if (fcb == null || fcb.ParentFCB == null)
                return NT.STATUS_FILE_CORRUPT_ERROR;

            var name = renameInformation.FileName.Substring(renameInformation.FileName.LastIndexOf('\\') + 1);

            if (!FatxIsValidFatFileName(name))
                return NT.STATUS_OBJECT_NAME_INVALID;

            var dcb = handle.Sp.SetFileParameters.FileObject == null ? fcb.ParentFCB : (FatxFcb) handle.Sp.SetFileParameters.FileObject.FsContext;

            if (!dcb.IsDirectory)
                throw new FatxException("Target location for moving or renaming is not a directory.");

            var flags = dcb.Flags + name.Length + 1;

            if (flags > 0xfa)
                return NT.STATUS_OBJECT_NAME_INVALID;

            FatxDirectoryEntry directoryEntry;

            uint directoryByteOffset, emptyDirectoryByteOffset;

            var status = FatxLookupElementNameInDirectory(dcb, name, out directoryEntry, out directoryByteOffset, out emptyDirectoryByteOffset);

            if (status == NT.STATUS_SUCCESS)
            {
                if (!renameInformation.ReplaceIfExists || directoryEntry.IsDirectory)
                    return NT.STATUS_OBJECT_NAME_COLLISION;

                status = FatxDeleteFile(dcb, directoryByteOffset);

                if (status != NT.STATUS_SUCCESS)
                    return status;
            }
            else if (status != NT.STATUS_OBJECT_NAME_NOT_FOUND)
                return status;

            var oldFileName = fcb.FileName;

            if (fcb.ParentFCB == dcb)
            {
                fcb.FileName = name;

                status = FatxUpdateDirectoryEntry(fcb);

                if (status != NT.STATUS_SUCCESS)
                    fcb.FileName = oldFileName;

                return status;
            }

            if (emptyDirectoryByteOffset == uint.MaxValue)
            {
                if (dcb.EndOfFile == uint.MaxValue)
                    throw new FatxException("attempted to extend an invalid directory");

                FatxExtendDirectoryAllocation(dcb);

                emptyDirectoryByteOffset = dcb.EndOfFile;
            }

            status = FatxMarkDirectoryEntryDeleted(fcb);

            if (status != NT.STATUS_SUCCESS)
                return status;

            FatxDereferenceFcb(fcb.ParentFCB);

            fcb.ParentFCB = dcb;

            dcb.ReferenceCount++;

            fcb.DirectoryEntryByteOffset = emptyDirectoryByteOffset;

            fcb.FileName = name;

            status = FatxUpdateDirectoryEntry(fcb);

            if (status == NT.STATUS_SUCCESS)
                fcb.Flags = (byte)(flags & 0xff);
            else
            {
                fcb.FileName = oldFileName;

                FatxDereferenceFcb(fcb);

                fcb.ParentFCB = null;
            }

            return status;
        }

        private NTSTATUS FatxCreateNewFile(FatxFcb directoryFcb, string elementName, uint emptyDirectoryOffset, FatxDirectoryEntry.Attribute fileAttributes, uint allocationSize, out FatxDirectoryEntry fatxDirectoryEntry, out FatxFcb returnedFcb)
        {
            if (!directoryFcb.IsDirectory)
                throw new FatxException("Parent FCB for a new file is not a directory.");

            if (emptyDirectoryOffset == uint.MaxValue)
            {
                if (directoryFcb.EndOfFile == uint.MaxValue)
                    throw new FatxException("Invalid end-of-file found.");

                emptyDirectoryOffset = directoryFcb.EndOfFile;

                this.FatxExtendDirectoryAllocation(directoryFcb);
            }

            uint lastAllocated, totalAllocated, firstAllocated = 0x00, endOfFile = 0x00;

            FatxAllocationState allocationState = null;

            if (fileAttributes == FatxDirectoryEntry.Attribute.Directory)
            {
                this.FatxAllocateClusters(0x00, 0x01, out allocationState, out totalAllocated, out lastAllocated);

                if (totalAllocated != 0x01)
                    throw new FatxException("The cluster allocation function returned an invalid allocated cluster count.");

                firstAllocated = allocationState.AllocationStates[0].FirstAllocatedCluster;

                endOfFile = _clusterSize;

                this.FatxInitializeDirectoryCluster(firstAllocated);
            }
            else if (allocationSize != 0x00)
            {
                endOfFile = _clusterSize + allocationSize - 1 & _clusterSizeM1N;

                this.FatxAllocateClusters(0x00, endOfFile >> _clusterShift, out allocationState, out totalAllocated, out lastAllocated);

                firstAllocated = allocationState.AllocationStates[0].FirstAllocatedCluster;
            }
            else
            {
                lastAllocated = 0x00;
                totalAllocated = 0x00;
            }

            var directoryEntry = new FatxDirectoryEntry();

            directoryEntry.FileNameLength = (byte)elementName.Length;
            directoryEntry.Filename = elementName;
            directoryEntry.Characteristics = (byte)fileAttributes;
            directoryEntry.CreationTimeStamp = FatxTimeToFatTimestamp(DateTime.Now);
            directoryEntry.LastAccessTimeStamp = directoryEntry.CreationTimeStamp;

            directoryEntry.FirstClusterNumber = firstAllocated;

            var fcb = FatxCreateFcb(directoryFcb, directoryEntry, emptyDirectoryOffset);

            this.FatxUpdateDirectoryEntry(fcb);

            if (endOfFile != 0x00)
            {
                if (fcb.EndOfFile != 0xffffffff)
                    throw new FatxException(string.Format("Invalid end-of-file for file size {0:X8}.", allocationSize));

                fcb.EndOfFile = endOfFile;
                fcb.LastCluster = lastAllocated;

                FatxAppendClusterRunsToClusterCache(fcb, 0x00, allocationState, totalAllocated);
            }
            else if (fcb.EndOfFile != 0x00)
                throw new FatxException("Invalid end-of-file for file size 0x00000000.");
            else if (fcb.LastCluster != 0x00)
                throw new FatxException("Invalid cluster for the end-of-file found.");

            returnedFcb = fcb;

            fatxDirectoryEntry = directoryEntry;

            return 0x00;
        }

        private void FatxSetAllocationSize(FatxFcb fcb, uint fileSize, bool deleteFile, bool disableTruncation)
        {
            var roundedSize = _clusterSize + fileSize - 1 & _clusterSizeM1N;

            if (roundedSize == 0x00)
            {
                this.FatxDeleteFileAllocation(fcb);
                return;
            }

            if (fcb.EndOfFile == uint.MaxValue)
            {
                if (fcb.LastCluster != 0x00)
                    throw new FatxException(string.Format("Invalid last cluster detected while setting a new allocation size for {0}.", fcb.FileName));

                uint cluster, continuousClusterCount;

                if (this.FatxFileByteOffsetToCluster(fcb, uint.MaxValue, out cluster, out continuousClusterCount) == 0xC0000102 && deleteFile)
                    this.FatxDeleteFileAllocation(fcb);
            }

            if (fcb.EndOfFile < roundedSize)
                this.FatxExtendFileAllocation(fcb, roundedSize);
            else if (!disableTruncation && fcb.EndOfFile > roundedSize)
                this.FatxTruncateFileAllocation(fcb, roundedSize);
        }

        private void FatxExtendFileAllocation(FatxFcb fcb, uint fileSize)
        {
            if (fileSize == 0x00 || (_clusterSizeM1 & fileSize) != 0x0)
                throw new FatxException(string.Format("Invalid extension length detected for {0}.", fcb.FileName));

            if (fcb.EndOfFile >= fileSize)
                throw new FatxException("Found an invalid FCB while extending the file's allocation.");

            if (fcb.IsDirectory)
                throw new FatxException("Attempted to extended a directory instead of a file.");

            if (fcb.EndOfFile != 0x00 && (fcb.FirstCluster == 0x00 || fcb.LastCluster == 0x00))
                throw new FatxException("Found an invalid FCB while extending the file's allocation.");

            uint lastAllocated, totalAllocated;

            FatxAllocationState allocationState;

            this.FatxAllocateClusters(fcb.LastCluster, fileSize - fcb.EndOfFile >> _clusterShift, out allocationState, out totalAllocated, out lastAllocated);

            if (fcb.FirstCluster == 0x00)
            {
                fcb.FirstCluster = allocationState.AllocationStates[0].FirstAllocatedCluster;

                this.FatxUpdateDirectoryEntry(fcb);
            }

            fcb.LastCluster = lastAllocated;

            FatxAppendClusterRunsToClusterCache(fcb, fcb.EndOfFile >> _clusterShift, allocationState, totalAllocated);

            fcb.EndOfFile = fileSize;
        }

        private void FatxTruncateFileAllocation(FatxFcb fcb, uint fileSize)
        {
            if (fileSize == 0x00 || (_clusterSizeM1 & fileSize) != 0x00)
                throw new FatxException("Detected an invalid truncation size.");

            if (fcb.EndOfFile == uint.MaxValue || fcb.EndOfFile <= fileSize || fcb.IsDirectory)
                throw new FatxException("Invalid FCB detected.");

            if (fcb.EndOfFile != 0x00 && (fcb.FirstCluster == 0x00 || fcb.LastCluster == 0x00))
                throw new FatxException("detected an invalid starting or final cluster.");

            uint returnedCluster, continuousClusterCount;

            if (this.FatxFileByteOffsetToCluster(fcb, fileSize - 1, out returnedCluster, out continuousClusterCount) != 0x00)
                return;

            if (fcb.FileSize > fileSize)
            {
                fcb.FileSize = fileSize;

                this.FatxUpdateDirectoryEntry(fcb);
            }

            this.FatxFreeClusters(returnedCluster, true);

            fcb.EndOfFile = fileSize;
            fcb.LastCluster = returnedCluster;

            FatxInvalidateClusterCache(fcb, returnedCluster);
        }

        private void FatxDeleteFileAllocation(FatxFcb fcb)
        {
            if (fcb.IsDirectory)
                throw new FatxException("Attempted to delete allocation for a directory.");

            if (fcb.FirstCluster != 0x00 || fcb.FileSize != 0x00)
            {
                var firstCluster = fcb.FirstCluster;

                fcb.FileSize = 0x00;
                fcb.FirstCluster = 0x00;

                this.FatxUpdateDirectoryEntry(fcb);

                this.FatxFreeClusters(firstCluster, false);

                fcb.EndOfFile = 0x00;
                fcb.LastCluster = 0x00;
            }
            else if (fcb.EndOfFile != 0x00 || fcb.LastCluster != 0x00)
                throw new FatxException("Detected an invalid FCB while deleting a file's allocation.");
        }

        private void FatxExtendDirectoryAllocation(FatxFcb fcb)
        {
            if (!fcb.IsDirectory)
                throw new FatxException("Attempted to extend a file as a directory.");

            if (fcb.EndOfFile == 0x00 || fcb.EndOfFile == uint.MaxValue || fcb.LastCluster == 0x00)
                throw new FatxException("Invalid FCB detected while extending directory allocation.");
            
            if (fcb.EndOfFile >= 0x40000)
                throw new FatxException("Directory cannot be extended. [0xC00002EA]");

            FatxAllocationState allocationState;

            uint totalAllocated, lastAllocatedCluster;

            this.FatxAllocateClusters(0x00, 0x01, out allocationState, out totalAllocated, out lastAllocatedCluster);

            if (totalAllocated != 0x01)
                throw new FatxException(string.Format("Allocated an invalid amount of clusters: {0}.", totalAllocated));

            var allocatedCluster = allocationState.AllocationStates[0].FirstAllocatedCluster;

            if (allocatedCluster != lastAllocatedCluster)
                throw new FatxException("Directory cluster allocation failed.");

            this.FatxInitializeDirectoryCluster(allocatedCluster);

            this.FatxLinkClusterChains(fcb.LastCluster, allocatedCluster);

            fcb.LastCluster = allocatedCluster;

            FatxAppendClusterRunsToClusterCache(fcb, fcb.EndOfFile >> _clusterShift, allocationState, totalAllocated);

            fcb.EndOfFile += _clusterSize;

            if (fcb.CloneFCB == null)
                return;

            fcb.CloneFCB.FirstCluster = fcb.FirstCluster;
            fcb.CloneFCB.LastCluster = fcb.LastCluster;
            fcb.CloneFCB.EndOfFile = fcb.EndOfFile;
        }

        private NTSTATUS FatxSetBasicInformation(FatxFcb fcb, FileBasicInformation basicInformation)
        {
            var updateDirectory = false;

            var creationTime = fcb.CreationTimeStamp;

            if (basicInformation.CreationTime != DateTime.MinValue && DateTime.Compare(FatxFatTimestampToTime(fcb.CreationTimeStamp), basicInformation.CreationTime) != 0x00)
            {
                creationTime = FatxTimeToFatTimestamp(basicInformation.CreationTime);
                updateDirectory = true;
            }

            var lastWriteTime = fcb.LastWriteTimeStamp;

            if (basicInformation.LastWriteTime != DateTime.MinValue && DateTime.Compare(fcb.LastWriteTimeStamp, basicInformation.LastWriteTime) != 0x00)
            {
                lastWriteTime = basicInformation.LastWriteTime;
                updateDirectory = true;
            }

            var lastAccessTime = fcb.LastAccessTimeStamp;

            if (basicInformation.LastAccessTime != DateTime.MinValue && DateTime.Compare(FatxFatTimestampToTime(fcb.LastAccessTimeStamp), basicInformation.LastAccessTime) != 0x00)
            {
                lastAccessTime = FatxTimeToFatTimestamp(basicInformation.LastAccessTime);
                updateDirectory = true;
            }

            var attributes = (byte)(basicInformation.FileAttributes & 0x10);

            if (attributes == 0x00)
                attributes = fcb.FileAttributes;
            else
            {
                if (fcb.IsDirectory)
                    attributes |= 0x10;
                else if (Bitwise.IsFlagOn(attributes, (long)FatxDirectoryEntry.Attribute.Directory))
                    return NT.STATUS_INVALID_PARAMETER;

                if ((basicInformation.FileAttributes & 0xff) != fcb.FileAttributes)
                    updateDirectory = true;
            }

            if (updateDirectory)
            {
                fcb.LastWriteTimeStamp = lastWriteTime;
                fcb.CreationTimeStamp = creationTime;
                fcb.LastAccessTimeStamp = lastAccessTime;
                fcb.FileAttributes = attributes;

                FatxUpdateDirectoryEntry(fcb);
            }

            return NT.STATUS_SUCCESS;
        }

        private NTSTATUS FatxSetDispositionInformation(FatxFcb fcb, FileDispositionInformation dispositionInfo)
        {
            NTSTATUS status = 0x00;

            if (!dispositionInfo.DeleteFile)
                fcb.State &= 0xef;
            else
            {
                if (fcb.IsDirectory)
                {
                    status = FatxIsDirectoryEmpty(fcb);

                    if (status != 0x00 && status != NT.STATUS_FILE_CORRUPT_ERROR)
                        return status;
                }

                fcb.State |= 0x10;
            }

            return status;
        }

        private NTSTATUS FatxSetEndOfFileInformation(FatxFcb fcb, FileEndOfFileInfo endOfFileInfo)
        {
            if (fcb.IsDirectory)
                throw new FatxException("Attempted to set the EOF of a directory.");

            if (endOfFileInfo.EndOfFile.HighPart != 0x00)
                return 0xC000007F;

            var newSize = endOfFileInfo.EndOfFile.LowPart;

            if (newSize == fcb.FileSize)
                return 0x00;

            if (newSize > fcb.FileSize)
                FatxSetAllocationSize(fcb, newSize, false, true);

            fcb.FileSize = newSize;

            return FatxUpdateDirectoryEntry(fcb);
        }

        private NTSTATUS FatxIsDirectoryEmpty(FatxFcb fcb)
        {
            if (!fcb.IsDirectory)
                throw new FatxException("Attempted to determine if a non-directory was empty.");

            FatxDirectoryEntry dirEnt;

            uint directoryOffset;

            var status = FatxFindNextDirectoryEntry(fcb, 0x00, null, out dirEnt, out directoryOffset);

            if (status == NT.STATUS_END_OF_FILE)
                return NT.STATUS_SUCCESS;

            return status != NT.STATUS_SUCCESS ? status : NT.STATUS_DIRECTORY_NOT_EMPTY;
        }

        private NTSTATUS FatxFindNextDirectoryEntry(FatxFcb fcb, uint searchStartOffset, string searchString, out FatxDirectoryEntry directoryEntry, out uint directoryOffset)
        {
            uint dwReturn = 0x00, returnedCluster, continuousClusterCount;

            directoryEntry = null;

            if (searchStartOffset >= _clusterSize)
                dwReturn = this.FatxFileByteOffsetToCluster(fcb, searchStartOffset, out returnedCluster, out continuousClusterCount);
            else
            {
                returnedCluster = fcb.FirstCluster;

                if (returnedCluster - 1 >= _clusterCount)
                    throw new FatxException("Invalid starting cluster for directory.");
            }

            var maxFileNameLength = 0xf9 - fcb.FileAttributes;

            if (maxFileNameLength > 0x2a)
                maxFileNameLength = 0x2a;

            if (maxFileNameLength < -1)
                throw new FatxException("Invalid file name length for directory. [0xC0000011]");

            do
            {
                uint
                    searchRemainder = _clusterSizeM1 & searchStartOffset,
                    remainder = _clusterSize - searchRemainder;

                var physicalOffset = _backingClusterOffset + ((long)(returnedCluster - 1) << _clusterShift) + searchRemainder;

                do
                {
                    var reader = new EndianIO(this.FscMapBuffer(physicalOffset), EndianType.Big);

                    var lowOffset = (uint)(0x1000 - (physicalOffset & 0xfff));

                    if (lowOffset < remainder)
                    {
                        remainder -= lowOffset;
                        physicalOffset += lowOffset;
                    }
                    else
                    {
                        lowOffset = remainder;
                        remainder = 0x00;
                    }

                    uint boundary = lowOffset, entryPosition = 0x00;

                    do
                    {
                        var dirEnt = new FatxDirectoryEntry(reader);

                        uint flags = dirEnt.FileNameLength;

                        if (flags == 0x00 || flags == 0xff)
                        {
                            directoryOffset = 0xffffffff;

                            return NT.STATUS_END_OF_FILE;
                        }

                        if (dirEnt.IsValid && dirEnt.FileNameLength <= maxFileNameLength)
                        {
                            flags = dirEnt.Characteristics & 0xfffffff8 & 0xffffffcf;

                            if (flags == 0x00 && FatxIsValidFatFileName(dirEnt.Filename) && (searchString == null || string.Compare(dirEnt.Filename, searchString, StringComparison.OrdinalIgnoreCase) == 0x00))
                            {
                                directoryEntry = dirEnt;

                                goto CleanupAndExit;
                            }
                        }

                        searchStartOffset += 0x40;
                        entryPosition += 0x40;

                        reader.Position = entryPosition;

                    } while (entryPosition < boundary);

                } while (remainder != 0x00);

                dwReturn = this.FatxFileByteOffsetToCluster(fcb, searchStartOffset, out returnedCluster, out continuousClusterCount);

            } while (searchStartOffset < 0x40000 && dwReturn == 0x00);

        CleanupAndExit:

            directoryOffset = searchStartOffset;

            return dwReturn;
        }

        private static bool FatxIsValidFatFileName(string fileName)
        {
            if (fileName.Length == 0 && fileName.Length > 0x2A)
                return false;

            if (fileName.Length == 1 && fileName[0] == '.' || fileName.Length == 2 && fileName[1] == '.')
                return false;

            return fileName.All(c => c >= 0x20 && c <= 0x7e && (1 << (c & 0x1f) & FatxFatIllegalTable[(c >> 3 & 0x1ffffffc) / 4]) == 0);
        }

        private NTSTATUS FatxLookupElementNameInDirectory(FatxFcb directoryFcb, string elementName, out FatxDirectoryEntry returnedDirectoryEntry, out uint returnedDirectoryByteOffset, out uint returnedEmptyDirectoryByteOffset)
        {
            if (!directoryFcb.IsDirectory)
                throw new FatxException(string.Format("Attempted to look-up an directoryEntry [{0}] in a non-directory.", elementName));

            returnedDirectoryEntry = null;
            returnedDirectoryByteOffset = 0x00;

            const uint didNotFind = 0xC0000034;

            uint fileByteOffset = directoryFcb.ByteOffset, dwErrorCode, currentCluster = directoryFcb.FirstCluster, freeEntryOffset = 0xffffffff;

            if (fileByteOffset != 0x00)
            {
                long physByteOffset;
                uint range;

                this.FatxFileByteOffsetToPhysicalByteOffset(directoryFcb, fileByteOffset, out physByteOffset, out range);

                var dirEnt = new FatxDirectoryEntry(new EndianIO(this.FscMapBuffer(physByteOffset), EndianType.Big));

                if ((dirEnt.FileNameLength == elementName.Length || dirEnt.FileNameLength <= 0x2A) && string.Compare(dirEnt.Filename, elementName, StringComparison.OrdinalIgnoreCase) == 0x00)
                {
                    returnedDirectoryEntry = dirEnt;
                    dwErrorCode = 0x00;
                    returnedDirectoryByteOffset = fileByteOffset;

                    goto FATX_EXIT_DIR_LOOKUP;
                }

                directoryFcb.ByteOffset = 0x00;

                fileByteOffset = 0x00;
            }

            if (currentCluster - 1 >= _clusterCount)
                throw new FatxException("Invalid starting cluster for directory.");

            while (true)
            {
                var physicalOffset = _backingClusterOffset + ((long)(currentCluster - 1) << _clusterShift);

                if ((physicalOffset & 0xfff) != 0)
                    throw new FatxException("Invalid physical offset was caught while looking up an element in the directory.");

                uint clusterSize = _clusterSize, continuousClusterCount, clusterIndex = 1, readerByteOffset = 0;

                do
                {
                    var readerPosition = 0;

                    var io = new EndianIO(this.FscMapBuffer(physicalOffset), EndianType.Big);

                    do
                    {
                        io.Position = readerPosition;

                        var dirEnt = new FatxDirectoryEntry(io);

                        var fileNameLength = dirEnt.FileNameLength;

                        if (freeEntryOffset == 0xffffffff && (fileNameLength == 0x00 || fileNameLength == 0xff || fileNameLength == 0xe5))
                            freeEntryOffset = readerByteOffset;

                        if (fileNameLength == 0x00 || fileNameLength == 0xff)
                        {
                            dwErrorCode = didNotFind;

                            goto FATX_EXIT_DIR_LOOKUP;
                        }

                        if (fileNameLength == elementName.Length && fileNameLength <= 0x2A && string.Compare(dirEnt.Filename, elementName, StringComparison.OrdinalIgnoreCase) == 0x00)
                        {
                            returnedDirectoryEntry = dirEnt;
                            returnedDirectoryByteOffset = readerByteOffset;

                            dwErrorCode = 0x00;

                            goto FATX_EXIT_DIR_LOOKUP;
                        }

                        readerPosition += 0x40;
                        readerByteOffset += 0x40;

                    } while (readerByteOffset < 0x1000 * clusterIndex);

                    clusterIndex++;

                    physicalOffset += 0x1000;

                } while ((clusterSize -= 0x1000) != 0);

                fileByteOffset += _clusterSize;

                dwErrorCode = this.FatxFileByteOffsetToCluster(directoryFcb, fileByteOffset, out currentCluster, out continuousClusterCount);

                if (dwErrorCode == 0xC0000011)
                    dwErrorCode = didNotFind;
                else if (fileByteOffset >= 0x40000)
                    dwErrorCode = 0xC0000102;

                if (dwErrorCode != 0x00)
                    break;
            }

        FATX_EXIT_DIR_LOOKUP:

            if (dwErrorCode != didNotFind)
                freeEntryOffset = 0xfffffffe;

            returnedEmptyDirectoryByteOffset = freeEntryOffset;

            return dwErrorCode;
        }

        private NTSTATUS FatxUpdateDirectoryEntry(FatxFcb directoryFcb)
        {
            if (directoryFcb.IsRootDir)
                throw new FatxException("Attempted to update the root directory, which is not a directory entry.");

            if (directoryFcb.ParentFCB == null)
                return NT.STATUS_FILE_CORRUPT_ERROR;

            uint range;
            long physicalOffset;

            this.FatxFileByteOffsetToPhysicalByteOffset(directoryFcb.ParentFCB, directoryFcb.DirectoryEntryByteOffset, out physicalOffset, out range);

            var dataBuffer = this.FscMapBuffer(physicalOffset);

            var io = new EndianIO(new MemoryStream(dataBuffer), EndianType.Big);

            io.Write((byte)directoryFcb.FileName.Length);
            io.Write(directoryFcb.FileAttributes);
            io.WriteAsciiString(directoryFcb.FileName);

            var padding = new byte[0x2a - directoryFcb.FileName.Length];
            padding.Memset(0xff);
            io.Write(padding);

            io.Position = 0x2c;

            io.Write(directoryFcb.FirstCluster);
            io.Write(directoryFcb.FileSize);
            io.Write(directoryFcb.CreationTimeStamp);
            io.Write(FatxTimeToFatTimestamp(directoryFcb.LastWriteTimeStamp));
            io.Write(directoryFcb.LastAccessTimeStamp);

            io.Close();

            FscWriteBuffer(physicalOffset, 0x40, dataBuffer);

            directoryFcb.State &= 0xDF;

            return NT.STATUS_SUCCESS;
        }

        private uint FatxFileByteOffsetToPhysicalByteOffset(FatxFcb fcb, uint fileByteOffset, out long physicalByteOffset, out uint continuousClusterRange)
        {
            uint cluster, continuousClusterCount;

            var dwErrorCode = this.FatxFileByteOffsetToCluster(fcb, fileByteOffset, out cluster, out continuousClusterCount);

            if (dwErrorCode != 0)
            {
                if (dwErrorCode == 0xC0000011)
                    dwErrorCode = 0xC0000102;

                physicalByteOffset = 0x00;
                continuousClusterRange = 0x00;
            }
            else
            {
                physicalByteOffset = _backingClusterOffset;
                physicalByteOffset += (long)cluster - 1 << _clusterShift;
                physicalByteOffset += _clusterSizeM1 & fileByteOffset;

                continuousClusterRange = (uint)(((long)continuousClusterCount << _clusterShift) - (_clusterSizeM1 & fileByteOffset));
            }

            return dwErrorCode;
        }

        private uint FatxFileByteOffsetToCluster(FatxFcb fcb, uint fileByteOffset, out uint returnedCluster, out uint continuousClusterCount)
        {
            returnedCluster = 0x00;
            continuousClusterCount = 0x00;

            if (fcb.EndOfFile != 0xffffffff && fileByteOffset >= fcb.EndOfFile)
                return 0xC0000011;

            int
                activeCacheEntryIndex = fcb.CacheHeadIndex,
                finalCacheEntryIndex = activeCacheEntryIndex,
                currentCacheEntryIndex = -1,
                lastClusterIndex = (int)(fileByteOffset >> _clusterShift),
                fcbCacheHeadIndex = activeCacheEntryIndex & 0xff;

            var fillEmptyClusterCacheEntry = false;

            FatxCacheEntry cacheEntry = null, finalCacheEntry;

            do
            {
                var entry = fcb.Cache[activeCacheEntryIndex];

                if (entry.ContiguousClusters == 0)
                {
                    fillEmptyClusterCacheEntry = true;
                    break;
                }

                if (lastClusterIndex >= entry.ClusterIndex)
                {
                    if (lastClusterIndex < entry.ClusterIndex + entry.ContiguousClusters)
                    {
                        finalCacheEntry = entry;

                        goto UpdateCacheAndReturn;
                    }

                    if (cacheEntry == null || entry.ClusterIndex > cacheEntry.ClusterIndex)
                    {
                        cacheEntry = entry;
                        currentCacheEntryIndex = activeCacheEntryIndex;
                    }
                }

                activeCacheEntryIndex = entry.NextIndex;
                finalCacheEntryIndex = activeCacheEntryIndex;

            } while (fcbCacheHeadIndex != activeCacheEntryIndex);

            int currentClusterIndex = 0, currentCluster;

            if (cacheEntry != null)
            {
                currentCluster = (int)(cacheEntry.StartingCluster + cacheEntry.ContiguousClusters) - 1;
                currentClusterIndex = (int)(cacheEntry.ContiguousClusters + cacheEntry.ClusterIndex) - 1;

                if (currentCluster - 1 >= _clusterCount)
                    throw new FatxException(string.Format("Invalid CurrentCluster detected [0x{0:X8}].", currentCluster - 1));
            }
            else
            {
                currentCluster = (int)fcb.FirstCluster;

                if (fcb.FirstCluster - 1 >= _clusterCount)
                {
                    if (currentCluster != 0 && currentCluster != -1 || currentCluster == 0 && fcb.IsDirectory)
                        return 0xC0000102;

                    fcb.LastCluster = (uint)activeCacheEntryIndex;
                    fcb.EndOfFile = (uint)(currentClusterIndex << _clusterShift);

                    return 0x00;
                }
            }

            int
                contiguousClusterCount = 1,
                firstCluster = currentCluster,
                firstContinuousClusterIndex = currentClusterIndex,
                chainBoundOffset = 0,
                nextCluster = currentCluster;

            byte[] buffer = null;

            if (currentClusterIndex < lastClusterIndex)
            {
                do
                {
                    currentClusterIndex++;

                    var chainMapOffset = nextCluster << _chainShift;

                    if (buffer == null || chainBoundOffset != (chainMapOffset & 0xFFFFF000))
                    {
                        chainBoundOffset = (int)(chainMapOffset & 0xFFFFF000);

                        buffer = this.FscMapBuffer(_bootSectorSize + chainBoundOffset);
                    }

                    var previousCluster = nextCluster;

                    if (_chainShift != 1)
                        nextCluster = buffer.ReadInt32(chainMapOffset & 0xfff);
                    else
                    {
                        nextCluster = buffer.ReadInt16(chainMapOffset & 0xfff);

                        if ((uint)nextCluster >= 0xfff0)
                            nextCluster = ExtendSign(nextCluster, 16);
                    }

                    if ((uint)(nextCluster - 1) >= _clusterCount)
                    {
                        if (nextCluster != -1)
                            return 0xC0000102;

                        fcb.LastCluster = (uint)previousCluster;
                        fcb.EndOfFile = (uint)(currentClusterIndex << _clusterShift);

                        return 0xC0000011;
                    }

                    if (previousCluster + 1 == nextCluster)
                        contiguousClusterCount++;
                    else
                    {
                        if (cacheEntry != null)
                        {
                            if (firstCluster != cacheEntry.StartingCluster + cacheEntry.ContiguousClusters - 1 || firstContinuousClusterIndex != cacheEntry.ClusterIndex + cacheEntry.ContiguousClusters - 1)
                                throw new FatxException("Detected invalid CurrentCluster.");

                            cacheEntry.ContiguousClusters = (uint)(cacheEntry.ContiguousClusters + contiguousClusterCount) - 1;

                            cacheEntry = null;
                        }
                        else if (fillEmptyClusterCacheEntry)
                            fillEmptyClusterCacheEntry = FatxFillEmptyClusterCacheEntry(fcb, (uint)firstCluster, (uint)firstContinuousClusterIndex, (uint)contiguousClusterCount);

                        contiguousClusterCount = 1;
                        firstCluster = nextCluster;
                        firstContinuousClusterIndex = currentClusterIndex;
                    }

                } while (currentClusterIndex < lastClusterIndex);

                while (true)
                {
                    var chainMapOffset = nextCluster << _chainShift;

                    if (chainBoundOffset != (chainMapOffset & 0xfffff000))
                        break;

                    var previousCluster = nextCluster;

                    if (_chainShift != 0x01)
                        nextCluster = buffer.ReadInt32(chainMapOffset & 0xfff);
                    else
                    {
                        nextCluster = buffer.ReadInt16(chainMapOffset & 0xfff);

                        if ((uint)nextCluster >= 0xFFF0)
                            nextCluster = ExtendSign(nextCluster, 16);
                    }

                    if (previousCluster + 1 == nextCluster)
                    {
                        contiguousClusterCount++;

                        continue;
                    }

                    if (nextCluster == -1)
                    {
                        fcb.EndOfFile = (uint)((firstContinuousClusterIndex + contiguousClusterCount) << _clusterShift);
                        fcb.LastCluster = (uint)previousCluster;
                    }
                    else if (nextCluster - 1 < _clusterCount && fillEmptyClusterCacheEntry)
                        FatxFillEmptyClusterCacheEntry(fcb, (uint)nextCluster, (uint)(firstContinuousClusterIndex + contiguousClusterCount), 1);

                    break;
                }
            }

            if (cacheEntry != null)
            {
                if (cacheEntry.StartingCluster + cacheEntry.ContiguousClusters - 1 != firstCluster || cacheEntry.ClusterIndex + cacheEntry.ContiguousClusters - 1 != firstContinuousClusterIndex)
                    throw new FatxException("Invalid cache directory entry detected.");

                cacheEntry.ContiguousClusters = (uint)(cacheEntry.ContiguousClusters + contiguousClusterCount) - 1;

                finalCacheEntryIndex = currentCacheEntryIndex & 0xff;

                finalCacheEntry = cacheEntry;
            }
            else
            {
                finalCacheEntryIndex = fcb.Cache[fcb.CacheHeadIndex].PreviousIndex;

                finalCacheEntry = fcb.Cache[finalCacheEntryIndex];

                finalCacheEntry.ContiguousClusters = (uint)contiguousClusterCount;
                finalCacheEntry.StartingCluster = (uint)firstCluster;
                finalCacheEntry.ClusterIndex = (uint)firstContinuousClusterIndex;
            }

        UpdateCacheAndReturn:

            var headCacheIndex = finalCacheEntryIndex & 0xff;

            if (headCacheIndex != fcb.CacheHeadIndex)
            {
                if (headCacheIndex != fcb.Cache[fcb.CacheHeadIndex].PreviousIndex)
                    FatxMoveClusterCacheEntryToTail(fcb, headCacheIndex);

                fcb.CacheHeadIndex = (byte)finalCacheEntryIndex;
            }

            returnedCluster = (uint)(finalCacheEntry.StartingCluster - finalCacheEntry.ClusterIndex + lastClusterIndex);

            continuousClusterCount = finalCacheEntry.StartingCluster + finalCacheEntry.ContiguousClusters - returnedCluster;

            return 0x00;
        }

        private static bool FatxFillEmptyClusterCacheEntry(FatxFcb fcb, uint startingCluster, uint clusterIndex, uint contiguousClusters)
        {
            int index = fcb.CacheHeadIndex;

            do
            {
                var entry = fcb.Cache[index];

                if (entry.ContiguousClusters != 0)
                {
                    index = entry.NextIndex;
                    continue;
                }

                entry.ContiguousClusters = contiguousClusters;
                entry.StartingCluster = startingCluster;
                entry.ClusterIndex = clusterIndex;

                return fcb.Cache[entry.NextIndex].ContiguousClusters == 0;

            } while (fcb.CacheHeadIndex != index);

            return false;
        }

        private static void FatxMoveClusterCacheEntryToTail(FatxFcb fcb, int index)
        {
            if (fcb.CacheHeadIndex == index)
                throw new FatxException("Attempted to move a cache directory entry to the same index.");

            var cacheEntry = fcb.Cache[index];
            var headCacheEntry = fcb.Cache[fcb.CacheHeadIndex];
            
            fcb.Cache[cacheEntry.PreviousIndex].NextIndex = cacheEntry.NextIndex;
            fcb.Cache[cacheEntry.NextIndex].PreviousIndex = cacheEntry.PreviousIndex;

            cacheEntry.NextIndex = fcb.CacheHeadIndex;
            cacheEntry.PreviousIndex = headCacheEntry.PreviousIndex;

            fcb.Cache[headCacheEntry.PreviousIndex].NextIndex = index;

            headCacheEntry.PreviousIndex = index;
        }

        private static void FatxResetClusterCache(FatxFcb fcb)
        {
            fcb.Cache = new List<FatxCacheEntry>(); //set max

            uint maxCtr, lastIndex;

            if (fcb.IsDirectory)
            {
                lastIndex = 0x10000000;
                maxCtr = 0x30000000;
            }
            else
            {
                lastIndex = 0x90000000;
                maxCtr = 0xB0000000;
            }

            uint ctr = 0x10000000, index = 0;

            do
            {
                var entry = new FatxCacheEntry();

                entry.State = entry.State & ~0xF0000000 | ctr & 0xF0000000;
                entry.Flags = entry.Flags & ~0xF0000000 | index << 28 & 0xF0000000;

                index++;

                fcb.Cache.Add(entry);

            } while ((ctr += 0x10000000) < maxCtr);

            fcb.Cache[0].Flags = fcb.Cache[0].Flags & ~0xf0000000 | lastIndex;

            fcb.Cache[fcb.Cache.Count - 1].State &= 0xfffffff;

            fcb.CacheHeadIndex = 0x00;
        }

        private static void FatxInvalidateClusterCache(FatxFcb fcb, uint cluster)
        {
            int index = fcb.CacheHeadIndex;

            do
            {
                var entry = fcb.Cache[index];

                if (entry.ContiguousClusters == 0x00)
                    break;

                var tempIndex = index;

                index = entry.NextIndex;

                if (cluster <= entry.PreviousIndex)
                {
                    entry.ContiguousClusters = 0x00;

                    if (fcb.CacheHeadIndex != tempIndex)
                        FatxMoveClusterCacheEntryToTail(fcb, tempIndex);
                    else
                        fcb.CacheHeadIndex = (byte)index;
                }
                else if (cluster < entry.PreviousIndex + entry.ContiguousClusters)
                    entry.ContiguousClusters = cluster - entry.ContiguousClusters;

            } while (index != fcb.CacheHeadIndex);
        }

        private static void FatxAppendClusterRunsToClusterCache(FatxFcb fcb, uint lastCluster, FatxAllocationState allocationState, long count)
        {
            if (fcb.EndOfFile == uint.MaxValue)
                throw new FatxException("Could not append the cluster runs for an invalid FCB.");

            if (count < 1)
                throw new FatxException("Cannot append zero clusters to the cache.");

            if (lastCluster == 0x00)
                goto append_to_cache;

            int index = fcb.CacheHeadIndex;

            do
            {
                var entry = fcb.Cache[index];

                if (entry.ContiguousClusters == 0x00)
                    goto append_to_cache;

                if (entry.ClusterIndex >= lastCluster)
                    throw new FatxException("Invalid cluster index detected while appending to the cache.");

                if (entry.ClusterIndex + entry.ContiguousClusters == lastCluster)
                {
                    index = entry.NextIndex;
                    continue;
                }

                var allocState = allocationState.AllocationStates[0];

                if (entry.StartingCluster + entry.ContiguousClusters != allocState.FirstAllocatedCluster)
                    break;

                allocState.FirstAllocatedCluster = entry.StartingCluster;
                allocState.ContiguousClusters += entry.ContiguousClusters;

                lastCluster = entry.ClusterIndex;

                if (index != fcb.CacheHeadIndex)
                    FatxMoveClusterCacheEntryToTail(fcb, fcb.CacheHeadIndex);
                else
                    fcb.CacheHeadIndex = (byte)entry.NextIndex;
                    
                break;

            } while (index != fcb.CacheHeadIndex);

            append_to_cache:

            var lastIndex = (int)(count - 1);

            for (var x = lastIndex; x > 0; x--)
                lastCluster += allocationState.AllocationStates[x].ContiguousClusters;

            var headEntry = fcb.Cache[fcb.CacheHeadIndex];

            for (index = 0; lastIndex > 0; lastIndex--)
            {
                index = headEntry.PreviousIndex;

                var lastAllocationState = allocationState.AllocationStates[lastIndex];

                lastCluster -= lastAllocationState.ContiguousClusters;

                var entry = fcb.Cache[index];

                entry.ClusterIndex = lastCluster;

                entry.StartingCluster = lastAllocationState.FirstAllocatedCluster;
                entry.ContiguousClusters = lastAllocationState.ContiguousClusters;

                headEntry = entry;
            }

            fcb.CacheHeadIndex = (byte)index;
        }

        private void FatxPartiallyCachedSynchronousIo(FatxFcb fcb, FatxIO operation, uint fileByteOffset, EndianIO io, uint bytesToReadWrite)
        {
            uint
                lowOffset = fileByteOffset & 0xfff,
                fileByteOffsetLocal = fileByteOffset,
                bytesToReadWriteLocal = bytesToReadWrite;

            if (lowOffset != 0)
            {
                var fullyCachedIOLength = 0x1000 - lowOffset;

                if (fullyCachedIOLength >= bytesToReadWrite)
                    throw new FatxException("Fully cached IO byte-length was greater than the total requested IO byte-length.");

                this.FatxFullyCachedSynchronousIo(fcb, operation, fileByteOffset, io, fullyCachedIOLength, true);

                fileByteOffset += fullyCachedIOLength;
                bytesToReadWrite -= fullyCachedIOLength;
            }

            if (bytesToReadWrite < 0x1000)
                throw new FatxException("Detected an invalid amount of bytes left to read/write.");

            var nonCachedIoLength = bytesToReadWrite & 0xfffff000;

            this.FatxNonCachedSynchronousIo(fcb, operation, fileByteOffset, io, nonCachedIoLength);

            var fullyCachedRemainder = bytesToReadWrite - nonCachedIoLength;

            if (fullyCachedRemainder != 0x00 && this.FatxFullyCachedSynchronousIo(fcb, operation, fileByteOffset + nonCachedIoLength, io, fullyCachedRemainder, true) > int.MaxValue)
                this.FatxSynchronousIoTail(fcb, operation, fileByteOffsetLocal, bytesToReadWriteLocal);
        }

        private uint FatxFullyCachedSynchronousIo(FatxFcb fcb, FatxIO operation, uint fileByteOffset, EndianIO io, uint bytesToReadWrite, bool doNotUpdateAccess)
        {
            if (bytesToReadWrite == 0)
                throw new FatxException("Attempted to read/write zero bytes.");

            var fileByteOffsetWriteLowZero = (fileByteOffset & 0xfff) == 0;

            uint
                contiguousClusterRange,
                bytesToReadWriteLocal = bytesToReadWrite,
                fileByteOffsetLocal = fileByteOffset;

            long physicalOffset;

            var dwErrorCode = this.FatxFileByteOffsetToPhysicalByteOffset(fcb, fileByteOffsetLocal, out physicalOffset, out contiguousClusterRange);

            if (dwErrorCode != 0)
                throw new FatxException(string.Format("FatxFileByteOffsetToPhysicalByteOffset failed with 0x{0:X8}.", dwErrorCode));

            do
            {
                do
                {
                    var remainder = 0x1000 - ((uint)physicalOffset & 0xfff);

                    if (remainder < contiguousClusterRange)
                        contiguousClusterRange -= remainder;
                    else
                    {
                        remainder = contiguousClusterRange;
                        contiguousClusterRange = 0;
                    }

                    if (remainder > bytesToReadWriteLocal)
                        remainder = bytesToReadWriteLocal;

                    if (operation == FatxIO.Read)
                        io.Write(this.FscMapBuffer(physicalOffset), 0x00, (int)remainder);
                    else
                    {
                        byte[] data;

                        if (remainder == 0x1000 || fileByteOffsetWriteLowZero && fileByteOffsetLocal + bytesToReadWriteLocal >= fcb.FileSize)
                            data = new byte[0x1000];
                        else
                            data = this.FscMapBuffer(physicalOffset);

                        data.Write(io.ReadByteArray(remainder));

                        this.FscWriteBuffer(physicalOffset, remainder, data);
                    }

                    bytesToReadWriteLocal -= remainder;
                    physicalOffset += remainder;

                    if (bytesToReadWriteLocal != 0)
                        fileByteOffsetLocal += remainder;
                    else
                    {
                        if (!doNotUpdateAccess)
                            this.FatxSynchronousIoTail(fcb, operation, fileByteOffset, bytesToReadWrite);

                        return 0x00;
                    }

                } while (contiguousClusterRange != 0);

            } while (this.FatxFileByteOffsetToPhysicalByteOffset(fcb, fileByteOffsetLocal, out physicalOffset, out contiguousClusterRange) == 0);

            return 0x00;
        }

        private void FatxNonCachedSynchronousIo(FatxFcb fcb, FatxIO operation, uint fileByteOffset, EndianIO io, uint bytesToReadWrite)
        {
            var ioCount = bytesToReadWrite + 0x1ff & ~0x1ffu;

            if (ioCount == 0)
                throw new FatxException("Detected an invalid amount of bytes to read/write for a non-cached request.");

            if ((_sectorsPerClusterM1 & fileByteOffset) != 0)
                throw new FatxException("Detected an invalid start position to read/write for a non-cached request.");

            long physicalOffset;
            uint clusterRange;

            this.FatxFileByteOffsetToPhysicalByteOffset(fcb, fileByteOffset, out physicalOffset, out clusterRange);

            do
            {
                uint temporaryBufferLength = 0;
                long currentPhysicalOffset = physicalOffset, nextPhysicalOffset;

                do
                {
                    if (clusterRange <= ioCount)
                        ioCount -= clusterRange;
                    else
                    {
                        clusterRange = ioCount;
                        ioCount = 0;
                    }

                    temporaryBufferLength += clusterRange;
                    fileByteOffset += clusterRange;

                    if (ioCount == 0)
                        break;

                    nextPhysicalOffset = physicalOffset + clusterRange;

                    var dwReturn = this.FatxFileByteOffsetToPhysicalByteOffset(fcb, fileByteOffset, out physicalOffset, out clusterRange);

                    if (dwReturn != 0)
                        throw new FatxException(string.Format("File byte offset to physical byte offset failed during non-cached synchronous I/O. [0x{0:X8}]", dwReturn));

                } while (nextPhysicalOffset == physicalOffset);

                if (operation == FatxIO.Read)
                    io.Write(this.FscMapBuffer(currentPhysicalOffset, temporaryBufferLength));
                else
                    this.FscWriteBuffer(currentPhysicalOffset, temporaryBufferLength, io.ReadByteArray(temporaryBufferLength));

            } while (ioCount != 0);
        }

        private void FatxSynchronousIoTail(FatxFcb fcb, FatxIO ioType, uint fileByteOffset, uint bytesToReadWrite)
        {
            if (ioType != FatxIO.Write)
                return;

            fcb.LastWriteTimeStamp = DateTime.Now;

            fcb.State |= 0x20;

            var endOfFile = fileByteOffset + bytesToReadWrite;

            if (endOfFile <= fcb.FileSize)
                return;

            fcb.FileSize = endOfFile;

            this.FatxUpdateDirectoryEntry(fcb);
        }

        private static readonly byte[] DeleteFileConst = { 0xe5 };

        private uint FatxDeleteFile(FatxFcb directoryFcb, uint directoryByteOffset)
        {
            if (!directoryFcb.IsDirectory)
                throw new FatxException("Invalid FCB detected while attempting to delete a file.");

            uint clusterRange;
            long physicalOffset;

            this.FatxFileByteOffsetToPhysicalByteOffset(directoryFcb, directoryByteOffset, out physicalOffset, out clusterRange);

            var dataBuffer = this.FscMapBuffer(physicalOffset);

            dataBuffer.Write(DeleteFileConst);

            this.FscWriteBuffer(physicalOffset, 0x40, dataBuffer);

            return FatxFreeClusters(dataBuffer.ReadUInt32(0x2c), false);
        }

        private uint FatxMarkDirectoryEntryDeleted(FatxFcb fcb)
        {
            if (fcb.IsRootDir)
                throw new FatxException("Attempted to mark the root directory listing as deleted.");

            if (fcb.ParentFCB == null)
                throw new FatxException("Detected an invalid parent FCB. [0xC0000102]");

            long physicalOffset;
            uint clusterRange;

            var dwReturn = this.FatxFileByteOffsetToPhysicalByteOffset(fcb.ParentFCB, fcb.DirectoryEntryByteOffset, out physicalOffset, out clusterRange);

            var buffer = this.FscMapBuffer(physicalOffset);

            buffer.Write(DeleteFileConst);

            this.FscWriteBuffer(physicalOffset, 0x40, buffer);

            return dwReturn;
        }

        private void FatxInitializeDirectoryCluster(uint cluster)
        {
            cluster--;

            if (cluster >= _clusterCount)
                throw new FatxException("Cluster was outside of volume cluster range.");

            var physicalOffset = _backingClusterOffset + ((long)cluster << _clusterShift);

            if ((physicalOffset & 0xfff) != 0x00)
                throw new FatxException("Detected an invalid physical offset.");

            var clusterSize = _clusterSize;

            var data = new byte[0x1000];

            data.Memset(0xff);

            do
            {
                this.FscWriteBuffer(physicalOffset, 0x1000, data);

                physicalOffset += 0x1000;

            } while ((clusterSize -= 0x1000) > 0x00);
        }

        private uint FatxFreeClusters(uint firstClusterNumber, bool updateEndofClusterLink)
        {
            if (firstClusterNumber - 1 >= _clusterCount)
                return 0x00;

            uint
                lastFreeClusterIndex = _lastFreeClusterIndex,
                previousChainMapPosition = 0,
                freedCluserCount = 0,
                closingLink = updateEndofClusterLink ? uint.MaxValue : 0x00,
                currentCluster = firstClusterNumber;

            EndianIO io = null;

            do
            {
                var chainMapPosition = currentCluster << _chainShift;

                if (io != null)
                {
                    if ((chainMapPosition & 0xfffff000) == previousChainMapPosition)
                        goto _continue;

                    this.FscWriteBuffer(_bootSectorSize + (previousChainMapPosition & 0xfffff000), 0x1000, io.ToArray());

                    FatxFreeClusterRange(previousChainMapPosition >> _chainShift >> _allocationShift);

                    _freedClusterCount += freedCluserCount;

                    _lastFreeClusterIndex = lastFreeClusterIndex;

                    if (_freedClusterCount != 0 && lastFreeClusterIndex - 1 >= _clusterCount)
                        throw new FatxException("Invalid cluster index detected while freeing clusters.");

                    freedCluserCount = 0x00;
                }

                previousChainMapPosition = chainMapPosition & 0xfffff000;

                io = new EndianIO(this.FscMapBuffer(_bootSectorSize + previousChainMapPosition), EndianType.Big);

            _continue:

                if (closingLink == 0x00 && currentCluster < lastFreeClusterIndex)
                    lastFreeClusterIndex = firstClusterNumber;

                var localChainMapPosition = chainMapPosition & 0xfff;

                io.Position = localChainMapPosition;

                if (_chainShift == 0x01)
                {
                    currentCluster = io.ReadUInt16();

                    if (currentCluster >= 0xfff0)
                        currentCluster = ExtendSign(currentCluster, 16);

                    io.Position = localChainMapPosition;

                    io.Write((ushort)closingLink);
                }
                else
                {
                    currentCluster = io.ReadUInt32();
                    io.Position = localChainMapPosition;
                    io.Write(closingLink);
                }

                if (closingLink == 0x00)
                    freedCluserCount++;

                closingLink = 0x00;

            } while (currentCluster - 1 < _clusterCount);

            if (currentCluster != uint.MaxValue)
                throw new FatxException("Corrupt FAT chain found while freeing clusters.");

            this.FscWriteBuffer(_bootSectorSize + previousChainMapPosition, 0x1000, io.ToArray());

            FatxFreeClusterRange(previousChainMapPosition >> _chainShift >> _allocationShift);

            _freedClusterCount += freedCluserCount;
            _lastFreeClusterIndex = lastFreeClusterIndex;

            if (_freedClusterCount != 0 && lastFreeClusterIndex - 1 >= _clusterCount)
                throw new FatxException("Invalid cluster index detected while freeing clusters.");

            if (_freedClusterCount >= _clusterCount)
                throw new FatxException("Freed cluster count was outside of total cluster range.");

            return 0x01;
        }

        private void FatxFreeClusterRange(uint clusterIndex)
        {
            if (this._allocSupport == null)
                return;

            var allocationIndex = clusterIndex >> 6;

            if (allocationIndex >= 0x20)
                throw new FatxException("Allocation support index was out of bounds.");

            var pos = allocationIndex << 3;

            this._allocSupport.Position = pos;

            var marker = 1L << (int)(0x3F - (clusterIndex & 0x3f)) | this._allocSupport.ReadInt64();

            this._allocSupport.Position = pos;

            this._allocSupport.Write(marker);
        }

        private uint FatxSkipAheadToFreeClusterRange(uint cluster)
        {
            if (cluster - 1 >= _clusterCount)
                throw new FatxException("Cluster was outside of volume cluster range.");

            if (this._allocSupport == null)
                return cluster;

            uint clusterIndex = cluster >> _allocationShift, allocationIndex = clusterIndex >> 6;

            if (allocationIndex >= 0x20)
                throw new FatxException("Allocation support index was out of bounds.");

            var marker = ulong.MaxValue >> (int)(clusterIndex & 0x3f);

            this._allocSupport.Position = allocationIndex << 3;

            do
            {
                marker &= this._allocSupport.ReadUInt64();

                if (marker != 0x00)
                {
                    var newAllocationIndex = (allocationIndex << 6) + ((long)marker).CountLeadingZerosDouble();

                    return newAllocationIndex == clusterIndex ? cluster : (uint)(newAllocationIndex << _allocationShift);
                }

                marker = ulong.MaxValue;

            } while (++allocationIndex < 0x20);

            return cluster;
        }

        private void FatxAllocateClusters(uint currentCluster, uint requestedAllocationCount, out FatxAllocationState allocationState, out uint totalAllocated, out uint lastAllocatedCluster)
        {
            if (requestedAllocationCount > _freedClusterCount)
                throw new FatxException("Requested amount of clusters to be allocated is outside of the free cluster range. [0xC000007F]");

            allocationState = new FatxAllocationState();

            uint newCurrentCluster, chainMapLocation;

            if (currentCluster != 0x00)
            {
                newCurrentCluster = currentCluster + 1;
                chainMapLocation = currentCluster << _chainShift;
            }
            else
            {
                newCurrentCluster = _lastFreeClusterIndex;
                chainMapLocation = uint.MaxValue;
            }

            uint
                startingCluster = currentCluster,
                lastCluster = 0x00,
                chainMapOffset = 0x00,
                firstAllocatedCluster = 0x00,
                nCurrentCluster = 0x00,
                tCurrentCluster = 0x00,
                allocatedContiguousClusters = 0x00,
                allocatedCount = 0x00,
                oldPosition = 0x00;

            bool
                clearFreeClusterRange = ((_lastFreeClusterIndex - newCurrentCluster).CountLeadingZerosWord() >> 5 & 0x01) != 0,
                isAllocationContiguous = false,
                flushBuffers = false,
                newTable = false;

            EndianIO io = null;

            while (true)
            {
                int clusterNum; // might have to be outside the loop
                uint clusterOffset;

                while (true)
                {
                    if (_clusterCount + 1 == newCurrentCluster)
                    {
                        if (clearFreeClusterRange)
                            throw new FatxException("FAT table is corrupt. [0xC0000032]");

                        clearFreeClusterRange = true;
                        newCurrentCluster = _lastFreeClusterIndex;
                    }

                    if (newCurrentCluster - 1 >= _clusterCount)
                        throw new FatxException("Cluster index is outside of the volume cluster range.");

                    clusterOffset = newCurrentCluster << _chainShift;

                    if (io != null)
                    {
                        if ((clusterOffset & 0xfffff000) == chainMapOffset)
                            goto UseSameFATXTableBuffer;

                        if (flushBuffers)
                        {
                            this.FscWriteBuffer(_bootSectorSize + chainMapOffset, 0x1000, io.ToArray());

                            _freedClusterCount -= allocatedCount;

                            if (startingCluster != 0x00 && !isAllocationContiguous)
                                this.FatxLinkClusterChains(startingCluster, lastCluster);

                            startingCluster = tCurrentCluster;
                        }

                        if (clearFreeClusterRange)
                        {
                            var allocationIndex = chainMapOffset + 0x1000 >> _chainShift >> _allocationShift;

                            if (allocationIndex != 0x00)
                                FatxFreeClusterRange(allocationIndex - 1);
                        }

                        io.Close();
                        io = null;

                        allocatedCount = 0x00;
                        flushBuffers = false;
                    }

                    var freeCurrentCluster = this.FatxSkipAheadToFreeClusterRange(newCurrentCluster);

                    if (freeCurrentCluster == newCurrentCluster)
                        break;

                    newCurrentCluster = freeCurrentCluster;
                }

                chainMapOffset = clusterOffset & 0xfffff000;

                io = new EndianIO(this.FscMapBuffer(_bootSectorSize + chainMapOffset), EndianType.Big);

                if (startingCluster == 0x00 || startingCluster != currentCluster || (chainMapLocation & 0xfffff000) != chainMapOffset)
                {
                    newTable = false;
                    isAllocationContiguous = false;

                    goto UseSameFATXTableBuffer;
                }

                oldPosition = chainMapLocation & 0xfff;

                io.Position = oldPosition;

                isAllocationContiguous = true;

                newTable = true;

                if (_chainShift != 0x01)
                    clusterNum = io.ReadInt32();
                else
                {
                    clusterNum = io.ReadInt16();

                    if (clusterNum >= -16)
                        clusterNum = ExtendSign(clusterNum, 16);
                }

                if (clusterNum != -1)
                    throw new FatxException("Found an invalid cluster number while allocating clusters.");

                UseSameFATXTableBuffer:

                io.Position = clusterOffset & 0xfff;

                if (_chainShift != 0x01)
                    clusterNum = io.ReadInt32();
                else
                {
                    clusterNum = io.ReadUInt16();

                    if (clusterNum >= -16)
                        clusterNum = ExtendSign(clusterNum, 16);
                }

                if (clusterNum != 0x00)
                {
                    newCurrentCluster++;
                    continue;
                }

                io.Position = clusterOffset & 0xfff;

                if (_chainShift == 0x01)
                    io.Write(ushort.MaxValue);
                else
                    io.Write(uint.MaxValue);

                flushBuffers = true;
                allocatedCount++;

                if (newTable)
                {
                    io.Position = oldPosition;

                    if (_chainShift == 0x01)
                        io.Write((ushort)newCurrentCluster);
                    else
                        io.Write(newCurrentCluster);
                }
                else
                {
                    newTable = true;
                    lastCluster = newCurrentCluster;
                }

                if (firstAllocatedCluster == 0x00)
                {
                    firstAllocatedCluster = newCurrentCluster;
                    nCurrentCluster = newCurrentCluster;

                    if (allocatedContiguousClusters != 0x00)
                        throw new FatxException("Invalid allocation index detected while allocating clusters.");
                }
                else if ((nCurrentCluster + allocatedContiguousClusters) != newCurrentCluster)
                {
                    if (allocationState.AllocationStates.Count < 0x0a)
                    {
                        allocationState.AllocationStates.Add(new FatxAllocationState.AllocationState {
                            FirstAllocatedCluster = nCurrentCluster,
                            ContiguousClusters = allocatedContiguousClusters
                        });
                    }

                    nCurrentCluster = newCurrentCluster;
                    allocatedContiguousClusters = 0x00;
                }

                allocatedContiguousClusters++;

                tCurrentCluster = newCurrentCluster;
                oldPosition = clusterOffset & 0xfff;

                if (--requestedAllocationCount == 0x00)
                    break;

                newCurrentCluster++;
            }

            this.FscWriteBuffer(_bootSectorSize + chainMapOffset, 0x1000, io.ToArray());

            if (clearFreeClusterRange)
                _lastFreeClusterIndex = newCurrentCluster + 1;

            _freedClusterCount -= allocatedCount;

            if (_lastFreeClusterIndex == 0x00 && _freedClusterCount - 1 >= _clusterCount)
                throw new FatxException("Free cluster count is outside of the volume cluster range.");

            if (startingCluster != 0x00 && !isAllocationContiguous)
                this.FatxLinkClusterChains(startingCluster, lastCluster);

            if (allocationState.AllocationStates.Count < 0x0A)
            {
                allocationState.AllocationStates.Add(new FatxAllocationState.AllocationState {
                    FirstAllocatedCluster = nCurrentCluster,
                    ContiguousClusters = allocatedContiguousClusters
                });
            }

            totalAllocated = (uint)allocationState.AllocationStates.Count;

            lastAllocatedCluster = newCurrentCluster;
        }

        private void FatxLinkClusterChains(uint startingCluster, uint lastCluster)
        {
            if (startingCluster - 1 >= _clusterCount)
                throw new FatxException("Starting cluster is outside of the total cluster range.");

            if (lastCluster - 1 >= _clusterCount)
                throw new FatxException("Ending cluster is outside of the total cluster range.");

            var physicalOffset = _bootSectorSize + (startingCluster << _chainShift & uint.MaxValue);

            var buffer = this.FscMapBuffer(physicalOffset);
            
            int currentCluster;

            if (_chainShift != 0x01)
                currentCluster = buffer.ReadInt32();
            else
            {
                currentCluster = buffer.ReadInt16();

                if (currentCluster >= -16)
                    currentCluster = ExtendSign(currentCluster, 16);
            }

            if (currentCluster != -1)
                throw new FatxException("Detected an invalid cluster chain while linking.");

            var io = new EndianIO(buffer, EndianType.Big);

            if (_chainShift == 0x01)
                io.Write((short)lastCluster);
            else
                io.Write(lastCluster);

            io.Close();

            this.FscWriteBuffer(physicalOffset, 0x1000, buffer);
        }

        private static bool FscTestForFullyCachedIo(uint fileByteOffset, uint bytesToReadWrite)
        {
            if (bytesToReadWrite == 0)
                throw new FatxException("Requested an I/O operation of zero bytes.");

            if (bytesToReadWrite < 0x1000)
                return true;

            return (fileByteOffset + bytesToReadWrite ^ fileByteOffset + 0xfff & 0xfffff000) == 0;
        }

        private byte[] FscMapBuffer(long physicalOffset, long dataSize = 0x1000)
        {
            var position = (long)((ulong)(this._deviceOffset + physicalOffset) & 0xfffffffffffff200);

            if (position >= this._deviceSize + this._deviceOffset)
                throw new FatxException(string.Format("Invalid read position detected : 0x{0:X16}.", position));

            _io.Position = position;

            var remainder = this._deviceOffset + physicalOffset & 0xfff & ~0x200;

            if (remainder == 0)
                return _io.ReadByteArray(dataSize);

            var data = new byte[dataSize];

            Array.Copy(_io.ReadByteArray(remainder + dataSize + 0xfff & 0xfffff000), remainder, data, 0, dataSize);

            return data;
        }

        private void FscWriteBuffer(long physicalOffset, long sizeToWrite, byte[] data)
        {
            var position = (long)((ulong)(this._deviceOffset + physicalOffset) & 0xfffffffffffff200);

            if (position >= this._deviceOffset + this._deviceSize)
                throw new FatxException(string.Format("Invalid write position detected : 0x{0:X16}.", position));

            _io.Position = position;

            var remainder = this._deviceOffset + physicalOffset & 0xfff & ~0x200;

            if (remainder != 0 || sizeToWrite % 0x200 != 0)
            {
                var sizeToWriteSpan = sizeToWrite + remainder + 0xfff & 0xfffff000;

                var tempBuffer = _io.ReadByteArray(sizeToWriteSpan);

                _io.Position -= sizeToWriteSpan;

                Array.Copy(data, 0, tempBuffer, remainder, sizeToWrite);

                sizeToWrite = sizeToWriteSpan;

                data = tempBuffer;
            }

            _io.Write(data, 0x00, (int)sizeToWrite);

            _io.Flush();
        }

        private static DateTime FatxFatTimestampToTime(int fatTimeStamp)
        {
            return new DateTime(
                ((fatTimeStamp >> 25) & 0x7f) + 1980,
                (fatTimeStamp >> 21) & 0x0f,
                (fatTimeStamp >> 16) & 0x1f,
                (fatTimeStamp >> 11) & 0x1f,
                (fatTimeStamp >> 5) & 0x3f,
                (fatTimeStamp & 0x1f) << 1).ToLocalTime();
        }

        private static int FatxTimeToFatTimestamp(DateTime dateTime)
        {
            dateTime = dateTime.ToUniversalTime();

            return dateTime.Year - 1980 << 25 
                | dateTime.Month << 21 
                | dateTime.Day << 16 
                | dateTime.Hour << 11 
                | dateTime.Minute << 5 
                | dateTime.Second >> 1;
        }

        private static int ExtendSign(int value, int bitLength)
        {
            if ((value & 1 << bitLength - 1) != 0)
                for (var x = bitLength; x < 32; x++)
                    value |= 1 << x;
            return value;
        }

        private static uint ExtendSign(uint value, int bitLength)
        {
            if ((value & 1 << bitLength - 1) != 0)
                for (var x = bitLength; x < 32; x++)
                    value |= 1u << x;
            return value;
        }
    }
}
