using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core.Execution;

namespace ZDebug.Terp
{
    internal class DummyScreen : IScreen
    {
        private readonly StringBuilder builder = new StringBuilder();

        public void Clear(int window)
        {
            throw new NotImplementedException();
        }

        public void ClearAll(bool unsplit = false)
        {
            throw new NotImplementedException();
        }

        public void Split(int height)
        {
            throw new NotImplementedException();
        }

        public void Unsplit()
        {
            throw new NotImplementedException();
        }

        public void SetWindow(int window)
        {
            throw new NotImplementedException();
        }

        public int GetCursorLine()
        {
            throw new NotImplementedException();
        }

        public int GetCursorColumn()
        {
            throw new NotImplementedException();
        }

        public void SetCursor(int line, int column)
        {
            throw new NotImplementedException();
        }

        public void SetTextStyle(ZTextStyle style)
        {
            throw new NotImplementedException();
        }

        public void SetForegroundColor(ZColor color)
        {
            throw new NotImplementedException();
        }

        public void SetBackgroundColor(ZColor color)
        {
            throw new NotImplementedException();
        }

        public ZFont SetFont(ZFont font)
        {
            throw new NotImplementedException();
        }

        public void ShowStatus()
        {
            throw new NotImplementedException();
        }

        public byte ScreenHeightInLines
        {
            get { throw new NotImplementedException(); }
        }

        public byte ScreenWidthInColumns
        {
            get { throw new NotImplementedException(); }
        }

        public ushort ScreenHeightInUnits
        {
            get { throw new NotImplementedException(); }
        }

        public ushort ScreenWidthInUnits
        {
            get { throw new NotImplementedException(); }
        }

        public byte FontHeightInUnits
        {
            get { throw new NotImplementedException(); }
        }

        public byte FontWidthInUnits
        {
            get { throw new NotImplementedException(); }
        }

        public ZColor DefaultBackgroundColor
        {
            get { throw new NotImplementedException(); }
        }

        public ZColor DefaultForegroundColor
        {
            get { throw new NotImplementedException(); }
        }

        public void Print(string text)
        {
            builder.Append(text);
        }

        public void Print(char ch)
        {
            builder.Append(ch);
        }

        public void ReadChar(Action<char> callback)
        {
            throw new NotImplementedException();
        }

        public void ReadCommand(int maxChars, Action<string> callback)
        {
            throw new NotImplementedException();
        }
    }
}
