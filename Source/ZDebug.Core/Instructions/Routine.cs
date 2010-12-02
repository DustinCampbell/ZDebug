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
        private readonly ReadOnlyCollection<Value> locals;
        private readonly ReadOnlyCollection<Instruction> instructions;

        private Routine(int address, ReadOnlyCollection<Value> locals, ReadOnlyCollection<Instruction> instructions)
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

        public ReadOnlyCollection<Value> Locals
        {
            get { return locals; }
        }

        public ReadOnlyCollection<Instruction> Instructions
        {
            get { return instructions; }
        }

        public static Routine Parse(IMemoryReader reader, byte version)
        {
            var address = reader.Address;
            var localCount = reader.NextByte();
            var locals = version < 5
                ? ArrayEx.Create(localCount, _ => new Value(ValueKind.Number, reader.NextWord()))
                : ArrayEx.Create(localCount, _ => new Value(ValueKind.Number, 0));

            var ireader = reader.AsInstructionReader(version);

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
                    var jumpOffset = (short)i.Operands[0].RawValue;
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
