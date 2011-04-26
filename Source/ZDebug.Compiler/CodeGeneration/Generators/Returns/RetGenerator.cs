using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class RetGenerator : OpcodeGenerator
    {
        private readonly Operand op;

        public RetGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op = instruction.Operands[0];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            if (!ReuseFirstOperand)
            {
                compiler.EmitLoadOperand(op);
            }

            compiler.EmitReturn();
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }
    }
}
