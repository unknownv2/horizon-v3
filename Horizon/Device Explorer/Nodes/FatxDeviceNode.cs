using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NoDev.Common;
using NoDev.Horizon.Properties;
using NoDev.XContent;
using NoDev.XProfile;
using DevComponents.AdvTree;

namespace NoDev.Horizon.DeviceExplorer
{
    class FatxDeviceNode : FatxNode
    {
        private readonly Dictionary<string, FatxPackageNode> _nodeMap;
        private readonly FileSystemWatcher _fileWatcher;

        internal FatxDeviceNode(FatxDevice device)
            : base(device, true)
        {
            this._nodeMap = new Dictionary<string, FatxPackageNode>();

            this._fileWatcher = new FileSystemWatcher(device.Drive.Name);
            this._fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            this._fileWatcher.Changed += _fileWatcher_Changed;
            this._fileWatcher.Created += _fileWatcher_Created;
            this._fileWatcher.Deleted += _fileWatcher_Deleted;
            this._fileWatcher.Renamed += _fileWatcher_Renamed;
            this._fileWatcher.IncludeSubdirectories = true;
            this._fileWatcher.EnableRaisingEvents = true;
        }

        private static readonly List<string> LockedFiles = new List<string>();
        private static bool WaitForFileUnlock(string filename)
        {
            if (LockedFiles.Contains(filename))
                return false;

            LockedFiles.Add(filename);

            for (int x = 0; x < 30; x++)
            {
                try
                {
                    using (File.Open(filename, FileMode.Open))
                    {
                        LockedFiles.Remove(filename);
                        return true;
                    }
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }

            LockedFiles.Remove(filename);
            return false;
        }

        internal void AddToNodeMap(FatxPackageNode fatxNode)
        {
            string lwr = fatxNode.Package.Filename.ToLower();
            if (!this._nodeMap.ContainsKey(lwr))
                this._nodeMap.Add(lwr, fatxNode);
        }

        private void _fileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            this.OnFileDeleted(e.OldFullPath);
            this.OnFileCreated(e.FullPath);
        }

        private void _fileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            this.OnFileDeleted(e.FullPath);
            this.TreeControl.Invoke(this.UpdateCells);
        }

        private void OnFileDeleted(string filePath)
        {
            string lwr = filePath.ToLower();

            if (!this._nodeMap.ContainsKey(lwr))
            {
                this.Device.EnsureContentFolder();
                return;
            }

            var fatxNode = this._nodeMap[lwr];

            if (fatxNode.Package.Header.Metadata.ContentType == XContentTypes.Profile)
            {
                ulong profileId = fatxNode.Package.Header.Metadata.Creator;
                this.Device.Profiles.RemoveAll(p => p.ProfileID == profileId);
            }

            this._nodeMap.Remove(lwr);

            if (fatxNode.Parent is FatxTitleNode && fatxNode.Parent.Nodes.Count == 1)
            {
                var contentNode = fatxNode.Parent.Parent;
                fatxNode.Invoke(fatxNode.Parent.Remove);
                if (contentNode.Nodes.Count == 0)
                    contentNode.TreeControl.Invoke(() => contentNode.Nodes.Add(NoContentNode));
                return;
            }

            if (fatxNode.Parent.Nodes.Count == 1)
                fatxNode.Invoke(() => fatxNode.Parent.Nodes.Add(NoContentNode));

            fatxNode.Invoke(fatxNode.Remove);
        }

        private void _fileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            this.OnFileCreated(e.FullPath);
            this.UpdateCells();
        }

        private void OnFileCreated(string filePath)
        {
            if (this.Nodes.Count == 0)
                return;

            string lwr = filePath.ToLower();

            if (!File.Exists(lwr) || this._nodeMap.ContainsKey(lwr))
                return;

            int lastIndex = lwr.LastIndexOf('\\');
            if (lastIndex != -1 && lastIndex > 4 && lwr.Substring(lastIndex - 5, 5) == ".data")
                return;

            if (!WaitForFileUnlock(lwr))
                return;

            XContentPackage package = null;
            try
            {
                package = new XContentPackage(filePath);

                var metaData = package.Header.Metadata;

                uint titleId = metaData.ExecutionId.TitleID;

                if (metaData.ContentType == XContentTypes.Profile && titleId != ProfileFile.TitleID)
                    throw new Exception(string.Format("Profile has invalid title ID (0x{0:X8}).", titleId));

                uint realTitleId = TitleControl.GetProperTitleID(package);

                if (realTitleId == 0)
                    throw new Exception("Cannot get proper title ID of package.");

                metaData.ExecutionId.TitleID = realTitleId;

                if (package.Filename.ToLower() != lwr)
                    throw new Exception("Package not in correct device folder.");

                metaData.ExecutionId.TitleID = titleId;

                var contentType = metaData.ContentType;

                if (contentType == XContentTypes.Profile)
                {
                    ulong profileId = metaData.Creator;
                    this.Device.Profiles.RemoveAll(p => p.ProfileID == profileId);
                    var profileInfo = new ProfileInfo();
                    profileInfo.Fill(package);
                    this.Device.Profiles.Add(profileInfo);
                    this.Device.SortProfiles();
                }

                for (int x = 0; x < this.Nodes.Count; x++)
                {
                    var contentNode = (FatxContentNode)this.Nodes[x];

                    if (!ContentTypeHelper.GetContentTypes(contentNode.GeneralContentType).Contains(contentType))
                        continue;

                    if (contentNode.Nodes.Count == 0)
                        continue;

                    contentNode.Invoke(() => contentNode.Insert(package));
                }
            }
            catch
            {

            }

            if (package != null)
                package.Close();

            this.Invoke(this.UpdateCells);
        }

        private void _fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            string lwr = e.FullPath.ToLower();

            if (!this._nodeMap.ContainsKey(lwr))
                return;

            var fatxNode = this._nodeMap[lwr];

            if (fatxNode.Package.IsOpened)
            {
                fatxNode.Invoke(() => {
                    fatxNode.UpdateImage();
                    fatxNode.UpdateCells();
                });
                
            }
            else
            {
                // Package modified outsite of Horizon.
                this.OnFileDeleted(e.FullPath);
                this.OnFileCreated(e.FullPath);
            }

            this.Invoke(this.UpdateCells);
        }

        protected override void BeforeCellEdit(CellEditEventArgs e)
        {
            e.Cell.Text = ((FatxDeviceNode)e.Cell.Parent).Device.Name;
        }

        protected override void AfterCellEdit(CellEditEventArgs e)
        {
            ((FatxDeviceNode)e.Cell.Parent).Device.Name = e.NewText;
            e.NewText = Cell0Text;
        }

        internal override string ExtractText
        {
            get { return "Extract All Content"; }
        }

        protected override void BeforeExpand()
        {
            foreach (GeneralContentType contentType in Enum.GetValues(typeof(GeneralContentType)))
            {
                var contentNode = new FatxContentNode(this.Device, contentType);

                this.Nodes.Add(contentNode);

                if (contentType == GeneralContentType.Gamer_Profiles)
                    contentNode.Populate();
            }
        }

        internal override async Task GetPackages(List<XContentPackage> packages)
        {
            this.BeforeExpand();

            foreach (FatxContentNode contentNode in this.Nodes)
                await contentNode.GetPackages(packages);
        }

        private string Cell0Text
        {
            get { return Device.Name + LineBreak + CreateGrayText("Storage Device"); }
        }

        internal override void UpdateCells()
        {
            Cells[0].Text = Cell0Text;
            Cells[1].Text = CreateGrayText(Formatting.GetSizeFromBytes(Device.Drive.TotalFreeSpace) + " Free");
        }

        internal override void UpdateImage()
        {
            this.Image = Resources.FatxHDD_24;
        }
    }
}
