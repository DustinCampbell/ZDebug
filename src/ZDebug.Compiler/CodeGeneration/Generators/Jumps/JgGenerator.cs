using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class JgGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;
        private readonly Branch branch;

        public JgGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op1 = instruction.Operands[0];
            this.op2 = instruction.Operands[1];
            this.branch = instruction.Branch;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            if (!ReuseFirstOperand)
            {
                compiler.EmitLoadOperand(op1);
            }

            il.Convert.ToInt16();

            compiler.EmitLoadOperand(op2);
            il.Convert.ToInt16();

            il.Compare.GreaterThan();
            compiler.EmitBranch(branch);
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }
    }
}
