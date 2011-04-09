using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal abstract class StoreMemoryGenerator : OpcodeGenerator
    {
        private Operand op1;
        private Operand op2;
        private Operand op3;

        public StoreMemoryGenerator(OpcodeGeneratorKind kind, Operand op1, Operand op2, Operand op3)
            : base(kind)
        {
            this.op1 = op1;
            this.op2 = op2;
            this.op3 = op3;
        }

        protected abstract void StoreMemory(int address, ILocal value, ICompiler compiler);
        protected abstract void StoreMemory(ILocal address, ILocal value, ICompiler compiler);

        private void GenerateWithAddress(int address, ILBuilder il, ICompiler compiler)
        {
            using (var value = il.NewLocal<byte>())
            {
                compiler.EmitOperandLoad(op3);
                il.Convert.ToUInt8();
                value.Store();

                StoreMemory(address, value, compiler);
            }
        }

        private void GenerateWithCalculatedAddress(ILBuilder il, ICompiler compiler)
        {
            using (var address = il.NewLocal<int>())
            using (var value = il.NewLocal<byte>())
            {
                compiler.EmitOperandLoad(op1);
                compiler.EmitOperandLoad(op2);
                il.Math.Add();
                address.Store();

                compiler.EmitOperandLoad(op3);
                il.Convert.ToUInt8();
                value.Store();

                StoreMemory(address, value, compiler);
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
