using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class StoreWGenerator : StoreMemoryGenerator
    {
        public StoreWGenerator(Operand op1, Operand op2, Operand op3)
            : base(OpcodeGeneratorKind.StoreW, op1, op2, op3)
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
