//#define PARALLEL_PROCESSING

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NoDev.Common;
using NoDev.Common.IO;
using NoDev.XContent;

namespace NoDev.Horizon.DeviceExplorer
{
    internal class FatxDevice
    {
        internal string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.Drive.VolumeLabel))
                    return "Memory Device";
                return this.Drive.VolumeLabel;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    this.Drive.VolumeLabel = value;
            }
        }

        internal readonly ProfileCollection Profiles;
        internal readonly DriveInfo Drive;

        private DirectoryInfo _content;

        internal FatxDevice(DriveInfo driveInfo)
        {
            this.Drive = driveInfo;

            this.Profiles = new ProfileCollection();

            this.EnsureContentFolder();

            this.GetProfiles();
        }

        internal void EnsureContentFolder()
        {
            if (this._content != null && this._content.Exists)
                return;

            this._content = null;

            var dirInfo = this.Drive.RootDirectory.GetDirectories();
            foreach (var dir in dirInfo.Where(dir => dir.Name == "Content"))
            {
                this._content = dir;
                break;
            }

            if (this._content == null)
                this._content = this.Drive.RootDirectory.CreateSubdirectory("Content");
        }

        internal ProfileCollection GetProfiles()
        {
            this.Profiles.Clear();

            if (!this._content.Exists)
                return null;

            foreach (var profileEntry in this._content.GetDirectories())
            {
                ulong profileId;
                if (profileEntry.Name.Length != 16 || !Numbers.TryParseUInt64Hex(profileEntry.Name, out profileId) || profileId == 0)
                    continue;

                var profileInfo = new ProfileInfo();
                profileInfo.ProfileID = profileId;

                var filePath = this.FormatProfileFilePath(profileId);
                if (!File.Exists(filePath))
                    profileInfo.SetUnknown();
                else
                {
                    profileInfo.Fill(filePath);
                    if (!profileInfo.Corrupted)
                        ProfileCache.SetProfile(new ProfileData(profileInfo.Gamertag, profileInfo.ProfileID, profileInfo.XUID, profileInfo.Gamerpic));
                }
                this.Profiles.Add(profileInfo);
            }

            ProfileCache.Save();

            this.SortProfiles();

            return this.Profiles;
        }

        internal void SortProfiles()
        {
            this.Profiles.Sort((p1, p2) => string.Compare(p1.Gamertag, p2.Gamertag, StringComparison.Ordinal));
        }

        internal async Task<List<uint>> GetTitlesAsync(XContentTypes[] filter)
        {
            return await Task.Run(() => this.GetTitles(filter));
        }

        internal List<uint> GetTitles(XContentTypes[] filter)
        {
            var usedTitleIds = new List<uint>();

            if (!this._content.Exists)
                return usedTitleIds;
            
            foreach (var profileDir in this._content.GetDirectories())
            {
                ulong profileId;
                if (profileDir.Name.Length != 16 || !Numbers.TryParseUInt64Hex(profileDir.Name, out profileId))
                    continue;

                foreach (var titleDir in profileDir.GetDirectories())
                {
                    uint titleId;
                    if (titleDir.Name.Length != 8 || !Numbers.TryParseUInt32Hex(titleDir.Name, out titleId) || usedTitleIds.Contains(titleId))
                        continue;

                    if (TitleNameCache.Contains(titleId))
                    {
                        usedTitleIds.Add(titleId);
                        continue;
                    }

                    foreach (var typeDir in titleDir.GetDirectories())
                    {
                        int typeId;
                        if (typeDir.Name.Length != 8 || !Numbers.TryParseInt32Hex(typeDir.Name, out typeId))
                            continue;

                        if (filter != null && !filter.Any(contentType => (int)contentType == typeId))
                            continue;

                        bool foundTitleName = false;

                        foreach (var packageFile in typeDir.GetFiles())
                        {
                            try
                            {
                                var io = new EndianIO(packageFile.OpenRead(), EndianType.Big);
                                io.Position = 0x1691;
                                string titleName = io.ReadUnicodeString(64).RemoveNullBytes();
                                io.Close();

                                if (titleName.Length != 0)
                                {
                                    foundTitleName = true;
                                    usedTitleIds.Add(titleId);
                                    if (!TitleNameCache.Contains(titleId))
                                        TitleNameCache.AddTitle(titleId, titleName);
                                    break;
                                }
                            }
                            catch
                            {
                                // Bad package. Don't have to handle this.
                            }
                        }

                        if (foundTitleName)
                            break;
                    }
                }
            }

            TitleNameCache.Save();

            return usedTitleIds;
        }

        internal async Task GetPackagesAsync(FatxPackageFilter filter, Action<XContentPackage> progressUpdate)
        {
            await Task.Run(() => this.GetPackages(filter, progressUpdate));
        }

        internal void GetPackages(FatxPackageFilter filter, Action<XContentPackage> progressUpdate)
        {
            if (!this._content.Exists)
                return;

            uint filterTitleId = filter.TitleID;

            var profileIds = filter.GetProfileIDs();

#if PARALLEL_PROCESSING
            Parallel.ForEach(this._content.GetDirectories(), profileDir =>
#else
            foreach (var profileDir in this._content.GetDirectories())
#endif
            {

                ulong profileId;
                if (profileDir.Name.Length != 16 || !Numbers.TryParseUInt64Hex(profileDir.Name, out profileId))
                    return;

                if (!profileIds.Contains(profileId))
                    return;

#if PARALLEL_PROCESSING
                Parallel.ForEach(profileDir.GetDirectories(), titleDir =>
#else
                foreach (var titleDir in profileDir.GetDirectories())
#endif
                {
                    uint titleId;
                    if (titleDir.Name.Length != 8 || !Numbers.TryParseUInt32Hex(titleDir.Name, out titleId))
                        return;

                    if (filterTitleId != 0 && filterTitleId != titleId)
                        return;

                    foreach (var typeDir in titleDir.GetDirectories())
                    {
                        int contentType;
                        if (typeDir.Name.Length != 8 || !Numbers.TryParseInt32Hex(typeDir.Name, out contentType))
                            continue;

                        if (filter.ContentTypes != null && !filter.ContentTypes.Contains((XContentTypes) contentType))
                            continue;

                        foreach (FileInfo file in typeDir.GetFiles())
                        {
                            try
                            {
                                var package = new XContentPackage(file.FullName);
                                if (!TitleNameCache.Contains(titleId) && package.Header.Metadata.TitleName.Length != 0)
                                    TitleNameCache.AddTitle(titleId, package.Header.Metadata.TitleName);
                                package.Close();
                                progressUpdate(package);
                            }
                            catch
                            {
                                // Bad package. Don't have to handle this.
                            }
                        }
                    }
#if PARALLEL_PROCESSING
                });
            });
#else
                }
            }
#endif
        }

        internal string FormatPath(ulong profileId)
        {
            return string.Format(@"{0}Content\{1:X16}", Drive.Name, profileId);
        }

        internal string FormatPath(ulong profileId, uint titleId)
        {
            return string.Format(@"{0}\{1:X8}", FormatPath(profileId), titleId);
        }

        internal string FormatPath(ulong profileId, uint titleId, XContentTypes contentType)
        {
            return string.Format(@"{0}\{1:X8}", FormatPath(profileId, titleId), (int)contentType);
        }

        internal string FormatPath(ulong profileId, uint titleId, XContentTypes contentType, string fileName)
        {
            return string.Format(@"{0}\{1}", FormatPath(profileId, titleId, contentType), fileName);
        }

        internal string FormatProfileFilePath(ulong profileId)
        {
            return this.FormatPath(profileId, 0xfffe07d1, XContentTypes.Profile, profileId.ToString("X16"));
        }
    }
}
