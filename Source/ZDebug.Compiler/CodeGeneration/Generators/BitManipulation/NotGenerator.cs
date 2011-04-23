using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class NotGenerator : OpcodeGenerator
    {
        private readonly Operand op;
        private readonly Variable store;

        public NotGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op = instruction.Operands[0];
            this.store = instruction.StoreVariable;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if first op is SP and last instruction stored to SP.

            compiler.EmitLoadOperand(op);

            il.Math.Not();
            il.Convert.ToUInt16();

            using (var result = il.NewLocal<ushort>())
            {
                result.Store();
                compiler.EmitStoreVariable(store, result);
            }
        }
    }
}
