using System;
using ZDebug.Core.Extensions;

namespace ZDebug.Core.Instructions;

public sealed class InstructionReader
{
    private const byte OpKind_LargeConstant = 0;
    private const byte OpKind_SmallConstant = 1;
    private const byte OpKind_Variable = 2;
    private const byte OpKind_Omitted = 3;

    private int _address;
    private readonly byte[] _memory;
    private readonly OpcodeTable _opcodeTable;
    private readonly InstructionCache _cache;

    // This is used in instruction parsing
    private readonly byte[] _operandKinds = new byte[8];

    public InstructionReader(int address, byte[] memory, InstructionCache cache = null)
    {
        this._memory = memory;
        this._address = address;
        this._opcodeTable = OpcodeTables.GetOpcodeTable(memory[0]);
        this._cache = cache ?? new InstructionCache();
    }

    private void ReadOperandKinds(int offset = 0)
    {
        var b = _memory.ReadByte(ref _address);

        _operandKinds[offset] = (byte)((b & 0xc0) >> 6);
        _operandKinds[offset + 1] = (byte)((b & 0x30) >> 4);
        _operandKinds[offset + 2] = (byte)((b & 0x0c) >> 2);
        _operandKinds[offset + 3] = (byte)(b & 0x03);
    }

    private Operand ReadOperand(byte kind)
        => kind switch
        {
            OpKind_LargeConstant => new Operand((OperandKind)kind, _memory.ReadWord(ref _address)),
            OpKind_SmallConstant or OpKind_Variable => new Operand((OperandKind)kind, _memory.ReadByte(ref _address)),
            _ => throw new InstructionReaderException("Attempted to read ommitted operand."),
        };

    private ReadOnlyMemory<Operand> ReadOperands()
    {
        int size = 8;
        for (int i = 0; i < _operandKinds.Length; i++)
        {
            if (_operandKinds[i] == OpKind_Omitted)
            {
                size = i;
                break;
            }
        }

        var result = _cache.AllocateOperands(size);
        var span = result.Span;

        for (int i = 0; i < size; i++)
        {
            span[i] = ReadOperand(_operandKinds[i]);
        }

        return result;
    }

    private Variable ReadVariable(ref int address)
        => Variable.FromByte(_memory.ReadByte(ref address));

    private Branch ReadBranch(ref int address)
    {
        var b1 = _memory.ReadByte(ref address);

        var condition = (b1 & 0x80) == 0x80;

        short offset;
        if ((b1 & 0x40) == 0x40) // is single byte
        {
            // bottom 6 bits
            offset = (short)(b1 & 0x3f);
        }
        else // is two bytes
        {
            // OR bottom 6 bits with the next byte
            b1 = (byte)(b1 & 0x3f);
            var b2 = _memory.ReadByte(ref address);
            var tmp = (ushort)((b1 << 8) | b2);

            // if bit 13, set bits 14 and 15 as well to produce proper signed value.
            if ((tmp & 0x2000) == 0x2000)
            {
                tmp = (ushort)(tmp | 0xc000);
            }

            offset = (short)tmp;
        }

        return new Branch(condition, offset, address);
    }

    private ushort[] ReadZWords(ref int address)
    {
        int count = 0;
        while (true)
        {
            var zword = _memory.ReadWord(address + (count++ * 2));
            if ((zword & 0x8000) != 0)
            {
                break;
            }
        }

        return _memory.ReadWords(ref address, count);
    }

    private static byte LongForm(byte opByte)
    {
        return (byte)(opByte & 0x1f);
    }

    private static byte ShortForm(byte opByte)
    {
        return (byte)(opByte & 0x0f);
    }

    private static byte VarForm(byte opByte)
    {
        return (byte)(opByte & 0x1f);
    }

    public Instruction NextInstruction()
    {
        var startAddress = _address;

        if (_cache.TryGet(startAddress, out var instruction))
        {
            _address += instruction.Length;
            return instruction;
        }

        var opByte = _memory.ReadByte(ref _address);

        Opcode opcode;

        for (int i = 0; i < 8; i++)
        {
            _operandKinds[i] = OpKind_Omitted;
        }

        switch (opByte)
        {
            case >= 0x00 and <= 0x1f:
                opcode = _opcodeTable[OpcodeKind.TwoOp, LongForm(opByte)];
                _operandKinds[0] = OpKind_SmallConstant;
                _operandKinds[1] = OpKind_SmallConstant;
                break;

            case >= 0x20 and <= 0x3f:
                opcode = _opcodeTable[OpcodeKind.TwoOp, LongForm(opByte)];
                _operandKinds[0] = OpKind_SmallConstant;
                _operandKinds[1] = OpKind_Variable;
                break;

            case >= 0x40 and <= 0x5f:
                opcode = _opcodeTable[OpcodeKind.TwoOp, LongForm(opByte)];
                _operandKinds[0] = OpKind_Variable;
                _operandKinds[1] = OpKind_SmallConstant;
                break;

            case >= 0x60 and <= 0x7f:
                opcode = _opcodeTable[OpcodeKind.TwoOp, LongForm(opByte)];
                _operandKinds[0] = OpKind_Variable;
                _operandKinds[1] = OpKind_Variable;
                break;

            case >= 0x80 and <= 0x8f:
                opcode = _opcodeTable[OpcodeKind.OneOp, ShortForm(opByte)];
                _operandKinds[0] = OpKind_LargeConstant;
                break;

            case >= 0x90 and <= 0x9f:
                opcode = _opcodeTable[OpcodeKind.OneOp, ShortForm(opByte)];
                _operandKinds[0] = OpKind_SmallConstant;
                break;

            case >= 0xa0 and <= 0xaf:
                opcode = _opcodeTable[OpcodeKind.OneOp, ShortForm(opByte)];
                _operandKinds[0] = OpKind_Variable;
                break;

            case >= 0xb0 and <= 0xbd:
            case 0xbf:
                opcode = _opcodeTable[OpcodeKind.ZeroOp, ShortForm(opByte)];
                break;

            case 0xbe:
                opcode = _opcodeTable[OpcodeKind.Ext, _memory.ReadByte(ref _address)];
                ReadOperandKinds();
                break;

            case >= 0xc0 and <= 0xdf:
                opcode = _opcodeTable[OpcodeKind.TwoOp, VarForm(opByte)];
                ReadOperandKinds();
                break;

            default:
                opcode = _opcodeTable[OpcodeKind.VarOp, VarForm(opByte)];
                ReadOperandKinds();
                break;
        }

        if (opcode.IsDoubleVariable)
        {
            ReadOperandKinds(4);
        }

        var operands = ReadOperands();

        Variable storeVariable = null;
        if (opcode.HasStoreVariable)
        {
            storeVariable = ReadVariable(ref _address);
        }

        Branch? branch = null;
        if (opcode.HasBranch)
        {
            branch = ReadBranch(ref _address);
        }

        ushort[] ztext = null;
        if (opcode.HasZText)
        {
            ztext = ReadZWords(ref _address);
        }

        var length = _address - startAddress;

        instruction = new Instruction(startAddress, length, opcode, operands, storeVariable, branch, ztext);
        _cache.Add(startAddress, instruction);
        return instruction;
    }

    public int Address
    {
        get { return _address; }
        set { _address = value; }
    }
}
