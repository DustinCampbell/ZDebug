using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class RetPoppedGenerator : OpcodeGenerator
    {
        public RetPoppedGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if last instruction stored to SP.

            compiler.EmitPopStack();
            compiler.EmitReturn();
        }
    }
}
