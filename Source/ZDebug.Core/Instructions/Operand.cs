
namespace ZDebug.Core.Instructions
{
    public struct Operand
    {
        public readonly OperandKind Kind;
        public readonly ushort Value;

        public Operand(OperandKind kind, ushort value)
        {
            this.Kind = kind;
            this.Value = value;
        }

        public bool IsConstant
        {
            get
            {
                return Kind == OperandKind.LargeConstant
                    || Kind == OperandKind.SmallConstant;
            }
        }

        public bool IsVariable
        {
            get
            {
                return Kind == OperandKind.Variable;
            }
        }

        public bool IsStackVariable
        {
            get
            {
                return Kind == OperandKind.Variable
                    && Value == 0;
            }
        }

        public bool IsLocalVariable
        {
            get
            {
                return Kind == OperandKind.Variable
                    && Value >= 1 && Value <= 15;
            }
        }

        public bool IsGlobalVariable
        {
            get
            {
                return Kind == OperandKind.Variable
                    && Value >= 16;
            }
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
}
