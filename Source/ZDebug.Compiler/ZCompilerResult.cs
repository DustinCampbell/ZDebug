using System;
using ZDebug.Compiler.Profiling;

namespace ZDebug.Compiler
{
    public sealed class ZCompilerResult
    {
        private readonly ZRoutine routine;
        private readonly ZRoutineCall[] calls;
        private readonly ZRoutineCode code;
        private readonly RoutineCompilationStatistics statistics;

        public ZCompilerResult(ZRoutine routine, ZRoutineCall[] calls, ZRoutineCode code, RoutineCompilationStatistics statistics)
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

            this.routine = routine;
            this.calls = calls;
            this.code = code;
            this.statistics = statistics;
        }

        /// <summary>
        /// The Z-machine routine that was compiled.
        /// </summary>
        public ZRoutine Routine
        {
            get { return routine; }
        }

        public ZRoutineCall[] Calls
        {
            get { return calls; }
        }

        /// <summary>
        /// A delegate of the compiled code.
        /// </summary>
        public ZRoutineCode Code
        {
            get { return code; }
        }

        /// <summary>
        /// Statistics about the routine that was compiled.
        /// </summary>
        public RoutineCompilationStatistics Statistics
        {
            get { return statistics; }
        }
    }
}
