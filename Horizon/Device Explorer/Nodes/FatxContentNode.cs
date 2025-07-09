using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NoDev.Horizon.Properties;
using NoDev.XContent;

namespace NoDev.Horizon.DeviceExplorer
{
    internal enum GeneralContentType
    {
        Games,
        Gamer_Profiles,
        Demos,
        Videos,
        Themes,
        Gamer_Pictures,
        Avatar_Items,
        Title_Updates,
        System_Items
    }

    class FatxContentNode : FatxNode
    {
        internal readonly GeneralContentType GeneralContentType;

        internal FatxContentNode(FatxDevice device, GeneralContentType generalContentType)
            : base(device, true)
        {
            this.Editable = false;
            this.GeneralContentType = generalContentType;
        }

        internal override void UpdateCells()
        {
            Cells[0].Text = GeneralContentType.ToString().Replace("_", " ");
        }

        internal override void UpdateImage()
        {
            this.Image = Resources.Folder_Closed_16;
            this.ImageExpanded = Resources.Folder_Opened_16;
        }

        private void OnTitleAdded(uint titleId)
        {
            var n = new FatxTitleNode(Device, titleId);
            this.Invoke(() => {
                this.RemoveDisabledNodes();
                this.Nodes.Add(n);
                this.Nodes.Sort();
            });
        }

        internal override async Task GetPackages(List<XContentPackage> packages)
        {
            switch (GeneralContentType)
            {
                case GeneralContentType.Games:
                    var filter = new FatxPackageFilter();
                    filter.ContentTypes = ContentTypeHelper.GetContentTypes(GeneralContentType);
                    filter.SetProfileFilterType(ProfileFilterType.All, Device);
                    await Device.GetPackagesAsync(filter, packages.Add);
                    break;
                case GeneralContentType.Gamer_Profiles:
                    packages.AddRange(from profileInfo in Device.Profiles where !profileInfo.Unknown select profileInfo.Package);
                    break;
                default:
                    await Device.GetPackagesAsync(CreatePackageFilter(), packages.Add);
                    break;
            }
        }

        internal void Insert(XContentPackage package)
        {
            this.RemoveDisabledNodes();
            if (this.GeneralContentType == GeneralContentType.Games)
            {
                bool handled = false;
                uint titleId = TitleControl.GetProperTitleID(package);
                foreach (FatxTitleNode titleNode in this.Nodes)
                {
                    if (titleNode.TitleId != titleId)
                        continue;

                    if (titleNode.Nodes.Count != 0)
                        titleNode.Insert(package);

                    handled = true;

                    break;
                }

                if (!handled)
                {
                    string titleName = package.Header.Metadata.TitleName;
                    if (titleName.Length != 0 && !TitleNameCache.Contains(titleId))
                        TitleNameCache.AddTitle(titleId, package.Header.Metadata.TitleName);
                    this.Nodes.Add(new FatxTitleNode(this.Device, titleId));
                }
            }
            else if (this.GeneralContentType == GeneralContentType.Gamer_Profiles)
            {
                ulong profileId = package.Header.Metadata.Creator;
                this.InsertProfile(this.Device.Profiles.First(p => p.ProfileID == profileId));
            }
            else
            {
                this.OnPackageAdded(package);
            }
        }

        internal void Populate()
        {
            this.BeforeExpand();
        }

        protected override async void BeforeExpand()
        {
            switch (GeneralContentType)
            {
                case GeneralContentType.Games:
                    this.Nodes.Add(LoadingNode);

                    var titleIds = await Device.GetTitlesAsync(ContentTypeHelper.GetContentTypes(GeneralContentType.Games));

                    titleIds.ForEach(this.OnTitleAdded);

                    this.OnAddingFinishedDefault();
                    break;
                case GeneralContentType.Gamer_Profiles:
                    foreach (var profileInfo in Device.Profiles.Where(profileInfo => !profileInfo.Unknown))
                        this.InsertProfile(profileInfo);
                    if (this.Nodes.Count == 0)
                        this.Nodes.Add(NoContentNode);
                    break;
                default:
                    this.Nodes.Add(LoadingNode);
                    await Device.GetPackagesAsync(CreatePackageFilter(), OnPackageAdded);
                    this.OnAddingFinishedDefault();
                    break;
            }
        }

        private void InsertProfile(ProfileInfo profileInfo)
        {
            var fatxNode = new FatxProfileNode(Device, profileInfo);
            ((FatxDeviceNode)this.Parent).AddToNodeMap(fatxNode);
            this.Nodes.Add(fatxNode);
            this.Nodes.Sort();
        }

        internal override string ExtractText
        {
            get { return "Extract " + GeneralContentType.ToString().Replace("_", " "); }
        }

        private FatxPackageFilter CreatePackageFilter()
        {
            var filter = new FatxPackageFilter();
            filter.ContentTypes = ContentTypeHelper.GetContentTypes(GeneralContentType);
            filter.SetProfileFilterType(ContentTypeHelper.IsPublicGeneralContent(GeneralContentType) ? ProfileFilterType.PublicOnly : ProfileFilterType.All, Device);
            return filter;
        }

        private void OnPackageAdded(XContentPackage package)
        {
            var fatxNode = new FatxPackageNode(Device, package);
            ((FatxDeviceNode)this.Parent).AddToNodeMap(fatxNode);

            this.Invoke(() => {
                this.RemoveDisabledNodes();
                this.Nodes.Add(fatxNode);
                this.Nodes.Sort();
            });
        }
    }
}
