using ZDebug.Core.Utilities;

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
            Profiler_Quit();
            il.ThrowException<ZMachineQuitException>();
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
            var verify = Reflection<CompiledZMachine>.GetMethod("Verify", @public: false);

            il.LoadArg(0);
            il.Call(verify);

            Branch();
        }

        private void op_piracy()
        {
            il.Load(1);
            Branch();
        }
    }
}
