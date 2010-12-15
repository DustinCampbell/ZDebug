
namespace ZDebug.Core.Instructions
{
    public sealed class Variable
    {
        public readonly VariableKind Kind;
        public readonly byte Index;

        public static readonly Variable Stack;
        private static readonly Variable[] locals;
        private static readonly Variable[] globals;

        static Variable()
        {
            Stack = new Variable(VariableKind.Stack, 0);

            locals = new Variable[15];
            for (byte i = 0; i < 15; i++)
            {
                locals[i] = new Variable(VariableKind.Local, i);
            }

            globals = new Variable[240];
            for (byte i = 0; i < 240; i++)
            {
                globals[i] = new Variable(VariableKind.Global, i);
            }
        }

        private Variable(VariableKind kind, byte index)
        {
            this.Kind = kind;
            this.Index = index;
        }

        public override string ToString()
        {
            switch (Kind)
            {
                case VariableKind.Stack:
                    return "SP";
                case VariableKind.Local:
                    return "L" + Index.ToString("x2");
                default: // VariableKind.Global:
                    return "G" + Index.ToString("x2");
            }
        }

        public static Variable FromByte(byte b)
        {
            if (b == 0x00)
            {
                return Variable.Stack;
            }
            else if (b >= 0x01 && b <= 0x0f)
            {
                return Variable.locals[b - 0x01];
            }
            else // b >= 0x10 && b <= 0xff
            {
                return Variable.globals[b - 0x10];
            }
        }
    }
}
