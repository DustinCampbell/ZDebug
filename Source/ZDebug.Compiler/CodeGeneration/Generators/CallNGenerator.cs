using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class CallNGenerator : OpcodeGenerator
    {
        public CallNGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitCall();

            // discard result
            il.Pop();
        }
    }
}
