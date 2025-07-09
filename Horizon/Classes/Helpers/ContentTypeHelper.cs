using System.Collections.Generic;
using System.Diagnostics.Contracts;
using NoDev.Horizon.DeviceExplorer;
using NoDev.XContent;

namespace NoDev.Horizon
{
    internal static class ContentTypeHelper
    {
        private static readonly Dictionary<GeneralContentType, XContentTypes[]> GeneralContentTypeMap;

        static ContentTypeHelper()
        {
            GeneralContentTypeMap = new Dictionary<GeneralContentType, XContentTypes[]>(10);

            GeneralContentTypeMap.Add(GeneralContentType.Games, new[] {
                XContentTypes.SavedGame,
                XContentTypes.Marketplace,
                XContentTypes.Publisher,
                XContentTypes.XNA,
                XContentTypes.XNACommunity,
                XContentTypes.InstalledXbox360Title,
                XContentTypes.XboxTitle,
                XContentTypes.SocialTitle,
                XContentTypes.Xbox360Title,
                XContentTypes.XboxSavedGame,
                XContentTypes.XboxDownload,
                XContentTypes.GameTitle,
                XContentTypes.Arcade
            });

            GeneralContentTypeMap.Add(GeneralContentType.Gamer_Profiles, new[] {
                XContentTypes.Profile
            });

            GeneralContentTypeMap.Add(GeneralContentType.Demos, new[] {
                XContentTypes.GameDemo
            });

            GeneralContentTypeMap.Add(GeneralContentType.Videos, new[] {
                XContentTypes.ViralVideo,
                XContentTypes.Video,
                XContentTypes.GameTrailer,
                XContentTypes.Movie,
                XContentTypes.TV,
                XContentTypes.MusicVideo,
                XContentTypes.PodcastVideo,
                XContentTypes.Promotional
            });

            GeneralContentTypeMap.Add(GeneralContentType.Themes, new[] {
                XContentTypes.ThematicSkin
            });

            GeneralContentTypeMap.Add(GeneralContentType.Gamer_Pictures, new[] {
                XContentTypes.GamerPicture
            });

            GeneralContentTypeMap.Add(GeneralContentType.Avatar_Items, new[] {
                XContentTypes.AvatarAsset
            });

            GeneralContentTypeMap.Add(GeneralContentType.Title_Updates, new[] {
                XContentTypes.Installer
            });

            GeneralContentTypeMap.Add(GeneralContentType.System_Items, new[] {
                XContentTypes.IPTVDVR,
                XContentTypes.IPTVPauseBuffer,
                XContentTypes.SystemUpdateStoragePack,
                XContentTypes.Cache,
                XContentTypes.StorageDownload,
                XContentTypes.LicenseStore,
                XContentTypes.Unknown
            });
        }

        internal static bool IsPublicGeneralContent(GeneralContentType generalContentType)
        {
            return generalContentType == GeneralContentType.Demos
                || generalContentType == GeneralContentType.Avatar_Items
                || generalContentType == GeneralContentType.System_Items
                || generalContentType == GeneralContentType.Videos;
        }

        [Pure]
        internal static XContentTypes[] GetContentTypes(GeneralContentType generalContentType)
        {
            return GeneralContentTypeMap[generalContentType];
        }

        internal static string ContentTypeToString(XContentTypes contentType)
        {
            switch (contentType)
            {
                case XContentTypes.SavedGame:
                    return "Saved Game";
                case XContentTypes.Marketplace:
                    return "Marketplace Content";
                case XContentTypes.Publisher:
                    return "Publisher Data";
                case XContentTypes.IPTVDVR:
                    return "IPTV DVR Data";
                case XContentTypes.IPTVPauseBuffer:
                    return "IPTV Pause Buffer";
                case XContentTypes.XNA:
                case XContentTypes.XNACommunity:
                    return "Indie Game";
                case XContentTypes.InstalledXbox360Title:
                    return "Installed Game";
                case XContentTypes.XboxTitle:
                    return "Original Xbox Game";
                case XContentTypes.SocialTitle:
                    return "Social Game";
                case XContentTypes.Xbox360Title:
                    return "Games on Demand";
                case XContentTypes.SystemUpdateStoragePack:
                    return "System Update Storage";
                case XContentTypes.AvatarAsset:
                    return "Avatar Item";
                case XContentTypes.Profile:
                    return "Gamer Profile";
                case XContentTypes.ThematicSkin:
                    return "Theme";
                case XContentTypes.GamerPicture:
                    return "Gamer Picture Pack";
                case XContentTypes.Cache:
                    return "Cached Data";
                case XContentTypes.StorageDownload:
                    return "Storage Download";
                case XContentTypes.XboxSavedGame:
                    return "Xbox Saved Game";
                case XContentTypes.XboxDownload:
                    return "Xbox Download";
                case XContentTypes.GameDemo:
                    return "Game Demo";
                case XContentTypes.ViralVideo:
                case XContentTypes.Video:
                    return "Video";
                case XContentTypes.GameTitle:
                    return "Game";
                case XContentTypes.Installer:
                    return "Title Update";
                case XContentTypes.GameTrailer:
                    return "Game Trailer";
                case XContentTypes.Arcade:
                    return "Xbox LIVE Arcade Game";
                case XContentTypes.LicenseStore:
                    return "License Storage";
                case XContentTypes.Movie:
                    return "Movie";
                case XContentTypes.TV:
                    return "TV Content";
                case XContentTypes.MusicVideo:
                    return "Music Video";
                case XContentTypes.Promotional:
                    return "Promotional Video";
                case XContentTypes.PodcastVideo:
                    return "Podcast Video";
                default:
                    return "Content";
            }
        }

        internal static string ContentTypeToPluralString(XContentTypes contentType)
        {
            switch (contentType)
            {
                case XContentTypes.SavedGame:
                    return "Saved Games";
                case XContentTypes.Marketplace:
                    return "Marketplace Content";
                case XContentTypes.Publisher:
                    return "Publisher Data";
                case XContentTypes.IPTVDVR:
                    return "IPTV DVR Data";
                case XContentTypes.IPTVPauseBuffer:
                    return "IPTV Pause Buffers";
                case XContentTypes.XNA:
                case XContentTypes.XNACommunity:
                    return "Indie Games";
                case XContentTypes.InstalledXbox360Title:
                    return "Installed Games";
                case XContentTypes.XboxTitle:
                    return "Original Xbox Games";
                case XContentTypes.SocialTitle:
                    return "Social Games";
                case XContentTypes.Xbox360Title:
                    return "Games on Demand";
                case XContentTypes.SystemUpdateStoragePack:
                    return "System Update Storage";
                case XContentTypes.AvatarAsset:
                    return "Avatar Items";
                case XContentTypes.Profile:
                    return "Gamer Profiles";
                case XContentTypes.ThematicSkin:
                    return "Themes";
                case XContentTypes.GamerPicture:
                    return "Gamer Picture Packs";
                case XContentTypes.Cache:
                    return "Cached Data";
                case XContentTypes.StorageDownload:
                    return "Storage Downloads";
                case XContentTypes.XboxSavedGame:
                    return "Xbox Saved Games";
                case XContentTypes.XboxDownload:
                    return "Xbox Downloads";
                case XContentTypes.GameDemo:
                    return "Game Demos";
                case XContentTypes.ViralVideo:
                case XContentTypes.Video:
                    return "Videos";
                case XContentTypes.GameTitle:
                    return "Games";
                case XContentTypes.Installer:
                    return "Title Updates";
                case XContentTypes.GameTrailer:
                    return "Game Trailers";
                case XContentTypes.Arcade:
                    return "Xbox LIVE Arcade Games";
                case XContentTypes.LicenseStore:
                    return "License Storage";
                case XContentTypes.Movie:
                    return "Movies";
                case XContentTypes.TV:
                    return "TV Content";
                case XContentTypes.MusicVideo:
                    return "Music Videos";
                case XContentTypes.Promotional:
                    return "Promotional Videos";
                case XContentTypes.PodcastVideo:
                    return "Podcast Videos";
                default:
                    return "Content";
            }
        }
    }
}
