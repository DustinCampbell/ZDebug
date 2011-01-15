using ZDebug.Core.Instructions;

namespace ZDebug.Compiler
{
    internal static class InstructionExtensions
    {
        private static bool Is(this Opcode op, OpcodeKind kind, int number)
        {
            return op.Kind == kind && op.Number == number;
        }

        public static bool UsesScreen(this Instruction i)
        {
            var op = i.Opcode;

            return op.Is(OpcodeKind.ZeroOp, 0x02)  // print
                || op.Is(OpcodeKind.ZeroOp, 0x0b); // new_line
        }

        public static bool UsesStack(this Instruction i)
        {
            if (i.HasStoreVariable && i.StoreVariable.Kind == VariableKind.Stack)
            {
                return true;
            }

            if (i.Opcode.IsFirstOpByRef && i.Operands[0].Value == 0)
            {
                return true;
            }

            foreach (var op in i.Operands)
            {
                if (op.Kind == OperandKind.Variable && op.Value == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
