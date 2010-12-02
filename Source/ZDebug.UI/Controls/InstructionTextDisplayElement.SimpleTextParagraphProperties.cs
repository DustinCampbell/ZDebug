using System.Windows;
using System.Windows.Media.TextFormatting;

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

            public override TextRunProperties DefaultTextRunProperties
            {
                get { return defaultTextRunProperties; }
            }

            public override bool FirstLineInParagraph
            {
                get { return false; }
            }

            public override FlowDirection FlowDirection
            {
                get { return FlowDirection.LeftToRight; }
            }

            public override double Indent
            {
                get { return 0.0; }
            }

            public override double LineHeight
            {
                get { return 0.0; }
            }

            public override TextAlignment TextAlignment
            {
                get { return TextAlignment.Left; }
            }

            public override TextMarkerProperties TextMarkerProperties
            {
                get { return null; }
            }

            public override TextWrapping TextWrapping
            {
                get { return TextWrapping.NoWrap; }
            }
        }

    }
}
