using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ZDebug.IO.Services;

namespace ZDebug.UI.Controls
{
    internal partial class InstructionTextDisplayElement
    {
        private class SimpleTextRunProperties : TextRunProperties
        {
            private readonly FontAndColorSetting fontAndColorSetting;

            public SimpleTextRunProperties(FontAndColorSetting fontAndColorSetting)
            {
                this.fontAndColorSetting = fontAndColorSetting;
            }

            public override Brush BackgroundBrush
            {
                get { return fontAndColorSetting.Background; }
            }

            public override CultureInfo CultureInfo
            {
                get { return CultureInfo.InvariantCulture; }
            }

            public override double FontHintingEmSize
            {
                get { return fontAndColorSetting.FontSize; }
            }

            public override double FontRenderingEmSize
            {
                get { return fontAndColorSetting.FontSize; }
            }

            public override Brush ForegroundBrush
            {
                get { return fontAndColorSetting.Foreground; }
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
                get { return fontAndColorSetting.GetTypeface(); }
            }
        }

    }
}
