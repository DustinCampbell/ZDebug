#nullable enable

using System;
using System.Buffers.Binary;

namespace ZDebug.Core.Extensions;

public static class ByteArrayExtensions
{
    private const int WordSize = 2;
    private const int DWordSize = 4;

    public static byte ReadByte(this ReadOnlySpan<byte> source)
        => source[0];

    public static byte ReadByte(this byte[] array, int index)
        => array.AsSpan(index).ReadByte();

    public static byte ReadByte(this byte[] array, ref int index)
    {
        var result = array.AsSpan(index).ReadByte();
        index++;

        return result;
    }

    public static void CopyBytes(this ReadOnlySpan<byte> source, Span<byte> destination)
    {
        source[..destination.Length].CopyTo(destination);
    }

    public static byte[] ReadBytes(this ReadOnlySpan<byte> source, int count)
    {
        byte[] result = new byte[count];
        source.CopyBytes(result);

        return result;
    }

    public static byte[] ReadBytes(this byte[] array, int index, int count)
    {
        return ReadBytes(array.AsSpan(index), count);
    }

    public static byte[] ReadBytes(this byte[] array, ref int index, int count)
    {
        var result = ReadBytes(array.AsSpan(index), count);
        index += count;

        return result;
    }

    public static ushort ReadWord(this ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadUInt16BigEndian(source);
    }

    public static ushort ReadWord(this byte[] array, int index)
    {
        return ReadWord(array.AsSpan(index));
    }

    public static ushort ReadWord(this byte[] array, ref int index)
    {
        var result = ReadWord(array.AsSpan(index));
        index += WordSize;

        return result;
    }

    public static void CopyWords(this ReadOnlySpan<byte> source, Span<ushort> destination)
    {
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = ReadWord(source);
            source = source[WordSize..];
        }
    }

    public static ushort[] ReadWords(this ReadOnlySpan<byte> source, int count)
    {
        ushort[] result = new ushort[count];
        source.CopyWords(result);

        return result;
    }

    public static ushort[] ReadWords(this byte[] array, int index, int count)
    {
        return ReadWords(array.AsSpan(index), count);
    }

    public static ushort[] ReadWords(this byte[] array, ref int index, int length)
    {
        var result = ReadWords(array.AsSpan(index), length);
        index += length * WordSize;

        return result;
    }

    public static uint ReadDWord(this ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadUInt32BigEndian(source);
    }

    public static uint ReadDWord(this byte[] array, int index)
    {
        return ReadDWord(array.AsSpan(index));
    }

    public static uint ReadDWord(this byte[] array, ref int index)
    {
        var result = ReadDWord(array.AsSpan(index));
        index += DWordSize;

        return result;
    }

    public static void CopyDWords(this ReadOnlySpan<byte> source, Span<uint> destination)
    {
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = ReadDWord(source);
            source = source[DWordSize..];
        }
    }

    public static uint[] ReadDWords(this ReadOnlySpan<byte> source, int count)
    {
        uint[] result = new uint[count];
        source.CopyDWords(result);

        return result;
    }

    public static uint[] ReadDWords(this byte[] array, int index, int count)
    {
        return ReadDWords(array.AsSpan(index), count);
    }

    public static uint[] ReadDWords(this byte[] array, ref int index, int count)
    {
        var result = ReadDWords(array.AsSpan(index), count);
        index += count * DWordSize;

        return result;
    }

    public static void WriteByte(this Span<byte> destination, byte value)
    {
        destination[0] = value;
    }

    public static void WriteByte(this byte[] array, int index, byte value)
    {
        array.AsSpan(index).WriteByte(value);
    }

    public static void WriteByte(this byte[] array, ref int index, byte value)
    {
        array.AsSpan(index).WriteByte(value);
        index++;
    }

    public static void WriteBytes(this Span<byte> destination, ReadOnlySpan<byte> values)
    {
        values.CopyTo(destination);
    }

    public static void WriteBytes(this byte[] array, int index, byte[] values)
    {
        array.AsSpan(index).WriteBytes(values);
    }

    public static void WriteBytes(this byte[] array, ref int index, byte[] values)
    {
        array.AsSpan(index).WriteBytes(values);
        index += values.Length;
    }

    public static void WriteWord(this Span<byte> destination, ushort value)
    {
        BinaryPrimitives.WriteUInt16BigEndian(destination, value);
    }

    public static void WriteWord(this byte[] bytes, int index, ushort value)
    {
        bytes.AsSpan(index).WriteWord(value);
    }

    public static void WriteWord(this byte[] bytes, ref int index, ushort value)
    {
        bytes.AsSpan(index).WriteWord(value);
        index += WordSize;
    }

    public static void WriteWords(this Span<byte> destination, ReadOnlySpan<ushort> values)
    {
        foreach (var value in values)
        {
            destination.WriteWord(value);
            destination = destination[WordSize..];
        }
    }

    public static void WriteWords(this byte[] array, int index, ushort[] values)
    {
        array.AsSpan(index).WriteWords(values);
    }

    public static void WriteWords(this byte[] array, ref int index, ushort[] values)
    {
        array.AsSpan(index).WriteWords(values);
        index += values.Length * WordSize;
    }

    public static void WriteDWord(this Span<byte> destination, uint value)
    {
        BinaryPrimitives.WriteUInt32BigEndian(destination, value);
    }

    public static void WriteDWord(this byte[] array, int index, uint value)
    {
        array.AsSpan(index).WriteDWord(value);
    }

    public static void WriteDWord(this byte[] array, ref int index, uint value)
    {
        array.AsSpan(index).WriteDWord(value);
        index += DWordSize;
    }

    public static void WriteDWords(this Span<byte> destination, ReadOnlySpan<uint> values)
    {
        foreach (var value in values)
        {
            destination.WriteDWord(value);
            destination = destination[DWordSize..];
        }
    }

    public static void WriteDWords(this byte[] array, int index, uint[] values)
    {
        array.AsSpan(index).WriteDWords(values);
    }

    public static void WriteDWords(this byte[] array, ref int index, uint[] values)
    {
        array.AsSpan(index).WriteDWords(values);
        index += values.Length * DWordSize;
    }
}
