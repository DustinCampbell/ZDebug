#nullable enable

using System;
using ZDebug.Core.Extensions;

namespace ZDebug.Core.Basics;

public struct MemoryReader(Memory<byte> memory, int address)
{
    private const int WordSize = 2;
    private const int DWordSize = 4;

    private readonly Memory<byte> memory = memory;
    private int address = address;

    private readonly ReadOnlySpan<byte> Span => memory.Span[address..];

    public readonly Memory<byte> Memory => memory;

    public int Address
    {
        readonly get => address;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, memory.Length);

            address = value;
        }
    }

    public readonly int BytesRemaining => Span.Length;

    public byte ReadByte()
    {
        var result = Span.ReadByte();
        address++;

        return result;
    }

    public void CopyBytes(Span<byte> destination)
    {
        Span.CopyBytes(destination);
        address += destination.Length;
    }

    public ushort ReadWord()
    {
        var result = Span.ReadWord();
        address += WordSize;

        return result;
    }

    public void CopyWords(Span<ushort> destination)
    {
        Span.CopyWords(destination);
        address += destination.Length * WordSize;
    }

    public uint ReadDWord()
    {
        var result = Span.ReadDWord();
        address += DWordSize;

        return result;
    }

    public void CopyDWords(Span<uint> destination)
    {
        Span.CopyDWords(destination);
        address += destination.Length * DWordSize;
    }

    public void Skip(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, BytesRemaining);

        address += count;
    }
}
