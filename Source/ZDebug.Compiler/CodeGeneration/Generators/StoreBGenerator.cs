using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class StoreBGenerator : StoreMemoryGenerator
    {
        public StoreBGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        protected override void StoreMemory(int address, ILocal value, ICompiler compiler)
        {
            compiler.EmitStoreMemoryByte(address, value);
        }

        protected override void StoreMemory(ILocal address, ILocal value, ICompiler compiler)
        {
            compiler.EmitStoreMemoryByte(address, value);
        }
    }
}
