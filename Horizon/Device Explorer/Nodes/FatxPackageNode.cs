using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using NoDev.Common;
using NoDev.Horizon.Properties;
using NoDev.XContent;
using DevComponents.AdvTree;

namespace NoDev.Horizon.DeviceExplorer
{
    internal class FatxPackageNode : FatxNode
    {
        internal readonly XContentPackage Package;

        internal FatxPackageNode(FatxDevice device, XContentPackage package)
            : base(device, false)
        {
            this.Editable = false;
            this.Package = package;
        }

        internal override string ExtractText
        {
            get { return "Extract Selected File"; }
        }

        internal override void UpdateCells()
        {
            XContentMetadata metaData = Package.Header.Metadata;
            string displayName = metaData.DisplayName.Trim();
            this.Cells[0].Text = (displayName.Length == 0 ? "No Name" : displayName) + LineBreak;
            if (metaData.ContentType == XContentTypes.Installer)
                this.Cells[0].Text += CreateGrayText(String.Format("Version {0}.0", metaData.ExecutionId.Version));
            else
            {
                string contentType = metaData.ContentType == XContentTypes.Arcade ? "Xbox LIVE<br></br>Arcade Game" : ContentTypeHelper.ContentTypeToString(metaData.ContentType);
                this.Cells[0].Text += CreateGrayText(contentType);
                if (metaData.ContentType == XContentTypes.SavedGame)
                {
                    this.Cells[0].Text += LineBreak + CreateGrayText("Profile: ");
                    if (ProfileCache.Contains(metaData.Creator))
                        this.Cells[0].Text += ProfileCache.GetProfile(metaData.Creator).Gamertag;
                    else
                        this.Cells[0].Text += "Unknown";
                }
            }

            var file = new FileInfo(this.Package.Filename);

            FillInfoCell(this.Cells[1], file.CreationTime, file.LastWriteTime, this.Package.Header.Metadata.VolumeType == XContentVolumeType.SVOD ? this.Package.Header.Metadata.DataFilesSize : (ulong)file.Length);
        }

        internal override void UpdateImage()
        {
            this.Image = this.ResizeImage(this.Package.Header.Metadata.Thumbnail.ToImage() ?? Resources.Console_64);
        }

        protected Image ResizeImage(Image image)
        {
            var b = new Bitmap(32, 41);
            var g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, 0, 5, 32, 32);
            g.Dispose();
            return b;
        }

        private const string TimeFormat = "M/d/yyyy";
        protected void FillInfoCell(Cell cell, DateTime creationTime, DateTime modifiedTime, ulong fileSize)
        {
            cell.Text = CreateGrayText("Size: ") + Formatting.GetSizeFromBytes(fileSize) + LineBreak;
            cell.Text += CreateGrayText("Created: ") + creationTime.ToString(TimeFormat) + LineBreak;
            cell.Text += CreateGrayText("Modified: ") + modifiedTime.ToString(TimeFormat);
        }
    }
}
