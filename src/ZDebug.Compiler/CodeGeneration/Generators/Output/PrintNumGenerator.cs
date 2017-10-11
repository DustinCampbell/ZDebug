using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class PrintNumGenerator : OpcodeGenerator
    {
        private readonly Operand number;

        public PrintNumGenerator(Instruction instruction)
            : base(instruction)
        {
            this.number = instruction.Operands[0];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var value = il.NewLocal<short>())
            {
                if (!ReuseFirstOperand)
                {
                    compiler.EmitLoadOperand(number);
                }

                il.Convert.ToInt16();
                value.Store();

                value.LoadAddress();
                il.Call(Reflection<short>.GetMethod("ToString", Types.None));

                compiler.EmitPrintText();
            }
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }
    }
}
