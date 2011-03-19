using System;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Basics
{
    public sealed class MemoryReader
    {
        private readonly byte[] memory;
        private int address;

        public MemoryReader(byte[] memory, int address)
        {
            this.memory = memory;
            this.address = address;
        }

        public byte NextByte()
        {
            if (address + 1 > memory.Length)
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
                return new byte[0];
            }

            if (address + length > memory.Length)
            {
                throw new InvalidOperationException("Attempted to read past end of memory");
            }

            var result = memory.ReadBytes(address, length);
            address += length;
            return result;
        }

        public ushort NextWord()
        {
            if (address + 2 > memory.Length)
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
                return new ushort[0];
            }

            if (address + (length * 2) > memory.Length)
            {
                throw new InvalidOperationException("Attempted to read past end of memory");
            }

            var result = memory.ReadWords(address, length);
            address += (length * 2);
            return result;
        }

        public uint NextDWord()
        {
            if (address + 4 > memory.Length)
            {
                throw new InvalidOperationException("Attempted to read past end of memory");
            }

            var result = memory.ReadDWord(address);
            address += 4;
            return result;
        }

        public uint[] NextDWords(int length)
        {
            if (length == 0)
            {
                return new uint[0];
            }

            if (address + (length * 4) > memory.Length)
            {
                throw new InvalidOperationException("Attempted to read past end of memory");
            }

            var result = memory.ReadDWords(address, length);
            address += (length * 4);
            return result;
        }

        public void Skip(int length)
        {
            if (length < 0 || address + length > memory.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            address += length;
        }

        public int Address
        {
            get { return address; }
            set
            {
                if (value < 0 || value > memory.Length)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                address = value;
            }
        }

        public int Size
        {
            get { return memory.Length; }
        }

        public int RemainingBytes
        {
            get { return memory.Length - address; }
        }

        public byte[] Memory
        {
            get { return memory; }
        }
    }
}
