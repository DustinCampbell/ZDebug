namespace ZDebug.Core.Instructions;

public readonly struct Branch(bool condition, short offset, int endAddress)
{
    public readonly bool Condition = condition;
    public readonly short Offset = offset;

    private readonly int _endAddress = endAddress;

    public BranchKind Kind => Offset switch
    {
        0 => BranchKind.RFalse,
        1 => BranchKind.RTrue,
        _ => BranchKind.Address,
    };

    public int TargetAddress => _endAddress + Offset - 2;
}
