using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGeneration
{
    internal abstract class OpcodeGenerator
    {
        public readonly OpcodeGeneratorKind Kind;

        protected OpcodeGenerator(OpcodeGeneratorKind kind)
        {
            this.Kind = kind;
        }

        public abstract void Generate(ILBuilder il, ICompiler compiler);
    }
}
