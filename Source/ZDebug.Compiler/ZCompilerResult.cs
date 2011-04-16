using System;
using ZDebug.Compiler.Profiling;
using ZDebug.Core.Routines;

namespace ZDebug.Compiler
{
    internal sealed class ZCompilerResult
    {
        public readonly ZRoutine Routine;
        public readonly RoutineCompilationStatistics Statistics;

        internal readonly ZRoutineCode Code;
        internal readonly ZRoutineCall[] Calls;

        internal ZCompilerResult(ZRoutine routine, ZRoutineCall[] calls, ZRoutineCode code, RoutineCompilationStatistics statistics)
        {
            if (routine == null)
            {
                throw new ArgumentNullException("routine");
            }

            if (calls == null)
            {
                throw new ArgumentNullException("calls");
            }

            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            if (statistics == null)
            {
                throw new ArgumentNullException("statistics");
            }

            this.Routine = routine;
            this.Calls = calls;
            this.Code = code;
            this.Statistics = statistics;
        }
    }
}
