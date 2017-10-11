using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class JinGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;
        private readonly Branch branch;

        public JinGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op1 = instruction.Operands[0];
            this.op2 = instruction.Operands[1];
            this.branch = instruction.Branch;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitLoadObjectParent(op1, reuse: ReuseFirstOperand);
            compiler.EmitLoadOperand(op2);

            il.Compare.Equal();
            compiler.EmitBranch(branch);
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }
    }
}
