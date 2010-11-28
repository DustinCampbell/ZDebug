namespace ZDebug.Core.Instructions
{
    public struct Operand
    {
        private readonly OperandKind kind;
        private readonly Value value;

        public Operand(OperandKind kind, Value value)
        {
            this.kind = kind;
            this.value = value;
        }

        public OperandKind Kind
        {
            get { return kind; }
        }

        public Value Value
        {
            get { return value; }
        }
    }
}
