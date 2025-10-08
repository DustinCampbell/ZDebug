using System;
using ZDebug.Core.Extensions;

namespace ZDebug.Core.Objects;

public class ZProperty
{
    private readonly byte[] memory;
    private readonly ZPropertyTable propertyTable;
    private readonly int index;
    private readonly int address;
    private readonly int number;
    private readonly int dataAddress;
    private readonly int length;

    internal ZProperty(byte[] memory, ZPropertyTable propertyTable, int index, int address, int number, int dataAddress, int length)
    {
        this.memory = memory;
        this.propertyTable = propertyTable;
        this.index = index;
        this.address = address;
        this.number = number;
        this.dataAddress = dataAddress;
        this.length = length;
    }

    public ZPropertyTable PropertyTable => propertyTable;

    public int Index => index;

    public int Address => address;

    public int Number => number;

    public int DataAddress => dataAddress;

    public int DataLength => length;

    public bool IsByte => length == 1;

    public bool IsWord => length == 2;

    public byte ReadAsByte()
    {
        if (!IsByte)
        {
            throw new InvalidOperationException("Attempted to read property with length " + length + " as a byte.");
        }

        return memory.ReadByte(dataAddress);
    }

    public void WriteAsByte(byte value)
    {
        if (!IsByte)
        {
            throw new InvalidOperationException("Attempted to write property with length " + length + " as a byte.");
        }

        memory.WriteByte(dataAddress, value);
    }

    public ushort ReadAsWord()
    {
        if (!IsWord)
        {
            throw new InvalidOperationException("Attempted to read property with length " + length + " as a word.");
        }

        return memory.ReadWord(dataAddress);
    }

    public void WriteAsWord(ushort value)
    {
        if (!IsWord)
        {
            throw new InvalidOperationException("Attempted to write property with length " + length + " as a word.");
        }

        memory.WriteWord(dataAddress, value);
    }

    public byte[] ReadAsBytes() => memory.ReadBytes(dataAddress, length);

    public void WriteAsBytes(byte[] values) => memory.WriteBytes(dataAddress, values);
}
