namespace ZDebug.Core.Instructions
{
    public class Opcode
    {
        private readonly OpcodeKind kind;
        private readonly int number;
        private readonly string name;
        private readonly OpcodeFlags flags;

        public Opcode(OpcodeKind kind, int number, string name, OpcodeFlags flags)
        {
            this.kind = kind;
            this.number = number;
            this.name = name;
            this.flags = flags;
        }

        public OpcodeKind Kind
        {
            get { return kind; }
        }

        public int Number
        {
            get { return number; }
        }

        public string Name
        {
            get { return name; }
        }

        public OpcodeFlags Flags
        {
            get { return flags; }
        }
    }
}
