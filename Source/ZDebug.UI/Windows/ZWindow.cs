using System.Windows.Controls;

namespace ZDebug.UI.Windows
{
    internal abstract class ZWindow : Grid
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

        public ZPairWindow WindowParent
        {
            get { return windowParent; }
        }

        public abstract void Clear();

        public abstract int RowHeight { get; }
        public abstract int ColumnWidth { get; }

        public abstract ZWindowType WindowType { get; }
    }
}
