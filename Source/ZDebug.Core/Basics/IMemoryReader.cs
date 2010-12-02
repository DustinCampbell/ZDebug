namespace ZDebug.Core.Basics
{
    public interface IMemoryReader
    {
        byte NextByte();
        byte[] NextBytes(int length);
        ushort NextWord();
        ushort[] NextWords(int length);

        void Skip(int length);

        int Address { get; set; }
        int Size { get; }
        int RemainingBytes { get; }

        Memory Memory { get; }
    }
}
