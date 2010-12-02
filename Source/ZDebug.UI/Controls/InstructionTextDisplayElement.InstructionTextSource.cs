using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Media.TextFormatting;
using ZDebug.UI.Services;

namespace ZDebug.UI.Controls
{
    internal partial class InstructionTextDisplayElement
    {
        private class InstructionTextSource : TextSource
        {
            private struct FormattedSpan
            {
                public readonly int Start;
                public readonly int Length;
                public readonly SimpleTextRunProperties Format;

                public FormattedSpan(int start, int length, SimpleTextRunProperties format)
                {
                    this.Start = start;
                    this.Length = length;
                    this.Format = format;
                }
            }

            private static readonly Dictionary<int, SimpleTextRunProperties> propMap = new Dictionary<int, SimpleTextRunProperties>();

            private readonly StringBuilder textBuilder = new StringBuilder();
            private readonly List<FormattedSpan> spans = new List<FormattedSpan>();

            public void Add(string text, FontAndColorSetting format)
            {
                SimpleTextRunProperties props;
                if (!propMap.TryGetValue(format.GetHashCode(), out props))
                {
                    props = new SimpleTextRunProperties(format);
                    propMap.Add(format.GetHashCode(), props);
                }

                var span = new FormattedSpan(textBuilder.Length, text.Length, props);

                textBuilder.Append(text);
                spans.Add(span);
            }

            public void Clear()
            {
                textBuilder.Clear();
                spans.Clear();
            }

            public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
            {
                var precedingSpan = spans.FindLast(s => s.Start <= textSourceCharacterIndexLimit && s.Start + s.Length >= textSourceCharacterIndexLimit);

                var range = new CharacterBufferRange(textBuilder.ToString(), precedingSpan.Start, textSourceCharacterIndexLimit - precedingSpan.Length);

                return new TextSpan<CultureSpecificCharacterBufferRange>(
                    length: textSourceCharacterIndexLimit - precedingSpan.Length,
                    value: new CultureSpecificCharacterBufferRange(CultureInfo.InvariantCulture, range));
            }

            public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
            {
                throw new NotImplementedException();
            }

            public override TextRun GetTextRun(int textSourceCharacterIndex)
            {
                if (textSourceCharacterIndex < 0)
                {
                    throw new ArgumentOutOfRangeException("textSourceCharacterIndex");
                }

                if (textSourceCharacterIndex < textBuilder.Length)
                {
                    var span = spans.Find(s => s.Start <= textSourceCharacterIndex && s.Start + s.Length > textSourceCharacterIndex);
                    var startIndex = textSourceCharacterIndex;
                    var length = (span.Start + span.Length) - textSourceCharacterIndex;

                    return new TextCharacters(textBuilder.ToString(), startIndex, length, span.Format);
                }

                return new TextEndOfParagraph(1);
            }

            public int Length
            {
                get { return textBuilder.Length; }
            }
        }
    }
}
