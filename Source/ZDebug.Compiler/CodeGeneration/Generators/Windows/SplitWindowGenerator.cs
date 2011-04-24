using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class SplitWindowGenerator : OpcodeGenerator
    {
        private readonly Operand heightOp;

        public SplitWindowGenerator(Instruction instruction)
            : base(instruction)
        {
            this.heightOp = instruction.Operands[0];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitSplitWindow(heightOp);
        }
    }
}
