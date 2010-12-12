using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ZDebug.IO.Services;

namespace ZDebug.IO.Windows
{
    internal sealed class ZTextBufferWindow : ZWindow
    {
        private readonly FlowDocument document;
        private readonly Paragraph paragraph;
        private readonly Size fontCharSize;

        private bool bold;
        private bool italic;
        private bool fixedPitch;

        private Typeface typeface;

        internal ZTextBufferWindow(ZWindowManager manager)
            : base(manager)
        {
            this.document = new FlowDocument();
            this.document.FontFamily = FontsAndColorsService.NormalFontFamily;
            this.document.FontSize = FontsAndColorsService.FontSize;
            this.document.PagePadding = new Thickness(4);
            this.paragraph = new Paragraph();
            this.document.Blocks.Add(paragraph);

            var scrollViewer = new FlowDocumentScrollViewer();
            scrollViewer.Document = this.document;

            this.Children.Add(scrollViewer);

            var zero = new FormattedText(
                textToFormat: "0",
                culture: CultureInfo.InstalledUICulture,
                flowDirection: FlowDirection.LeftToRight,
                typeface: new Typeface(FontsAndColorsService.NormalFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                emSize: FontsAndColorsService.FontSize,
                foreground: Brushes.Black);

            fontCharSize = new Size(zero.Width, zero.Height);
        }

        private Run GetFormattedRun(string text)
        {
            var run = new Run(text);

            if (bold)
            {
                run.FontWeight = FontWeights.Bold;
            }

            if (italic)
            {
                run.FontStyle = FontStyles.Italic;
            }

            if (fixedPitch)
            {
                run.FontFamily = FontsAndColorsService.FixedFontFamily;
            }

            return run;
        }

        public override void Clear()
        {
            this.paragraph.Inlines.Clear();
        }

        public override void PutString(string text)
        {
            this.paragraph.Inlines.Add(GetFormattedRun(text));
        }

        public override void PutChar(char ch)
        {
            this.paragraph.Inlines.Add(GetFormattedRun(ch.ToString()));
        }

        public override void SetBold(bool value)
        {
            bold = value;
        }

        public override void SetItalic(bool value)
        {
            italic = value;
        }

        public override void SetFixedPitch(bool value)
        {
            fixedPitch = value;
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
            get { return ZWindowType.TextBuffer; }
        }
    }
}
