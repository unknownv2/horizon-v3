using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using NoDev.Horizon.Properties;
using NoDev.XContent;
using NoDev.XProfile;

namespace NoDev.Horizon.DeviceExplorer
{
    internal class ProfileCollection : List<ProfileInfo>
    {
        internal bool Contains(ulong profileId)
        {
            return this.Find(p => p.ProfileID == profileId) != null;
        }

        internal ProfileInfo this[ulong profileId]
        {
            get
            {
                ProfileInfo profileInfo = this.Find(p => p.ProfileID == profileId);
                if (profileInfo == null)
                    throw new Exception(string.Format("Profile not found in collection [0x{0:X8}]", profileId));
                return profileInfo;
            }
        }
    }

    internal class ProfileInfo
    {
        internal XContentPackage Package { get; private set; }

        internal ulong ProfileID;
        internal string Gamertag;
        internal Image Gamerpic;
        internal ulong XUID;

        internal bool Corrupted;

        internal bool Unknown
        {
            get
            {
                return this.Package == null;
            }
        }

        internal bool UnknownOrCorrupted
        {
            get
            {
                return this.Unknown || this.Corrupted;
            }
        }

        internal void Fill(string filename)
        {
            try
            {
                var package = new XContentPackage(filename);
                this.Fill(package);
                package.UnMount();
                package.Close();
            }
            catch
            {
                this.SetCorrupted();
            }
        }

        private void SetCorrupted()
        {
            this.Corrupted = true;
            this.Gamertag = "Corrupted Profile";
        }

        internal void SetUnknown()
        {
            this.Package = null;
            this.Gamertag = "Unknown Profile";
            this.Gamerpic = Resources.QuestionMark_64;
        }

        internal void Fill(XContentPackage package)
        {
            this.Package = package;

            this.ProfileID = Package.Header.Metadata.Creator;

            this.Gamerpic = package.Header.Metadata.Thumbnail.ToImage() ?? Resources.Console_64;

            package.Open();

            if (!package.IsMounted)
                package.Mount();

            string accountPath = package.Drive.Name + "Account";
            if (!File.Exists(accountPath))
                this.SetCorrupted();
            else
            {
                try
                {
                    var acct = new XProfileAccount(File.ReadAllBytes(accountPath));
                    string gt = XProfileAccount.FilterGamertag(acct.Gamertag).Trim();
                    this.Gamertag = gt.Length == 0 ? "No Name" : gt;
                    this.XUID = acct.XuidOnline;
                }
                catch
                {
                    this.SetCorrupted();
                }
            }
        }
    }
}
