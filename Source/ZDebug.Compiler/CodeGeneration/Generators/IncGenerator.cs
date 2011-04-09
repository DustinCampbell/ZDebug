using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class IncGenerator : UnaryOpGenerator
    {
        public IncGenerator(Operand op)
            : base(OpcodeGeneratorKind.Inc, op)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Add(1);
        }
    }
}
