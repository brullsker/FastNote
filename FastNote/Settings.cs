using Common;
using Windows.Storage;

namespace FastNote
{
    public class Settings : ObservableSettings
    {
        private static Settings settings = new Settings();
        public static Settings Default
        {
            get { return settings; }
        }

        public Settings()
            : base(ApplicationData.Current.LocalSettings)
        {
        }

        [DefaultSettingValue(Value = true)]
        public bool ThemeDefault
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = false)]
        public bool ThemeDark
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = false)]
        public bool ThemeLight
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = "FastNote Export")]
        public string DefaultExportName
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = "FastNote Share")]
        public string DefaultShareName
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = true)]
        public bool SpellCheckEnabled
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = 0)]
        public int ShareFileType
        {
            get { return Get<int>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = 2)]
        public int ImportOption
        {
            get { return Get<int>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = 0)]
        public int LanguageIndex
        {
            get { return Get<int>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = false)]
        public bool MenuOpenOnStartup
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = 0)]
        public int FontFamily
        {
            get { return Get<int>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = 15)]
        public int FontSize
        {
            get { return Get<int>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = true)]
        public bool FontFamSegFS
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = false)]
        public bool ToolBarOnBottomDesktop
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = true)]
        public bool ToolBarOnBottomMobile
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = true)]
        public bool ShowAd
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [DefaultSettingValue(Value = 0)]
        public int HTMLEncoding
        {
            get { return Get<int>(); }
            set { Set(value); }
        }
    }
}