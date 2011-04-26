using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal abstract class LoadMemoryGenerator : OpcodeGenerator
    {
        private Operand op1;
        private Operand op2;
        private Variable store;

        public LoadMemoryGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op1 = instruction.Operands[0];
            this.op2 = instruction.Operands[1];
            this.store = instruction.StoreVariable;
        }

        protected abstract int CalculateAddress(int address, int offset);
        protected abstract void EmitCalculateAddress(Operand addressOp, Operand offsetOp, ILBuilder il, ICompiler compiler);

        protected abstract void LoadMemory(int address, ICompiler compiler);
        protected abstract void LoadMemory(ILocal address, ICompiler compiler);

        private void GenerateWithAddress(int address, ILBuilder il, ICompiler compiler)
        {
            LoadMemory(address, compiler);

            using (var result = il.NewLocal<ushort>())
            {
                result.Store();
                compiler.EmitStoreVariable(store, result, reuse: ReuseStoreVariable);
            }
        }

        private void GenerateWithCalculatedAddress(ILBuilder il, ICompiler compiler)
        {
            using (var address = il.NewLocal<int>())
            {
                EmitCalculateAddress(op1, op2, il, compiler);
                address.Store();

                LoadMemory(address, compiler);

                using (var result = il.NewLocal<ushort>())
                {
                    result.Store();
                    compiler.EmitStoreVariable(store, result, reuse: ReuseStoreVariable);
                }
            }
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            if (op1.IsConstant && op2.IsConstant)
            {
                GenerateWithAddress(CalculateAddress(op1.Value, op2.Value), il, compiler);
            }
            else
            {
                GenerateWithCalculatedAddress(il, compiler);
            }
        }

        public override bool CanReuseStoreVariable
        {
            get { return true; }
        }
    }
}
