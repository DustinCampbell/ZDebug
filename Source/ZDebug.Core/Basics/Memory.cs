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

        private void OnMemoryChanged(int address, int length, byte[] oldValues, byte[] newValues)
        {
            var handler = MemoryChanged;
            if (handler != null)
            {
                handler(this, new MemoryChangedEventArgs(this, address, length, oldValues, newValues));
            }
        }

        public void WriteByte(int address, byte value)
        {
            if (address < 0 || address + 1 > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            byte oldValue = bytes[address];

            bytes[address] = value;

            OnMemoryChanged(
                address,
                length: 1,
                oldValues: new byte[] { oldValue },
                newValues: new byte[] { value });
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

            var oldValues = bytes.ShallowCopy(address, values.Length);

            Array.Copy(values, 0, bytes, address, values.Length);

            OnMemoryChanged(
                address,
                length: values.Length,
                oldValues: oldValues,
                newValues: values);
        }

        public void WriteWord(int address, ushort value)
        {
            if (address < 0 || address + 2 > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            var old1 = bytes[address];
            var old2 = bytes[address + 1];

            var b1 = (byte)(value >> 8);
            var b2 = (byte)(value & 0x00ff);

            bytes[address] = b1;
            bytes[address + 1] = b2;

            OnMemoryChanged(
                address,
                length: 2,
                oldValues: new byte[] { old1, old2 },
                newValues: new byte[] { b1, b2 });
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

            var oldValues = bytes.ShallowCopy(address, values.Length * 2);

            for (int i = 0; i < values.Length; i++)
            {
                bytes[address + (i * 2)] = (byte)(values[i] >> 8);
                bytes[address + (i * 2) + 1] = (byte)(values[i] & 0x00ff);
            }

            var newValues = bytes.ShallowCopy(address, values.Length * 2);

            OnMemoryChanged(
                address,
                length: values.Length * 2,
                oldValues: oldValues,
                newValues: newValues);
        }

        public IMemoryReader CreateReader(int address)
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

        public event EventHandler<MemoryChangedEventArgs> MemoryChanged;
    }
}
