using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class CallNGenerator : OpcodeGenerator
    {
        public CallNGenerator()
            : base(OpcodeGeneratorKind.CallN)
        {
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitCall();

            // discard result
            il.Pop();
        }
    }
}
