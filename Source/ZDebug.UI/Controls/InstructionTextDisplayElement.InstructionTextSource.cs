using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Media.TextFormatting;
using ZDebug.IO.Services;

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

            private readonly TextRunCache cache = new TextRunCache();
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

                cache.Change(span.Start, span.Length, 0);
            }

            public void Clear()
            {
                textBuilder.Clear();
                spans.Clear();
                cache.Invalidate();
            }

            public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
            {
                return new TextSpan<CultureSpecificCharacterBufferRange>(0, new CultureSpecificCharacterBufferRange(CultureInfo.CurrentUICulture, new CharacterBufferRange("", 0, 0)));
            }

            public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
            {
                throw new NotSupportedException();
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

            public TextRunCache Cache
            {
                get { return cache; }
            }

            public int Length
            {
                get { return textBuilder.Length; }
            }
        }
    }
}
