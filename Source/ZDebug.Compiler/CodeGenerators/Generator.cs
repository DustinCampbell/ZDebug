using ZDebug.Compiler.Generate;
namespace ZDebug.Compiler.CodeGenerators
{
    internal abstract class Generator
    {
        protected readonly ILBuilder IL;
        public readonly GeneratorKind Kind;

        protected Generator(ILBuilder il, GeneratorKind kind)
        {
            this.IL = il;
            this.Kind = kind;
        }

        public abstract void Generate();
    }
}
