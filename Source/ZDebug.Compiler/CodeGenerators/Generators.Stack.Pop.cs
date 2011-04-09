using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Stack
        {
            internal class Pop : Generator
            {
                private readonly IArrayLocal stack;
                private readonly IRefLocal sp;
                private readonly Generator stackPeek;
                private readonly Generator decrementSP;

                public Pop(ILBuilder il, IArrayLocal stack, IRefLocal sp, bool @checked = false)
                    : base(il, GeneratorKind.StackPop)
                {
                    this.stack = stack;
                    this.sp = sp;
                    this.stackPeek = new Stack.Peek(il, stack, sp, @checked);
                    this.decrementSP = new Primitives.DecrementRefLocal(il, sp);
                }

                public override void Generate()
                {
                    stackPeek.Generate();
                    decrementSP.Generate();
                }
            }
        }
    }
}
