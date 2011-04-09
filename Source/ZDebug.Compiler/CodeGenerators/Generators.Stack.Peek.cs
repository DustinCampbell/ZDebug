using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Stack
        {
            internal class Peek : Generator
            {
                private readonly IArrayLocal stack;
                private readonly IRefLocal sp;
                private readonly Generator test;

                public Peek(ILBuilder il, IArrayLocal stack, IRefLocal sp, bool @checked = false)
                    : base(il, GeneratorKind.StackPeek)
                {
                    this.stack = stack;
                    this.sp = sp;
                    this.test = @checked ? new Stack.EmptyTest(il, stack, sp) : null;
                }

                public override void Generate()
                {
                    if (test != null)
                    {
                        test.Generate();
                    }

                    stack.LoadElement(
                        indexLoader: () =>
                        {
                            sp.Load();
                            sp.LoadIndirectValue();
                        });
                }
            }
        }
    }
}
