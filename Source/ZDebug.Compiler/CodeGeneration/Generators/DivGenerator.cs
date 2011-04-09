using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class DivGenerator : BinaryOperatorGenerator
    {
        public DivGenerator(Operand op1, Operand op2, Variable store)
            : base(OpcodeGeneratorKind.Div, op1, op2, store, signed: true)
        {
        }

        protected override void Operation(ILBuilder il, ICompiler compiler)
        {
            il.Math.Divide();
        }
    }
}
