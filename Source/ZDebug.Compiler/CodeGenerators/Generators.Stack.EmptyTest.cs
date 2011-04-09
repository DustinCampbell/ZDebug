using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Stack
        {
            internal class EmptyTest : Generator
            {
                private readonly IArrayLocal stack;
                private readonly IRefLocal sp;

                public EmptyTest(ILBuilder il, IArrayLocal stack, IRefLocal sp)
                    : base(il, GeneratorKind.StackEmptyTest)
                {
                    this.stack = stack;
                    this.sp = sp;
                }

                public override void Generate()
                {
                    sp.Load();
                    sp.LoadIndirectValue();
                    IL.Load(-1);
                    IL.Compare.Equal();

                    var ok = IL.NewLabel();
                    ok.BranchIf(Condition.False, @short: true);
                    IL.RuntimeError("Stack is empty.");

                    ok.Mark();
                }
            }
        }
    }
}
