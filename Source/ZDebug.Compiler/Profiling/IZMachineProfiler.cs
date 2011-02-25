using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.Compiler.Profiling
{
    public interface IZMachineProfiler
    {
        void RoutineCompiled(RoutineCompilationStatistics statistics);

        void EnterRoutine(int address);
        void ExitRoutine(int address);

        void MarkInstruction(int address);
    }
}
