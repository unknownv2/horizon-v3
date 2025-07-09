using System.Drawing;
using NoDev.Common.IO;
using NoDev.XProfile;

namespace NoDev.Horizon.Editors.Avatar_Color_Editor
{
    [EditorInfo(0x06, "Avatar Color Editor", "Thumb_Generic_Person", Group.All, ControlGroup.Profile)]
    internal partial class AvatarColorEditor : ProfileEditor
    {
        internal AvatarColorEditor()
        {
            InitializeComponent();
        }

        private const long ColorPosition = 0xfc;

        protected override void Open()
        {
            IO = new EndianIO(Profile.Settings.GetBinaryValue(XProfileID.GamercardAvatarInfo1), EndianType.Big);
            IO.Stream.Position = ColorPosition;
            cpSkin.SelectedColor = Color.FromArgb(IO.ReadInt32());
            cpHair.SelectedColor = Color.FromArgb(IO.ReadInt32());
            cpLip.SelectedColor = Color.FromArgb(IO.ReadInt32());
            cpEye.SelectedColor = Color.FromArgb(IO.ReadInt32());
            cpEyeBrow.SelectedColor = Color.FromArgb(IO.ReadInt32());
            cpEyeShadow.SelectedColor = Color.FromArgb(IO.ReadInt32());
            cpFaceHair.SelectedColor = Color.FromArgb(IO.ReadInt32());
            cpFacePaint.SelectedColor = Color.FromArgb(IO.ReadInt32());
            cpFacePaint2.SelectedColor = Color.FromArgb(IO.ReadInt32());
        }

        protected override void Save()
        {
            IO.Stream.Position = ColorPosition;
            IO.Write(cpSkin.SelectedColor.ToArgb());
            IO.Write(cpHair.SelectedColor.ToArgb());
            IO.Write(cpLip.SelectedColor.ToArgb());
            IO.Write(cpEye.SelectedColor.ToArgb());
            IO.Write(cpEyeBrow.SelectedColor.ToArgb());
            IO.Write(cpEyeShadow.SelectedColor.ToArgb());
            IO.Write(cpFaceHair.SelectedColor.ToArgb());
            IO.Write(cpFacePaint.SelectedColor.ToArgb());
            IO.Write(cpFacePaint2.SelectedColor.ToArgb());
            Profile.Settings.UpdateOrCreateBinaryValue(XProfileID.GamercardAvatarInfo1, IO.ToArray());
        }
    }
}
