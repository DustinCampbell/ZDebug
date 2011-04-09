using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Stack
        {
            internal class FullTest : Generator
            {
                private readonly IArrayLocal stack;
                private readonly IRefLocal sp;

                public FullTest(ILBuilder il, IArrayLocal stack, IRefLocal sp)
                    : base(il, GeneratorKind.StackFullTest)
                {
                    this.stack = stack;
                    this.sp = sp;
                }

                public override void Generate()
                {
                    sp.Load();
                    sp.LoadIndirectValue();
                    IL.Load(CompiledZMachine.STACK_SIZE - 1);
                    IL.Compare.Equal();

                    var ok = IL.NewLabel();
                    ok.BranchIf(Condition.False, @short: true);
                    IL.RuntimeError("Stack is full.");

                    ok.Mark();
                }
            }
        }
    }
}
