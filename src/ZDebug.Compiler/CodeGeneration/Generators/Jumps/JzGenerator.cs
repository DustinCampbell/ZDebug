using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class JzGenerator : OpcodeGenerator
    {
        private readonly Operand op;
        private readonly Branch branch;

        public JzGenerator(Instruction instruction)
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

            il.Load(0);
            il.Compare.Equal();

            compiler.EmitBranch(branch);
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }
    }
}
