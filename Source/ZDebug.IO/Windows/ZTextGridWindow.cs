using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ZDebug.IO.Services;

namespace ZDebug.IO.Windows
{
    internal sealed class ZTextGridWindow : ZWindow
    {
        private readonly Size fontCharSize;

        internal ZTextGridWindow(ZWindowManager manager)
            : base(manager)
        {
            var zero = new FormattedText(
                textToFormat: "0",
                culture: CultureInfo.InstalledUICulture,
                flowDirection: FlowDirection.LeftToRight,
                typeface: FontsAndColorsService.FixedTypeface,
                emSize: FontsAndColorsService.FontSize,
                foreground: Brushes.Black);

            fontCharSize = new Size(zero.Width, zero.Height);
        }

        public override void Clear()
        {
        }

        public override void Print(string text)
        {
        }

        public override void Print(char ch)
        {
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
