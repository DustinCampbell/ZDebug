using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Primitives
        {
            internal class IncrementRefLocal : Generator
            {
                private readonly IRefLocal local;

                public IncrementRefLocal(ILBuilder il, IRefLocal local)
                    : base(il, GeneratorKind.IncrementRefLocal)
                {
                    this.local = local;
                }

                public override void Generate()
                {
                    local.Load();
                    local.Load();
                    local.LoadIndirectValue();
                    IL.Math.Add(1);
                    local.StoreIndirectValue();
                }
            }
        }
    }
}
