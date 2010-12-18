namespace ZDebug.Core.Text
{
    public struct ZCommandToken
    {
        public readonly int Start;
        public readonly int Length;
        public readonly string Text;

        public ZCommandToken(int start, int length, string text)
        {
            this.Start = start;
            this.Length = length;
            this.Text = text;
        }
    }
}
