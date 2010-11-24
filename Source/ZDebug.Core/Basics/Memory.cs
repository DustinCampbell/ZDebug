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

        private void OnMemoryChanged(int index, int length, byte[] oldValues, byte[] newValues)
        {
            var handler = MemoryChanged;
            if (handler != null)
            {
                handler(this, new MemoryChangedEventArgs(this, index, length, oldValues, newValues));
            }
        }

        public void WriteByte(int index, byte value)
        {
            if (index < 0 || index + 1 > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            byte oldValue = bytes[index];

            bytes[index] = value;

            OnMemoryChanged(
                index,
                length: 1,
                oldValues: new byte[] { oldValue },
                newValues: new byte[] { value });
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

            if (index < 0 || index + values.Length > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var oldValues = bytes.Copy(index, values.Length);

            Array.Copy(values, 0, bytes, index, values.Length);

            OnMemoryChanged(
                index,
                length: values.Length,
                oldValues: oldValues,
                newValues: values);
        }

        public void WriteWord(int index, ushort value)
        {
            if (index < 0 || index + 2 > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var old1 = bytes[index];
            var old2 = bytes[index + 1];

            var b1 = (byte)(value >> 8);
            var b2 = (byte)(value & 0x00ff);

            bytes[index] = b1;
            bytes[index + 1] = b2;

            OnMemoryChanged(
                index,
                length: 2,
                oldValues: new byte[] { old1, old2 },
                newValues: new byte[] { b1, b2 });
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

            if (index < 0 || index + (values.Length * 2) > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var oldValues = bytes.Copy(index, values.Length * 2);

            for (int i = 0; i < values.Length; i++)
            {
                bytes[index + (i * 2)] = (byte)(values[i] >> 8);
                bytes[index + (i * 2) + 1] = (byte)(values[i] & 0x00ff);
            }

            var newValues = bytes.Copy(index, values.Length * 2);

            OnMemoryChanged(
                index,
                length: values.Length * 2,
                oldValues: oldValues,
                newValues: newValues);
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

        public event EventHandler<MemoryChangedEventArgs> MemoryChanged;
    }
}
