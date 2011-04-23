using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class LoadWGenerator : LoadMemoryGenerator
    {
        public LoadWGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        protected override void LoadMemory(int address, ICompiler compiler)
        {
            compiler.EmitLoadMemoryWord(address);
        }

        protected override void LoadMemory(ILocal address, ICompiler compiler)
        {
            compiler.EmitLoadMemoryWord(address);
        }
    }
}
