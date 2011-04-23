using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class QuitGenerator : OpcodeGenerator
    {
        public QuitGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.Quit();
        }
    }
}
