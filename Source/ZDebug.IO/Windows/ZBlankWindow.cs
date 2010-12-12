using System.Windows.Media;
using System.Windows.Shapes;

namespace ZDebug.IO.Windows
{
    internal sealed class ZBlankWindow : ZWindow
    {
        public ZBlankWindow()
        {
            var rect = new Rectangle()
            {
                Fill = Brushes.Blue
            };

            this.Children.Add(rect);
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
            get { return 0; }
        }

        public override int ColumnWidth
        {
            get { return 0; }
        }

        public override ZWindowType WindowType
        {
            get { return ZWindowType.Blank; }
        }
    }
}
