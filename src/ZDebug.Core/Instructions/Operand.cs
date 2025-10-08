
namespace ZDebug.Core.Instructions;

public readonly struct Operand
{
    public readonly OperandKind Kind;
    public readonly ushort Value;

    public Operand(OperandKind kind, ushort value)
    {
        Kind = kind;
        Value = value;
    }

    public bool IsConstant
    {
        get => Kind == OperandKind.LargeConstant
              || Kind == OperandKind.SmallConstant;
    }

    public bool IsVariable => Kind == OperandKind.Variable;

    public bool IsStackVariable
    {
        get => Kind == OperandKind.Variable
              && Value == 0;
    }

    public bool IsLocalVariable
    {
        get => Kind == OperandKind.Variable
              && Value >= 1 && Value <= 15;
    }

    public bool IsGlobalVariable
    {
        get => Kind == OperandKind.Variable
              && Value >= 16;
    }

    public override string ToString()
    {
        switch (Kind)
        {
            case OperandKind.LargeConstant:
                return "#" + Value.ToString("x4");
            case OperandKind.SmallConstant:
                return "#" + Value.ToString("x2");
            default: // OperandKind.Variable
                return Variable.FromByte((byte)Value).ToString();
        }
    }
}
