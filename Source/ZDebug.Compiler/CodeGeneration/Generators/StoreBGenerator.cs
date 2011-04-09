using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class StoreBGenerator : StoreMemoryGenerator
    {
        public StoreBGenerator(Operand op1, Operand op2, Operand op3)
            : base(OpcodeGeneratorKind.StoreB, op1, op2, op3)
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
