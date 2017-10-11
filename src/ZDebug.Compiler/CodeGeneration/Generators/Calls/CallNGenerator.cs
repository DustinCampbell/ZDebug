using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class CallNGenerator : CallGenerator
    {
        public CallNGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        protected override void PostCall(ILBuilder il, ICompiler compiler)
        {
            // discard result
            il.Pop();
        }
    }
}
