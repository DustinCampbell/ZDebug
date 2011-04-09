using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class MulGenerator : BinaryOpGenerator
    {
        public MulGenerator(Operand op1, Operand op2, Variable store)
            : base(OpcodeGeneratorKind.Mul, op1, op2, store, signed: true)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Multiply();
        }
    }
}
