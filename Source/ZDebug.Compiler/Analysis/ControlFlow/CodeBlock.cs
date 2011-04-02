using System.Collections.Generic;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.Analysis.ControlFlow
{
    internal class CodeBlock : Block
    {
        private readonly int address;
        private readonly List<Instruction> instructions;

        public CodeBlock(int address)
            : base(isEntry: false, isExit: false)
        {
            this.address = address;
            this.instructions = new List<Instruction>();
        }

        public void AddInstruction(Instruction instruction)
        {
            this.instructions.Add(instruction);
        }

        public int Address
        {
            get
            {
                return this.address;
            }
        }

        public IEnumerable<Instruction> Instructions
        {
            get
            {
                foreach (var instruction in this.instructions)
                {
                    yield return instruction;
                }
            }
        }

        public override string ToString()
        {
            return "Code block: " + this.address.ToString("x4");
        }
    }
}
