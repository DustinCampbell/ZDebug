using System.Reflection.Emit;
using ZDebug.Core.Instructions;
using ZDebug.Core.Text;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private void op_new_line(Instruction i)
        {
            PrintChar('\n');
        }

        private void op_print(Instruction i)
        {
            var text = machine.ConvertEmbeddedZText(i.ZText);
            PrintText(text);
        }

        private void op_quit(Instruction i)
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
