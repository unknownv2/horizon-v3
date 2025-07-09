using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using NoDev.Common.IO;
using NoDev.Horizon.Properties;

namespace NoDev.Horizon
{
    internal static class ProfileCache
    {
        private static readonly Dictionary<ulong, ProfileData> Cache;

        internal static event EventHandler CacheChange;

        internal static void SetProfile(ProfileData profileData)
        {
            if (profileData.Gamerpic == null)
                profileData.Gamerpic = Resources.Console_64;

            if (Cache.ContainsKey(profileData.ProfileID))
            {
                profileData.Favorite = Cache[profileData.ProfileID].Favorite;
                Cache[profileData.ProfileID] = profileData;
            }
            else if (profileData.ProfileID != 0)
                Cache.Add(profileData.ProfileID, profileData);

            if (CacheChange != null)
                CacheChange.Invoke(null, null);
        }

        internal static void Remove(ulong profileId)
        {
            Cache.Remove(profileId);

            if (CacheChange != null)
                CacheChange.Invoke(null, null);
        }

        internal static void Clear()
        {
            Cache.Clear();

            if (CacheChange != null)
                CacheChange.Invoke(null, null);
        }

        internal static int Count
        {
            get { return Cache.Count; }
        }

        [Pure]
        internal static bool Contains(ulong profileId)
        {
            return Cache.ContainsKey(profileId);
        }

        internal static List<ProfileData> GetFavorites()
        {
            return Cache.Values.Where(profile => profile.Favorite).ToList();
        }

        internal static IEnumerable<ProfileData> GetAll()
        {
            return Cache.Values.ToList();
        }

        [Pure]
        internal static ProfileData GetProfile(ulong profileId)
        {
            return Cache[profileId];
        }

        static ProfileCache()
        {
            if (!Settings.ContainsFile("ProfileCache"))
            {
                Cache = new Dictionary<ulong, ProfileData>();
                return;
            }

            EndianIO io = Settings.OpenRead("ProfileCache");
            int numProfiles = io.ReadInt32();

            Cache = new Dictionary<ulong, ProfileData>(numProfiles);

            for (int x = 0; x < numProfiles; x++)
            {
                var profileData = new ProfileData(io);
                Cache.Add(profileData.ProfileID, profileData);
            }

            io.Close();
        }

        internal static void Save()
        {
            EndianIO io = Settings.OpenWrite("ProfileCache");
            io.Write(Cache.Count);
            foreach (var profile in Cache.Values)
                profile.Write(io);
            io.Close();
        }
    }
}
