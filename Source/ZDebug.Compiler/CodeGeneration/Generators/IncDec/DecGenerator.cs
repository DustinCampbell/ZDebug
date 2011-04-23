using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class DecGenerator : UnaryOpGenerator
    {
        public DecGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Subtract(1);
        }
    }
}
