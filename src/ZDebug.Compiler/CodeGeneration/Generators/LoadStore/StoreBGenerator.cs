using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class StoreBGenerator : StoreMemoryGenerator
    {
        public StoreBGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        protected override int CalculateAddress(int address, int offset)
        {
            return address + offset;
        }

        protected override void EmitCalculateAddress(Operand addressOp, Operand offsetOp, ILBuilder il, ICompiler compiler)
        {
            if (ReuseFirstOperand)
            {
                compiler.EmitLoadOperand(offsetOp);
            }
            else if (ReuseSecondOperand)
            {
                compiler.EmitLoadOperand(addressOp);
            }
            else
            {
                compiler.EmitLoadOperand(addressOp);
                compiler.EmitLoadOperand(offsetOp);
            }

            il.Math.Add();
        }

        protected override void LoadValue(Operand valueOp, ILBuilder il, ICompiler compiler)
        {
            compiler.EmitLoadOperand(valueOp);
            il.Convert.ToUInt8();
        }

        protected override void StoreMemory(int address, ILocal value, ICompiler compiler)
        {
            compiler.EmitStoreMemoryByte(address, value);
        }

        protected override void StoreMemory(ILocal address, ILocal value, ICompiler compiler)
        {
            compiler.EmitStoreMemoryByte(address, value);
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }

        public override bool CanReuseSecondOperand
        {
            get { return true; }
        }
    }
}
