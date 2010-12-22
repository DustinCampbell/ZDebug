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
        private byte storeVariable;
        private bool branchCondition;
        private short branchOffset;
        private ushort[] zwords;

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
                    operandValues[operandCount++] = bytes.ReadWord(globalVariableTableAddress + ((variableIndex - 0x10) * 2));
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
            /* Instructions which test a condition are called "branch" instructions. The branch information is
             * stored in one or two bytes, indicating what to do with the result of the test. If bit 7 of the first
             * byte is 0, a branch occurs when the condition was false; if 1, then branch is on true. If bit 6 is set,
             * then the branch occupies 1 byte only, and the "offset" is in the range 0 to 63, given in the bottom
             * 6 bits. If bit 6 is clear, then the offset is a signed 14-bit number given in bits 0 to 5 of the first
             * byte followed by all 8 of the second. */

            byte specifier = bytes[address++];

            byte offset1 = (byte)(specifier & 0x3f);

            ushort offset;
            if ((specifier & 0x40) == 0) // long branch
            {
                if ((offset1 & 0x20) != 0) // propogate sign bit
                {
                    offset1 |= 0xc0;
                }

                byte offset2 = bytes[address++];

                offset = (ushort)((offset1 << 8) | offset2);
            }
            else // short branch
            {
                offset = offset1;
            }

            branchCondition = (specifier & 0x80) != 0;
            branchOffset = (short)offset;
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
