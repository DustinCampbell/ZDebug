using System;

namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        private class NullScreen : IScreen
        {
            private NullScreen()
            {
            }

            public void Clear(int window)
            {
            }

            public void ClearAll(bool unsplit = false)
            {
            }

            public void Split(int height)
            {
            }

            public void Unsplit()
            {
            }

            public void SetWindow(int window)
            {
            }

            public int GetCursorLine()
            {
                return 0;
            }

            public int GetCursorColumn()
            {
                return 0;
            }

            public void SetCursor(int line, int column)
            {
            }

            public void SetTextStyle(ZTextStyle style)
            {
            }

            public void SetForegroundColor(ZColor color)
            {
            }

            public void SetBackgroundColor(ZColor color)
            {
            }

            public ZFont SetFont(ZFont font)
            {
                return 0;
            }

            public void ShowStatus()
            {
            }

            public void Print(string text)
            {
            }

            public void Print(char ch)
            {
            }

            public void ReadChar(Action<char> callback)
            {
                callback('\0');
            }

            public void ReadCommand(int maxChars, Action<string> callback)
            {
                callback(string.Empty);
            }

            public byte ScreenHeightInLines
            {
                get { return 0; }
            }

            public byte ScreenWidthInColumns
            {
                get { return 0; }
            }

            public ushort ScreenHeightInUnits
            {
                get { return 0; }
            }

            public ushort ScreenWidthInUnits
            {
                get { return 0; }
            }

            public byte FontHeightInUnits
            {
                get { return 0; }
            }

            public byte FontWidthInUnits
            {
                get { return 0; }
            }

            public static readonly IScreen Instance = new NullScreen();

            public ZColor DefaultBackgroundColor
            {
                get { return ZColor.White; }
            }

            public ZColor DefaultForegroundColor
            {
                get { return ZColor.Black; }
            }
        }
    }
}
