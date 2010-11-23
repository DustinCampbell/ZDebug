namespace ZDebug.Core
{
    public interface IMemoryReader
    {
        byte NextByte();
        byte[] NextBytes(int length);
        ushort NextWord();
        ushort[] NextWords(int length);

        int Index { get; }
        int Size { get; }
    }
}
