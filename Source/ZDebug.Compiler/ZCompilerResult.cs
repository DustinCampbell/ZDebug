using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Compiler.Profiling;

namespace ZDebug.Compiler
{
    public sealed class ZCompilerResult
    {
        private readonly ZRoutine routine;
        private readonly ZRoutineCode code;
        private readonly RoutineCompilationStatistics statistics;

        public ZCompilerResult(ZRoutine routine, ZRoutineCode code, RoutineCompilationStatistics statistics)
        {
            this.routine = routine;
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
