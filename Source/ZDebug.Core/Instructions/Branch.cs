namespace ZDebug.Core.Instructions
{
    public struct Branch
    {
        private readonly bool condition;
        private readonly short offset;

        public Branch(bool condition, short offset)
        {
            this.condition = condition;
            this.offset = offset;
        }

        public BranchKind Kind
        {
            get
            {
                switch (offset)
                {
                    case 0:
                        return BranchKind.RFalse;
                    case 1:
                        return BranchKind.RTrue;
                    default:
                        return BranchKind.Address;
                }
            }
        }

        public bool Condition
        {
            get { return condition; }
        }

        public short Offset
        {
            get { return offset; }
        }
    }
}
