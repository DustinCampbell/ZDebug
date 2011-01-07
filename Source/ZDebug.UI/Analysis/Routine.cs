using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZDebug.Core.Basics;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Instructions
{
    public sealed class Routine
    {
        private readonly int address;
        private readonly ReadOnlyCollection<ushort> locals;
        private readonly ReadOnlyCollection<Instruction> instructions;
        private string name;

        private Routine(int address, ReadOnlyCollection<ushort> locals, ReadOnlyCollection<Instruction> instructions, string name)
        {
            this.address = address;
            this.locals = locals;
            this.instructions = instructions;
            this.name = name ?? string.Empty;
        }

        public int Address
        {
            get { return address; }
        }

        public int Length
        {
            get
            {
                if (instructions.Count > 0)
                {
                    var last = instructions.Last();
                    return last.Address + last.Length + 1 - address;
                }
                else
                {
                    return 0;
                }
            }
        }

        public ReadOnlyCollection<ushort> Locals
        {
            get { return locals; }
        }

        public ReadOnlyCollection<Instruction> Instructions
        {
            get { return instructions; }
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

        internal static Routine Parse(int address, Story story, InstructionCache cache, string name = null)
        {
            var routineAddress = address;

            // read locals
            var localCount = story.Memory.ReadByte(address++);
            var locals = new ushort[localCount];
            if (story.Version <= 4)
            {
                locals = story.Memory.ReadWords(address, localCount);
                address += localCount * 2;
            }

            var reader = new InstructionReader(address, story.Memory.Bytes, cache);

            var instructions = new List<Instruction>();
            var lastAddressKnown = address;

            while (true)
            {
                var i = reader.NextInstruction();

                instructions.Add(i);

                if ((i.Opcode.IsReturn || i.Opcode.IsQuit) && reader.Address > lastAddressKnown)
                {
                    break;
                }
                else if (i.Opcode.IsJump)
                {
                    var jumpOffset = (short)i.Operands[0].Value;
                    var jumpAddress = reader.Address + jumpOffset - 2;
                    if (jumpAddress > lastAddressKnown)
                    {
                        lastAddressKnown = jumpAddress;
                    }

                    if (reader.Address > lastAddressKnown)
                    {
                        break;
                    }
                }
                else if (i.HasBranch && i.Branch.Kind == BranchKind.Address)
                {
                    var branchAddress = reader.Address + i.Branch.Offset - 2;
                    if (branchAddress > lastAddressKnown)
                    {
                        lastAddressKnown = branchAddress;
                    }
                }
            }

            return new Routine(routineAddress, locals.AsReadOnly(), instructions.AsReadOnly(), name);
        }
    }
}
