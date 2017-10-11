using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class ShowStatusGenerator : OpcodeGenerator
    {
        public ShowStatusGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitShowStatus();
        }
    }
}
