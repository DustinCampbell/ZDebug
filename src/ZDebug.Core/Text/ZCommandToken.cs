namespace ZDebug.Core.Text;

public readonly struct ZCommandToken
{
    public readonly int Start;
    public readonly int Length;
    public readonly string Text;

    public ZCommandToken(int start, int length, string text)
    {
        Start = start;
        Length = length;
        Text = text;
    }
}
