using System;
using System.Collections.Generic;

namespace ZDebug.Core.Instructions;

public sealed class InstructionCache
{
    private readonly Dictionary<int, Instruction> _map;

    private Operand[] _operandArray = new Operand[1024];
    private int _operandArrayFreeIndex;
    private int _operandArraySize = 1024;

    private ushort[] _zwordArray = new ushort[1024];
    private int _zwordArrayFreeIndex;
    private int _zwordArraySize = 1024;

    public InstructionCache(int capacity = 0)
    {
        _map = new Dictionary<int, Instruction>(capacity);
    }

    internal bool TryGet(int address, out Instruction instruction)
    {
        return _map.TryGetValue(address, out instruction);
    }

    internal void Add(int address, Instruction instruction)
    {
        _map.Add(address, instruction);
    }

    internal Memory<Operand> AllocateOperands(int length)
    {
        if (length == 0)
        {
            return Memory<Operand>.Empty;
        }

        if (_operandArrayFreeIndex > _operandArraySize - length)
        {
            var newSize = _operandArray.Length * 2;
            var newOperandArray = new Operand[newSize];
            Array.Copy(_operandArray, 0, newOperandArray, 0, _operandArray.Length);
            _operandArray = newOperandArray;
            _operandArraySize = newSize;
        }

        var result = _operandArray.AsMemory(_operandArrayFreeIndex, length);
        _operandArrayFreeIndex += length;
        return result;
    }

    internal Memory<ushort> AllocateZWords(int length)
    {
        if (length == 0)
        {
            return Memory<ushort>.Empty;
        }

        if (_zwordArrayFreeIndex > _zwordArraySize - length)
        {
            var newSize = _zwordArray.Length * 2;
            var newZWordsArray = new ushort[newSize];
            Array.Copy(_zwordArray, 0, newZWordsArray, 0, _zwordArray.Length);
            _zwordArray = newZWordsArray;
            _zwordArraySize = newSize;
        }

        var result = _zwordArray.AsMemory(_zwordArrayFreeIndex, length);
        _zwordArrayFreeIndex += length;
        return result;
    }
}
