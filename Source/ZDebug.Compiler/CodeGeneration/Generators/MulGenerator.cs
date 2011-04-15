using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class MulGenerator : BinaryOpGenerator
    {
        public MulGenerator(Instruction instruction)
            : base(instruction, signed: true)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Multiply();
        }
    }
}
