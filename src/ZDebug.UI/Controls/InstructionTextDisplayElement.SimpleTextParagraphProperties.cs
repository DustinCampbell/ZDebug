using System.Windows;
using System.Windows.Media.TextFormatting;
using ZDebug.IO.Services;

namespace ZDebug.UI.Controls
{
    internal partial class InstructionTextDisplayElement
    {
        private class SimpleTextParagraphProperties : TextParagraphProperties
        {
            private readonly TextRunProperties defaultTextRunProperties;

            public SimpleTextParagraphProperties(FontAndColorSetting defaultSetting)
            {
                this.defaultTextRunProperties = new SimpleTextRunProperties(defaultSetting);
            }

            public override TextRunProperties DefaultTextRunProperties => defaultTextRunProperties;

            public override bool FirstLineInParagraph => false;

            public override FlowDirection FlowDirection => FlowDirection.LeftToRight;

            public override double Indent => 0.0;

            public override double LineHeight => 0.0;

            public override TextAlignment TextAlignment => TextAlignment.Left;

            public override TextMarkerProperties TextMarkerProperties => null;

            public override TextWrapping TextWrapping => TextWrapping.Wrap;
        }

    }
}
