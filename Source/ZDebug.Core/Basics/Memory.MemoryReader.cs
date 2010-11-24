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
                if (index + 1 > memory.Size)
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

                if (index + length > memory.Size)
                {
                    throw new InvalidOperationException("Attempted to read past end of memory");
                }

                var result = memory.ReadBytes(index, length);
                index += length;
                return result;
            }

            public ushort NextWord()
            {
                if (index + 2 > memory.Size)
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

                if (index + (length * 2) > memory.Size)
                {
                    throw new InvalidOperationException("Attempted to read past end of memory");
                }

                var result = memory.ReadWords(index, length);
                index += (length * 2);
                return result;
            }

            public void Skip(int length)
            {
                if (length < 0 || index + length > memory.Size)
                {
                    throw new ArgumentOutOfRangeException("length");
                }

                index += length;
            }

            public int Index
            {
                get { return index; }
            }

            public int Size
            {
                get { return memory.Size; }
            }

            public int RemainingBytes
            {
                get { return memory.Size - index; }
            }
        }
    }
}
