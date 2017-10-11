using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class PrintCharGenerator : OpcodeGenerator
    {
        private readonly Operand charOp;

        public PrintCharGenerator(Instruction instruction)
            : base(instruction)
        {
            this.charOp = instruction.Operands[0];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            if (!ReuseFirstOperand)
            {
                compiler.EmitLoadOperand(charOp);
            }

            compiler.EmitPrintChar();
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }
    }
}
