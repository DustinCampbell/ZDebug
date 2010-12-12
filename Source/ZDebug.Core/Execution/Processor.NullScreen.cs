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

            public void SetTextStyle(ZTextStyle style)
            {
            }

            public void Print(string text)
            {
            }

            public void Print(char ch)
            {
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

            public event EventHandler DimensionsChanged;

            public static readonly IScreen Instance = new NullScreen();
        }
    }
}
