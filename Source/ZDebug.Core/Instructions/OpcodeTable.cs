namespace ZDebug.Core.Instructions
{
    internal sealed class OpcodeTable
    {
        private readonly byte version;
        private readonly Opcode[][] opcodes;

        public OpcodeTable(byte version)
        {
            this.version = version;

            this.opcodes = new Opcode[5][];
            for (int i = 0; i < 5; i++)
            {
                this.opcodes[i] = new Opcode[32];
            }
        }

        public void Add(
            OpcodeKind kind,
            byte number,
            string name,
            OpcodeFlags flags = OpcodeFlags.None)
        {
            opcodes[(int)kind][number] = new Opcode(kind, number, name, flags);
        }

        public Opcode this[OpcodeKind kind, byte number]
        {
            get
            {
                var opcode = opcodes[(int)kind][number];

                if (opcode == null)
                {
                    throw new OpcodeException(
                        string.Format("Could not find opcode (Kind = {0}, Number = {1:x2}, Version = {2})", kind, number, version));
                }

                return opcode;
            }
        }

        public byte Version
        {
            get { return version; }
        }
    }
}
