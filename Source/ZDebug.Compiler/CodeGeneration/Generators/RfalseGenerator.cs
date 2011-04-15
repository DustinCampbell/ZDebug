using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class RfalseGenerator : OpcodeGenerator
    {
        public RfalseGenerator()
            : base(OpcodeGeneratorKind.Rfalse)
        {
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Load(0);
            compiler.EmitReturn();
        }
    }
}
