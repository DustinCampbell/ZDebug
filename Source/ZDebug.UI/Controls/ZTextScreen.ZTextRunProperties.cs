using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace ZDebug.UI.Controls
{
    public sealed partial class ZTextScreen
    {
        private sealed class ZTextRunProperties : TextRunProperties
        {
            private readonly Brush backgroundBrush;
            private readonly double fontSize;
            private readonly Brush foregroundBrush;
            private readonly Typeface typeface;

            public ZTextRunProperties(Typeface typeface, double fontSize, Brush foregroundBrush, Brush backgroundBrush)
            {
                this.typeface = typeface;
                this.fontSize = fontSize;
                this.foregroundBrush = foregroundBrush;
                this.backgroundBrush = backgroundBrush;
            }

            public override Brush BackgroundBrush
            {
                get { return backgroundBrush; }
            }

            public override CultureInfo CultureInfo
            {
                get { return CultureInfo.CurrentUICulture; }
            }

            public override double FontHintingEmSize
            {
                get { return fontSize; }
            }

            public override double FontRenderingEmSize
            {
                get { return fontSize; }
            }

            public override Brush ForegroundBrush
            {
                get { return foregroundBrush; }
            }

            public override TextDecorationCollection TextDecorations
            {
                get { return null; }
            }

            public override TextEffectCollection TextEffects
            {
                get { return null; }
            }

            public override Typeface Typeface
            {
                get { return typeface; }
            }
        }
    }
}
