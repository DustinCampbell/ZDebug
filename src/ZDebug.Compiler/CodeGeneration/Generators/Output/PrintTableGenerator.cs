using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class PrintTableGenerator : OpcodeGenerator
    {
        private readonly Operand addressOp;
        private readonly Operand widthOp;
        private readonly Operand? heightOp;
        private readonly Operand? skipOp;

        public PrintTableGenerator(Instruction instruction)
            : base(instruction)
        {
            this.addressOp = instruction.Operands[0];
            this.widthOp = instruction.Operands[1];

            if (instruction.OperandCount > 2)
            {
                this.heightOp = instruction.Operands[2];
            }

            if (instruction.OperandCount > 3)
            {
                this.skipOp = instruction.Operands[3];
            }
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Arguments.LoadMachine();
            compiler.EmitLoadOperand(addressOp);
            compiler.EmitLoadOperand(widthOp);

            if (heightOp != null)
            {
                compiler.EmitLoadOperand(heightOp.Value);
            }
            else
            {
                il.Load(1);
            }

            if (skipOp != null)
            {
                compiler.EmitLoadOperand(skipOp.Value);
            }
            else
            {
                il.Load(0);
            }

            il.Call(Reflection<CompiledZMachine>.GetMethod("op_print_table", Types.Array<ushort, ushort, ushort, ushort>(), @public: false));
        }
    }
}
