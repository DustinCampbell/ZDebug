using System;
using ZDebug.Core.Extensions;

namespace ZDebug.Core.Objects;

public class ZProperty
{
    private readonly Memory<byte> memory;
    private readonly ZPropertyTable propertyTable;
    private readonly int index;
    private readonly int address;
    private readonly int number;
    private readonly int dataAddress;
    private readonly int length;

    internal ZProperty(Memory<byte> memory, ZPropertyTable propertyTable, int index, int address, int number, int dataAddress, int length)
    {
        this.memory = memory;
        this.propertyTable = propertyTable;
        this.index = index;
        this.address = address;
        this.number = number;
        this.dataAddress = dataAddress;
        this.length = length;
    }

    public ZPropertyTable PropertyTable
    {
        get { return propertyTable; }
    }

    public int Index
    {
        get { return index; }
    }

    public int Address
    {
        get { return address; }
    }

    public int Number
    {
        get { return number; }
    }

    public int DataAddress
    {
        get { return dataAddress; }
    }

    public int DataLength
    {
        get { return length; }
    }

    public bool IsByte
    {
        get { return length == 1; }
    }

    public bool IsWord
    {
        get { return length == 2; }
    }

    private Span<byte> Data => memory.Span[dataAddress..];

    public byte ReadAsByte()
    {
        if (!IsByte)
        {
            throw new InvalidOperationException("Attempted to read property with length " + length + " as a byte.");
        }

        return Data.ReadByte();
    }

    public void WriteAsByte(byte value)
    {
        if (!IsByte)
        {
            throw new InvalidOperationException("Attempted to write property with length " + length + " as a byte.");
        }

        Data.WriteByte(value);
    }

    public ushort ReadAsWord()
    {
        if (!IsWord)
        {
            throw new InvalidOperationException("Attempted to read property with length " + length + " as a word.");
        }

        return Data.ReadWord();
    }

    public void WriteAsWord(ushort value)
    {
        if (!IsWord)
        {
            throw new InvalidOperationException("Attempted to write property with length " + length + " as a word.");
        }

        Data.WriteWord(value);
    }

    public byte[] ReadAsBytes()
    {
        return Data.ReadBytes(length);
    }

    public void WriteAsBytes(byte[] values)
    {
        Data.WriteBytes(values);
    }
}
