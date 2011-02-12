using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.Compiler
{
    public sealed class ZCompilerResult
    {
        private readonly ZRoutine routine;
        private readonly ZRoutineCode code;
        private readonly int instructions;
        private readonly int opcodes;

        public ZCompilerResult(ZRoutine routine, ZRoutineCode code, int instructions, int opcodes)
        {
            this.routine = routine;
            this.code = code;
            this.instructions = instructions;
            this.opcodes = opcodes;
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
        /// The number of Z-machine instructions that were compiled.
        /// </summary>
        public int Instructions
        {
            get { return instructions; }
        }

        /// <summary>
        /// The number of IL opcodes that were generated.
        /// </summary>
        public int Opcodes
        {
            get { return opcodes; }
        }
    }
}
