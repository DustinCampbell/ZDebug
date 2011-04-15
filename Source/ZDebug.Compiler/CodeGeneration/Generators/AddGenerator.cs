using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class AddGenerator : BinaryOpGenerator
    {
        public AddGenerator(Instruction instruction)
            : base(instruction, signed: true)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Add();
        }
    }
}
