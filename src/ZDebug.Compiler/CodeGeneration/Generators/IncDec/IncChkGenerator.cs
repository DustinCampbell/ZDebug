using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class IncChkGenerator : UnaryOpWithBranchGenerator
    {
        public IncChkGenerator(Instruction instruction)
            : base(instruction)
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
