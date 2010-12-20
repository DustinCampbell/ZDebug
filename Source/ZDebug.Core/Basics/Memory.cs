using System;
using System.IO;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Basics
{
    public sealed partial class Memory
    {
        private readonly byte[] bytes;

        internal Memory(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            this.bytes = bytes;
        }

        internal Memory(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.bytes = stream.ReadFully();
        }

        public byte ReadByte(int address)
        {
            if (address < 0 || address > bytes.Length - 1)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            return bytes[address];
        }

        public byte[] ReadBytes(int address, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (length == 0)
            {
                return ArrayEx.Empty<byte>();
            }

            if (address < 0 || address > bytes.Length - length)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            byte[] result = new byte[length];
            Array.Copy(bytes, address, result, 0, length);
            return result;
        }

        public ushort ReadWord(int address)
        {
            if (address < 0 || address > bytes.Length - 2)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            var b1 = bytes[address];
            var b2 = bytes[address + 1];

            return (ushort)(b1 << 8 | b2);
        }

        public ushort[] ReadWords(int address, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (length == 0)
            {
                return ArrayEx.Empty<ushort>();
            }

            if (address < 0 || address > bytes.Length - (length * 2))
            {
                throw new ArgumentOutOfRangeException("address");
            }

            ushort[] result = new ushort[length];
            for (int i = 0; i < length; i++)
            {
                var b1 = bytes[address + (i * 2)];
                var b2 = bytes[address + (i * 2) + 1];

                result[i] = (ushort)(b1 << 8 | b2);
            }

            return result;
        }

        public uint ReadDWord(int address)
        {
            if (address < 0 || address > bytes.Length - 4)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            var b1 = bytes[address];
            var b2 = bytes[address + 1];
            var b3 = bytes[address + 2];
            var b4 = bytes[address + 3];

            return (uint)(b1 << 24 | b2 << 16 | b3 << 8 | b4);
        }

        public uint[] ReadDWords(int address, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (length == 0)
            {
                return ArrayEx.Empty<uint>();
            }

            if (address < 0 || address > bytes.Length - (length * 4))
            {
                throw new ArgumentOutOfRangeException("address");
            }

            uint[] result = new uint[length];
            for (int i = 0; i < length; i++)
            {
                var b1 = bytes[address + (i * 4)];
                var b2 = bytes[address + (i * 4) + 1];
                var b3 = bytes[address + (i * 4) + 2];
                var b4 = bytes[address + (i * 4) + 3];

                result[i] = (uint)(b1 << 24 | b2 << 16 | b3 << 8 | b4);
            }

            return result;
        }

        public void WriteByte(int address, byte value)
        {
            if (address < 0 || address + 1 > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            byte oldValue = bytes[address];

            bytes[address] = value;

            var handler = MemoryChanged;
            if (handler != null)
            {
                handler(this, new MemoryEventArgs(this, address, 1));
            }
        }

        public void WriteBytes(int address, byte[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length == 0)
            {
                return;
            }

            if (address < 0 || address + values.Length > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            Array.Copy(values, 0, bytes, address, values.Length);

            var handler = MemoryChanged;
            if (handler != null)
            {
                handler(this, new MemoryEventArgs(this, address, values.Length));
            }
        }

        public void WriteWord(int address, ushort value)
        {
            if (address < 0 || address + 2 > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            var b1 = (byte)(value >> 8);
            var b2 = (byte)(value & 0x00ff);

            bytes[address] = b1;
            bytes[address + 1] = b2;

            var handler = MemoryChanged;
            if (handler != null)
            {
                handler(this, new MemoryEventArgs(this, address, 2));
            }
        }

        public void WriteWords(int address, ushort[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length == 0)
            {
                return;
            }

            if (address < 0 || address + (values.Length * 2) > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            for (int i = 0; i < values.Length; i++)
            {
                bytes[address + (i * 2)] = (byte)(values[i] >> 8);
                bytes[address + (i * 2) + 1] = (byte)(values[i] & 0x00ff);
            }

            var handler = MemoryChanged;
            if (handler != null)
            {
                handler(this, new MemoryEventArgs(this, address, values.Length * 2));
            }
        }

        public void WriteDWord(int address, uint value)
        {
            if (address < 0 || address + 4 > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            var b1 = (byte)(value >> 24);
            var b2 = (byte)((value & 0x00ff0000) >> 16);
            var b3 = (byte)((value & 0x0000ff00) >> 8);
            var b4 = (byte)(value & 0x000000ff);

            bytes[address] = b1;
            bytes[address + 1] = b2;
            bytes[address + 2] = b3;
            bytes[address + 3] = b4;

            var handler = MemoryChanged;
            if (handler != null)
            {
                handler(this, new MemoryEventArgs(this, address, 4));
            }
        }

        public void WriteDWords(int address, uint[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length == 0)
            {
                return;
            }

            if (address < 0 || address + (values.Length * 4) > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            for (int i = 0; i < values.Length; i++)
            {
                bytes[address + (i * 4)] = (byte)(values[i] >> 24);
                bytes[address + (i * 4) + 1] = (byte)((values[i] & 0x00ff0000) >> 16);
                bytes[address + (i * 4) + 2] = (byte)((values[i] & 0x0000ff00) >> 8);
                bytes[address + (i * 4) + 3] = (byte)(values[i] & 0x000000ff);
            }

            var handler = MemoryChanged;
            if (handler != null)
            {
                handler(this, new MemoryEventArgs(this, address, values.Length * 4));
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

        public event EventHandler<MemoryEventArgs> MemoryChanged;
    }
}
