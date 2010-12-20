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

        private Routine(int address, ReadOnlyCollection<ushort> locals, ReadOnlyCollection<Instruction> instructions)
        {
            this.address = address;
            this.locals = locals;
            this.instructions = instructions;
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

        internal static Routine Parse(MemoryReader reader, byte version, InstructionCache cache)
        {
            var address = reader.Address;

            // read locals
            var localCount = reader.NextByte();
            var locals = new ushort[localCount];
            if (version <= 4)
            {
                locals = reader.NextWords(localCount);
            }

            var ireader = new InstructionReader(reader, version, cache);

            var instructions = new List<Instruction>();
            var lastAddressKnown = reader.Address;

            while (true)
            {
                var i = ireader.NextInstruction();

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

            return new Routine(address, locals.AsReadOnly(), instructions.AsReadOnly());
        }
    }
}
