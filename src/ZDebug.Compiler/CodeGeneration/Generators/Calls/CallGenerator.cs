using System;
using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal abstract class CallGenerator : OpcodeGenerator
    {
        private Operand AddressOp => Instruction.Operands[0];
        private ReadOnlySpan<Operand> Args => Instruction.Operands.Slice(1);

        public CallGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        protected abstract void PostCall(ILBuilder il, ICompiler compiler);

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitCall(AddressOp, Args, reuse: ReuseFirstOperand);

            PostCall(il, compiler);
        }

        public override bool CanReuseFirstOperand => AddressOp.IsVariable;
    }
}
