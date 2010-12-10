using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace ZDebug.UI.Controls
{
    public sealed partial class ZTextScreen
    {
        private class ZTextSource : TextSource
        {
            private readonly TextRunCache cache = new TextRunCache();
            private readonly StringBuilder textBuilder = new StringBuilder();
            private readonly List<FormattedSpan> spans = new List<FormattedSpan>();

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

            public void Add(string text, Typeface typeface, double fontSize, Brush foreground, Brush background)
            {
                var props = new ZTextRunProperties(typeface, fontSize, foreground, background);
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
