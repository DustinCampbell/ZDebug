using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class ModGenerator : BinaryOpGenerator
    {
        public ModGenerator(Operand op1, Operand op2, Variable store)
            : base(OpcodeGeneratorKind.Mod, op1, op2, store, signed: true)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Remainder();
        }
    }
}
