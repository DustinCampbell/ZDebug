using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class DivGenerator : BinaryOpGenerator
    {
        public DivGenerator(Instruction instruction)
            : base(instruction, signed: true)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Divide();
        }
    }
}
