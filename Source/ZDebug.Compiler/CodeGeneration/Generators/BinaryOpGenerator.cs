using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal abstract class BinaryOpGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;
        private readonly Variable store;
        private readonly bool signed;

        public BinaryOpGenerator(Instruction instruction, bool signed)
            : base(instruction)
        {
            this.op1 = instruction.Operands[0];
            this.op2 = instruction.Operands[1];
            this.store = instruction.StoreVariable;
            this.signed = signed;
        }

        protected abstract void Operation(ILBuilder il);

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if first op is SP and last instruction stored to SP.
            // OPTIMIZE: If the storing to SP and the next instruction uses SP in its first operand, we don't need
            // the call to EmitStore().

            compiler.EmitLoadOperand(op1);
            if (signed)
            {
                il.Convert.ToInt16();
            }

            compiler.EmitLoadOperand(op2);
            if (signed)
            {
                il.Convert.ToInt16();
            }

            Operation(il);
            il.Convert.ToUInt16();

            using (var result = il.NewLocal<ushort>())
            {
                result.Store();
                compiler.EmitStoreVariable(store, result);
            }
        }
    }
}
