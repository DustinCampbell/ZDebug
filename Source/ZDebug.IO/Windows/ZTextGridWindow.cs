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
            public readonly bool Reverse;
            public readonly bool Bold;
            public readonly bool Italic;

            public FormattedChar(char value, bool reverse, bool bold, bool italic)
            {
                this.Value = value;
                this.Reverse = reverse;
                this.Bold = bold;
                this.Italic = italic;
            }
        }

        private readonly Size fontCharSize;

        private readonly TextBlock display;

        private readonly List<List<FormattedChar>> lines;
        private int cursorX;
        private int cursorY;
        private bool reverse;
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
            bool currentReverse = false;
            bool currentBold = false;
            bool currentItalic = false;

            var inlines = new List<Inline>();
            var chars = new StringBuilder();

            Action addInline = () =>
            {
                Inline inline = new Run()
                {
                    Text = chars.ToString(),
                    FontWeight = currentBold ? FontWeights.Bold : FontWeights.Normal,
                    FontStyle = currentItalic ? FontStyles.Italic : FontStyles.Normal,
                    Foreground = currentReverse ? FontsAndColorsService.Background : FontsAndColorsService.Foreground,
                    Background = currentReverse ? FontsAndColorsService.Foreground : FontsAndColorsService.Background
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
                    if (currentReverse != ch.Reverse || currentBold != ch.Bold || currentItalic != ch.Italic)
                    {
                        addInline();

                        currentReverse = ch.Reverse;
                        currentBold = ch.Bold;
                        currentItalic = ch.Italic;
                    }

                    chars.Append(ch.Value);
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
                    line.Add(new FormattedChar(' ', false, false, false));
                }
            }

            if (ch == '\n')
            {
                cursorY++;
                cursorX = 0;
            }
            else
            {
                line[cursorX] = new FormattedChar(ch, reverse, bold, italic);
                cursorX++;
            }

            this.InvalidateArrange();
        }

        public override void SetCursor(int x, int y)
        {
            cursorX = x;
            cursorY = y;
        }

        public override bool SetReverse(bool value)
        {
            var oldValue = reverse;
            reverse = value;
            return oldValue;
        }

        public override bool SetBold(bool value)
        {
            var oldValue = bold;
            bold = value;
            return oldValue;
        }

        public override bool SetItalic(bool value)
        {
            var oldValue = italic;
            italic = value;
            return oldValue;
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
