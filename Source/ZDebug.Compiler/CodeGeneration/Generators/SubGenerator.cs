using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class SubGenerator : BinaryOperatorGenerator
    {
        public SubGenerator(Operand op1, Operand op2, Variable store)
            : base(OpcodeGeneratorKind.Sub, op1, op2, store, signed: true)
        {
        }

        protected override void Operation(ILBuilder il, ICompiler compiler)
        {
            il.Math.Subtract();
        }
    }
}
