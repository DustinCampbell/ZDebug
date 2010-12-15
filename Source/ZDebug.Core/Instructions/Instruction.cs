using System.Collections.ObjectModel;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Instructions
{
    public sealed class Instruction
    {
        public readonly int Address;
        public readonly int Length;
        public readonly Opcode Opcode;
        public readonly Operand[] Operands;
        public readonly bool HasStoreVariable;
        public readonly Variable StoreVariable;
        public readonly bool HasBranch;
        public readonly Branch Branch;
        public readonly bool HasZText;
        public readonly ReadOnlyCollection<ushort> ZText;

        internal Instruction(
            int address,
            int length,
            Opcode opcode,
            Operand[] operands,
            Variable storeVariable = null,
            Branch? branch = null,
            ushort[] ztext = null)
        {
            this.Address = address;
            this.Length = length;
            this.Opcode = opcode;
            this.Operands = operands;

            if (storeVariable != null)
            {
                this.StoreVariable = storeVariable;
                this.HasStoreVariable = true;
            }
            else
            {
                this.StoreVariable = null;
                this.HasStoreVariable = false;
            }

            if (branch != null)
            {
                this.Branch = branch.Value;
                this.HasBranch = true;
            }
            else
            {
                this.Branch = default(Branch);
                this.HasBranch = false;
            }

            if (ztext != null)
            {
                this.ZText = ztext.AsReadOnly();
                this.HasZText = true;
            }
            else
            {
                this.ZText = null;
                this.HasZText = false;
            }
        }
    }
}
