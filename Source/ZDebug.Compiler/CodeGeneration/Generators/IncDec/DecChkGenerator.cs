using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class DecChkGenerator : UnaryOpWithBranchGenerator
    {
        public DecChkGenerator(Instruction instruction)
            : base(instruction)
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
