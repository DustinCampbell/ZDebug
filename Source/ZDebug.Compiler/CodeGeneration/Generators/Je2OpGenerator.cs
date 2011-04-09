using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration
{
    internal class Je2OpGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;

        public Je2OpGenerator(Operand op1, Operand op2)
            : base(OpcodeGeneratorKind.Je2Op)
        {
            this.op1 = op1;
            this.op2 = op2;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            // OPIMIZE: Use IL evaluation stack if first op is SP and last instruction stored to SP.

            compiler.LoadOperand(op1);
            compiler.LoadOperand(op2);

            il.Compare.Equal();

            compiler.Branch();
        }
    }
}
