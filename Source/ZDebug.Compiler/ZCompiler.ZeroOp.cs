using System.Reflection.Emit;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private void op_new_line(Instruction i)
        {
            il.ThrowException("'" + i.Opcode.Name + "' not implemented.");
        }

        private void op_print(Instruction i)
        {
            il.ThrowException("'" + i.Opcode.Name + "' not implemented.");
        }

        private void op_rfalse(Instruction i)
        {
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);
        }

        private void op_rtrue(Instruction i)
        {
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);
        }
    }
}
