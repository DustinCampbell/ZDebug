using System;
using System.Windows;
using System.Windows.Media;

namespace ZDebug.UI.Services
{
    internal sealed partial class FontAndColorSetting
    {
        private readonly FontAndColorSetting baseSetting;
        private readonly FontFamily fontFamily;
        private readonly double fontSize;
        private readonly Brush foreground;
        private readonly Brush background;
        private readonly FontWeight fontWeight;
        private readonly FontStyle fontStyle;
        private readonly FontStretch fontStretch;

        private readonly bool hasFontFamily;
        private readonly bool hasFontSize;
        private readonly bool hasForeground;
        private readonly bool hasBackground;
        private readonly bool hasFontWeight;
        private readonly bool hasFontStyle;
        private readonly bool hasFontStretch;

        private Typeface typeface;

        public FontAndColorSetting(
            FontAndColorSetting baseSetting,
            FontFamily fontFamily = null,
            double? fontSize = null,
            Brush foreground = null,
            Brush background = null,
            FontWeight? fontWeight = null,
            FontStyle? fontStyle = null,
            FontStretch? fontStretch = null)
        {
            if (baseSetting == null)
            {
                throw new ArgumentNullException("baseSetting");
            }

            this.baseSetting = baseSetting;

            if (fontFamily != null)
            {
                this.hasFontFamily = true;
                this.fontFamily = fontFamily;
            }

            if (fontSize != null)
            {
                this.hasFontSize = true;
                this.fontSize = fontSize.Value;
            }

            if (foreground != null)
            {
                this.hasForeground = true;
                this.foreground = foreground;
            }

            if (background != null)
            {
                this.hasBackground = true;
                this.background = background;
            }

            if (fontWeight != null)
            {
                this.hasFontWeight = true;
                this.fontWeight = fontWeight.Value;
            }

            if (fontStyle != null)
            {
                this.hasFontStyle = true;
                this.fontStyle = fontStyle.Value;
            }

            if (fontStretch != null)
            {
                this.hasFontStretch = true;
                this.fontStretch = fontStretch.Value;
            }
        }

        public FontAndColorSetting(
            FontFamily fontFamily,
            double fontSize,
            Brush foreground = null,
            Brush background = null,
            FontWeight? fontWeight = null,
            FontStyle? fontStyle = null,
            FontStretch? fontStretch = null)
        {
            if (fontFamily == null)
            {
                throw new ArgumentNullException("fontFamily");
            }

            if (fontSize < 0.0)
            {
                throw new ArgumentOutOfRangeException("fontSize");
            }

            this.fontFamily = fontFamily;
            this.fontSize = fontSize;
            this.foreground = foreground ?? Brushes.Black;
            this.background = background ?? Brushes.White;
            this.fontWeight = fontWeight ?? FontWeights.Normal;
            this.fontStyle = fontStyle ?? FontStyles.Normal;
            this.fontStretch = fontStretch ?? FontStretches.Normal;

            this.hasFontFamily = true;
            this.hasFontSize = true;
            this.hasForeground = true;
            this.hasBackground = true;
            this.hasFontWeight = true;
            this.hasFontStyle = true;
            this.hasFontStretch = true;
        }

        public Typeface GetTypeface()
        {
            if (typeface == null)
            {
                typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
            }

            return typeface;
        }

        public FontFamily FontFamily
        {
            get { return hasFontFamily ? fontFamily : baseSetting.fontFamily; }
        }

        public double FontSize
        {
            get { return hasFontSize ? fontSize : baseSetting.fontSize; }
        }

        public Brush Foreground
        {
            get { return hasForeground ? foreground : baseSetting.foreground; }
        }

        public Brush Background
        {
            get { return hasBackground ? background : baseSetting.background; }
        }

        public FontWeight FontWeight
        {
            get { return hasFontWeight ? fontWeight : baseSetting.fontWeight; }
        }

        public FontStyle FontStyle
        {
            get { return hasFontStyle ? fontStyle : baseSetting.fontStyle; }
        }

        public FontStretch FontStretch
        {
            get { return hasFontStretch ? fontStretch : baseSetting.fontStretch; }
        }
    }
}
