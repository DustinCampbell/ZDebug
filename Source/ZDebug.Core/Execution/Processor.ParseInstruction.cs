using System;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        // constants used for instruction parsing
        private const byte opKind_Two = 0 * 32;
        private const byte opKind_One = 1 * 32;
        private const byte opKind_Zero = 2 * 32;
        private const byte opKind_Var = 3 * 32;
        private const byte opKind_Ext = 4 * 32;

        private const byte opKind_LargeConstant = 0;
        private const byte opKind_SmallConstant = 1;
        private const byte opKind_Variable = 2;
        private const byte opKind_Omitted = 3;

        // instruction state
        private int startAddress;
        private Opcode opcode;
        private readonly ushort[] operandValues = new ushort[8];
        private int operandCount;

        private void LoadOperand(byte kind)
        {
            if ((kind & opKind_Variable) != 0)
            {
                byte variableIndex = this.Memory[pc++];
                if (variableIndex < 16)
                {
                    if (variableIndex > 0)
                    {
                        operandValues[operandCount++] = locals[variableIndex - 0x01];
                    }
                    else
                    {
                        if (sp == callFrame)
                        {
                            throw new InvalidOperationException("Local stack is empty.");
                        }

                        operandValues[operandCount++] = (ushort)stack[sp--];
                    }
                }
                else
                {
                    operandValues[operandCount++] = this.Memory.ReadWord(globalVariableTableAddress + ((variableIndex - 0x10) * 2));
                }
            }
            else if ((kind & opKind_SmallConstant) != 0)
            {
                operandValues[operandCount++] = this.Memory[pc++];
            }
            else // kind == opKind_LargeConstant
            {
                operandValues[operandCount++] = this.Memory.ReadWord(pc);
                pc += 2;
            }
        }

        private void LoadAllOperands(byte kinds)
        {
            byte kind;

            kind = (byte)((kinds >> 6) & 0x03);
            if (kind == opKind_Omitted)
            {
                return;
            }

            LoadOperand(kind);

            kind = (byte)((kinds >> 4) & 0x03);
            if (kind == opKind_Omitted)
            {
                return;
            }

            LoadOperand(kind);

            kind = (byte)((kinds >> 2) & 0x03);
            if (kind == opKind_Omitted)
            {
                return;
            }

            LoadOperand(kind);

            kind = (byte)(kinds & 0x03);
            if (kind == opKind_Omitted)
            {
                return;
            }

            LoadOperand(kind);
        }

        private void ReadNextInstruction()
        {
            startAddress = pc;

            operandCount = 0;

            byte opByte = this.Memory[pc++];

            Opcode op;
            if (opByte < 0x80) // 2OP opcodes
            {
                op = opcodes[opKind_Two + (opByte & 0x1f)]; // long form
                LoadOperand((opByte & 0x40) != 0 ? opKind_Variable : opKind_SmallConstant);
                LoadOperand((opByte & 0x20) != 0 ? opKind_Variable : opKind_SmallConstant);
            }
            else if (opByte < 0xb0) // 1OP opcodes
            {
                op = opcodes[opKind_One + (opByte & 0x0f)]; // short form
                LoadOperand((byte)(opByte >> 4));
            }
            else if (opByte == 0xbe) // EXT opcodes
            {
                op = opcodes[opKind_Ext + this.Memory[pc++]];
                LoadAllOperands(this.Memory[pc++]);
            }
            else if (opByte < 0xc0) // 0OP opcodes
            {
                op = opcodes[opKind_Zero + (opByte & 0x0f)]; // short form
            }
            else // VAR opcodes
            {
                op = opcodes[(opByte < 0xe0 ? opKind_Two : opKind_Var) + (opByte & 0x1f)]; // var form

                if (!op.IsDoubleVariable)
                {
                    LoadAllOperands(this.Memory[pc++]);
                }
                else
                {
                    byte kinds1 = this.Memory[pc++];
                    byte kinds2 = this.Memory[pc++];
                    LoadAllOperands(kinds1);
                    LoadAllOperands(kinds2);
                }
            }

            opcode = op;
        }
    }
}
