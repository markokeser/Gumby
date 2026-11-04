namespace Gumby.Services
{
    /// <summary>
    /// Jedinstveni kodovi komandi koje se upisuju na NFC čip (format: "kod|vrednost").
    /// Koriste ih i stranice za upis i <see cref="ChipService"/> pri čitanju,
    /// tako da se kodovi ne mogu razići.
    /// </summary>
    public static class CommandCodes
    {
        public const string Url = "0";
        public const string Volume = "1";
        public const string Call = "2";
        public const string Sms = "3";
        public const string OpenApp = "4";
        public const string SetAlarm = "5";
        public const string StartTimer = "6";
        public const string Wifi = "7";
        public const string Contact = "8";
        public const string Bluetooth = "10";
        public const string Spotify = "11";
        public const string YouTube = "12";
        public const string Instagram = "13";
        public const string GoogleMaps = "14";

        /// <summary>
        /// Omotač za komandu čija vrednost sadrži separator komandi.
        /// Format: "B64|&lt;base64 cele komande&gt;".
        /// </summary>
        public const string Base64 = "B64";

        /// <summary>Separator između više komandi na jednom čipu.</summary>
        public const char TaskSeparator = '\x1F';
    }
}
