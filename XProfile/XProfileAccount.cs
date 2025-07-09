using System;
using System.IO;
using NoDev.Common.IO;
using NoDev.Xbox360;

namespace NoDev.XProfile
{
    public class XProfileAccount
    {
        public static string FilterGamertag(string gamertag)
        {
            if (gamertag == null)
                throw new XProfileException("Attempted to filter a null gamertag.");

            for (int x = 0; x < gamertag.Length; x++)
            {
                char c = gamertag[x];
                if (c == '^')
                {
                    if (x == gamertag.Length - 1)
                        gamertag = gamertag.Remove(x, 1);
                    else
                    {
                        c = gamertag[x + 1];
                        gamertag = gamertag.Remove(c >= '0' && c <= '9' ? 2 : 1);
                        x--;
                    }
                }
                else if (c < ' ' || c > '~' || c == '>' || c == '<' || c == '/')
                    gamertag = gamertag.Remove(x--, 1);
            }
            return gamertag;
        }

        public bool DeveloperAccount;
        public uint OnlineServiceNetworkID;
        public ulong XuidOnline;
        private byte[] _onlineKey, _passcode;

        public string
            Gamertag,
            OnlineDomain,
            OnlineKerberosRealm,
            UserPassportMembername,
            UserPassportPassword,
            OwnerPassportMembername;

        private uint
            _liveFlags,
            _cachedUserFlags,
            _reserved;

        private const uint
            MembershipTypeMask = 0x1f00000,
            XOnlineCountryMask = 0xff00,
            XOnlineLanguageMask = 0x3e000000;

        private const uint
            PasswordProtectedFlag = 0x10000000,
            XboxLiveEnabledMask = 0x20000000,
            RecoveringMask = 0x40000000;

        public bool Recovering
        {
            get { return (this._liveFlags & RecoveringMask) != 0x00; }
            set { this.SetLiveFlag(RecoveringMask, value); }
        }

        public bool XboxLiveEnabled
        {
            get { return (this._liveFlags & XboxLiveEnabledMask) != 0x00; }
            set { this.SetLiveFlag(XboxLiveEnabledMask, value); }
        }

        public bool PasswordProtected
        {
            get { return (this._liveFlags & PasswordProtectedFlag) != 0x00; }
            set { this.SetLiveFlag(PasswordProtectedFlag, value); }
        }

        private void SetLiveFlag(uint mask, bool value)
        {
            if (value)
                this._liveFlags |= mask;
            else
                this._liveFlags &= ~mask;
        }

        public XOnlineTierType MembershipType
        {
            get { return (XOnlineTierType)(this._cachedUserFlags & MembershipTypeMask); }
            set { SetCachedUserFlag(MembershipTypeMask, (uint)value); }
        }

        public XOnlineCountry XCountry
        {
            get { return (XOnlineCountry)(this._cachedUserFlags & XOnlineCountryMask); }
            set { SetCachedUserFlag(XOnlineCountryMask, (uint)value); }
        }

        public XOnlineLanguage XLanguage
        {
            get { return (XOnlineLanguage)(this._cachedUserFlags & XOnlineLanguageMask); }
            set { SetCachedUserFlag(XOnlineLanguageMask, (uint)value); }
        }

        private void SetCachedUserFlag(uint mask, uint value)
        {
            this._cachedUserFlags = (this._cachedUserFlags & ~mask) | (value & mask);
        }

        public void SetPasscode(XOnlinePassCodeType[] passcode)
        {
            if (passcode.Length != 0x04)
                throw new XProfileException("Passcode must be 4 buttons long.");

            this._passcode[0] = (byte)passcode[0];
            this._passcode[1] = (byte)passcode[1];
            this._passcode[2] = (byte)passcode[2];
            this._passcode[3] = (byte)passcode[3];
        }

        public XOnlinePassCodeType[] GetPasscode()
        {
            return new[]
            {
                (XOnlinePassCodeType)this._passcode[0],
                (XOnlinePassCodeType)this._passcode[1],
                (XOnlinePassCodeType)this._passcode[2],
                (XOnlinePassCodeType)this._passcode[3]
            };
        }

        public XProfileAccount(byte[] accountBuffer)
        {
            byte[] data = XeCrypt.XeKeysUnObfuscate(1, accountBuffer, false);

            if (data == null)
            {
                data = XeCrypt.XeKeysUnObfuscate(1, accountBuffer, true);

                if (data == null)
                    throw new XProfileException("Account file is corrupted.");

                this.DeveloperAccount = true;
            }

            var io = new EndianIO(data, EndianType.Big);
            this._liveFlags = io.ReadUInt32();
            this._reserved = io.ReadUInt32();
            this.Gamertag = io.ReadUnicodeString(16).RemoveNullBytes();
            this.XuidOnline = io.ReadUInt64();
            this._cachedUserFlags = io.ReadUInt32();
            this.OnlineServiceNetworkID = io.ReadUInt32();
            this._passcode = io.ReadByteArray(4);
            this.OnlineDomain = io.ReadAsciiString(20).RemoveNullBytes();
            this.OnlineKerberosRealm = io.ReadAsciiString(24).RemoveNullBytes();
            this._onlineKey = io.ReadByteArray(16);
            this.UserPassportMembername = io.ReadAsciiString(114).RemoveNullBytes();
            this.UserPassportPassword = io.ReadAsciiString(32).RemoveNullBytes();
            this.OwnerPassportMembername = io.ReadAsciiString(114).RemoveNullBytes();
            io.Close();
        }

        public byte[] ToArray()
        {
            var io = new EndianIO(new MemoryStream(404), EndianType.Big);
            io.Write(this._liveFlags);
            io.Write(this._reserved);
            io.WriteUnicodeString(this.Gamertag, 16);
            io.Write(this.XuidOnline);
            io.Write(this._cachedUserFlags);
            io.Write(this.OnlineServiceNetworkID);
            io.Write(this._passcode);
            io.WriteAsciiString(this.OnlineDomain, 20);
            io.WriteAsciiString(this.OnlineKerberosRealm, 24);
            io.Write(this._onlineKey);
            io.WriteAsciiString(this.UserPassportMembername, 114);
            io.WriteAsciiString(this.UserPassportPassword, 32);
            io.WriteAsciiString(this.OwnerPassportMembername, 114);
            io.Close();

            return XeCrypt.XeKeysObfuscate(1, io.ToArray(), this.DeveloperAccount);
        }
    }
}