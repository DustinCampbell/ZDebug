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
            var text = machine.ConvertZText(currentInstruction.ZText);
            PrintText(text);
        }

        private void op_print_ret()
        {
            var text = machine.ConvertZText(currentInstruction.ZText);
            PrintText(text);
            Return(1);
        }

        private void op_restart()
        {
            NotImplemented();
        }

        private void op_quit()
        {
            NotImplemented();
        }

        private void op_ret_popped()
        {
            PopStack();
            Return();
        }

        private void op_rfalse()
        {
            Return(0);
        }

        private void op_rtrue()
        {
            Return(1);
        }

        private void op_show_status()
        {
            ShowStatus();
        }

        private void op_verify()
        {
            NotImplemented();
        }

        private void op_piracy()
        {
            NotImplemented();
        }
    }
}
