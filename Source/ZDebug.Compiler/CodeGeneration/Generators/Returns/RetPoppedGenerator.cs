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
            if (!ReuseStack)
            {
                compiler.EmitPopStack();
            }

            compiler.EmitReturn();
        }

        public override bool CanReuseStack
        {
            get { return true; }
        }
    }
}
