using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class RtrueGenerator : OpcodeGenerator
    {
        public RtrueGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Load(1);
            compiler.EmitReturn();
        }
    }
}
