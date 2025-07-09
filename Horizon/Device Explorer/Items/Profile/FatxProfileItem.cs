using System.Drawing;

namespace NoDev.Horizon.DeviceExplorer
{
    abstract class FatxProfileItem : FatxItem
    {
        internal readonly ProfileInfo Profile;

        internal FatxProfileItem(FatxDevice device, ProfileInfo profile)
            : base(device)
        {
            this.Profile = profile;
            this.ImageFixedSize = new Size(32, 32);
            this.SetData();
        }

        private void SetData()
        {
            this.Text = Profile.Gamertag + LineBreak + CreateGrayText(Profile.ProfileID.ToString("X16"));
            this.Image = Profile.Gamerpic;
        }
    }
}
