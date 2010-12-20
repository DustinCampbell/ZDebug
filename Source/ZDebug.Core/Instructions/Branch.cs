namespace ZDebug.Core.Instructions
{
    public struct Branch
    {
        public readonly BranchKind Kind;
        public readonly bool Condition;
        public readonly short Offset;

        public Branch(bool condition, short offset)
        {
            this.Condition = condition;
            this.Offset = offset;

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
    }
}
