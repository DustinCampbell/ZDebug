using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class LoadBGenerator : LoadMemoryGenerator
    {
        public LoadBGenerator(Operand op1, Operand op2, Variable store)
            : base(OpcodeGeneratorKind.LoadB, op1, op2, store)
        {
        }
        protected override void LoadMemory(int address, ICompiler compiler)
        {
            compiler.EmitLoadMemoryByte(address);
        }

        protected override void LoadMemory(ILocal address, ICompiler compiler)
        {
            compiler.EmitLoadMemoryByte(address);
        }
    }
}
