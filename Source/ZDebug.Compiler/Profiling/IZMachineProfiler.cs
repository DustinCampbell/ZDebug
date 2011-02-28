namespace ZDebug.Compiler.Profiling
{
    public interface IZMachineProfiler
    {
        void RoutineCompiled(RoutineCompilationStatistics statistics);

        void EnterRoutine(int address);
        void ExitRoutine(int address);

        void ExecutingInstruction(int address);
        void ExecutedInstruction(int address);

        void Quit();
        void Interrupt();
    }
}
