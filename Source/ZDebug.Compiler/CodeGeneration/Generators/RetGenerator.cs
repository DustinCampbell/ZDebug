using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class RetGenerator : OpcodeGenerator
    {
        private readonly Operand op;

        public RetGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op = instruction.Operands[0];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if first op is SP and last instruction stored to SP.

            compiler.EmitLoadOperand(op);
            compiler.EmitReturn();
        }
    }
}
