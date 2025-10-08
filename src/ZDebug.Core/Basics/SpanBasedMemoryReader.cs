using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace ZDebug.Core.Basics;

public ref struct SpanBasedMemoryReader
{
    private readonly Memory<byte> memory;
    private ReadOnlySpan<byte> span;
    private int address;

    public SpanBasedMemoryReader(byte[] memory, int address)
    {
        this.memory = memory;
        span = this.memory.Span[address..];
        this.address = address;
    }

    public readonly Memory<byte> Memory => memory;
    public readonly int Size => memory.Length;
    public readonly int RemainingBytes => memory.Length - address;

    public int Address
    {
        readonly get => address;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, Size);

            address = value;
        }
    }

    public byte NextByte()
    {
        var result = span[0];
        span = span[1..];
        address++;

        return result;
    }

    public ReadOnlySpan<byte> NextBytes(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, RemainingBytes);

        var result = span[..count];
        span = span[count..];
        address += count;

        return result;
    }

    public void CopyNextBytes(int count, Span<byte> destination)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, RemainingBytes);

        span[..count].CopyTo(destination);
        span = span[count..];
        address += count;
    }

    public ushort NextWord()
    {
        var result = BinaryPrimitives.ReadUInt16BigEndian(span);
        span = span[2..];
        address += 2;

        return result;
    }

    public ReadOnlySpan<ushort> NextWords(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count * 2, RemainingBytes);

        var result = new ushort[count];

        for (var i = 0; i < count; i++)
        {
            result[i] = NextWord();
        }

        return result;
    }

    public void CopyNextWords(int count, Span<ushort> destination)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count * 2, RemainingBytes);

        for (var i = 0; i < count; i++)
        {
            destination[i] = NextWord();
        }
    }

    public uint NextDWord()
    {
        var result = BinaryPrimitives.ReadUInt32BigEndian(span);
        span = span[4..];
        address += 4;

        return result;
    }

    public ReadOnlySpan<uint> NextDWords(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count * 4, RemainingBytes);

        var result = new uint[count];

        for (var i = 0; i < count; i++)
        {
            result[i] = NextDWord();
        }

        return result;
    }

    public void Skip(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, RemainingBytes);

        span = span[count..];
        address += count;
    }
}
