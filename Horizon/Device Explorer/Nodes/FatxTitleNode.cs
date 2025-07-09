using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NoDev.Horizon.Properties;
using NoDev.XContent;

namespace NoDev.Horizon.DeviceExplorer
{
    class FatxTitleNode : FatxNode
    {
        internal readonly uint TitleId;

        internal FatxTitleNode(FatxDevice device, uint titleId)
            : base(device, true)
        {
            this.Editable = false;
            this.TitleId = titleId;
        }

        internal void Insert(XContentPackage package)
        {
            this.OnPackageAdded(package);
        }

        internal override async Task GetPackages(List<XContentPackage> packages)
        {
            await Device.GetPackagesAsync(CreatePackageFilter(), packages.Add);
        }

        protected override async void BeforeExpand()
        {
            this.Nodes.Add(LoadingNode);

            await Device.GetPackagesAsync(CreatePackageFilter(), OnPackageAdded);

            this.OnAddingFinishedDefault();
        }

        internal override string ExtractText
        {
            get { return "Extract Game Content"; }
        }

        private FatxPackageFilter CreatePackageFilter()
        {
            var filter = new FatxPackageFilter();
            filter.ContentTypes = ContentTypeHelper.GetContentTypes(GeneralContentType.Games);
            filter.SetProfileFilterType(ProfileFilterType.All, Device);
            filter.TitleID = this.TitleId;
            return filter;
        }

        private void OnPackageAdded(XContentPackage packageMeta)
        {
            var fatxNode = new FatxPackageNode(Device, packageMeta);
            ((FatxDeviceNode)this.Parent.Parent).AddToNodeMap(fatxNode);

            this.Invoke(() => {
                this.RemoveDisabledNodes();
                this.Nodes.Add(fatxNode);
                this.Nodes.Sort();
            });
        }

        internal override void UpdateCells()
        {
            if (TitleNameCache.Contains(this.TitleId))
                this.Cells[0].Text = TitleNameCache.GetTitleName(this.TitleId);
            else
                this.Cells[0].Text = string.Format("Title {0:X8}", this.TitleId);
            this.Cells[1].Text = CreateGrayText("Title ID: ") + this.TitleId.ToString("X8");
        }

        internal override void UpdateImage()
        {
            bool editorExists = ControlManager.Editors.Any(editorInfo => editorInfo.Group == ControlGroup.Game && editorInfo.TitleID == this.TitleId);

            this.Image = editorExists ? Resources.Dot_Green : Resources.Dot_Gray;
        }
    }
}
