#nullable enable

using System;
using ZDebug.Core.Extensions;

namespace ZDebug.Core.Basics;

public struct MemoryWriter(Memory<byte> memory, int address)
{
    private const int WordSize = 2;
    private const int DWordSize = 4;

    private readonly Memory<byte> memory = memory;
    private int address = address;

    private readonly Span<byte> Span => memory.Span[address..];

    public readonly int Address => address;
    public readonly int BytesRemaining => Span.Length;

    public void WriteByte(byte value)
    {
        Span.WriteByte(value);
        address++;
    }

    public void WriteWord(ushort value)
    {
        Span.WriteWord(value);
        address += WordSize;
    }

    public void WriteDWord(uint value)
    {
        Span.WriteDWord(value);
        address += DWordSize;
    }
}
