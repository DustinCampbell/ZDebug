using System;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Basics
{
    public sealed partial class Memory
    {
        private class MemoryReader : IMemoryReader
        {
            private readonly Memory memory;
            private int address;

            public MemoryReader(Memory memory, int address)
            {
                this.memory = memory;
                this.address = address;
            }

            public byte NextByte()
            {
                if (address + 1 > memory.Size)
                {
                    throw new InvalidOperationException("Attempted to read past end of memory");
                }

                var result = memory.ReadByte(address);
                address++;
                return result;
            }

            public byte[] NextBytes(int length)
            {
                if (length == 0)
                {
                    return ArrayEx.Empty<byte>();
                }

                if (address + length > memory.Size)
                {
                    throw new InvalidOperationException("Attempted to read past end of memory");
                }

                var result = memory.ReadBytes(address, length);
                address += length;
                return result;
            }

            public ushort NextWord()
            {
                if (address + 2 > memory.Size)
                {
                    throw new InvalidOperationException("Attempted to read past end of memory");
                }

                var result = memory.ReadWord(address);
                address += 2;
                return result;
            }

            public ushort[] NextWords(int length)
            {
                if (length == 0)
                {
                    return ArrayEx.Empty<ushort>();
                }

                if (address + (length * 2) > memory.Size)
                {
                    throw new InvalidOperationException("Attempted to read past end of memory");
                }

                var result = memory.ReadWords(address, length);
                address += (length * 2);
                return result;
            }

            public void Skip(int length)
            {
                if (length < 0 || address + length > memory.Size)
                {
                    throw new ArgumentOutOfRangeException("length");
                }

                address += length;
            }

            public int Address
            {
                get { return address; }
            }

            public int Size
            {
                get { return memory.Size; }
            }

            public int RemainingBytes
            {
                get { return memory.Size - address; }
            }

            public Memory Memory
            {
                get { return memory; }
            }
        }
    }
}
