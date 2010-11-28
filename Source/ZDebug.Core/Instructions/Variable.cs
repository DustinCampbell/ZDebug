using ZDebug.Core.Collections;

namespace ZDebug.Core.Instructions
{
    public sealed class Variable
    {
        private readonly VariableKind kind;
        private readonly byte index;

        public static readonly Variable Stack;
        public static readonly IIndexedEnumerable<Variable> Locals;
        public static readonly IIndexedEnumerable<Variable> Globals;

        static Variable()
        {
            Stack = new Variable(VariableKind.Stack, 0);

            var locals = new Variable[15];
            for (byte i = 0; i < 15; i++)
            {
                locals[i] = new Variable(VariableKind.Local, i);
            }

            Locals = locals.ToIndexedEnumerable();

            var globals = new Variable[240];
            for (byte i = 0; i < 240; i++)
            {
                locals[i] = new Variable(VariableKind.Global, i);
            }

            Globals = globals.ToIndexedEnumerable();
        }

        private Variable(VariableKind kind, byte index)
        {
            this.kind = kind;
            this.index = index;
        }

        public override string ToString()
        {
            switch (kind)
            {
                case VariableKind.Stack:
                    return "SP";
                case VariableKind.Local:
                    return "L" + index.ToString("x2");
                default: // VariableKind.Global:
                    return "G" + index.ToString("x2");
            }
        }

        public VariableKind Kind
        {
            get { return kind; }
        }

        public byte Index
        {
            get { return index; }
        }

        public static Variable FromByte(byte b)
        {
            if (b == 0x00)
            {
                return Variable.Stack;
            }
            else if (b >= 0x01 && b <= 0x0f)
            {
                return Variable.Locals[b - 0x01];
            }
            else // b >= 0x10 && b <= 0xff
            {
                return Variable.Globals[b - 0x10];
            }
        }
    }
}
