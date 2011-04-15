using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class StoreWGenerator : StoreMemoryGenerator
    {
        public StoreWGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        protected override void StoreMemory(int address, ILocal value, ICompiler compiler)
        {
            compiler.EmitStoreMemoryWord(address, value);
        }

        protected override void StoreMemory(ILocal address, ILocal value, ICompiler compiler)
        {
            compiler.EmitStoreMemoryWord(address, value);
        }
    }
}
