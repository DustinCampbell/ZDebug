namespace ZDebug.Core.Instructions;

public readonly struct Branch
{
    public readonly BranchKind Kind;
    public readonly bool Condition;
    public readonly short Offset;

    private readonly int endAddress;

    public Branch(bool condition, short offset, int endAddress)
    {
        Condition = condition;
        Offset = offset;
        this.endAddress = endAddress;

        if (offset == 0)
        {
            Kind = BranchKind.RFalse;
        }
        else if (offset == 1)
        {
            Kind = BranchKind.RTrue;
        }
        else
        {
            Kind = BranchKind.Address;
        }
    }

    public int TargetAddress => endAddress + Offset - 2;
}
