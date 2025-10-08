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

            public override Brush BackgroundBrush => fontAndColorSetting.Background;

            public override CultureInfo CultureInfo => CultureInfo.InvariantCulture;

            public override double FontHintingEmSize => fontAndColorSetting.FontSize;

            public override double FontRenderingEmSize => fontAndColorSetting.FontSize;

            public override Brush ForegroundBrush => fontAndColorSetting.Foreground;

            public override TextDecorationCollection TextDecorations => null;

            public override TextEffectCollection TextEffects => null;

            public override Typeface Typeface => fontAndColorSetting.GetTypeface();
        }

    }
}
