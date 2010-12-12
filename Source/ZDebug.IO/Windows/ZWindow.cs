using System;
using System.Windows.Controls;

namespace ZDebug.IO.Windows
{
    public abstract class ZWindow : Grid
    {
        protected readonly ZWindowManager manager;
        private ZPairWindow windowParent;

        internal ZWindow(ZWindowManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

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
