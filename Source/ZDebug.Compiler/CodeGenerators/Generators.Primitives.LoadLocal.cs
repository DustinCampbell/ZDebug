using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Primitives
        {
            internal class LoadLocal : Generator
            {
                private readonly ILocal local;

                public LoadLocal(ILBuilder il, ILocal local)
                    : base(il, GeneratorKind.LoadLocal)
                {
                    this.local = local;
                }

                public override void Generate()
                {
                    local.Load();
                }
            }
        }
    }
}
