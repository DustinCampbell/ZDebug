namespace ZDebug.Core.Basics
{
    internal interface IMemoryReader
    {
        byte NextByte();
        byte[] NextBytes(int length);
        ushort NextWord();
        ushort[] NextWords(int length);

        int Index { get; }
        int Size { get; }
    }
}
