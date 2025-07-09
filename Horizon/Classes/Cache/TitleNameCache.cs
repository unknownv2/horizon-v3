using System.Collections.Generic;
using Newtonsoft.Json;

namespace NoDev.Horizon
{
    internal static class TitleNameCache
    {
        private static readonly Dictionary<uint, string> Cache;
        private static bool _cacheUpdated;

        static TitleNameCache()
        {
            string titleNameCache = Settings.ReadString("TitleNameCache");

            if (titleNameCache == null)
            {
                Cache = new Dictionary<uint, string>();
                Cache.Add(0xfffe07d1, "Xbox 360 Dashboard");
                _cacheUpdated = true;
            }
            else
                Cache = JsonConvert.DeserializeObject<Dictionary<uint, string>>(titleNameCache);
        }

        internal static void Save()
        {
            if (!_cacheUpdated)
                return;

            Settings.WriteString("TitleNameCache", JsonConvert.SerializeObject(Cache, Formatting.Indented));
        }

        internal static string GetTitleName(uint titleId)
        {
            return Cache[titleId];
        }

        internal static void AddTitle(uint titleId, string titleName)
        {
            Cache.Add(titleId, titleName);
            _cacheUpdated = true;
        }

        internal static bool Contains(uint titleId)
        {
            return Cache.ContainsKey(titleId);
        }

        internal static void Clear()
        {
            Cache.Clear();
        }
    }
}
