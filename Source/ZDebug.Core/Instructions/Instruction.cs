using System.Collections.ObjectModel;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Instructions
{
    public sealed class Instruction
    {
        private readonly int address;
        private readonly int length;
        private readonly Opcode opcode;
        private readonly ReadOnlyCollection<Operand> operands;
        private readonly Variable storeVariable;
        private readonly Branch? branch;
        private readonly ReadOnlyCollection<ushort> ztext;

        public Instruction(
            int address,
            int length,
            Opcode opcode,
            Operand[] operands,
            Variable storeVariable = null,
            Branch? branch = null,
            ushort[] ztext = null)
        {
            this.address = address;
            this.length = length;
            this.opcode = opcode;
            this.operands = operands.AsReadOnly();
            this.storeVariable = storeVariable;
            this.branch = branch;
            this.ztext = ztext != null ? ztext.AsReadOnly() : null;
        }

        public int Address
        {
            get { return address; }
        }

        public int Length
        {
            get { return length; }
        }

        public Opcode Opcode
        {
            get { return opcode; }
        }

        public ReadOnlyCollection<Operand> Operands
        {
            get { return operands; }
        }

        public bool HasStoreVariable
        {
            get { return storeVariable != null; }
        }

        public Variable StoreVariable
        {
            get { return storeVariable; }
        }

        public bool HasBranch
        {
            get { return branch.HasValue; }
        }

        public Branch Branch
        {
            get { return branch.Value; }
        }

        public bool HasZText
        {
            get { return ztext != null; }
        }

        public ReadOnlyCollection<ushort> ZText
        {
            get { return ztext; }
        }
    }
}
