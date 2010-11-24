using System;
using System.IO;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Basics
{
    internal sealed partial class Memory : IMemory
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

        public byte ReadByte(int index)
        {
            if (index < 0 || index > bytes.Length - 1)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            return bytes[index];
        }

        public byte[] ReadBytes(int index, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (length == 0)
            {
                return ArrayEx.Empty<byte>();
            }

            if (index < 0 || index > bytes.Length - length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            byte[] result = new byte[length];
            Array.Copy(bytes, index, result, 0, length);
            return result;
        }

        public ushort ReadWord(int index)
        {
            if (index < 0 || index > bytes.Length - 2)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var b1 = bytes[index];
            var b2 = bytes[index + 1];

            return (ushort)(b1 << 8 | b2);
        }

        public ushort[] ReadWords(int index, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (length == 0)
            {
                return ArrayEx.Empty<ushort>();
            }

            if (index < 0 || index > bytes.Length - (length * 2))
            {
                throw new ArgumentOutOfRangeException("index");
            }

            ushort[] result = new ushort[length];
            for (int i = 0; i < length; i++)
            {
                var b1 = bytes[index + (i * 2)];
                var b2 = bytes[index + (i * 2) + 1];

                result[i] = (ushort)(b1 << 8 | b2);
            }

            return result;
        }

        public void WriteByte(int index, byte value)
        {
            if (index < 0 || index > bytes.Length - 1)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            bytes[index] = value;
        }

        public void WriteBytes(int index, byte[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length == 0)
            {
                return;
            }

            if (index < 0 || index > bytes.Length - values.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            Array.Copy(values, 0, bytes, index, values.Length);
        }

        public void WriteWord(int index, ushort value)
        {
            if (index < 0 || index > bytes.Length - 2)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            bytes[index] = (byte)(value >> 8);
            bytes[index + 1] = (byte)(value & 0x00ff);
        }

        public void WriteWords(int index, ushort[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length == 0)
            {
                return;
            }

            if (index < 0 || index > bytes.Length - (values.Length * 2))
            {
                throw new ArgumentOutOfRangeException("index");
            }

            for (int i = 0; i < values.Length; i++)
            {
                bytes[index + (i * 2)] = (byte)(values[i] >> 8);
                bytes[index + (i * 2) + 1] = (byte)(values[i] & 0x00ff);
            }
        }

        public IMemoryReader CreateReader(int index)
        {
            if (index < 0 || index >= bytes.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            return new MemoryReader(this, index);
        }

        public int Size
        {
            get { return bytes.Length; }
        }
    }
}
