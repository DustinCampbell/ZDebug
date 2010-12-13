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

            this.manager = manager;
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

        public void Activate()
        {
            manager.Activate(this);
        }

        public void Close()
        {
            manager.Close(this);
        }

        public abstract void Clear();

        public abstract void PutChar(char ch);
        public abstract void PutString(string s);

        public virtual void SetCursor(int x, int y)
        {
        }

        public virtual void SetBold(bool value)
        {
        }

        public virtual void SetItalic(bool value)
        {
        }

        public virtual void SetFixedPitch(bool value)
        {
        }

        public abstract int RowHeight { get; }
        public abstract int ColumnWidth { get; }

        public abstract ZWindowType WindowType { get; }
    }
}
