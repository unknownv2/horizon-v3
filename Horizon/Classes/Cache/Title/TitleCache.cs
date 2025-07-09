using System.Collections.Generic;

namespace NoDev.Horizon
{
    internal static class TitleCache
    {
        private static readonly Dictionary<uint, Title> _titles;

        private const string CacheFile = "TitleCache";
        private const string DataFile = "Titles\\{0:x8}.dat";

        static TitleCache()
        {
            if (!Settings.ContainsFile(CacheFile))
                return;

            var io = Settings.OpenRead(CacheFile);
            int titleCount = io.ReadInt32();
            _titles = new Dictionary<uint, Title>(titleCount);
            for (int x = 0; x < titleCount; x++)
            {
                var title = new Title(io);
                _titles.Add(title.ID, title);
            }
            io.Close();
        }

        private static readonly Dictionary<uint, string> _pathCache = new Dictionary<uint, string>();
        private static string CreateDataFilePath(uint titleId)
        {
            if (!_pathCache.ContainsKey(titleId))
                _pathCache.Add(titleId, string.Format(DataFile, titleId));

            return _pathCache[titleId];
        }

        internal static bool HasData(uint titleId)
        {
            return Settings.ContainsFile(CreateDataFilePath(titleId));
        }

        internal static bool HasTitles()
        {
            return _titles != null;
        }

        internal static Title GetTitle(uint titleId)
        {
            return _titles[titleId];
        }

        internal static bool Contains(uint titleId)
        {
            return _titles.ContainsKey(titleId);
        }
    }
}
