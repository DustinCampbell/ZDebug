namespace ZDebug.Core.Basics
{
    public interface IMemoryReader
    {
        byte NextByte();
        byte[] NextBytes(int length);
        ushort NextWord();
        ushort[] NextWords(int length);

        void Skip(int length);

        int Index { get; }
        int Size { get; }
        int RemainingBytes { get; }
    }
}
