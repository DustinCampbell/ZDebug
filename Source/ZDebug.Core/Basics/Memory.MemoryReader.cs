using System;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Basics
{
    public sealed partial class Memory
    {
        private class MemoryReader : IMemoryReader
        {
            private readonly Memory memory;
            private int index;

            public MemoryReader(Memory memory, int index)
            {
                this.memory = memory;
                this.index = index;
            }

            public byte NextByte()
            {
                if (index > memory.Size - 1)
                {
                    throw new InvalidOperationException("Attempted to read past end of memory");
                }

                var result = memory.ReadByte(index);
                index++;
                return result;
            }

            public byte[] NextBytes(int length)
            {
                if (length == 0)
                {
                    return ArrayEx.Empty<byte>();
                }

                if (index > memory.Size - length)
                {
                    throw new InvalidOperationException("Attempted to read past end of memory");
                }

                var result = memory.ReadBytes(index, length);
                index += length;
                return result;
            }

            public ushort NextWord()
            {
                if (index > memory.Size - 2)
                {
                    throw new InvalidOperationException("Attempted to read past end of memory");
                }

                var result = memory.ReadWord(index);
                index += 2;
                return result;
            }

            public ushort[] NextWords(int length)
            {
                if (length == 0)
                {
                    return ArrayEx.Empty<ushort>();
                }

                if (index > memory.Size - (length * 2))
                {
                    throw new InvalidOperationException("Attempted to read past end of memory");
                }

                var result = memory.ReadWords(index, length);
                index += (length * 2);
                return result;
            }

            public int Index
            {
                get { return index; }
            }

            public int Size
            {
                get { return memory.Size; }
            }
        }
    }
}
