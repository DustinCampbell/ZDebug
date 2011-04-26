using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class StoreWGenerator : StoreMemoryGenerator
    {
        public StoreWGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        protected override int CalculateAddress(int address, int offset)
        {
            return address + (offset * 2);
        }

        protected override void EmitCalculateAddress(Operand addressOp, Operand offsetOp, ILBuilder il, ICompiler compiler)
        {
            if (ReuseFirstOperand)
            {
                compiler.EmitLoadOperand(offsetOp);
                il.Math.Multiply(2);
            }
            else if (ReuseSecondOperand)
            {
                il.Math.Multiply(2);
                compiler.EmitLoadOperand(addressOp);
            }
            else
            {
                compiler.EmitLoadOperand(addressOp);
                compiler.EmitLoadOperand(offsetOp);
                il.Math.Multiply(2);
            }

            il.Math.Add();
        }

        protected override void LoadValue(Operand valueOp, ILBuilder il, ICompiler compiler)
        {
            compiler.EmitLoadOperand(valueOp);
        }

        protected override void StoreMemory(int address, ILocal value, ICompiler compiler)
        {
            compiler.EmitStoreMemoryWord(address, value);
        }

        protected override void StoreMemory(ILocal address, ILocal value, ICompiler compiler)
        {
            compiler.EmitStoreMemoryWord(address, value);
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
