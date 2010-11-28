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
        private readonly ReadOnlyCollection<ushort> zwords;

        public Instruction(
            int address,
            int length,
            Opcode opcode,
            Operand[] operands,
            Variable storeVariable = null,
            Branch? branch = null,
            ushort[] zwords = null)
        {
            this.address = address;
            this.length = length;
            this.opcode = opcode;
            this.operands = operands.AsReadOnly();
            this.storeVariable = storeVariable;
            this.branch = branch;
            this.zwords = zwords != null ? zwords.AsReadOnly() : null;
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

        public bool HasZWords
        {
            get { return zwords != null; }
        }

        public ReadOnlyCollection<ushort> ZWords
        {
            get { return zwords; }
        }
    }
}
