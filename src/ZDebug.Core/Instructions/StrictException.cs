using System;

namespace ZDebug.Core.Instructions
{
    public class StrictException : Exception
    {
        private readonly Instruction instruction;

        public StrictException(Instruction instruction, string message)
            : base(message)
        {
            this.instruction = instruction;
        }

        public Instruction Instruction
        {
            get { return instruction; }
        }
    }
}
