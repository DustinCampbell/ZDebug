namespace ZDebug.UI.Controls
{
    public sealed partial class ZTextScreen
    {
        private struct FormattedSpan
        {
            public readonly int Start;
            public readonly int Length;
            public readonly ZTextRunProperties Format;

            public FormattedSpan(int start, int length, ZTextRunProperties format)
            {
                this.Start = start;
                this.Length = length;
                this.Format = format;
            }
        }
    }
}
