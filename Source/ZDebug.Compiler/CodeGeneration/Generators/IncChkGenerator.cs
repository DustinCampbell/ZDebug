using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class IncChkGenerator : UnaryOpWithBranchGenerator
    {
        public IncChkGenerator(Operand op1, Operand op2, Branch branch)
            : base(OpcodeGeneratorKind.IncChk, op1, op2, branch)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Add(1);
        }

        protected override void Compare(ILBuilder il)
        {
            il.Compare.GreaterThan();
        }
    }
}
