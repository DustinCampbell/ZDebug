using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class AddGenerator : BinaryOpGenerator
    {
        public AddGenerator(Instruction instruction)
            : base(instruction, signed: true)
        {
        }

        protected override void LoadOperands(ILBuilder il, ICompiler compiler, Operand op1, Operand op2)
        {
            if (ReuseFirstOperand)
            {
                compiler.EmitLoadOperand(op2);
                il.Convert.ToInt16();
            }
            else if (ReuseSecondOperand)
            {
                compiler.EmitLoadOperand(op1);
                il.Convert.ToInt16();
            }
            else
            {
                compiler.EmitLoadOperand(op1);
                il.Convert.ToInt16();

                compiler.EmitLoadOperand(op2);
                il.Convert.ToInt16();
            }
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Add();
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
