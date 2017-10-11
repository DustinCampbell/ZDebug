namespace ZDebug.IO.Streams
{
    public abstract class ZStream
    {
        public abstract void PutChar(char ch);
        public abstract void PutString(string s);
    }
}
