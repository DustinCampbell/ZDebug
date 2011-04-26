using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class LoadWGenerator : LoadMemoryGenerator
    {
        public LoadWGenerator(Instruction instruction)
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

        protected override void LoadMemory(int address, ICompiler compiler)
        {
            compiler.EmitLoadMemoryWord(address);
        }

        protected override void LoadMemory(ILocal address, ICompiler compiler)
        {
            compiler.EmitLoadMemoryWord(address);
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
