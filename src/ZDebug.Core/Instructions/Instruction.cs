using System;

namespace ZDebug.Core.Instructions;

public sealed class Instruction
{
    private readonly ReadOnlyMemory<Operand> _operands;

    public readonly int Address;
    public readonly int Length;
    public readonly Opcode Opcode;
    public readonly bool HasStoreVariable;
    public readonly Variable StoreVariable;
    public readonly bool HasBranch;
    public readonly Branch Branch;
    public readonly bool HasZText;
    public readonly ushort[] ZText;

    public ReadOnlySpan<Operand> Operands => _operands.Span;

    internal Instruction(
        int address,
        int length,
        Opcode opcode,
        ReadOnlyMemory<Operand> operands,
        Variable storeVariable = null,
        Branch? branch = null,
        ushort[] ztext = null)
    {
        this.Address = address;
        this.Length = length;
        this.Opcode = opcode;
        _operands = operands;

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
            this.Branch = default;
            this.HasBranch = false;
        }

        if (ztext != null)
        {
            this.ZText = ztext;
            this.HasZText = true;
        }
        else
        {
            this.ZText = null;
            this.HasZText = false;
        }
    }

    public override string ToString()
    {
        return string.Format("{0:x4}: {1}", Address, Opcode.Name);
    }
}
