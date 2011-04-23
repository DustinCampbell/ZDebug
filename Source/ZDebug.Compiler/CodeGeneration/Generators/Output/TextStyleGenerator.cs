using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class TextStyleGenerator : OpcodeGenerator
    {
        private readonly Operand styleOp;

        public TextStyleGenerator(Instruction instruction)
            : base(instruction)
        {
            this.styleOp = instruction.Operands[0];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitSetTextStyle(styleOp);
        }
    }
}
