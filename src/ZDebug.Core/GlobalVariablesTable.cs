using System;
using ZDebug.Core.Basics;
using ZDebug.Core.Extensions;

namespace ZDebug.Core;

public class GlobalVariablesTable
{
    private readonly byte[] memory;
    private readonly int address;

    public GlobalVariablesTable(byte[] memory)
    {
        this.memory = memory;
        address = Header.ReadGlobalVariableTableAddress(memory);
    }

    private ushort ReadGlobalVariable(int index)
    {
        if (index < 0 || index > 239)
        {
            throw new ArgumentOutOfRangeException("index");
        }

        return memory.ReadWord(address + (index * 2));
    }

    private void WriteGlobalVariable(int index, ushort value)
    {
        if (index < 0 || index > 239)
        {
            throw new ArgumentOutOfRangeException("index");
        }

        memory.WriteWord(address + (index * 2), value);
    }

    public int Address => address;

    public ushort this[int index]
    {
        get => ReadGlobalVariable(index); set => WriteGlobalVariable(index, value);
    }
}
