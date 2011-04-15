using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class RetPoppedGenerator : OpcodeGenerator
    {
        public RetPoppedGenerator()
            : base(OpcodeGeneratorKind.RetPopped)
        {
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if last instruction stored to SP.

            compiler.EmitPopStack();
            compiler.EmitReturn();
        }
    }
}
