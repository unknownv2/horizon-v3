using System;
using System.Drawing;
using System.Threading.Tasks;
using NoDev.Horizon.Properties;
using NoDev.XContent;

namespace NoDev.Horizon.DeviceExplorer
{
    sealed class FatxPackageItem : FatxItem
    {
        internal FatxPackageItem(FatxDevice device, XContentPackage package, Func<XContentPackage, Task<bool>> onClick)
            : base(device)
        {
            this.ImageFixedSize = new Size(32, 32);

            this.Click += async delegate { await onClick(package); };

            this.Image = package.Header.Metadata.Thumbnail.ToImage() ?? Resources.Console_64;

            this.Text = package.Header.Metadata.DisplayName + LineBreak
                + CreateGrayText(ContentTypeHelper.ContentTypeToString(package.Header.Metadata.ContentType));

            if (package.Header.Metadata.ContentType == XContentTypes.SavedGame)
            {
                string creator = " for ";
                if (ProfileCache.Contains(package.Header.Metadata.Creator))
                    creator += ProfileCache.GetProfile(package.Header.Metadata.Creator).Gamertag;
                else
                    creator += "Unknown";
                this.Text += CreateGrayText(creator);
            }
        }
    }
}
