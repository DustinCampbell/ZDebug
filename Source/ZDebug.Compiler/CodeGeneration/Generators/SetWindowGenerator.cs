using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class SetWindowGenerator : OpcodeGenerator
    {
        private readonly Operand windowOp;

        public SetWindowGenerator(Instruction instruction)
            : base(instruction)
        {
            this.windowOp = instruction.Operands[0];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitSetWindow(windowOp);
        }
    }
}
