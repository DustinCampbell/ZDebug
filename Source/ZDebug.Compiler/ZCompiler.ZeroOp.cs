using System.Reflection.Emit;
using ZDebug.Core.Instructions;
using ZDebug.Core.Text;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private void op_new_line()
        {
            PrintChar('\n');
        }

        private void op_print()
        {
            var text = machine.ConvertEmbeddedZText(currentInstruction.ZText);
            PrintText(text);
        }

        private void op_quit()
        {
            NotImplemented();
        }

        private void op_rfalse()
        {
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);
        }

        private void op_rtrue()
        {
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);
        }
    }
}
