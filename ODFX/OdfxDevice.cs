using System;
using System.Runtime.InteropServices;
using CallbackFS;
using NoDev.Common;
using NoDev.Common.IO;

namespace NoDev.Odfx
{
    using NTSTATUS = UInt32;

    public abstract class OdfxDevice : GdfDevice
    {
        private readonly object _threadLock = new object();
        private readonly CallbackFileSystem _drive;

        private GdfFcb _directoryFcb;
        public string MountPoint { get; private set; }

        private uint _lastBlockNumber;
        private uint _pageSize;
        private byte _pageShift;

        protected const string OdfxGdfVolumeDescriptorSignature = "MICROSOFT*XBOX*MEDIA";

        protected OdfxDevice(string mountingPoint)
        {
            CallbackFileSystem.Initialize("44D14AF2-249C-408D-ACD3-A0BA7C8E1E11");

            this._drive = new CallbackFileSystem();

            this.MountPoint = mountingPoint;

            this.RegisterCBFSEventHandlers();
        }

        public virtual void Unmount()
        {
            this._drive.DeleteMountingPoint(0);
            this._drive.UnmountMedia(true);
            this._drive.DeleteStorage(true);
        }

        private void CbFsMountEvent(CallbackFileSystem sender)
        {
            var diskGeometry = new byte[0x08];

            DeviceControl(0x2404c, ref diskGeometry);

            _pageSize = diskGeometry.ReadUInt32(0x04);

            _pageShift = (byte)(0x1f - _pageSize.CountLeadingZerosWord());

            _lastBlockNumber = (uint)(((_pageSize * diskGeometry.ReadInt32() & ~(long)(_pageSize - 1)) >> _pageShift) & 0xffffffff);

            if (_pageSize != 0x800)
                throw new OdfxException("Invalid page size detected while mounting volume [0xC000014F].");

            var io = new EndianIO(FscMapBuffer(0x10000, 0x1000), EndianType.Little);

            if (io.ReadAsciiString(0x00, 0x14) != OdfxGdfVolumeDescriptorSignature || io.ReadAsciiString(0x7ec, 0x14) != OdfxGdfVolumeDescriptorSignature)
                throw new OdfxException("Invalid GDFX volume descriptor signature [0xC000014F].");

            io.Position = 0x14;

            _directoryFcb = new GdfFcb(0x05) {
                FirstBlockNumber = io.ReadUInt32(),
                FileSize = io.ReadInt32(),
                TimeStamp = DateTime.FromFileTime(io.ReadInt64())
            };
        }

        protected abstract byte[] FscMapBuffer(long physicalOffset, long dataSize);

        private static void CbFsUnmountEvent(CallbackFileSystem sender) { }
        private static void CbFsStorageEjectedEvent(CallbackFileSystem sender) { }
        private static void CbFsSetVolumeLabelEvent(CallbackFileSystem sender, string volumeLabel) { }
        private static void CbFsSetFileAttributesEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo, DateTime creationTime, DateTime lastAccessTime, DateTime lastWriteTime, uint attributes) { }
        private static void CbFsSetEndOfFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, long endOfFile) { }
        private static void CbFsSetAllocationSizeEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, long allocationSize) { }
        private static void CbFsRenameOrMoveEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, string newFileName) { }
        private static void CbFsWriteFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, long position, byte[] buffer, int bytesToWrite, ref int bytesWritten) { }
        private static void CbFsGetFileNameByFileIdEvent(CallbackFileSystem sender, long fileId, ref string filePath, ref ushort filePathLength) { }
        private static void CbFsFlushFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo) { }
        private static void CbFsDeleteFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo) { }
        private static void CbFsCreateFileEvent(CallbackFileSystem sender, string fileName, uint desiredAccess, uint fileAttributes, uint shareMode, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo) { }

        private static void CbFsCanFileBeDeletedEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo, ref bool canBeDeleted)
        {
            canBeDeleted = false;
        }

        private void CbFsGetVolumeLabelEvent(CallbackFileSystem sender, ref string volumeLabel)
        {
            // what we doing?
        }

        private void CbFsReadFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, long position, byte[] buffer, int bytesToRead, ref int bytesRead)
        {
            if (fileInfo.UserContext == IntPtr.Zero)
                return;

            lock (_threadLock)
            {
                var handle = (IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target;

                handle.Irp.Sp.ReadParameters = new IrpParametersRead {
                    BufferOffset = 0x00,
                    ByteOffset = position,
                    CacheBuffer = new byte[bytesToRead],
                    Length = (uint)bytesToRead
                };

                if (OdfxFsdRead(handle) != NT.STATUS_SUCCESS)
                    return;

                Array.Copy(handle.Irp.Sp.ReadParameters.CacheBuffer, buffer, bytesToRead);

                bytesRead = bytesToRead;
            }
        }

        private void CbFsOpenFileEvent(CallbackFileSystem sender, string fileName, uint desiredAccess, uint fileAttributes, uint shareMode, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            if (fileInfo.UserContext != IntPtr.Zero)
                return;

            lock (_threadLock)
            {
                var handle = new IoFsdHandle {
                    Sp = {
                        CreateParametersParameters = new IrpCreateParameters {
                            DesiredAccess = desiredAccess,
                            Options = 0x1000060,
                            FileAttributes = 0x80,
                            ShareAccess = 0x01,
                            RemainingName = fileName
                        }
                    }
                };

                if (OdfxFsdCreate(handle) != NT.STATUS_SUCCESS)
                    return;

                fileInfo.UserContext = GCHandle.ToIntPtr(GCHandle.Alloc(handle));
            }
        }

        private void CbFsCloseFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo)
        {
            if (handleInfo.UserContext == IntPtr.Zero)
                return;

            lock (_threadLock)
            {
                GCHandle.FromIntPtr(handleInfo.UserContext).Free();

                fileInfo.UserContext = IntPtr.Zero;
            }
        }

        private void CbFsIsDirectoryEmptyEvent(CallbackFileSystem sender, CbFsFileInfo directoryInfo, string fileName, ref bool isEmpty)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            lock (_threadLock)
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

                if (OdfxFsdCreate(handle) == NT.STATUS_SUCCESS)
                    return;

                IoFsdDirectoryInformation directoryInformation;

                isEmpty = OdfxFsdDirectoryControl(handle, "*", out directoryInformation) == NT.STATUS_SUCCESS;
            }
        }

        private void CbFsGetVolumeSizeEvent(CallbackFileSystem sender, ref long totalAllocationUnits, ref long availableAllocationUnits)
        {
            totalAllocationUnits = FileSize / 0x200;
            availableAllocationUnits = 0x00;
        }

        private uint _volumeId;

        private void CbFsGetVolumeIDEvent(CallbackFileSystem sender, ref uint volumeId)
        {
            if (_volumeId == 0)
                _volumeId = (uint)new Random().Next(1, int.MaxValue);

            volumeId = _volumeId;
        }

        private void CbFsGetFileInfoEvent(CallbackFileSystem sender, string fileName, ref bool fileExists,
            ref DateTime creationTime, ref DateTime lastAccessTime, ref DateTime lastWriteTime,
            ref long endOfFile, ref long allocationSize, ref CBFS_LARGE_INTEGER fileId,
            ref uint fileAttributes, ref string shortFileName, ref string realFileName)
        {
            lock (_threadLock)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    return;

                var searchSeperator = fileName.LastIndexOf('\\') + 1;
                var name = fileName.Substring(0x00, searchSeperator);

                var handle = new IoFsdHandle();

                handle.Sp.CreateParametersParameters = new IrpCreateParameters {
                    DesiredAccess = 0x100001,
                    Options = 0x1004021,
                    FileAttributes = 0x40,
                    ShareAccess = 0x03,
                    RemainingName = name
                };

                if (OdfxFsdCreate(handle) != NT.STATUS_SUCCESS)
                    return;

                IoFsdDirectoryInformation directoryInformation;

                fileExists = OdfxFsdDirectoryControl(handle, fileName.Substring(searchSeperator, fileName.Length - searchSeperator), out directoryInformation) == NT.STATUS_SUCCESS;

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
            lock (_threadLock)
            {
                OdfxDirectoryEnumerationContext context;

                if (restart && enumerationInfo.UserContext != IntPtr.Zero)
                {
                    if (GCHandle.FromIntPtr(enumerationInfo.UserContext).IsAllocated)
                        GCHandle.FromIntPtr(enumerationInfo.UserContext).Free();

                    enumerationInfo.UserContext = IntPtr.Zero;
                }

                if (enumerationInfo.UserContext != IntPtr.Zero)
                    context = (OdfxDirectoryEnumerationContext) GCHandle.FromIntPtr(enumerationInfo.UserContext).Target;
                else
                {
                    context = new OdfxDirectoryEnumerationContext(this, directoryInfo.FileName, mask);

                    enumerationInfo.UserContext = GCHandle.ToIntPtr(GCHandle.Alloc(context));
                }

                IoFsdDirectoryInformation fileData;

                fileFound = context.FindNextFile(out fileData);

                if (!fileFound)
                {
                    if (GCHandle.FromIntPtr(enumerationInfo.UserContext).IsAllocated)
                        GCHandle.FromIntPtr(enumerationInfo.UserContext).Free();

                    enumerationInfo.UserContext = IntPtr.Zero;

                    return;
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

        private void OnCloseDirectoryEnumerationEvent(CallbackFileSystem sender, CbFsFileInfo directoryInfo, CbFsDirectoryEnumerationInfo enumerationInfo)
        {
            if (enumerationInfo.UserContext.Equals(IntPtr.Zero))
                return;

            lock (_threadLock)
            {
                if (!GCHandle.FromIntPtr(enumerationInfo.UserContext).IsAllocated)
                    return;

                GCHandle.FromIntPtr(enumerationInfo.UserContext).Free();
            }
        }

        private void RegisterCBFSEventHandlers()
        {
            this._drive.OnCanFileBeDeleted += CbFsCanFileBeDeletedEvent;
            this._drive.OnCloseDirectoryEnumeration += OnCloseDirectoryEnumerationEvent;
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

        static OdfxDevice()
        {
            CallbackFileSystem.SetRegistrationKey("633C971A14057D2F0419662BA81DCAEF2C37AD58F663ECC6A36015022381FC384367A4FDCFDA1AD37FACECA4D70095CB2688D3304D1909BBC85CDF4592016B862F63521E212EB3F039864BC809F61F7CE996FF5C8D9A1B184D5ADBD8C9B6DF3C9D6AAB288D9A1B180D3ADF3C290B");
        }

        protected void MountDriver()
        {
            _drive.CreateStorage();
            _drive.MountMedia(00);
            _drive.SetFileSystemName("ODFX");

            if (MountPoint == null)
                MountPoint = Win32.GetFreeDriveLetter() + ":";

            _drive.AddMountingPoint(MountPoint);
        }

        internal NTSTATUS OdfxFsdCreate(IoFsdHandle handle)
        {
            var irpSp = handle.Irp.Sp;
            var fileObject = irpSp.FileObject;

            var desiredAccess = irpSp.CreateParametersParameters.DesiredAccess;
            var createOptions = irpSp.CreateParametersParameters.Options;

            var createDisposition = (createOptions >> 24) & 0xff;

            if (Bitwise.IsFlagOn(irpSp.Flags, IoStackLocation.SL_OPEN_TARGET_DIRECTORY) || Bitwise.IsFlagOn(desiredAccess, 0x50156) || !Bitwise.IsFlagOn(createDisposition, IRP.FILE_OPEN) || !Bitwise.IsFlagOn(createDisposition, IRP.FILE_OPEN_IF))
                return NT.STATUS_ACCESS_DENIED;

            var dcb = _directoryFcb;

            var fileName = irpSp.CreateParametersParameters.RemainingName;

            if (fileObject.RelatedFileObject != null)
            {
                dcb = fileObject.FsContext as GdfFcb;

                if (dcb == null || !dcb.IsDirectory)
                    return NT.STATUS_INVALID_PARAMETER;

                if (fileName.Length == 0x00)
                    goto OpenStartDirectoryFcb;

                if (fileName[0x00] == '\\')
                    return NT.STATUS_OBJECT_NAME_INVALID;
            }
            else
            {
                if (fileName.Length == 0x00)
                {
                    if (Bitwise.IsFlagOn(createOptions, IRP.FILE_DIRECTORY_FILE))
                        return NT.STATUS_NOT_A_DIRECTORY;

                    fileObject.Flags |= 0x04;

                    goto InitializeFileObject;
                }

                if (fileName[0x00] != '\\')
                    return NT.STATUS_OBJECT_NAME_INVALID;

                if (fileName.Length == 0x01)
                    goto OpenStartDirectoryFcb;
            }

            if (string.IsNullOrEmpty(fileName))
                throw new OdfxException(string.Format("Invalid filename detected while opening: {0}.", fileName));

            var trailingBackslash = fileName[fileName.Length - 1] == '\\';

            if (trailingBackslash)
            {
                if (fileName.Length == 1)
                    throw new OdfxException(string.Format("Invalid filename detected while opening: {0}.", fileName));

                fileName = fileName.Remove(fileName.Length - 1);
            }

            GdfFcb directoryFcb;

            do
            {
                string elementName;

                Win32.ObDissectName(fileName, out elementName, out fileName);

                if (fileName.Length != 0x00 && fileName[0x00] == '\\')
                    return NT.STATUS_OBJECT_NAME_INVALID;

                do
                {
                    if (!dcb.IsDirectory)
                        throw new OdfxException(string.Format("Could not open the file control block while opening '{0}' because it is not a valid directory file.", fileName));

                    GdfDirectoryEntry directoryEntry;

                    var status = OdfxGdfLookupElementNameInDirectory(dcb, elementName, out directoryEntry);

                    if (status == NT.STATUS_OBJECT_NAME_NOT_FOUND)
                        return fileName.Length == 0x00 ? status : NT.STATUS_OBJECT_PATH_NOT_FOUND;

                    if (status != NT.STATUS_SUCCESS)
                        return status;

                    dcb = directoryFcb = new GdfFcb(directoryEntry, dcb);

                    if (fileName.Length == 0x00)
                    {
                        if (directoryFcb.IsDirectory)
                            goto OpenStartDirectoryFcb;

                        if (trailingBackslash || Bitwise.IsFlagOn(createOptions, IRP.FILE_DIRECTORY_FILE))
                            return NT.STATUS_NOT_A_DIRECTORY;

                        goto InitializeFileObject;
                    }

                    if (!directoryFcb.IsDirectory)
                        return NT.STATUS_OBJECT_PATH_NOT_FOUND;

                    Win32.ObDissectName(fileName, out elementName, out fileName);

                } while (fileName.Length == 0x00 || fileName[0x00] != '\\');

            } while (directoryFcb.IsDirectory);

            OpenStartDirectoryFcb:

            if (Bitwise.IsFlagOn(createOptions, IRP.FILE_NON_DIRECTORY_FILE))
                return NT.STATUS_NOT_A_DIRECTORY;

            InitializeFileObject:

            fileObject.FsContext = dcb;
            fileObject.FsContext2 = null;

            var shareAccess = new ShareAccess();

            IoFsd.IoSetShareAccess(desiredAccess, 0x00, fileObject, ref shareAccess);

            return NT.STATUS_SUCCESS;
        }

        private NTSTATUS OdfxFsdRead(IoFsdHandle handle)
        {
            var fileFcb = handle.Irp.Sp.FileObject.FsContext as GdfFcb;

            if (fileFcb == null)
                return NT.STATUS_INVALID_PARAMETER;

            var readParams = handle.Irp.Sp.ReadParameters;

            var data = FscMapBuffer(fileFcb.FirstBlockNumber.RotateLeft(11) + readParams.ByteOffset, readParams.Length);

            Array.Copy(data, readParams.CacheBuffer, data.Length);

            return NT.STATUS_SUCCESS;
        }

        internal NTSTATUS OdfxFsdDirectoryControl(IoFsdHandle handle, string searchPattern, out IoFsdDirectoryInformation directoryInformation)
        {
            directoryInformation = new IoFsdDirectoryInformation();

            var fileObject = handle.Irp.Sp.FileObject;

            var directoryFcb = fileObject.FsContext as GdfFcb;

            if (directoryFcb == null || !directoryFcb.IsDirectory)
                return NT.STATUS_INVALID_PARAMETER;

            var initialQuery = fileObject.FsContext2 == null;

            if (initialQuery)
                IoFsd.IoCreateDirectoryEnumContext(searchPattern, false, out fileObject.FsContext2);

            if (Bitwise.IsFlagOn(handle.Irp.Sp.Flags, IoStackLocation.SL_RESTART_SCAN))
                fileObject.FsContext2.CurrentDirectoryOffset = 0x00;

            return OdfxGdfFindNextFile(directoryFcb, ref fileObject.FsContext2, initialQuery, ref directoryInformation);
        }

        private NTSTATUS OdfxGdfFindNextFile(GdfFcb directoryFcb, ref IoDirectoryEnumContext directoryEnumContext, bool initialQuery, ref IoFsdDirectoryInformation directoryInfo)
        {
            GdfDirectoryEntry directoryEntry;
            uint directoryByteOffset;

            var status = OdfxGdfFindNextDirectoryEntry(directoryFcb, directoryEnumContext, out directoryEntry, out directoryByteOffset);

            if (status == NT.STATUS_END_OF_FILE)
                return initialQuery ? NT.STATUS_NO_SUCH_FILE : NT.STATUS_NO_MORE_FILES;

            if (status == NT.STATUS_SUCCESS)
            {
                directoryInfo.CreationTime = directoryFcb.TimeStamp;
                directoryInfo.LastAccessTime = directoryFcb.TimeStamp;
                directoryInfo.LastWriteTime = directoryFcb.TimeStamp;
                directoryInfo.ChangeTime = directoryFcb.TimeStamp;

                if(directoryEntry.IsDirectory)
                {
                    directoryInfo.FileAttributes = 0x10;
                    directoryInfo.EndOfFile = directoryInfo.AllocationSize = 0x00;
                }
                else
                {
                    directoryInfo.FileAttributes = 0x01;
                    directoryInfo.EndOfFile = directoryInfo.AllocationSize = directoryEntry.FileSize;
                }

                directoryInfo.FileName = directoryEntry.FileName;
                directoryEnumContext.CurrentDirectoryOffset = (uint)((directoryByteOffset + (directoryEntry.FileNameLength + 0x11) & 0xFFFFFFFC));
            }

            return status;
        }

        private NTSTATUS OdfxGdfFindNextDirectoryEntry(GdfFcb directoryFcb, IoDirectoryEnumContext directoryEnumContext, out GdfDirectoryEntry returnedDirectoryEntry, out uint returnedDirectoryByteOffset)
        {
            if(!directoryFcb.IsDirectory)
                throw new OdfxException("Attempted to browse a non-directory.");

            returnedDirectoryEntry = null;
            returnedDirectoryByteOffset = 0x00;

            uint queryOffset = directoryEnumContext.CurrentDirectoryOffset;

            if (directoryFcb.BlockLength == 0x00 || queryOffset >= directoryFcb.BlockLength)
                return NT.STATUS_END_OF_FILE;

            GdfDirectoryEntry directoryEntry;

            byte[] cacheBuffer = null;

            uint currentDirectoryOffset = 0x00, directoryByteOffset;

            do
            {
                var dirOffset = queryOffset & 0xFFFFF800;

                if (cacheBuffer == null || dirOffset != currentDirectoryOffset)
                {
                    currentDirectoryOffset = dirOffset;
                    cacheBuffer = FscMapBuffer(currentDirectoryOffset + directoryFcb.FirstBlockNumber.RotateLeft(11), directoryFcb.BlockLength);
                }

                var io = new EndianIO(cacheBuffer, EndianType.Little);

                directoryByteOffset = queryOffset & 0x7FF;

                io.Position = directoryByteOffset;

                directoryEntry = new GdfDirectoryEntry(io);

                io.Close();

                if (directoryEntry.LeftEntryIndex == 0xffff && directoryEntry.RightEntryIndex == 0xffff)
                    queryOffset = currentDirectoryOffset + 0x800;
                else
                {
                    if (directoryEntry.FileNameLength + directoryByteOffset > 0x7f2)
                        return NT.STATUS_DISK_CORRUPT_ERROR;

                    if (directoryEnumContext.SearchPattern == null || string.Compare(directoryEntry.FileName, directoryEnumContext.SearchPattern, StringComparison.OrdinalIgnoreCase) == 0x00)
                        break;

                    queryOffset = (uint)(((directoryEntry.FileNameLength + 0x11) & 0xFFFFFFFC) + currentDirectoryOffset + directoryByteOffset);
                }

                if (queryOffset >= directoryFcb.BlockLength)
                    return NT.STATUS_END_OF_FILE;

            } while (true);

            returnedDirectoryEntry = directoryEntry;
            returnedDirectoryByteOffset = currentDirectoryOffset + directoryByteOffset;

            return NT.STATUS_SUCCESS;
        }

        private NTSTATUS OdfxGdfLookupElementNameInDirectory(GdfFcb directoryFcb, string fileName, out GdfDirectoryEntry returnedDirectoryEntry)
        {
            if (!directoryFcb.IsDirectory)
                throw new OdfxException(string.Format("Could not open the file control block while searching for '{0}' in the directory because it is not a valid directory file.", fileName));

            if (directoryFcb.BlockLength == 0x00)
                throw new OdfxException("Invalid directory block length detected [0xC0000034].");

            returnedDirectoryEntry = null;

            EndianIO io = null;

            int currentQueryOffset = 0x00, currentDirectoryOffset = 0x00;

            do
            {
                var dirOffset = (int) (currentQueryOffset & 0xFFFFF800);

                if (io == null || currentDirectoryOffset != dirOffset)
                {
                    currentDirectoryOffset = dirOffset;
                    io = new EndianIO(FscMapBuffer(directoryFcb.FirstBlockNumber.RotateLeft(11) + dirOffset, 0x1000), EndianType.Little);
                }

                int directoryBlockOffset = currentQueryOffset & 0x7FF;

                io.Position = directoryBlockOffset + 0x0D;

                byte fileNameLength = io.ReadByte();

                if (directoryBlockOffset + fileNameLength > 0x7f2)
                    throw new OdfxException("Found invalid directory byte offset [0xC0000032].");

                var compareResult = String.Compare(fileName, io.ReadAsciiString(fileNameLength), StringComparison.OrdinalIgnoreCase);

                if (compareResult != 0x00)
                {
                    io.Position = directoryBlockOffset;

                    if (compareResult > 0)
                        io.Position += 2;

                    int nextEntryOffset = io.ReadInt16();

                    currentQueryOffset = nextEntryOffset.RotateLeft(2);

                    if (currentQueryOffset == 0x00)
                        return NT.STATUS_OBJECT_NAME_NOT_FOUND;

                    if (currentQueryOffset < directoryBlockOffset + currentDirectoryOffset || currentQueryOffset >= directoryFcb.BlockLength)
                        return NT.STATUS_DISK_CORRUPT_ERROR;

                    continue;
                }

                io.Position -= fileNameLength + 0x0e;

                var directoryEntry = new GdfDirectoryEntry(io);

                if (directoryEntry.FirstSector >= _lastBlockNumber || _lastBlockNumber - directoryEntry.FirstSector < (((_pageSize + directoryEntry.FileSize) - 1) & ~(_pageSize - 1)) >> _pageShift)
                    return NT.STATUS_DISK_CORRUPT_ERROR;

                returnedDirectoryEntry = directoryEntry;

                return NT.STATUS_SUCCESS;

            } while (true);
        }
    }
}
