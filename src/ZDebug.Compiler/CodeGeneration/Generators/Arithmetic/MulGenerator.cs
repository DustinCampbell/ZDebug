using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class MulGenerator : BinaryOpGenerator
    {
        public MulGenerator(Instruction instruction)
            : base(instruction, signed: true)
        {
        }

        protected override void LoadOperands(ILBuilder il, ICompiler compiler, Operand op1, Operand op2)
        {
            if (ReuseFirstOperand)
            {
                il.Convert.ToInt16();

                compiler.EmitLoadOperand(op2, convertResult: false);
                il.Convert.ToInt16();
            }
            else if (ReuseSecondOperand)
            {
                il.Convert.ToInt16();

                compiler.EmitLoadOperand(op1, convertResult: false);
                il.Convert.ToInt16();
            }
            else
            {
                compiler.EmitLoadOperand(op1, convertResult: false);
                il.Convert.ToInt16();

                compiler.EmitLoadOperand(op2, convertResult: false);
                il.Convert.ToInt16();
            }
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Multiply();
        }

        public override bool CanReuseSecondOperand
        {
            get
            {
                return true;
            }
        }
    }
}
