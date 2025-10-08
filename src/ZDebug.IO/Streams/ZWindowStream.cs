using ZDebug.IO.Windows;

namespace ZDebug.IO.Streams;

public class ZWindowStream : ZStream
{
    private readonly ZWindow window;

    internal ZWindowStream(ZWindow window)
    {
        this.window = window;
    }

    public ZWindow Window => window;

    public override void PutChar(char ch) => window.PutChar(ch);

    public override void PutString(string s) => window.PutString(s);
}
