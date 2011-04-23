using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class IncGenerator : UnaryOpGenerator
    {
        public IncGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Add(1);
        }
    }
}
