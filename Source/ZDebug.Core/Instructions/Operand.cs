
namespace ZDebug.Core.Instructions
{
    public struct Operand
    {
        public readonly OperandKind Kind;
        public readonly ushort RawValue;

        public Operand(OperandKind kind, ushort rawValue)
        {
            this.Kind = kind;
            this.RawValue = rawValue;
        }
    }
}
