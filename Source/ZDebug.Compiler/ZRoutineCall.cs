using ZDebug.Core.Routines;

namespace ZDebug.Compiler
{
    public sealed class ZRoutineCall
    {
        public readonly CompiledZMachine Machine;
        public readonly ZRoutine Routine;
        private ZCompilerResult compilationResult;

        public ZRoutineCall(ZRoutine routine, CompiledZMachine machine)
        {
            this.Machine = machine;
            this.Routine = routine;
        }

        public void Compile()
        {
            if (compilationResult == null)
            {
                compilationResult = Machine.Compile(Routine);
            }
        }

        public ushort Invoke()
        {
            Compile();

            return compilationResult.Code(compilationResult.Calls);
        }
    }
}
