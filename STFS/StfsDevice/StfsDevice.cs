using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CallbackFS;
using NoDev.Common;
using NoDev.Common.IO;
using NoDev.Xbox360;

namespace NoDev.StfsV3
{
    using ACCESS_MASK = UInt32;
    using NTSTATUS = UInt32;
    using Irp = IRP;

    public class StfsDevice
    {
        //private static readonly string MGuid = "44D14AF2-249C-408D-ACD3-A0BA7C8E1E11";
        private readonly object _threadLocker = new object();
        [Obfuscation]
        public bool IsModified { get; private set; }

        private readonly CallbackFileSystem _drive;
        private string _volumeLabel;

        private readonly StfsVolumeDescriptor _volumeDescriptor;
        [Obfuscation]
        public StfsVolumeExtension VolumeExtension;
        private StfsFcb _directoryFcb;
        private List<StfsFcb> _fcbs;

        private readonly EndianIO _io;

        private const uint Block = 0x1000; // STFS block length

        public StfsDevice(string mountingPoint, EndianIO io, StfsCreatePacket createPacket, string volumeLabel)
        {
            this._io = io;

            // STFS Device Initialization
            VolumeExtension = new StfsVolumeExtension(createPacket);
            _volumeDescriptor = createPacket.VolumeDescriptor;

            SetVolumeLabel(volumeLabel.Length > 25 ? volumeLabel.Substring(0, 25) : volumeLabel);

            var drive = new CallbackFileSystem();

            this._drive = drive;

            this.RegisterCBFSEventHandlers();
            CallbackFileSystem.Initialize("44D14AF2-249C-408D-ACD3-A0BA7C8E1E11");

            drive.ParallelProcessingAllowed = true;
            drive.SerializeCallbacks = false;

            drive.MaxReadWriteBlockSize = Block;
            //drive.DisableFileCache(true); //CBFS automatically caches the directory listings :)
            drive.CreateStorage();
            drive.MountMedia(0);
            drive.SetFileSystemName("STFS");
            drive.AddMountingPoint(mountingPoint ?? Win32.GetFreeDriveLetter() + ":");
        }

        [Obfuscation]
        public void Unmount()
        {
            _drive.DeleteMountingPoint(0x00);
            this._drive.UnmountMedia(true);
            this._drive.DeleteStorage(true);
        }

        // This notification event is fired after Callback File System unmounts the storage.
        private void CbFsUnmountEvent(CallbackFileSystem sender)
        {

        }

        // This notification event is fired when the storage is removed by the user using Eject command in Explorer.
        private void CbFsStorageEjectedEvent(CallbackFileSystem sender)
        {
            this._drive.UnmountMedia(true);
            this._drive.DeleteStorage(true);
        }

        // The event is fired when the OS needs to get the volume label.
        private void CbFsGetVolumeLabelEvent(CallbackFileSystem sender, ref string volumeLabel)
        {
            volumeLabel = _volumeLabel;
        }

        // The event is fired when the OS wants to change the volume label. 
        private void CbFsSetVolumeLabelEvent(CallbackFileSystem sender, string volumeLabel)
        {
            SetVolumeLabel(volumeLabel);
        }

        // This notification event is fired after Callback File System mounts the storage.
        private void CbFsMountEvent(CallbackFileSystem sender)
        {
            this.StfsCreateDevice();
        }

        // The event is fired when the OS needs to get information about the storage (disk) size and free space.
        private void CbFsGetVolumeSizeEvent(CallbackFileSystem sender, ref long totalAllocationUnits, ref long availableAllocationUnits)
        {
            lock (_threadLocker)
            {
                // Sector Size = 0x200
                if (VolumeExtension.ReadOnly)
                {
                    totalAllocationUnits = (VolumeExtension.NumberOfTotalBlocks * Block) / 0x200;
                    availableAllocationUnits = 0x00;
                }
                else
                {
                    totalAllocationUnits = 0x400000; // 2 GBs
                    availableAllocationUnits = (0x80000000 - ((VolumeExtension.NumberOfTotalBlocks - (VolumeExtension.NumberOfFreeBlocks 
                        + VolumeExtension.NumberOfFreePendingBlocks)) * Block)) / 0x200;
                }
            }
        }


        private uint _volumeId;

        // The event is fired when Callback File System needs to get the volume ID.
        private void CbFsGetVolumeIdEvent(CallbackFileSystem sender, ref uint volumeID)
        {
            if (_volumeId == 0)
                _volumeId = (uint)new Random().Next(1, int.MaxValue);
            volumeID = _volumeId;
        }

        // The event is fired when translation of the File ID to file path must be done.
        private void CbFsGetFileNameByFileIdEvent(CallbackFileSystem sender, long fileId,
                                                 ref string filePath, ref ushort filePathLength)
        {
        }

        // The event is fired when the OS needs to check if the directory is empty.
        private void CbFsIsDirectoryEmptyEvent(CallbackFileSystem sender, CbFsFileInfo directoryInfo, string fileName, ref bool isEmpty)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            lock (_threadLocker)
            {
                var handle = new IoFsdHandle();
                handle.Sp.CreateParametersParameters = new IrpCreateParameters()
                {
                    DesiredAccess = 0x100001,
                    Options = 0x4021,
                    FileAttributes = 0x40,
                    ShareAccess = 0x03,
                    RemainingName = fileName
                };
                IoFsdDirectoryInformation directoryInformation;
                StfsFsdCreate(handle);
                isEmpty = StfsFsdDirectoryControl(handle, "*", out directoryInformation) != 0x00;
                StfsFsdClose(handle);
            }
        }

        // The event is fired when the OS needs to get information about the file or directory.
        private void CbFsGetFileInfoEvent(CallbackFileSystem sender, string fileName, ref bool fileExists,
            ref DateTime creationTime, ref DateTime lastAccessTime, ref DateTime lastWriteTime,
            ref long endOfFile, ref long allocationSize, ref CBFS_LARGE_INTEGER fileId,
            ref uint fileAttributes, ref string shortFileName, ref string realFileName)
        {
            lock (_threadLocker)
            {
                if (string.IsNullOrWhiteSpace(fileName)) return;
                var handle = new IoFsdHandle();
                var searchSeperator = fileName.LastIndexOf('\\');
                var name = fileName.Substring(0x00, searchSeperator + 1);
                var search = fileName.Substring(searchSeperator + 1, fileName.Length - (searchSeperator + 1));
                handle.Sp.CreateParametersParameters = new IrpCreateParameters()
                {
                    DesiredAccess = 0x100001,
                    Options = 0x1004021,
                    FileAttributes = 0x40,
                    ShareAccess = 0x03,
                    RemainingName = name
                };
                if (StfsFsdCreate(handle) != 0x00)
                    return;
                IoFsdDirectoryInformation directoryInformation;
                fileExists = StfsFsdDirectoryControl(handle, search, out directoryInformation) == 0x00;
                if (fileExists)
                {
                    creationTime = directoryInformation.CreationTime;
                    lastAccessTime = directoryInformation.LastAccessTime;
                    lastWriteTime = directoryInformation.LastWriteTime;
                    endOfFile = directoryInformation.EndOfFile;
                    allocationSize = directoryInformation.AllocationSize;
                    fileAttributes = directoryInformation.FileAttributes;
                    fileId.QuadPart = 0x00;
                }
            }
        }

        // The event is fired when the OS needs to enumerate directory contents.
        private void CbFsEnumerateDirectoryEvent(CallbackFileSystem sender, CbFsFileInfo directoryInfo,
            CbFsHandleInfo handleInfo, CbFsDirectoryEnumerationInfo enumerationInfo, string mask, int index, bool restart, ref bool fileFound,
            ref string fileName, ref string shortFileName,
            ref DateTime creationTime, ref DateTime lastAccessTime,
            ref DateTime lastWriteTime, ref long endOfFile, ref long allocationSize,
            ref CBFS_LARGE_INTEGER fileId, ref uint fileAttributes)
        {
            lock (_threadLocker)
            {

                StfsDirectoryEnumerationContext context = null;
                if (restart && (enumerationInfo.UserContext != IntPtr.Zero))
                {
                    if (GCHandle.FromIntPtr(enumerationInfo.UserContext).IsAllocated)
                    {
                        GCHandle.FromIntPtr(enumerationInfo.UserContext).Free();
                    }
                    enumerationInfo.UserContext = IntPtr.Zero;
                }

                if (enumerationInfo.UserContext.Equals(IntPtr.Zero))
                {
                    context = new StfsDirectoryEnumerationContext(this, directoryInfo.FileName, mask);

                    GCHandle gch = GCHandle.Alloc(context);

                    enumerationInfo.UserContext = GCHandle.ToIntPtr(gch);
                }
                else
                {
                    context = (StfsDirectoryEnumerationContext)GCHandle.FromIntPtr(enumerationInfo.UserContext).Target;
                }

                IoFsdDirectoryInformation fileData;
                fileFound = context.FindNextFile(out fileData);
                if (fileFound)
                {
                    fileName = fileData.FileName;
                    creationTime = fileData.CreationTime;
                    lastAccessTime = fileData.LastAccessTime;
                    lastWriteTime = fileData.LastWriteTime;
                    fileAttributes = fileData.FileAttributes;
                    endOfFile = fileData.EndOfFile;
                    allocationSize = fileData.AllocationSize;
                    fileId.QuadPart = 0x00;
                }
                else
                {
                    if (GCHandle.FromIntPtr(enumerationInfo.UserContext).IsAllocated)
                    {
                        GCHandle.FromIntPtr(enumerationInfo.UserContext).Free();
                    }
                    enumerationInfo.UserContext = IntPtr.Zero;
                }
            }
        }

        // The event is fired when the OS has finished enumerating contents of the directory.
        private void OnCloseDirectoryEnumerationEvent(CallbackFileSystem sender, CbFsFileInfo directoryInfo, CbFsDirectoryEnumerationInfo enumerationInfo)
        {
            if (enumerationInfo.UserContext.Equals(IntPtr.Zero)) return;
            lock (_threadLocker)
            {
                if (!GCHandle.FromIntPtr(enumerationInfo.UserContext).IsAllocated)
                    return;
                // close the STFS directory enumeration handle
                var enumContext = (StfsDirectoryEnumerationContext) GCHandle.FromIntPtr(enumerationInfo.UserContext).Target;
                enumContext.Close();
                GCHandle.FromIntPtr(enumerationInfo.UserContext).Free();
            }
        }

        // This event is fired when the OS needs to flush the data of the open file or volume.
        private void CbFsFlushFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo)
        {
            if (fileInfo == null || fileInfo.UserContext == IntPtr.Zero) return;
            lock (_threadLocker)
            {
                var ioHandle = (IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target;
                StfsFsdFlushBuffers(ioHandle);
            }
        }

        // This event is fired when the OS needs to delete the file or directory.
        private void CbFsDeleteFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo)
        {
            lock (_threadLocker)
            {
                var handle = new IoFsdHandle();
                var irpSp = handle.Sp;
                irpSp.CreateParametersParameters = new IrpCreateParameters()
                {
                    DesiredAccess = 0x10000,
                    Options = 0x1004040,
                    FileAttributes = 0x00,
                    ShareAccess = 0x07,
                    RemainingName = fileInfo.FileName
                };
                NTSTATUS status = StfsFsdCreate(handle);
                if (status == 0x00)
                {
                    handle.Irp.UserBuffer = new byte[] { 0x01 };
                    irpSp.SetFileParameters = new IrpParametersSetFile()
                    {
                        FileInformationClass = FileInformationClass.FileDispositionInformation,
                        FileObject = irpSp.FileObject,
                        Length = (uint)handle.Irp.UserBuffer.Length
                    };
                    StfsFsdSetInformation(handle);
                    StfsFsdCleanup(handle);
                    StfsFsdClose(handle);
                    IsModified = true;
                }
                handle.Fcb = null;
                handle.Sp = null;
            }
        }

        // This event is fired when the OS handles file or directory creation event.
        private void CbFsCreateFileEvent(CallbackFileSystem sender, string fileName, ACCESS_MASK desiredAccess, uint fileAttributes, uint shareMode, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo)
        {
            lock (_threadLocker)
            {
                var handle = new IoFsdHandle
                {
                    Sp =
                    {
                        CreateParametersParameters =
                            (fileAttributes & (uint)CbFsFileAttributes.CBFS_FILE_ATTRIBUTE_DIRECTORY) == 0x00
                                ? new IrpCreateParameters
                                {
                                    DesiredAccess = desiredAccess,
                                    Options = 0x5000060,
                                    FileAttributes = 0x80,
                                    ShareAccess = 0x00,
                                    RemainingName = fileName
                                }
                                : new IrpCreateParameters
                                {
                                    DesiredAccess = 0x100001,
                                    Options = 0x2004021,
                                    FileAttributes = 0x10,
                                    ShareAccess = 0x03,
                                    RemainingName = fileName
                                }
                    }
                };
                if (StfsFsdCreate(handle) == NT.STATUS_SUCCESS)
                {
                    IsModified = true;
                    fileInfo.UserContext = GCHandle.ToIntPtr(GCHandle.Alloc(handle));
                }
                else
                {
                    handle.Fcb = null;
                    handle.Sp = null;
                }
            }
        }

        // This event is fired when the OS handles file or directory open event.
        private void CbFsOpenFileEvent(CallbackFileSystem sender, string fileName, ACCESS_MASK desiredAccess, uint fileAttributes, uint shareMode, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo)
        {
            IoFsdHandle handle;
            if (fileInfo.UserContext != IntPtr.Zero)
            {
                //handle = (IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target;
                return;
            }
            lock (_threadLocker)
            {
                handle = new IoFsdHandle
                {
                    Sp =
                    {
                        CreateParametersParameters = new IrpCreateParameters()
                        {
                            DesiredAccess = desiredAccess,
                            Options = 0x1000060,
                            FileAttributes = 0x80,
                            ShareAccess = 0x01,
                            RemainingName = fileName
                        }
                    }
                };
                if (StfsFsdCreate(handle) != NT.STATUS_SUCCESS)
                    return;
                fileInfo.UserContext = GCHandle.ToIntPtr(GCHandle.Alloc(handle));
            }
        }

        // This event is fired when the OS needs to close the file.
        private void CbFsCloseFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo)
        {
            if (fileInfo.UserContext == IntPtr.Zero) return;
            lock (_threadLocker)
            {
                var ioHandle = (IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target;

                StfsFsdCleanup(ioHandle);
                StfsFsdClose(ioHandle);

                GCHandle.FromIntPtr(fileInfo.UserContext).Free();
                handleInfo.UserContext = IntPtr.Zero;
            }
        }

        // This event is fired when the OS needs to read the data from the open file or volume.
        private void CbFsReadFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, long position, byte[] buffer, int bytesToRead, ref int bytesRead)
        {
            if (fileInfo.UserContext.Equals(IntPtr.Zero)) return;
            lock (_threadLocker)
            {
                var ioHandle = (IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target;
                var ctx = (StfsFcb)ioHandle.Fcb;
                if (bytesToRead > 0x00 && !ctx.IsDirectory)
                {
                    var io = new EndianIO(buffer, EndianType.Big);
                    var endOfReading = position + bytesToRead;
                    int fileRemainder = (int)(ctx.Filesize - position);
                    if (bytesToRead > fileRemainder)
                        bytesToRead = fileRemainder;

                    if (endOfReading > ctx.ValidAllocBlocks)
                    {
                        StfsFullyCachedRead(ctx, (uint)position, (uint)bytesToRead, io);
                    }
                    else
                    {
                        bool shouldReadFullyCached = StfsTestForFullyCachedIo(ctx, position, bytesToRead);
                        if (shouldReadFullyCached)
                        {
                            StfsFullyCachedRead(ctx, (uint)position, (uint)bytesToRead, io);
                        }
                        else
                        {
                            StfsPartiallyCachedRead(ctx, (uint)position, (uint)bytesToRead, io);
                        }
                    }
                    bytesRead = bytesToRead;
                    io.Flush();
                }
            }
        }

        // This event is fired when the OS needs to write the data to the open file or volume.
        private void CbFsWriteFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, long position, byte[] buffer, int bytesToWrite,
            ref int bytesWritten)
        {
            if (fileInfo.UserContext.Equals(IntPtr.Zero)) return;
            lock (_threadLocker)
            {
                var ioHandle = (IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target;
                var fcb = (StfsFcb)ioHandle.Fcb;

                if (StfsEnsureWriteableDirectoryEntry(fcb) != 0x00) return;
                var endOfWrite = position + bytesToWrite;
                var bEndOfWrite = (endOfWrite + 0xFFF) & 0xFFFFF000;

                if (bEndOfWrite > position)
                {
                    StfsEnsureWriteableBlocksAvailable(fcb, (uint)bytesToWrite, (uint)position);
                }

                if (endOfWrite > fcb.AllocationBlocks)
                {
                    StfsSetAllocationSize(fcb, (uint)bEndOfWrite, false);
                }

                var highOffset = position & 0xFFFFF000;
                while (fcb.ValidAllocBlocks < highOffset)
                {
                    int hashBlockCacheIndex = -1, datablockCacheIndex = -1;
                    StfsHashBlock hashBlock = null;
                    byte[] dataBlock = new byte[0x1000];
                    uint returnedBlockNum = 0xffffffff;

                    StfsBeginDataBlockUpdate(fcb, ref hashBlock, ref hashBlockCacheIndex, fcb.ValidAllocBlocks,
                                             ref dataBlock, ref datablockCacheIndex, true, ref returnedBlockNum);
                    StfsEndDataBlockUpdate(dataBlock, datablockCacheIndex, returnedBlockNum, ref hashBlock,
                                           hashBlockCacheIndex);

                    fcb.ValidAllocBlocks += 0x1000;
                }

                var doFullyCachedWrite = StfsTestForFullyCachedIo(fcb, position, bytesToWrite);
                if (doFullyCachedWrite)
                {
                    StfsFullyCachedWrite(fcb, (uint)position, (uint)bytesToWrite, buffer);
                }
                else
                {
                    StfsPartiallyCachedWrite(fcb, (uint)position, (uint)bytesToWrite, buffer);
                }

                if (fcb.Filesize < endOfWrite)
                    fcb.Filesize = (uint)endOfWrite;

                fcb.State |= 0x10;

                IsModified = true;

                bytesWritten = bytesToWrite;
            }
        }

        // This event is fired when the OS or the application needs to change the attributes or times of the open file.
        private void CbFsSetFileAttributesEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, CbFsHandleInfo handleInfo,
            DateTime creationTime, DateTime lastAccessTime, DateTime lastWriteTime, uint attributes)
        {
            if (attributes == 0x00) return;
            if (fileInfo.UserContext.Equals(IntPtr.Zero)) return;
            lock (_threadLocker)
            {
                var handle = (IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target;
                if (handle.Fcb == null)
                    return;

                var infoWriter = new EndianIO(new MemoryStream(), EndianType.Big);
                infoWriter.Write(creationTime.ToBinary());
                infoWriter.Write(lastAccessTime.ToBinary());
                infoWriter.Write(lastWriteTime.ToBinary());
                infoWriter.Write(lastWriteTime.ToBinary());
                infoWriter.Write(attributes);
                infoWriter.Flush();

                handle.Sp.SetFileParameters = new IrpParametersSetFile()
                {
                    FileInformationClass = FileInformationClass.FileBasicInformation,
                    FileObject = null,
                    Length = (uint)infoWriter.Length
                };
                handle.Irp.UserBuffer = infoWriter.ToArray();
                infoWriter.Close();
                StfsFsdSetInformation(handle);
            }
        }

        // This event is fired when the OS or the application needs to change the size of the open file.
        private void CbFsSetEndOfFileEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, long endOfFile)
        {
            if (fileInfo.UserContext.Equals(IntPtr.Zero)) return;
            lock (_threadLocker)
            {
                var handle = (IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target;
                handle.Irp.UserBuffer = new byte[0x08];
                handle.Irp.UserBuffer.WriteInt64(0x00, endOfFile);
                handle.Sp.SetFileParameters = new IrpParametersSetFile
                {
                    FileInformationClass =
                        FileInformationClass.FileEndOfFileInformation,
                    FileObject = null,
                    Length = (uint)handle.Irp.UserBuffer.Length
                };
                StfsFsdSetInformation(handle);
            }
        }

        // This event is fired when the OS or the application needs to set the allocation size of the file.
        private void CbFsSetAllocationSizeEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, long allocationSize)
        {
            if (fileInfo.UserContext.Equals(IntPtr.Zero)) return;
            lock (_threadLocker)
            {
                var handle = (IoFsdHandle)GCHandle.FromIntPtr(fileInfo.UserContext).Target;
                handle.Irp.UserBuffer = new byte[0x08];
                handle.Irp.UserBuffer.WriteInt64(0x00, allocationSize);
                handle.Sp.SetFileParameters = new IrpParametersSetFile()
                {
                    FileInformationClass =
                        FileInformationClass.FileAllocationInformation,
                    FileObject = null,
                    Length = (uint)handle.Irp.UserBuffer.Length
                };
                StfsFsdSetInformation(handle);
            }
        }

        // This event is fired when the OS needs to rename or move the file within a file system.
        private void CbFsRenameOrMoveEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo,
                                          string newFileName)
        {
            lock (_threadLocker)
            {
                var handle = new IoFsdHandle();
                var irpSp = handle.Sp;

                // First we open the file we are moving/renaming
                irpSp.CreateParametersParameters = new IrpCreateParameters()
                {
                    DesiredAccess = 0x110000,
                    Options = 0x1004020,
                    FileAttributes = 0x00,
                    ShareAccess = 0x07,
                    RemainingName = fileInfo.FileName
                };
                // open the old file
                if (StfsFsdCreate(handle) != NT.STATUS_SUCCESS)
                    return;

                var directoryHandle = new IoFsdHandle();
                var dirIrpSp = directoryHandle.Sp;

                // find and open the directory we are moving the file to
                dirIrpSp.CreateParametersParameters = new IrpCreateParameters()
                {
                    DesiredAccess = 0x100002,
                    Options = 0x1004000,
                    FileAttributes = 0x00,
                    ShareAccess = 0x03,
                    RemainingName = newFileName
                };
                dirIrpSp.Flags = 0x04;
                // open the new directory for writing
                if (StfsFsdCreate(directoryHandle) != NT.STATUS_SUCCESS)
                    return;

                // Fill in the FileRenameInformationBuffer
                var infoWriter = new EndianIO(new MemoryStream(), EndianType.Big);

                infoWriter.Write(0x00); // Set ReplaceIfExists to false
                infoWriter.Write(-3); // Set RootDirectory to null
                infoWriter.Write((ushort)newFileName.Length); // write filename length
                infoWriter.Write((ushort)(newFileName.Length + 1)); // write filename max length
                infoWriter.WriteAsciiString(newFileName); // Fill in the new filename
                infoWriter.Flush();

                handle.Irp.UserBuffer = infoWriter.ToArray();
                irpSp.SetFileParameters = new IrpParametersSetFile()
                {
                    FileInformationClass = FileInformationClass.FileRenameInformation,
                    FileObject = irpSp.FileObject,
                    Length = (uint)handle.Irp.UserBuffer.Length
                };
                infoWriter.Close();
                // copy the file object that we opened for the target directory
                irpSp.FileObject = dirIrpSp.FileObject;
                // Set the file information
                StfsFsdSetInformation(handle);
            }
        }

        // This event is fired when the OS needs to query the possibility to delete the file or directory.
        private void CbFsCanFileBeDeletedEvent(CallbackFileSystem sender, CbFsFileInfo fileInfo, CbFsHandleInfo HandleInfo, ref bool canBeDeleted)
        {
            canBeDeleted = !VolumeExtension.ReadOnly;
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
            this._drive.OnGetVolumeId += CbFsGetVolumeIdEvent;
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

        static StfsDevice()
        {
            CallbackFileSystem.SetRegistrationKey(
                "633C971A14057D2F0419662BA81DCAEF2C37AD58F663ECC6A36015022381FC384367A4FDCFDA1AD37FACECA4D70095CB2688D3304D1909BBC85CDF4592016B862F63521E212EB3F039864BC809F61F7CE996FF5C8D9A1B184D5ADBD8C9B6DF3C9D6AAB288D9A1B180D3ADF3C290B");
        }

        [Conditional("DEBUG")]
        public static void InstallDriver()
        {
            uint i = 0; // Does not require restart.
            CallbackFileSystem.Install(@"C:\Program Files (x86)\EldoS\Callback File System\Drivers\cbfs.cab",
                                       "44D14AF2-249C-408D-ACD3-A0BA7C8E1E11", null, false, 0x00020001, ref i);
        }

        // Start of STFS functions
        /// <summary>
        /// Calculate the position of the requested block inside the STFS file.
        /// </summary>
        /// <param name="BlockNum">The requested, 0-based index block number.</param>
        /// <param name="ActiveIndex">For hash blocks only. Specifies currently active index. Set to 0 for data blocks.</param>
        /// <returns>The position of the requested block inside the STFS file. </returns>
        public static long GetBlockOffset(uint BlockNum, int ActiveIndex)
        {
            return (BlockNum + ActiveIndex) << 12;
        }

        // File System Driver Functions
        private void StfsCreateDevice()
        {
            this._directoryFcb = new StfsFcb
            {
                FirstBlockNumber = _volumeDescriptor.DirectoryFirstBlockNumber,
                Filesize = 0xfffe,
                BlockPosition = 0xfffffff,
                ContiguousBytesRead = 0x0000,
                LastUnContiguousBlockNum = _volumeDescriptor.DirectoryFirstBlockNumber,
                AllocationBlocks = (uint)(_volumeDescriptor.DirectoryAllocationBlocks * 0x1000),
                ValidAllocBlocks = (uint)(_volumeDescriptor.DirectoryAllocationBlocks * 0x1000),
                LastBlockNumber = 0xffffffff,
                DirectoryEntryIndex = 0xffff,
                State = 0x06,
                Referenced = 0x01
            };

            // for the Root Directory FCB, the creation timestamp and last write timestamp are also overwritten with the ParentDcbLinks
            _directoryFcb.CloneFcb = _directoryFcb;
            _directoryFcb.ParentFcb = _directoryFcb;

            _directoryFcb.ParentDcbQueue.InitializeListHead(_directoryFcb);

            this.VolumeExtension.DirectoryAllocationBlockCount = this._volumeDescriptor.DirectoryAllocationBlocks;
            this.VolumeExtension.NumberOfFreeBlocks = this._volumeDescriptor.NumberOfFreeBlocks;
            this.VolumeExtension.NumberOfTotalBlocks = this._volumeDescriptor.NumberOfTotalBlocks;

            this._fcbs = new List<StfsFcb>();
            //_directoryEntries = new List<StfsDirectoryEntry>();

            StfsResetBlockCache();
            //StfsParseDirectoryListing();
        }

        internal uint StfsFsdCreate(IoFsdHandle fsdHandle)
        {
            IoStackLocation irpSp;
            IRP Irp;
            StfsDirectoryEntry directoryEntry = new StfsDirectoryEntry();
            StfsFcb fsdDirectoryFcb;
            StfsFcb directoryFcb;
            string currentName = string.Empty;
            string fileName;
            bool trailingBackslash = false;
            //bool createDirectory;
            //bool directoryFile;

            uint errorCode = 0x00;
            uint createDisposition;

            Irp = fsdHandle.Irp;
            irpSp = fsdHandle.Sp;

            directoryFcb = (StfsFcb)irpSp.FileObject.FsContext;

            //directoryFile = Bitwise.IsFlagOn(irpSp.CreateParametersParameters.Options, IRP.FILE_DIRECTORY_FILE);

            createDisposition = irpSp.CreateParametersParameters.Options >> 24;
            fileName = irpSp.CreateParametersParameters.RemainingName;


            if (directoryFcb != null)
            {
                if (!directoryFcb.IsDirectory)
                    throw new StfsException("attemped to lookup a file inside a non-directory [0xC000000D].");
                fsdDirectoryFcb = StfsReferenceFcb(directoryFcb);
            }
            else
            {
                fsdDirectoryFcb = StfsReferenceFcb(this._directoryFcb);
            }
            if (fileName[0x00] != '\\')
            {
                errorCode = 0xC0000033;
                goto STFS_EXIT_FSD_CREATE;
            }

            if (fileName.Length == 0x01)
            {
                goto STFS_FINISH_FSD_CREATE;
            }

            if (string.IsNullOrEmpty(fileName))
                throw new StfsException(string.Format("invalid filename detected while opening: {0}", fileName));

            if ((fileName.Length != 0x00) && fileName[fileName.Length - 1] == '\\')
            {
                fileName = fileName.Remove(fileName.Length - 1, 1);
                trailingBackslash = true;
            }

            do
            {
                ObDissectName(fileName, out currentName, out fileName);
                if (fileName.Length != 0x00 && fileName[0x00] == '\\')
                {
                    errorCode = 0xC0000033;
                    goto STFS_EXIT_FSD_CREATE;
                }

                directoryFcb = StfsFindOpenChildFcb(fsdDirectoryFcb, currentName);
                if (directoryFcb != null)
                {
                    directoryFcb = StfsReferenceFcb(directoryFcb);
                    if (directoryFcb == null)
                        throw new StfsException(
                            "detected an invalid FCB while referencing a directory FCB [0xC000009A].");

                    StfsDereferenceFcb(fsdDirectoryFcb);
                    fsdDirectoryFcb = directoryFcb;
                    if (fileName.Length == 0x00)
                    {
                        if (directoryFcb.CloneFcb == null)
                            goto STFS_FINISH_FSD_CREATE;
                    }
                }
                else
                {
                    do
                    {
                        if (!StfsIsValidFileName(currentName))
                        {
                            errorCode = 0xC0000033;
                            goto STFS_EXIT_FSD_CREATE;
                        }

                        if ((errorCode = StfsLookupElementNameInDirectory(fsdDirectoryFcb, currentName, null, out directoryEntry)) != 0x00)
                        {
                            if ((errorCode == 0xC0000034))
                            {
                                if (fileName.Length != 0x00)
                                    errorCode = 0xC000003A;
                                if (fsdDirectoryFcb.DeleteOnClose)
                                    errorCode = 0xC0000056;

                                // means we have to create the directory/file
                                if ((fsdHandle.Sp.Flags & IoStackLocation.SL_OPEN_TARGET_DIRECTORY) == 0x00)
                                {
                                    goto STFS_CREATE_NEW_FILE;
                                }
                                goto STFS_OPEN_TARGET_DIRECTORY;
                            }

                        }
                        if (fileName.Length != 0x00 && !directoryEntry.IsDirectory)
                            errorCode = 0xC000003A;

                        directoryFcb = StfsCreateFcb(directoryEntry, fsdDirectoryFcb);
                        StfsDereferenceFcb(fsdDirectoryFcb);
                        fsdDirectoryFcb = directoryFcb;
                        if (fileName.Length == 0x00)
                        {
                            if ((fsdHandle.Sp.Flags & IoStackLocation.SL_OPEN_TARGET_DIRECTORY) != 0x00)
                            {
                                goto STFS_OPEN_TARGET_DIRECTORY;
                            }
                            goto STFS_FINISH_FSD_CREATE;
                        }
                        ObDissectName(fileName, out currentName, out fileName);
                    } while (fileName.Length == 0x00 || (fileName[0x00] != '\\'));
                }
            } while (directoryFcb.IsDirectory);

        STFS_OPEN_TARGET_DIRECTORY:
            StfsOpenTargetDirectory(fsdHandle, fsdDirectoryFcb);
            goto STFS_EXIT_FSD_CREATE;
        STFS_CREATE_NEW_FILE:
            if (createDisposition == IRP.FILE_OPEN || createDisposition == IRP.FILE_OVERWRITE)
                goto STFS_EXIT_FSD_CREATE;

            if (trailingBackslash && ((irpSp.Flags & IRP.FILE_NON_DIRECTORY_FILE) == 0x00))
            {
                errorCode = 0xC0000033;
                goto STFS_EXIT_FSD_CREATE;
            }

            StfsFcb fcbFileObject;
            if (StfsCreateNewFile(fsdDirectoryFcb, currentName, irpSp.CreateParametersParameters.Options,
                              Irp.Overlay.AllocationSize.LowPart, ref directoryEntry, out fcbFileObject) == 0x00)
            {
                StfsDereferenceFcb(fsdDirectoryFcb);
                fsdDirectoryFcb = fcbFileObject;
                errorCode = 0x00;
            }

        STFS_FINISH_FSD_CREATE:
            fsdHandle.Fcb = fsdDirectoryFcb;
            fsdHandle.DirectoryEnumContext = null;

        STFS_EXIT_FSD_CREATE:
            if (errorCode != 0x00)
            {
                if (fsdDirectoryFcb != null)
                {
                    StfsDereferenceFcb(fsdDirectoryFcb);
                }
            }
            return errorCode;
        }

        internal uint StfsFsdDirectoryControl(IoFsdHandle ioHandle, string searchPattern, out IoFsdDirectoryInformation fsdDirectoryInformation)
        {
            fsdDirectoryInformation = new IoFsdDirectoryInformation();
            var fcb = (StfsFcb)ioHandle.Fcb;

            if (!fcb.IsDirectory)
            {
                return 0xC000000D;
                //throw new StfsException("attempted to search a non-directory for files [0xC000000D].");
            }

            if (ioHandle.DirectoryEnumContext == null)
            {
                IoFsd.IoCreateDirectoryEnumContext(searchPattern, false, out ioHandle.DirectoryEnumContext);
            }

            var enumContext = ioHandle.DirectoryEnumContext;
            StfsDirectoryEntry directoryEntry;
            var errorCode = StfsLookupElementNameInDirectory(fcb, null, enumContext, out directoryEntry);
            if (errorCode == 0x00)
            {
                fsdDirectoryInformation.CreationTime = StfsTimeStampToDatetime(directoryEntry.CreationTimeStamp);
                fsdDirectoryInformation.LastAccessTime = fsdDirectoryInformation.ChangeTime = fsdDirectoryInformation.LastWriteTime = fsdDirectoryInformation.LastWriteTime = StfsTimeStampToDatetime(directoryEntry.LastWriteTimeStamp);
                if (directoryEntry.IsDirectory)
                {
                    fsdDirectoryInformation.FileAttributes = 0x10;
                    fsdDirectoryInformation.EndOfFile = 0x00;
                    fsdDirectoryInformation.AllocationSize = 0x00;
                }
                else
                {
                    fsdDirectoryInformation.FileAttributes = 0x80;
                    fsdDirectoryInformation.AllocationSize = directoryEntry.FileBounds.Filesize;
                    fsdDirectoryInformation.EndOfFile = directoryEntry.FileBounds.Filesize;
                }
                fsdDirectoryInformation.FileName = directoryEntry.FileName;

                enumContext.CurrentDirectoryOffset = (directoryEntry.DirectoryEntryByteOffset + 0x40);
            }
            return errorCode;
        }

        internal void StfsFsdSetInformation(IoFsdHandle handle)
        {
            uint errorCode = 0x00;

            var irp = handle.Irp;
            var irpSp = handle.Sp;
            StfsFcb fcb = (StfsFcb)handle.Fcb;

            if (VolumeExtension.ReadOnly && irpSp.SetFileParameters.FileInformationClass != FileInformationClass.FilePositionInformation)
                goto FSD_INVALID_OPERATION;

            switch (irpSp.SetFileParameters.FileInformationClass)
            {
                case FileInformationClass.FileRenameInformation:
                    {
                        if (fcb.IsRootDirectory)
                            goto FSD_INVALID_FCB;
                        if (fcb.CloneFcb != null)
                            goto FSD_INVALID_OPERATION;

                        StfsSetRenameInformation(handle, fcb, new FileRenameInformation(irp.UserBuffer));
                    }
                    break;

                case FileInformationClass.FileDispositionInformation:
                    {
                        if (fcb.IsRootDirectory)
                            goto FSD_INVALID_FCB;
                        if (fcb.CloneFcb != null)
                            goto FSD_INVALID_OPERATION;

                        StfsSetDispositionInformation(fcb, new FileDispositionInformation(irp.UserBuffer));
                    }
                    break;
                case FileInformationClass.FileAllocationInformation:
                    {
                        if (fcb.IsDirectory)
                            goto FSD_INVALID_FCB;
                        if (fcb.CloneFcb != null)
                            goto FSD_INVALID_OPERATION;

                        var allocationInformation = new FileAllocationInformation(irp.UserBuffer);
                        if (allocationInformation.AllocationSize.HighPart != 0x00)
                            goto FSD_INVALID_ALLOCATION_SIZE;

                        StfsSetAllocationSize(fcb, allocationInformation.AllocationSize.LowPart, false);
                    }
                    break;
                case FileInformationClass.FileEndOfFileInformation:
                    {
                        if (fcb.IsDirectory)
                            goto FSD_INVALID_FCB;
                        if (fcb.CloneFcb != null)
                            goto FSD_INVALID_OPERATION;

                        StfsSetEndOfFileInformation(fcb, new FileEndOfFileInfo(irp.UserBuffer));
                    }
                    break;
                case FileInformationClass.FileBasicInformation:
                    {
                        if (fcb.IsRootDirectory)
                            goto FSD_INVALID_FCB;
                        if (fcb.CloneFcb != null)
                            goto FSD_INVALID_OPERATION;

                        StfsSetBasicInformation(fcb, new FileBasicInformation(irp.UserBuffer));
                    }
                    break;
            }
            goto FSD_SET_STATUS;
        FSD_INVALID_ALLOCATION_SIZE:
            errorCode = 0xC000007F;
            goto FSD_SET_STATUS;
        FSD_INVALID_OPERATION:
            errorCode = 0xC0000022;
            goto FSD_SET_STATUS;
        FSD_INVALID_FCB:
            errorCode = 0xC000000D;
        FSD_SET_STATUS:
            handle.Irp.UserIosb.Information = errorCode;
        }

        internal void StfsFsdClose(IoFsdHandle ioFsdHandle)
        {
            if (ioFsdHandle.DirectoryEnumContext != null)
            {
                ioFsdHandle.DirectoryEnumContext.CurrentDirectoryOffset = 0x00;
                ioFsdHandle.DirectoryEnumContext.SearchPattern = null;
            }
            if (ioFsdHandle.Fcb != null)
            {
                StfsDereferenceFcb((StfsFcb)ioFsdHandle.Fcb);
            }
        }

        internal void StfsFsdFlushBuffers(IoFsdHandle ioFsdHandle)
        {
            StfsFlushBlockCache(0x00, 0xFFFFFFFF);
        }

        internal void StfsFsdCleanup(IoFsdHandle ioFsdHandle)
        {
            var fcb = (StfsFcb)ioFsdHandle.Fcb;
            if (fcb == null || fcb.Referenced != 0x01) return;
            if ((fcb.State & 0x08) != 0)
            {
                StfsUpdateDirectoryEntry(fcb, true);

                if (!fcb.IsDirectory && fcb.AllocationBlocks != 0x00)
                {
                    StfHashEntry hashEntry = null;
                    StfsFreeBlocks(fcb.FirstBlockNumber, false, ref hashEntry);
                }
                StfsDereferenceFcb(fcb.ParentFcb);
                fcb.ParentFcb = null;
                IsModified = true;
            }
            else
            {
                if ((fcb.State & 0x10) != 0)
                {
                    StfsUpdateDirectoryEntry(fcb, false);
                    IsModified = true;
                }
            }
        }

        public void StfsControlDevice(StfsControlCode ControlCode, object ControlBuffer)
        {
            switch (ControlCode)
            {
                case StfsControlCode.StfsBuildVolumeDescriptor:

                    var volumeDescriptor = (StfsVolumeDescriptor)ControlBuffer;
                    this.StfsBuildVolumeDescriptor(volumeDescriptor);

                    break;
                case StfsControlCode.StfsFlushDirtyBuffers:

                    if ((this.VolumeExtension.RootHashEntry.LevelAsUint & 0x80000000) != 0)
                    {
                        if (!VolumeExtension.ReadOnly)
                        {
                            this.StfsExtendBackingFileSize(this.VolumeExtension.NumberOfExtendedBlocks);
                        }

                        this.StfsFlushUpdateDirectoryEntries();
                        this.StfsFlushBlockCache(0, 0xffffffff);
                    }
                    break;
                case StfsControlCode.StfsResetWriteState:

                    this.VolumeExtension.RootHashEntry.LevelAsUint &= 0x7FFFFFFF;
                    this.VolumeExtension.NumberOfFreeBlocks += this.VolumeExtension.NumberOfFreePendingBlocks;
                    this.VolumeExtension.NumberOfFreePendingBlocks = 0;
                    this.VolumeExtension.DirectoryAllocationBlockCount = (ushort)(this._directoryFcb.AllocationBlocks / 0x1000);

                    this._directoryFcb.State &= 0xDF;

                    this.StfsResetWriteableBlockCache();

                    break;
            }
        }

        [Obfuscation]
        public void StfsFlush()
        {
            lock (_threadLocker)
            {
                StfsControlDevice(StfsControlCode.StfsFlushDirtyBuffers, null);
                StfsControlDevice(StfsControlCode.StfsBuildVolumeDescriptor, _volumeDescriptor);
                StfsControlDevice(StfsControlCode.StfsResetWriteState, null);
                IsModified = false;
            }
        }

        [Obfuscation]
        public void SetVolumeLabel(string volumeLabel)
        {
            if (!string.IsNullOrWhiteSpace(volumeLabel))
                _volumeLabel = volumeLabel;
        }

        private void StfsBuildVolumeDescriptor(StfsVolumeDescriptor volumeDescriptor)
        {
            volumeDescriptor.DescriptorLength = 0x24;

            VolumeExtension.ReadOnly = false;

            volumeDescriptor.RootActiveIndex = (byte)(((this.VolumeExtension.RootHashEntry.LevelAsUint >> 30) & 1));

            volumeDescriptor.DirectoryFirstBlockNumber = this._directoryFcb.FirstBlockNumber;
            volumeDescriptor.DirectoryAllocationBlocks = (ushort)(this._directoryFcb.AllocationBlocks / 0x1000);

            volumeDescriptor.RootHash = this.VolumeExtension.RootHashEntry.Hash;

            volumeDescriptor.NumberOfTotalBlocks = this.VolumeExtension.NumberOfTotalBlocks;
            volumeDescriptor.NumberOfFreeBlocks = this.VolumeExtension.NumberOfFreePendingBlocks +
                                                  this.VolumeExtension.NumberOfFreeBlocks;
        }

        private uint StfsCreateNewFile(StfsFcb parentDcb, string name, uint options, uint allocationSize, ref StfsDirectoryEntry dirent, out StfsFcb fileFcb)
        {
            if (VolumeExtension.ReadOnly)
            {
                throw new StfsException("Attempted to create a new file on a read-only device [0xC000009A].");
            }

            var byteOffset = dirent.DirectoryEndOfListing;
            if (byteOffset == 0xffffffff)
            {
                byteOffset = this._directoryFcb.ValidAllocBlocks;
                if ((byteOffset & 0xFFF) != 0)
                {
                    throw new StfsException(
                        string.Format(
                            "Detected an invalid amount of allocation blocks for the directory while creating {0}.",
                            name));
                }
                StfsSetAllocationSize(this._directoryFcb, this._directoryFcb.ValidAllocBlocks + 0x1000, false);
            }
            else if (byteOffset > 0x3FFF80)
            {
                throw new StfsException("cannot make new directory entry [0xC00002EA].");
            }

            dirent.DirectoryEntryByteOffset = byteOffset;

            dirent.FileName = name;
            dirent.FileNameLength = (byte)name.Length;

            dirent.IsDirectory = Bitwise.IsFlagOn(options, IRP.FILE_DIRECTORY_FILE);
            dirent.CreationTimeStamp = StfsDateTimeToTimeStamp(DateTime.Now);
            dirent.LastWriteTimeStamp = dirent.CreationTimeStamp;

            StfsFcb fcb = StfsCreateFcb(dirent, parentDcb);

            if (!fcb.IsDirectory)
            {
                StfsSetAllocationSize(fcb, 0x00, false);
            }

            StfsUpdateDirectoryEntry(fcb, false);

            fileFcb = fcb;

            return 0x00;
        }

        internal void StfsOverwriteExistingFile(StfsFcb fileFcb, uint allocationSize)
        {
            fileFcb.CreationTimeStamp = StfsDateTimeToTimeStamp(DateTime.Now);
            fileFcb.LastWriteTimeStamp = fileFcb.CreationTimeStamp;

            StfsSetAllocationSize(fileFcb, allocationSize, false);

            StfsUpdateDirectoryEntry(fileFcb, false);
        }

        internal StfsFcb StfsCreateFcb(StfsDirectoryEntry dirEnt, StfsFcb parentFcb)
        {
            var newFcb = new StfsFcb(dirEnt, parentFcb);
            newFcb.ParentDcbLinks = parentFcb.ParentDcbQueue.InsertHeadList(newFcb);
            _fcbs.Add(newFcb);
            return newFcb;
        }

        internal StfsFcb StfsCreateCloneFcb(StfsFcb fcb)
        {
            return new StfsFcb(fcb);
        }

        internal StfsFcb StfsFindOpenChildFcb(StfsFcb fcb, string fileName)
        {
            bool dcbAcquired = false;
            StfsFcb dcb = null;
            do
            {
            //Restart_Scan: // not referenced?
                if(fcb != fcb.ParentDcbQueue.FirstEntrySList())
                {
                    dcb = (StfsFcb)fcb.ParentDcbLinks.Fcb;
                }
                else
                {
                    if (dcbAcquired)
                        goto Exit;
                    if (!fcb.IsRootDirectory && fcb.CloneFcb == null)
                        goto Exit;
                    fcb = fcb.CloneFcb;
                    dcbAcquired = true;
                }
            } while (true);
        Exit:
            return dcb;
        }

        internal StfsFcb StfsReferenceFcb(StfsFcb fcb)
        {
            if ((fcb.State & 0x04) != 0x00)
            {
                fcb.Referenced++;
                return fcb;
            }

            if(fcb.CloneFcb != null)
            {
                fcb.Referenced++;
                return fcb;
            }

            //var localFcb = fcb;
            //var cloneFcb = StfsCreateCloneFcb(fcb);

            return null;
        }

        internal void StfsDereferenceFcb(StfsFcb Fcb)
        {
            if (Fcb.Referenced <= 0)
            {
                throw new StfsException("Attempted to de-reference an FCB with zero references.");
            }

            do
            {
                Fcb.Referenced--;
                if (Fcb.Referenced != 0)
                    break;
                
                if (Fcb.ParentFcb != null)
                {
                    Fcb.ParentFcb.ParentDcbQueue.RemoveHeadList();
                }

                Fcb.CloneFcb = null;
                StfsFcb parentFcb = Fcb.ParentFcb;
                // free the current FCB from the pool
                _fcbs.Remove(Fcb);
                Fcb = parentFcb;

            } while (Fcb != null);
        }

        internal uint StfsOpenTargetDirectory(IoFsdHandle handle, StfsFcb dcb)
        {
            if (!dcb.IsDirectory)
                throw new StfsException("attmpted to open a non-directory for updating.");

            if (VolumeExtension.ReadOnly)
                return 0xC0000022;

            var irpSp = handle.Sp;
            var fileObject = irpSp.FileObject;
            if (dcb.ShareAccess.OpenCount == 0x00)
                IoFsd.IoSetShareAccess(irpSp.CreateParametersParameters.DesiredAccess, irpSp.CreateParametersParameters.ShareAccess, fileObject, ref dcb.ShareAccess);
            else
                IoFsd.IoCheckShareAccess(irpSp.CreateParametersParameters.DesiredAccess, irpSp.CreateParametersParameters.ShareAccess,
                                         fileObject, ref dcb.ShareAccess, true);

            StfsReferenceFcb(dcb);

            fileObject.FsContext = dcb;
            fileObject.FsContext2 = null;

            return 0x00;
        }

        private static bool StfsIsValidFileName(string fileName)
        {
            if (fileName.Length == 0 || fileName.Length > 40)
                return false;

            if (fileName.Length == 1 && fileName[0] == '.' || 
                fileName.Length == 2 && fileName[1] == '.')
                return false;

            var invalidTable = new uint[] { 0xFFFFFFFF, 0xFC009C04, 0x10000000, 0x10000000 };

            return fileName.All(c => ((1 << (c & 0x1f)) & invalidTable[((c >> 3) & 0x1FFFFFFC)/4]) == 0);
        }

        private NTSTATUS StfsSetRenameInformation(IoFsdHandle handle, StfsFcb fcb, FileRenameInformation renameInformation)
        {
            if (renameInformation.FileName.Length == 0x00)
                return 0xC000000D;

            var directoryEntry = new StfsDirectoryEntry();
            StfsFcb dcb = null;
            var irpsSp = handle.Sp;

            var fileName = renameInformation.FileName;
            var searchSeperator = fileName.LastIndexOf('\\');
            //var directoryPath = fileName.Substring(0x00, searchSeperator + 1);
            var name = fileName.Substring(searchSeperator + 1, fileName.Length - (searchSeperator + 1));

            if (!StfsIsValidFileName(name))
                return 0xC0000033;

            // Get the fcb for the target directory
            if (irpsSp.FileObject.FsContext != null)
            {
                dcb = (StfsFcb)irpsSp.FileObject.FsContext;
                if (dcb.IsRootDirectory == false && Bitwise.IsFlagOn(fcb.State ^ dcb.State, 0x01))
                    goto STFS_STATUS_ACCCESS_DENIED;
            }
            else
            {
                dcb = fcb.ParentFcb;
            }

            if (!dcb.IsDirectory)
                throw new StfsException("opened target directory was not a directory");

            if (fcb.ParentDcbQueue.FirstEntrySList() != fcb)
            {
                if (!fcb.IsDirectory)
                    throw new StfsException("the current FCB does not belong to a directory");

                goto STFS_STATUS_ACCCESS_DENIED;
            }

            uint errorCode;
            if ((errorCode = StfsLookupElementNameInDirectory(dcb, name, null, out directoryEntry)) == 0x00)
            {
                uint returnedBlockNumber = 0xFFFFFFFF;
                byte[] dataBlock = null;
                int cacheBlockIndex = -1, hashBlockCacheIndex = -1;
                StfsHashBlock hashBlock = null;
                StfsBeginDirectoryEntryUpdate((uint)fcb.DirectoryEntryIndex * 0x40, ref directoryEntry,
                                              ref returnedBlockNumber, ref dataBlock, ref cacheBlockIndex,
                                              ref hashBlock, ref hashBlockCacheIndex);
                dataBlock[(fcb.DirectoryEntryIndex * 0x40) + 0x28] &= 0xC0;

                StfsEndDataBlockUpdate(dataBlock, cacheBlockIndex, returnedBlockNumber, ref hashBlock,
                                       hashBlockCacheIndex);

                var freeEntry = new StfHashEntry();
                if (directoryEntry.AllocationBlocks != 0x00)
                {
                    StfsFreeBlocks(directoryEntry.FirstBlockNumber, false, ref freeEntry);
                }
            }

            if (errorCode == 0xC0000034 || errorCode == 0x00)
            {
                if (string.CompareOrdinal(fcb.FileName, fileName) != 0x00)
                {
                    fcb.FileName = name;
                }
                if (fcb.ParentFcb != dcb)
                {
                    StfsDereferenceFcb(fcb.ParentFcb);
                    fcb.ParentFcb = dcb;
                    StfsReferenceFcb(dcb);
                }
                StfsUpdateDirectoryEntry(fcb, false);
                IsModified = true;
            }

            return NT.STATUS_SUCCESS;
        STFS_STATUS_ACCCESS_DENIED:
            return NT.STATUS_ACCESS_DENIED;
        }

        internal uint StfsLookupElementNameInDirectory(StfsFcb directoryFcb, string elementName, IoDirectoryEnumContext directoryEnumContext,
                                                       out StfsDirectoryEntry directoryLookup)
        {
            uint dirAllocBlocks,
                 returnedFileRunLength = 0,
                 byteOffset,
                 dirBlockPosition = 0,
                 dirBlockEndPosition = 0,
                 endOfListing = 0xffffffff,
                 dwReturn = 0;

            var blockCacheIndex = -1;

            EndianIO reader = null;
            directoryLookup = new StfsDirectoryEntry();

            if (_volumeDescriptor.DirectoryIndexBoundsValid != 0)
            {
                if (!VolumeExtension.ReadOnly)
                {
                    dirAllocBlocks = ((_directoryFcb.Filesize & 0xffff) + 1) * 0x40;
                    byteOffset = ((_directoryFcb.Filesize & 0xffff0000) * 0x40);
                }
                else
                {
                    throw new StfsException("Function is not available for this package type. [ Read-only format].");
                }
            }
            else
            {
                dirAllocBlocks = _directoryFcb.AllocationBlocks;
                dirBlockPosition = 0;
                byteOffset = 0x00;
            }

            if (directoryEnumContext != null)
            {
                byteOffset = byteOffset < directoryEnumContext.CurrentDirectoryOffset ? directoryEnumContext.CurrentDirectoryOffset : byteOffset;
            }

            StfsDirectoryEntry dirEnt = null;
            do
            {
                if (dirBlockPosition == dirBlockEndPosition)
                {
                    if (blockCacheIndex != -1)
                    {
                        this.StfsDereferenceBlock(blockCacheIndex);
                        blockCacheIndex = -1;
                    }

                    if (byteOffset < dirAllocBlocks)
                    {
                        byte[] dirBlock =
                            this.StfsMapReadableDataBlock(
                                this.StfsByteOffsetToBlockNumber(this._directoryFcb, byteOffset,
                                                                 ref returnedFileRunLength), ref blockCacheIndex);

                        dirBlockEndPosition += 0x1000;

                        reader = new EndianIO(new MemoryStream(dirBlock, false), EndianType.Big);
                        reader.Position = byteOffset & 0xFFF;
                    }
                    else
                    {
                        dwReturn = 0xC0000034;
                        break;
                    }
                }

                dirEnt = new StfsDirectoryEntry(reader);

                if (dirEnt.FileNameLength == 0)
                {
                    if (endOfListing == 0xffffffff)
                    {
                        endOfListing = byteOffset;
                    }
                }
                else
                {
                    if (dirEnt.DirectoryIndex == directoryFcb.DirectoryEntryIndex)
                    {
                        if (directoryEnumContext != null)
                        {
                            if (directoryEnumContext.SearchPattern != null)
                            {
                                if (string.Compare(directoryEnumContext.SearchPattern, dirEnt.FileName, true) == 0x00)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (String.Compare(dirEnt.FileName, elementName, true) == 0x00)
                            {
                                break;
                            }
                        }
                    }
                }

                dirBlockPosition += 0x40;
                byteOffset += 0x40;
            } while (true);

            if (byteOffset > 0x3FFF80) // MAX FileEntry count = 65534
            {
                throw new StfsException(
                    string.Format("The current directory's [ {0} ] index is out of range.",
                                  dirEnt.FileName));
            }

            if (blockCacheIndex != -1)
                StfsDereferenceBlock(blockCacheIndex);

            if (reader != null)
                reader.Close();

            if (dirEnt != null && dwReturn == 0x00)
            {
                dirEnt.DirectoryEntryByteOffset = byteOffset;
                directoryLookup = dirEnt;
            }
            directoryLookup.DirectoryEndOfListing = endOfListing;
            return dwReturn;
        }

        internal string StfsFindNextDirectoryName(StfsFcb Fcb, uint SearchOffset)
        {
            if (!Fcb.IsDirectory)
            {
                throw new StfsException("attempted to determine if a non-directory was empty.");
            }

            var dirEnt = new StfsDirectoryEntry();

            uint dwReturn = this.StfsFindNextDirectoryEntry(Fcb, SearchOffset, null, ref dirEnt);

            if (dwReturn != 0x00)
            {
                throw new StfsException(string.Format("failed to find a directory entry in the folder '{0}'",
                                                      Fcb.FileName));
            }

            return dirEnt.FileName;
        }

        internal uint StfsFindNextDirectoryEntry(StfsFcb _directoryFcb, uint SearchStartOffset, string Name,
                                               ref StfsDirectoryEntry DirectoryLookup)
        {
            uint DirAllocBlocks = 0,
                 ReturnedFileRunLength = 0,
                 ByteOffset = SearchStartOffset,
                 DirBlockPosition = 0,
                 DirBlockEndPosition = 0,
                 EndOfListing = 0xffffffff,
                 dwReturn = 0;

            int BlockCacheIndex = -1;

            EndianIO reader = null;
            StfsDirectoryEntry dirEnt = null;

            if (_volumeDescriptor.DirectoryIndexBoundsValid != 0)
            {
                if (!this.VolumeExtension.ReadOnly)
                {
                    DirAllocBlocks = ((_directoryFcb.Filesize & 0xffff) + 1) * 0x40;
                    ByteOffset = ((_directoryFcb.Filesize & 0xffff0000) * 0x40);
                }
                else
                {
                    throw new StfsException("Function is not available for this package type. [ Read-only format].");
                }
            }
            else
            {
                DirAllocBlocks = this._directoryFcb.AllocationBlocks;
                DirBlockPosition = 0;
            }

            do
            {
                if (DirBlockPosition == DirBlockEndPosition)
                {
                    if (BlockCacheIndex != -1)
                    {
                        this.StfsDereferenceBlock(BlockCacheIndex);
                        BlockCacheIndex = -1;
                    }

                    if (ByteOffset < DirAllocBlocks)
                    {
                        byte[] dirBlock =
                            this.StfsMapReadableDataBlock(
                                this.StfsByteOffsetToBlockNumber(this._directoryFcb, ByteOffset,
                                                                 ref ReturnedFileRunLength), ref BlockCacheIndex);

                        reader = new EndianIO(new MemoryStream(dirBlock, false), EndianType.Big);
                        reader.Position = ByteOffset & 0xFFF;
                        DirBlockPosition = (ByteOffset & 0xFFF) + DirBlockEndPosition;
                        DirBlockEndPosition += 0x1000;
                    }
                    else
                    {
                        dwReturn = 0xC0000034;
                        break;
                    }
                }

                dirEnt = new StfsDirectoryEntry(reader);

                if (dirEnt.FileNameLength == 0)
                {
                    if (EndOfListing == 0xffffffff)
                    {
                        EndOfListing = ByteOffset;
                    }
                }
                else
                {
                    if (dirEnt.DirectoryIndex == _directoryFcb.DirectoryEntryIndex)
                    {
                        if (ByteOffset > 0x3FFF80)
                        {
                            throw new StfsException(
                                string.Format("The current directory's [ {0} ] index is out of range.", dirEnt.FileName));
                        }

                        else if (Name == null || dirEnt.FileName.ToLower().Contains(Name.ToLower()))
                        {
                            dirEnt.DirectoryEntryByteOffset = ByteOffset;

                            DirectoryLookup = dirEnt;

                            break;
                        }
                    }
                }

                DirBlockPosition += 0x40;
                ByteOffset += 0x40;
            } while (true);

            if (BlockCacheIndex != -1)
            {
                this.StfsDereferenceBlock(BlockCacheIndex);
            }

            if (reader != null)
            {
                reader.Close();
            }

            DirectoryLookup.DirectoryEndOfListing = EndOfListing;

            return dwReturn;
        }

        internal int StfsEnsureWriteableDirectoryEntry(StfsFcb Fcb)
        {
            if (!this.VolumeExtension.ReadOnly)
            {
                if ((Fcb.State & 4) == 0)
                {
                    if ((Fcb.State & 0x20) == 0)
                    {
                        byte[] DataBlock = new byte[0x1000];
                        StfsHashBlock hashBlock = null;
                        StfsDirectoryEntry dirEnt = null;
                        int dataCacheIndex = -1, hashCacheIndex = -1;
                        uint blockNumer = 0xffffffff;

                        this.StfsBeginDirectoryEntryUpdate((uint)Fcb.DirectoryEntryIndex * 0x40, ref dirEnt, ref blockNumer,
                                                           ref DataBlock, ref dataCacheIndex, ref hashBlock,
                                                           ref hashCacheIndex);

                        Fcb.State |= 0x20;

                        this.StfsEndDataBlockUpdate(DataBlock, dataCacheIndex, blockNumer, ref hashBlock, hashCacheIndex);
                    }
                }
                else
                {
                    throw new StfsException(
                        string.Format(
                            "Detected an invalid FCB state while attempting to ensure a writeable directory entry for {0}.",
                            Fcb.FileName));
                }
            }
            else
            {
                throw new StfsException("Attempted to write with a read-only device.");
            }

            return 0x00;
        }

        internal void StfsUpdateDirectoryEntry(StfsFcb Fcb, bool ApplyDeleteOnClose)
        {
            int hashBlockCacheIndex = -1, dataBlockCacheIndex = -1;

            byte[] dataBlock = new byte[0x1000];
            StfsHashBlock hashBlock = null;
            StfsDirectoryEntry dirEntry = null;
            uint retBlockNum = 0xffffffff;

            this.StfsBeginDirectoryEntryUpdate((uint)Fcb.DirectoryEntryIndex * 0x40, ref dirEntry, ref retBlockNum,
                                               ref dataBlock, ref dataBlockCacheIndex, ref hashBlock,
                                               ref hashBlockCacheIndex);

            this.StfsFillDirectoryEntryFromFcb(Fcb, ref dirEntry, ApplyDeleteOnClose);

            Array.Copy(dirEntry.ToArray(), 0, dataBlock, (Fcb.DirectoryEntryIndex * 0x40) & 0xFFF, 0x40);

            this.StfsEndDataBlockUpdate(dataBlock, dataBlockCacheIndex, retBlockNum, ref hashBlock, hashBlockCacheIndex);

            Fcb.State = (byte)((Fcb.State & 0xCF) | 0x20);
        }

        internal void StfsBeginDirectoryEntryUpdate(uint Offset, ref StfsDirectoryEntry ReturnedDirectoryEntry,
                                                   ref uint ReturnedBlockNumber, ref byte[] ReturnedDataBlock,
                                                   ref int dataBlockCacheIndex, ref StfsHashBlock ReturnedHashBlock,
                                                   ref int HashBlockCacheIndex)
        {
            this.StfsBeginDataBlockUpdate(this._directoryFcb, ref ReturnedHashBlock, ref HashBlockCacheIndex, Offset,
                                          ref ReturnedDataBlock, ref dataBlockCacheIndex, false, ref ReturnedBlockNumber);

            var reader = new EndianIO(new MemoryStream(ReturnedDataBlock, false), EndianType.Big);
            reader.Position = Offset & 0xfff;

            ReturnedDirectoryEntry = new StfsDirectoryEntry(reader);

            reader.Close();

            if (Offset == this._directoryFcb.ValidAllocBlocks)
            {
                this._directoryFcb.ValidAllocBlocks += 0x1000;

                if (this._directoryFcb.ValidAllocBlocks != this._directoryFcb.AllocationBlocks)
                {
                    throw new StfsException("The directory listing's allocation state was found to be invalid.");
                }
            }
        }

        /*
        private void StfsRefreshDirectoryEntryFromOpenChildFcb(StfsFcb fcb, StfsDirectoryEntry directoryEntry)
        {
            bool exit = false;
            do
            {
                
            } while (true);
        }
        */

        internal void StfsFillDirectoryEntryFromFcb(StfsFcb fcb, ref StfsDirectoryEntry DirectoryEntry,
                                                   bool ApplyDeleteOnClose)
        {
            uint AllocBlocks = fcb.AllocationBlocks / 0x1000;
            bool Contiguous = false;

            if (fcb.BlockPosition != 0)
            {
                if (AllocBlocks == 2)
                {
                    if ((fcb.FirstBlockNumber + 1) == fcb.LastBlockNumber)
                    {
                        Contiguous = true;
                    }
                }
            }
            else
            {
                if (AllocBlocks == 0)
                {
                    throw new StfsException(
                        "Invalid allocation block count detected while filling directory entry from the FCB.");
                }
                if (fcb.ContiguousBytesRead == fcb.AllocationBlocks ||
                    ((fcb.ContiguousBytesRead + 0x1000) == fcb.AllocationBlocks &&
                     (fcb.LastBlockNumber == (AllocBlocks + fcb.FirstBlockNumber - 1))))
                {
                    Contiguous = true;
                }
            }

            DirectoryEntry.FileName = fcb.FileName;
            DirectoryEntry.Contiguous = Contiguous;
            DirectoryEntry.IsDirectory = fcb.IsDirectory;
            DirectoryEntry.DirectoryIndex = fcb.ParentFcb.DirectoryEntryIndex;

            if (ApplyDeleteOnClose && (fcb.State & 8) != 0)
            {
                DirectoryEntry.FileNameLength = 0;
            }
            else
            {
                DirectoryEntry.FileNameLength = (byte)fcb.FileName.Length;
            }

            DirectoryEntry.ValidDataBlocks = fcb.ValidAllocBlocks / 0x1000;
            DirectoryEntry.AllocationBlocks = fcb.AllocationBlocks / 0x1000;
            DirectoryEntry.FirstBlockNumber = fcb.FirstBlockNumber;

            DirectoryEntry.FileBounds.Filesize = fcb.Filesize;
            DirectoryEntry.CreationTimeStamp = fcb.CreationTimeStamp;
            DirectoryEntry.LastWriteTimeStamp = fcb.LastWriteTimeStamp;
        }

        internal void StfsResetWriteableDirectoryBlock(uint FileByteOffset, byte[] DataBlock)
        {
            EndianIO IO = new EndianIO(DataBlock, EndianType.Big);

            if ((FileByteOffset & 0xFFF) != 0)
            {
                throw new StfsException("Invalid byte offset found while resetting a writeable directory block.");
            }

            IO.Stream.Position = 0;

            do
            {
                StfsDirectoryEntry dirEnt = new StfsDirectoryEntry(IO);

                if (!dirEnt.IsDirectory)
                {
                    uint dataBlocks = ((dirEnt.FileBounds.Filesize + 0xFFF) >> 12);
                    if (dirEnt.ValidDataBlocks > dataBlocks)
                    {
                        dirEnt.ValidDataBlocks = dataBlocks;

                        IO.Stream.Position -= 0x40;
                        IO.Write(dirEnt.ToArray());
                    }
                }
            } while (IO.Stream.Position < 0x1000);

            IO.Close();

            uint EntryIndex = (FileByteOffset >> 6), NextEntryIndex = EntryIndex + 0x40;

            foreach (StfsFcb fcb in this._fcbs)
            {
                if (fcb.IsDirectory) continue;
                if (fcb.DirectoryEntryIndex < EntryIndex || fcb.DirectoryEntryIndex >= NextEntryIndex) continue;
                uint newFilesize = RoundToBlock(fcb.Filesize);
                if (fcb.ValidAllocBlocks > newFilesize)
                {
                    fcb.Filesize = newFilesize;
                }
            }
        }

        public void StfsFreeBlocks(uint BlockNumber, bool MarkFirstAsLast, ref StfHashEntry FreeSingleHashEntry)
        {
            uint NumberOfFreeBlocks = 0, NumberOfFreePendingBlocks = 0, NexBlockNumber = 0xffffff;

            StfsFreeBlockState FreeBlockState = new StfsFreeBlockState();
            FreeBlockState.MarkFirstAsLast = MarkFirstAsLast;
            FreeBlockState.HashEntry = FreeSingleHashEntry;

            this.StfsSetInAllocationSupport(true);

            if (this.VolumeExtension.RootHashHierarchy > 0)
            {
                this.StfsFreeBlocksFromLevelNHashBlock(BlockNumber, this.VolumeExtension.RootHashHierarchy,
                                                       ref NumberOfFreePendingBlocks, ref NumberOfFreeBlocks,
                                                       ref NexBlockNumber, ref FreeBlockState);
            }
            else
            {
                this.StfsFreeBlocksFromLevel0HashBlock(BlockNumber, ref NumberOfFreeBlocks,
                                                       ref NumberOfFreePendingBlocks, ref NexBlockNumber,
                                                       ref FreeBlockState);
            }

            this.StfsSetInAllocationSupport(false);

            if (FreeSingleHashEntry != null)
            {
                FreeSingleHashEntry = FreeBlockState.HashEntry;
            }

            this.VolumeExtension.NumberOfFreePendingBlocks += NumberOfFreePendingBlocks;
            this.VolumeExtension.NumberOfFreeBlocks += NumberOfFreeBlocks;
        }

        internal uint StfsFreeBlocksFromLevel0HashBlock(uint blockNumber, ref uint numberOfFreeBlocks,
                                                       ref uint numberOfFreePendingBlocks, ref uint nextBlockNumber,
                                                       ref StfsFreeBlockState freeBlockState)
        {
            int cacheIndex = -1;
            nextBlockNumber = numberOfFreeBlocks = numberOfFreePendingBlocks = 0x00;
            const uint endLink = 0xffffff;
            uint hashBlockIndex = blockNumber / 0xAA;

            if (blockNumber < this.VolumeExtension.NumberOfTotalBlocks)
            {
                StfsHashBlock hashBlock = this.StfsMapWriteableHashBlock(blockNumber, 0, false, ref cacheIndex);
                do
                {
                    StfHashEntry hashEntry = hashBlock.RetrieveHashEntry((int)blockNumber);
                    nextBlockNumber = hashEntry.Level0.NextBlockNumber;
                    if (!freeBlockState.MarkFirstAsLast)
                    {
                        switch (hashEntry.Level0.State)
                        {
                            case StfsHashEntryLevel0State.Allocated:
                                hashEntry.LevelAsUint &= 0x3FFFFFFF;
                                hashEntry.LevelAsUint |= 0x40000000;
                                //hashEntry.Level0.State = StfsHashEntryLevel0State.FreedPending;
                                numberOfFreePendingBlocks++;
                                break;
                            case StfsHashEntryLevel0State.Pending:
                                hashEntry.LevelAsUint &= 0x3FFFFFFF;
                                //hashEntry.Level0.State = StfsHashEntryLevel0State.Unallocated;
                                numberOfFreeBlocks++;
                                break;
                            default:
                                throw new StfsException(string.Format("free of unallocated block number 0x{0:x}.",
                                                                      blockNumber));
                        }
                    }
                    else
                    {
                        freeBlockState.MarkFirstAsLast = false;
                        if (hashEntry.Level0.State != StfsHashEntryLevel0State.Allocated &&
                            hashEntry.Level0.State != StfsHashEntryLevel0State.Pending)
                        {
                            throw new StfsException(string.Format("reference of unallocated block number 0x{0:x}.",
                                                                  blockNumber));
                        }
                    }
                    hashEntry.SetNextBlockNumber(endLink);
                    if (freeBlockState.HashEntry != null)
                    {
                        freeBlockState.HashEntry = hashEntry;
                        nextBlockNumber = endLink;
                    }

                    if (nextBlockNumber == endLink)
                        break;

                    blockNumber = nextBlockNumber;
                } while ((nextBlockNumber / 0xAA) == hashBlockIndex);

                hashBlock.Save();

                StfsDereferenceBlock(cacheIndex);
            }
            else
            {
                throw new StfsException(string.Format("trying to free invalid block number 0x{0:x}. [0xC0000032]",
                                                      blockNumber));
            }
            return 0;
        }

        internal void StfsFreeBlocksFromLevelNHashBlock(uint BlockNumber, int CurrentLevel,
                                                       ref uint ReturnedNumberOfFreePendingBlocks,
                                                       ref uint ReturnedNumberOfFreeBlocks,
                                                       ref uint ReturnedNextBlockNumber,
                                                       ref StfsFreeBlockState FreeBlockState)
        {
            if (BlockNumber >= this.VolumeExtension.NumberOfTotalBlocks)
            {
                throw new StfsException(string.Format("trying to free invalid block number {0:0x} [0xC0000032].",
                                                      BlockNumber));
            }

            uint blocksPerCurrLevel = this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[CurrentLevel];
            uint blocksPerPrevLevel = this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[CurrentLevel - 1];

            int hashBlockCacheIndex = -1;
            StfsHashBlock hashBlock = StfsMapWriteableHashBlock(BlockNumber, CurrentLevel, false,
                                                                     ref hashBlockCacheIndex);

            uint blockIndex = BlockNumber / blocksPerCurrLevel, freeBlockCount = 0, freePendingBlockCount = 0;
            uint numberOfFreeBlocks = 0, numberOfFreePendingBlocks = 0, nextBlockNumber = 0;
            do
            {
                var prevBlockIndex = (BlockNumber / blocksPerPrevLevel) % 0xAA;
                if (CurrentLevel != 1)
                {
                    StfsFreeBlocksFromLevelNHashBlock(BlockNumber, CurrentLevel - 1, ref numberOfFreePendingBlocks,
                                                           ref numberOfFreeBlocks, ref nextBlockNumber,
                                                           ref FreeBlockState);
                }
                else
                {
                    StfsFreeBlocksFromLevel0HashBlock(BlockNumber, ref numberOfFreeBlocks,
                                                           ref numberOfFreePendingBlocks, ref nextBlockNumber,
                                                           ref FreeBlockState);
                }

                hashBlock = new StfsHashBlock(this.VolumeExtension.BlockCache.Data[hashBlockCacheIndex]);
                StfHashEntry hashEntry = hashBlock.RetrieveHashEntry((int)prevBlockIndex);
                if (hashEntry.LevelN.Writeable != 1)
                {
                    throw new StfsException(
                        string.Format("Detected a non-writeable entry while freeing blocks at level {0:d}", CurrentLevel));
                }
                hashEntry.SetNumberOfFreePendingBlocks(hashEntry.LevelN.NumberOfFreePendingBlocks +
                                                       numberOfFreePendingBlocks);
                hashEntry.SetNumberOfFreeBlocks(hashEntry.LevelN.NumberOfFreeBlocks + numberOfFreeBlocks);

                if (hashEntry.LevelN.NumberOfFreePendingBlocks > blocksPerPrevLevel)
                {
                    throw new StfsException(
                        string.Format("Detected an invalid amount of free-pending blocks for level {0:d}.", CurrentLevel));
                }
                if (hashEntry.LevelN.NumberOfFreeBlocks > blocksPerPrevLevel)
                {
                    throw new StfsException(string.Format("Detected an invalid amount of free blocks for level {0:d}.",
                                                          CurrentLevel));
                }

                uint freeBlocksForCurrentLevel = BlockNumber % blocksPerPrevLevel;
                uint blockNumToDiscard = BlockNumber - freeBlocksForCurrentLevel;
                freeBlocksForCurrentLevel = VolumeExtension.NumberOfTotalBlocks - blockNumToDiscard;

                if (freeBlocksForCurrentLevel > blocksPerPrevLevel)
                    freeBlocksForCurrentLevel = blocksPerPrevLevel;

                if (freeBlocksForCurrentLevel == hashEntry.LevelN.NumberOfFreePendingBlocks && freeBlocksForCurrentLevel == blocksPerPrevLevel)
                {
                    StfsDiscardBlock(blockNumToDiscard, CurrentLevel);
                }
                else if (freeBlocksForCurrentLevel == hashEntry.LevelN.NumberOfFreeBlocks)
                {
                    StfsDiscardBlock(blockNumToDiscard, CurrentLevel);
                }

                freeBlockCount += numberOfFreeBlocks;
                freePendingBlockCount += numberOfFreePendingBlocks;

                BlockNumber = nextBlockNumber;

                hashBlock.Save();
                if (nextBlockNumber == 0xFFFFFF)
                    break;

            } while ((BlockNumber / blocksPerCurrLevel) == blockIndex);

            ReturnedNextBlockNumber = nextBlockNumber;
            ReturnedNumberOfFreeBlocks = freeBlockCount;
            ReturnedNumberOfFreePendingBlocks = freePendingBlockCount;

            StfsDereferenceBlock(hashBlockCacheIndex);
        }

        internal int StfsAllocateBlocks(uint NumberOfNeededBlocks, uint AllocateBlocksType, uint StartingLinkBlockNumber,
                                       uint EndingLinkBlockNumber, ref uint ReturnedFirstAllocatedBlockNumber,
                                       ref uint ReturnedLastAllocatedBlockNumber)
        {
            if (this.VolumeExtension.ReadOnly)
            {
                throw new StfsException("Attempted to allocate blocks on a read-only device.");
            }
            else if (NumberOfNeededBlocks == 0)
            {
                throw new StfsException("Requested an invalid amount of blocks [zero] to be to be allocated.");
            }

            if (EndingLinkBlockNumber != 0xffffffff)
            {
                if (EndingLinkBlockNumber != 0xffffff)
                {
                    if (EndingLinkBlockNumber >= this.VolumeExtension.NumberOfTotalBlocks)
                    {
                        throw new StfsException(
                            "The ending link block number was greatr than the allocated number of blocks.");
                    }
                }
            }
            else
            {
                EndingLinkBlockNumber = 0xffffff;
            }

            uint TotalBlockCount = 0;
            switch (AllocateBlocksType)
            {
                case 0:
                    TotalBlockCount = this.VolumeExtension.DirectoryAllocationBlockCount + NumberOfNeededBlocks;

                    break;
                case 1:
                    if (NumberOfNeededBlocks != 1)
                    {
                        throw new StfsException(
                            "Attempted to allocate an invalid amount of blocks for allocation type-1 blocks.");
                    }
                    TotalBlockCount = this.VolumeExtension.DirectoryAllocationBlockCount + NumberOfNeededBlocks + 1;

                    break;
                case 2:
                    if (NumberOfNeededBlocks != 1)
                    {
                        throw new StfsException(
                            "Attempted to allocate an invalid amount of blocks for allocation type-2 blocks.");
                    }
                    if (this.VolumeExtension.DirectoryAllocationBlockCount == 0)
                    {
                        throw new StfsException("There are no blocks allocated for the directory listing.");
                    }
                    TotalBlockCount = this.VolumeExtension.DirectoryAllocationBlockCount + NumberOfNeededBlocks - 1;

                    break;
                case 3:
                    TotalBlockCount = this.VolumeExtension.DirectoryAllocationBlockCount + NumberOfNeededBlocks;

                    break;
                default:
                    throw new StfsException("Invalid allocation block type requested for allocation.");
            }

            if (this.VolumeExtension.NumberOfFreeBlocks < TotalBlockCount)
            {
                this.StfsExtendBackingAllocationSize(TotalBlockCount - this.VolumeExtension.NumberOfFreeBlocks);

                if (this.VolumeExtension.NumberOfFreeBlocks != TotalBlockCount)
                {
                    throw new StfsException("Invalid number of free blocks detected while allocating blocks.");
                }
            }

            if (AllocateBlocksType != 3)
            {
                StfsAllocateBlockState AllocateBlockState = new StfsAllocateBlockState();

                AllocateBlockState.NumberOfNeededBlocks = NumberOfNeededBlocks;
                AllocateBlockState.FirstAllocatedBlockNumber = 0xffffffff;
                AllocateBlockState.LastAllocatedBlockNumber = 0;
                AllocateBlockState.Block = -1;
                AllocateBlockState.HashEntry = null;

                this.StfsSetInAllocationSupport(true);

                bool MapEmptyHashBlock = (this.VolumeExtension.NumberOfFreeBlocks -
                                          this.VolumeExtension.NumberOfTotalBlocks) == 0
                                             ? true
                                             : false;

                if (this.VolumeExtension.RootHashHierarchy != 0)
                {
                    this.StfsAllocateBlocksFromLevelNHashBlock(0, this.VolumeExtension.RootHashHierarchy,
                                                               MapEmptyHashBlock, ref AllocateBlockState);
                }
                else
                {
                    this.StfsAllocateBlocksFromLevel0HashBlock(0, MapEmptyHashBlock, ref AllocateBlockState);
                }

                this.StfsSetInAllocationSupport(false);

                if (AllocateBlockState.HashEntry != null)
                {
                    StfsHashBlock hashBlock =
                        new StfsHashBlock(this.VolumeExtension.BlockCache.Data[AllocateBlockState.Block]);
                    hashBlock.RetrieveHashEntry((int)AllocateBlockState.LastAllocatedBlockNumber).SetNextBlockNumber(
                        EndingLinkBlockNumber);
                    hashBlock.Save();

                    this.StfsDereferenceBlock(AllocateBlockState.Block);
                }
                if (StartingLinkBlockNumber != 0xffffffff)
                {
                    if (StartingLinkBlockNumber < this.VolumeExtension.NumberOfTotalBlocks)
                    {
                        int cacheIndex = -1;

                        StfsHashBlock hashBlock = this.StfsMapWriteableHashBlock(StartingLinkBlockNumber, 0, false,
                                                                                 ref cacheIndex);

                        StfHashEntry hashEntry = hashBlock.RetrieveHashEntry((int)StartingLinkBlockNumber);
                        hashEntry.SetNextBlockNumber(AllocateBlockState.FirstAllocatedBlockNumber);

                        hashBlock.Save();

                        this.StfsDereferenceBlock(cacheIndex);
                    }
                    else
                    {
                        throw new StfsException(
                            "The starting link block number was greater than the number of total blocks.");
                    }
                }

                ReturnedFirstAllocatedBlockNumber = AllocateBlockState.FirstAllocatedBlockNumber;
                ReturnedLastAllocatedBlockNumber = AllocateBlockState.LastAllocatedBlockNumber;
            }
            return 0;
        }

        internal void StfsAllocateBlocksFromLevel0HashBlock(uint CurrentBlockNumber, bool MapEmptyHashBlock,
                                                           ref StfsAllocateBlockState AllocateBlockState)
        {
            if ((CurrentBlockNumber % 0xAA) != 0)
            {
                throw new StfsException("Invalid block number supplied for level 0 allocation.");
            }

            int blockCacheIndex = -1;

            StfsHashBlock hashBlock = this.StfsMapWriteableHashBlock(CurrentBlockNumber, 0, MapEmptyHashBlock,
                                                                     ref blockCacheIndex);

            uint EndingBlockNumber = this.VolumeExtension.NumberOfTotalBlocks - CurrentBlockNumber;
            uint FirstAllocBlockNum = 0xffffffff;

            if (EndingBlockNumber > 0xAA)
            {
                EndingBlockNumber = 0xAA;
            }

            var freeBlockCount = VolumeExtension.NumberOfFreeBlocks;
            if (AllocateBlockState.NumberOfNeededBlocks == 0 || freeBlockCount < AllocateBlockState.NumberOfNeededBlocks)
            {
                throw new StfsException("Requested an invalid amount of level 0 hash blocks to be allocated.");
            }

            EndingBlockNumber += CurrentBlockNumber;

            uint idx = 0, uIdx = CurrentBlockNumber, actualIdx = 0;
            StfHashEntry usedHashEntry = AllocateBlockState.HashEntry;

            while (uIdx < EndingBlockNumber)
            {
                StfHashEntry hashEntry = hashBlock.RetrieveHashEntry((int)idx);

                if (hashEntry.Level0.State == StfsHashEntryLevel0State.Unallocated)
                {
                    if (usedHashEntry != null)
                    {
                        usedHashEntry.SetNextBlockNumber(uIdx);

                        if (AllocateBlockState.Block != -1)
                        {
                            StfsHashBlock HashBlock =
                                new StfsHashBlock(this.VolumeExtension.BlockCache.Data[AllocateBlockState.Block]);
                            HashBlock.SetEntry((int)AllocateBlockState.HashEntryIndex, usedHashEntry);
                            HashBlock.Save();

                            this.StfsDereferenceBlock(AllocateBlockState.Block);
                            AllocateBlockState.Block = -1;
                        }
                    }
                    if (FirstAllocBlockNum > uIdx)
                    {
                        FirstAllocBlockNum = uIdx;
                    }

                    hashEntry.LevelAsUint |= 0xC0000000;
                    //hashEntry.Level0.State = StfsHashEntryLevel0State.Pending;

                    usedHashEntry = hashBlock.RetrieveHashEntry((int)idx);

                    AllocateBlockState.HashEntryIndex = idx;

                    AllocateBlockState.NumberOfNeededBlocks--;

                    freeBlockCount--;

                    actualIdx = uIdx;

                    if (AllocateBlockState.NumberOfNeededBlocks == 0)
                        break;
                }
                uIdx++;
                idx++;
            }

            hashBlock.Save();

            if (AllocateBlockState.FirstAllocatedBlockNumber > FirstAllocBlockNum)
            {
                AllocateBlockState.FirstAllocatedBlockNumber = FirstAllocBlockNum;
            }

            AllocateBlockState.LastAllocatedBlockNumber = actualIdx;

            if (usedHashEntry != null)
            {
                AllocateBlockState.HashEntry = usedHashEntry;
            }
            else
            {
                throw new StfsException("Detected an invalid hash entry while allocating level 0 hash blocks.");
            }

            AllocateBlockState.Block = blockCacheIndex;

            this.VolumeExtension.NumberOfFreeBlocks = freeBlockCount;
        }

        internal void StfsAllocateBlocksFromLevelNHashBlock(uint CurrentBlockNumber, int CurrentLevel,
                                                           bool MapEmptyHashBlock,
                                                           ref StfsAllocateBlockState AllocateBlockState)
        {
            if (CurrentLevel > this.VolumeExtension.RootHashHierarchy)
            {
                throw new StfsException(string.Format("Attempted to allocate blocks for an invalid level [{0}].",
                                                      CurrentLevel));
            }
            uint LevelBlockCount = this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[CurrentLevel];
            uint PrevLevelBlockCount = this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[CurrentLevel - 1];
            int hashBlockCacheIndex = -1;

            StfsHashBlock hashBlock = this.StfsMapWriteableHashBlock(CurrentBlockNumber, CurrentLevel, MapEmptyHashBlock,
                                                                     ref hashBlockCacheIndex);

            uint remainderCount = this.VolumeExtension.NumberOfTotalBlocks - CurrentBlockNumber;
            uint CommittedBlockCount = hashBlock.NumberOfCommittedBlocks;

            if (CommittedBlockCount > LevelBlockCount)
            {
                throw new StfsException("Found a hash block with more blocks committed than the maximum.");
            }

            if (remainderCount > LevelBlockCount)
            {
                remainderCount = LevelBlockCount;
            }
            uint hashBlockRemainder = remainderCount - CommittedBlockCount;
            if (hashBlockRemainder != 0)
            {
                uint blocklevelIndex = CommittedBlockCount % PrevLevelBlockCount;
                uint blockIndex = CommittedBlockCount / PrevLevelBlockCount;
                uint bIdx = blockIndex;

                StfHashEntry hashEntry = hashBlock.RetrieveHashEntry((int)bIdx);

                if (blocklevelIndex != 0)
                {
                    uint lowLevelBlock = PrevLevelBlockCount - blocklevelIndex;
                    if (lowLevelBlock != 0)
                    {
                        if (lowLevelBlock > hashBlockRemainder)
                        {
                            lowLevelBlock = hashBlockRemainder;
                        }

                        if (hashEntry.LevelN.NumberOfFreeBlocks > PrevLevelBlockCount)
                        {
                            throw new StfsException("Detected an invalid amount of free blocks while allocating blocks.");
                        }

                        hashEntry.SetNumberOfFreeBlocks(hashEntry.LevelN.NumberOfFreeBlocks + lowLevelBlock);

                        if (hashEntry.LevelN.NumberOfFreeBlocks > PrevLevelBlockCount)
                        {
                            throw new StfsException(
                                "Detected an invalid amount of new free blocks while allocating blocks.");
                        }

                        hashBlockRemainder -= lowLevelBlock;
                        hashEntry = hashBlock.RetrieveHashEntry((int)++bIdx);
                    }
                }

                if (hashBlockRemainder != 0)
                {
                    do
                    {
                        uint PerBlockLevel = PrevLevelBlockCount;
                        if (PrevLevelBlockCount > hashBlockRemainder)
                        {
                            PerBlockLevel = hashBlockRemainder;
                        }

                        if (hashEntry.LevelN.ActiveIndex != 0 || hashEntry.LevelN.Writeable != 0 ||
                            hashEntry.LevelN.NumberOfFreePendingBlocks != 0)
                        {
                            throw new StfsException(
                                string.Format("Found an invalid hash entry while allocating [Index: {0}, Level: {1}.",
                                              bIdx, CurrentLevel));
                        }
                        hashBlockRemainder -= PerBlockLevel;
                        hashEntry.SetNumberOfFreeBlocks(PerBlockLevel);

                        hashEntry = hashBlock.RetrieveHashEntry((int)++bIdx);
                    } while (hashBlockRemainder != 0);
                }

                if (bIdx > 0xAA)
                {
                    throw new StfsException("Detected an invalid hash-block entry index.");
                }

                hashBlock.NumberOfCommittedBlocks = remainderCount;
                CommittedBlockCount = remainderCount;

                hashBlock.Save();
            }

            uint committedTotal = CommittedBlockCount + CurrentBlockNumber, idx = 0;
            uint NumberOfNeededBlocks = AllocateBlockState.NumberOfNeededBlocks;

            if (CurrentBlockNumber < committedTotal)
            {
                do
                {
                    StfHashEntry hashEntry = hashBlock.RetrieveHashEntry((int)idx);

                    uint FreeBlocks = hashEntry.LevelN.NumberOfFreeBlocks;

                    if (FreeBlocks == 0)
                    {
                        idx++;
                        continue;
                    }

                    bool newMapEmptyHashBlock = FreeBlocks == PrevLevelBlockCount || (committedTotal - CurrentBlockNumber) == FreeBlocks;

                    if (CurrentLevel != 1)
                    {
                        this.StfsAllocateBlocksFromLevelNHashBlock(CurrentBlockNumber, CurrentLevel - 1,
                                                                   newMapEmptyHashBlock, ref AllocateBlockState);
                    }
                    else
                    {
                        this.StfsAllocateBlocksFromLevel0HashBlock(CurrentBlockNumber, newMapEmptyHashBlock,
                                                                   ref AllocateBlockState);
                    }

                    hashBlock = new StfsHashBlock(this.VolumeExtension.BlockCache.Data[hashBlockCacheIndex]);
                    hashEntry = hashBlock.RetrieveHashEntry((int)idx);

                    uint NewFreeBlocks = FreeBlocks;
                    if (FreeBlocks > NumberOfNeededBlocks)
                    {
                        NewFreeBlocks = NumberOfNeededBlocks;
                    }

                    NumberOfNeededBlocks -= NewFreeBlocks;
                    hashEntry.SetNumberOfFreeBlocks(hashEntry.LevelN.NumberOfFreeBlocks - NewFreeBlocks);
                    hashBlock.Save();

                    if (AllocateBlockState.NumberOfNeededBlocks != NumberOfNeededBlocks)
                    {
                        throw new StfsException("Detected an invalid amount of requested blocks to be allocated.");
                    }
                    idx++;
                } while (((CurrentBlockNumber += PrevLevelBlockCount) < committedTotal) && (NumberOfNeededBlocks != 0));
            }

            AllocateBlockState.NumberOfNeededBlocks = NumberOfNeededBlocks;

            this.StfsDereferenceBlock(hashBlockCacheIndex);
        }

        internal uint StfsSetBasicInformation(StfsFcb fcb, FileBasicInformation basicInformation)
        {
            bool updateDirectory = false;
            StfsTimeStamp creationTime = fcb.CreationTimeStamp, lastWriteTime = fcb.LastWriteTimeStamp;
            if ((basicInformation.CreationTime != DateTime.MinValue) && DateTime.Compare(fcb.CreationTime, basicInformation.CreationTime) != 0x00)
            {
                creationTime = StfsDateTimeToTimeStamp(basicInformation.CreationTime);
                updateDirectory = true;
            }

            if ((basicInformation.LastWriteTime != DateTime.MinValue) && DateTime.Compare(fcb.LastWriteTime, basicInformation.LastWriteTime) != 0x00)
            {
                lastWriteTime = StfsDateTimeToTimeStamp(basicInformation.LastWriteTime);
                updateDirectory = true;
            }

            // validate the attributes of the file or directory
            if (basicInformation.FileAttributes != 0x00)
            {
                byte attributes = (byte)basicInformation.FileAttributes;
                if (fcb.IsDirectory && (attributes & 0x10) == 0x00)
                    goto FSD_STATUS_INVALID_PARAMETER;
                if (!fcb.IsDirectory && (attributes & 0x10) != 0x00)
                    goto FSD_STATUS_INVALID_PARAMETER;
            }

            if (updateDirectory)
            {
                if (StfsEnsureWriteableDirectoryEntry(fcb) == 0x00)
                {
                    fcb.CreationTimeStamp = creationTime;
                    fcb.LastWriteTimeStamp = lastWriteTime;

                    fcb.State |= 0x10;
                }
                IsModified = true;
            }

            return 0x00;
        FSD_STATUS_INVALID_PARAMETER:
            return 0xC000000D;
        }

        /// <summary>
        /// Sets the end of file for the file specified by the FCB. 
        /// </summary>
        /// <param name="fcb">The FCB for the file.</param>
        /// <param name="endOfFileInfo">Information about the new end-of-file.</param>
        internal uint StfsSetEndOfFileInformation(StfsFcb fcb, FileEndOfFileInfo endOfFileInfo)
        {
            // Make sure the end-of-file is not out of range
            if (endOfFileInfo.EndOfFile.HighPart != 0x00)
                return 0xC000007F;

            var endOfFile = endOfFileInfo.EndOfFile.LowPart;
            // Make sure the new end-of-file is not equal to the current file size
            if (fcb.Filesize != endOfFile)
            {
                // Ensure that the FCB is writeable
                if (this.StfsEnsureWriteableDirectoryEntry(fcb) == 0)
                {
                    // if the end-of-file is greater than the current file size, expand the file
                    if (endOfFile > fcb.Filesize)
                    {
                        this.StfsSetAllocationSize(fcb, endOfFile, true);
                    }

                    // Set the file's new size
                    fcb.Filesize = endOfFile;

                    // If the FCB is not modifiable, throw an error
                    if ((fcb.State & 0x20) == 0)
                    {
                        throw new StfsException(
                            string.Format(
                                "Detected an invalid FCB state while setting end-of-file information for {0}.",
                                fcb.FileName));
                    }

                    // FCB has been modified
                    fcb.State |= 0x10;
                }
            }
            return 0x00;
        }

        private void StfsExtendBackingFileSize(uint numberOfExtendedBlocks)
        {
            if (VolumeExtension.ReadOnly)
            {
                throw new StfsException("Attempted to expand on an non-expandable device.");
            }

            if (numberOfExtendedBlocks > this.VolumeExtension.NumberOfExtendedBlocks)
            {
                throw new StfsException(
                    "The requested extension block count is greater than the volume's allowed number of extended blocks.");
            }

            if (numberOfExtendedBlocks > this.VolumeExtension.CurrentlyExtendedBlocks)
            {
                _io.Stream.SetLength(this.VolumeExtension.BackingFileOffset +
                                         (this.VolumeExtension.NumberOfExtendedBlocks << 12));

                this.VolumeExtension.CurrentlyExtendedBlocks = this.VolumeExtension.NumberOfExtendedBlocks;
            }
        }

        /// <summary>
        /// Expand the physical file on the drive by the amount of requested blocks.
        /// </summary>
        /// <param name="AllocationBlocks">Number of blocks (0x1000 bytes) to expand the file by.</param>
        internal uint StfsExtendBackingAllocationSize(uint AllocationBlocks)
        {
            if (AllocationBlocks == 0)
            {
                throw new StfsException("Attempted to extend the device's backing allocation size by zero.");
            }
            uint TotalAllocBlocks = this.VolumeExtension.NumberOfTotalBlocks + AllocationBlocks;

            if (AllocationBlocks > this.VolumeExtension.DataBlockCount ||
                TotalAllocBlocks > this.VolumeExtension.DataBlockCount)
            {
                return 0xC000007F;
            }

            uint blockIndex = TotalAllocBlocks + 0xA9, blocksPerLevel = 0xAA;
            uint blocksPerLevelIndex = blockIndex / blocksPerLevel;
            uint RootHierarchy = 0;

            if (blocksPerLevelIndex > 1)
            {
                blockIndex = (blocksPerLevelIndex + 0xA9) / blocksPerLevel;
                RootHierarchy = 1;
            }
            else
            {
                blockIndex = 0x00;
            }

            if (blockIndex > 1)
            {
                RootHierarchy = 2;
                blocksPerLevel = (blockIndex + 0xA9) / blocksPerLevel;
            }
            else
            {
                blocksPerLevel = 0x00;
            }

            if (!this.VolumeExtension.ReadOnly)
            {
                // expand file on disk
                uint BlockExpansion = (((blockIndex + blocksPerLevel) + blocksPerLevelIndex) << 1) + TotalAllocBlocks;
                _io.Stream.SetLength(this.VolumeExtension.BackingFileOffset + (BlockExpansion * Block));

                this.VolumeExtension.NumberOfExtendedBlocks = BlockExpansion;
            }

            if (this.VolumeExtension.NumberOfTotalBlocks != 0)
            {
                if (this.VolumeExtension.RootHashHierarchy < RootHierarchy)
                {
                    this.StfsFlushBlockCache(0, 0xffffffff);
                }

                int NewLevel = this.VolumeExtension.RootHashHierarchy + 1;

                while (NewLevel <= RootHierarchy)
                {
                    StfHashEntry hashEntry = null;
                    StfsHashBlock hashBlock = null;
                    int hashBlockCacheIndex = -1;

                    this.StfsMapEmptyHashBlock(0, NewLevel, ref hashEntry, ref hashBlock, ref hashBlockCacheIndex);

                    hashBlock.SetHashForEntry(0, this.VolumeExtension.RootHashEntry.Hash);
                    StfHashEntry newHashEntry = hashBlock.RetrieveHashEntry(0);
                    newHashEntry.SetNumberOfFreeBlocks(this.VolumeExtension.NumberOfFreeBlocks);
                    newHashEntry.SetNumberOfFreePendingBlocks(this.VolumeExtension.NumberOfFreePendingBlocks);
                    newHashEntry.LevelAsUint = (newHashEntry.LevelAsUint & 0x7FFFFFFF) |
                                               (this.VolumeExtension.RootHashEntry.LevelAsUint & 0x80000000);
                    newHashEntry.LevelAsUint = (this.VolumeExtension.RootHashEntry.LevelAsUint & 0x40000000) |
                                               (newHashEntry.LevelAsUint & 0xFFFFFFFF);

                    hashBlock.NumberOfCommittedBlocks = this.VolumeExtension.NumberOfTotalBlocks;
                    hashBlock.Save();

                    this.VolumeExtension.RootHashEntry.Hash =
                        XeCrypt.XeCryptSha(this.VolumeExtension.BlockCache.Data[hashBlockCacheIndex], null, null);

                    this.StfsDereferenceBlock(hashBlockCacheIndex);

                    this.VolumeExtension.RootHashEntry.LevelAsUint &= 0x3FFFFFFF;
                    this.VolumeExtension.RootHashEntry.LevelAsUint |= 0x80000000;
                    //this._volumeExtension.RootHashEntry.Level0.State = StfsHashEntryLevel0State.Allocated;

                    NewLevel++;
                }
            }

            // increment the block count in the volume descriptor
            this.VolumeExtension.NumberOfTotalBlocks = TotalAllocBlocks;
            this.VolumeExtension.NumberOfFreeBlocks += AllocationBlocks;

            this.VolumeExtension.RootHashHierarchy = (int)RootHierarchy;

            return 0x000000000;
        }

        internal void StfsEnsureWriteableBlocksAvailable(StfsFcb Fcb, uint Length, uint FileByteOffset)
        {
            if (this.VolumeExtension.ReadOnly)
            {
                throw new StfsException(
                    "Called a write function on a read-only device [Function: To ensure writeable blocks].");
            }

            if (Length == 0)
            {
                throw new StfsException("Detected an invalid length to ensure writeable blocks for.");
            }

            uint TotalBlocks = RoundToBlock(FileByteOffset + Length), BlockOffset = FileByteOffset & 0xFFFFF000;

            uint BlocksToAllocate = 0;

            if (TotalBlocks <= BlockOffset)
            {
                throw new StfsException("The file offset was farther than the file's length.");
            }

            if (TotalBlocks > Fcb.AllocationBlocks)
            {
                BlocksToAllocate = (TotalBlocks - Fcb.AllocationBlocks) / 0x1000;
                TotalBlocks = Fcb.AllocationBlocks;
            }

            if (TotalBlocks > Fcb.ValidAllocBlocks)
            {
                TotalBlocks = Fcb.ValidAllocBlocks;
            }

            if (BlockOffset > TotalBlocks)
            {
                BlockOffset = TotalBlocks;
            }

            if (BlocksToAllocate != 0 || (TotalBlocks - BlockOffset) != 0x1000)
            {
                if ((((TotalBlocks - BlockOffset) >> 12) + this.VolumeExtension.DirectoryAllocationBlockCount) +
                    BlocksToAllocate > this.VolumeExtension.NumberOfTotalBlocks)
                {
                    uint BlockPosition = Fcb.BlockPosition,
                         BytesRead = Fcb.ContiguousBytesRead,
                         LastUnContigBlockNum = Fcb.LastUnContiguousBlockNum;
                    uint BlockNumber = 0, returnedRunFileLength = 0;
                    uint NewOffset = BlockOffset;
                    do
                    {
                        if (TotalBlocks < BlockOffset)
                        {
                            int hashBlockCacheIndex = -1;

                            BlockNumber = this.StfsByteOffsetToBlockNumber(Fcb, BlockOffset, ref returnedRunFileLength);

                            StfsHashEntryLevel0State State;
                            using (
                                var hashBlock = this.StfsMapReadableHashBlock(BlockNumber, 0, ref hashBlockCacheIndex))
                            {
                                State = hashBlock.RetrieveHashEntry((int)BlockNumber).Level0.State;

                                if ((this.StfsBlockCacheElementFromBlock(hashBlockCacheIndex).State & 4) == 0 &&
                                    State == StfsHashEntryLevel0State.Pending)
                                {
                                    State = StfsHashEntryLevel0State.Allocated;
                                }
                            }

                            if (State != StfsHashEntryLevel0State.Allocated)
                            {
                                if (State != StfsHashEntryLevel0State.Pending)
                                {
                                    throw new StfsException(string.Format(
                                        "trying to update unallocated block 0x{0:x8}.", BlockNumber));
                                }
                                else if (BlockOffset == NewOffset)
                                {
                                    LastUnContigBlockNum = Fcb.LastUnContiguousBlockNum;
                                    BytesRead = Fcb.ContiguousBytesRead;
                                    BlockPosition = Fcb.BlockPosition;
                                }
                            }
                            else
                            {
                                TotalBlocks++;
                                if (BlockOffset == NewOffset)
                                {
                                    if (NewOffset < BlockPosition)
                                    {
                                        BlockPosition = Fcb.BlockPosition;
                                        BytesRead = Fcb.ContiguousBytesRead;
                                        LastUnContigBlockNum = Fcb.LastUnContiguousBlockNum;
                                    }
                                }
                            }
                            this.StfsDereferenceBlock(hashBlockCacheIndex);
                            BlockOffset += 0x1000;
                        }
                        else
                        {
                            if (Fcb.BlockPosition != BlockPosition)
                            {
                                Fcb.BlockPosition = BlockPosition;
                                Fcb.ContiguousBytesRead = BytesRead;
                                Fcb.LastUnContiguousBlockNum = LastUnContigBlockNum;
                            }

                            if (BlocksToAllocate != 0)
                            {
                                uint FirstAlloc = 0xffffff, LastAlloc = 0xffffff;
                                this.StfsAllocateBlocks(BlocksToAllocate, 3, 0, 0, ref FirstAlloc, ref LastAlloc);
                            }

                            break;
                        }
                    } while (true);
                }
            }
        }

        internal uint StfsSetAllocationSize(StfsFcb Fcb, uint AllocationLength, bool DisableTruncaction)
        {
            if (this.VolumeExtension.ReadOnly)
            {
                throw new StfsException("Attempted to extend allocation for a file using a read-only device.");
            }

            if ((Fcb.State & 2) == 0)
            {
                if (Fcb.Filesize > Fcb.AllocationBlocks)
                {
                    throw new StfsException("The FCB's filesize was greater than it's allocated blocks.");
                }
            }
            if (AllocationLength > 0xFFFFF000)
            {
                return 0xC000007F;
            }

            uint roundedSize = (AllocationLength + 0xFFF) & 0xFFFFF000;
            if (roundedSize != Fcb.AllocationBlocks)
            {
                if ((Fcb.State & 0x24) == 0)
                {
                    this.StfsEnsureWriteableDirectoryEntry(Fcb);
                }
            }

            if (roundedSize != Fcb.AllocationBlocks)
            {
                if (roundedSize != 0)
                {
                    if (Fcb.AllocationBlocks >= roundedSize)
                    {
                        if (!DisableTruncaction && Fcb.AllocationBlocks > roundedSize)
                        {
                            this.StfsTruncateFileAllocation(Fcb, roundedSize);
                        }
                    }
                    else
                    {
                        this.StfsExtendFileAllocation(Fcb, roundedSize);
                    }
                }
                else
                {
                    this.StfsDeleteFileAllocation(Fcb);
                }
            }
            return 0x00000000;
        }

        internal void StfsDeleteFileAllocation(StfsFcb Fcb)
        {
            if (this.VolumeExtension.ReadOnly)
            {
                throw new StfsException(
                    string.Format("Attempted to delete a file's [{0}] allocation on a read-only device.", Fcb.FileName));
            }

            if ((Fcb.State & 2) != 0)
            {
                throw new StfsException(string.Format("Attempted to delete a folder's [{0}] allocation.", Fcb.FileName));
            }

            if (Fcb.AllocationBlocks == 0)
            {
                throw new StfsException(
                    string.Format("Attempted to delete a file's [{0}] allocation with zero allocation blocks.",
                                  Fcb.FileName));
            }

            StfHashEntry hashEntry = null;

            this.StfsFreeBlocks(Fcb.FirstBlockNumber, false, ref hashEntry);

            Fcb.FirstBlockNumber = 0;
            Fcb.LastBlockNumber = 0xffffffff;
            Fcb.AllocationBlocks = 0;
            Fcb.Filesize = 0;
            Fcb.ValidAllocBlocks = 0;
            Fcb.BlockPosition = 0xffffffff;

            if ((Fcb.State & 0x20) == 0)
            {
                throw new StfsException(
                    string.Format("Detected an invalid FCB state while deleting a file's [{0}] allocation.",
                                  Fcb.FileName));
            }

            Fcb.State |= 0x10;
        }

        internal void StfsExtendFileAllocation(StfsFcb Fcb, uint AllocationSize)
        {
            if (this.VolumeExtension.ReadOnly)
            {
                throw new StfsException("Attempted to extend a file's size with a read-only device.");
            }

            if (AllocationSize == 0 || (AllocationSize & 0xfff) != 0)
            {
                throw new StfsException(
                    string.Format(
                        "Attempted to extend a file's length by an invalid number of bytes [Count: 0x{0:0X}].",
                        (AllocationSize & 0xfff)));
            }

            if (Fcb.AllocationBlocks >= AllocationSize)
            {
                throw new StfsException(
                    "The allocation size supplied for a file extension is less than, or equal to, the file's current allocation size.");
            }

            uint blockNumber = Fcb.LastBlockNumber, retFileLength = 0, oldAllocBlocks = Fcb.AllocationBlocks;
            if (Fcb.AllocationBlocks != 0 && Fcb.LastBlockNumber == 0xffffffff)
            {
                blockNumber = this.StfsByteOffsetToBlockNumber(Fcb, Fcb.AllocationBlocks - 1, ref retFileLength);
                Fcb.LastBlockNumber = blockNumber;
            }
            uint allocBlocks = (AllocationSize - Fcb.AllocationBlocks) / 0x1000,
                 firsAllocBlockNum = 0,
                 lastAllocBlockNum = 0;
            uint allocBlockType = 0xff;

            if ((Fcb.State & 4) == 0)
            {
                if ((Fcb.State & 2) != 0)
                {
                    throw new StfsException(
                        string.Format("Invalid FCB state detected while extending a file's [{0}] allocation size.",
                                      Fcb.FileName));
                }
                allocBlockType = 0;
            }
            else
            {
                if (allocBlocks != 1)
                {
                    throw new StfsException(
                        string.Format("Invalid allocation block count detected for {0} with state 0x{1:0x}.",
                                      Fcb.FileName, Fcb.State));
                }
                allocBlockType = 1;
            }

            this.StfsAllocateBlocks(allocBlocks, allocBlockType, blockNumber, 0xffffffff, ref firsAllocBlockNum,
                                    ref lastAllocBlockNum);

            Fcb.LastBlockNumber = lastAllocBlockNum;
            Fcb.AllocationBlocks = AllocationSize;

            if (oldAllocBlocks == 0)
            {
                Fcb.FirstBlockNumber = firsAllocBlockNum;

                if ((lastAllocBlockNum - firsAllocBlockNum) + 1 == allocBlocks)
                {
                    Fcb.ContiguousBytesRead = AllocationSize;
                    Fcb.LastUnContiguousBlockNum = firsAllocBlockNum;
                    Fcb.BlockPosition = 0;
                }
            }
            if ((Fcb.State & 4) != 0)
            {
                this.VolumeExtension.DirectoryAllocationBlockCount++;
            }
            else
            {
                if ((Fcb.State & 0x20) == 0)
                {
                    throw new StfsException(
                        string.Format("Invalid FCB state found while extending {0}'s allocation size.", Fcb.FileName));
                }
                Fcb.State |= 0x10;
            }
        }

        internal void StfsTruncateFileAllocation(StfsFcb Fcb, uint AllocationSize)
        {
            if (this.VolumeExtension.ReadOnly)
            {
                throw new StfsException("Attempted to truncate a file with a read-only device.");
            }

            if (AllocationSize == 0)
            {
                throw new StfsException("Attempted to truncate a file to zero bytes.");
            }
            else if ((AllocationSize & 0xfff) != 0)
            {
                throw new StfsException("Detected an invalid allocation size supplied for truncation.");
            }

            if (Fcb.IsDirectory)
            {
                throw new StfsException("Attempted to truncate a directory.");
            }

            uint retFileLength = 0xffffffff;
            StfHashEntry hashEntry = null;

            uint blockNum = this.StfsByteOffsetToBlockNumber(Fcb, AllocationSize - 1, ref retFileLength);

            this.StfsFreeBlocks(blockNum, true, ref hashEntry);

            Fcb.LastBlockNumber = blockNum;
            Fcb.AllocationBlocks = AllocationSize;
            Fcb.BlockPosition = 0xffffffff;

            if (Fcb.Filesize > AllocationSize)
            {
                Fcb.Filesize = AllocationSize;
            }
            if (Fcb.ValidAllocBlocks > AllocationSize)
            {
                Fcb.ValidAllocBlocks = AllocationSize;
            }

            if ((Fcb.State & 0x20) == 0)
            {
                throw new StfsException("Invalid FCB state detected while truncating file.");
            }
            Fcb.State |= 0x10;
        }

        private void StfsSetDispositionInformation(StfsFcb fcb, FileDispositionInformation dispositionInformation)
        {
            if (dispositionInformation.DeleteFile)
            {
                if (fcb.IsDirectory)
                {
                    var directoryEntry = new StfsDirectoryEntry();
                    if (StfsFindNextDirectoryEntry(fcb, 0x00, null, ref directoryEntry) != 0xC0000034)
                        return;
                }
                if (StfsEnsureWriteableDirectoryEntry(fcb) == 0)
                {
                    fcb.State |= 0x08;
                }
            }
            else
            {
                fcb.State &= 0xF7;
            }
        }

        private void StfsSetInAllocationSupport(bool inAllocationSupport)
        {
            this.VolumeExtension.InAllocationSupport = inAllocationSupport;
            if (inAllocationSupport == false && this.VolumeExtension.BlockCacheElementCount != 0)
            {
                for (var x = 0; x < this.VolumeExtension.BlockCacheElementCount; x++)
                {
                    this.VolumeExtension.ElementCache.RetrieveElement(x).State &= 0xF7;
                }
            }
        }

        private uint StfsByteOffsetToBlockNumber(StfsFcb Fcb, long Offset, ref uint ReturnedFileRunLength)
        {
            uint blockOffset = (uint)Offset & 0xFFF;
            Offset &= 0xFFFFF000;

            if (Offset > Fcb.AllocationBlocks)
            {
                throw new StfsException("Attempted to seek beyond valid, allocated blocks. [0xC0000011].");
            }

            uint BlockNum = 0xFFFFFF,
                 OldBlockNum = 0,
                 NewBlockNum = 0,
                 ContiguousBytesRead = Fcb.ContiguousBytesRead,
                 LastUnContiguousBlockNum = Fcb.LastUnContiguousBlockNum,
                 RecBlockNum = 0,
                 OtherBlockNum = 0;

            int CacheIndex = 0;
            uint BlockPosition = Fcb.BlockPosition, AllocBlocksRead = 0;

            BlockNum = Fcb.FirstBlockNumber;

            StfsHashBlock hashBlock = null;
            StfHashEntry hashEntry = null;

            if (Offset >= Fcb.BlockPosition)
            {
                if (Offset < (Fcb.ContiguousBytesRead + Fcb.BlockPosition))
                {
                    ReturnedFileRunLength =
                        (uint)((Fcb.ContiguousBytesRead - (Offset - Fcb.BlockPosition)) - blockOffset);
                    return (uint)(((Offset - Fcb.BlockPosition) / 0x1000) + Fcb.LastUnContiguousBlockNum);
                }
            }
            if (Offset == 0)
            {
                ReturnedFileRunLength = 0x1000 - blockOffset;
                return Fcb.FirstBlockNumber;
            }
            else
            {
                if ((Offset + 0x1000) == Fcb.AllocationBlocks)
                {
                    if (Fcb.LastBlockNumber != 0xffffffff)
                    {
                        ReturnedFileRunLength = 0x1000 - blockOffset;
                        return Fcb.LastBlockNumber;
                    }
                }

                if (Offset <= Fcb.BlockPosition)
                {
                    AllocBlocksRead = 0;
                    BlockPosition = 0;
                    ContiguousBytesRead = 0x1000;
                    BlockNum = Fcb.FirstBlockNumber;
                    LastUnContiguousBlockNum = Fcb.FirstBlockNumber;
                }
                else
                {
                    AllocBlocksRead = (uint)((Fcb.ContiguousBytesRead + Fcb.BlockPosition) - 0x1000);
                    BlockNum = ((Fcb.ContiguousBytesRead / 0x1000) + Fcb.LastUnContiguousBlockNum) - 1;
                }
                do
                {
                    OldBlockNum = BlockNum % 0xAA;
                    NewBlockNum = BlockNum - OldBlockNum;

                    if (hashBlock == null)
                    {
                        hashBlock = this.StfsMapReadableHashBlock(NewBlockNum, 0, ref CacheIndex);

                        RecBlockNum = NewBlockNum;
                    }

                    else if (RecBlockNum != NewBlockNum)
                    {
                        this.StfsDereferenceBlock(CacheIndex);

                        hashBlock.Dispose();

                        hashBlock = this.StfsMapReadableHashBlock(NewBlockNum, 0, ref CacheIndex);

                        RecBlockNum = NewBlockNum;
                    }

                    hashEntry = hashBlock.RetrieveHashEntry((int)OldBlockNum);

                    if (hashEntry.Level0.NextBlockNumber > this.VolumeExtension.NumberOfTotalBlocks)
                    {
                        throw new StfsException(string.Format("reference to illegal block number 0x{0} [0xC0000032].",
                                                              hashEntry.Level0.NextBlockNumber.ToString("X")));
                    }

                    AllocBlocksRead += 0x1000;

                    if ((BlockNum + 1) != hashEntry.Level0.NextBlockNumber)
                    {
                        BlockPosition = AllocBlocksRead;
                        ContiguousBytesRead = 0x1000;
                        LastUnContiguousBlockNum = hashEntry.Level0.NextBlockNumber;
                    }
                    else
                    {
                        ContiguousBytesRead += 0x1000;
                    }

                    BlockNum = hashEntry.Level0.NextBlockNumber;
                } while (Offset != AllocBlocksRead);
            }

            uint TotalBlocks = AllocBlocksRead + 0x1000, PreviousBlockNumber = BlockNum;

            do
            {
                if (TotalBlocks != Fcb.AllocationBlocks)
                {
                    OtherBlockNum = PreviousBlockNumber - NewBlockNum;
                    if (OtherBlockNum < 0xA9)
                    {
                        var CurrentBlockNumber = hashBlock.RetrieveHashEntry((int)OtherBlockNum).Level0.NextBlockNumber;

                        if (CurrentBlockNumber >= this.VolumeExtension.NumberOfTotalBlocks)
                        {
                            throw new StfsException(
                                string.Format("reference to illegal block number 0x{0:0x} [0xC0000032].",
                                              hashEntry.Level0.NextBlockNumber.ToString("X")));
                        }

                        if ((PreviousBlockNumber + 1) == CurrentBlockNumber)
                        {
                            TotalBlocks += 0x1000;
                            ContiguousBytesRead += 0x1000;
                            PreviousBlockNumber = CurrentBlockNumber;
                        }
                        else break;
                    }
                    else break;
                }
                else
                {
                    Fcb.LastBlockNumber = PreviousBlockNumber;
                    break;
                }
            } while (true);

            //if (hashBlock != null)
            //{
                hashBlock.Dispose();

                this.StfsDereferenceBlock(CacheIndex);
            //}

            if (BlockNum == 0xffffff)
            {
                throw new StfsException("Invalid block number was to be returned from StfsByteOffsetToBlockNumber.");
            }

            ReturnedFileRunLength = (uint)((TotalBlocks - blockOffset) - Offset);

            Fcb.BlockPosition = BlockPosition;
            Fcb.ContiguousBytesRead = ContiguousBytesRead;
            Fcb.LastUnContiguousBlockNum = LastUnContiguousBlockNum;

            return BlockNum;
        }

        private void StfsFullyCachedRead(StfsFcb Fcb, uint FileByteOffset, uint Length, EndianIO writer)
        {
            if (Length == 0)
                throw new StfsException("Requested a cached read with an invalid length of bytes.");

            if (FileByteOffset > Fcb.AllocationBlocks)
                throw new StfsException("Requested a cached read at an invalid starting position.");

            if ((FileByteOffset + Length) > Fcb.Filesize)
                throw new StfsException(string.Format("Requested a cached read larger than available from the file {0}.", Fcb.FileName));

            uint num2 = 0, ReturnedFileRunLength = 0;
            int BlockCache = -1;
            do
            {
                uint num = FileByteOffset & 0xfff;
                num2 = ~num + 0x1000 + 1;

                if (num2 > Length)
                    num2 = Length;

                writer.Write(
                    this.StfsMapReadableDataBlock(
                        this.StfsByteOffsetToBlockNumber(Fcb, FileByteOffset, ref ReturnedFileRunLength), ref BlockCache), (int)(FileByteOffset % 0x1000), (int)num2);

                this.StfsDereferenceBlock(BlockCache);

                FileByteOffset += num2;
            } while ((Length -= num2) != 0);
        }

        internal void StfsPartiallyCachedRead(StfsFcb Fcb, uint FileByteOffset, uint Length, EndianIO writer)
        {
            uint fullCacheLength = 0, remainder = (FileByteOffset & 0xfff);
            uint ReturnedFileRunLength = 0;

            if (remainder != 0)
            {
                fullCacheLength = ~remainder + 0x1000 + 1;
                if (fullCacheLength < Length)
                {
                    this.StfsFullyCachedRead(Fcb, FileByteOffset, fullCacheLength, writer);
                }
                else
                {
                    throw new StfsException("Invalid partial cache read request [Remainder larger than length].");
                }
                Length -= fullCacheLength;
                FileByteOffset += fullCacheLength;
            }

            if (Length < 0x1000)
            {
                throw new StfsException("Invalid partial cache read request [Invalid read-remainder size].");
            }
            do
            {
                if (FileByteOffset >= Fcb.Filesize || FileByteOffset >= Fcb.ValidAllocBlocks ||
                    (FileByteOffset + Length) > Fcb.Filesize || (FileByteOffset + Length) > Fcb.ValidAllocBlocks)
                {
                    throw new StfsException("Attempted to read beyond end-of-file.");
                }

                uint blockNum = this.StfsByteOffsetToBlockNumber(Fcb, FileByteOffset, ref ReturnedFileRunLength);
                int BlockCacheIndex = -1;
                if (!this.StfsMapExistingDataBlock(blockNum, ref BlockCacheIndex))
                {
                    if (Length >= 0x1000)
                    {
                        uint nonCacheRemainder = (Length & 0xFFFFF000);
                        if (nonCacheRemainder < 0x1000)
                        {
                            throw new StfsException(
                                "Invalid partial cache read request [Invalid read-remainder block length].");
                        }
                        this.StfsNonCachedRead(Fcb, FileByteOffset, nonCacheRemainder, writer);

                        Length -= nonCacheRemainder;
                        FileByteOffset += nonCacheRemainder;
                    }
                    break;
                }

                writer.Write(this.VolumeExtension.BlockCache.Data[BlockCacheIndex]);

                this.StfsDereferenceBlock(BlockCacheIndex);

                FileByteOffset += 0x1000;
            } while ((Length -= 0x1000) >= 0x1000);

            if (Length != 0)
            {
                this.StfsFullyCachedRead(Fcb, FileByteOffset, Length, writer);
            }
        }

        internal void StfsNonCachedRead(StfsFcb Fcb, uint FileByteOffset, uint Length, EndianIO writer)
        {
            int CacheIndex = -1;
            uint ReturnedFileRunLength = 0;
            do
            {
                uint BlockNum = this.StfsByteOffsetToBlockNumber(Fcb, FileByteOffset, ref ReturnedFileRunLength);

                uint NewBlockNum = (~(BlockNum % 0xAA) + 0xAA + 1) << 12;

                if (ReturnedFileRunLength > NewBlockNum)
                {
                    ReturnedFileRunLength = NewBlockNum;
                }
                if (ReturnedFileRunLength > Length)
                {
                    ReturnedFileRunLength = Length;
                }
                if (ReturnedFileRunLength > 0x1000)
                {
                    ReturnedFileRunLength = 0x1000;
                }

                if (!this.VolumeExtension.ReadOnly)
                {
                    this.StfsFlushBlockCache(BlockNum, (ReturnedFileRunLength / 0x1000) + BlockNum - 1);
                }

                var hashBlock = this.StfsMapReadableHashBlock(BlockNum, 0, ref CacheIndex);

                byte[] Data = this.StfsSynchronousReadFile(this.StfsComputeBackingDataBlockNumber(BlockNum),
                                                           ReturnedFileRunLength, 0);

                writer.Write(Data);

                int TempCopyIndex = 0;
                byte[] TempBlock = new byte[Block];

                Length -= ReturnedFileRunLength;
                FileByteOffset += ReturnedFileRunLength;

                do
                {
                    Array.Copy(Data, TempCopyIndex, TempBlock, 0, 0x1000);

#if DEBUG && HASH_MISMATCH
                    {
                        if (
                            !ArrayEquals(XeCrypt.XeCryptSha(TempBlock, null, null),
                                                      hashBlock.RetrieveHashEntry(BlockNum).Hash))
                        {
                            throw new StfsException(
                                string.Format("hash mismatch for block number 0x{0:x8} [0xC0000032].", BlockNum));
                        }
                    }
#endif

                    TempCopyIndex += 0x1000;
                    BlockNum++;
                } while ((ReturnedFileRunLength -= 0x1000) != 0);

                this.StfsDereferenceBlock(CacheIndex);

                hashBlock.Dispose();
            } while (Length != 0);
        }

        internal bool StfsTestForFullyCachedIo(StfsFcb Fcb, long FileByteOffset, long Length)
        {
            if (Length == 0)
            {
                throw new StfsException("Detected an invalid amount of bytes for reading/writing.");
            }

            if (Length >= 0x1000)
            {
                if ((((FileByteOffset + Length) ^ (FileByteOffset + 0xFFF)) & 0xFFFFF000) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        internal void StfsFullyCachedWrite(StfsFcb Fcb, uint FileByteOffset, uint Length, byte[] DataBuffer)
        {
            if (Length == 0)
                throw new StfsException("Attempted to write an invalid amount of bytes.");

            uint num = 0, blockLength = 0, returnedBlockNumber = 0xffffffff, bufferIndex = 0;

            StfsHashBlock hashBlock = null;

            byte[] DataBlock = new byte[0x1000];

            int dataBlockCacheIndex = -1, hashBlockCacheIndex = -1;

            do
            {
                num = FileByteOffset & 0xfff;
                blockLength = ~num + 0x1000 + 1;

                if (blockLength > Length)
                    blockLength = Length;

                this.StfsBeginDataBlockUpdate(Fcb, ref hashBlock, ref hashBlockCacheIndex, FileByteOffset, ref DataBlock,
                                              ref dataBlockCacheIndex, (blockLength - 0x1000) == 0,
                                              ref returnedBlockNumber);

                Array.Copy(DataBuffer, bufferIndex, DataBlock, num, blockLength);

                this.StfsEndDataBlockUpdate(DataBlock, dataBlockCacheIndex, returnedBlockNumber, ref hashBlock,
                                            hashBlockCacheIndex);

                if (Fcb.ValidAllocBlocks <= FileByteOffset)
                {
                    Fcb.ValidAllocBlocks += 0x1000;
                }

                FileByteOffset += blockLength;
                bufferIndex += blockLength;
            } while ((Length -= blockLength) != 0);
        }

        internal void StfsPartiallyCachedWrite(StfsFcb Fcb, uint FileByteOffset, uint Length, byte[] Buffer)
        {
            var reader = new EndianIO(new MemoryStream(Buffer, false), EndianType.Big);

            uint remainder = (FileByteOffset & 0xfff);
            if (remainder != 0)
            {
                remainder = 0x1000 - remainder;

                this.StfsFullyCachedWrite(Fcb, FileByteOffset, remainder, reader.ReadByteArray((int)remainder));

                Length -= remainder;
                FileByteOffset += remainder;
            }
            uint nonCacheRemainder = (Length & 0xFFFFF000);

            this.StfsNonCachedWrite(Fcb, FileByteOffset, reader.ReadByteArray((int)nonCacheRemainder), nonCacheRemainder);

            FileByteOffset += (Length & 0xFFFFF000);

            remainder = Length - nonCacheRemainder;

            if (remainder != 0)
            {
                this.StfsFullyCachedWrite(Fcb, FileByteOffset, remainder, reader.ReadByteArray((int)remainder));
            }

            reader.Close();
        }

        internal void StfsNonCachedWrite(StfsFcb Fcb, uint FileByteOffset, byte[] Buffer, uint Length)
        {
            var reader = new EndianIO(new MemoryStream(Buffer, false), EndianType.Big);

            if (Length == 0)
            {
                throw new StfsException("Attempted to write a zero bytes [Non-cached write].");
            }

            uint BlockIndex = 0, finalBlock = 0xffffffff;

            MemoryStream ms = new MemoryStream();

            var writer = new EndianIO(ms, EndianType.Big);

            do
            {
                StfsHashBlock hashBlock = null;
                byte[] DataBlock = null;
                int hashBlockCacheIndex = -1, dataBlockCacheIndex = -1;
                uint retBlockNum = 0xffffffff;

                this.StfsBeginDataBlockUpdate(Fcb, ref hashBlock, ref hashBlockCacheIndex, FileByteOffset, ref DataBlock,
                                              ref dataBlockCacheIndex, true, ref retBlockNum);

                byte[] data = reader.ReadByteArray(0x1000);
                hashBlock.SetHashForEntry(retBlockNum, XeCrypt.XeCryptSha(data, null, null));

                hashBlock.Save();
                this.StfsDereferenceBlock(hashBlockCacheIndex);

                this.StfsDiscardBlock(retBlockNum, 0);

                uint BlockNum = this.StfsComputeBackingDataBlockNumber(retBlockNum);


                if (BlockIndex != 0)
                {
                    if ((finalBlock + BlockIndex) != BlockNum)
                    {
                        this.StfsSynchronousWriteFile(finalBlock, 0, ms.ToArray(), BlockIndex * 0x1000);

                        writer.Stream.SetLength(0);

                        BlockIndex = 1;
                        finalBlock = BlockNum;
                    }
                    else
                    {
                        BlockIndex++;
                    }
                }
                else
                {
                    BlockIndex = 1;
                    finalBlock = BlockNum;
                }

                writer.Write(data);
                writer.Flush();


                if (Fcb.ValidAllocBlocks <= FileByteOffset)
                {
                    Fcb.ValidAllocBlocks += 0x1000;
                }

                FileByteOffset += 0x1000;
            } while ((Length -= 0x1000) != 0);

            if (finalBlock != 0xffffffff && BlockIndex != 0)
            {
                this.StfsSynchronousWriteFile(finalBlock, 0, ms.ToArray(), BlockIndex * 0x1000);
            }

            writer.Close();
            reader.Close();
        }

        internal void StfsBeginDataBlockUpdate(StfsFcb Fcb, ref StfsHashBlock HashBlock, ref int hashBlockCacheIndex,
                                             uint FileByteOffset, ref byte[] ReturnedDataBlock,
                                             ref int DataBlockCacheIndex, bool MapEmptyDataBlock,
                                             ref uint ReturnedBlockNumber)
        {
            uint ByteOffset = FileByteOffset & 0xFFFFF000, BlockNumber = 0xffffffff, ReturnedFileRunLength = 0;
            uint StartLinkBlockNum = 0xffffffff;

            if (ByteOffset < Fcb.AllocationBlocks)
            {
                BlockNumber = this.StfsByteOffsetToBlockNumber(Fcb, ByteOffset, ref ReturnedFileRunLength);

                if (BlockNumber > this.VolumeExtension.NumberOfTotalBlocks)
                {
                    throw new StfsException(
                        string.Format("Could not calculate a block number for file '{0}' at offset 0x{1:x8}.",
                                      Fcb.FileName, ByteOffset));
                }

                HashBlock = this.StfsMapWriteableHashBlock(BlockNumber, 0, false, ref hashBlockCacheIndex);
                StfHashEntry hashEntry = HashBlock.RetrieveHashEntry((int)BlockNumber);
                if (ByteOffset != Fcb.ValidAllocBlocks)
                {
                    if (hashEntry.Level0.State != StfsHashEntryLevel0State.Pending)
                    {
                        this.StfsDereferenceBlock(hashBlockCacheIndex);

                        if (hashEntry.Level0.State == StfsHashEntryLevel0State.Allocated)
                        {
                            if (ByteOffset != 0)
                            {
                                uint prevBlock = ByteOffset - 0x1000;
                                if (prevBlock < Fcb.BlockPosition ||
                                    prevBlock >= (Fcb.BlockPosition + Fcb.ContiguousBytesRead))
                                {
                                    StartLinkBlockNum = this.StfsByteOffsetToBlockNumber(Fcb, prevBlock,
                                                                                         ref ReturnedFileRunLength);
                                }
                                else
                                {
                                    StartLinkBlockNum = ((prevBlock - Fcb.BlockPosition) / 0x1000) +
                                                        Fcb.LastUnContiguousBlockNum;
                                }
                            }
                            uint AllocBlocksType = 0,
                                 ReturnedFirstAllocatedBlockNumber = 0,
                                 ReturnedLastAllocatedBlockNumber = 0;
                            if ((Fcb.State & 4) != 0)
                            {
                                if (_volumeDescriptor.DirectoryAllocationBlocks != 0)
                                {
                                    AllocBlocksType = 2;
                                }
                                else
                                {
                                    throw new StfsException("Invalid directory allocation block count detected.");
                                }
                            }

                            this.StfsAllocateBlocks(1, AllocBlocksType, StartLinkBlockNum,
                                                    hashEntry.Level0.NextBlockNumber,
                                                    ref ReturnedFirstAllocatedBlockNumber,
                                                    ref ReturnedLastAllocatedBlockNumber);

                            if (BlockNumber == Fcb.FirstBlockNumber)
                            {
                                Fcb.FirstBlockNumber = ReturnedFirstAllocatedBlockNumber;
                            }

                            if (BlockNumber == Fcb.LastBlockNumber)
                            {
                                Fcb.LastBlockNumber = ReturnedLastAllocatedBlockNumber;
                            }

                            if ((Fcb.BlockPosition + Fcb.ContiguousBytesRead) != ByteOffset ||
                                ((Fcb.ContiguousBytesRead / 0x1000) + Fcb.LastUnContiguousBlockNum) !=
                                ReturnedFirstAllocatedBlockNumber)
                            {
                                Fcb.BlockPosition = ByteOffset;
                                Fcb.LastUnContiguousBlockNum = ReturnedFirstAllocatedBlockNumber;
                                Fcb.ContiguousBytesRead = 0x1000;
                            }
                            else
                            {
                                Fcb.ContiguousBytesRead += 0x1000;
                            }

                            if ((Fcb.State & 4) == 0)
                            {
                                if ((Fcb.State & 0x20) != 0)
                                {
                                    Fcb.State |= 0x10;
                                }
                                else
                                {
                                    throw new StfsException(
                                        "Invalid Fcb state detected while beginning data block update.");
                                }
                            }
                            else
                            {
                                this.VolumeExtension.DirectoryAllocationBlockCount += 0xFFFF;
                            }

                            StfHashEntry FreeSingleHashEntry = new StfHashEntry();
                            this.StfsFreeBlocks(BlockNumber, false, ref FreeSingleHashEntry);

                            if (ReturnedDataBlock != null)
                            {
                                if (MapEmptyDataBlock)
                                {
                                    this.StfsMapEmptyDataBlock(ReturnedFirstAllocatedBlockNumber, ref ReturnedDataBlock,
                                                               ref DataBlockCacheIndex);
                                }
                                else
                                {
                                    ReturnedDataBlock =
                                        this.StfsMapWriteableCopyOfDataBlock(ReturnedFirstAllocatedBlockNumber,
                                                                             BlockNumber, ref FreeSingleHashEntry,
                                                                             ref DataBlockCacheIndex);
                                }
                            }
                            if (!MapEmptyDataBlock)
                            {
                                if ((Fcb.State & 4) != 0)
                                {
                                    this.StfsResetWriteableDirectoryBlock(ByteOffset, ReturnedDataBlock);
                                }
                            }

                            if (ReturnedFirstAllocatedBlockNumber > this.VolumeExtension.NumberOfTotalBlocks)
                            {
                                throw new StfsException(
                                    string.Format("Failed to allocate a new block while updating {0}.", Fcb.FileName));
                            }

                            HashBlock = this.StfsMapWriteableHashBlock(ReturnedFirstAllocatedBlockNumber, 0, false,
                                                                       ref hashBlockCacheIndex);
                            ReturnedBlockNumber = ReturnedFirstAllocatedBlockNumber;
                        }
                        else
                        {
                            throw new StfsException(
                                string.Format("trying to update unallocated block 0x{0:x8} [0xC0000032].", BlockNumber));
                        }
                    }
                    else
                    {
                        if (ReturnedDataBlock != null)
                        {
                            if (MapEmptyDataBlock)
                            {
                                this.StfsMapEmptyDataBlock(BlockNumber, ref ReturnedDataBlock, ref DataBlockCacheIndex);
                            }
                            else
                            {
                                ReturnedDataBlock = this.StfsMapWriteableDataBlock(BlockNumber, hashEntry,
                                                                                   ref DataBlockCacheIndex);
                            }
                        }
                        ReturnedBlockNumber = BlockNumber;
                    }
                }
                else
                {
                    if (hashEntry.Level0.State == StfsHashEntryLevel0State.Allocated ||
                        hashEntry.Level0.State == StfsHashEntryLevel0State.Pending)
                    {
                        if (ReturnedDataBlock != null)
                        {
                            this.StfsMapEmptyDataBlock(BlockNumber, ref ReturnedDataBlock,
                                                                 ref DataBlockCacheIndex);
                        }

                        hashEntry.LevelAsUint |= 0xC0000000;
                        //hashEntry.Level0.State = StfsHashEntryLevel0State.Pending;
                        ReturnedBlockNumber = BlockNumber;
                    }
                    else
                    {
                        throw new StfsException(
                            string.Format("trying to update unallocated block 0x{0:x8} [0xC0000032].", BlockNumber));
                    }
                }
            }
            else
            {
                throw new StfsException(
                    string.Format(
                        "The byte offset supplied for a block update was farther than the file's [{0}] allocated block length.",
                        Fcb.FileName));
            }
        }

        public void StfsEndDataBlockUpdate(byte[] DataBlock, int DataBlockCacheIndex, uint BlockNumber,
                                           ref StfsHashBlock HashBlock, int hashBockCacheIndex)
        {
            if (DataBlock.Length != Block) // data blocks in STFS must be 0x1000 in size
                throw new StfsException("Invalid data block buffer length found.");

            // check to make sure the block numbe is not greater than the allocated block count
            if (BlockNumber > this.VolumeExtension.NumberOfTotalBlocks)
                throw new StfsException("Invalid block number found before attempting to update it.");

            // hash the data block we are writing
            HashBlock.SetHashForEntry(BlockNumber, XeCrypt.XeCryptSha(DataBlock, null, null));

            HashBlock.Save();

            this.StfsDereferenceBlock(DataBlockCacheIndex);
            this.StfsDereferenceBlock(hashBockCacheIndex);
        }

        internal StfsHashBlock StfsMapReadableHashBlock(uint BlockNum, int RequestedLevel, ref int CacheIndex)
        {
            int CurrentLevel = 0, NewLevel = 0;
            StfCacheElement element = null;
            StfHashEntry hashEntry = null;

            bool ShouldMapNewHashBlock = false;

            if (BlockNum <= this.VolumeExtension.NumberOfTotalBlocks)
            {
                if (RequestedLevel <= this.VolumeExtension.RootHashHierarchy)
                {
                    CurrentLevel = RequestedLevel + 1;
                    NewLevel = RequestedLevel;
                    if (!this.StfsMapExistingBlock(BlockNum, CurrentLevel, ref CacheIndex, ref element))
                    {
                        do
                        {
                            if (NewLevel == this.VolumeExtension.RootHashHierarchy)
                            {
                                ShouldMapNewHashBlock = true;
                                break;
                            }
                            NewLevel = CurrentLevel;
                            CurrentLevel = NewLevel + 1;
                        } while (!this.StfsMapExistingBlock(BlockNum, CurrentLevel, ref CacheIndex, ref element));

                        if (!ShouldMapNewHashBlock && NewLevel == RequestedLevel)
                        {
                            goto STFS_RETURN_HASHBLOCK;
                        }

                        hashEntry = this.VolumeExtension.RootHashEntry;

                        do
                        {
                            if (!ShouldMapNewHashBlock)
                            {
                                if (NewLevel != RequestedLevel)
                                {
                                    using (var hashBlock = new StfsHashBlock(this.VolumeExtension.BlockCache.Data[CacheIndex]))
                                    {
                                        hashEntry = hashBlock.RetrieveHashEntry((int)(BlockNum / this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[NewLevel - 1]));
                                    }

                                    if ((element.State & 4) == 0)
                                    {
                                        hashEntry.LevelAsUint &= 0x7FFFFFFF;
                                    }

                                    this.StfsDereferenceBlock(CacheIndex);

                                    NewLevel--;
                                }
                            }

                            this.StfsMapNewBlock(BlockNum, NewLevel + 1, hashEntry, ref CacheIndex, ref element);

                            if ((hashEntry.LevelAsUint & 0x80000000) != 0)
                                element.State |= 4;

                            ShouldMapNewHashBlock = false;
                        } while (NewLevel != RequestedLevel);
                    }
                }
            }
        STFS_RETURN_HASHBLOCK:
            return
                new StfsHashBlock(this.VolumeExtension.BlockCache.Data[CacheIndex]);
        }

        private StfsHashBlock StfsMapWriteableHashBlock(uint BlockNumber, int RequestedLevel, bool MapEmptyHashBlock,
                                                        ref int BlockCacheIndex)
        {
            int currentLevel = this.VolumeExtension.RootHashHierarchy, CacheBlockIndex = -1, oldCacheIndex = -1;

            StfHashEntry hashEntry = this.VolumeExtension.RootHashEntry;
            StfsHashBlock hashBlock, oldHashBlock = null;
            StfCacheElement element = null;
            uint blockNum = 0xffffffff;
            do
            {
                if (MapEmptyHashBlock && (currentLevel == RequestedLevel))
                {
                    byte[] dataBlock = null;

                    this.StfsMapNewEmptyBlock(currentLevel + 1, BlockNumber, ref element, ref dataBlock,
                                              ref CacheBlockIndex);

                    hashEntry.LevelAsUint = (hashEntry.LevelAsUint & 0x3FFFFFFF) | 0x80000000;
                }
                else if (!this.StfsMapExistingBlock(BlockNumber, currentLevel + 1, ref CacheBlockIndex, ref element))
                {
                    if (
                        !this.StfsMapNewBlock(BlockNumber, currentLevel + 1, hashEntry, ref CacheBlockIndex, ref element))
                    {
                        throw new StfsException("Failed to map writeable hash block.");
                    }
                }
                hashBlock =
                    new StfsHashBlock(this.VolumeExtension.BlockCache.Data[CacheBlockIndex]);

                if (((hashEntry.LevelAsUint >> 25 ^ element.State) & 0x20) != 0)
                {
                    throw new StfsException("You went on the wrong Paper Trail.");
                }

                if ((hashEntry.LevelAsUint & 0x80000000) == 0)
                {
                    if ((element.State & 0x40) != 0)
                    {
                        throw new StfsException(
                            "Detected an invalid cache element state while mapping a write-able hash block.");
                    }

                    uint notState = (~hashEntry.LevelAsUint >> 30) & 1;
                    uint NewState = (uint)(0x80000000 | ((notState << 30) & 0x40000000));
                    //int elementState = element.State;
                    uint temp = ((uint)(element.State & 0xDF) | (uint)(notState << 5));
                    element.State = (byte)(temp & 0xff);
                    hashEntry.LevelAsUint = (hashEntry.LevelAsUint & 0x3FFFFFFF) | NewState;

                    if (currentLevel == 0)
                    {
                        for (int x = 0; x < 0xaa; x++)
                        {
                            StfHashEntry Level0HashEntry = hashBlock.RetrieveHashEntry(x);
                            switch (Level0HashEntry.Level0.State)
                            {
                                case StfsHashEntryLevel0State.FreedPending:
                                    Level0HashEntry.LevelAsUint &= 0x3FFFFFFF;
                                    //hashEntry.Level0.State = StfsHashEntryLevel0State.Unallocated;
                                    break;
                                case StfsHashEntryLevel0State.Pending:
                                    Level0HashEntry.LevelAsUint &= 0x3FFFFFFF;
                                    Level0HashEntry.LevelAsUint |= 0x80000000;
                                    //hashEntry.Level0.State = StfsHashEntryLevel0State.Allocated;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // Convert free-pending blocks to free blocks
                        for (int x = 0; x < 0xAA; x++)
                        {
                            StfHashEntry LevelNashEntry = hashBlock.RetrieveHashEntry(x);
                            uint level = LevelNashEntry.LevelAsUint;
                            level &= 0x7FFFFFFF;
                            uint pendingBlocks = level >> 15;
                            uint allocState = level & 0xC0000000;
                            level += pendingBlocks;
                            level &= 0x7FFF;
                            level |= allocState;
                            LevelNashEntry.LevelAsUint = level;
                        }
                    }

                    hashBlock.Save();
                }

                if (oldHashBlock != null)
                {
                    oldHashBlock =
                        new StfsHashBlock(this.VolumeExtension.BlockCache.Data[oldCacheIndex]);
                    oldHashBlock.SetLevelForEntry(blockNum, hashEntry.LevelAsUint);
                    oldHashBlock.Save();
                }

                element.State |= 0x44;

                element.State =
                    (byte)
                    (((element.State & ~8) | ((((this.VolumeExtension.InAllocationSupport ? 1 : 0) << 3) & 8))) &
                     0xFF);

                if ((element.State & 0x10) == 0)
                {
                    element.State |= 0x10;
                    if (oldCacheIndex != -1)
                    {
                        this.StfsReferenceBlock(oldCacheIndex);
                    }
                }

                if (oldCacheIndex != -1)
                {
                    this.StfsDereferenceBlock(oldCacheIndex);
                }

                if (RequestedLevel != currentLevel)
                {
                    oldHashBlock = hashBlock;

                    oldCacheIndex = CacheBlockIndex;

                    blockNum = BlockNumber / this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[currentLevel - 1];

                    hashEntry = oldHashBlock.RetrieveHashEntry((int)blockNum);

                    currentLevel--;
                }
                else
                {
                    break;
                }
            } while (true);

            if (this.VolumeExtension.BlockCache.Data[CacheBlockIndex] == null ||
                this.VolumeExtension.BlockCache.Data[CacheBlockIndex].Length == 0)
                throw new StfsException("Could not map a valid writeable hash block.");

            BlockCacheIndex = CacheBlockIndex;

            return
                new StfsHashBlock(this.VolumeExtension.BlockCache.Data[CacheBlockIndex]);
        }

        public byte[] StfsMapReadableDataBlock(uint BlockNumber, ref int BlockCacheIndex)
        {
            if (BlockNumber > this.VolumeExtension.NumberOfTotalBlocks)
                throw new StfsException("Requested block number was outside of range.");

            int CacheIndex = 0;
            StfCacheElement element = null;
            if (!this.StfsMapExistingBlock(BlockNumber, 0, ref BlockCacheIndex, ref element))
            {
                using (var hashBlock = this.StfsMapReadableHashBlock(BlockNumber, 0, ref CacheIndex))
                {
                    if (
                        !this.StfsMapNewBlock(BlockNumber, 0, hashBlock.RetrieveHashEntry((int)BlockNumber),
                                              ref BlockCacheIndex, ref element))
                    {
                        throw new StfsException("Failed to map a readable data block.");
                    }

#if DEBUG && HASH_MISMATCH
                    {
                        if (
                            !ArrayEquals(
                                XeCrypt.XeCryptSha(this.VolumeExtension.BlockCache.Data[BlockCacheIndex], null, null),
                                hashBlock.RetrieveHashEntry(BlockNumber).Hash))
                        {
                            throw new StfsException(
                                string.Format("hash mismatch for block number 0x{0:x8} [0xC0000032].", BlockNumber));
                        }
                    }
#endif
                }

                this.StfsDereferenceBlock(CacheIndex);
            }

            if (this.VolumeExtension.BlockCache.Data[BlockCacheIndex].Length == 0 ||
                this.VolumeExtension.BlockCache.Data[CacheIndex] == null)
            {
                throw new StfsException("Could not map a valid read-able data block.");
            }

            return this.VolumeExtension.BlockCache.Data[BlockCacheIndex];
        }

        public byte[] StfsMapWriteableDataBlock(uint BlockNumber, StfHashEntry hashEntry, ref int DataBlockCacheIndex)
        {
            if (BlockNumber > this.VolumeExtension.NumberOfTotalBlocks)
                throw new StfsException("Requested block number was outside of range.");

            StfCacheElement element = null;

            if (!this.StfsMapExistingBlock(BlockNumber, 0, ref DataBlockCacheIndex, ref element))
            {
                if (!this.StfsMapNewBlock(BlockNumber, 0, hashEntry, ref DataBlockCacheIndex, ref element))
                {
                    throw new StfsException("Failed to map a writeable data block.");
                }
            }
            if (element.BlockNumber != BlockNumber && element.Referenced != 1)
            {
                throw new StfsException("Invalid cache element returned for a mapped block.");
            }
            element.State |= 0x40;

            if (this.VolumeExtension.BlockCache.Data[DataBlockCacheIndex].Length == 0 ||
                this.VolumeExtension.BlockCache.Data[DataBlockCacheIndex] == null)
            {
                throw new StfsException("Could not map a valid write-able data block.");
            }

            return this.VolumeExtension.BlockCache.Data[DataBlockCacheIndex];
        }

        internal byte[] StfsMapWriteableCopyOfDataBlock(uint BlockNumber, uint SourceBlockNumber,
                                                       ref StfHashEntry SourceHashEntry, ref int DataBlockCacheIndex)
        {
            if (this.VolumeExtension.ReadOnly)
            {
                throw new StfsException("Attempted to map a writeable data block on a read-only device.");
            }
            else if (BlockNumber >= this.VolumeExtension.NumberOfTotalBlocks)
            {
                throw new StfsException(
                    "Attempted to map a writeable data block with a block number greater than the total allocated for this device.");
            }

            this.StfsDiscardBlock(BlockNumber, 0);

            int blockCacheIndex = -1;
            StfCacheElement element = null;
            if (!this.StfsMapExistingBlock(SourceBlockNumber, 0, ref blockCacheIndex, ref element))
            {
                if (!this.StfsMapNewBlock(SourceBlockNumber, 0, SourceHashEntry, ref blockCacheIndex, ref element))
                {
                    throw new StfsException("Failed to map a new block [Writeable copy].");
                }
            }
            if (element.BlockNumber != SourceBlockNumber)
            {
                throw new StfsException("Mapped a cache element with a mismatched block number.");
            }
            else if (element.Referenced != 1)
            {
                throw new StfsException("Mapped a cache element with an invalid amount of references.");
            }
            else if ((element.State & 0x40) != 0)
            {
                throw new StfsException("Detected a cache element with an invalid state.");
            }
            element.BlockNumber = BlockNumber;
            element.State |= 0x40;
            DataBlockCacheIndex = blockCacheIndex;
            return this.VolumeExtension.BlockCache.Data[blockCacheIndex];
        }

        internal StfCacheElement StfsBlockCacheElementFromBlock(int BlockCaheIndex)
        {
            return this.VolumeExtension.ElementCache.RetrieveElement(BlockCaheIndex);
        }

        internal bool StfsMapExistingDataBlock(uint BlockNum, ref int BlockCacheIndex)
        {
            StfCacheElement element = null;
            return this.StfsMapExistingBlock(BlockNum, 0, ref BlockCacheIndex, ref element);
        }

        internal bool StfsMapExistingBlock(uint BlockNum, int ElementType, ref int BlockCacheIndex,
                                          ref StfCacheElement Element)
        {
            if (!this.StfsLookupBlockCacheEntry(BlockNum, ElementType, ref Element, ref BlockCacheIndex))
            {
                return false;
            }
            if (Element != null)
            {
                this.StfsMoveBlockCacheEntry(BlockCacheIndex, true);

                if (Element.Referenced == 0xff)
                {
                    throw new StfsException("Invalid cache element detected with a bad reference count.");
                }

                Element.Referenced++;

                return true; // success
            }
            else
            {
                BlockCacheIndex = -1;
                return false; // failure
            }
        }

        internal bool StfsMapNewBlock(uint blockNumber, int elementType, StfHashEntry hashEntry, ref int blockCacheIndex,
                                     ref StfCacheElement element)
        {
            uint NewBlockNum, ActiveIndex = 0;

            if (elementType != 0)
            {
                blockNumber = (blockNumber -
                               (blockNumber % this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[elementType - 1]));
                NewBlockNum = StfsComputeLevelNBackingHashBlockNumber(blockNumber, elementType - 1);
                ActiveIndex = (hashEntry.LevelAsUint >> 30) & 1;
            }
            else
            {
                NewBlockNum = StfsComputeBackingDataBlockNumber(blockNumber);
            }

            this.StfsAllocateBlockCacheEntry(ref blockCacheIndex, ref element);

            this.VolumeExtension.BlockCache.Data[blockCacheIndex] = this.StfsSynchronousReadFile(NewBlockNum, 0x1000, (int)ActiveIndex);

#if DEBUG && HASH_MISMATCH
            {
                byte[] hash = XeCrypt.XeCryptSha(this.VolumeExtension.BlockCache.Data[blockCacheIndex]);
                if (!ArrayEquals(hash, hashEntry.Hash))
                {
                    throw new StfsException(string.Format("hash mismatch for block number 0x{0:x8}:{1:d}.", blockNumber,
                                                          elementType));
                }
            }
#endif
            this.StfsMoveBlockCacheEntry(blockCacheIndex, true);

            element.BlockNumber = blockNumber;

            element.State =
                (byte)
                ((((element.State & 0xDC) | ((int)((0x1F80 | (ActiveIndex << 5) & 0x20)))) | (elementType & 3)) & 0xFF);

            element.Referenced = 1;

            return this.VolumeExtension.BlockCache.Data[blockCacheIndex] != null && this.VolumeExtension.BlockCache.Data[blockCacheIndex].Length > 0;
        }

        private int StfsMapEmptyHashBlock(uint blockNumber, int requestedLevel, ref StfHashEntry hashEntry,
                                          ref StfsHashBlock returnedHashBlock, ref int blockCacheIndex)
        {
            StfCacheElement cacheElement = null;
            byte[] dataBlock = null;

            int errorCode = this.StfsMapNewEmptyBlock(requestedLevel + 1, blockNumber, ref cacheElement,
                                                      ref dataBlock, ref blockCacheIndex);
            if (errorCode < 0)
            {
                return errorCode;
            }

            returnedHashBlock = new StfsHashBlock(dataBlock);

            if (hashEntry != null)
            {
                hashEntry.LevelAsUint &= 0x3FFFFFFF;
                hashEntry.LevelAsUint |= 0x80000000;
                cacheElement.State |= 0x04;
            }

            return 0;
        }

        internal int StfsMapEmptyDataBlock(uint blockNumber, ref byte[] dataBlock, ref int blockCacheIndex)
        {
            if (dataBlock != null && dataBlock.Length != 0x1000)
                throw new StfsException("Invalid data block supplied for new data block mapping.");

            StfCacheElement element = null;

            return this.StfsMapNewEmptyBlock(0, blockNumber, ref element, ref dataBlock, ref blockCacheIndex);
        }

        internal int StfsMapNewEmptyBlock(int elementType, uint blockNumber, ref StfCacheElement element,
                                         ref byte[] dataBlock, ref int blockCacheIndex)
        {
            if (!this.StfsLookupBlockCacheEntry(blockNumber, elementType, ref element, ref blockCacheIndex))
            {
                int ret = this.StfsAllocateBlockCacheEntry(ref blockCacheIndex, ref element);
                if (ret < 0)
                    return ret;
            }

            if (elementType > 0)
            {
                blockNumber = blockNumber -
                              (blockNumber % this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[elementType - 1]);
            }

            element.BlockNumber = blockNumber;

            Array.Clear(this.VolumeExtension.BlockCache.Data[blockCacheIndex], 0, 0x1000);

            dataBlock = this.VolumeExtension.BlockCache.Data[blockCacheIndex];

            this.StfsMoveBlockCacheEntry(blockCacheIndex, true);

            element.BlockNumber = blockNumber;

            int State = (element.State & ~3) | (elementType & 3);

            State = (int)((State & ~0xFFFFFFE0) | ((3 << 6) & 0xFFFFFFE0));

            element.State = (byte)State;

            if (element.Referenced == 0xff)
            {
                throw new StfsException("Invalid cache element detected while mapping a new empty block.");
            }
            element.Referenced++;

            return 0;
        }

        internal void StfsReferenceBlock(int blockCacheIndex)
        {
            if (blockCacheIndex < this.VolumeExtension.ElementCache.CacheElementCount)
            {
                StfCacheElement cacheElement = this.VolumeExtension.ElementCache.RetrieveElement(blockCacheIndex);

                if (cacheElement.Referenced != 0 && cacheElement.Referenced != 0xff)
                {
                    cacheElement.Referenced++;
                }
                else
                {
                    throw new StfsException("Attempted to reference an invalid cache element.");
                }
            }
        }

        internal void StfsDereferenceBlock(int blockCacheIndex)
        {
            if (blockCacheIndex < this.VolumeExtension.ElementCache.CacheElementCount)
            {
                StfCacheElement cacheElement = this.VolumeExtension.ElementCache.RetrieveElement(blockCacheIndex);

                if (cacheElement.Referenced == 0)
                {
                    throw new StfsException("Attempted to dereference an invalid cache element.");
                }
                else
                {
                    cacheElement.Referenced += 0xff;
                }
            }
        }

        internal void StfsDiscardBlock(uint blockNumber, int ElementType)
        {
            StfCacheElement element = null;
            int BlockCacheIndex = -1;

            if (this.StfsLookupBlockCacheEntry(blockNumber, ElementType, ref element, ref BlockCacheIndex))
            {
                if (element.Referenced != 0)
                {
                    throw new StfsException("Invalid cache element found while discarding block.");
                }

                if ((element.State & 0x10) != 0)
                {
                    if (ElementType != (this.VolumeExtension.RootHashHierarchy + 1))
                    {
                        StfCacheElement elementTwo = null;
                        int secondBlockCacheIndex = -1;

                        this.StfsLookupBlockCacheEntry(blockNumber, ElementType + 1, ref elementTwo,
                                                       ref secondBlockCacheIndex);

                        if (elementTwo.Referenced == 0 || (elementTwo.State & 0x40) == 0)
                        {
                            throw new StfsException("Invalid cache element found while discarding block.");
                        }
                        this.StfsDereferenceBlock(secondBlockCacheIndex);
                    }
                }

                element.State = 0;

                this.StfsMoveBlockCacheEntry(BlockCacheIndex, false);
            }
        }

        internal void StfsMoveBlockCacheEntry(int BlockCacheIndex, bool MoveToHead)
        {
            if (BlockCacheIndex != this.VolumeExtension.CacheHeadIndex)
            {
                var element = this.VolumeExtension.ElementCache.RetrieveElement(this.VolumeExtension.CacheHeadIndex);
                if (element.BlockCacheIndex != BlockCacheIndex)
                {
                    element = this.VolumeExtension.ElementCache.RetrieveElement(BlockCacheIndex);

                    this.VolumeExtension.ElementCache.RetrieveElement(element.BlockCacheIndex).Index = element.Index;
                    this.VolumeExtension.ElementCache.RetrieveElement(element.Index).BlockCacheIndex =
                        element.BlockCacheIndex;

                    element.Index = this.VolumeExtension.CacheHeadIndex;

                    var headElement =
                        this.VolumeExtension.ElementCache.RetrieveElement(this.VolumeExtension.CacheHeadIndex);

                    element.BlockCacheIndex = headElement.BlockCacheIndex;
                    this.VolumeExtension.ElementCache.RetrieveElement(headElement.BlockCacheIndex).Index =
                        BlockCacheIndex;

                    headElement.BlockCacheIndex = BlockCacheIndex;
                }

                if (MoveToHead)
                {
                    this.VolumeExtension.CacheHeadIndex = BlockCacheIndex;
                }
            }
            else
            {
                if (!MoveToHead)
                {
                    this.VolumeExtension.CacheHeadIndex =
                        this.VolumeExtension.ElementCache.RetrieveElement(this.VolumeExtension.CacheHeadIndex).Index;
                }
            }
        }

        internal bool StfsLookupBlockCacheEntry(uint BlockNumber, int ElementType, ref StfCacheElement element,
                                               ref int BlockCacheIndex)
        {
            if (ElementType != 0)
            {
                BlockNumber = (BlockNumber -
                               (BlockNumber % this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[ElementType - 1]));
            }

            int idx = this.VolumeExtension.CacheHeadIndex;

            do
            {
                element = this.VolumeExtension.ElementCache.RetrieveElement(idx);

                if ((element.State & 0x80) != 0)
                {
                    if (element.BlockNumber == BlockNumber && element.ElementType == ElementType)
                    {
                        BlockCacheIndex = idx;
                        return true;
                    }
                    if (element.Index == this.VolumeExtension.CacheHeadIndex)
                        return false;

                    idx = element.Index;
                }
                else
                {
                    return false;
                }
            } while (true);
        }

        internal int StfsAllocateBlockCacheEntry(ref int BlockCacheIndex, ref StfCacheElement Element)
        {
            int idx =
                this.VolumeExtension.ElementCache.RetrieveElement(this.VolumeExtension.CacheHeadIndex).BlockCacheIndex;
            int elementIndex = idx;
            do
            {
                var element = this.VolumeExtension.ElementCache.RetrieveElement(elementIndex);

                if (element.Referenced != 0)
                {
                    if (element.BlockCacheIndex == idx)
                    {
                        throw new StfsException("block cache overcommitted. [0xC00000E5]");
                    }
                    elementIndex = element.BlockCacheIndex;
                }
                else
                {
                    if ((element.State & 0x40) != 0)
                    {
                        if ((element.State & 8) != 0)
                        {
                            this.StfsFlushInAllocationSupportBlocks();
                        }
                        else
                        {
                            this.StfsFlushBlockCacheEntry(elementIndex, ref element);
                        }
                        if ((element.State & 0x40) != 0)
                        {
                            throw new StfsException("Invalid cache element detected.");
                        }
                    }

                    element.State = 0x00;

                    this.StfsMoveBlockCacheEntry(elementIndex, false);

                    if (element.Referenced != 0)
                    {
                        throw new StfsException("Invalid cache element detected.");
                    }

                    BlockCacheIndex = elementIndex;
                    Element = element;

                    return 0;
                }
            } while (true);
        }

        internal void StfsResetBlockCache()
        {
            int cacheElementCount = this.VolumeExtension.BlockCacheElementCount;
            this.VolumeExtension.ElementCache = new StfsCacheElement(cacheElementCount);
            this.VolumeExtension.BlockCache = new StfsCacheBlock(cacheElementCount);

            this.VolumeExtension.ElementCache.Cache[0].BlockCacheIndex = ((cacheElementCount & 0xFF) + 0xFF) & 0xff;
            this.VolumeExtension.ElementCache.Cache[cacheElementCount - 1].Index = 0;
            this.VolumeExtension.CacheHeadIndex = 0;
        }

        internal void StfsResetWriteableBlockCache()
        {
            for (var x = 0; x < this.VolumeExtension.BlockCacheElementCount; x++)
            {
                this.VolumeExtension.ElementCache.Cache[x].State &= 0xFB;
            }
        }

        internal void StfsFlushBlockCacheEntry(int BlockCacheIndex, ref StfCacheElement BlockCacheElement)
        {
            uint BlockNumber = BlockCacheElement.BlockNumber, NewBlockNum = 0;
            int ElementType = BlockCacheElement.ElementType, CacheIndex = -1;
            if (ElementType != 0)
            {
                NewBlockNum = StfsComputeLevelNBackingHashBlockNumber(BlockNumber, ElementType - 1);
            }
            else
            {
                NewBlockNum = StfsComputeBackingDataBlockNumber(BlockNumber);
            }
            if ((BlockCacheElement.State & 0x10) != 0x0)
            {
                byte[] Hash = XeCrypt.XeCryptSha(this.VolumeExtension.BlockCache.Data[BlockCacheIndex], null, null);

                if (this.VolumeExtension.RootHashHierarchy + 1 != ElementType)
                {
                    StfCacheElement element = null;
                    if (!this.StfsLookupBlockCacheEntry(BlockNumber, ElementType + 1, ref element, ref CacheIndex))
                    {
                        throw new StfsException("Could the not find requested cache entry to flush.");
                    }

                    if (element.Referenced == 0)
                    {
                        throw new StfsException("Illegal reference count detected while flushing cache entry.");
                    }

                    if ((element.State & 0x40) == 0)
                    {
                        throw new StfsException("Invalid element state detected while flushing cache entry.");
                    }

                    StfsHashBlock hashBlock =
                        new StfsHashBlock(this.VolumeExtension.BlockCache.Data[CacheIndex]);
                    BlockNumber = BlockNumber / this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[ElementType - 1];
                    hashBlock.SetHashForEntry(BlockNumber, Hash);
                    hashBlock.Save();

                    this.StfsDereferenceBlock(CacheIndex);
                }
                else
                {
                    this.VolumeExtension.RootHashEntry.Hash = Hash;
                }
                BlockCacheElement.State &= 0xEF;
            }
            this.StfsSynchronousWriteFile(NewBlockNum, (BlockCacheElement.State >> 5) & 1,
                                          this.VolumeExtension.BlockCache.Data[BlockCacheIndex], 0x1000);

            BlockCacheElement.State &= 0xBF;
        }

        public void StfsFlushBlockCache(uint StartingBlockNumber, uint EndingBlockNumber)
        {
            int hierarchy = EndingBlockNumber == 0xffffffff ? this.VolumeExtension.RootHashHierarchy + 1 : 0, i = 0;
            do
            {
                for (int x = 0; x < this.VolumeExtension.BlockCacheElementCount; x++)
                {
                    StfCacheElement element = this.VolumeExtension.ElementCache.RetrieveElement(x);

                    if (element.ElementType == i)
                    {
                        if (element.Referenced == 0 && (element.State & 0x40) != 0
                            && element.BlockNumber >= StartingBlockNumber && element.BlockNumber <= EndingBlockNumber)
                        {
                            this.StfsFlushBlockCacheEntry(x, ref element);
                        }
                    }
                }
            } while (++i <= hierarchy);
        }

        internal void StfsFlushInAllocationSupportBlocks()
        {
            if (!this.VolumeExtension.InAllocationSupport)
            {
                throw new StfsException("Attempted to flush blocks without in-allocation support.");
            }

            int idx = this.VolumeExtension.CacheHeadIndex,
                idx2 = this.VolumeExtension.ElementCache.RetrieveElement(idx).BlockCacheIndex;
            StfCacheElement element = null;
            do
            {
                element = this.VolumeExtension.ElementCache.RetrieveElement(idx2);

                if (element.Referenced == 0)
                {
                    if ((element.State & 0x48) != 0)
                    {
                        this.StfsFlushBlockCacheEntry(idx2, ref element);
                    }
                }
                if (idx2 == idx)
                {
                    break;
                }
                idx2 = element.BlockCacheIndex;
            } while (true);
        }

        internal void StfsFlushUpdateDirectoryEntries()
        {
            for (int x = 0; x < this._fcbs.Count; x++)
            {
                StfsFcb fcb = _fcbs[x];
                if ((fcb.State & 0x10) != 0)
                {
                    this.StfsUpdateDirectoryEntry(fcb, false);
                }
                if ((fcb.State & 0x10) != 0)
                {
                    throw new StfsException(
                        string.Format("Detected an invalid state after updating the directory entry for file {0}.",
                                      _fcbs[x].FileName));
                }
            }
        }

        internal byte[] StfsSynchronousReadFile(uint BlockNumber, uint Length, int ActiveIndex)
        {
            _io.Stream.Position = this.VolumeExtension.BackingFileOffset +
                                             GetBlockOffset(BlockNumber, ActiveIndex);

            return _io.ReadByteArray((int)Length);
        }

        internal void StfsSynchronousWriteFile(uint BlockNumber, int ActiveIndex, byte[] DataBlock, uint DataLength)
        {
            if (!VolumeExtension.ReadOnly)
            {
                this.StfsExtendBackingFileSize(
                    (uint)((DataBlock.Length >> 12) + (BlockNumber + ActiveIndex)));
            }

            _io.Stream.Position = this.VolumeExtension.BackingFileOffset +
                                              GetBlockOffset(BlockNumber, ActiveIndex);

            _io.Write(DataBlock, 0x00, (int)DataLength);

            _io.Stream.Flush();
        }

        public uint StfsComputeBackingDataBlockNumber(uint BlockNum)
        {
            uint num1 = (((BlockNum + this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[0]) /
                          this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[0])
                         << this.VolumeExtension.FormatShift) + BlockNum;
            if (BlockNum < this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[0])
                return num1;
            num1 = (((BlockNum + this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[1]) /
                     this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[1])
                    << this.VolumeExtension.FormatShift) + num1;
            if (BlockNum < this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[1])
                return num1;
            return (uint)(num1 + (1 << this.VolumeExtension.FormatShift));
        }

        public uint StfsComputeLevelNBackingHashBlockNumber(uint BlockNum, int RequestedLevel)
        {
            uint num1, num2, num3 = (uint)(1 << this.VolumeExtension.FormatShift);
            switch (RequestedLevel)
            {
                case 0:
                    num1 = (BlockNum / this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[0]);
                    num2 = num1 * this.VolumeExtension.BlockValues[0];
                    if (num1 == 0)
                        return num2;
                    num1 = (BlockNum / this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[1]);
                    num2 += (num1 + 1) << this.VolumeExtension.FormatShift;
                    if (num1 == 0)
                        return num2;
                    return num2 + num3;
                case 1:
                    num1 = (BlockNum / this.VolumeExtension.StfsDataBlocksPerHashTreeLevel[1]);
                    num2 = num1 * this.VolumeExtension.BlockValues[1];

                    if (num1 == 0)
                        return num2 + this.VolumeExtension.BlockValues[0];

                    return num2 + num3;
                case 2:
                    return this.VolumeExtension.BlockValues[1];
            }
            return 0xffffff;
        }

        /*public static uint StfsComputeNumberOfDataBlocks(ulong NumberOfBackingBlocks)
        {
            ulong num1 = NumberOfBackingBlocks, num3, num4;
            if (num1 > 0x4BDA85)
                return 0x4AF768;

            ulong num2 = 0xAC;
            if (num1 < 0x723A)
            {
                if (num1 >= 0xAC)
                {
                    num1 -= 2;
                    if (num1 <= 0xB0)
                    {
                        num1 = num2;
                    }
                }
            }
            else if (num1 > 0x723A)
            {
                num1 -= 2;
                num4 = 0x723a;
                if (num1 + 2 <= 0x7240)
                {
                    num1 = num4;
                }
                num3 = num1 / num4;
                num3 = num3 * 0x723a;
                num3 = num1 - num3;
                if (num3 <= 4)
                {
                    num1 = num1 - num3;
                }
                num3 = num1 + 0x7239;
                num3 = num3 / num4;
                num3 = num3 << 1;
                num1 = num1 - num3;
            }
            num3 = num1 / num2;
            num3 = num3 * 0xAC;
            num3 = num1 - num3;
            if (num3 <= 2)
            {
                num1 = num1 - num3;
            }
            num3 = num1 + 0xAB;
            num3 = num3 / num2;
            num3 = num3 << 1;

            return (uint)(num1 - num3);
        }*/

        internal static DateTime StfsTimeStampToDatetime(StfsTimeStamp Timestamp)
        {
            return new DateTime(Timestamp.Year, Timestamp.Month, Timestamp.Day, Timestamp.Hour, Timestamp.Minute,
                                Timestamp.DoubleSeconds / 2);
        }

        internal static StfsTimeStamp StfsDateTimeToTimeStamp(DateTime Time)
        {
            StfsTimeStamp timestamp = new StfsTimeStamp();
            timestamp.Year = Time.Year;
            timestamp.Month = Time.Month;
            timestamp.Day = Time.Day;
            timestamp.Hour = Time.Hour;
            timestamp.Minute = Time.Minute;
            timestamp.DoubleSeconds = Time.Second * 2;
            return timestamp;
        }

        public static void ObDissectName(string name, out string firstName, out string remainingName)
        {
            if (name.Length > 0x00)
            {
                int startIndex = name[0x00] == '\\' ? 0x01 : 0x00, idx = 0x00;
                idx = name.IndexOf('\\', startIndex);
                if (idx != -1)
                {
                    firstName = name.Substring(startIndex, idx - startIndex);
                    remainingName = name.Substring(++idx, name.Length - (idx));
                }
                else
                {
                    firstName = name.Substring(startIndex, name.Length - startIndex);
                    remainingName = string.Empty;
                }
            }
            else
            {
                firstName = string.Empty;
                remainingName = string.Empty;
            }
        }

        /// <summary>
        /// Rounds a value to the nearest STFS block size ( = 0x1000 )
        /// </summary>
        /// <param name="value">The value to round</param>
        /// <returns>The value rounded to 4096.</returns>
        public static uint RoundToBlock(uint value)
        {
            return (value + 0xFFF) & 0xFFFFF000;
        }
    }

    internal class StfsFcb : IoFcb
    {
        public StfsFcb ParentFcb;
        public StfsFcb CloneFcb;
        public uint BlockPosition;
        public uint FirstBlockNumber;
        public uint AllocationBlocks;
        public uint ValidAllocBlocks;
        public string FileName;
        public uint Filesize;
        public uint ContiguousBytesRead;
        public uint LastUnContiguousBlockNum;
        public uint LastBlockNumber;
        public ushort DirectoryEntryIndex;
        public ushort ParentDirectoryIndex;
        public StfsTimeStamp CreationTimeStamp;
        public StfsTimeStamp LastWriteTimeStamp;

        public DateTime CreationTime { get { return StfsDevice.StfsTimeStampToDatetime(CreationTimeStamp); } }
        public DateTime LastWriteTime { get { return StfsDevice.StfsTimeStampToDatetime(LastWriteTimeStamp); } }

        public int Referenced;
        public byte State = 0;

        public IoLinkledList ParentDcbQueue;
        public IoLink ParentDcbLinks;
        public ShareAccess ShareAccess;

        /* States
         * 0x01 - Is Title-Owned
         * 0x02 - Is Folder/Directory
         * 0x04 - Is Root Directory FCB
         * 0x08 - Mark For Deletion
         * 0x10 - Modified
         * 0x20 - Is Writeable
        */

        public bool IsDirectory
        {
            get { return Bitwise.IsFlagOn(State, 2); }
            set { State = (byte)((State & ~2) | (byte)(value ? 1 : 0)); }
        }
        public bool DeleteOnClose
        {
            get { return Bitwise.IsFlagOn(State, 0x08); }
        }
        public bool IsRootDirectory
        {
            get { return Bitwise.IsFlagOn(State, 0x04); }
        }

        // Clone FCB function
        public StfsFcb(StfsFcb fcb)
        {
            ParentDcbQueue = new IoLinkledList();
            //ParentDcbLinks = new IoLink(fcb);
            ParentDcbQueue.InitializeListHead(this);

            DirectoryEntryIndex = fcb.DirectoryEntryIndex;
            FileName = fcb.FileName;

            if (fcb.IsDirectory) IsDirectory = true;

            FirstBlockNumber = fcb.FirstBlockNumber;
            Filesize = fcb.Filesize;
            AllocationBlocks = fcb.AllocationBlocks;
            ValidAllocBlocks = fcb.ValidAllocBlocks;
            CreationTimeStamp = fcb.CreationTimeStamp;
            LastWriteTimeStamp = fcb.LastWriteTimeStamp;
            LastBlockNumber = fcb.LastBlockNumber;
            BlockPosition = fcb.BlockPosition;
            ContiguousBytesRead = fcb.ContiguousBytesRead;
            LastUnContiguousBlockNum = fcb.LastUnContiguousBlockNum;
        }

        public StfsFcb(StfsDirectoryEntry dirEnt, StfsFcb parentFcb)
        {
            InitializeDcb(parentFcb);

            parentFcb.Referenced++;
            Referenced = 0x01;

            ParentFcb = parentFcb;

            LastBlockNumber = 0xffffffff;
            BlockPosition = 0xffffffff;

            ParentDirectoryIndex = dirEnt.DirectoryIndex;
            DirectoryEntryIndex = (ushort)(dirEnt.DirectoryEntryByteOffset / 0x40);

            FileName = dirEnt.FileName;

            if (dirEnt.IsDirectory)
            {
                State |= 2;
            }

            FirstBlockNumber = dirEnt.FirstBlockNumber;

            AllocationBlocks = dirEnt.AllocationBlocks * 0x1000;
            ValidAllocBlocks = dirEnt.ValidDataBlocks * 0x1000;

            Filesize = dirEnt.FileBounds.Filesize;

            if (ValidAllocBlocks > AllocationBlocks)
            {
                throw new StfsException(
                    "The number of valid allocation blocks was higher than the allocation block count.");
            }
            if (Filesize > AllocationBlocks)
            {
                throw new StfsException(
                    string.Format("The file size of the file {0} was greater than the allocated byte count.",
                                  dirEnt.FileName));
            }


            ContiguousBytesRead = 0x0000;
            LastUnContiguousBlockNum = 0;

            if (dirEnt.ValidDataBlocks == 0 || !dirEnt.Contiguous)
            {
                LastBlockNumber = 0xffffffff;
                BlockPosition = 0xffffffff;
            }
            else
            {
                LastBlockNumber = FirstBlockNumber + dirEnt.ValidDataBlocks - 1;
                BlockPosition = 0;
                ContiguousBytesRead = ValidAllocBlocks;
                LastUnContiguousBlockNum = FirstBlockNumber;
            }
            CreationTimeStamp = dirEnt.CreationTimeStamp;
            LastWriteTimeStamp = dirEnt.LastWriteTimeStamp;
        }

        public StfsFcb()
        {
            ParentDcbQueue = new IoLinkledList();
            ParentDcbLinks = new IoLink(this);
        }

        private void InitializeDcb(StfsFcb parentFcb)
        {
            ParentDcbQueue = new IoLinkledList();

            ParentDcbQueue.InitializeListHead(this);
        }

        public void Dispose()
        {
            this.CloneFcb = null;
            this.ParentFcb = null;
        }
    }

    internal struct StfsTimeStamp
    {
        public int DoubleSeconds
        {
            get { return (AsInt & 0x1F) / 2; }
            set { AsInt = AsInt | (value * 2) & 0x1F; }
        }

        public int Minute
        {
            get { return ((AsInt >> 5) & 0x3f); }
            set { AsInt = AsInt | (value & 0x3f) << 5; }
        }

        public int Hour
        {
            get { return ((AsInt >> 11) & 0x1F); }
            set { AsInt = AsInt | (value & 0x1f) << 11; }
        }

        public int Day
        {
            get { return ((AsInt >> 16) & 0x1F); }
            set { AsInt = AsInt | (value & 0x1f) << 16; }
        }

        public int Month
        {
            get { return ((AsInt >> 21) & 0x0F); }
            set { AsInt = AsInt | (value & 0x0f) << 21; }
        }

        public int Year
        {
            get { return ((AsInt >> 25) & 0x7f) + 1980; }
            set { AsInt = AsInt | ((value & 0x7f) - 1980) << 25; }
        }

        public int AsInt { get; set; }
    }

    internal class StfsDirectoryEntry
    {
        public string FileName; // 40
        public byte FileNameLength;
        public bool Contiguous;
        public bool IsDirectory;
        public uint ValidDataBlocks;
        public uint AllocationBlocks;
        public uint FirstBlockNumber;
        public ushort DirectoryIndex;
        public StfsFileBounds FileBounds;
        public StfsTimeStamp CreationTimeStamp;
        public StfsTimeStamp LastWriteTimeStamp;

        public bool IsEntryBound
        {
            get { return FileNameLength != 0; }
        }

        public uint DirectoryEntryByteOffset;
        public uint DirectoryEndOfListing;

        public StfsDirectoryEntry()
        {
            this.FileName = String.Empty;
            this.FileNameLength = 0;
            this.Contiguous = false;
            this.IsDirectory = false;
            this.ValidDataBlocks = 0;
            this.AllocationBlocks = 0;
            this.DirectoryIndex = 0xffff;
            this.FileBounds.Filesize = 0;
            this.CreationTimeStamp.AsInt = 0;
            this.LastWriteTimeStamp.AsInt = 0;
        }

        public StfsDirectoryEntry(string filename, bool isDirectory)
            : this(filename, isDirectory, 0xffff)
        {
        }

        public StfsDirectoryEntry(string filename, bool isDirectory, ushort directoryIndex)
        {
            this.FileName = filename;
            this.FileNameLength = (byte)filename.Length;
            this.Contiguous = false;
            this.IsDirectory = isDirectory;
            this.ValidDataBlocks = 0;
            this.AllocationBlocks = 0;
            this.DirectoryIndex = directoryIndex;
            this.FileBounds.Filesize = 0;
            this.CreationTimeStamp.AsInt = 0;
            this.LastWriteTimeStamp.AsInt = 0;
        }

        public StfsDirectoryEntry(EndianIO reader)
        {
            FileName = reader.ReadAsciiString(40);

            byte attributes = reader.ReadByte();
            FileNameLength = (byte)(attributes & 0x3F);
            Contiguous = ((attributes >> 6) & 1) == 1;
            IsDirectory = ((attributes >> 7) & 1) == 1;

            reader.Endianness = EndianType.Little;

            ValidDataBlocks = reader.ReadUInt24(); // valid
            AllocationBlocks = reader.ReadUInt24(); // allocation
            FirstBlockNumber = reader.ReadUInt24();

            reader.Endianness = EndianType.Big;

            DirectoryIndex = reader.ReadUInt16();
            FileBounds.Filesize = reader.ReadUInt32();

            CreationTimeStamp.AsInt = reader.ReadInt32();
            LastWriteTimeStamp.AsInt = reader.ReadInt32();

            if (FileNameLength != 0x00 && FileName.Length > FileNameLength)
            {
                FileName = FileName.Remove(FileNameLength);
            }
        }

        public byte[] ToArray()
        {
            var ms = new MemoryStream();
            var ew = new EndianIO(ms, EndianType.Big);

            ew.WriteAsciiString(FileName, 0x28);
            ew.Write(
                (byte)
                ((FileNameLength & 0x3f) | (((byte)(Contiguous ? 1 : 0) & 0x1) << 6) |
                (((byte)(IsDirectory ? 1 : 0) & 0x1) << 7)));
            ew.Endianness = EndianType.Little;
            ew.WriteUInt24(ValidDataBlocks);
            ew.WriteUInt24(AllocationBlocks);
            ew.WriteUInt24(FirstBlockNumber);
            ew.Endianness = EndianType.Big;
            ew.Write(DirectoryIndex);
            ew.Write(FileBounds.Filesize);
            ew.Write(CreationTimeStamp.AsInt);
            ew.Write(LastWriteTimeStamp.AsInt);

            ew.Close();
            return ms.ToArray();
        }
    }

    internal class StfsDirectoryEnumerationContext
    {
        public StfsDirectoryEnumerationContext(StfsDevice device, string directoryInfo, string mask)
        {
            _ioFsdHandle = new IoFsdHandle();
            _device = device;
            _searchPattern = mask;
            _ioFsdHandle.Sp.CreateParametersParameters = new IrpCreateParameters()
            {
                DesiredAccess = 0x100001,
                Options = 0x1004021,
                FileAttributes = 0x40,
                ShareAccess = 0x03,
                RemainingName = directoryInfo
            };
            device.StfsFsdCreate(_ioFsdHandle);
        }

        public bool FindNextFile(out IoFsdDirectoryInformation findFileData)
        {
            return _device.StfsFsdDirectoryControl(_ioFsdHandle, _searchPattern, out findFileData) == 0x00;
        }
        public void Close()
        {
            _device.StfsFsdClose(_ioFsdHandle);
        }
        private readonly string _searchPattern;
        private readonly IoFsdHandle _ioFsdHandle;
        private readonly StfsDevice _device;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct StfsFileBounds
    {
        [FieldOffset(0)]
        public uint Filesize;
        [FieldOffset(0)]
        public ushort FirstChildDirectoryIndex;
        [FieldOffset(2)]
        public ushort LastChildDirectoryIndex;
    }

    public class StfCacheElement
    {
        public byte Referenced;
        public uint BlockNumber;
        public int Index;
        public int BlockCacheIndex;

        // 0x10 - Modified
        // 0x40 - Writable data block
        // 0x80 - In Use

        public byte State;

        public int ElementType
        {
            get { return this.State & 3; }
            set { this.State = (byte)((this.State & ~3) | value); }
        }

        // 0 - data block, 1 - level0 hash block, 2 - level1 hash block, 3 - level2 hash block

        public StfCacheElement(int blockCacheIndex)
        {
            this.Index = blockCacheIndex;
            this.BlockCacheIndex = (blockCacheIndex + 0xFE) & 0xFF;
        }
    }

    public class StfsCacheElement : IDisposable
    {
        public int CacheElementCount;
        public List<StfCacheElement> Cache;

        public StfsCacheElement(int cacheCount)
        {
            this.Cache = new List<StfCacheElement>();

            this.CacheElementCount = cacheCount;
            for (var x = 0; x < cacheCount; x++)
            {
                this.Cache.Add(new StfCacheElement(x + 1));
            }
        }

        public StfCacheElement RetrieveElement(int index)
        {
            if (index < this.Cache.Count)
            {
                return this.Cache[index];
            }
            return null;
        }

        public void Dispose()
        {
            this.Cache.Clear();
        }
    }

    public struct StfsAllocateBlockState
    {
        public uint NumberOfNeededBlocks; // 0x00
        public uint FirstAllocatedBlockNumber; // 0x04
        public uint LastAllocatedBlockNumber; // 0x08
        public StfHashEntry HashEntry; // 0x0C - ptr to hash entry
        public uint HashEntryIndex;
        public int Block; // 0x10 hashBlock, cache Index in this case
    }

    public struct StfsFreeBlockState
    {
        public bool MarkFirstAsLast; // 0x00
        public StfHashEntry HashEntry; // 0x04
    }

    public enum StfsHashEntryLevel0State
    {
        Unallocated = 0,
        FreedPending = 1,
        Allocated = 2,
        Pending = 3
    }

    public struct StfsHashEntryLevel0
    {
        public uint NextBlockNumber;
        public StfsHashEntryLevel0State State;
    }

    public struct StfsHashEntryLevelN
    {
        public uint NumberOfFreeBlocks;
        public uint NumberOfFreePendingBlocks;
        public uint ActiveIndex;
        public uint Writeable;
    }

    public class StfHashEntry
    {
        public byte[] Hash; // 20
        public StfsHashEntryLevel0 Level0;
        public StfsHashEntryLevelN LevelN;

        internal uint _level;

        public uint LevelAsUint
        {
            get
            {
                //uint level0 = ((Level0.nextBlockNumber & 0xFFFFFF) | ((Convert.ToUInt32(Level0.State) << 30)));
                //uint levelN = ((LevelN.NumberOfFreeBlocks & 0x7FFF) | ((LevelN.NumberOfFreePendingBlocks & 0x7fff) << 15)
                //|((LevelN.ActiveIndex & 0x01) << 30) | ((LevelN.Writeable & 1) << 31));

                //return level0 | levelN;   
                return _level;
            }
            set
            {
                Level0.NextBlockNumber = (value & 0xFFFFFF);
                Level0.State = (StfsHashEntryLevel0State)(value >> 30);

                LevelN.NumberOfFreeBlocks = value & 0x7FFF;
                LevelN.NumberOfFreePendingBlocks = (value >> 15) & 0x7FFF;
                LevelN.ActiveIndex = (value >> 30) & 1;
                LevelN.Writeable = (value >> 31) & 1;

                _level = value;
            }
        }

        public bool IsBlockAllocated
        {
            get
            {
                return (Level0.State != StfsHashEntryLevel0State.FreedPending &&
                        Level0.State != StfsHashEntryLevel0State.Unallocated);
            }
        }

        public StfHashEntry()
        {
            Hash = new byte[0x14];
            LevelAsUint = 0x00ffffff;
        }

        public StfHashEntry(int blockNum)
        {
            Hash = new byte[0x14];
            LevelAsUint = (uint)((0x80 << 0x18) | (blockNum & 0xFFFFFF));
        }

        public StfHashEntry(EndianIO reader)
        {
            Hash = reader.ReadByteArray(0x14);
            LevelAsUint = reader.ReadUInt32();
        }

        public void SetNextBlockNumber(uint nextBlockNumber)
        {
            this.LevelAsUint = (nextBlockNumber & 0xFFFFFF) | (this.LevelAsUint & 0xFF000000);
        }

        public void SetNumberOfFreeBlocks(uint numberOfFreeBlocks)
        {
            this.LevelAsUint = ((this.LevelAsUint & 0xFFFF8000) | (numberOfFreeBlocks & 0x7FFF));
        }

        public void SetNumberOfFreePendingBlocks(uint numberOfFreePendingBlocks)
        {
            this.LevelAsUint = (this.LevelAsUint & 0xC0007FFF) | ((numberOfFreePendingBlocks & 0x7FFF) << 15);
        }
    }

    public class StfsHashBlock : IDisposable
    {
        public List<StfHashEntry> Entries;
        public uint NumberOfCommittedBlocks;

        internal readonly EndianIO _io;

        public StfsHashBlock()
        {
            this.Entries = new List<StfHashEntry>();
            for (var x = 0; x < 0xAA; x++)
            {
                this.Entries.Add(new StfHashEntry());
            }
            this.NumberOfCommittedBlocks = 0;
        }

        public StfsHashBlock(byte[] data)
            : this(new EndianIO(data, EndianType.Big))
        {
        }

        public StfsHashBlock(EndianIO reader)
        {
            _io = reader;
            this.Entries = new List<StfHashEntry>();
            for (var x = 0; x < 0xAA; x++)
            {
                this.Entries.Add(new StfHashEntry(reader));
            }
            this.NumberOfCommittedBlocks = reader.ReadUInt32();
        }

        public StfHashEntry RetrieveHashEntry(int blockNumber)
        {
            if (blockNumber < 0x00)
                throw new StfsException("Invalid block number detected while retrieving an entry from a hash block.");
            return this.Entries[blockNumber % 0xAA];
        }

        public void SetEntry(int blockNumber, StfHashEntry hashEntry)
        {
            if (blockNumber < 0x00)
                throw new StfsException("Invalid block number detected while setting an entry for a hash block.");

            var entry = this.Entries[blockNumber % 0xAA];
            Array.Copy(entry.Hash, hashEntry.Hash, 0x14);
            entry.LevelAsUint = hashEntry.LevelAsUint;
        }

        public void SetHashForEntry(uint blockNumber, byte[] hash)
        {
            if (hash.Length != 0x14)
                throw new StfsException("Attempted to set hash with invalid length.");
            if (blockNumber == 0xffffff || blockNumber == 0xffffff)
                throw new StfsException("Invalid block number supplied when replacing a hash.");

            this.Entries[(int)blockNumber % 0xAA].Hash = hash;
        }

        public void SetLevelForEntry(uint blockNumber, uint level)
        {
            this.Entries[(int)blockNumber % 0xAA].LevelAsUint = level;
        }

        public void Save()
        {
            _io.Stream.Position = 0;

            for (int x = 0; x < 0xaa; x++)
            {
                _io.Write(this.Entries[x].Hash);
                _io.Write(this.Entries[x].LevelAsUint);
            }

            _io.Write(this.NumberOfCommittedBlocks);
            _io.Write(new byte[12]);
        }

        public void Dispose()
        {
            this.Entries.Clear();
            this._io.Close();
        }
    }

    public class StfsCacheBlock : IDisposable
    {
        public List<byte[]> Data;

        public StfsCacheBlock(int cacheBlockCount)
        {
            this.Data = new List<byte[]>();

            for (var x = 0; x < cacheBlockCount; x++)
            {
                Data.Add(new byte[0x1000]);
            }
        }

        public void Dispose()
        {
            this.Data.Clear();
        }
    }

    public class StfsVolumeExtension : IDisposable
    {
        public uint[] BlockValues = new uint[2];
        // @ 0 is the number of backing blocks per Level0 hash tree, @1 is the number of backing blocks per Level1 hash tree

        public readonly uint[] StfsDataBlocksPerHashTreeLevel = new uint[] { 0xAA, 0x70E4, 0x4AF768 };
        [Obfuscation]
        public long BackingFileOffset;
        public StfHashEntry RootHashEntry;
        public ulong BackingMaximumVolumeSize;
        public uint VolumeFlags;
        public int RootHashHierarchy;
        public int FormatShift;
        public byte BlockCacheElementCount;
        public byte BackingFilePresized;
        public byte VolumeCharacteristics;
        public uint DataBlockCount; // INT24
        public uint VolumeExtensionSize;

        public bool ReadOnly;

        public StfsCacheBlock BlockCache;
        public StfsCacheElement ElementCache;
        public int CacheHeadIndex;
        public bool InAllocationSupport;
        public ushort DirectoryAllocationBlockCount;
        public uint NumberOfFreeBlocks;
        public uint NumberOfFreePendingBlocks;

        [Obfuscation]
        public uint NumberOfTotalBlocks;

        [Obfuscation]
        public uint NumberOfExtendedBlocks;

        public uint CurrentlyExtendedBlocks;

        public StfsVolumeExtension(StfsCreatePacket createPacket)
        {
            this.ReadOnly = createPacket.VolumeDescriptor.ReadOnlyFormat != 0;

            this.VolumeFlags = (uint)(createPacket.VolumeDescriptor.Flags << 29) & 0x40000000;

            BlockValues = ReadOnly ? new uint[] { 0xAB, 0x718F } : new uint[] { 0xAC, 0x723A };

            BackingFileOffset = createPacket.BackingFileOffset;
            VolumeExtensionSize = createPacket.DeviceExtensionSize;

            uint numberOfTotalBlocks = createPacket.VolumeDescriptor.NumberOfTotalBlocks;

            if (numberOfTotalBlocks > StfsDataBlocksPerHashTreeLevel[1])
                RootHashHierarchy = 2;
            else if (numberOfTotalBlocks > StfsDataBlocksPerHashTreeLevel[0])
                RootHashHierarchy = 1;
            else if (numberOfTotalBlocks < StfsDataBlocksPerHashTreeLevel[0])
                RootHashHierarchy = 0;

            if (!ReadOnly)
            {
                BackingMaximumVolumeSize = (createPacket.BackingMaximumVolumeSize >> 0x0C);
                BackingMaximumVolumeSize = BackingMaximumVolumeSize > 0xFFFFFFFF ? 0xFFFFFFFF : BackingMaximumVolumeSize;
                this.DataBlockCount = StfsComputeNumberOfDataBlocks(BackingMaximumVolumeSize);

                if (this.DataBlockCount > 0x4AF768)
                {
                    throw new StfsException(string.Format(
                        "Detected an invalid amount of data blocks [0x{0:0x} blocks].", DataBlockCount));
                }

                uint num1 = numberOfTotalBlocks + 0xA9;
                uint num2 = num1 / 0xAA;
                if (num2 > 1)
                    num1 = (num2 + 0xA9) / 0xAA;
                else
                    num1 = 0;
                uint num3 = 0;
                if (num1 > 1)
                    num3 = (num1 + 0xA9) / 0xAA;

                NumberOfExtendedBlocks = (((num1 + num3) + num2) << 1) + numberOfTotalBlocks;
                CurrentlyExtendedBlocks = 0;
            }

            FormatShift = ReadOnly == true ? 0 : 1;

            BlockCacheElementCount = createPacket.BlockCacheElementCount;

            BackingFilePresized = createPacket.BackingFilePresized;
            VolumeCharacteristics = createPacket.DeviceCharacteristics;

            RootHashEntry = new StfHashEntry
            {
                Hash = createPacket.VolumeDescriptor.RootHash,
                LevelAsUint = this.VolumeFlags
            };
        }

        internal uint StfsComputeNumberOfDataBlocks(ulong numberOfBackingBlocks)
        {
            ulong num1 = numberOfBackingBlocks, num3;
            if (num1 > 0x4BDA85)
                return 0x4AF768;

            const ulong num2 = 0xAC;
            if (num1 < 0x723A)
            {
                if (num1 >= 0xAC)
                {
                    num1 -= 2;
                    if (num1 <= 0xB0)
                    {
                        num1 = num2;
                    }
                }
            }
            else if (num1 > 0x723A)
            {
                num1 -= 2;
                const ulong num4 = 0x723a;
                if (num1 + 2 <= 0x7240)
                {
                    num1 = num4;
                }
                num3 = num1 / num4;
                num3 = num3 * 0x723a;
                num3 = num1 - num3;
                if (num3 <= 4)
                {
                    num1 = num1 - num3;
                }
                num3 = num1 + 0x7239;
                num3 = num3 / num4;
                num3 = num3 << 1;
                num1 = num1 - num3;
            }
            num3 = num1 / num2;
            num3 = num3 * 0xAC;
            num3 = num1 - num3;
            if (num3 <= 2)
            {
                num1 = num1 - num3;
            }
            num3 = num1 + 0xAB;
            num3 = num3 / num2;
            num3 = num3 << 1;

            return (uint)(num1 - num3);
        }

        public void Dispose()
        {
            this.BlockCache.Dispose();
            this.ElementCache.Dispose();
        }
    }
}