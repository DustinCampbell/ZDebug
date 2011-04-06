using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class LocalVariables
        {
            internal class Store : Generator
            {
                private readonly IRefLocal local;
                private readonly Generator loadValue;

                public Store(ILBuilder il, IRefLocal local, Generator loadValue)
                    : base(il, GeneratorKind.StoreLocalVariable)
                {
                    this.local = local;
                    this.loadValue = loadValue;
                }

                public override void Generate()
                {
                    local.Load();
                    loadValue.Generate();
                    local.LoadIndirectValue();
                }
            }
        }
    }
}
