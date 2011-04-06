using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class LocalVariables
        {
            internal class Load : Generator
            {
                private readonly IRefLocal local;

                public Load(ILBuilder il, IRefLocal local)
                    : base(il, GeneratorKind.LoadLocalVariable)
                {
                    this.local = local;
                }

                public override void Generate()
                {
                    local.Load();
                    local.LoadIndirectValue();
                }
            }
        }
    }
}
