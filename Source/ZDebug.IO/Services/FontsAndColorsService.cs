using System.Windows;
using System.Windows.Media;

namespace ZDebug.IO.Services
{
    public static class FontsAndColorsService
    {
        private static readonly FontFamily normalFontFamily = new FontFamily("Segoe UI");
        private static readonly FontFamily fixedFontFamily = new FontFamily("Consolas");
        private static readonly double fontSize = 14.0;

        private static readonly Brush defaultForeground = Brushes.Black;
        private static readonly Brush defaultBackground = Brushes.White;

        private static Brush foreground = defaultForeground;
        private static Brush background = defaultBackground;

        private static Typeface normalTypeface;
        private static Typeface fixedTypeface;

        public static FontFamily NormalFontFamily
        {
            get { return normalFontFamily; }
        }

        public static FontFamily FixedFontFamily
        {
            get { return fixedFontFamily; }
        }

        public static double FontSize
        {
            get { return fontSize; }
        }

        public static Brush DefaultForeground
        {
            get { return defaultForeground; }
        }

        public static Brush DefaultBackground
        {
            get { return defaultBackground; }
        }

        public static Brush Foreground
        {
            get { return foreground; }
            set { foreground = value; }
        }

        public static Brush Background
        {
            get { return background; }
            set { background = value; }
        }

        public static Typeface NormalTypeface
        {
            get
            {
                if (normalTypeface == null)
                {
                    normalTypeface = new Typeface(normalFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                }

                return normalTypeface;
            }
        }

        public static Typeface FixedTypeface
        {
            get
            {
                if (fixedTypeface == null)
                {
                    fixedTypeface = new Typeface(fixedFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                }

                return fixedTypeface;
            }
        }

        private static readonly FontAndColorSetting defaultSetting =
            new FontAndColorSetting(new FontFamily("Consolas"), 15.0, background: Brushes.Transparent);
        private static readonly FontAndColorSetting addressSetting =
            new FontAndColorSetting(defaultSetting, foreground: Brushes.Gray);
        private static readonly FontAndColorSetting commentSetting =
            new FontAndColorSetting(defaultSetting, foreground: Brushes.DarkGreen);
        private static readonly FontAndColorSetting constantSetting =
            new FontAndColorSetting(defaultSetting, foreground: Brushes.Peru);
        private static readonly FontAndColorSetting globalVariableSetting =
            new FontAndColorSetting(defaultSetting, foreground: Brushes.Teal);
        private static readonly FontAndColorSetting keywordSetting =
            new FontAndColorSetting(defaultSetting, foreground: Brushes.MediumBlue);
        private static readonly FontAndColorSetting localVariableSetting =
            new FontAndColorSetting(defaultSetting, foreground: Brushes.Teal);
        private static readonly FontAndColorSetting separatorSetting =
            new FontAndColorSetting(defaultSetting, foreground: Brushes.DarkGray);
        private static readonly FontAndColorSetting stackVariableSetting =
            new FontAndColorSetting(defaultSetting, foreground: Brushes.Teal);
        private static readonly FontAndColorSetting ztextSetting =
            new FontAndColorSetting(defaultSetting, new FontFamily("Times New Roman"), 15.5, Brushes.Maroon, new SolidColorBrush(Color.FromRgb(0xff, 0xff, 0xe6)));

        public static FontAndColorSetting DefaultSetting
        {
            get { return defaultSetting; }
        }

        public static FontAndColorSetting AddressSetting
        {
            get { return addressSetting; }
        }

        public static FontAndColorSetting CommentSetting
        {
            get { return commentSetting; }
        }

        public static FontAndColorSetting ConstantSetting
        {
            get { return constantSetting; }
        }

        public static FontAndColorSetting GlobalVariableSetting
        {
            get { return globalVariableSetting; }
        }

        public static FontAndColorSetting KeywordSetting
        {
            get { return keywordSetting; }
        }

        public static FontAndColorSetting LocalVariableSetting
        {
            get { return localVariableSetting; }
        }

        public static FontAndColorSetting SeparatorSetting
        {
            get { return separatorSetting; }
        }

        public static FontAndColorSetting StackVariableSetting
        {
            get { return stackVariableSetting; }
        }

        public static FontAndColorSetting ZTextSetting
        {
            get { return ztextSetting; }
        }
    }
}
