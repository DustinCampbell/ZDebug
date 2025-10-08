
namespace ZDebug.Core.Instructions;

public readonly struct Operand(OperandKind kind, ushort value)
{
    public readonly OperandKind Kind = kind;
    public readonly ushort Value = value;

    public bool IsConstant
        => Kind is OperandKind.LargeConstant or OperandKind.SmallConstant;

    public bool IsVariable
        => Kind is OperandKind.Variable;

    public bool IsStackVariable
        => Kind is OperandKind.Variable && Value is 0;

    public bool IsLocalVariable
        => Kind is OperandKind.Variable && Value is >= 1 and <= 15;

    public bool IsGlobalVariable
        => Kind is OperandKind.Variable && Value is >= 16;

    public override string ToString() => Kind switch
    {
        OperandKind.LargeConstant => $"#{Value:x4}",
        OperandKind.SmallConstant => $"#{Value:x2}",

        // OperandKind.Variable
        _ => Variable.FromByte((byte)Value).DisplayText,
    };
}
