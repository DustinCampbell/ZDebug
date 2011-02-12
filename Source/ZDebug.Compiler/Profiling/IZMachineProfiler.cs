using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.Compiler.Profiling
{
    public interface IZMachineProfiler
    {
        void RoutineCompiled(RoutineStatistics statistics);
    }
}
