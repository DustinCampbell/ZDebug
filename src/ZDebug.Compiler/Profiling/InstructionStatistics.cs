using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.Profiling
{
    public struct InstructionStatistics
    {
        private readonly Instruction instruction;
        private readonly int offset;
        private readonly int size;

        internal InstructionStatistics(Instruction instruction, int offset, int size)
        {
            this.instruction = instruction;
            this.offset = offset;
            this.size = size;
        }

        public Instruction Instruction
        {
            get { return instruction; }
        }

        public int Offset
        {
            get { return offset; }
        }

        public int Size
        {
            get { return size; }
        }
    }
}
