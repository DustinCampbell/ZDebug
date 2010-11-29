using System.Windows;
using System.Windows.Media;

namespace ZDebug.UI.Controls
{
    internal sealed class FontAndColorSetting : DependencyObject
    {
        private Typeface typeface;

        public Typeface GetTypeface()
        {
            if (typeface == null)
            {
                typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
            }

            return typeface;
        }

        private static void TypefacePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((FontAndColorSetting)obj).typeface = null;
        }

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register(
                "Background",
                typeof(Brush),
                typeof(FontAndColorSetting));

        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register(
                "FontFamily",
                typeof(FontFamily),
                typeof(FontAndColorSetting),
                new PropertyMetadata(new FontFamily("Consolas")));

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(
                "FontSize",
                typeof(double),
                typeof(FontAndColorSetting),
                new PropertyMetadata(15.0));

        public static readonly DependencyProperty FontStretchProperty =
            DependencyProperty.Register(
                "FontStretch",
                typeof(FontStretch),
                typeof(FontAndColorSetting));

        public static readonly DependencyProperty FontStyleProperty =
            DependencyProperty.Register(
                "FontStyle",
                typeof(FontStyle),
                typeof(FontAndColorSetting));

        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register(
                "FontWeight",
                typeof(FontWeight),
                typeof(FontAndColorSetting));

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register(
                "Foreground",
                typeof(Brush),
                typeof(FontAndColorSetting));

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
    }
}
