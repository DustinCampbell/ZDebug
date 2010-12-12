using System.Windows.Controls;

namespace ZDebug.IO.Windows
{
    public abstract class ZWindow : Grid
    {
        private ZPairWindow windowParent;

        public ZWindow()
        {
            this.ShowGridLines = true;
        }

        internal void SetWindowParent(ZPairWindow windowParent)
        {
            this.windowParent = windowParent;
        }

        internal ZPairWindow WindowParent
        {
            get { return windowParent; }
        }

        public abstract void Clear();

        public abstract void Print(string text);
        public abstract void Print(char ch);

        public abstract int RowHeight { get; }
        public abstract int ColumnWidth { get; }

        public abstract ZWindowType WindowType { get; }
    }
}
