using ZDebug.Compiler.Generate;
using ZDebug.Core.Execution;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class SetColorGenerator : OpcodeGenerator
    {
        private readonly Operand foregroundOp;
        private readonly Operand backgroundOp;

        public SetColorGenerator(Instruction instruction)
            : base(instruction)
        {
            this.foregroundOp = instruction.Operands[0];
            this.backgroundOp = instruction.Operands[1];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var foreground = il.NewLocal<ZColor>())
            using (var background = il.NewLocal<ZColor>())
            {
                compiler.EmitLoadOperand(foregroundOp);
                foreground.Store();

                compiler.EmitLoadOperand(backgroundOp);
                background.Store();

                compiler.EmitSetColor(foreground, background);
            }
        }
    }
}
