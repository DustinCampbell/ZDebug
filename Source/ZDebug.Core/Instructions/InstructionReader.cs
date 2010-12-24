using ZDebug.Core.Basics;
using ZDebug.Core.Collections;

namespace ZDebug.Core.Instructions
{
    public sealed class InstructionReader
    {
        private const byte opKind_LargeConstant = 0;
        private const byte opKind_SmallConstant = 1;
        private const byte opKind_Variable = 2;
        private const byte opKind_Omitted = 3;

        private int address;
        private readonly Memory memory;
        private readonly OpcodeTable opcodeTable;
        private readonly InstructionCache cache;

        // This is used in instruction parsing
        private byte[] operandKinds = new byte[8];

        public InstructionReader(int address, Memory memory, InstructionCache cache = null)
        {
            this.memory = memory;
            this.address = address;
            this.opcodeTable = OpcodeTables.GetOpcodeTable(memory.ReadVersion());
            this.cache = cache ?? new InstructionCache();
        }

        private void ReadOperandKinds(int offset = 0)
        {
            var b = memory.ReadByte(ref address);

            operandKinds[offset] = (byte)((b & 0xc0) >> 6);
            operandKinds[offset + 1] = (byte)((b & 0x30) >> 4);
            operandKinds[offset + 2] = (byte)((b & 0x0c) >> 2);
            operandKinds[offset + 3] = (byte)(b & 0x03);
        }

        private Operand ReadOperand(byte kind)
        {
            switch (kind)
            {
                case opKind_LargeConstant:
                    return new Operand((OperandKind)kind, memory.ReadWord(ref address));

                case opKind_SmallConstant:
                case opKind_Variable:
                    return new Operand((OperandKind)kind, memory.ReadByte(ref address));

                default:
                    throw new InstructionReaderException("Attempted to read ommitted operand.");
            }
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
            var startAddress = address;

            Instruction instruction;
            if (cache.TryGet(startAddress, out instruction))
            {
                address += instruction.Length;
                return instruction;
            }

            var opByte = memory.ReadByte(ref address);

            Opcode opcode;

            for (int i = 0; i < 8; i++)
            {
                operandKinds[i] = opKind_Omitted;
            }

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
                ReadOperandKinds();
            }
            else if (opByte >= 0xc0 && opByte <= 0xdf)
            {
                opcode = opcodeTable[OpcodeKind.TwoOp, VarForm(opByte)];
                ReadOperandKinds();
            }
            else // opByte >= 0xe0 && opByte <= 0xff
            {
                opcode = opcodeTable[OpcodeKind.VarOp, VarForm(opByte)];
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
                storeVariable = memory.ReadVariable(ref address);
            }

            Branch? branch = null;
            if (opcode.HasBranch)
            {
                branch = memory.ReadBranch(ref address);
            }

            ushort[] ztext = null;
            if (opcode.HasZText)
            {
                ztext = memory.ReadZWords(ref address);
            }

            var length = address - startAddress;

            instruction = new Instruction(startAddress, length, opcode, operands, operands.Length, storeVariable, branch, ztext);
            cache.Add(startAddress, instruction);
            return instruction;
        }

        public int Address
        {
            get { return address; }
            set { address = value; }
        }
    }
}
