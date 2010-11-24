namespace ZDebug.Core.Basics
{
    public interface IMemory
    {
        byte ReadByte(int index);
        byte[] ReadBytes(int index, int length);
        ushort ReadWord(int index);
        ushort[] ReadWords(int index, int length);

        void WriteByte(int index, byte value);
        void WriteBytes(int index, byte[] values);
        void WriteWord(int index, ushort value);
        void WriteWords(int index, ushort[] values);

        IMemoryReader CreateReader(int index);

        int Size { get; }
    }
}
