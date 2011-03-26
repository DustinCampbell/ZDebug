using ZDebug.Core.Routines;

namespace ZDebug.Compiler
{
    public sealed class ZRoutineCall
    {
        private readonly CompiledZMachine machine;
        private readonly ZRoutine routine;
        private ZCompilerResult compilationResult;

        public ZRoutineCall(ZRoutine routine, CompiledZMachine machine)
        {
            this.machine = machine;
            this.routine = routine;
        }

        public void Compile()
        {
            if (compilationResult == null)
            {
                compilationResult = machine.Compile(routine);
            }
        }

        public ushort Invoke()
        {
            Compile();

            return compilationResult.Code(compilationResult.Calls);
        }

        /// <summary>
        /// The Z-machine routine that was compiled.
        /// </summary>
        public ZRoutine Routine
        {
            get { return routine; }
        }
    }
}
