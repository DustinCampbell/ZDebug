using System;
using ZDebug.Core.Basics;
using ZDebug.Core.Collections;

namespace ZDebug.Core.Instructions
{
    internal sealed class InstructionReader
    {
        private const byte opKind_LargeConstant = 0;
        private const byte opKind_SmallConstant = 1;
        private const byte opKind_Variable = 2;
        private const byte opKind_Omitted = 3;

        private readonly MemoryReader reader;
        private readonly byte version;
        private readonly InstructionCache cache;

        // This is used in instruction parsing
        private byte[] operandKinds = new byte[8];

        internal InstructionReader(MemoryReader reader, byte version, InstructionCache cache)
        {
            this.reader = reader;
            this.version = version;
            this.cache = cache != null ? cache : new InstructionCache();
        }

        private void ReadOperandKinds(int offset = 0)
        {
            var b = reader.NextByte();

            operandKinds[offset] = (byte)((b & 0xc0) >> 6);
            operandKinds[offset + 1] = (byte)((b & 0x30) >> 4);
            operandKinds[offset + 2] = (byte)((b & 0x0c) >> 2);
            operandKinds[offset + 3] = (byte)(b & 0x03);
        }

        private Operand ReadOperand(byte kind)
        {
            ushort rawValue;
            if (kind == opKind_LargeConstant)
            {
                rawValue = reader.NextWord();
            }
            else if (kind == opKind_SmallConstant)
            {
                rawValue = reader.NextByte();
            }
            else if (kind == opKind_Variable)
            {
                rawValue = reader.NextByte();
            }
            else // omitted
            {
                throw new InvalidOperationException("Attempted to read omitted operand.");
            }

            return new Operand((OperandKind)kind, rawValue);
        }

        private ReadOnlyArray<Operand> ReadOperands()
        {
            int size = 8;
            for (int i = 0; i < operandKinds.Length; i++)
            {
                if (operandKinds[i] == opKind_Omitted)
                {
                    size = i;
                    break;
                }
            }

            var result = cache.AllocateOperands(size);

            for (int i = 0; i < size; i++)
            {
                result[i] = ReadOperand(operandKinds[i]);
            }

            return result;
        }

        private Variable ReadStoreVariable()
        {
            return Variable.FromByte(reader.NextByte());
        }

        private Branch ReadBranch()
        {
            var b1 = reader.NextByte();

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
                var b2 = reader.NextByte();
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

        private ushort[] ReadZText()
        {
            return reader.NextZWords();
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

        private Opcode GetOpcode(OpcodeKind kind, byte number)
        {
            return OpcodeTable.GetOpcode(kind, number, version);
        }

        public Instruction NextInstruction()
        {
            var address = reader.Address;

            Instruction instruction;
            if (cache.TryGet(address, out instruction))
            {
                reader.Address += instruction.Length;
                return instruction;
            }

            var opByte = reader.NextByte();

            Opcode opcode;

            for (int i = 0; i < 8; i++)
            {
                operandKinds[i] = opKind_Omitted;
            }

            if (opByte >= 0x00 && opByte <= 0x1f)
            {
                opcode = GetOpcode(OpcodeKind.TwoOp, LongForm(opByte));
                operandKinds[0] = opKind_SmallConstant;
                operandKinds[1] = opKind_SmallConstant;
            }
            else if (opByte >= 0x20 && opByte <= 0x3f)
            {
                opcode = GetOpcode(OpcodeKind.TwoOp, LongForm(opByte));
                operandKinds[0] = opKind_SmallConstant;
                operandKinds[1] = opKind_Variable;
            }
            else if (opByte >= 0x40 && opByte <= 0x5f)
            {
                opcode = GetOpcode(OpcodeKind.TwoOp, LongForm(opByte));
                operandKinds[0] = opKind_Variable;
                operandKinds[1] = opKind_SmallConstant;
            }
            else if (opByte >= 0x60 && opByte <= 0x7f)
            {
                opcode = GetOpcode(OpcodeKind.TwoOp, LongForm(opByte));
                operandKinds[0] = opKind_Variable;
                operandKinds[1] = opKind_Variable;
            }
            else if (opByte >= 0x80 && opByte <= 0x8f)
            {
                opcode = GetOpcode(OpcodeKind.OneOp, ShortForm(opByte));
                operandKinds[0] = opKind_LargeConstant;
            }
            else if (opByte >= 0x90 && opByte <= 0x9f)
            {
                opcode = GetOpcode(OpcodeKind.OneOp, ShortForm(opByte));
                operandKinds[0] = opKind_SmallConstant;
            }
            else if (opByte >= 0xa0 && opByte <= 0xaf)
            {
                opcode = GetOpcode(OpcodeKind.OneOp, ShortForm(opByte));
                operandKinds[0] = opKind_Variable;
            }
            else if ((opByte >= 0xb0 && opByte <= 0xbd) || opByte == 0xbf)
            {
                opcode = GetOpcode(OpcodeKind.ZeroOp, ShortForm(opByte));
            }
            else if (opByte == 0xbe)
            {
                opcode = GetOpcode(OpcodeKind.Ext, reader.NextByte());
                ReadOperandKinds();
            }
            else if (opByte >= 0xc0 && opByte <= 0xdf)
            {
                opcode = GetOpcode(OpcodeKind.TwoOp, VarForm(opByte));
                ReadOperandKinds();
            }
            else // opByte >= 0xe0 && opByte <= 0xff
            {
                opcode = GetOpcode(OpcodeKind.VarOp, VarForm(opByte));
                ReadOperandKinds();
            }

            if (opcode.IsDoubleVariable)
            {
                ReadOperandKinds(4);
            }

            var operands = ReadOperands();

            Variable storeVariable = null;
            if (opcode.HasStoreVariable)
            {
                storeVariable = ReadStoreVariable();
            }

            Branch? branch = null;
            if (opcode.HasBranch)
            {
                branch = ReadBranch();
            }

            ushort[] ztext = null;
            if (opcode.HasZText)
            {
                ztext = ReadZText();
            }

            var length = reader.Address - address;

            instruction = new Instruction(address, length, opcode, operands, storeVariable, branch, ztext);
            cache.Add(address, instruction);
            return instruction;
        }
    }
}
