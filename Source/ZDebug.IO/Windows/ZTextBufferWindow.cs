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
                typeface: FontsAndColorsService.NormalTypeface,
                emSize: FontsAndColorsService.FontSize,
                foreground: Brushes.Black);

            fontCharSize = new Size(zero.Width, zero.Height);
        }

        public override void Clear()
        {
            this.paragraph.Inlines.Clear();
        }

        public override void Print(string text)
        {
            this.paragraph.Inlines.Add(new Run(text));
        }

        public override void Print(char ch)
        {
            this.paragraph.Inlines.Add(new Run(ch.ToString()));
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
