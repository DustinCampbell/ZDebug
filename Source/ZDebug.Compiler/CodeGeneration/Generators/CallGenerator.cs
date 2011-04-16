using ZDebug.Compiler.Generate;
using ZDebug.Core.Collections;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal abstract class CallGenerator : OpcodeGenerator
    {
        private readonly Operand address;
        private readonly ReadOnlyArray<Operand> args;

        public CallGenerator(Instruction instruction)
            : base(instruction)
        {
            this.address = instruction.Operands[0];
            this.args = instruction.Operands.Skip(1);
        }

        protected abstract void PostCall(ILBuilder il, ICompiler compiler);

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitCall(address, args);

            PostCall(il, compiler);
        }
    }
}
