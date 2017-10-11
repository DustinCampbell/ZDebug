namespace ZDebug.Core.Instructions
{
    public struct Branch
    {
        public readonly BranchKind Kind;
        public readonly bool Condition;
        public readonly short Offset;

        private readonly int endAddress;

        public Branch(bool condition, short offset, int endAddress)
        {
            this.Condition = condition;
            this.Offset = offset;
            this.endAddress = endAddress;

            if (offset == 0)
            {
                this.Kind = BranchKind.RFalse;
            }
            else if (offset == 1)
            {
                this.Kind = BranchKind.RTrue;
            }
            else
            {
                this.Kind = BranchKind.Address;
            }
        }

        public int TargetAddress
        {
            get
            {
                return endAddress + Offset - 2;
            }
        }
    }
}
