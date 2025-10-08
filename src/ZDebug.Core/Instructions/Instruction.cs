using ZDebug.Core.Collections;

namespace ZDebug.Core.Instructions;

public sealed class Instruction
{
    public readonly int Address;
    public readonly int Length;
    public readonly Opcode Opcode;
    public readonly ReadOnlyArray<Operand> Operands;
    public readonly int OperandCount;
    public readonly bool HasStoreVariable;
    public readonly Variable StoreVariable;
    public readonly bool HasBranch;
    public readonly Branch Branch;
    public readonly bool HasZText;
    public readonly ushort[] ZText;

    internal Instruction(
        int address,
        int length,
        Opcode opcode,
        ReadOnlyArray<Operand> operands,
        int operandCount,
        Variable storeVariable = null,
        Branch? branch = null,
        ushort[] ztext = null)
    {
        Address = address;
        Length = length;
        Opcode = opcode;
        Operands = operands;
        OperandCount = operandCount;

        if (storeVariable != null)
        {
            StoreVariable = storeVariable;
            HasStoreVariable = true;
        }
        else
        {
            StoreVariable = null;
            HasStoreVariable = false;
        }

        if (branch != null)
        {
            Branch = branch.Value;
            HasBranch = true;
        }
        else
        {
            Branch = default(Branch);
            HasBranch = false;
        }

        if (ztext != null)
        {
            ZText = ztext;
            HasZText = true;
        }
        else
        {
            ZText = null;
            HasZText = false;
        }
    }

    public override string ToString() => string.Format("{0:x4}: {1}", Address, Opcode.Name);
}
