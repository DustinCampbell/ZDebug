using System;
using System.Buffers.Binary;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Extensions;

public static class ByteArrayExtensions
{
    public static byte ReadByte(this byte[] array, int index)
        => array[index];

    public static byte ReadByte(this byte[] array, ref int index)
        => array[index++];

    public static byte[] ReadBytes(this byte[] array, int index, int length)
    {
        var result = new byte[length];
        Array.Copy(array, index, result, 0, length);
        return result;
    }

    public static byte[] ReadBytes(this byte[] array, ref int index, int length)
    {
        var result = new byte[length];
        Array.Copy(array, index, result, 0, length);
        index += length;
        return result;
    }

    public static ushort ReadWord(this byte[] array, int index)
        => BinaryPrimitives.ReadUInt16BigEndian(array.AsSpan(index));

    public static ushort ReadWord(this byte[] array, ref int index)
    {
        var result = BinaryPrimitives.ReadUInt16BigEndian(array.AsSpan(index));
        index += 2;

        return result;
    }

    public static ushort[] ReadWords(this byte[] array, int index, int length)
    {
        var span = array.AsSpan(index);
        var result = new ushort[length];

        for (var i = 0; i < length; i++)
        {
            result[i] = BinaryPrimitives.ReadUInt16BigEndian(span);
            span = span.Slice(2);
        }

        return result;
    }

    public static ushort[] ReadWords(this byte[] array, ref int index, int length)
    {
        var span = array.AsSpan(index);
        var result = new ushort[length];

        for (var i = 0; i < length; i++)
        {
            result[i] = BinaryPrimitives.ReadUInt16BigEndian(span);
            span = span.Slice(2);
        }

        index += length * 2;

        return result;
    }

    public static uint ReadDWord(this byte[] array, int index)
    {
        var b1 = array[index];
        var b2 = array[index + 1];
        var b3 = array[index + 2];
        var b4 = array[index + 3];

        return (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
    }

    public static uint ReadDWord(this byte[] array, ref int index)
    {
        var b1 = array[index];
        var b2 = array[index + 1];
        var b3 = array[index + 2];
        var b4 = array[index + 3];

        index += 4;

        return (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
    }

    public static uint[] ReadDWords(this byte[] array, int index, int length)
    {
        var result = new uint[length];
        for (var i = 0; i < length; i++)
        {
            var offset = i * 4;
            var b1 = array[index + offset];
            var b2 = array[index + offset + 1];
            var b3 = array[index + offset + 2];
            var b4 = array[index + offset + 3];

            result[i] = (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        return result;
    }

    public static uint[] ReadDWords(this byte[] array, ref int index, int length)
    {
        if (length == 0)
        {
            return ArrayEx.Empty<uint>();
        }

        var result = new uint[length];
        for (var i = 0; i < length; i++)
        {
            var offset = i * 4;
            var b1 = array[index + offset];
            var b2 = array[index + offset + 1];
            var b3 = array[index + offset + 2];
            var b4 = array[index + offset + 3];

            result[i] = (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        index += length * 4;

        return result;
    }

    public static void WriteByte(this byte[] array, int index, byte value) => array[index] = value;

    public static void WriteByte(this byte[] array, ref int index, byte value) => array[index++] = value;

    public static void WriteBytes(this byte[] array, int index, byte[] values) => Array.Copy(values, 0, array, index, values.Length);

    public static void WriteBytes(this byte[] array, ref int index, byte[] values)
    {
        Array.Copy(values, 0, array, index, values.Length);
        index += values.Length;
    }

    public static void WriteWord(this byte[] bytes, int index, ushort value)
    {
        var b1 = (byte)(value >> 8);
        var b2 = (byte)(value & 0x00ff);

        bytes[index] = b1;
        bytes[index + 1] = b2;
    }

    public static void WriteWord(this byte[] bytes, ref int index, ushort value)
    {
        var b1 = (byte)(value >> 8);
        var b2 = (byte)(value & 0x00ff);

        bytes[index++] = b1;
        bytes[index++] = b2;
    }

    public static void WriteWords(this byte[] array, int index, ushort[] values)
    {
        var i = 0;
        foreach (var value in values)
        {
            var b1 = (byte)(value >> 8);
            var b2 = (byte)(value & 0x00ff);

            array[index + i] = b1;
            array[index + i + 1] = b2;

            i += 2;
        }
    }

    public static void WriteWords(this byte[] array, ref int index, ushort[] values)
    {
        foreach (var value in values)
        {
            var b1 = (byte)(value >> 8);
            var b2 = (byte)(value & 0x00ff);

            array[index++] = b1;
            array[index++] = b2;
        }
    }

    public static void WriteDWord(this byte[] array, int index, uint value)
    {
        var b1 = (byte)(value >> 24);
        var b2 = (byte)((value & 0x00ff0000) >> 16);
        var b3 = (byte)((value & 0x0000ff00) >> 8);
        var b4 = (byte)(value & 0x000000ff);

        array[index] = b1;
        array[index + 1] = b2;
        array[index + 2] = b3;
        array[index + 3] = b4;
    }

    public static void WriteDWord(this byte[] array, ref int index, uint value)
    {
        var b1 = (byte)(value >> 24);
        var b2 = (byte)((value & 0x00ff0000) >> 16);
        var b3 = (byte)((value & 0x0000ff00) >> 8);
        var b4 = (byte)(value & 0x000000ff);

        array[index] = b1;
        array[index + 1] = b2;
        array[index + 2] = b3;
        array[index + 3] = b4;

        index += 4;
    }

    public static void WriteDWords(this byte[] array, int index, uint[] values)
    {
        if (values.Length == 0)
        {
            return;
        }

        for (var i = 0; i < values.Length; i++)
        {
            var offset = i * 4;
            var value = values[i];
            array[index + offset] = (byte)(value >> 24);
            array[index + offset + 1] = (byte)((value & 0x00ff0000) >> 16);
            array[index + offset + 2] = (byte)((value & 0x0000ff00) >> 8);
            array[index + offset + 3] = (byte)(value & 0x000000ff);
        }
    }

    public static void WriteDWords(this byte[] array, ref int index, uint[] values)
    {
        if (values.Length == 0)
        {
            return;
        }

        for (var i = 0; i < values.Length; i++)
        {
            var value = values[i];
            array[index++] = (byte)(value >> 24);
            array[index++] = (byte)((value & 0x00ff0000) >> 16);
            array[index++] = (byte)((value & 0x0000ff00) >> 8);
            array[index++] = (byte)(value & 0x000000ff);
        }
    }
}
