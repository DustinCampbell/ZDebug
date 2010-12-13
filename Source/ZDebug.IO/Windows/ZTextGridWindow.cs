using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ZDebug.IO.Services;

namespace ZDebug.IO.Windows
{
    internal sealed partial class ZTextGridWindow : ZWindow
    {
        private readonly Size fontCharSize;

        private ZTextGrid textGrid;
        private bool reverse;
        private bool bold;
        private bool italic;

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

            textGrid = new ZTextGrid();

            this.Children.Add(textGrid);
        }

        public override void Clear()
        {
            textGrid.Clear();
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
            textGrid.PutChar(ch);
        }

        public override void SetCursor(int x, int y)
        {
            textGrid.SetCursor(x, y);
        }

        public override bool SetReverse(bool value)
        {
            var oldValue = reverse;
            reverse = value;

            if (reverse)
            {
                textGrid.SetBackground(FontsAndColorsService.Foreground);
                textGrid.SetForeground(FontsAndColorsService.Background);
            }
            else
            {
                textGrid.SetBackground(FontsAndColorsService.Background);
                textGrid.SetForeground(FontsAndColorsService.Foreground);
            }

            return oldValue;
        }

        public override bool SetBold(bool value)
        {
            var oldValue = bold;
            bold = value;
            textGrid.SetBold(value);
            return oldValue;
        }

        public override bool SetItalic(bool value)
        {
            var oldValue = italic;
            italic = value;
            textGrid.SetItalic(value);
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
