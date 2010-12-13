using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ZDebug.IO.Services;

namespace ZDebug.IO.Windows
{
    internal sealed class ZTextGrid : FrameworkElement
    {
        private readonly VisualCollection visuals;

        private readonly Size fontCharSize;

        private readonly GuidelineSet guidelineSet;
        private int cursorX;
        private int cursorY;

        private Typeface typeface;
        private bool bold;
        private bool italic;
        private Brush foreground;
        private Brush background;

        public ZTextGrid()
        {
            visuals = new VisualCollection(this);

            var zero = new FormattedText(
                textToFormat: "0",
                culture: CultureInfo.InstalledUICulture,
                flowDirection: FlowDirection.LeftToRight,
                typeface: GetTypeface(),
                emSize: FontsAndColorsService.FontSize,
                foreground: Brushes.Black);

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
                var backgroundVisual = new DrawingVisual();
                var backgroundContext = backgroundVisual.RenderOpen();

                var x = fontCharSize.Width * cursorX;
                var y = fontCharSize.Height * cursorY;

                var backgroundRect = new Rect(
                    Math.Floor(x),
                    Math.Floor(y),
                    Math.Ceiling(fontCharSize.Width + .5),
                    Math.Ceiling(fontCharSize.Height));

                backgroundContext.DrawRectangle(background, null, backgroundRect);

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
                        foreground,
                        new NumberSubstitution(NumberCultureSource.User, CultureInfo.CurrentUICulture, NumberSubstitutionMethod.AsCulture),
                        TextFormattingMode.Display),
                    new Point(x, y));

                textContext.Close();

                visuals.Add(textVisual);

                cursorX++;
            }
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

        public void SetForeground(Brush value)
        {
            foreground = value;
        }

        public void SetBackground(Brush value)
        {
            background = value;
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
