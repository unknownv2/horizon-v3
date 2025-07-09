
namespace NoDev.XProfile
{
    public static class XProfileID
    {
        public const int
            WebConnectionSpeed = 0x1004200b,
            WebEmailFormat = 0x10042000,
            WebFavoriteGame = 0x10042004,
            WebFavoriteGame1 = 0x10042005,
            WebFavoriteGame2 = 0x10042006,
            WebFavoriteGame3 = 0x10042007,
            WebFavoriteGame4 = 0x10042008,
            WebFavoriteGame5 = 0x10042009,
            WebFavoriteGenre = 0x10042003,
            WebFlags = 0x10042001,
            WebFlash = 0x1004200c,
            WebPlatformsOwned = 0x1004200a,
            WebSpam = 0x10042002,
            WebVideoPreference = 0x1004200d,
            MusicBackgroundLargePublic = 0x406403fe,
            MusicBackgroundSmallPublic = 0x406403fd,
            MusicBio = 0x43e803fa,
            MusicBackgroundImage = 0x100403f3,
            MusicLastChangeTime = 0x700803f4,
            MusicMediaMotto = 0x410003f6,
            MusicMediaPicture = 0x406403e8,
            MusicMediaStyle1 = 0x100403ea,
            MusicMediaStyle2 = 0x100403eb,
            MusicMediaStyle3 = 0x100403ec,
            MusicOfflineID = 0x603403f2,
            MusicTopAlbum1 = 0x100403ed,
            MusicTopAlbum2 = 0x100403ee,
            MusicTopAlbum3 = 0x100403ef,
            MusicTopAlbum4 = 0x100403f0,
            MusicTopAlbum5 = 0x100403f1,
            MusicTopMediaID1 = 0x601003f7,
            MusicTopMediaID2 = 0x601003f8,
            MusicTopMediaID3 = 0x601003f9,
            MusicTopMusic = 0x60a803f5,
            FriendsAppShowBuddies = 0x1004003e,
            GameActionAutoAim = 0x10040022,
            GamerActionAutoCenter = 0x10040023,
            GamerActionMovementControl = 0x10040024,
            GamerControlSensitivity = 0x10040018,
            GamerDifficulty = 0x10040015,
            GamerPreferredColor1 = 0x1004001d,
            GamerPreferredColor2 = 0x1004001e,
            GamerPresenceUserState = 0x10040007,
            GamerRaceAcceleratorControl = 0x10040029,
            GamerRaceBrakeControl = 0x10040028,
            GamerRaceCameraLocation = 0x10040027,
            GamerRaceTransmission = 0x10040026,
            GamerTier = 0x1004003a, // obsolete
            GamerType = 0x10040001,
            GamerLastSubscriptionDate = 0x70080049,
            GamerYAxisInversion = 0x10040002,
            GamercardAchievementsEarned = 0x10040013,
            GamercardAvatarInfo1 = 0x63e80044,
            GamercardAvatarInfo2 = 0x63e80045,
            GamercardCreditEarned = 0x10040006,
            GamercardHasVision = 0x10040008,
            GamercardMotto = 0x402c0011,
            GamercardPartyInfo = 0x60800046,
            GamercardPersonalPicture = 0x40640010,
            GamercardPictureKey = 0x4064000f,
            GamercardRegion = 0x10040005,
            GamercardReputation = 0x5004000b,
            GamercardServiceTypeFlags = 0x1004003f,
            //GamercardTitleAchievementsEarned = 0x10040039,
            //GamercardTitleCreditEarned = 0x10040038,
            GamercardTitlesPlayed = 0x10040012,
            GamercardUserBio = 0x43e80043,
            GamercardUserLocation = 0x40520041,
            GamercardUserName = 0x41040040,
            GamercardUserURL = 0x41900042,
            GamercardZone = 0x10040004,
            GamercardTenureLevel = 0x10040047,
            GamercardTenureMilestone = 0x10040048,
            MessengerAutoSignIn = 0x1004003c,
            MessengerSignupState = 0x1004003b,
            OptionControllerVibration = 0x10040003,
            OptionVoiceMuted = 0x1004000c,
            OptionVoiceThroughSpeaker = 0x1004000d,
            OptionVoiceVolume = 0x1004000e,
            Permissions = 0x10040000,
            SaveWindowsLivePassword = 0x1004003d,
            TitleSpecific1 = 0x63e83fff,
            TitleSpecific2 = 0x63e83ffe,
            TitleSpecific3 = 0x63e83ffd;
    }

    internal enum GamercardZone
    {
        Xbox1,
        Recreation,
        Pro,
        Family,
        Underground
    }

    public enum Platform : uint
    {
        Xbox360 = 0x00100000,
        PC = 0x00200000
    }

    public enum AchievementType : uint
    {
        Completion = 1,
        Leveling,
        Unlock,
        Event,
        Tournament,
        Checkpoint,
        Other
    }

    public enum AssetBodyType : byte
    {
        Unknown,
        Male,
        Female,
        Both
    }

    public enum AssetGuidType
    {
        Custom,
        TOC,
        Awardable,
        MarketPlace
    }

    public enum AssetCategory
    {
        Animation = 0x400000,
        BlendShapes = 0x1380000,
        Body = 2,
        Carryable = 0x1000,
        Chin = 0x100000,
        Clothing = 0x801ff8,
        Costume = 0x800000,
        Earrings = 0x400,
        Ears = 0x200000,
        Eyebrows = 0x4000,
        Eyes = 0x2000,
        EyeShadow = 0x40000,
        FacialHair = 0x10000,
        FacialOther = 0x20000,
        Glasses = 0x100,
        Gloves = 0x80,
        Hair = 4,
        Hat = 0x40,
        Head = 1,
        Models = 0x1fff,
        Mouth = 0x8000,
        None = 0,
        Nose = 0x80000,
        Ring = 0x800,
        Shape = 0x1000000,
        Shirt = 8,
        Shoes = 0x20,
        Textures = 0x7e000,
        Trousers = 0x10,
        Valid = 0x1ffffff,
        Wristwear = 0x200
    }

    public enum XOnlineLanguage : byte
    {
        Null,
        English,
        Japanese,
        German,
        French,
        Spanish,
        Italian,
        Korean,
        Chinese,
        Portuguese,
        Chinese2,
        Polish,
        Russian,
        Danish,
        Finnish,
        Norwegian,
        Dutch,
        Swedish,
        Czech,
        Greek,
        Hungarian
    }

    public enum XOnlineCountry : byte
    {
        Null = 0,
        Austria = 5,
        Australia = 6,
        Belgium = 8,
        Brazil = 0xd,
        Canada = 0x10,
        Switzerland = 0x12,
        Chile = 0x13,
        China = 0x14,
        Colombia = 0x15,
        CzechRepublic = 0x17,
        Germany = 0x18,
        Denmark = 0x19,
        Spain = 0x1f,
        Finland = 0x20,
        France = 0x22,
        GreatBritain = 0x23,
        Greece = 0x25,
        HongKong = 0x27,
        Hungary = 0x2a,
        Ireland = 0x2c,
        India = 0x2e,
        Italy = 0x32,
        Japan = 0x35,
        Korea = 0x38,
        Mexico = 0x47,
        Netherlands = 0x4a,
        Norway = 0x4b,
        NewZealand = 0x4c,
        Poland = 0x52,
        PuertoRico = 0x54,
        Russia = 0x58,
        Sweeden = 0x5a,
        Singapore = 0x5b,
        Taiwan = 0x65,
        UnitedStates = 0x67,
        SouthAfrica = 0x6d
    }

    public enum XOnlinePassCodeType : byte
    {
        None = 0x00,
        DpadUp = 0x01,
        DpadDown = 0x02,
        DpadLeft = 0x03,
        DpadRight = 0x04,
        GamepadX = 0x05,
        GamepadY = 0x06,
        GamepadLeftTrigger = 0x09,
        GamepadRightTrigger = 0x0A,
        GamepadLeftShoulder = 0x0B,
        GamepadRightShoulder = 0x0C
    }

    public enum XOnlineTierType
    {
        Invalid = 0x0,
        Silver = 0x3,
        Gold = 0x6
    }
}
