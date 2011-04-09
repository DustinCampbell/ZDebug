using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGeneration
{
    internal class RtrueGenerator : OpcodeGenerator
    {
        public RtrueGenerator()
            : base(OpcodeGeneratorKind.Rtrue)
        {
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Load(1);
            compiler.EmitReturn();
        }
    }
}
