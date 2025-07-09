using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NoDev.XProfile;

namespace NoDev.Horizon.Editors.Account_Editor
{
    [EditorInfo(0x08, "Account Editor", "Thumb_Generic_PageGear", Group.Guest | Group.Regular | Group.Diamond | Group.Tester | Group.Developer, ControlGroup.Profile)]
    internal partial class AccountEditor : ProfileEditor
    {
        internal AccountEditor()
        {
            InitializeComponent();

            cbService.Items.Add("Xbox LIVE");
            cbService.Items.Add("PartnerNet");
            cbService.Items.Add("Other...");

            Array passCodes = Enum.GetValues(typeof(XOnlinePassCodeType));
            foreach (XOnlinePassCodeType passCode in passCodes)
            {
                string codeStr = PassCodeToString(passCode);
                cbP1.Items.Add(codeStr);
                cbP2.Items.Add(codeStr);
                cbP3.Items.Add(codeStr);
                cbP4.Items.Add(codeStr);
            }
        }

        private const string NoPassCode = "--";
        private static string PassCodeToString(XOnlinePassCodeType passCode)
        {
            if (passCode == (byte)XOnlinePassCodeType.None)
                return NoPassCode;

            return ((PassCodeTypes)passCode).ToString();
        }

        private enum PassCodeTypes : byte
        {
            NA = 0x00,
            Du = 0x01,
            Dd = 0x02,
            Dl = 0x03,
            Dr = 0x04,
            X = 0x05,
            Y = 0x06,
            LT = 0x09,
            RT = 0x0A,
            LB = 0x0B,
            RB = 0x0C
        };

        protected override void Open()
        {
            ckDevAccount.Checked = Account.DeveloperAccount;
            this.LoadService();
            txtGamertag.Text = Account.Gamertag;
            txtXuid.Text = Account.XuidOnline.ToString("X16");
            txtDomain.Text = Account.OnlineDomain;
            txtKerberosRealm.Text = Account.OnlineKerberosRealm;
            ckLiveEnabled.Checked = Account.XboxLiveEnabled;
            ckRecovering.Checked = Account.Recovering;
            this.ckLiveEnabled_CheckedChanged(null, null);
            ckPasswordProtected.Checked = Account.PasswordProtected;
            txtService.Enabled = cbService.SelectedIndex == 2;
            this.LoadPassCodes();
        }

        private void LoadService()
        {
            byte[] netId = BitConverter.GetBytes(Account.OnlineServiceNetworkID);
            Array.Reverse(netId);
            string netIdStr = Encoding.ASCII.GetString(netId);
            txtService.Text = netIdStr;

            if (netIdStr == "PROD")
                cbService.SelectedIndex = 0;
            else if (netIdStr == "PART")
                cbService.SelectedIndex = 1;
            else if (netIdStr.Length == 0)
                cbService.SelectedIndex = ckDevAccount.Checked ? 1 : 0;
            else
                cbService.SelectedIndex = 2;
        }

        private void LoadPassCodes()
        {
            var p = Account.GetPasscode();
            cbP1.Text = PassCodeToString(p[0]);
            cbP2.Text = PassCodeToString(p[1]);
            cbP3.Text = PassCodeToString(p[2]);
            cbP4.Text = PassCodeToString(p[3]);
        }

        private XOnlinePassCodeType StringToPassCode(string passCode)
        {
            if (passCode == NoPassCode)
                return XOnlinePassCodeType.None;

            return (XOnlinePassCodeType)Enum.Parse(typeof(PassCodeTypes), passCode);
        }

        protected override void Save()
        {
            if (cbService.Enabled && txtService.Text.Length != 4)
                throw new Exception("The service ID must be 4 characters!");

            if (ckPasswordProtected.Checked && (cbP1.SelectedIndex == 0 || cbP2.SelectedIndex == 0 || cbP3.SelectedIndex == 0 || cbP4.SelectedIndex == 0))
                throw new Exception("All four pass codes must be set!");

            Account.Gamertag = txtGamertag.Text;
            Account.DeveloperAccount = ckDevAccount.Checked;
            if (cbService.Enabled)
            {
                byte[] serviceId = Encoding.ASCII.GetBytes(txtService.Text);
                Array.Reverse(serviceId);
                Account.OnlineServiceNetworkID = BitConverter.ToUInt32(serviceId, 0);
            }
            else
                Account.OnlineServiceNetworkID = 0;

            Account.XboxLiveEnabled = ckLiveEnabled.Checked;
            Account.Recovering = ckRecovering.Checked;
            Account.PasswordProtected = ckPasswordProtected.Checked;
            Account.XuidOnline = ulong.Parse(txtXuid.Text, System.Globalization.NumberStyles.HexNumber);

            Account.SetPasscode(new[]
            {
                StringToPassCode(cbP1.Text),
                StringToPassCode(cbP2.Text),
                StringToPassCode(cbP3.Text),
                StringToPassCode(cbP4.Text)
            });

            Account.OnlineDomain = txtDomain.Text;
            Account.OnlineKerberosRealm = txtKerberosRealm.Text;

            this.Profile.SaveAccount();

            if (!ProfileCache.Contains(this.Package.Header.Metadata.Creator))
                return;

            ProfileData profileData = ProfileCache.GetProfile(Package.Header.Metadata.Creator);
            profileData.XUID = Account.XuidOnline;
            profileData.Gamertag = Account.Gamertag;
            ProfileCache.SetProfile(profileData);
        }

        private void ckLiveEnabled_CheckedChanged(object sender, EventArgs e)
        {
            cbService.Enabled = ckLiveEnabled.Checked;
            txtService.Enabled = ckLiveEnabled.Checked;
            ckRecovering.Enabled = ckLiveEnabled.Checked;

            if (ckLiveEnabled.Checked)
            {
                ckRecovering.Checked = Account.Recovering;
                LoadService();
            }
            else
            {
                cbService.SelectedIndex = 2;
                txtService.Text = string.Empty;
                ckRecovering.Checked = false;
            }
        }

        private static bool _warningShown;
        private void txtGamertag_Click(object sender, EventArgs e)
        {
            if (_warningShown)
                return;

            DialogBox.Show("This will not let you change your online gamertag for free.\n"
                + "It only changes the information in the account file.\n\n"
                + "Modifying this information may prohibit you from signing into Xbox LIVE.",
                "Gamertag Changes", MessageBoxIcon.Information);
            _warningShown = true;
        }

        private void ckPasswordProtected_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = ckPasswordProtected.Checked;
            cbP1.Enabled = isChecked;
            cbP2.Enabled = isChecked;
            cbP3.Enabled = isChecked;
            cbP4.Enabled = isChecked;

            if (isChecked)
            {
                this.LoadPassCodes();
            }
            else
            {
                cbP1.Text = NoPassCode;
                cbP2.Text = NoPassCode;
                cbP3.Text = NoPassCode;
                cbP4.Text = NoPassCode;
            }
        }

        private void cbService_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtService.Enabled = cbService.SelectedIndex == 2;
            if (cbService.SelectedIndex == 0)
                txtService.Text = "PROD";
            else if (cbService.SelectedIndex == 1)
                txtService.Text = "PART";
        }
    }
}
