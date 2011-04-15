using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class SubGenerator : BinaryOpGenerator
    {
        public SubGenerator(Instruction instruction)
            : base(instruction, signed: true)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Subtract();
        }
    }
}
