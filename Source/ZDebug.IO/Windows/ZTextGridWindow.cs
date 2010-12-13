using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ZDebug.IO.Services;

namespace ZDebug.IO.Windows
{
    internal sealed partial class ZTextGridWindow : ZWindow
    {
        private struct FormattedChar
        {
            public readonly char Value;
            public readonly bool Bold;
            public readonly bool Italic;

            public FormattedChar(char value, bool bold, bool italic)
            {
                this.Value = value;
                this.Bold = bold;
                this.Italic = italic;
            }
        }

        private readonly Size fontCharSize;

        private readonly TextBlock display;

        private readonly List<List<FormattedChar>> lines;
        private int cursorX;
        private int cursorY;
        private bool bold;
        private bool italic;

        internal ZTextGridWindow(ZWindowManager manager)
            : base(manager)
        {
            lines = new List<List<FormattedChar>>();

            var zero = new FormattedText(
                textToFormat: "0",
                culture: CultureInfo.InstalledUICulture,
                flowDirection: FlowDirection.LeftToRight,
                typeface: FontsAndColorsService.FixedTypeface,
                emSize: FontsAndColorsService.FontSize,
                foreground: Brushes.Black);

            fontCharSize = new Size(zero.Width, zero.Height);

            display = new TextBlock();
            display.FontFamily = FontsAndColorsService.FixedFontFamily;
            display.FontSize = FontsAndColorsService.FontSize;

            this.Children.Add(display);
        }

        private void BuildInlines()
        {
            bool currentBold = false;
            bool currentItalic = false;

            var inlines = new List<Inline>();
            var chars = new StringBuilder();

            Action addInline = () =>
            {
                var inline = new Run()
                {
                    Text = chars.ToString(),
                    FontWeight = currentBold ? FontWeights.Bold : FontWeights.Normal,
                    FontStyle = currentItalic ? FontStyles.Italic : FontStyles.Normal
                };

                inlines.Add(inline);

                chars.Clear();
            };

            bool first = true;
            foreach (var line in lines)
            {
                if (!first)
                {
                    inlines.Add(new LineBreak());
                }

                first = false;

                foreach (var ch in line)
                {
                    if (currentBold == ch.Bold && currentItalic == ch.Italic)
                    {
                        chars.Append(ch.Value);
                    }
                    else
                    {
                        addInline();
                        currentBold = ch.Bold;
                        currentItalic = ch.Italic;
                    }
                }

                addInline();
            }

            display.Inlines.Clear();
            display.Inlines.AddRange(inlines);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            BuildInlines();

            return base.ArrangeOverride(arrangeSize);
        }

        public override void Clear()
        {
            lines.Clear();
            display.Inlines.Clear();
        }

        public override void PutString(string text)
        {
            foreach (var ch in text)
            {
                PutChar(ch);
            }
        }

        public override void PutChar(char ch)
        {
            var lineCount = lines.Count;
            if (cursorY >= lineCount)
            {
                for (int i = 0; i < cursorY - lineCount + 1; i++)
                {
                    lines.Add(new List<FormattedChar>());
                }
            }

            var line = lines[cursorY];
            var lineLength = line.Count;
            if (cursorX >= lineLength)
            {
                for (int i = 0; i < cursorX - lineLength + 1; i++)
                {
                    line.Add(new FormattedChar(' ', bold, italic));
                }
            }

            if (ch == '\n')
            {
                cursorY++;
            }
            else
            {
                line[cursorX] = new FormattedChar(ch, bold, italic);
                cursorX++;
            }

            this.InvalidateArrange();
        }

        public override void SetCursor(int x, int y)
        {
            cursorX = x;
            cursorY = y;
        }

        public override void SetBold(bool value)
        {
            bold = value;
        }

        public override void SetItalic(bool value)
        {
            italic = value;
        }

        public override int RowHeight
        {
            get { return (int)fontCharSize.Height; }
        }

        public override int ColumnWidth
        {
            get { return (int)fontCharSize.Width; }
        }

        public override ZWindowType WindowType
        {
            get { return ZWindowType.TextGrid; }
        }
    }
}
