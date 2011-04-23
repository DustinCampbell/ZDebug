using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class AndGenerator : BinaryOpGenerator
    {
        public AndGenerator(Instruction instruction)
            : base(instruction, signed: false)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.And();
        }
    }
}
