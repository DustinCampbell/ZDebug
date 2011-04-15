using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class RetGenerator : OpcodeGenerator
    {
        private readonly Operand op;

        public RetGenerator(Operand op)
            : base(OpcodeGeneratorKind.Ret)
        {
            this.op = op;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if first op is SP and last instruction stored to SP.

            compiler.EmitLoadOperand(op);
            compiler.EmitReturn();
        }
    }
}
