using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.Compiler.Profiling
{
    public sealed class RoutineStatistics
    {
        private readonly int opcodeCount;
        private readonly int localCount;

        public RoutineStatistics(int opcodeCount, int localCount)
        {
            this.opcodeCount = opcodeCount;
            this.localCount = localCount;
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
    }
}
