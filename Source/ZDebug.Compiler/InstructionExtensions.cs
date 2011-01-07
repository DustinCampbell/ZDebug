using ZDebug.Core.Instructions;

namespace ZDebug.Compiler
{
    internal static class InstructionExtensions
    {
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
