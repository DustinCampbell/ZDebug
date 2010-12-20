
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
    }
}
