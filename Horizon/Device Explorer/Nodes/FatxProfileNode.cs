using System.IO;

namespace NoDev.Horizon.DeviceExplorer
{
    internal class FatxProfileNode : FatxPackageNode
    {
        internal readonly ProfileInfo ProfileInfo;

        internal FatxProfileNode(FatxDevice device, ProfileInfo profileInfo)
            : base(device, profileInfo.Package)
        {
            this.ProfileInfo = profileInfo;
        }

        internal override string ExtractText
        {
            get { return "Extract Selected Profile"; }
        }

        internal override void UpdateCells()
        {
            this.Cells[0].Text = ProfileInfo.Gamertag + LineBreak + CreateGrayText(ProfileInfo.ProfileID.ToString("X16"));

            var file = new FileInfo(ProfileInfo.Package.Filename);
            this.FillInfoCell(this.Cells[1], file.CreationTime, file.LastWriteTime, (ulong)file.Length);
        }

        internal override void UpdateImage()
        {
            this.Image = this.ResizeImage(ProfileInfo.Gamerpic);
        }
    }
}
