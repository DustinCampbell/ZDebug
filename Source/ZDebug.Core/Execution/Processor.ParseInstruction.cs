using ZDebug.Core.Instructions;

namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        // constants used for instruction parsing
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
        private Variable store;
        private Branch branch;
        private ushort[] zwords;

        private void ReadOperandKinds(ref int address, int offset = 0)
        {
            var b = memory.ReadByte(ref address);

            operandKinds[offset] = (byte)((b & 0xc0) >> 6);
            operandKinds[offset + 1] = (byte)((b & 0x30) >> 4);
            operandKinds[offset + 2] = (byte)((b & 0x0c) >> 2);
            operandKinds[offset + 3] = (byte)(b & 0x03);
        }

        private Variable ReadVariable(ref int address)
        {
            return Variable.FromByte(memory.ReadByte(ref address));
        }

        private ushort ReadOperandValue(ref int address, byte kind)
        {
            switch (kind)
            {
                case opKind_LargeConstant:
                    return memory.ReadWord(ref address);

                case opKind_SmallConstant:
                    return memory.ReadByte(ref address);

                case opKind_Variable:
                    Variable var = ReadVariable(ref address);
                    return ReadVariableValue(var);

                default:
                    throw new InstructionReaderException("Attempted to read ommitted operand.");
            }
        }

        private void ReadOperandValues(ref int address)
        {
            operandCount = 8;
            for (int i = 0; i < operandKinds.Length; i++)
            {
                var opKind = operandKinds[i];
                if (opKind != opKind_Omitted)
                {
                    operandValues[i] = ReadOperandValue(ref address, opKind);
                }
                else
                {
                    operandCount = i;
                    break;
                }
            }
        }

        private Branch ReadBranch(ref int address)
        {
            var b1 = memory.ReadByte(ref address);

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
                var b2 = memory.ReadByte(ref address);
                var tmp = (ushort)((b1 << 8) | b2);

                // if bit 13, set bits 14 and 15 as well to produce proper signed value.
                if ((tmp & 0x2000) == 0x2000)
                {
                    tmp = (ushort)(tmp | 0xc000);
                }

                offset = (short)tmp;
            }

            return new Branch(condition, offset);
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

            for (int i = 0; i < 8; i++)
            {
                operandKinds[i] = opKind_Omitted;
            }

            var opByte = memory.ReadByte(ref address);

            if (opByte >= 0x00 && opByte <= 0x1f)
            {
                opcode = opcodeTable[OpcodeKind.TwoOp, LongForm(opByte)];
                operandKinds[0] = opKind_SmallConstant;
                operandKinds[1] = opKind_SmallConstant;
            }
            else if (opByte >= 0x20 && opByte <= 0x3f)
            {
                opcode = opcodeTable[OpcodeKind.TwoOp, LongForm(opByte)];
                operandKinds[0] = opKind_SmallConstant;
                operandKinds[1] = opKind_Variable;
            }
            else if (opByte >= 0x40 && opByte <= 0x5f)
            {
                opcode = opcodeTable[OpcodeKind.TwoOp, LongForm(opByte)];
                operandKinds[0] = opKind_Variable;
                operandKinds[1] = opKind_SmallConstant;
            }
            else if (opByte >= 0x60 && opByte <= 0x7f)
            {
                opcode = opcodeTable[OpcodeKind.TwoOp, LongForm(opByte)];
                operandKinds[0] = opKind_Variable;
                operandKinds[1] = opKind_Variable;
            }
            else if (opByte >= 0x80 && opByte <= 0x8f)
            {
                opcode = opcodeTable[OpcodeKind.OneOp, ShortForm(opByte)];
                operandKinds[0] = opKind_LargeConstant;
            }
            else if (opByte >= 0x90 && opByte <= 0x9f)
            {
                opcode = opcodeTable[OpcodeKind.OneOp, ShortForm(opByte)];
                operandKinds[0] = opKind_SmallConstant;
            }
            else if (opByte >= 0xa0 && opByte <= 0xaf)
            {
                opcode = opcodeTable[OpcodeKind.OneOp, ShortForm(opByte)];
                operandKinds[0] = opKind_Variable;
            }
            else if ((opByte >= 0xb0 && opByte <= 0xbd) || opByte == 0xbf)
            {
                opcode = opcodeTable[OpcodeKind.ZeroOp, ShortForm(opByte)];
            }
            else if (opByte == 0xbe)
            {
                opcode = opcodeTable[OpcodeKind.Ext, memory.ReadByte(ref address)];
                ReadOperandKinds(ref address);
            }
            else if (opByte >= 0xc0 && opByte <= 0xdf)
            {
                opcode = opcodeTable[OpcodeKind.TwoOp, VarForm(opByte)];
                ReadOperandKinds(ref address);
            }
            else // opByte >= 0xe0 && opByte <= 0xff
            {
                opcode = opcodeTable[OpcodeKind.VarOp, VarForm(opByte)];
                ReadOperandKinds(ref address);
            }

            if (opcode.IsDoubleVariable)
            {
                ReadOperandKinds(ref address, 4);
            }

            ReadOperandValues(ref address);

            if (opcode.HasStoreVariable)
            {
                store = ReadVariable(ref address);
            }

            if (opcode.HasBranch)
            {
                branch = ReadBranch(ref address);
            }

            if (opcode.HasZText)
            {
                zwords = ReadZWords(ref address);
            }

            pc += address - startAddress;
        }
    }
}
