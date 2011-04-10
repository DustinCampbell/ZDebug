using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal abstract class LoadMemoryGenerator : OpcodeGenerator
    {
        private Operand op1;
        private Operand op2;
        private Variable store;

        public LoadMemoryGenerator(OpcodeGeneratorKind kind, Operand op1, Operand op2, Variable store)
            : base(kind)
        {
            this.op1 = op1;
            this.op2 = op2;
            this.store = store;
        }

        protected abstract void LoadMemory(int address, ICompiler compiler);
        protected abstract void LoadMemory(ILocal address, ICompiler compiler);

        private void GenerateWithAddress(int address, ILBuilder il, ICompiler compiler)
        {
            LoadMemory(address, compiler);

            using (var result = il.NewLocal<ushort>())
            {
                result.Store();
                compiler.EmitStoreVariable(store, result);
            }
        }

        private void GenerateWithCalculatedAddress(ILBuilder il, ICompiler compiler)
        {
            using (var address = il.NewLocal<int>())
            {
                compiler.EmitLoadOperand(op1);
                compiler.EmitLoadOperand(op2);
                il.Math.Add();
                address.Store();

                LoadMemory(address, compiler);

                using (var result = il.NewLocal<ushort>())
                {
                    result.Store();
                    compiler.EmitStoreVariable(store, result);
                }
            }
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            if (op1.IsConstant && op2.IsConstant)
            {
                GenerateWithAddress((int)op1.Value + (int)op2.Value, il, compiler);
            }
            else
            {
                GenerateWithCalculatedAddress(il, compiler);
            }
        }
    }
}
