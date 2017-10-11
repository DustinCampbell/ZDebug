using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class OrGenerator : BinaryOpGenerator
    {
        public OrGenerator(Instruction instruction)
            : base(instruction, signed: false)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Or();
        }
    }
}
