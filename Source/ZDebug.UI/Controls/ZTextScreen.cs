using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace ZDebug.UI.Controls
{
    public sealed partial class ZTextScreen : FrameworkElement
    {
        private readonly ZTextSource textSource;
        private readonly TextFormatter formatter;

        private readonly ZTextRunProperties defaultRunProperties;
        private readonly ZTextParagraphProperties defaultParagraphProperties;
        private readonly Typeface typeface;

        public ZTextScreen()
        {
            textSource = new ZTextSource();
            formatter = TextFormatter.Create(TextFormattingMode.Display);

            // TODO: Retrieve these from the current window
            typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            defaultRunProperties = new ZTextRunProperties(typeface, 13.0, Brushes.Black, Brushes.White);
            defaultParagraphProperties = new ZTextParagraphProperties(defaultRunProperties, TextWrapping.WrapWithOverflow);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // TODO: Measure both windows
            var height = 0.0;

            int textSourcePosition = 0;
            while (textSourcePosition < textSource.Length)
            {
                using (var line = formatter.FormatLine(textSource, textSourcePosition, availableSize.Width, defaultParagraphProperties, previousLineBreak: null, textRunCache: textSource.Cache))
                {
                    height += line.Height;
                    textSourcePosition += line.Length;
                }
            }

            return new Size(availableSize.Width, height);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var top = 0.0;

            int textSourcePosition = 0;
            while (textSourcePosition < textSource.Length)
            {
                using (var line = formatter.FormatLine(textSource, textSourcePosition, this.DesiredSize.Width, defaultParagraphProperties, previousLineBreak: null, textRunCache: textSource.Cache))
                {
                    line.Draw(drawingContext, new Point(0.0, top), InvertAxes.None);
                    top += line.Height;
                    textSourcePosition += line.Length;
                }
            }
        }

        private void Invalidate()
        {
            InvalidateMeasure();
            InvalidateVisual();
        }

        public void Clear(int window)
        {
            if (window == 0) // lower window
            {
                textSource.Clear();
                Invalidate();
            }
        }

        public void ClearAll(bool unsplit)
        {
            textSource.Clear();
            Invalidate();
        }

        public void Print(string text)
        {
            textSource.Add(text, typeface, 13.0, Brushes.Black, Brushes.White);
            Invalidate();
        }

        public void Print(char ch)
        {
            textSource.Add(ch.ToString(), typeface, 13.0, Brushes.Black, Brushes.White);
            Invalidate();
        }
    }
}
