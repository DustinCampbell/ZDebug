using ZDebug.Core.Extensions;

namespace ZDebug.Core.Execution;

public abstract partial class ZMachine
{
    private class MemoryOutputStream : IOutputStream
    {
        private readonly byte[] memory;
        private readonly int address;
        private ushort count;

        public MemoryOutputStream(byte[] memory, int address)
        {
            this.memory = memory;
            this.address = address;

            count = 0;
            memory.WriteWord(address, count);
        }

        private byte CharToByte(char ch) => ch == '\n' ? (byte)13 : (byte)ch;

        public void Print(string text)
        {
            var bytes = text.ToCharArray().ConvertAll(CharToByte);
            memory.WriteBytes(address + 2 + count, bytes);
            count += (ushort)bytes.Length;
            memory.WriteWord(address, count);
        }

        public void Print(char ch)
        {
            memory.WriteByte(address + 2 + count, CharToByte(ch));
            count++;
            memory.WriteWord(address, count);
        }
    }
}
