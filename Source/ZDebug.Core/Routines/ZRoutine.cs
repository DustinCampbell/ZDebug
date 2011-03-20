using System.Collections.Generic;
using ZDebug.Core.Collections;
using ZDebug.Core.Extensions;
using ZDebug.Core.Instructions;

namespace ZDebug.Core.Routines
{
    public sealed class ZRoutine
    {
        private readonly int address;
        private readonly int length;
        private readonly ReadOnlyArray<Instruction> instructions;
        private readonly ReadOnlyArray<ushort> locals;
        private string name;

        private ZRoutine(int address, Instruction[] instructions, ushort[] locals, string name)
        {
            this.address = address;
            this.instructions = new ReadOnlyArray<Instruction>(instructions);
            this.locals = new ReadOnlyArray<ushort>(locals);

            if (instructions.Length > 0)
            {
                var last = instructions[instructions.Length - 1];
                this.length = last.Address + last.Length - address + 1;
            }
            else
            {
                this.length = 0;
            }

            this.name = name ?? string.Empty;
        }

        public int Address
        {
            get { return address; }
        }

        public int Length
        {
            get { return length; }
        }

        public ReadOnlyArray<Instruction> Instructions
        {
            get { return instructions; }
        }

        public ReadOnlyArray<ushort> Locals
        {
            get { return locals; }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value ?? string.Empty;
            }
        }

        private static ushort[] ReadLocals(ref int address, byte[] memory, byte version)
        {
            var localCount = memory.ReadByte(ref address);
            if (version <= 4)
            {
                var locals = memory.ReadWords(ref address, localCount);
                return locals;
            }
            else
            {
                return new ushort[localCount];
            }
        }

        private static Instruction[] ReadInstructions(int address, byte[] memory, InstructionCache cache)
        {
            var reader = new InstructionReader(address, memory, cache);
            var instructions = new List<Instruction>();
            var lastKnownAddress = address;

            while (true)
            {
                var i = reader.NextInstruction();

                instructions.Add(i);

                if ((i.Opcode.IsReturn || i.Opcode.IsQuit) && reader.Address > lastKnownAddress)
                {
                    break;
                }
                else if (i.Opcode.IsJump)
                {
                    var jumpOffset = (short)i.Operands[0].Value;
                    var jumpAddress = reader.Address + jumpOffset - 2;
                    if (jumpAddress > lastKnownAddress)
                    {
                        lastKnownAddress = jumpAddress;
                    }

                    if (reader.Address > lastKnownAddress)
                    {
                        break;
                    }
                }
                else if (i.HasBranch && i.Branch.Kind == BranchKind.Address)
                {
                    var branchAddress = reader.Address + i.Branch.Offset - 2;
                    if (branchAddress > lastKnownAddress)
                    {
                        lastKnownAddress = branchAddress;
                    }
                }
            }

            return instructions.ToArray();
        }

        public static ZRoutine Create(int address, byte[] memory, InstructionCache cache = null, string name = null)
        {
            var version = memory.ReadByte(0);

            var startAddress = address;
            var locals = ReadLocals(ref address, memory, version);
            var instructions = ReadInstructions(address, memory, cache);

            return new ZRoutine(startAddress, instructions, locals, name);
        }
    }
}
