using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class DecChkGenerator : UnaryOpWithBranchGenerator
    {
        public DecChkGenerator(Operand op1, Operand op2, Branch branch)
            : base(OpcodeGeneratorKind.DecChk, op1, op2, branch)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Subtract(1);
        }

        protected override void Compare(ILBuilder il)
        {
            il.Compare.LessThan();
        }
    }
}
