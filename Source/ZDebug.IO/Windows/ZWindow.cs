using System;
using System.Windows.Controls;
using System.Windows.Media;

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

            this.UseLayoutRounding = true;
            this.SnapsToDevicePixels = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.Auto);
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

        public virtual int GetCursorLine()
        {
            return 0;
        }

        public virtual int GetCursorColumn()
        {
            return 0;
        }

        public virtual void SetCursor(int x, int y)
        {
        }

        public virtual void ReadChar(Action<char> callback)
        {
        }

        public virtual void ReadCommand(int maxChars, Action<string> callback)
        {
        }

        public virtual bool SetReverse(bool value)
        {
            return false;
        }

        public virtual bool SetBold(bool value)
        {
            return false;
        }

        public virtual bool SetItalic(bool value)
        {
            return false;
        }

        public virtual bool SetFixedPitch(bool value)
        {
            return false;
        }

        public abstract int RowHeight { get; }
        public abstract int ColumnWidth { get; }

        public abstract ZWindowType WindowType { get; }
    }
}
