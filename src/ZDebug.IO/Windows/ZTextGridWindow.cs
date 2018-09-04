using System.Globalization;
using System.Windows;
using System.Windows.Controls;
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
                foreground: Brushes.Black,
                pixelsPerDip: 1.0);

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

        public override int GetCursorColumn()
        {
            return textGrid.GetCursorX();
        }

        public override int GetCursorLine()
        {
            return textGrid.GetCursorY();
        }

        public override void SetCursor(int x, int y)
        {
            textGrid.SetCursor(x, y);
        }

        public override bool SetReverse(bool value)
        {
            var oldValue = reverse;
            reverse = value;
            textGrid.SetReverse(value);
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

        public override int GetHeight()
        {
            var rowIndex = Grid.GetRow(this);
            return (int)WindowParent.RowDefinitions[rowIndex].Height.Value / RowHeight;
        }

        public override void SetHeight(int lines)
        {
            var rowIndex = Grid.GetRow(this);
            WindowParent.RowDefinitions[rowIndex].Height = new GridLength(lines * RowHeight, GridUnitType.Pixel);
            textGrid.SetHeight(lines);
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
