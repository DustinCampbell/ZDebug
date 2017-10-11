using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class CheckArgCountGenerator : OpcodeGenerator
    {
        private readonly Operand op;
        private readonly Branch branch;

        public CheckArgCountGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op = instruction.Operands[0];
            this.branch = instruction.Branch;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            if (!ReuseFirstOperand)
            {
                compiler.EmitLoadOperand(op);
            }

            il.Arguments.LoadArgCount();

            il.Compare.AtMost();
            compiler.EmitBranch(branch);
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }
    }
}
