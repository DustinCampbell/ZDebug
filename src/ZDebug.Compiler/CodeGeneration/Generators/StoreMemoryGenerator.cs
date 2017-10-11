using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal abstract class StoreMemoryGenerator : OpcodeGenerator
    {
        private Operand op1;
        private Operand op2;
        private Operand op3;

        public StoreMemoryGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op1 = instruction.Operands[0];
            this.op2 = instruction.Operands[1];
            this.op3 = instruction.Operands[2];
        }

        protected abstract int CalculateAddress(int address, int offset);
        protected abstract void EmitCalculateAddress(Operand addressOp, Operand offsetOp, ILBuilder il, ICompiler compiler);
        protected abstract void LoadValue(Operand valueOp, ILBuilder il, ICompiler compiler);

        protected abstract void StoreMemory(int address, ILocal value, ICompiler compiler);
        protected abstract void StoreMemory(ILocal address, ILocal value, ICompiler compiler);

        private void GenerateWithAddress(int address, ILBuilder il, ICompiler compiler)
        {
            using (var value = il.NewLocal<ushort>())
            {
                LoadValue(op3, il, compiler);
                value.Store();

                StoreMemory(address, value, compiler);
            }
        }

        private void GenerateWithCalculatedAddress(ILBuilder il, ICompiler compiler)
        {
            using (var address = il.NewLocal<int>())
            using (var value = il.NewLocal<ushort>())
            {
                EmitCalculateAddress(op1, op2, il, compiler);
                address.Store();

                LoadValue(op3, il, compiler);
                value.Store();

                StoreMemory(address, value, compiler);
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
    }
}
