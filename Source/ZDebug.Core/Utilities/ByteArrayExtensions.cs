using System;

namespace ZDebug.Core.Utilities
{
    public static class ByteArrayExtensions
    {
        public static byte ReadByte(this byte[] array, int index)
        {
            return array[index];
        }

        public static byte ReadByte(this byte[] array, ref int index)
        {
            return array[index++];
        }

        public static byte[] ReadBytes(this byte[] array, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(array, index, result, 0, length);
            return result;
        }

        public static byte[] ReadBytes(this byte[] array, ref int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(array, index, result, 0, length);
            index += length;
            return result;
        }

        public static ushort ReadWord(this byte[] bytes, int index)
        {
            byte b1 = bytes[index];
            byte b2 = bytes[index + 1];

            return (ushort)(b1 << 8 | b2);
        }

        public static ushort ReadWord(this byte[] array, ref int index)
        {
            byte b1 = array[index++];
            byte b2 = array[index++];

            return (ushort)(b1 << 8 | b2);
        }

        public static ushort[] ReadWords(this byte[] array, int index, int length)
        {
            ushort[] result = new ushort[length];

            for (int i = 0; i < length; i++)
            {
                byte b1 = array[index + (i * 2)];
                byte b2 = array[index + (i * 2) + 1];

                result[i] = (ushort)(b1 << 8 | b2);
            }

            return result;
        }

        public static ushort[] ReadWords(this byte[] array, ref int index, int length)
        {
            ushort[] result = new ushort[length];

            for (int i = 0; i < length; i++)
            {
                byte b1 = array[index++];
                byte b2 = array[index++];

                result[i] = (ushort)(b1 << 8 | b2);
            }

            return result;
        }

        public static void WriteByte(this byte[] array, int index, byte value)
        {
            array[index] = value;
        }

        public static void WriteByte(this byte[] array, ref int index, byte value)
        {
            array[index++] = value;
        }

        public static void WriteBytes(this byte[] array, int index, byte[] values)
        {
            Array.Copy(values, 0, array, index, values.Length);
        }

        public static void WriteBytes(this byte[] array, ref int index, byte[] values)
        {
            Array.Copy(values, 0, array, index, values.Length);
            index += values.Length;
        }

        public static void WriteWord(this byte[] bytes, int index, ushort value)
        {
            byte b1 = (byte)(value >> 8);
            byte b2 = (byte)(value & 0x00ff);

            bytes[index] = b1;
            bytes[index + 1] = b2;
        }

        public static void WriteWord(this byte[] bytes, ref int index, ushort value)
        {
            byte b1 = (byte)(value >> 8);
            byte b2 = (byte)(value & 0x00ff);

            bytes[index++] = b1;
            bytes[index++] = b2;
        }

        public static void WriteWords(this byte[] array, int index, ushort[] values)
        {
            int i = 0;
            foreach (var value in values)
            {
                byte b1 = (byte)(value >> 8);
                byte b2 = (byte)(value & 0x00ff);

                array[index + i] = b1;
                array[index + i+ 1] = b2;

                i += 2;
            }
        }

        public static void WriteWords(this byte[] array, ref int index, ushort[] values)
        {
            foreach (var value in values)
            {
                byte b1 = (byte)(value >> 8);
                byte b2 = (byte)(value & 0x00ff);

                array[index++] = b1;
                array[index++] = b2;
            }
        }
    }
}
