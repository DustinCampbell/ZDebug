using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.Compiler.Profiling
{
    public sealed class RoutineCompilationStatistics
    {
        private readonly ZRoutine routine;
        private readonly int opcodeCount;
        private readonly int localCount;
        private readonly int size;
        private readonly TimeSpan compileTime;

        public RoutineCompilationStatistics(ZRoutine routine, int opcodeCount, int localCount, int size, TimeSpan compileTime)
        {
            this.routine = routine;
            this.opcodeCount = opcodeCount;
            this.localCount = localCount;
            this.size = size;
            this.compileTime = compileTime;
        }

        /// <summary>
        /// The Z-machine routine that was compiled.
        /// </summary>
        public ZRoutine Routine
        {
            get { return routine; }
        }

        /// <summary>
        /// The number of IL opcodes generated.
        /// </summary>
        public int OpcodeCount
        {
            get { return opcodeCount; }
        }

        /// <summary>
        /// The number of IL locals generated.
        /// </summary>
        public int LocalCount
        {
            get { return localCount; }
        }

        /// <summary>
        /// The size of the IL generated.
        /// </summary>
        public int Size
        {
            get { return size; }
        }

        /// <summary>
        /// The time elapsed during compilation.
        /// </summary>
        public TimeSpan CompileTime
        {
            get { return compileTime; }
        }

        public override string ToString()
        {
            return string.Format("{{{0:x4}, Opcodes={1}, Locals={2}, Size={3}, CompileTime={4}}}", routine.Address, opcodeCount, localCount, size, compileTime);
        }
    }
}
