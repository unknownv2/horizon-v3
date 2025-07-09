using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CallbackFS;
using NoDev.Common.IO;
using NoDev.Stfs;
using NoDev.Svod;
using NoDev.XContent;
using NoDev.XProfile;
using NoDev.XProfile.Records;
using NoDev.XProfile.Trackers;
using NoDev.Xdbf;

namespace Test
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

            XContentPackage.RegisterDevice(XContentVolumeType.STFS, typeof(StfsDevice));
            XContentPackage.RegisterDevice(XContentVolumeType.SVOD, typeof(SvodDevice));
        }

        private void cmdProfileExtractAll_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() != DialogResult.OK)
                return;

            var profilePaths = Directory.GetFiles(fbd.SelectedPath);

            foreach (var profilePath in profilePaths)
            {
                var profileDir = Path.Combine(fbd.SelectedPath, Path.GetFileName(profilePath) + " - Dump");

                var package = new XContentPackage(profilePath);
                package.Mount();

                ExtractDataFilesRecords(profileDir, package.Drive.RootDirectory.GetFiles());

                string pecPath = package.Drive.Name + "PEC";

                if (File.Exists(pecPath))
                {
                    profileDir += "\\__PEC";

                    var pec = new ProfileEmbeddedContent(new EndianIO(pecPath, EndianType.Big));

                    ExtractDataFilesRecords(profileDir, pec.Drive.RootDirectory.GetFiles());

                    pec.Close();
                }

                package.Close();
            }
        }

        private void ExtractDataFilesRecords(string baseDir, IEnumerable<FileInfo> dataFiles)
        {
            foreach (var gpdFile in dataFiles)
            {
                if (gpdFile.Extension != ".gpd")
                    continue;

                string fileName = gpdFile.Name;

                if (fileName[0] == 'F')
                    fileName = "_" + fileName;

                var gpdDir = Path.Combine(baseDir, fileName);

                Directory.CreateDirectory(gpdDir);

                File.Copy(gpdFile.FullName, gpdDir + "\\_" + fileName);

                var dataFile = new DataFile(gpdFile.FullName, DataFileOrigin.PEC);

                dataFile.ExtractRecords(gpdDir);

                dataFile.Close();
            }
        }

        private async void cmdAddTitleRecords_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            string fileName = ofd.FileName + " - Titles";

            if (File.Exists(fileName))
                File.Delete(fileName);

            File.Copy(ofd.FileName, fileName);

            await Task.Run(() => this.AddTitles(fileName));
        }

        private void AddTitles(string fileName)
        {
            var package = new XContentPackage(fileName);

            package.Mount();

            var profile = new ProfileFile(package.Drive, package.Header.Metadata.Creator);

            foreach (var titleIdStr in this.txtTitleIds.Lines)
            {
                uint titleId = uint.Parse(titleIdStr.Substring(2, 8), NumberStyles.HexNumber);

                if (profile.TitleRecordExists(titleId))
                    continue;

                var titleRecord = new TitleRecord(titleId, titleIdStr.Substring(13), 10, 1000, 0, 0, 0);

                profile.AddTitleRecord(titleRecord);
            }

            profile.Close();

            package.SaveIfModified();

            package.Close();
        }

        private void panelMountStfs_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private XContentPackage _mountedPackage;
        private void panelMountStfs_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length == 0)
                return;

            if (_mountedPackage != null)
                cmdCloseStfs_Click(null, null);

            _mountedPackage = new XContentPackage(files[0]);

            _mountedPackage.Mount();

            Process.Start(_mountedPackage.Drive.Name);
        }

        private void cmdCloseStfs_Click(object sender, EventArgs e)
        {
            if (_mountedPackage == null)
                return;

            _mountedPackage.SaveIfModified();

            _mountedPackage.Close();

            _mountedPackage = null;
        }

        private void cmdCreateGameAdderData_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var package = new XContentPackage(ofd.FileName);

            package.Mount();

            var profile = new ProfileFile(package.Drive, package.Header.Metadata.Creator);

            var titles = profile.GetAllTitleRecords().ToList();

            titles.Sort((t1, t2) => string.CompareOrdinal(t1.TitleName, t2.TitleName));

            var io = new EndianIO(ofd.FileName + ".gad", EndianType.Little, FileMode.Create);

            foreach (var title in titles)
            {
                if (title.TitleID == ProfileFile.TitleID)
                    continue;

                io.Write(title.TitleID);
                io.Write(title.TitleName.Length);
                io.WriteUnicodeString(title.TitleName);
                io.Write(title.AchievementsPossible);
                io.Write(title.CreditPossible);
                io.Write(title.AllAvatarAwards.Possible);
                io.Write(title.MaleAvatarAwards.Possible);
                io.Write(title.FemaleAvatarAwards.Possible);

                if (title.AchievementsPossible != 0)
                {
                    var achs = new AchievementTracker(profile, title.TitleID);

                    if (title.AchievementsPossible != achs.Achievements.Count)
                        throw new Exception();

                    io.Write(achs.Achievements.Count);

                    foreach (var ach in achs.Achievements)
                    {
                        io.Write(ach.ID);
                        io.Write(ach.ImageID);
                        io.Write(ach.Credit);
                        io.Write(ach.Label.Length);
                        io.WriteUnicodeString(ach.Label);
                        io.Write(ach.Description.Length);
                        io.WriteUnicodeString(ach.Description);
                        io.Write(ach.UnachievedDescription.Length);
                        io.WriteUnicodeString(ach.UnachievedDescription);
                    }

                    achs.Close();
                }

                if (title.AllAvatarAwards.Possible != 0)
                {
                    var aws = new AvatarAwardTracker(profile, title.TitleID);

                    if (title.AllAvatarAwards.Possible != aws.Awards.Count)
                        throw new Exception();

                    io.Write(aws.Awards.Count);

                    foreach (var aw in aws.Awards)
                    {
                        io.Write(aw.ID);
                        io.Write(aw.Reserved);
                        io.Write(aw.ImageID);
                        io.Write(aw.Name.Length);
                        io.WriteUnicodeString(aw.Name);
                        io.Write(aw.Description.Length);
                        io.WriteUnicodeString(aw.Description);
                        io.Write(aw.UnawardedDescription.Length);
                        io.WriteUnicodeString(aw.UnawardedDescription);
                    }

                    aws.Close();
                }
            }

            io.Close();

            profile.Close();

            package.Close();
        }

        private void cmdInstallDriver_Click(object sender, EventArgs e)
        {
            uint i = 0;
            CallbackFileSystem.Install(@"C:\Program Files (x86)\EldoS\Callback File System\Drivers\cbfs.cab", "44D14AF2-249C-408D-ACD3-A0BA7C8E1E11", null, false, 0x00020001, ref i);
        }

        private void cmdRegisterPublicKey_Click(object sender, EventArgs e)
        {
            Process.Start("\"C:\\Program Files (x86)\\Microsoft SDKs\\Windows\\v8.0A\\bin\\NETFX 4.0 Tools\\sn.exe\" -Vr *,c6e1a43c64dc7910");
        }
    }
}
