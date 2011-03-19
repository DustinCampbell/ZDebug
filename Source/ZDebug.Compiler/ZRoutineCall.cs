using ZDebug.Core.Routines;
namespace ZDebug.Compiler
{
    public sealed class ZRoutineCall
    {
        private readonly ZMachine machine;
        private readonly ZRoutine routine;
        private ZCompilerResult compilationResult;

        public ZRoutineCall(ZRoutine routine, ZMachine machine)
        {
            this.machine = machine;
            this.routine = routine;
        }

        public ushort Invoke()
        {
            if (compilationResult == null)
            {
                compilationResult = machine.Compile(routine);
            }

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
