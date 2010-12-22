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
        private byte[] operandKinds = new byte[8];
        private ushort[] operandValues = new ushort[8];
        private int operandCount;
        private byte storeVariable;
        private bool branchCondition;
        private short branchOffset;
        private ushort[] zwords;

        private void ReadOperandKinds(ref int address, int offset = 0)
        {
            byte b = bytes[address++];

            byte[] opKinds = operandKinds;

            opKinds[offset] = (byte)((b & 0xc0) >> 6);
            opKinds[offset + 1] = (byte)((b & 0x30) >> 4);
            opKinds[offset + 2] = (byte)((b & 0x0c) >> 2);
            opKinds[offset + 3] = (byte)(b & 0x03);
        }

        private void LoadOperand(byte kind, ref int address)
        {
            if ((kind & opKind_Variable) != 0)
            {
                byte variableIndex = bytes[address++];
                if (variableIndex < 16)
                {
                    if (variableIndex > 0)
                    {
                        operandValues[operandCount++] = locals[variableIndex - 0x01];
                    }
                    else
                    {
                        if (localStackSize == 0)
                        {
                            throw new InvalidOperationException("Local stack is empty.");
                        }

                        localStackSize--;
                        operandValues[operandCount++] = (ushort)stack[stackPointer--];
                    }
                }
                else
                {
                    operandValues[operandCount++] = bytes.ReadWord(this.globalVariableTableAddress + ((variableIndex - 0x10) * 2));
                }
            }
            else if ((kind & opKind_SmallConstant) != 0)
            {
                operandValues[operandCount++] = bytes[address++];
            }
            else // kind == opKind_LargeConstant
            {
                operandValues[operandCount++] = bytes.ReadWord(address);
                address += 2;
            }
        }

        private void LoadAllOperands(byte kinds, ref int address)
        {
            for (int i = 6; i >= 0; i -= 2)
            {
                byte kind = (byte)((kinds >> i) & 0x03);
                if (kind == opKind_Omitted)
                {
                    break;
                }

                LoadOperand(kind, ref address);
            }
        }

        private void ReadBranch(ref int address)
        {
            var b1 = bytes[address++];

            var condition = (b1 & 0x80) != 0;

            short offset;
            if ((b1 & 0x40) != 0) // is single byte
            {
                // bottom 6 bits
                offset = (short)(b1 & 0x3f);
            }
            else // is two bytes
            {
                // OR bottom 6 bits with the next byte
                b1 = (byte)(b1 & 0x3f);
                var b2 = bytes[address++];
                var tmp = (ushort)((b1 << 8) | b2);

                // if bit 13, set bits 14 and 15 as well to produce proper signed value.
                if ((tmp & 0x2000) != 0)
                {
                    tmp = (ushort)(tmp | 0xc000);
                }

                offset = (short)tmp;
            }

            this.branchCondition = condition;
            this.branchOffset = offset;
        }

        private ushort[] ReadZWords(ref int address)
        {
            int count = 0;
            while (true)
            {
                var zword = memory.ReadWord(address + (count++ * 2));
                if ((zword & 0x8000) != 0)
                {
                    break;
                }
            }

            return memory.ReadWords(ref address, count);
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

        private void ReadNextInstruction()
        {
            startAddress = pc;
            int address = startAddress;

            operandCount = 0;

            byte opByte = bytes[address++];

            Opcode op;
            if (opByte < 0x80) // 2OP opcodes
            {
                op = opcodes[opKind_Two + (opByte & 0x1f)]; // long form
                LoadOperand((opByte & 0x40) != 0 ? opKind_Variable : opKind_SmallConstant, ref address);
                LoadOperand((opByte & 0x20) != 0 ? opKind_Variable : opKind_SmallConstant, ref address);
            }
            else if (opByte < 0xb0) // 1OP opcodes
            {
                op = opcodes[opKind_One + (opByte & 0x0f)]; // short form
                LoadOperand((byte)(opByte >> 4), ref address);
            }
            else if (opByte == 0xbe) // EXT opcodes
            {
                op = opcodes[opKind_Ext + bytes[address++]];
                LoadAllOperands(bytes[address++], ref address);
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
                    LoadAllOperands(bytes[address++], ref address);
                }
                else
                {
                    byte kinds1 = bytes[address++];
                    byte kinds2 = bytes[address++];
                    LoadAllOperands(kinds1, ref address);
                    LoadAllOperands(kinds2, ref address);
                }
            }

            if (op.HasStoreVariable)
            {
                storeVariable = bytes[address++];
            }

            if (op.HasBranch)
            {
                ReadBranch(ref address);
            }

            if (op.HasZText)
            {
                zwords = ReadZWords(ref address);
            }

            opcode = op;
            pc += address - startAddress;
        }
    }
}
