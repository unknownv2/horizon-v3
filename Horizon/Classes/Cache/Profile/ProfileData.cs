using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NoDev.Common.IO;
using NoDev.Horizon.Properties;

namespace NoDev.Horizon
{
    internal class ProfileData
    {
        internal readonly ulong ProfileID;
        internal string Gamertag;
        internal ulong XUID;
        internal bool Favorite;
        internal Image Gamerpic;

        internal ProfileData(string gamertag, ulong profileId, ulong xuid, Image gamerpic)
        {
            this.Gamertag = gamertag;
            this.ProfileID = profileId;
            this.XUID = xuid;
            this.Gamerpic = gamerpic;
            this.Favorite = false;
        }

        internal ProfileData(EndianIO io)
        {
            this.Gamertag = io.ReadAsciiString(io.ReadInt32());
            this.ProfileID = io.ReadUInt64();
            this.XUID = io.ReadUInt64();
            this.Favorite = io.ReadBoolean();

            int imgLength = io.ReadInt32();
            this.Gamerpic = imgLength == 0 ? Resources.Console_64 : io.ReadByteArray(imgLength).ToImage();
        }

        internal void Write(EndianIO io)
        {
            io.Write(this.Gamertag.Length);
            io.WriteAsciiString(this.Gamertag);
            io.Write(this.ProfileID);
            io.Write(this.XUID);
            io.Write(this.Favorite);

            if (this.Gamerpic == Resources.Console_64)
            {
                io.Write(0);
                return;
            }

            var imageStream = new MemoryStream();
            this.Gamerpic.Save(imageStream, ImageFormat.Png);
            imageStream.Close();

            byte[] imageBytes = imageStream.ToArray();
            io.Write(imageBytes.Length);
            io.Write(imageBytes);
        }
    }
}
