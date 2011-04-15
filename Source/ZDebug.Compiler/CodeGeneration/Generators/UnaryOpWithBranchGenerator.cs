using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal abstract class UnaryOpWithBranchGenerator : UnaryOpGenerator
    {
        private readonly Operand op2;
        private readonly Branch branch;

        public UnaryOpWithBranchGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op2 = instruction.Operands[1];
            this.branch = instruction.Branch;
        }

        protected abstract void Compare(ILBuilder il);

        protected override void PostOperation(ILocal result, ILBuilder il, ICompiler compiler)
        {
            result.Load();
            compiler.EmitLoadOperand(op2);
            il.Convert.ToInt16();

            Compare(il);
            compiler.EmitBranch(branch);
        }
    }
}
