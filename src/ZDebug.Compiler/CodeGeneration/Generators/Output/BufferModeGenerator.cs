using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class BufferModeGenerator : OpcodeGenerator
    {
        private readonly Operand op;

        public BufferModeGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op = instruction.Operands[0];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitLoadOperand(op);

            // TODO: What does buffer_mode mean in this terp?
            il.Pop();
        }
    }
}
