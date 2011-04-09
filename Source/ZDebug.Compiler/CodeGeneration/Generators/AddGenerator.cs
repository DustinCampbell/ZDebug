using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class AddGenerator : BinaryOperatorGenerator
    {
        public AddGenerator(Operand op1, Operand op2, Variable store)
            : base(OpcodeGeneratorKind.Add, op1, op2, store, signed: true)
        {
        }

        protected override void Operation(ILBuilder il, ICompiler compiler)
        {
            il.Math.Add();
        }
    }
}
