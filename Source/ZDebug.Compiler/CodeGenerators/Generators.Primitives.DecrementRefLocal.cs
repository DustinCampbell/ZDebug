using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Primitives
        {
            internal class DecrementRefLocal : Generator
            {
                private readonly IRefLocal local;

                public DecrementRefLocal(ILBuilder il, IRefLocal local)
                    : base(il, GeneratorKind.DecrementRefLocal)
                {
                    this.local = local;
                }

                public override void Generate()
                {
                    local.Load();
                    local.Load();
                    local.LoadIndirectValue();
                    IL.Math.Subtract(1);
                    local.StoreIndirectValue();
                }
            }
        }
    }
}
