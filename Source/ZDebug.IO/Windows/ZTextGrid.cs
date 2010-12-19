using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ZDebug.IO.Services;

namespace ZDebug.IO.Windows
{
    internal sealed class ZTextGrid : FrameworkElement
    {
        private struct CursorPos : IComparable<CursorPos>
        {
            public readonly int X;
            public readonly int Y;

            public CursorPos(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public int CompareTo(CursorPos other)
            {
                if (this.Y < other.Y)
                {
                    return -1;
                }
                else if (this.Y > other.Y)
                {
                    return 1;
                }
                else if (this.X < other.X)
                {
                    return -1;
                }
                else if (this.X > other.X)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        private struct VisualPair
        {
            public readonly Visual Background;
            public readonly Visual Character;

            public VisualPair(Visual background, Visual character)
            {
                this.Background = background;
                this.Character = character;
            }
        }

        private readonly VisualCollection visuals;
        private readonly SortedList<CursorPos, VisualPair> visualPairs;
        private readonly Size fontCharSize;

        private int cursorX;
        private int cursorY;

        private Typeface typeface;
        private bool bold;
        private bool italic;
        private bool reverse;

        public ZTextGrid()
        {
            visuals = new VisualCollection(this);
            visualPairs = new SortedList<CursorPos, VisualPair>();

            var zero = new FormattedText(
                textToFormat: "0",
                culture: CultureInfo.InstalledUICulture,
                flowDirection: FlowDirection.LeftToRight,
                typeface: GetTypeface(),
                emSize: FontsAndColorsService.FontSize,
                foreground: FontsAndColorsService.DefaultForeground);

            fontCharSize = new Size(zero.Width, zero.Height);

            Clear();
        }

        private Typeface GetTypeface()
        {
            if (typeface == null)
            {
                typeface = new Typeface(
                    FontsAndColorsService.FixedFontFamily,
                    italic ? FontStyles.Italic : FontStyles.Normal,
                    bold ? FontWeights.Bold : FontWeights.Normal,
                    FontStretches.Normal);
            }

            return typeface;
        }

        public void Clear()
        {
            visuals.Clear();
            cursorX = 0;
            cursorY = 0;
        }

        public void PutChar(char ch)
        {
            if (ch == '\n')
            {
                cursorY++;
                cursorX = 0;
            }
            else
            {
                // First, see if we've already inserted something at this position. If so, delete the old visuals.
                var cursorPos = new CursorPos(cursorX, cursorY);
                if (visualPairs.ContainsKey(cursorPos))
                {
                    var pair = visualPairs[cursorPos];
                    visuals.Remove(pair.Background);
                    visuals.Remove(pair.Character);
                    visualPairs.Remove(cursorPos);
                }

                var backgroundVisual = new DrawingVisual();
                var backgroundContext = backgroundVisual.RenderOpen();

                var x = fontCharSize.Width * cursorX;
                var y = fontCharSize.Height * cursorY;

                Brush fg, bg;
                if (reverse)
                {
                    fg = FontsAndColorsService.Background;
                    bg = FontsAndColorsService.Foreground;
                }
                else
                {
                    fg = FontsAndColorsService.Foreground;
                    bg = FontsAndColorsService.Background;
                }

                var backgroundRect = new Rect(
                    Math.Floor(x),
                    Math.Floor(y),
                    Math.Ceiling(fontCharSize.Width + .5),
                    Math.Ceiling(fontCharSize.Height));

                backgroundContext.DrawRectangle(bg, null, backgroundRect);

                backgroundContext.Close();

                visuals.Insert(0, backgroundVisual);

                var textVisual = new DrawingVisual();
                var textContext = textVisual.RenderOpen();

                textContext.DrawText(
                    new FormattedText(
                        ch.ToString(),
                        CultureInfo.CurrentUICulture,
                        FlowDirection.LeftToRight,
                        GetTypeface(),
                        FontsAndColorsService.FontSize,
                        fg,
                        new NumberSubstitution(NumberCultureSource.User, CultureInfo.CurrentUICulture, NumberSubstitutionMethod.AsCulture),
                        TextFormattingMode.Display),
                    new Point(x, y));

                textContext.Close();

                visuals.Add(textVisual);

                var newPair = new VisualPair(backgroundVisual, textVisual);
                visualPairs.Add(cursorPos, newPair);

                cursorX++;
            }
        }

        public int GetCursorX()
        {
            return cursorX;
        }

        public int GetCursorY()
        {
            return cursorY;
        }

        public void SetCursor(int x, int y)
        {
            cursorX = x;
            cursorY = y;
        }

        public void SetBold(bool value)
        {
            bold = value;
            typeface = null;
        }

        public void SetItalic(bool value)
        {
            italic = value;
            typeface = null;
        }

        public void SetReverse(bool value)
        {
            reverse = value;
        }

        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }

        protected override int VisualChildrenCount
        {
            get { return visuals.Count; }
        }
    }
}
