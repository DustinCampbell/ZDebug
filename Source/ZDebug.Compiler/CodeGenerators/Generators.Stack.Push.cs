using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Stack
        {
            internal class Push : Generator
            {
                private readonly IArrayLocal stack;
                private readonly IRefLocal sp;
                private readonly Generator test;
                private readonly Generator stackPeek;
                private readonly Generator incrementSP;

                public Push(ILBuilder il, IArrayLocal stack, IRefLocal sp, Generator loadValue, bool @checked = false)
                    : base(il, GeneratorKind.StackPush)
                {
                    this.stack = stack;
                    this.sp = sp;
                    this.test = @checked ? new Stack.FullTest(il, stack, sp) : null;
                    this.stackPeek = new Stack.SetTop(il, stack, sp, loadValue, @checked: false);
                    this.incrementSP = new Primitives.IncrementRefLocal(il, sp);
                }

                public override void Generate()
                {
                    if (test != null)
                    {
                        test.Generate();
                    }

                    incrementSP.Generate();
                    stackPeek.Generate();
                }
            }
        }
    }
}
