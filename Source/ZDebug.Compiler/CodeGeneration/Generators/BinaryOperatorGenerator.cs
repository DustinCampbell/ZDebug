using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration
{
    internal abstract class BinaryOperatorGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;
        private readonly Variable store;
        private readonly bool signed;

        public BinaryOperatorGenerator(OpcodeGeneratorKind kind, Operand op1, Operand op2, Variable store, bool signed)
            : base(kind)
        {
            this.op1 = op1;
            this.op2 = op2;
            this.store = store;
            this.signed = signed;
        }

        protected abstract void Operation(ILBuilder il, ICompiler compiler);

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if first op is SP and last instruction stored to SP.
            // OPTIMIZE: If the storing to SP and the next instruction uses SP in its first operand, we don't need
            // the call to EmitStore().

            compiler.EmitOperandLoad(op1);
            if (signed)
            {
                il.Convert.ToInt16();
            }

            compiler.EmitOperandLoad(op2);
            if (signed)
            {
                il.Convert.ToInt16();
            }

            Operation(il, compiler);
            il.Convert.ToUInt16();

            compiler.EmitStore(store);
        }
    }
}
