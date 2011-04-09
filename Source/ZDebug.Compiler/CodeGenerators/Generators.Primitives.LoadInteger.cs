using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Primitives
        {
            internal class LoadInteger : Generator
            {
                private readonly int value;

                public LoadInteger(ILBuilder il, int value)
                    : base(il, GeneratorKind.LoadInteger)
                {
                    this.value = value;
                }

                public override void Generate()
                {
                    IL.Load(value);
                }
            }
        }
    }
}
