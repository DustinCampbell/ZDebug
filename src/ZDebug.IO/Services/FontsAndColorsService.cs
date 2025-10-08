using System.Windows;
using System.Windows.Media;

namespace ZDebug.IO.Services;

public static class FontsAndColorsService
{
    private static readonly FontFamily normalFontFamily = new FontFamily("Cambria");
    private static readonly FontFamily fixedFontFamily = new FontFamily("Consolas");
    private static readonly double fontSize = 16.0;

    private static FontFamily fontFamily = normalFontFamily;

    private static readonly Brush defaultForeground = Brushes.Black;
    private static readonly Brush defaultBackground = Brushes.White;

    private static Brush foreground = defaultForeground;
    private static Brush background = defaultBackground;

    private static Typeface normalTypeface;
    private static Typeface fixedTypeface;

    public static FontFamily NormalFontFamily => normalFontFamily;

    public static FontFamily FixedFontFamily => fixedFontFamily;

    public static double FontSize => fontSize;

    public static FontFamily FontFamily
    {
        get => fontFamily; set => fontFamily = value;
    }

    public static Brush DefaultForeground => defaultForeground;

    public static Brush DefaultBackground => defaultBackground;

    public static Brush Foreground
    {
        get => foreground; set => foreground = value;
    }

    public static Brush Background
    {
        get => background; set => background = value;
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

    public static FontAndColorSetting DefaultSetting => defaultSetting;

    public static FontAndColorSetting AddressSetting => addressSetting;

    public static FontAndColorSetting CommentSetting => commentSetting;

    public static FontAndColorSetting ConstantSetting => constantSetting;

    public static FontAndColorSetting GlobalVariableSetting => globalVariableSetting;

    public static FontAndColorSetting KeywordSetting => keywordSetting;

    public static FontAndColorSetting LocalVariableSetting => localVariableSetting;

    public static FontAndColorSetting SeparatorSetting => separatorSetting;

    public static FontAndColorSetting StackVariableSetting => stackVariableSetting;

    public static FontAndColorSetting ZTextSetting => ztextSetting;
}
