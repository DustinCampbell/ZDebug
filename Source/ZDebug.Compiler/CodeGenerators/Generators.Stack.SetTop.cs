using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Stack
        {
            internal class SetTop : Generator
            {
                private readonly IArrayLocal stack;
                private readonly IRefLocal sp;
                private readonly Generator test;
                private readonly Generator loadValue;

                public SetTop(ILBuilder il, IArrayLocal stack, IRefLocal sp, Generator loadValue, bool @checked = false)
                    : base(il, GeneratorKind.StackSetTop)
                {
                    this.stack = stack;
                    this.sp = sp;
                    this.test = @checked ? new Stack.FullTest(il, stack, sp) : null;
                    this.loadValue = loadValue;
                }

                public override void Generate()
                {
                    if (test != null)
                    {
                        test.Generate();
                    }

                    stack.StoreElement(
                        indexLoader: () =>
                        {
                            sp.Load();
                            sp.LoadIndirectValue();
                        },
                        valueLoader: () =>
                        {
                            loadValue.Generate();
                        });
                }
            }
        }
    }
}
