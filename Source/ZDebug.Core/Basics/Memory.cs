using System;
using System.IO;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Basics
{
    public sealed partial class Memory
    {
        private readonly byte[] bytes;

        public Memory(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            this.bytes = bytes;
        }

        public Memory(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.bytes = stream.ReadFully();
        }

        /// <summary>
        /// Access to underlying byte array for high-performance routines.
        /// </summary>
        public byte[] Bytes
        {
            get { return bytes; }
        }

        public byte ReadByte(int address)
        {
            return bytes[address];
        }

        public byte ReadByte(ref int address)
        {
            return bytes[address++];
        }

        public byte[] ReadBytes(int address, int length)
        {
            if (length == 0)
            {
                return ArrayEx.Empty<byte>();
            }

            byte[] result = new byte[length];
            Array.Copy(bytes, address, result, 0, length);
            return result;
        }

        public byte[] ReadBytes(ref int address, int length)
        {
            if (length == 0)
            {
                return ArrayEx.Empty<byte>();
            }

            byte[] result = new byte[length];
            Array.Copy(bytes, address, result, 0, length);
            address += length;
            return result;
        }

        public ushort ReadWord(int address)
        {
            var b1 = bytes[address];
            var b2 = bytes[address + 1];

            return (ushort)(b1 << 8 | b2);
        }

        public ushort ReadWord(ref int address)
        {
            var b1 = bytes[address];
            var b2 = bytes[address + 1];

            address += 2;

            return (ushort)(b1 << 8 | b2);
        }

        public ushort[] ReadWords(int address, int length)
        {
            if (length == 0)
            {
                return ArrayEx.Empty<ushort>();
            }

            ushort[] result = new ushort[length];
            for (int i = 0; i < length; i++)
            {
                var offset = i * 2;
                var b1 = bytes[address + offset];
                var b2 = bytes[address + offset + 1];

                result[i] = (ushort)(b1 << 8 | b2);
            }

            return result;
        }

        public ushort[] ReadWords(ref int address, int length)
        {
            if (length == 0)
            {
                return ArrayEx.Empty<ushort>();
            }

            ushort[] result = new ushort[length];
            for (int i = 0; i < length; i++)
            {
                var offset = i * 2;
                var b1 = bytes[address + offset];
                var b2 = bytes[address + offset + 1];

                result[i] = (ushort)(b1 << 8 | b2);
            }

            address += length * 2;

            return result;
        }

        public uint ReadDWord(int address)
        {
            var b1 = bytes[address];
            var b2 = bytes[address + 1];
            var b3 = bytes[address + 2];
            var b4 = bytes[address + 3];

            return (uint)(b1 << 24 | b2 << 16 | b3 << 8 | b4);
        }

        public uint ReadDWord(ref int address)
        {
            var b1 = bytes[address];
            var b2 = bytes[address + 1];
            var b3 = bytes[address + 2];
            var b4 = bytes[address + 3];

            address += 4;

            return (uint)(b1 << 24 | b2 << 16 | b3 << 8 | b4);
        }

        public uint[] ReadDWords(int address, int length)
        {
            if (length == 0)
            {
                return ArrayEx.Empty<uint>();
            }

            uint[] result = new uint[length];
            for (int i = 0; i < length; i++)
            {
                var offset = i * 4;
                var b1 = bytes[address + offset];
                var b2 = bytes[address + offset + 1];
                var b3 = bytes[address + offset + 2];
                var b4 = bytes[address + offset + 3];

                result[i] = (uint)(b1 << 24 | b2 << 16 | b3 << 8 | b4);
            }

            return result;
        }

        public uint[] ReadDWords(ref int address, int length)
        {
            if (length == 0)
            {
                return ArrayEx.Empty<uint>();
            }

            uint[] result = new uint[length];
            for (int i = 0; i < length; i++)
            {
                var offset = i * 4;
                var b1 = bytes[address + offset];
                var b2 = bytes[address + offset + 1];
                var b3 = bytes[address + offset + 2];
                var b4 = bytes[address + offset + 3];

                result[i] = (uint)(b1 << 24 | b2 << 16 | b3 << 8 | b4);
            }

            address += length * 4;

            return result;
        }

        public void WriteByte(int address, byte value)
        {
            byte oldValue = bytes[address];

            bytes[address] = value;
        }

        public void WriteBytes(int address, byte[] values)
        {
            if (values.Length == 0)
            {
                return;
            }

            Array.Copy(values, 0, bytes, address, values.Length);
        }

        public void WriteWord(int address, ushort value)
        {
            var b1 = (byte)(value >> 8);
            var b2 = (byte)(value & 0x00ff);

            bytes[address] = b1;
            bytes[address + 1] = b2;
        }

        public void WriteWords(int address, ushort[] values)
        {
            if (values.Length == 0)
            {
                return;
            }

            for (int i = 0; i < values.Length; i++)
            {
                var offset = i * 2;
                var value = values[i];
                bytes[address + offset] = (byte)(value >> 8);
                bytes[address + offset + 1] = (byte)(value & 0x00ff);
            }
        }

        public void WriteDWord(int address, uint value)
        {
            var b1 = (byte)(value >> 24);
            var b2 = (byte)((value & 0x00ff0000) >> 16);
            var b3 = (byte)((value & 0x0000ff00) >> 8);
            var b4 = (byte)(value & 0x000000ff);

            bytes[address] = b1;
            bytes[address + 1] = b2;
            bytes[address + 2] = b3;
            bytes[address + 3] = b4;
        }

        public void WriteDWords(int address, uint[] values)
        {
            if (values.Length == 0)
            {
                return;
            }

            for (int i = 0; i < values.Length; i++)
            {
                var offset = i * 4;
                var value = values[i];
                bytes[address + offset] = (byte)(value >> 24);
                bytes[address + offset + 1] = (byte)((value & 0x00ff0000) >> 16);
                bytes[address + offset + 2] = (byte)((value & 0x0000ff00) >> 8);
                bytes[address + offset + 3] = (byte)(value & 0x000000ff);
            }
        }

        public MemoryReader CreateReader(int address)
        {
            if (address < 0 || address >= bytes.Length)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            return new MemoryReader(this, address);
        }

        public int Size
        {
            get { return bytes.Length; }
        }
    }
}
