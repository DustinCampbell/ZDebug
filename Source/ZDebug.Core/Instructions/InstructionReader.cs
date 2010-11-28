using ZDebug.Core.Basics;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Instructions
{
    public sealed class InstructionReader
    {
        private readonly IMemoryReader reader;
        private readonly byte version;

        public InstructionReader(IMemoryReader reader, byte version)
        {
            this.reader = reader;
            this.version = version;
        }

        private OperandKind[] ReadOperandKinds()
        {
            var b = reader.NextByte();

            var kinds = new byte[]
            { 
                (byte)((b & 0xc0) >> 6),
                (byte)((b & 0x30) >> 4),
                (byte)((b & 0x0c) >> 2),
                (byte)(b & 0x03)
            };

            // Search for the first 'omitted' operand (kind == 3).
            int size = -1;
            for (int i = 0; i < 4; i++)
            {
                if (kinds[i] == 3)
                {
                    size = i;
                    break;
                }
            }

            if (size < 0)
            {
                size = 4;
            }

            var result = new OperandKind[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = (OperandKind)kinds[i];
            }

            return result;
        }

        private Operand ReadOperand(OperandKind kind)
        {
            ushort value;
            if (kind == OperandKind.LargeConstant)
            {
                value = reader.NextWord();
            }
            else // OperandKind.SmallConstant || OperandKind.Variable
            {
                value = reader.NextByte();
            }

            return new Operand(kind, new Value(ValueKind.Number, value));
        }

        private Operand[] ReadOperands(OperandKind[] kinds)
        {
            var result = new Operand[kinds.Length];

            for (int i = 0; i < kinds.Length; i++)
            {
                result[i] = ReadOperand(kinds[i]);
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

            var opByte = reader.NextByte();

            Opcode opcode;
            OperandKind[] operandKinds;

            if (opByte >= 0x00 && opByte <= 0x1f)
            {
                opcode = GetOpcode(OpcodeKind.TwoOp, LongForm(opByte));
                operandKinds = new OperandKind[] { OperandKind.SmallConstant, OperandKind.SmallConstant };
            }
            else if (opByte >= 0x20 && opByte <= 0x3f)
            {
                opcode = GetOpcode(OpcodeKind.TwoOp, LongForm(opByte));
                operandKinds = new OperandKind[] { OperandKind.SmallConstant, OperandKind.Variable };
            }
            else if (opByte >= 0x40 && opByte <= 0x5f)
            {
                opcode = GetOpcode(OpcodeKind.TwoOp, LongForm(opByte));
                operandKinds = new OperandKind[] { OperandKind.Variable, OperandKind.SmallConstant };
            }
            else if (opByte >= 0x60 && opByte <= 0x7f)
            {
                opcode = GetOpcode(OpcodeKind.TwoOp, LongForm(opByte));
                operandKinds = new OperandKind[] { OperandKind.Variable, OperandKind.Variable };
            }
            else if (opByte >= 0x80 && opByte <= 0x8f)
            {
                opcode = GetOpcode(OpcodeKind.OneOp, ShortForm(opByte));
                operandKinds = new OperandKind[] { OperandKind.LargeConstant };
            }
            else if (opByte >= 0x90 && opByte <= 0x9f)
            {
                opcode = GetOpcode(OpcodeKind.OneOp, ShortForm(opByte));
                operandKinds = new OperandKind[] { OperandKind.SmallConstant };
            }
            else if (opByte >= 0xa0 && opByte <= 0xaf)
            {
                opcode = GetOpcode(OpcodeKind.OneOp, ShortForm(opByte));
                operandKinds = new OperandKind[] { OperandKind.Variable };
            }
            else if ((opByte >= 0xb0 && opByte <= 0xbd) || opByte == 0xbf)
            {
                opcode = GetOpcode(OpcodeKind.ZeroOp, ShortForm(opByte));
                operandKinds = new OperandKind[] { };
            }
            else if (opByte == 0xbe)
            {
                opcode = GetOpcode(OpcodeKind.Ext, reader.NextByte());
                operandKinds = ReadOperandKinds();
            }
            else if (opByte >= 0xc0 && opByte <= 0xdf)
            {
                opcode = GetOpcode(OpcodeKind.TwoOp, VarForm(opByte));
                operandKinds = ReadOperandKinds();
            }
            else // opByte >= 0xe0 && opByte <= 0xff
            {
                opcode = GetOpcode(OpcodeKind.VarOp, VarForm(opByte));
                operandKinds = ReadOperandKinds();
            }

            if (opcode.IsDoubleVariable)
            {
                operandKinds = operandKinds.Concat(ReadOperandKinds());
            }

            var operands = ReadOperands(operandKinds);

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

            return new Instruction(address, length, opcode, operands, storeVariable, branch, ztext);
        }
    }
}
